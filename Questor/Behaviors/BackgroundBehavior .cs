// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DirectEve;
using Questor.Modules.Caching;
using Questor.Modules.Logging;
using Questor.Modules.Lookup;
using Questor.Modules.Activities;
using Questor.Modules.States;
using Questor.Modules.Combat;
using Questor.Modules.Actions;
using Questor.Modules.BackgroundTasks;

namespace Questor.Behaviors
{
    public class BackgroundBehavior
    {
        //private readonly Combat _combat;
        //private readonly Drones _drones;
        //private readonly Arm _arm;

        private DateTime _lastPulse;
        //private DateTime _lastSalvageTrip = DateTime.MinValue;
        //private readonly Salvage _salvage;
        private readonly Panic _panic;
        public DateTime LastAction;
        private readonly Stopwatch _watch;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        //private bool _firstStart = true;
        public bool PanicStateReset; //false;

        private bool ValidSettings { get; set; }

        public bool CloseQuestorFlag = true;

        public string CharacterName { get; set; }

        //DateTime _nextAction = DateTime.UtcNow;

        private DateTime _nextBookmarkRefreshCheck = DateTime.MinValue;
        private DateTime _nextBookmarksrefresh = DateTime.MinValue;

        public BackgroundBehavior()
        {
            _lastPulse = DateTime.MinValue;

            //_arm = new Arm();
            //_salvage = new Salvage();
            //_combat = new Combat();
            //_drones = new Drones();
            _watch = new Stopwatch();
            _panic = new Panic();

            //
            // this is combat mission specific and needs to be generalized
            //
            Settings.Instance.SettingsLoaded += SettingsLoaded;

            // States.CurrentBackgroundBehaviorState fixed on ExecuteMission
            _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Idle;
        }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ApplySalvageSettings();
            ValidateCombatMissionSettings();
        }

