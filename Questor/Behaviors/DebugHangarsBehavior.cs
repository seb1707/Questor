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
using Questor.Modules.Combat;
using Questor.Modules.Logging;
using Questor.Modules.Lookup;
using Questor.Modules.Activities;
using Questor.Modules.States;
using Questor.Modules.Actions;
using Questor.Modules.BackgroundTasks;

namespace Questor.Behaviors
{
    public class DebugHangarsBehavior
    {
        public DateTime LastAction;
        public static long AgentID;

        private readonly Stopwatch _watch;

        public bool PanicStateReset; //false;

        private bool ValidSettings { get; set; }

        public bool CloseQuestorFlag = true;

        public string CharacterName { get; set; }

        public DebugHangarsBehavior()
        {
            _watch = new Stopwatch();

            //
            // this is combat mission specific and needs to be generalized
            //
            Settings.Instance.SettingsLoaded += SettingsLoaded;

            _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            _States.CurrentUnloadLootState = UnloadLootState.Idle;
            _States.CurrentTravelerState = TravelerState.Idle;
        }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ValidateCombatMissionSettings();
        }

        public void DebugPerformanceClearandStartTimer()
        {
            _watch.Reset();
            _watch.Start();
        }

        public void DebugPerformanceStopandDisplayTimer(string whatWeAreTiming)
        {
            _watch.Stop();
            if (Logging.DebugPerformance)
                Logging.Log(whatWeAreTiming, " took " + _watch.ElapsedMilliseconds + "ms", Logging.White);
        }

        public void ValidateCombatMissionSettings()
        {
            ValidSettings = true;
            if (Combat.Ammo.Select(a => a.DamageType).Distinct().Count() != 4)
            {
                if (Combat.Ammo.All(a => a.DamageType != DamageType.EM))
                    Logging.Log("Settings", ": Missing EM damage type!", Logging.Orange);
                if (Combat.Ammo.All(a => a.DamageType != DamageType.Thermal))
                    Logging.Log("Settings", "Missing Thermal damage type!", Logging.Orange);
                if (Combat.Ammo.All(a => a.DamageType != DamageType.Kinetic))
                    Logging.Log("Settings", "Missing Kinetic damage type!", Logging.Orange);
                if (Combat.Ammo.All(a => a.DamageType != DamageType.Explosive))
                    Logging.Log("Settings", "Missing Explosive damage type!", Logging.Orange);

                Logging.Log("Settings", "You are required to specify all 4 damage types in your settings xml file!", Logging.White);
                ValidSettings = false;
            }

            DirectAgent agent = Cache.Instance.DirectEve.GetAgentByName(Cache.Instance.CurrentAgent);

            if (agent == null || !agent.IsValid)
            {
                Logging.Log("Settings", "Unable to locate agent [" + Cache.Instance.CurrentAgent + "]", Logging.White);
                ValidSettings = false;
            }
            else
            {
                Arm.AgentId = agent.AgentId;
                AgentID = agent.AgentId;
            }
        }

        private void BeginClosingQuestor()
        {
            Time.EnteredCloseQuestor_DateTime = DateTime.UtcNow;
            _States.CurrentQuestorState = QuestorState.CloseQuestor;
        }

        private void TravelToAgentsStation()
        {
            try
            {
                StationDestination baseDestination = Traveler.Destination as StationDestination;
                if (baseDestination == null || baseDestination.StationId != Cache.Instance.Agent.StationId)
                    Traveler.Destination = new StationDestination(Cache.Instance.Agent.SolarSystemId,
                                                                   Cache.Instance.Agent.StationId,
                                                                   Cache.Instance.DirectEve.GetLocationName(
                                                                       Cache.Instance.Agent.StationId));
            }
            catch (Exception ex)
            {
                Logging.Log("DebugHangarsBehavior", "TravelToAgentsStation: Exception caught: [" + ex.Message + "]", Logging.Red);
                return;
            }
            if (Cache.Instance.InSpace)
            {
                if (!Cache.Instance.ActiveShip.Entity.IsCloaked || (Time.Instance.LastSessionChange.AddSeconds(60) > DateTime.UtcNow))
                {
                    Combat.ProcessState();
                    Drones.ProcessState(); //do we really want to use drones here?
                }
            }
            if (Cache.Instance.InSpace && !Combat.TargetedBy.Any(t => t.IsWarpScramblingMe))
            {
                Drones.IsMissionPocketDone = true; //tells drones.cs that we can pull drones
                //Logging.Log("CombatmissionBehavior","TravelToAgentStation: not pointed",Logging.White);
            }
            Traveler.ProcessState();
        }

