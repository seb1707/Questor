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
using System.Globalization;
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
using Questor.Storylines;
//using SlimDX.Direct2D;

namespace Questor.Behaviors
{
    public class CombatMissionsBehavior
    {
        //private readonly AgentInteraction _agentInteraction;
        //private readonly Arm _arm;
        private readonly SwitchShip _switchShip;
        //private readonly Combat _combat;
        private readonly CourierMissionCtrl _courierMissionCtrl;
        //private readonly Drones _drones;

        private DateTime _lastPulse;
        private DateTime _lastSalvageTrip = DateTime.MinValue;
        private readonly CombatMissionCtrl _combatMissionCtrl;
        private readonly Traveler _traveler;
        private readonly Panic _panic;
        private readonly Storyline _storyline;
        private readonly Statistics _statistics;
        //private readonly Salvage _salvage;
        private readonly UnloadLoot _unloadLoot;
        public DateTime LastAction;
        private readonly Random _random;
        private int _randomDelay;
        public static long AgentID;
        private readonly Stopwatch _watch;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private bool _firstStart = true;
        public bool PanicStateReset; //false;

        private bool ValidSettings { get; set; }

        public bool CloseQuestorFlag = true;

        public string CharacterName { get; set; }

        //DateTime _nextAction = DateTime.UtcNow;

        private DateTime _nextBookmarkRefreshCheck = DateTime.MinValue;
        private DateTime _nextBookmarksrefresh = DateTime.MinValue;
        public CombatMissionsBehavior()
        {
            _lastPulse = DateTime.MinValue;

            _traveler = new Traveler();
            _random = new Random();
            //_salvage = new Salvage();
            //_combat = new Combat();
            //_drones = new Drones();
            _unloadLoot = new UnloadLoot();
            //_agentInteraction = new AgentInteraction();
            //_arm = new Arm();
            _courierMissionCtrl = new CourierMissionCtrl();
            _switchShip = new SwitchShip();
            _combatMissionCtrl = new CombatMissionCtrl();
            _panic = new Panic();
            _storyline = new Storyline();
            _statistics = new Statistics();
            _watch = new Stopwatch();

            //
            // this is combat mission specific and needs to be generalized
            //
            Settings.Instance.SettingsLoaded += SettingsLoaded;

            // States.CurrentCombatMissionBehaviorState fixed on ExecuteMission
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            _States.CurrentUnloadLootState = UnloadLootState.Idle;
            _States.CurrentTravelerState = TravelerState.AtDestination;
        }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ApplySalvageSettings();
            ValidateCombatMissionSettings();
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

            AgentInteraction.AgentId = Cache.Instance.AgentId;
            CombatMissionCtrl.AgentId = Cache.Instance.AgentId;
            Arm.AgentId = Cache.Instance.AgentId;
            _statistics.AgentID = Cache.Instance.AgentId;
            AgentID = Cache.Instance.AgentId;
        }

        public void ApplySalvageSettings()
        {
            Salvage.Ammo = Settings.Instance.Ammo;
            Salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
            Salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
            Salvage.LootEverything = Settings.Instance.LootEverything;
        }

        private void BeginClosingQuestor()
        {
            Cache.Instance.EnteredCloseQuestor_DateTime = DateTime.UtcNow;
            _States.CurrentQuestorState = QuestorState.CloseQuestor;
        }

        private bool WeShouldBeInStationAndOutOfSessionChange()
        {
            if (!Cache.Instance.InSpace && !Cache.Instance.InStation)
            {
                if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug1", "if (!Cache.Instance.InSpace && !Cache.Instance.InStation)", Logging.Debug);
                //wait... we must be in a session change
                return false;
            }

            //
            // this state should never be reached in space. if we are in space and in this state we should switch to gotomission
            //
            if (Cache.Instance.InSpace)
            {
                Logging.Log(_States.CurrentCombatMissionBehaviorState.ToString(), "We are in space, how did we get set to this state while in space? Changing state to: GotoBase", Logging.White);
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                return false;
            }

            if (Cache.Instance.InStation)
            {
                if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug1", "if (Cache.Instance.InStation)", Logging.Debug);
                if (DateTime.UtcNow < Time.Instance.LastInSpace.AddSeconds(16))
                {
                    if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug1", "if (DateTime.UtcNow < Time.Instance.LastInSpace.AddSeconds(16))", Logging.Debug);
                    //wait... we must be in a session change
                    return false;    
                }

                return true;
            }

            Logging.Log("WeShouldBeInStationAndOutOfSessionChange", "We aren't in space, We aren't in station, waiting", Logging.Debug);
            return false;
        }

        private bool WeShouldBeInSpaceAndOutOfSessionChange()
        {
            if (!Cache.Instance.InSpace && !Cache.Instance.InStation)
            {
                //wait... we must be in a session change
                if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug2", "if (!Cache.Instance.InSpace && !Cache.Instance.InStation)", Logging.Debug);
                return false;
            }

            if (Cache.Instance.InSpace)
            {
                if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug2", "if (Cache.Instance.InSpace)", Logging.Debug);
                if (DateTime.UtcNow < Time.Instance.LastInStation.AddSeconds(16))
                {
                    if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug2", "if (DateTime.UtcNow < Time.Instance.LastInStation.AddSeconds(16))", Logging.Debug);
                    //wait... we must be in a session change
                    return false;
                }

                return true;
            }

            if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug2", "Are we not in space?! InSpace[" + Cache.Instance.InSpace + "] InStation[" + Cache.Instance.InStation + "]", Logging.Debug);
            return false;
        }

        private bool WeShouldBeInSpaceORInStationAndOutOfSessionChange()
        {
            if (!Cache.Instance.InSpace && !Cache.Instance.InStation)
            {
                if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug3", "if (!Cache.Instance.InSpace && !Cache.Instance.InStation)", Logging.Debug);
                //wait... we must be in a session change
                return false;
            }

            if ((Cache.Instance.InStation && DateTime.UtcNow < Time.Instance.LastInSpace.AddSeconds(16)) || //if we are in Station and are still in session change
                (Cache.Instance.InSpace && DateTime.UtcNow < Time.Instance.LastInStation.AddSeconds(16)))   //if we are in Space and are still in session change
            {
                if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug3", "InSpace or InStation and still in a session change", Logging.Debug);
                //wait... we must be in a session change
                return false;
            }

            if (Settings.Instance.DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange) Logging.Log("Debug3", "We are not in a session change, continue", Logging.Debug);
            return true;
        }