        public void DebugBackgroundBehaviorStates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("BackgroundBehavior.State is", _States.CurrentBackgroundBehaviorState.ToString(), Logging.White);
        }

        public void DebugPanicstates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("Panic.State is ", _States.CurrentPanicState.ToString(), Logging.White);
        }

        public void DebugPerformanceClearandStartTimer()
        {
            _watch.Reset();
            _watch.Start();
        }

        public void DebugPerformanceStopandDisplayTimer(string whatWeAreTiming)
        {
            _watch.Stop();
            if (Settings.Instance.DebugPerformance)
                Logging.Log(whatWeAreTiming, " took " + _watch.ElapsedMilliseconds + "ms", Logging.White);
        }

        public void ValidateCombatMissionSettings()
        {
            ValidSettings = true;
            if (Settings.Instance.Ammo.Select(a => a.DamageType).Distinct().Count() != 4)
            {
                if (Settings.Instance.Ammo.All(a => a.DamageType != DamageType.EM)) Logging.Log("Settings", ": Missing EM damage type!", Logging.Orange);
                if (Settings.Instance.Ammo.All(a => a.DamageType != DamageType.Thermal)) Logging.Log("Settings", "Missing Thermal damage type!", Logging.Orange);
                if (Settings.Instance.Ammo.All(a => a.DamageType != DamageType.Kinetic)) Logging.Log("Settings", "Missing Kinetic damage type!", Logging.Orange);
                if (Settings.Instance.Ammo.All(a => a.DamageType != DamageType.Explosive)) Logging.Log("Settings", "Missing Explosive damage type!", Logging.Orange);

                Logging.Log("Settings", "You are required to specify all 4 damage types in your settings xml file!", Logging.White);
                ValidSettings = false;
            }

            if (Cache.Instance.Agent == null || !Cache.Instance.Agent.IsValid)
            {
                Logging.Log("Settings", "Unable to locate agent [" + Cache.Instance.CurrentAgent + "]", Logging.White);
                ValidSettings = false;
                return;
            }
        }

        public void ApplySalvageSettings()
        {
            Salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
            Salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
            Salvage.LootEverything = Settings.Instance.LootEverything;
        }

        private void BeginClosingQuestor()
        {
            Cache.Instance.EnteredCloseQuestor_DateTime = DateTime.UtcNow;
            _States.CurrentQuestorState = QuestorState.CloseQuestor;
        }

        public void ProcessState()
        {
            // Only pulse state changes every 1.5s
            //if (DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < Time.Instance.QuestorPulse_milliseconds) //default: 1500ms
            //    return;
            //_lastPulse = DateTime.UtcNow;

            // Invalid settings, quit while we're ahead
            //if (!ValidSettings)
            //{
            //    if (DateTime.UtcNow.Subtract(LastAction).TotalSeconds < Time.Instance.ValidateSettings_seconds) //default is a 15 second interval
            //    {
            //        ValidateCombatMissionSettings();
            //        LastAction = DateTime.UtcNow;
            //    }
            //    return;
            //}

            if (Cache.Instance.SessionState == "Quitting")
            {
                BeginClosingQuestor();
            }

            if (Cache.Instance.GotoBaseNow)
            {
                _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoBase;
            }

            //
            // Panic always runs, not just in space
            //
            DebugPerformanceClearandStartTimer();
            _panic.ProcessState();
            DebugPerformanceStopandDisplayTimer("Panic.ProcessState");
            if (_States.CurrentPanicState == PanicState.Panic || _States.CurrentPanicState == PanicState.Panicking)
            {
                // If Panic is in panic state, questor is in panic States.CurrentCombatMissionBehaviorState :)
                _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Panic;

                if (PanicStateReset)
                {
                    _States.CurrentPanicState = PanicState.Normal;
                    PanicStateReset = false;
                }
            }
            else if (_States.CurrentPanicState == PanicState.Resume)
            {
                // Reset panic state
                _States.CurrentPanicState = PanicState.Normal;

                // Head back to the mission
                _States.CurrentTravelerState = TravelerState.Idle;
                _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
            }
            
            switch (_States.CurrentBackgroundBehaviorState)
            {
                case BackgroundBehaviorState.Idle:

                    _States.CurrentDroneState = DroneState.Idle;
                    _States.CurrentSalvageState = SalvageState.Idle;
                    
                    if (Cache.Instance.StopBot)
                    {
                        //
                        // this is used by the 'local is safe' routines - standings checks - at the moment is stops questor for the rest of the session.
                        //
                        if (Settings.Instance.DebugIdle) Logging.Log("BackgroundBehavior", "if (Cache.Instance.StopBot)", Logging.White);
                        return;
                    }

                    if (Cache.Instance.InSpace)
                    {
                        if (Settings.Instance.DebugIdle) Logging.Log("BackgroundBehavior", "if (Cache.Instance.InSpace)", Logging.White);
                        Combat.ProcessState();
                        Drones.ProcessState();
                        return;
                    }

                    if (Cache.Instance.InStation)
                    {
                        if (Settings.Instance.DebugIdle) Logging.Log("BackgroundBehavior", "if (Cache.Instance.InStation)", Logging.White);
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Start;
                        return;
                    }

                    if (Settings.Instance.DebugIdle) Logging.Log("BackgroundBehavior", "if (Cache.Instance.InSpace) else", Logging.White);

                    if (DateTime.UtcNow < Cache.Instance.LastInSpace.AddSeconds(10))
                    {
                        return;
                    }

                    break;

                case BackgroundBehaviorState.Start:
                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Arm;
                    break;

                case BackgroundBehaviorState.Arm:

                    //
                    // this state should never be reached in space. if we are in space and in this state we should switch to gotomission
                    //
                    if (Cache.Instance.InSpace)
                    {
                        Logging.Log(_States.CurrentBackgroundBehaviorState.ToString(), "We are in space, how did we get set to this state while in space? Changing state to: GotoBase", Logging.White);
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoBase;
                    }

                    if (_States.CurrentArmState == ArmState.Idle)
                    {
                        Logging.Log("Arm", "Begin", Logging.White);
                        _States.CurrentArmState = ArmState.Begin;

                        // Load right ammo based on mission
                        Arm.AmmoToLoad.Clear();
                        Cache.Instance.DamageType = DamageType.EM;
                        Arm.LoadSpecificAmmo(new[] { Cache.Instance.DamageType });
                    }

                    Arm.ProcessState();

                    if (Settings.Instance.DebugStates) Logging.Log("Arm.State", "is" + _States.CurrentArmState, Logging.White);

                    if (_States.CurrentArmState == ArmState.NotEnoughAmmo)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm", "Armstate.NotEnoughAmmo", Logging.Orange);
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                    }

                    if (_States.CurrentArmState == ArmState.NotEnoughDrones)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm", "Armstate.NotEnoughDrones", Logging.Orange);
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                    }

                    if (_States.CurrentArmState == ArmState.Done)
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDroneState = DroneState.WaitingForTargets;

                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.WarpOutStation;
                    }

                    break;

                case BackgroundBehaviorState.SwitchToNoobShip1:
                    _States.CurrentArmState = ArmState.ActivateNoobShip;
                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.SwitchToNoobShip2;
                    break;

                case BackgroundBehaviorState.SwitchToNoobShip2:
                    Arm.ProcessState();
                    if (_States.CurrentArmState == ArmState.Done)
                    {
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDroneState = DroneState.WaitingForTargets;
                        _States.CurrentPanicState = PanicState.Idle;
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                    }
                    break;

                case BackgroundBehaviorState.WarpOutStation:
                    DirectBookmark warpOutBookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkWarpOut ?? "").OrderByDescending(b => b.CreatedOn).FirstOrDefault(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);

                    //DirectBookmark _bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.bookmarkWarpOut + "-" + Cache.Instance.CurrentAgent ?? "").OrderBy(b => b.CreatedOn).FirstOrDefault();
                    long solarid = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (warpOutBookmark == null)
                    {
                        Logging.Log("BackgroundBehavior.WarpOut", "No Bookmark", Logging.White);
                        if (_States.CurrentBackgroundBehaviorState == BackgroundBehaviorState.WarpOutStation) _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.BeginGotoBookmark;
                    }
                    else if (warpOutBookmark.LocationId == solarid)
                    {
                        if (Traveler.Destination == null)
                        {
                            Logging.Log("BackgroundBehavior.WarpOut", "Warp at " + warpOutBookmark.Title, Logging.White);
                            Traveler.Destination = new BookmarkDestination(warpOutBookmark);
                            Cache.Instance.DoNotBreakInvul = true;
                        }

                        Traveler.ProcessState();
                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            Logging.Log("BackgroundBehavior.WarpOut", "Safe!", Logging.White);
                            Cache.Instance.DoNotBreakInvul = false;
                            if (_States.CurrentBackgroundBehaviorState == BackgroundBehaviorState.WarpOutStation) _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.BeginGotoBookmark;
                            Traveler.Destination = null;
                        }
                    }
                    else
                    {
                        Logging.Log("BackgroundBehavior.WarpOut", "No Bookmark in System", Logging.Orange);
                        if (_States.CurrentBackgroundBehaviorState == BackgroundBehaviorState.WarpOutStation) _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.BeginGotoBookmark;
                    }
                    break;

                case BackgroundBehaviorState.GotoBase:
                    break;

                case BackgroundBehaviorState.BeginGotoBookmark:
                    
                    if (DateTime.UtcNow > _nextBookmarkRefreshCheck)
                    {
                        _nextBookmarkRefreshCheck = DateTime.UtcNow.AddMinutes(1);
                        if (Cache.Instance.InStation && (DateTime.UtcNow > _nextBookmarksrefresh))
                        {
                            _nextBookmarksrefresh = DateTime.UtcNow.AddMinutes(Cache.Instance.RandomNumber(1, 2));
                            Logging.Log("BackgroundBehavior.BeginAftermissionSalvaging", "Next Bookmark refresh in [" +
                                           Math.Round(_nextBookmarksrefresh.Subtract(DateTime.UtcNow).TotalMinutes, 0) + "min]", Logging.White);
                            Cache.Instance.DirectEve.RefreshBookmarks();
                        }
                        else
                        {
                            Logging.Log("BackgroundBehavior.BeginAftermissionSalvaging", "Next Bookmark refresh in [" +
                                           Math.Round(_nextBookmarksrefresh.Subtract(DateTime.UtcNow).TotalMinutes, 0) + "min]", Logging.White);
                        }
                    }

                    Cache.Instance.OpenWrecks = true;
                    
                    DirectBookmark TraveltoThisBookmark = Cache.Instance.GetTravelBookmark;
                    if (TraveltoThisBookmark == null)
                    {
                        Logging.Log("BackgroundBehavior.BeginGotoBookmark", "Unable to find bookmark to travel to", Logging.Teal);
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                        return;
                    }

                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.TravelToBookmark;
                    //_lastSalvageTrip = DateTime.UtcNow;
                    Traveler.Destination = new BookmarkDestination(TraveltoThisBookmark);
                    break;

                case BackgroundBehaviorState.TravelToBookmark:
                    Traveler.ProcessState();

                    if (_States.CurrentTravelerState == TravelerState.AtDestination || Cache.Instance.GateInGrid())
                    {
                        if (!Combat.ReloadAll(Cache.Instance.MyShipEntity)) return;
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Idle;
                        Traveler.Destination = null;
                        return;
                    }

                    if (Settings.Instance.DebugStates) Logging.Log("Traveler.State is ", _States.CurrentTravelerState.ToString(), Logging.White);
                    break;

                case BackgroundBehaviorState.GotoSalvageBookmark:
                    Traveler.ProcessState();

                    if (_States.CurrentTravelerState == TravelerState.AtDestination || Cache.Instance.GateInGrid())
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Salvage;
                        Traveler.Destination = null;
                        return;
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State is ", _States.CurrentTravelerState.ToString(), Logging.White);
                    break;

                case BackgroundBehaviorState.Salvage:
                    if (Settings.Instance.DebugSalvage) Logging.Log("BackgroundBehavior", "salvage: attempting to open cargo hold", Logging.White);
                    if (Cache.Instance.CurrentShipsCargo == null) return;
                    if (Settings.Instance.DebugSalvage) Logging.Log("BackgroundBehavior", "salvage: done opening cargo hold", Logging.White);
                    Cache.Instance.SalvageAll = true;
                    Cache.Instance.OpenWrecks = true;

                    EntityCache deadlyNPC = Cache.Instance.Entities.Where(t => t.Distance < (int)Distances.OnGridWithMe && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeColidableStructure).OrderBy(t => t.Distance).FirstOrDefault();
                    if (deadlyNPC != null)
                    {
                        // found NPCs that will likely kill out fragile salvage boat!
                        List<DirectBookmark> missionSalvageBookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                        Logging.Log("BackgroundBehavior.Salvage", "could not be completed because of NPCs left in the mission: deleting on grid salvage bookmark", Logging.White);

                        if (Settings.Instance.DeleteBookmarksWithNPC)
                        {
                            if (!Cache.Instance.DeleteBookmarksOnGrid("BackgroundBehavior.Salvage")) return;
                            _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoSalvageBookmark;
                            DirectBookmark bookmark = missionSalvageBookmarks.OrderBy(i => i.CreatedOn).FirstOrDefault();
                            Traveler.Destination = new BookmarkDestination(bookmark);
                            break;
                        }
                        else
                        {
                            Logging.Log("BackgroundBehavior.Salvage", "could not be completed because of NPCs left in the mission: on grid salvage bookmark not deleted", Logging.Orange);
                            Cache.Instance.SalvageAll = false;
                            Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                            _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoBase;
                            return;
                        }
                    }
                    else
                    {
                        if (Cache.Instance.CurrentShipsCargo == null) return;

                        if (Settings.Instance.UnloadLootAtStation && Cache.Instance.CurrentShipsCargo.IsValid && (Cache.Instance.CurrentShipsCargo.Capacity - Cache.Instance.CurrentShipsCargo.UsedCapacity) < Settings.Instance.ReserveCargoCapacity)
                        {
                            Logging.Log("BackgroundBehavior.Salvage", "We are full, go to base to unload", Logging.White);
                            _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoBase;
                            break;
                        }

                        if (!Cache.Instance.UnlootedContainers.Any())
                        {
                            Logging.Log("BackgroundBehavior.Salvage", "Finished salvaging the room", Logging.White);
                            if (!Cache.Instance.DeleteBookmarksOnGrid("BackgroundBehavior.Salvage")) return;
                            Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;

                            if (!Cache.Instance.AfterMissionSalvageBookmarks.Any() && !Cache.Instance.GateInGrid())
                            {
                                Logging.Log("BackgroundBehavior.Salvage", "We have salvaged all bookmarks, go to base", Logging.White);
                                Cache.Instance.SalvageAll = false;
                                Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                                _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoBase;
                                return;
                            }
                            else
                            {
                                if (!Cache.Instance.GateInGrid()) //no acceleration gate found
                                {
                                    Logging.Log("BackgroundBehavior.Salvage", "Go to the next salvage bookmark", Logging.White);
                                    DirectBookmark bookmark;
                                    if (Settings.Instance.FirstSalvageBookmarksInSystem)
                                    {
                                        bookmark = Cache.Instance.AfterMissionSalvageBookmarks.FirstOrDefault(c => c.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId) ?? Cache.Instance.AfterMissionSalvageBookmarks.FirstOrDefault();
                                    }
                                    else
                                    {
                                        bookmark = Cache.Instance.AfterMissionSalvageBookmarks.OrderBy(i => i.CreatedOn).FirstOrDefault() ?? Cache.Instance.AfterMissionSalvageBookmarks.FirstOrDefault();
                                    }
                                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoSalvageBookmark;
                                    Traveler.Destination = new BookmarkDestination(bookmark);
                                }
                                else if (Settings.Instance.UseGatesInSalvage) // acceleration gate found, are we configured to use it or not?
                                {
                                    Logging.Log("BackgroundBehavior.Salvage", "Acceleration gate found - moving to next pocket", Logging.White);
                                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.SalvageUseGate;
                                }
                                else //acceleration gate found but we are configured to not use it, gotobase instead
                                {
                                    Logging.Log("BackgroundBehavior.Salvage", "Acceleration gate found, useGatesInSalvage set to false - Returning to base", Logging.White);
                                    Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoBase;
                                    Traveler.Destination = null;
                                }
                            }
                            break;
                        }

                        //we __cannot ever__ approach in salvage.cs so this section _is_ needed.
                        Salvage.MoveIntoRangeOfWrecks();
                        try
                        {
                            // Overwrite settings, as the 'normal' settings do not apply
                            Salvage.MaximumWreckTargets = Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets, Cache.Instance.DirectEve.Me.MaxLockedTargets);
                            Salvage.ReserveCargoCapacity = 80;
                            Salvage.LootEverything = true;
                            Salvage.ProcessState();

                            //Logging.Log("number of max cache ship: " + Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets);
                            //Logging.Log("number of max cache me: " + Cache.Instance.DirectEve.Me.MaxLockedTargets);
                            //Logging.Log("number of max math.min: " + _salvage.MaximumWreckTargets);
                        }
                        finally
                        {
                            ApplySalvageSettings();
                        }
                    }
                    break;

                case BackgroundBehaviorState.SalvageUseGate:
                    Cache.Instance.OpenWrecks = true;

                    if (Cache.Instance.AccelerationGates == null || !Cache.Instance.AccelerationGates.Any())
                    {
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.GotoSalvageBookmark;
                        return;
                    }

                    _lastX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

                    EntityCache closest = Cache.Instance.AccelerationGates.OrderBy(t => t.Distance).FirstOrDefault();
                    if (closest.Distance < (int)Distances.DecloakRange)
                    {
                        Logging.Log("BackgroundBehavior.Salvage", "Acceleration gate found - GroupID=" + closest.GroupId, Logging.White);

                        // Activate it and move to the next Pocket
                        closest.Activate();

                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        Logging.Log("BackgroundBehavior.Salvage", "Activate [" + closest.Name + "] and change States.CurrentBackgroundBehaviorState to 'NextPocket'", Logging.White);

                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.SalvageNextPocket;
                        _lastPulse = DateTime.UtcNow;
                        return;
                    }

                    if (closest.Distance < (int)Distances.WarptoDistance)
                    {
                        // Move to the target
                        if (Cache.Instance.NextApproachAction < DateTime.UtcNow && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                        {
                            Logging.Log("BackgroundBehavior.Salvage", "Approaching target [" + closest.Name + "][ID: " + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.White);
                            closest.Approach();
                        }
                    }
                    else
                    {
                        // Probably never happens
                        if (DateTime.UtcNow > Cache.Instance.NextWarpTo)
                        {
                            Logging.Log("BackgroundBehavior.Salvage", "Warping to [" + closest.Name + "] which is [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.White);
                            closest.WarpTo();
                        }
                    }
                    _lastPulse = DateTime.UtcNow.AddSeconds(10);
                    break;

                case BackgroundBehaviorState.SalvageNextPocket:
                    Cache.Instance.OpenWrecks = true;
                    double distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distances.NextPocketDistance)
                    {
                        //we know we are connected here...
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        Logging.Log("BackgroundBehavior.Salvage", "We have moved to the next Pocket [" + Math.Round(distance / 1000, 0) + "k away]", Logging.White);

                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Salvage;
                        return;
                    }

                    if (DateTime.UtcNow.Subtract(_lastPulse).TotalMinutes > 2)
                    {
                        Logging.Log("BackgroundBehavior.Salvage", "We have timed out, retry last action", Logging.White);

                        // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.SalvageUseGate;
                    }
                    break;


                case BackgroundBehaviorState.Traveler:
                    Cache.Instance.OpenWrecks = false;
                    List<int> destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                    if (destination == null || destination.Count == 0)
                    {
                        // happens if autopilot is not set and this QuestorState is chosen manually
                        // this also happens when we get to destination (!?)
                        Logging.Log("BackgroundBehavior.Traveler", "No destination?", Logging.White);
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                        return;
                    }

                    if (destination.Count == 1 && destination.FirstOrDefault() == 0)
                        destination[0] = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                    if (Traveler.Destination == null || Traveler.Destination.SolarSystemId != destination.LastOrDefault())
                    {
                        IEnumerable<DirectBookmark> bookmarks = Cache.Instance.DirectEve.Bookmarks.Where(b => b.LocationId == destination.LastOrDefault()).ToList();
                        if (bookmarks.FirstOrDefault() != null && bookmarks.Any())
                            Traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).FirstOrDefault());
                        else
                        {
                            Logging.Log("BackgroundBehavior.Traveler", "Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]", Logging.White);
                            Traveler.Destination = new SolarSystemDestination(destination.LastOrDefault());
                        }
                    }
                    else
                    {
                        Traveler.ProcessState();

                        //we also assume you are connected during a manual set of questor into travel mode (safe assumption considering someone is at the kb)
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                            {
                                Logging.Log("BackgroundBehavior.Traveler", "an error has occurred", Logging.White);
                                _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                                return;
                            }

                            if (Cache.Instance.InSpace)
                            {
                                Logging.Log("BackgroundBehavior.Traveler", "Arrived at destination (in space, Questor stopped)", Logging.White);
                                _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error;
                                return;
                            }

                            Logging.Log("BackgroundBehavior.Traveler", "Arrived at destination", Logging.White);
                            _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Idle;
                            return;
                        }
                    }
                    break;

                case BackgroundBehaviorState.GotoNearestStation:
                    if (!Cache.Instance.InSpace || Cache.Instance.InWarp) return;
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        if (station.Distance > (int)Distances.WarptoDistance)
                        {
                            Logging.Log("BackgroundBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);
                            station.WarpToAndDock();
                            _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Salvage;
                            break;
                        }

                        if (station.Distance < 1900)
                        {
                            if (DateTime.UtcNow > Cache.Instance.NextDockAction)
                            {
                                Logging.Log("BackgroundBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);
                                station.Dock();
                            }
                        }
                        else
                        {
                            if (Cache.Instance.NextApproachAction < DateTime.UtcNow && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id))
                            {
                                Logging.Log("BackgroundBehavior.GotoNearestStation", "Approaching [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);
                                station.Approach();
                            }
                        }
                    }
                    else
                    {
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Error; //should we goto idle here?
                    }
                    break;

                case BackgroundBehaviorState.Default:
                    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Idle;
                    break;
            }
        }
    }
}