        private void AvoidBumpingThings()
        {
            //if It has not been at least 60 seconds since we last session changed do not do anything
            if (Cache.Instance.InStation || !Cache.Instance.InSpace || Cache.Instance.ActiveShip.Entity.IsCloaked || (Cache.Instance.InSpace && Time.Instance.LastSessionChange.AddSeconds(60) < DateTime.UtcNow))
                return;
            //
            // if we are "too close" to the bigObject move away... (is orbit the best thing to do here?)
            //
            if (Cache.Instance.ClosestStargate.Distance > 9000 || Cache.Instance.ClosestStation.Distance > 5000)
            {
                EntityCache thisBigObject = Cache.Instance.BigObjects.FirstOrDefault();
                if (thisBigObject != null)
                {
                    if (thisBigObject.Distance >= (int)Distances.TooCloseToStructure)
                    {
                        //we are no longer "too close" and can proceed.
                    }
                    else
                    {

                        if (thisBigObject.Orbit((int)Distances.SafeDistancefromStructure))
                        {
                            Logging.Log("DebugBehavior", _States.CurrentDebugBehaviorState +
                                        ": initiating Orbit of [" + thisBigObject.Name +
                                        "] orbiting at [" + Distances.SafeDistancefromStructure + "]", Logging.White);
                            return;
                        }

                        return;
                        //we are still too close, do not continue through the rest until we are not "too close" anymore
                    }
                }
            }
        }