        private void IdleCMBState()
        {
            //
            // Note: This does not interact with EVE, no need for ANY delays in this State
            //

            if (Cache.Instance.StopBot)
            {
                //
                // this is used by the 'local is safe' routines - standings checks - at the moment is stops questor for the rest of the session.
                //
                if (Settings.Instance.DebugAutoStart || Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "DebugIdle: StopBot [" + Cache.Instance.StopBot + "]", Logging.White);
                return;
            }

            if (Cache.Instance.InSpace)
            {
                if (Settings.Instance.DebugAutoStart || Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "DebugIdle: InSpace [" + Cache.Instance.InSpace + "]", Logging.White);

                // Questor does not handle in space starts very well, head back to base to try again
                Logging.Log("CombatMissionsBehavior", "Started questor while in space, heading back to base in [" + _randomDelay + "] seconds", Logging.White);
                LastAction = DateTime.UtcNow;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.DelayedGotoBase;
                return;
            }

            _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            _States.CurrentDroneState = DroneState.Idle;
            _States.CurrentSalvageState = SalvageState.Idle;
            _States.CurrentStorylineState = StorylineState.Idle;
            _States.CurrentTravelerState = TravelerState.AtDestination;
            _States.CurrentUnloadLootState = UnloadLootState.Idle;

            if (Settings.Instance.AutoStart)
            {
                if (Settings.Instance.DebugAutoStart || Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "DebugAutoStart: Autostart [" + Settings.Instance.AutoStart + "]", Logging.White);

                // Don't start a new action an hour before downtime
                if (DateTime.UtcNow.Hour == 10)
                {
                    if (Settings.Instance.DebugAutoStart || Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "DebugIdle: Don't start a new action an hour before downtime, DateTime.UtcNow.Hour [" + DateTime.UtcNow.Hour + "]", Logging.White);
                    return;
                }

                // Don't start a new action near downtime
                if (DateTime.UtcNow.Hour == 11 && DateTime.UtcNow.Minute < 15)
                {
                    if (Settings.Instance.DebugAutoStart || Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "DebugIdle: Don't start a new action near downtime, DateTime.UtcNow.Hour [" + DateTime.UtcNow.Hour + "] DateTime.UtcNow.Minute [" + DateTime.UtcNow.Minute + "]", Logging.White);
                    return;
                }

                if (Settings.Instance.RandomDelay > 0 || Settings.Instance.MinimumDelay > 0)
                {
                    _randomDelay = (Settings.Instance.RandomDelay > 0 ? _random.Next(Settings.Instance.RandomDelay) : 0) + Settings.Instance.MinimumDelay;
                    LastAction = DateTime.UtcNow;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.DelayedStart;
                    Logging.Log("CombatMissionsBehavior", "Random start delay of [" + _randomDelay + "] seconds", Logging.White);
                    return;
                }

                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Cleanup;
                return;
            }

            if (Settings.Instance.DebugAutoStart) Logging.Log("CombatMissionsBehavior", "DebugIdle: Autostart is currently [" + Settings.Instance.AutoStart + "]", Logging.White);
            Time.Instance.LastScheduleCheck = DateTime.UtcNow;
            Questor.TimeCheck();   //Should we close questor due to stoptime or runtime?

            //Questor.WalletCheck(); //Should we close questor due to no wallet balance change? (stuck?)
        }
        
        private void DelayedStartCMBState()
        {
            if (DateTime.UtcNow.Subtract(LastAction).TotalSeconds < _randomDelay)
            {
                return;
            }

            _storyline.Reset();
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Cleanup;
            return;
        }
        
        private void DelayedGotoBaseCMBState()
        {
            if (DateTime.UtcNow.Subtract(LastAction).TotalSeconds < Time.Instance.DelayedGotoBase_seconds)
            {
                return;
            }

            Logging.Log("CombatMissionsBehavior", "Heading back to base", Logging.White);
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
            return;
        }

        private void CleanupCMBState()
        {
            // this States.CurrentCombatMissionBehaviorState is needed because forced disconnects
            // and crashes can leave "extra" cargo in the
            // cargo hold that is undesirable and causes
            // problems loading the correct ammo on occasion
            //
            if (Cache.Instance.LootAlreadyUnloaded == false)
            {
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                return;
            }

            ValidateCombatMissionSettings();
            Cleanup.CheckEVEStatus();
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Start;
            return;
        }
        
        private void StartCMBState()
        {
            if (!WeShouldBeInSpaceORInStationAndOutOfSessionChange()) return;

            if (Cache.Instance.InSpace)
            {
                // Questor does not handle in space starts very well, head back to base to try again
                Logging.Log("CombatMissionsBehavior", "Started questor while in space, heading back to base in [" + _randomDelay + "] seconds", Logging.White);
                LastAction = DateTime.UtcNow;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.DelayedGotoBase;
                return;
            }

            if (_firstStart)
            {
                //if you are in wrong station and is not first agent
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Switch;
                _firstStart = false;
                return;
            }

            Cache.Instance.OpenWrecks = false;
            if (_States.CurrentAgentInteractionState == AgentInteractionState.Idle)
            {
                Cache.Instance.Wealth = Cache.Instance.DirectEve.Me.Wealth;

                Cache.Instance.WrecksThisMission = 0;
                if (Settings.Instance.EnableStorylines && _storyline.HasStoryline())
                {
                    Logging.Log("CombatMissionsBehavior", "Storyline detected, doing storyline.", Logging.White);
                    _storyline.Reset();
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.PrepareStorylineSwitchAgents;
                    return;
                }

                Logging.Log("AgentInteraction", "Start conversation [Start Mission]", Logging.White);
                _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                AgentInteraction.Purpose = AgentInteractionPurpose.StartMission;
            }

            AgentInteraction.ProcessState();

            if (AgentInteraction.Purpose == AgentInteractionPurpose.CompleteMission) //AgentInteractionPurpose was likely changed by AgentInteraction.ProcessState()
            {
                if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
                {
                    _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                    if (Cache.Instance.CourierMission)
                    {
                        Cache.Instance.CourierMission = false;
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                        _States.CurrentQuestorState = QuestorState.Idle;
                    }
                    else
                    {
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.UnloadLoot;
                    }

                    return;
                }

                return;
            }

            if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
            {
                Questor.UpdateMissionName(AgentID);
                _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Arm;
                return;
            }

            if (_States.CurrentAgentInteractionState == AgentInteractionState.ChangeAgent)
            {
                _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                ValidateCombatMissionSettings();
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Switch;
                return;
            }

            return;
        }
        
        private void SwitchCMBState()
        {
            if (!WeShouldBeInStationAndOutOfSessionChange()) return;

            if (_States.CurrentSwitchShipState == SwitchShipState.Idle)
            {
                Logging.Log("Switch", "Begin", Logging.White);
                _States.CurrentSwitchShipState = SwitchShipState.Begin;
            }

            _switchShip.ProcessState();

            if (_States.CurrentSwitchShipState == SwitchShipState.Done)
            {
                _States.CurrentSwitchShipState = SwitchShipState.Idle;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
            }

            return;
        }
        
        private void ArmCMBState()
        {
            if (!WeShouldBeInStationAndOutOfSessionChange()) return;

            if (_States.CurrentArmState == ArmState.Idle)
            {
                if (Cache.Instance.CourierMission)
                {
                    Logging.Log("Arm", "Begin: CourierMission", Logging.White);
                    _States.CurrentArmState = ArmState.SwitchToTransportShip;
                }
                else
                {
                    Logging.Log("Arm", "Begin", Logging.White);
                    _States.CurrentArmState = ArmState.Begin;

                    // Load right ammo based on mission
                    Arm.AmmoToLoad.Clear();
                    if (Settings.Instance.WeaponGroupId != 53) //civilian guns of all types
                    {
                        //
                        // only add ammo to the AmmoToLoad if we are not using civilian guns
                        //
                        Arm.AmmoToLoad.AddRange(AgentInteraction.AmmoToLoad);
                    }
                }
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
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                return;
            }

            if (_States.CurrentArmState == ArmState.NotEnoughDrones)
            {
                // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                Logging.Log("Arm", "Armstate.NotEnoughDrones", Logging.Orange);
                _States.CurrentArmState = ArmState.Idle;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                return;
            }

            if (_States.CurrentArmState == ArmState.Done)
            {
                //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                _States.CurrentArmState = ArmState.Idle;
                _States.CurrentDroneState = DroneState.WaitingForTargets;
                _States.CurrentCombatMissionBehaviorState = Cache.Instance.CourierMission ? CombatMissionsBehaviorState.CourierMission : CombatMissionsBehaviorState.LocalWatch;
                return;
            }

            return;
        }
        
        private void LocalWatchCMBState()
        {
            if (DateTime.UtcNow < Time.Instance.NextArmAction)
            {
                //Logging.Log("Cleanup", "Closing Inventory Windows: waiting [" + Math.Round(Time.Instance.NextArmAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "]sec", Logging.White);
                return;
            }

            if (Settings.Instance.UseLocalWatch)
            {
                Time.Instance.LastLocalWatchAction = DateTime.UtcNow;
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                if (Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                {
                    Logging.Log("CombatMissionsBehavior.LocalWatch", "local is clear", Logging.White);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                    return;
                }

                Logging.Log("CombatMissionsBehavior.LocalWatch", "Bad standings pilots in local: We will stay 5 minutes in the station and then we will check if it is clear again", Logging.Orange);
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.WaitingforBadGuytoGoAway;
                return;
            }

            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
            return;
        }
        
        private void WaitingFoBadGuyToGoAway()
        {
            Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
            Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
            if (DateTime.UtcNow.Subtract(Time.Instance.LastLocalWatchAction).TotalMinutes < Time.Instance.WaitforBadGuytoGoAway_minutes + Cache.Instance.RandomNumber(1, 3))
            {
                return;
            }

            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.LocalWatch;
            return;
        }
        
        private void WarpOutBookmarkCMBState()
        {
            if (!string.IsNullOrEmpty(Settings.Instance.UndockBookmarkPrefix))
            {
                IEnumerable<DirectBookmark> warpOutBookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.UndockBookmarkPrefix ?? "");
                if (warpOutBookmarks != null && warpOutBookmarks.Any())
                {
                    DirectBookmark warpOutBookmark = warpOutBookmarks.OrderByDescending(b => b.CreatedOn).FirstOrDefault(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);

                    long solarid = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (warpOutBookmark == null)
                    {
                        Logging.Log("CombatMissionsBehavior.WarpOut", "No Bookmark", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                    }
                    else if (warpOutBookmark.LocationId == solarid)
                    {
                        if (Traveler.Destination == null)
                        {
                            Logging.Log("CombatMissionsBehavior.WarpOut", "Warp at " + warpOutBookmark.Title, Logging.White);
                            Traveler.Destination = new BookmarkDestination(warpOutBookmark);
                            Cache.Instance.DoNotBreakInvul = true;
                        }

                        Traveler.ProcessState();
                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            Logging.Log("CombatMissionsBehavior.WarpOut", "Safe!", Logging.White);
                            Cache.Instance.DoNotBreakInvul = false;
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                            Traveler.Destination = null;
                        }
                    }
                    else
                    {
                        Logging.Log("CombatMissionsBehavior.WarpOut", "No Bookmark in System", Logging.Orange);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                    }

                    return;
                }
            }

            Logging.Log("CombatMissionsBehavior.WarpOut", "No Bookmark in System", Logging.Orange);
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
            return;
        }
        
        private void GotoBaseCMBState()
        {
            //
            // we can start this in station OR in space, but we STILL do not want to do anything while in session change!
            //
            if (!WeShouldBeInSpaceORInStationAndOutOfSessionChange()) return;

            //
            // This will take a variable number of passes based on what traveler has to do.
            //
            Cache.Instance.IsMissionPocketDone = true; //pulls drones if we are not scrambled
            if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: AvoidBumpingThings()", Logging.White);
            Cache.Instance.CurrentlyShouldBeSalvaging = false;

            if (Settings.Instance.AvoidBumpingThings)
            {
                if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: if (Settings.Instance.AvoidBumpingThings)", Logging.White);
                if (NavigateOnGrid.AvoidBumpingThings(Cache.Instance.BigObjects.FirstOrDefault(), "CombatMissionsBehaviorState.GotoBase")) return; //if true then we sent a command to eve, wait a sec before doing something else w the game.
            }

            if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: Traveler.TravelHome()", Logging.White);

            Traveler.TravelHome("CombatMissionsBehavior.TravelHome");

            if (_States.CurrentTravelerState == TravelerState.AtDestination && Cache.Instance.InStation && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(Cache.Instance.RandomNumber(10, 15))) // || DateTime.UtcNow.Subtract(Cache.Instance.EnteredCloseQuestor_DateTime).TotalMinutes > 10)
            {
                if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: We are at destination", Logging.White);
                Cache.Instance.GotoBaseNow = false; //we are there - turn off the 'forced' gotobase
                if (AgentID != 0)
                {
                    try
                    {
                        Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID, true);
                    }
                    catch (Exception exception)
                    {
                        Logging.Log("CombatMissionsBehavior", "Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID); [" + exception + "]", Logging.Teal);
                    }

                    //if (Cache.Instance.Mission == null)
                    //{
                    //    Logging.Log("CombatMissionsBehavior", "Cache.Instance.Mission == null - retry on next iteration", Logging.Teal);
                    //    return;
                    //}
                }

                if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                {
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                }
                else if (_States.CurrentCombatState != CombatState.OutOfAmmo && Cache.Instance.Mission != null && Cache.Instance.Mission.State == (int)MissionState.Accepted)
                {
                    ValidateCombatMissionSettings();
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.CompleteMission;
                }
                else
                {
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.UnloadLoot;
                }

                Traveler.Destination = null;
            }