        public void ProcessState()
        {
            //Logging.Log("DebugHangarsBehavior","ProcessState - every tick",Logging.Teal);
            if (Cleanup.SignalToQuitQuestorAndEVEAndRestartInAMoment)
            {
                if (_States.CurrentQuestorState != QuestorState.CloseQuestor)
                {
                    _States.CurrentQuestorState = QuestorState.CloseQuestor;
                    BeginClosingQuestor();
                }
            }

            if (Cache.Instance.GotoBaseNow)
            {
                _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.GotoBase;
            }
            if ((DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalSeconds > 10) && (DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalSeconds < 60))
            {
                if (Cache.Instance.QuestorJustStarted)
                {
                    Cache.Instance.QuestorJustStarted = false;
                    Cleanup.SessionState = "Starting Up";

                    // write session log
                    Statistics.WriteSessionLogStarting();
                }
            }

            //
            // Panic always runs, not just in space
            //
            Panic.ProcessState();
            if (_States.CurrentPanicState == PanicState.Panic || _States.CurrentPanicState == PanicState.Panicking)
            {
                // If Panic is in panic state, questor is in panic States.CurrentDebugHangarBehaviorState :)
                _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Panic;

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

                // Sit Idle and wait for orders.
                _States.CurrentTravelerState = TravelerState.Idle;
                _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
            }
            

            //Logging.Log("test");
            switch (_States.CurrentDebugHangarBehaviorState)
            {
                case DebugHangarsBehaviorState.Idle:

                    if (Cache.Instance.StopBot)
                    {
                        //
                        // this is used by the 'local is safe' routines - standings checks - at the moment is stops questor for the rest of the session.
                        //
                        if (Logging.DebugAutoStart || Logging.DebugIdle) Logging.Log("DebugHangarsBehavior", "DebugIdle: StopBot [" + Cache.Instance.StopBot + "]", Logging.White);
                        return;
                    }

                    if (Cache.Instance.InSpace)
                    {
                        if (Logging.DebugAutoStart || Logging.DebugIdle) Logging.Log("DebugHangarsBehavior", "DebugIdle: InSpace [" + Cache.Instance.InSpace + "]", Logging.White);

                        // Questor does not handle in space starts very well, head back to base to try again
                        Logging.Log("DebugHangarsBehavior", "Started questor while in space, heading back to base in 15 seconds", Logging.White);
                        LastAction = DateTime.UtcNow;
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.DelayedGotoBase;
                        break;
                    }
                    
                    if (DateTime.UtcNow < Time.Instance.LastInSpace.AddSeconds(10))
                    {
                        if (Logging.DebugAutoStart || Logging.DebugIdle) Logging.Log("DebugHangarsBehavior", "DebugIdle: Cache.Instance.LastInSpace [" + Time.Instance.LastInSpace.Subtract(DateTime.UtcNow).TotalSeconds + "] sec ago, waiting until we have been docked for 10+ seconds", Logging.White);
                        return;
                    }

                    _States.CurrentArmState = ArmState.Idle;
                    _States.CurrentDroneState = DroneState.Idle;
                    _States.CurrentSalvageState = SalvageState.Idle;
                    _States.CurrentTravelerState = TravelerState.Idle;
                    _States.CurrentUnloadLootState = UnloadLootState.Idle;
                    _States.CurrentTravelerState = TravelerState.Idle;

                    LastAction = DateTime.UtcNow;
                    break;

                case DebugHangarsBehaviorState.DelayedGotoBase:
                    if (DateTime.UtcNow.Subtract(LastAction).TotalSeconds < Time.Instance.DelayedGotoBase_seconds)
                        break;

                    Logging.Log("DebugHangarsBehavior", "Heading back to base", Logging.White);
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.GotoBase;
                    break;

                case DebugHangarsBehaviorState.Arm:
                    //
                    // only used when someone manually selects the arm state.
                    //
                    if (_States.CurrentArmState == ArmState.Idle)
                    {
                        Logging.Log("Arm", "Begin", Logging.White);
                        _States.CurrentArmState = ArmState.Begin;

                        // Load right ammo based on mission
                        Arm.AmmoToLoad.Clear();
                        Arm.LoadSpecificAmmo(new[] { MissionSettings.MissionDamageType });
                    }

                    Arm.ProcessState();

                    if (_States.CurrentArmState == ArmState.NotEnoughAmmo)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm", "Armstate.NotEnoughAmmo", Logging.Orange);
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    }

                    if (_States.CurrentArmState == ArmState.NotEnoughDrones)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm", "Armstate.NotEnoughDrones", Logging.Orange);
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    }