            return;
        }
        
        private void CompleteMissionCMBState()
        {
            if (!WeShouldBeInStationAndOutOfSessionChange()) return; // Do Not Continue if we are not in station and out of session change

            //
            // 2 Iterations needed to finish Complete Mission (1 sec a piece == 2 seconds)
            //
            if (_States.CurrentAgentInteractionState == AgentInteractionState.Idle)
            {
                Logging.Log("AgentInteraction", "Start Conversation [Complete Mission]", Logging.White);
                _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                AgentInteraction.Purpose = AgentInteractionPurpose.CompleteMission;
            }

            AgentInteraction.ProcessState();

            if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
            {
                _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                if (Cache.Instance.CourierMission)
                {
                    Cache.Instance.CourierMission = false;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    _States.CurrentQuestorState = QuestorState.Idle;
                }
                else if (Statistics.Instance.LastMissionCompletionError.AddSeconds(10) < DateTime.UtcNow)
                {
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Statistics;
                }
                else
                {
                    Logging.Log("CurrentCombatMissionBehavior.CompleteMission", "Skipping statistics: We have not yet completed a mission", Logging.Teal);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.UnloadLoot;
                }

                return;
            }

            return;
        }
        
        private void StatisticsCMBState()
        {
            if (Settings.Instance.UseDrones)
            {
                DirectInvType drone = null;
                Cache.Instance.DirectEve.InvTypes.TryGetValue(Settings.Instance.DroneTypeId, out drone);
                if (drone != null)
                {
                    if (!Cache.Instance.OpenDroneBay("Statistics: WriteDroneStatsLog")) return;
                    Statistics.Instance.LostDrones = (int)Math.Floor((Cache.Instance.DroneBay.Capacity - Cache.Instance.DroneBay.UsedCapacity) / drone.Volume);
                    //Logging.Log("CombatMissionsBehavior: Starting: Statistics.WriteDroneStatsLog");
                    if (!Statistics.WriteDroneStatsLog()) return;
                }
                else
                {
                    Logging.Log("DroneStats", "Could not find the drone TypeID specified in the character settings xml; this should not happen!", Logging.White);
                }
            }

            //Logging.Log("CombatMissionsBehavior: Starting: Statistics.AmmoConsumptionStatistics");
            if (!Statistics.AmmoConsumptionStatistics()) return;
            Statistics.Instance.FinishedMission = DateTime.UtcNow;

            // only attempt to write the mission statistics logs if one of the mission stats logs is enabled in settings
            if (Settings.Instance.MissionStats1Log || Settings.Instance.MissionStats3Log || Settings.Instance.MissionStats3Log)
            {
                try
                {
                    //Logging.Log("CombatMissionsBehavior.Idle", "Cache.Instance.ActiveShip.Givenname.ToLower() [" + Cache.Instance.ActiveShip.GivenName.ToLower() + "]", Logging.Teal);
                    //Logging.Log("CombatMissionsBehavior.Idle", "Settings.Instance.CombatShipName.ToLower() [" + Settings.Instance.CombatShipName.ToLower() + "]", Logging.Teal);
                    if (!Statistics.Instance.MissionLoggingCompleted)
                    {
                        if (Settings.Instance.DebugStatistics) Logging.Log("CombatMissionsBehavior.Idle", "Statistics.WriteMissionStatistics(AgentID);", Logging.Teal);
                        Statistics.WriteMissionStatistics(AgentID);
                        if (Settings.Instance.DebugStatistics) Logging.Log("CombatMissionsBehavior.Idle", "Done w Statistics.WriteMissionStatistics(AgentID);", Logging.Teal);
                        return;
                    }
                }
                catch
                {
                    Logging.Log("CombatMissionsBehavior.Idle", "if (Cache.Instance.ActiveShip != null && Cache.Instance.ActiveShip.GivenName.ToLower() == Settings.Instance.CombatShipName.ToLower())", Logging.Teal);
                }
            }

            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.UnloadLoot;
        }
        
        private void ExecuteMissionCMBState()
        {
            if (!WeShouldBeInSpaceAndOutOfSessionChange()) return; //if we are not in space and out of session change wait.

            //
            // Execution time will vary heavily on the mission!
            //

            _combatMissionCtrl.ProcessState();
            Combat.ProcessState();
            Drones.ProcessState();
            Salvage.ProcessState();
            
            // If we are out of ammo, return to base, the mission will fail to complete and the bot will reload the ship
            // and try the mission again
            if (_States.CurrentCombatState == CombatState.OutOfAmmo)
            {
                Logging.Log("Combat", "Out of Ammo!", Logging.Orange);
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;

                // Clear looted containers
                Cache.Instance.LootedContainers.Clear();
                //Cache.Instance.InvalidateBetweenMissionsCache();
            }

            if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Done)
            {
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                // Clear looted containers
                Cache.Instance.LootedContainers.Clear();
                //Cache.Instance.InvalidateBetweenMissionsCache();
            }

            // If in error state, just go home and stop the bot
            if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
            {
                Logging.Log("MissionController", "Error", Logging.Red);
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;

                // Clear looted containers
                Cache.Instance.LootedContainers.Clear();
                //Cache.Instance.InvalidateBetweenMissionsCache();
            }

            return;
        }
        
        private void TravelerCMBState()
        {
            try
            {
                if (!WeShouldBeInSpaceORInStationAndOutOfSessionChange()) return; //either in space or in station is fine, but we should not continue until we are not in a session change

                if (Settings.Instance.SpeedTank) Cache.Instance.OpenWrecks = false;

                List<int> destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                if (destination == null || destination.Count == 0)
                {
                    // happens if autopilot is not set and this QuestorState is chosen manually
                    // this also happens when we get to destination (!?)
                    Logging.Log("CombatMissionsBehavior.Traveler", "No destination?", Logging.White);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                    return;
                }

                if (destination.Count == 1 && destination.FirstOrDefault() == 0)
                {
                    destination[0] = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                }

                if (Traveler.Destination == null || Traveler.Destination.SolarSystemId != destination.LastOrDefault())
                {
                    if (Cache.Instance.AllBookmarks != null && Cache.Instance.AllBookmarks.Any()) //we might have no bookmarks...
                    {
                        IEnumerable<DirectBookmark> bookmarks = Cache.Instance.AllBookmarks.Where(b => b.LocationId == destination.LastOrDefault()).ToList();
                        if (bookmarks.FirstOrDefault() != null && bookmarks.Any())
                        {
                            Traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).FirstOrDefault());
                            return;
                        }

                        Logging.Log("CombatMissionsBehavior.Traveler", "Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]", Logging.White);
                        Traveler.Destination = new SolarSystemDestination(destination.LastOrDefault());
                        return;
                    }

                    return;
                }

                Traveler.ProcessState();

                //we also assume you are connected during a manual set of questor into travel mode (safe assumption considering someone is at the kb)
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                if (_States.CurrentTravelerState == TravelerState.AtDestination)
                {
                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                    {
                        Logging.Log("CombatMissionsBehavior.Traveler", "an error has occurred", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                        return;
                    }

                    if (Cache.Instance.InSpace)
                    {
                        Logging.Log("CombatMissionsBehavior.Traveler", "Arrived at destination (in space, Questor stopped)", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                        return;
                    }

                    Logging.Log("CombatMissionsBehavior.Traveler", "Arrived at destination", Logging.White);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    return;
                }

                return;
            }
            catch (Exception exception)
            {
                Logging.Log("CombatMissionsBehavior.Traveler", "Exception [" + exception + "]", Logging.Debug);
                return;
            }
        }
        
        private void GotoMissionCMBState()
        {
            Logging.Log("CombatMissionsBehavior", "WeShouldBeInSpaceORInStationAndOutOfSessionChange: Starting", Logging.White);
            if (!WeShouldBeInSpaceORInStationAndOutOfSessionChange()) return; //either in space or in station is fine, but we should not continue until we are not in a session change
            Logging.Log("CombatMissionsBehavior", "WeShouldBeInSpaceORInStationAndOutOfSessionChange: Done", Logging.White);
            Statistics.Instance.MissionLoggingCompleted = false;
            Cache.Instance.IsMissionPocketDone = false;

            MissionBookmarkDestination missionDestination = Traveler.Destination as MissionBookmarkDestination;

            if (missionDestination == null || missionDestination.AgentId != AgentID) // We assume that this will always work "correctly" (tm)
            {
                string nameOfBookmark = "";
                if (Settings.Instance.EveServerName == "Tranquility") nameOfBookmark = "Encounter";
                if (Settings.Instance.EveServerName == "Serenity") nameOfBookmark = "遭遇战";
                if (nameOfBookmark == "") nameOfBookmark = "Encounter";
                Logging.Log("CombatMissionsBehavior", "Setting Destination to 1st bookmark from AgentID: " + AgentID + " with [" + nameOfBookmark + "] in the title", Logging.White);
                Traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(AgentID, nameOfBookmark));
                Cache.Instance.MissionSolarSystem = Cache.Instance.DirectEve.Navigation.GetLocation(Traveler.Destination.SolarSystemId);
            }

            if (Cache.Instance.PotentialCombatTargets.Any())
            {
                Logging.Log("CombatMissionsBehavior.GotoMission", "[" + Cache.Instance.PotentialCombatTargets.Count() + "] potentialCombatTargets found , Running combat.ProcessState", Logging.White);
                Combat.ProcessState();
            }

            Traveler.ProcessState();
            
            if (_States.CurrentTravelerState == TravelerState.AtDestination)
            {
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.ExecuteMission;
                // Seeing as we just warped to the mission, start the mission controller
                _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Start;
                Traveler.Destination = null;
                return;
            }

            return;
        }
        
        private void UnloadLootCMBState()
        {
            if (!WeShouldBeInStationAndOutOfSessionChange()) return; //if we are in space GotoBase, if we are in station wait until we are out of any session changes before continuing

            if (_States.CurrentUnloadLootState == UnloadLootState.Idle)
            {
                Logging.Log("CombatMissionsBehavior", "UnloadLoot: Begin", Logging.White);
                _States.CurrentUnloadLootState = UnloadLootState.Begin;
            }

            _unloadLoot.ProcessState();

            if (_States.CurrentUnloadLootState == UnloadLootState.Done)
            {
                Cache.Instance.LootAlreadyUnloaded = true;
                _States.CurrentUnloadLootState = UnloadLootState.Idle;
                Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID, true);

                //if (Cache.Instance.Mission == null)
                //{
                //    Logging.Log("CombatMissionsBehavior", "Cache.Instance.Mission == null - retry on next iteration", Logging.Teal);
                //    return;
                //}

                if (_States.CurrentCombatState == CombatState.OutOfAmmo) // on mission
                {
                    Logging.Log("CombatMissionsBehavior.UnloadLoot", "We are out of ammo", Logging.Orange);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    return;
                }

                if ((Cache.Instance.Mission != null) && (Cache.Instance.Mission.State != (int)MissionState.Offered)) // on mission
                {
                    Logging.Log("CombatMissionsBehavior.Unloadloot", "We are on mission", Logging.White);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    return;
                }

                //This salvaging decision tree does not belong here and should be separated out into a different QuestorState
                if (Settings.Instance.AfterMissionSalvaging)
                {
                    if (Cache.Instance.GetSalvagingBookmark == null)
                    {
                        Logging.Log("CombatMissionsBehavior.Unloadloot", " No more salvaging bookmarks. Setting FinishedSalvaging Update.", Logging.White);

                        //if (Settings.Instance.CharacterMode == "Salvager")
                        //{
                        //    Logging.Log("Salvager mode set and no bookmarks making delay");
                        //    States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorStateState.Error; //or salvageonly. need to check difference
                        //}

                        if (_States.CurrentQuestorState == QuestorState.DedicatedBookmarkSalvagerBehavior)
                        {
                            Logging.Log("CombatMissionsBehavior.UnloadLoot", "Character mode is BookmarkSalvager and no bookmarks salvage.", Logging.White);

                            //We just need a NextSalvagerSession timestamp to key off of here to add the delay
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                        }
                        else
                        {
                            //Logging.Log("CombatMissionsBehavior: Character mode is not salvage going to next mission.");
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle; //add pause here
                            _States.CurrentQuestorState = QuestorState.Idle;
                        }
                        Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                        return;
                    }
                    else //There is at least 1 salvage bookmark
                    {
                        Logging.Log("CombatMissionsBehavior.Unloadloot", "There are [ " + Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Count + " ] more salvage bookmarks left to process", Logging.White);

                        // Salvage only after multiple missions have been completed
                        if (Settings.Instance.SalvageMultipleMissionsinOnePass)
                        {
                            //if we can still complete another mission before the Wrecks disappear and still have time to salvage
                            if (DateTime.UtcNow.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes > (Time.Instance.WrecksDisappearAfter_minutes - Time.Instance.AverageTimeToCompleteAMission_minutes - Time.Instance.AverageTimetoSalvageMultipleMissions_minutes))
                            {
                                Logging.Log("CombatMissionsBehavior.UnloadLoot", "The last finished after mission salvaging session was [" + DateTime.UtcNow.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes + "] ago ", Logging.White);
                                Logging.Log("CombatMissionsBehavior.UnloadLoot", "we are after mission salvaging again because it has been at least [" + (Time.Instance.WrecksDisappearAfter_minutes - Time.Instance.AverageTimeToCompleteAMission_minutes - Time.Instance.AverageTimetoSalvageMultipleMissions_minutes) + "] min since the last session. ", Logging.White);
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.CheckBookmarkAge;
                                Statistics.Instance.StartedSalvaging = DateTime.UtcNow;

                                //FIXME: should we be overwriting this timestamp here? What if this is the 3rd run back and fourth to the station?
                            }
                            else //we are salvaging mission 'in one pass' and it has not been enough time since our last run... do another mission
                            {
                                Logging.Log("CombatMissionsBehavior.UnloadLoot", "The last finished after mission salvaging session was [" + DateTime.UtcNow.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes + "] ago ", Logging.White);
                                Logging.Log("CombatMissionsBehavior.UnloadLoot", "we are going to the next mission because it has not been [" + (Time.Instance.WrecksDisappearAfter_minutes - Time.Instance.AverageTimeToCompleteAMission_minutes - Time.Instance.AverageTimetoSalvageMultipleMissions_minutes) + "] min since the last session. ", Logging.White);
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                            }
                        }
                        else //begin after mission salvaging now, rather than later
                        {
                            if (_States.CurrentQuestorState == QuestorState.DedicatedBookmarkSalvagerBehavior)
                            {
                                Logging.Log("CombatMissionsBehavior.Unloadloot", "CharacterMode: [" + Settings.Instance.CharacterMode + "], AfterMissionSalvaging: [" + Settings.Instance.AfterMissionSalvaging + "], CombatMissionsBehaviorState: [" + _States.CurrentCombatMissionBehaviorState + "]", Logging.White);
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.CheckBookmarkAge;
                                Statistics.Instance.StartedSalvaging = DateTime.UtcNow;
                            }
                            else
                            {
                                Logging.Log("CombatMissionsBehavior.UnloadLoot", "The last after mission salvaging session was [" + Math.Round(DateTime.UtcNow.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes, 0) + "min] ago ", Logging.White);
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.CheckBookmarkAge;
                                Statistics.Instance.StartedSalvaging = DateTime.UtcNow;
                            }
                        }
                    }
                }
                else
                {
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    _States.CurrentQuestorState = QuestorState.Idle;
                    Logging.Log("CombatMissionsBehavior.Unloadloot", "CharacterMode: [" + Settings.Instance.CharacterMode + "], AfterMissionSalvaging: [" + Settings.Instance.AfterMissionSalvaging + "], CombatMissionsBehaviorState: [" + _States.CurrentCombatMissionBehaviorState + "]", Logging.White);
                    return;
                }

                return;
            }

            return;
        }
        
        private void BeginAftermissionSalvagingCMBState()
        {
            Statistics.Instance.StartedSalvaging = DateTime.UtcNow; //this will be reset for each "run" between the station and the field if using <unloadLootAtStation>true</unloadLootAtStation>
            Cache.Instance.IsMissionPocketDone = false;
            Cache.Instance.CurrentlyShouldBeSalvaging = true;

            if (DateTime.UtcNow.Subtract(_lastSalvageTrip).TotalMinutes < Time.Instance.DelayBetweenSalvagingSessions_minutes && Settings.Instance.CharacterMode.ToLower() == "salvage".ToLower())
            {
                Logging.Log("CombatMissionsBehavior.BeginAftermissionSalvaging", "Too early for next salvage trip", Logging.White);
                return;
            }

            if (DateTime.UtcNow > _nextBookmarkRefreshCheck)
            {
                _nextBookmarkRefreshCheck = DateTime.UtcNow.AddMinutes(1);
                if (Cache.Instance.InStation && (DateTime.UtcNow > _nextBookmarksrefresh))
                {
                    _nextBookmarksrefresh = DateTime.UtcNow.AddMinutes(Cache.Instance.RandomNumber(2, 4));
                    Logging.Log("CombatMissionsBehavior.BeginAftermissionSalvaging", "Next Bookmark refresh in [" + Math.Round(_nextBookmarksrefresh.Subtract(DateTime.UtcNow).TotalMinutes, 0) + "min]", Logging.White);
                    Cache.Instance.DirectEve.RefreshBookmarks();
                    return;
                }

                Logging.Log("CombatMissionsBehavior.BeginAftermissionSalvaging", "Next Bookmark refresh in [" + Math.Round(_nextBookmarksrefresh.Subtract(DateTime.UtcNow).TotalMinutes, 0) + "min]", Logging.White);
            }

            if (Settings.Instance.SpeedTank || !Settings.Instance.SpeedTank)
            {
                Cache.Instance.OpenWrecks = true;
            }

            if (_States.CurrentArmState == ArmState.Idle)
            {
                _States.CurrentArmState = ArmState.SwitchToSalvageShip;
            }

            Arm.ProcessState();
            if (_States.CurrentArmState == ArmState.Done)
            {
                _States.CurrentArmState = ArmState.Idle;
                DirectBookmark bookmark = Cache.Instance.GetSalvagingBookmark;
                if (bookmark == null && Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Any())
                {
                    bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").OrderBy(b => b.CreatedOn).FirstOrDefault();
                    if (bookmark == null)
                    {
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                        return;
                    }
                }

                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                _lastSalvageTrip = DateTime.UtcNow;
                Traveler.Destination = new BookmarkDestination(bookmark);
                return;
            }

            return;
        }
        
        private void SalvageCMBState()
        {
            if (Settings.Instance.DebugSalvage) Logging.Log("CombatMissionsBehavior", "salvage:: attempting to open cargo hold", Logging.White);
            if (Cache.Instance.CurrentShipsCargo == null)
            {
                Logging.Log("CombatMissionsBehavior", "salvage:: if (Cache.Instance.CurrentShipsCargo == null)", Logging.Teal);
                return;
            }

            if (Settings.Instance.DebugSalvage) Logging.Log("CombatMissionsBehavior", "salvage:: done opening cargo hold", Logging.White);
            Cache.Instance.SalvageAll = true;
            if (Settings.Instance.SpeedTank || !Settings.Instance.SpeedTank) Cache.Instance.OpenWrecks = true;
            Cache.Instance.CurrentlyShouldBeSalvaging = true;

            EntityCache deadlyNPC = Cache.Instance.PotentialCombatTargets.FirstOrDefault();
            if (deadlyNPC != null)
            {
                // found NPCs that will likely kill out fragile salvage boat!
                List<DirectBookmark> missionSalvageBookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                Logging.Log("CombatMissionsBehavior.Salvage", "could not be completed because of NPCs left in the mission: deleting on grid salvage bookmark", Logging.White);

                if (Settings.Instance.DeleteBookmarksWithNPC)
                {
                    if (!Cache.Instance.DeleteBookmarksOnGrid("CombatMissionsBehavior.Salvage")) return;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                    DirectBookmark bookmark = missionSalvageBookmarks.OrderBy(i => i.CreatedOn).FirstOrDefault();
                    Traveler.Destination = new BookmarkDestination(bookmark);
                    return;
                }

                Logging.Log("CombatMissionsBehavior.Salvage", "could not be completed because of NPCs left in the mission: on grid salvage bookmark not deleted", Logging.Orange);
                Cache.Instance.SalvageAll = false;
                Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                return;
            }

            if (Settings.Instance.UnloadLootAtStation)
            {
                if (Cache.Instance.CurrentShipsCargo != null && Cache.Instance.CurrentShipsCargo.UsedCapacity > 0)
                {
                    if ((Cache.Instance.CurrentShipsCargo.Capacity - Cache.Instance.CurrentShipsCargo.UsedCapacity) < Settings.Instance.ReserveCargoCapacity + 10)
                    {
                        Logging.Log("CombatMissionsBehavior.Salvage", "We are full: My Cargo is at [" + Math.Round(Cache.Instance.CurrentShipsCargo.UsedCapacity, 2) + "m3] of[" + Math.Round(Cache.Instance.CurrentShipsCargo.Capacity, 2) + "] Reserve [" + Math.Round((double)Settings.Instance.ReserveCargoCapacity, 2) + "m3 + 10], go to base to unload", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                        return;        
                    }
                }
            }

            //if we have no salvagers check for UnlootedContainers before moving on
            //if we have salvagers check for any wrecks before moving on
            if ((!Cache.Instance.MyShipEntity.SalvagersAvailable && !Cache.Instance.UnlootedContainers.Any()) 
               || Cache.Instance.MyShipEntity.SalvagersAvailable && !Cache.Instance.Wrecks.Any())
            {
                if (!Cache.Instance.DeleteBookmarksOnGrid("CombatMissionsBehavior.Salvage")) return;
                Logging.Log("CombatMissionsBehavior.Salvage", "Finished salvaging the pocket. UnlootedContainers [" + Cache.Instance.UnlootedContainers.Count() + "] Wrecks [" + Cache.Instance.Wrecks + "] Salvagers? [" + Cache.Instance.MyShipEntity.SalvagersAvailable + "]", Logging.White);
                Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;

                if (!Cache.Instance.AfterMissionSalvageBookmarks.Any() && !Cache.Instance.GateInGrid())
                {
                    Logging.Log("CombatMissionsBehavior.Salvage", "We have salvaged all bookmarks, go to base", Logging.White);
                    Cache.Instance.SalvageAll = false;
                    Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    return;
                }

                if (!Cache.Instance.GateInGrid()) //no acceleration gate found
                {
                    Logging.Log("CombatMissionsBehavior.Salvage", "Go to the next salvage bookmark", Logging.White);
                    DirectBookmark bookmark;
                    if (Settings.Instance.FirstSalvageBookmarksInSystem)
                    {
                        bookmark = Cache.Instance.AfterMissionSalvageBookmarks.FirstOrDefault(c => c.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId) ?? Cache.Instance.AfterMissionSalvageBookmarks.FirstOrDefault();
                    }
                    else
                    {
                        bookmark = Cache.Instance.AfterMissionSalvageBookmarks.OrderBy(i => i.CreatedOn).FirstOrDefault() ?? Cache.Instance.AfterMissionSalvageBookmarks.FirstOrDefault();
                    }

                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                    Traveler.Destination = new BookmarkDestination(bookmark);
                    return;
                }
                
                if (Settings.Instance.UseGatesInSalvage) // acceleration gate found, are we configured to use it or not?
                {
                    Logging.Log("CombatMissionsBehavior.Salvage", "Acceleration gate found - moving to next pocket", Logging.White);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.SalvageUseGate;
                    return;
                }

                Logging.Log("CombatMissionsBehavior.Salvage", "Acceleration gate found, useGatesInSalvage set to false - Returning to base", Logging.White);
                Statistics.Instance.FinishedSalvaging = DateTime.UtcNow;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                Traveler.Destination = null;
                return;

            }

            if (Settings.Instance.DebugSalvage) Logging.Log("CombatMissionsBehavior", "salvage: we __cannot ever__ approach in salvage.cs so this section _is_ needed", Logging.White);
            Salvage.MoveIntoRangeOfWrecks();
            try
            {
                // Overwrite settings, as the 'normal' settings do not apply
                Salvage.MaximumWreckTargets = Cache.Instance.MaxLockedTargets;
                Salvage.ReserveCargoCapacity = 80;
                Salvage.LootEverything = true;
                Salvage.ProcessState();

                //Logging.Log("number of max cache ship: " + Cache.Instance.ActiveShip.MaxLockedTargets);
                //Logging.Log("number of max cache me: " + Cache.Instance.DirectEve.Me.MaxLockedTargets);
                //Logging.Log("number of max math.min: " + _salvage.MaximumWreckTargets);
            }
            finally
            {
                ApplySalvageSettings();
            }

            return;
        }
        
        private void SalvageGotoBookmarkCMBState()
        {
            Traveler.ProcessState();

            if (_States.CurrentTravelerState == TravelerState.AtDestination || Cache.Instance.GateInGrid())
            {
                //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Salvage;
                Traveler.Destination = null;
                return;
            }

            return;
        }
        
        private void SalageUseGateCMBState()
        {
            if (Settings.Instance.SpeedTank || !Settings.Instance.SpeedTank) Cache.Instance.OpenWrecks = true;

            if (Cache.Instance.AccelerationGates == null || !Cache.Instance.AccelerationGates.Any())
            {
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                return;
            }

            _lastX = Cache.Instance.ActiveShip.Entity.X;
            _lastY = Cache.Instance.ActiveShip.Entity.Y;
            _lastZ = Cache.Instance.ActiveShip.Entity.Z;

            EntityCache closest = Cache.Instance.AccelerationGates.OrderBy(t => t.Distance).FirstOrDefault();
            if (closest != null && closest.Distance < (int)Distances.DecloakRange)
            {
                Logging.Log("CombatMissionsBehavior.Salvage", "Gate found: [" + closest.Name + "] groupID[" + closest.GroupId + "]", Logging.White);

                // Activate it and move to the next Pocket
                if (closest.Activate())
                {
                    // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                    Logging.Log("CombatMissionsBehavior.Salvage", "Activate [" + closest.Name + "] and change States.CurrentCombatMissionBehaviorState to 'NextPocket'", Logging.White);
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.SalvageNextPocket;
                    _lastPulse = DateTime.UtcNow;
                    return;
                }

                return;
            }

            if (closest != null && closest.Distance < (int)Distances.WarptoDistance)
            {
                // Move to the target
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id || Cache.Instance.MyShipEntity.Velocity < 50)
                {
                    if (closest.Approach())
                    {
                        Logging.Log("CombatMissionsBehavior.Salvage", "Approaching target [" + closest.Name + "][" + Cache.Instance.MaskedID(closest.Id) + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.White);
                    }
                }
            }
            else if (closest != null)
            {
                // Probably never happens
                if (closest.WarpTo())
                {
                    Logging.Log("CombatMissionsBehavior.Salvage", "Warping to [" + closest.Name + "] which is [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.White);
                }
            }

            _lastPulse = DateTime.UtcNow.AddSeconds(10);
        }
        
        private void SalvageNextPocketCMBState()
        {
            if (Settings.Instance.SpeedTank || !Settings.Instance.SpeedTank) Cache.Instance.OpenWrecks = true;
            double distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
            if (distance > (int)Distances.NextPocketDistance)
            {
                //we know we are connected here...
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                Logging.Log("CombatMissionsBehavior.Salvage", "We have moved to the next Pocket [" + Math.Round(distance / 1000, 0) + "k away]", Logging.White);

                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Salvage;
                return;
            }

            if (DateTime.UtcNow.Subtract(_lastPulse).TotalMinutes > 2)
            {
                Logging.Log("CombatMissionsBehavior.Salvage", "We have timed out, retry last action", Logging.White);

                // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.SalvageUseGate;
                return;
            }

            return;
        }
        
        private void CMBEveryPulse()
        {
            if (Settings.Instance.DebugDisableCombatMissionsBehavior)
            {
                return;
            }
            // Only pulse state changes every 1.5s
            //if (DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < Time.Instance.QuestorPulse_milliseconds) //default: 1500ms
            //    return;
            //_lastPulse = DateTime.UtcNow;

            // Invalid settings, quit while we're ahead
            if (!ValidSettings)
            {
                if (DateTime.UtcNow.Subtract(LastAction).TotalSeconds < Time.Instance.ValidateSettings_seconds) //default is a 15 second interval
                {
                    ValidateCombatMissionSettings();
                    LastAction = DateTime.UtcNow;
                }
                return;
            }

            //If local unsafe go to base and do not start mission again (for the whole session?)
            if (Settings.Instance.FinishWhenNotSafe && (_States.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.GotoNearestStation))
            {
                //need to remove spam
                if (Cache.Instance.InSpace && !Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                {
                    EntityCache station = null;
                    if (Cache.Instance.Stations != null && Cache.Instance.Stations.Any())
                    {
                        station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    }

                    if (station != null)
                    {
                        Logging.Log("Local not safe", "Station found. Going to nearest station", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoNearestStation;
                    }
                    else
                    {
                        Logging.Log("Local not safe", "Station not found. Going back to base", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    }
                    Cache.Instance.StopBot = true;
                }
            }

            if (Cache.Instance.SessionState == "Quitting")
            {
                BeginClosingQuestor();
            }

            if (Cache.Instance.GotoBaseNow)
            {
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
            }

            if ((DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalSeconds > 10) && (DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalSeconds < 60))
            {
                if (Cache.Instance.QuestorJustStarted)
                {
                    Cache.Instance.QuestorJustStarted = false;
                    Cache.Instance.SessionState = "Starting Up";

                    // write session log
                    Statistics.WriteSessionLogStarting();
                }
            }

            Cache.Instance.InMission = _States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.ExecuteMission;
            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Storyline && _States.CurrentStorylineState == StorylineState.ExecuteMission)
            {
                Cache.Instance.InMission |= _storyline.StorylineHandler is GenericCombatStoryline && (_storyline.StorylineHandler as GenericCombatStoryline).State == GenericCombatStorylineState.ExecuteMission;
            }

            //
            // Panic always runs, not just in space
            //
            _panic.ProcessState();
            if (_States.CurrentPanicState == PanicState.Panic || _States.CurrentPanicState == PanicState.Panicking)
            {
                // If Panic is in panic state, questor is in panic States.CurrentCombatMissionBehaviorState :)
                _States.CurrentCombatMissionBehaviorState = _States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Storyline ? CombatMissionsBehaviorState.StorylinePanic : CombatMissionsBehaviorState.Panic;

                if (PanicStateReset && (DateTime.UtcNow > Time.Instance.LastSessionChange.AddSeconds(30 + Cache.Instance.RandomNumber(1, 15))))
                {
                    _States.CurrentPanicState = PanicState.Normal;
                    PanicStateReset = false;
                }
            }
            else if (_States.CurrentPanicState == PanicState.Resume)
            {
                if (Cache.Instance.InSpace || (Cache.Instance.InStation && DateTime.UtcNow > Time.Instance.LastSessionChange.AddSeconds(30 + Cache.Instance.RandomNumber(1, 15))))
                {
                    // Reset panic state
                    _States.CurrentPanicState = PanicState.Normal;

                    // Ugly storyline resume hack
                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.StorylinePanic)
                    {
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Storyline;
                        if (_storyline.StorylineHandler is GenericCombatStoryline)
                        {
                            (_storyline.StorylineHandler as GenericCombatStoryline).State = GenericCombatStorylineState.GotoMission;
                        }
                    }
                    else
                    {
                        // Head back to the mission
                        _States.CurrentTravelerState = TravelerState.Idle;
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                    }

                    return;
                }

                return;
            }

            return;
        }

        public void ProcessState()
        
        {
            CMBEveryPulse();

            switch (_States.CurrentCombatMissionBehaviorState)
            {
                case CombatMissionsBehaviorState.Idle:
                    IdleCMBState();
                    break;

                case CombatMissionsBehaviorState.DelayedStart:
                    DelayedStartCMBState();
                    break;

                case CombatMissionsBehaviorState.DelayedGotoBase:
                    DelayedGotoBaseCMBState();
                    break;

                case CombatMissionsBehaviorState.Cleanup:
                    CleanupCMBState();
                    break;

                case CombatMissionsBehaviorState.Start:
                    StartCMBState();
                    break;

                case CombatMissionsBehaviorState.Switch:
                    SwitchCMBState();
                    break;

                case CombatMissionsBehaviorState.Arm:
                    ArmCMBState();
                    break;

                case CombatMissionsBehaviorState.LocalWatch:
                    LocalWatchCMBState();
                    break;

                case CombatMissionsBehaviorState.WaitingforBadGuytoGoAway:
                    WaitingFoBadGuyToGoAway();
                    break;

                case CombatMissionsBehaviorState.WarpOutStation:
                    WarpOutBookmarkCMBState();
                    break;

                case CombatMissionsBehaviorState.GotoMission:
                    GotoMissionCMBState();
                    break;

                case CombatMissionsBehaviorState.ExecuteMission:
                    ExecuteMissionCMBState();
                    break;

                case CombatMissionsBehaviorState.GotoBase:
                    GotoBaseCMBState();
                    break;

                case CombatMissionsBehaviorState.CompleteMission:
                    CompleteMissionCMBState();
                    break;

                case CombatMissionsBehaviorState.Statistics:
                    StatisticsCMBState();
                    break;

                case CombatMissionsBehaviorState.UnloadLoot:
                    UnloadLootCMBState();
                    break;

                case CombatMissionsBehaviorState.CheckBookmarkAge:
                    if (Settings.Instance.DebugDisableCombatMissionsBehavior) Logging.Log("CombatMissionsBehaviorState", "Checking for any old bookmarks that may still need to be removed.", Logging.White);
                    if (!Cache.Instance.DeleteUselessSalvageBookmarks("RemoveOldBookmarks")) return;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.BeginAfterMissionSalvaging;
                    Statistics.Instance.StartedSalvaging = DateTime.UtcNow;
                    break;

                case CombatMissionsBehaviorState.BeginAfterMissionSalvaging:
                    BeginAftermissionSalvagingCMBState();
                    break;

                case CombatMissionsBehaviorState.GotoSalvageBookmark:
                    SalvageGotoBookmarkCMBState();
                    break;

                case CombatMissionsBehaviorState.Salvage:
                    SalvageCMBState();
                    break;

                case CombatMissionsBehaviorState.SalvageUseGate:
                    SalageUseGateCMBState();
                    break;

                case CombatMissionsBehaviorState.SalvageNextPocket:
                    SalvageNextPocketCMBState();
                    break;

                case CombatMissionsBehaviorState.PrepareStorylineSwitchAgents:                    
                    //
                    // change agents to agent #1, so we can go there and use the storyline ships (transport, shuttle, etc)
                    //
                    Cache.Instance.CurrentAgent = Cache.Instance.SwitchAgent();
                    Logging.Log("AgentInteraction", "new agent is " + Cache.Instance.CurrentAgent, Logging.Yellow);

                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.PrepareStorylineGotoBase;
                    break;

                case CombatMissionsBehaviorState.PrepareStorylineGotoBase:
                    if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "PrepareStorylineGotoBase: AvoidBumpingThings()", Logging.White);

                    if (Settings.Instance.AvoidBumpingThings)
                    {
                        if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "PrepareStorylineGotoBase: if (Settings.Instance.AvoidBumpingThings)", Logging.White);
                        NavigateOnGrid.AvoidBumpingThings(Cache.Instance.BigObjects.FirstOrDefault(), "CombatMissionsBehaviorState.PrepareStorylineGotoBase");
                    }

                    if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "PrepareStorylineGotoBase: Traveler.TravelHome()", Logging.White);

                    Traveler.TravelHome("CombatMissionsBehavior.TravelHome");

                    if (_States.CurrentTravelerState == TravelerState.AtDestination && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(5)) // || DateTime.UtcNow.Subtract(Cache.Instance.EnteredCloseQuestor_DateTime).TotalMinutes > 10)
                    {
                        if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "PrepareStorylineGotoBase: We are at destination", Logging.White);
                        Cache.Instance.GotoBaseNow = false; //we are there - turn off the 'forced' gotobase
                        if (AgentID != 0)
                        {
                            try
                            {
                                Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID, true);
                            }
                            catch (Exception exception)
                            {
                                Logging.Log("CombatMissionsBehavior", "Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID); [" + exception + "]", Logging.Teal);
                            }
                        }

                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Storyline;
                    }

                    break;

                case CombatMissionsBehaviorState.Storyline:
                    _storyline.ProcessState();

                    if (_States.CurrentStorylineState == StorylineState.Done)
                    {
                        Logging.Log("CombatMissionsBehavior.Storyline", "We have completed the storyline, returning to base", Logging.White);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                        break;
                    }
                    break;

                case CombatMissionsBehaviorState.CourierMission:

                    if (_States.CurrentCourierMissionCtrlState == CourierMissionCtrlState.Idle)
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.GotoPickupLocation;

                    _courierMissionCtrl.ProcessState();

                    if (_States.CurrentCourierMissionCtrlState == CourierMissionCtrlState.Done)
                    {
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.Idle;
                        Cache.Instance.CourierMission = false;
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    }
                    break;

                case CombatMissionsBehaviorState.Traveler:
                    TravelerCMBState();
                    break;

                case CombatMissionsBehaviorState.GotoNearestStation:
                    if (!Cache.Instance.InSpace || (Cache.Instance.InSpace && Cache.Instance.InWarp)) return;
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
                                Logging.Log("CombatMissionsBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Salvage;
                                break;
                            }
                            
                            break;
                        }

                        if (station.Distance < 1900)
                        {
                            if (station.Dock())
                            {
                                Logging.Log("CombatMissionsBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);       
                            }
                        }
                        else
                        {
                            if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id || Cache.Instance.MyShipEntity.Velocity < 50)
                            {
                                if (station.Approach())
                                {
                                    Logging.Log("CombatMissionsBehavior.GotoNearestStation", "Approaching [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.White);    
                                }
                            }   
                        }
                    }
                    else
                    {
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error; //should we goto idle here?
                    }
                    break;

                case CombatMissionsBehaviorState.Default:
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    break;
            }
        }
    }
}