                    if (_States.CurrentArmState == ArmState.Done)
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDroneState = DroneState.WaitingForTargets;
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
                    }
                    break;

                case DebugHangarsBehaviorState.Salvage:
                    if (!Cache.Instance.InSpace)
                        return;

                    Salvage.SalvageAll = true;
                    Salvage.OpenWrecks = true;

                    if (Cache.Instance.CurrentShipsCargo == null) return;

                    if (Salvage.UnloadLootAtStation && Cache.Instance.CurrentShipsCargo.Window.IsReady && (Cache.Instance.CurrentShipsCargo.Capacity - Cache.Instance.CurrentShipsCargo.UsedCapacity) < 100)
                    {
                        Logging.Log("CombatMissionsBehavior.Salvage", "We are full, go to base to unload", Logging.White);
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.GotoBase;
                        break;
                    }

                    if (!Cache.Instance.UnlootedContainers.Any())
                    {
                        break;
                    }

                    //we __cannot ever__ approach in salvage.cs so this section _is_ needed.
                    Salvage.MoveIntoRangeOfWrecks();
                    try
                    {
                        // Overwrite settings, as the 'normal' settings do not apply
                        Salvage.DedicatedSalvagerMaximumWreckTargets = Cache.Instance.MaxLockedTargets;
                        Salvage.DedicatedSalvagerReserveCargoCapacity = 80;
                        Salvage.DedicatedSalvagerLootEverything = true;
                        Salvage.ProcessState();
                        //Logging.Log("number of max cache ship: " + Cache.Instance.ActiveShip.MaxLockedTargets);
                        //Logging.Log("number of max cache me: " + Cache.Instance.DirectEve.Me.MaxLockedTargets);
                        //Logging.Log("number of max math.min: " + _salvage.MaximumWreckTargets);
                    }
                    finally
                    {
                        Salvage.DedicatedSalvagerMaximumWreckTargets = null;
                        Salvage.DedicatedSalvagerReserveCargoCapacity = null;
                        Salvage.DedicatedSalvagerLootEverything = null;
                    }

                    break;

                case DebugHangarsBehaviorState.GotoBase:
                    if (Logging.DebugGotobase) Logging.Log("DebugHangarsBehavior", "GotoBase: AvoidBumpingThings()", Logging.White);

                    AvoidBumpingThings();

                    if (Logging.DebugGotobase) Logging.Log("DebugHangarsBehavior", "GotoBase: TravelToAgentsStation()", Logging.White);

                    TravelToAgentsStation();

                    if (_States.CurrentTravelerState == TravelerState.AtDestination) // || DateTime.UtcNow.Subtract(Time.EnteredCloseQuestor_DateTime).TotalMinutes > 10)
                    {
                        if (Logging.DebugGotobase) Logging.Log("DebugHangarsBehavior", "GotoBase: We are at destination", Logging.White);
                        Cache.Instance.GotoBaseNow = false; //we are there - turn off the 'forced' GoToBase
                        MissionSettings.Mission = Cache.Instance.GetAgentMission(AgentID, false);
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.UnloadLoot;
                        Traveler.Destination = null;
                    }

                    break;

                case DebugHangarsBehaviorState.UnloadLoot:
                    if (_States.CurrentUnloadLootState == UnloadLootState.Idle)
                    {
                        Logging.Log("DebugHangarsBehavior", "UnloadLoot: Begin", Logging.White);
                        _States.CurrentUnloadLootState = UnloadLootState.Begin;
                    }

                    UnloadLoot.ProcessState();
                    
                    if (_States.CurrentUnloadLootState == UnloadLootState.Done)
                    {
                        Cache.Instance.LootAlreadyUnloaded = true;
                        _States.CurrentUnloadLootState = UnloadLootState.Idle;
                        MissionSettings.Mission = Cache.Instance.GetAgentMission(AgentID, false);
                        if (_States.CurrentCombatState == CombatState.OutOfAmmo) // on mission
                        {
                            Logging.Log("DebugHangarsBehavior.UnloadLoot", "We are out of ammo", Logging.Orange);
                            _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
                            return;
                        }

                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
                        Logging.Log("DebugHangarsBehavior.Unloadloot", "CharacterMode: [" + Settings.Instance.CharacterMode + "], AfterMissionSalvaging: [" + Salvage.AfterMissionSalvaging + "], DebugHangarsBehaviorState: [" + _States.CurrentDebugHangarBehaviorState + "]", Logging.White);
                        Statistics.FinishedMission = DateTime.UtcNow;
                        return;
                    }
                    break;

                case DebugHangarsBehaviorState.Traveler:
                    Salvage.OpenWrecks = false;
                    List<int> destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                    if (destination == null || destination.Count == 0)
                    {
                        // happens if autopilot is not set and this QuestorState is chosen manually
                        // this also happens when we get to destination (!?)
                        Logging.Log("DebugHangarsBehavior.Traveler", "No destination?", Logging.White);
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                        return;
                    }
                    else if (destination.Count == 1 && destination.FirstOrDefault() == 0)
                        destination[0] = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (Traveler.Destination == null || Traveler.Destination.SolarSystemId != destination.Last())
                    {
                        IEnumerable<DirectBookmark> bookmarks = Cache.Instance.AllBookmarks.Where(b => b.LocationId == destination.Last()).ToList();
                        if (bookmarks != null && bookmarks.Any())
                            Traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).FirstOrDefault());
                        else
                        {
                            Logging.Log("DebugHangarsBehavior.Traveler", "Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]", Logging.White);
                            Traveler.Destination = new SolarSystemDestination(destination.Last());
                        }
                    }
                    else
                    {
                        Traveler.ProcessState();

                        //we also assume you are connected during a manual set of questor into travel mode (safe assumption considering someone is at the kb)
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                            {
                                Logging.Log("DebugHangarsBehavior.Traveler", "an error has occurred", Logging.White);
                                _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                                return;
                            }

                            if (Cache.Instance.InSpace)
                            {
                                Logging.Log("DebugHangarsBehavior.Traveler", "Arrived at destination (in space, Questor stopped)", Logging.White);
                                _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                                return;
                            }

                            Logging.Log("DebugHangarsBehavior.Traveler", "Arrived at destination", Logging.White);
                            _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
                            return;
                        }
                    }
                    break;

                case DebugHangarsBehaviorState.GotoNearestStation:
                    if (!Cache.Instance.InSpace || Cache.Instance.InWarp) return;
                    EntityCache station = null;
                    if (Cache.Instance.Stations != null && Cache.Instance.Stations.Any())
                    {
                        station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();    
                    }

                    if (station != null)
                    {
                        if (station.Distance > (int)Distances.WarptoDistance)
                        {
                            if (station.WarpTo())
                            {
                                Logging.Log("DebugHangarsBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);
                                _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Idle;
                                break;    
                            }

                            break;
                        }

                        if (station.Distance < 1900)
                        {
                            if (station.Dock())
                            {
                                Logging.Log("DebugBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);

                            }
                        }
                        else
                        {
                            if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id)
                            {
                                if (station.Approach())
                                {
                                    Logging.Log("DebugBehavior.GotoNearestStation", "Approaching [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);
                                }
                            }
                        }
                    }
                    else
                    {
                        _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error; //should we goto idle here?
                    }
                    break;

                case DebugHangarsBehaviorState.ReadyItemsHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.ReadyItemsHangar:", Logging.White);
                    if (Cache.Instance.ItemHangar == null) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackItemsHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackItemsHangar:", Logging.White);
                    if (!Cache.Instance.StackItemsHangarAsAmmoHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseItemsHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseItemsHangar:", Logging.White);
                    if (!Cache.Instance.CloseItemsHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenShipsHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenShipsHangar:", Logging.White);
                    if (!Cache.Instance.OpenShipsHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackShipsHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackShipsHangar:", Logging.White);
                    if (!Cache.Instance.StackShipsHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseShipsHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseShipsHangar:", Logging.White);
                    if (!Cache.Instance.CloseShipsHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenLootContainer:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenLootContainer:", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackLootContainer:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackLootContainer:", Logging.White);
                    if (!Cache.Instance.StackLootContainer("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseLootContainer:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseLootContainer:", Logging.White);
                    if (!Cache.Instance.CloseLootContainer("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    //Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenCorpAmmoHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenCorpAmmoHangar:", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    Logging.Log("OpenCorpAmmoHangar", "AmmoHangar Contains [" + Cache.Instance.AmmoHangar.Items.Count() + "] Items", Logging.Debug);
                    
                    try
                    {
                        int icount = 0;
                        foreach (DirectItem itemfound in Cache.Instance.AmmoHangar.Items)
                        {
                            icount++;
                            Logging.Log("Arm.MoveItems", "Found: Name [" + itemfound.TypeName + "] Quantity [" + itemfound.Quantity + "] in the AmmoHangar", Logging.Red);
                            if (icount > 20)
                            {
                                Logging.Log("Arm.MoveItems", "max items to log reached (over 20). there are probably more items but we only log 20 of em.", Logging.Red);
                                break;
                            }

                            continue;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logging.Log("OpenCorpLootHangar", "Exception was: [" + exception + "]", Logging.Debug);
                    }

                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackCorpAmmoHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackCorpAmmoHangar:", Logging.White);
                    if (!Cache.Instance.StackCorpAmmoHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseCorpAmmoHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseCorpAmmoHangar:", Logging.White);
                    if (!Cache.Instance.CloseCorpHangar("DebugHangars", "AMMO")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenCorpLootHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenCorpLootHangar:", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    Logging.Log("OpenCorpLootHangar", "LootHangar Contains [" + Cache.Instance.LootHangar.Items.Count() + "] Items", Logging.Debug);

                    try
                    {
                        int icount2 = 0;
                        foreach (DirectItem itemfound in Cache.Instance.LootHangar.Items)
                        {
                            icount2++;
                            Logging.Log("Arm.MoveItems", "Found: Name [" + itemfound.TypeName + "] Quantity [" + itemfound.Quantity + "] in the LootHangar", Logging.Red);
                            if (icount2 > 20)
                            {
                                Logging.Log("Arm.MoveItems", "max items to log reached (over 20). there are probably more items but we only log 20 of em.", Logging.Red);
                                break;
                            }

                            continue;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logging.Log("OpenCorpLootHangar", "Exception was: [" +  exception+ "]", Logging.Debug);
                    }
                    
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackCorpLootHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackCorpLootHangar:", Logging.White);
                    if (!Cache.Instance.StackCorpLootHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseCorpLootHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseCorpLootHangar:", Logging.White);
                    if (!Cache.Instance.CloseCorpHangar("DebugHangars", "LOOT")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenAmmoHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenAmmoHangar:", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackAmmoHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackAmmoHangar:", Logging.White);
                    if (!Cache.Instance.StackAmmoHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseAmmoHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseAmmoHangar:", Logging.White);
                    if (!Cache.Instance.CloseAmmoHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenLootHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenLootHangar:", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackLootHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackLootHangar:", Logging.White);
                    if (!Cache.Instance.StackLootHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseLootHangar:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseLootHangar:", Logging.White);
                    if (!Cache.Instance.CloseLootHangar("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseAllInventoryWindows:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseAllInventoryWindows:", Logging.White);
                    if (!Cleanup.CloseInventoryWindows()) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenCargoHold:
                    Logging.Log("DebugHangars", "DebugHangarsState.StackLootHangar:", Logging.White);
                    if (Cache.Instance.CurrentShipsCargo == null) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.StackCargoHold:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseLootHangar:", Logging.White);
                    if (!Cache.Instance.StackCargoHold("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.CloseCargoHold:
                    Logging.Log("DebugHangars", "DebugHangarsState.CloseAllInventoryWindows:", Logging.White);
                    if (!Cache.Instance.CloseCargoHold("DebugHangars")) return;
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.GetAmmoHangarID:
                    Logging.Log("DebugHangars", "DebugHangarsState.GetAmmoHangarID:", Logging.White);
                    if (!Cache.Instance.GetCorpAmmoHangarID()) return;
                    Logging.Log("DebugHangars", "AmmoHangarId [" + Cache.Instance.AmmoHangarID + "]", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.GetLootHangarID:
                    Logging.Log("DebugHangars", "DebugHangarsState.GetLootHangarID:", Logging.White);
                    if (!Cache.Instance.GetCorpLootHangarID()) return;
                    Logging.Log("DebugHangars", "LootHangarId [" + Cache.Instance.LootHangarID + "]", Logging.White);
                    Cache.Instance.DebugInventoryWindows("DebugHangars");
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenInventory:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenInventory:", Logging.White);
                    if (!Cache.Instance.OpenInventoryWindow("DebugHangarsState.OpenInventoryWindow")) return;
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.ListInvTree:
                    Logging.Log("DebugHangars", "DebugHangarsState.ListInvTree:", Logging.White);
                    if (!Cache.Instance.ListInvTree("DebugHangarsState.ListInvTree")) return;
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.OpenOreHold:
                    Logging.Log("DebugHangars", "DebugHangarsState.OpenOreHold:", Logging.White);
                    if (!Cache.Instance.OpenOreHold("DebugHangarsState.OpenOreHold")) return;
                    _States.CurrentDebugHangarBehaviorState = DebugHangarsBehaviorState.Error;
                    Cache.Instance.Paused = true;
                    break;

                case DebugHangarsBehaviorState.Default:
                    break;
            }
        }
    }
}