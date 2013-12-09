// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.BackgroundTasks
{
    using System;
    using System.Linq;
    using InnerSpaceAPI;
    using LavishScriptAPI;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class Slave
    {
        private static DateTime _nextSlaveProcessState;
        private static DateTime _nextSlavetoMasterIsMasterDocked;
        private static DateTime _nextSlavetoMasterQueryLocationID;
        public static long missionLocationID;

        public Slave()
        {
            
        }

        //
        // Innerspace Method
        //
        private static void SlaveToMasterIsMasterDockedviaInnerspace()
        {
            const string RelayToWhere = "all";
            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "IsFleetSupportMasterDocked" + " " + Settings.Instance.FleetName;
            if (Settings.Instance.DebugFleetSupportSlave) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
        }

        public static void SlavetoMasterIsMasterDocked()
        {
            if (DateTime.UtcNow > _nextSlavetoMasterIsMasterDocked)
            {
                _nextSlavetoMasterIsMasterDocked = DateTime.UtcNow.AddSeconds(10);
                SlaveToMasterIsMasterDockedviaInnerspace();
            }
        }

        //
        // Innerspace Method
        //
        private static void SlaveToMasterQueryDestinationLocationIDviaInnerspace()
        {
            
        }

        private static void BroadcastEventViaInnerspace(string _eventNameToTrigger, string data1 = "", string _fleetNumber = "1", string _relayToWhere = "all")
        {
            string LavishEventToBroadcast = "uplink exec relay " + _relayToWhere + " " + "-event " + _eventNameToTrigger + " " + _fleetNumber + " " + data1;
            if (Settings.Instance.DebugFleetSupportSlave) InnerSpace.Echo(string.Format("[BroadcastEventViaInnerspace] " + LavishEventToBroadcast));
            LavishScript.ExecuteCommand(LavishEventToBroadcast);
        }

        public static void SlaveToMasterQueryDestinationLocationID()
        {
            if (DateTime.UtcNow > _nextSlavetoMasterQueryLocationID)
            {
                _nextSlavetoMasterQueryLocationID = DateTime.UtcNow.AddSeconds(20);
                BroadcastEventViaInnerspace("all", "QueryDestinationLocationID");    
            }
        }

        public static bool SetDestToMissionSystem(long locationid)
        {
            try
            {
                if (DateTime.UtcNow < Cache.Instance.LastSessionChange.AddSeconds(10))
                    return false;

                if (Cache.Instance.DirectEve.ActiveShip.Entity != null || !Cache.Instance.DirectEve.Session.IsReady)
                    return false;

                if (Cache.Instance.DirectEve.Session.SolarSystemId != locationid)
                {
                    Cache.Instance.DirectEve.Navigation.SetDestination(locationid);
                }
            }
            catch (Exception exception)
            {
                Logging.Log("QuestorUI.SetDestToSystem", "Exception [" + exception + "]", Logging.Debug);
            }


            switch (_States.CurrentQuestorState)
            {
                case QuestorState.Idle:
                    return true;

                case QuestorState.CombatMissionsBehavior:
                    if (_States.CurrentTravelerState != TravelerState.Traveling)
                    {
                        _States.CurrentTravelerState = TravelerState.Idle;
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Traveler;

                    }

                    return true;

                case QuestorState.DedicatedBookmarkSalvagerBehavior:
                    if (_States.CurrentTravelerState != TravelerState.Traveling)
                    {
                        _States.CurrentTravelerState = TravelerState.Idle;
                        _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Traveler;
                    }

                    return true;

                case QuestorState.CombatHelperBehavior:
                    if (_States.CurrentTravelerState != TravelerState.Traveling)
                    {
                        _States.CurrentTravelerState = TravelerState.Idle;
                        _States.CurrentCombatHelperBehaviorState = CombatHelperBehaviorState.Traveler;
                    }

                    return true;

                case QuestorState.BackgroundBehavior:
                    if (_States.CurrentTravelerState != TravelerState.Traveling)
                    {
                        _States.CurrentTravelerState = TravelerState.Idle;
                        _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Traveler;
                    }

                    return true;
            }

            return true;
        }

        /// <summary>
        ///   Target targets passed from master that are within targeting range
        /// </summary>
        public static bool AddPriorityTargets(long TargetNumber, long TargetID, string FleetName)
        {
            if (DateTime.UtcNow < Cache.Instance.NextTargetAction)
            {
                if (Settings.Instance.DebugTargetWrecks) Logging.Log("Salvage.TargetWrecks", "Debug: Cache.Instance.NextTargetAction is still in the future, waiting", Logging.Teal);
                return false;
            }

            if (Settings.Instance.FleetSupportMaster) return true; //no need to add targets if we are Fleet Support Master
            if (!Settings.Instance.FleetSupportSlave) return true;  //no need to add targets if we have Fleet Support Slave == false

            if (FleetName.ToLower() == Settings.Instance.FleetName.ToLower())
            {
                EntityCache TargetEntity = Cache.Instance.Entities.FirstOrDefault(e => e.Id == TargetID);

                if (TargetEntity == null)
                {
                    Logging.Log("Slave.AddTargets", "if (TargetEntity == null)", Logging.Debug);
                    return false;
                }

                if (TargetEntity.Distance < 150000)
                {
                    PrimaryWeaponPriority _priority = PrimaryWeaponPriority.PriorityKillTarget;

                    if (TargetEntity.IsSensorDampeningMe) _priority = PrimaryWeaponPriority.Dampening;
                    if (TargetEntity.IsTargetPaintingMe) _priority = PrimaryWeaponPriority.TargetPainting;
                    if (TargetEntity.IsWebbingMe) _priority = PrimaryWeaponPriority.Webbing;
                    if (TargetEntity.IsTrackingDisruptingMe) _priority = PrimaryWeaponPriority.TrackingDisrupting;
                    if (TargetEntity.IsNeutralizingMe) _priority = PrimaryWeaponPriority.Neutralizing;
                    if (TargetEntity.IsJammingMe) _priority = PrimaryWeaponPriority.Jamming;
                    if (TargetEntity.IsWarpScramblingMe) _priority = PrimaryWeaponPriority.WarpScrambler;

                    Cache.Instance.AddPrimaryWeaponPriorityTargets(new[] { TargetEntity }, _priority, "Slave: AddedFromMaster");

                    return true;
                }
                
                // we may need to travel... we need to be able to query the master to tell us his current locationId
                // warp to the master?
            }

            return true;
        }

        /// <summary>
        ///   Loot any wrecks & cargo containers close by
        /// </summary>
        
        public static void ProcessState()
        {
            if (DateTime.UtcNow < _nextSlaveProcessState) //if it has not been 100ms since the last time we ran this ProcessState return. We can't do anything that close together anyway
                return;

            _nextSlaveProcessState = DateTime.UtcNow.AddMilliseconds(500);

            // Nothing to salvage in stations
            if (Cache.Instance.InStation)
            {
                _States.CurrentSlaveState = SlaveState.IsMasterDocked;
                return;
            }

            if (!Cache.Instance.InSpace)
            {
                _States.CurrentSlaveState = SlaveState.Idle;
                return;
            }

            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
            {
                _States.CurrentSlaveState = SlaveState.Idle;
                return;
            }

            // When in warp there's nothing we can do, so ignore everything
            if (Cache.Instance.InWarp)
            {
                _States.CurrentSlaveState = SlaveState.Idle;
                return;
            }

            // There is no salving when cloaked -
            // why not? seems like we might be able to ninja-salvage with a covert-ops hauler with some additional coding (someday?)
            if (Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
            {
                _States.CurrentSlaveState = SlaveState.Idle;
                return;
            }

            if (Cache.Instance.CurrentShipsCargo == null) return;

            switch (_States.CurrentSlaveState)
            {
                case SlaveState.Idle:
                    missionLocationID = -1;
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.DirectEve.ActiveShip.Entity != null &&
                        !Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked &&
                        (Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.CombatShipName.ToLower() ||
                        Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.SalvageShipName.ToLower()) &&
                        !Cache.Instance.InWarp)
                    {
                        //_States.CurrentSlaveState = SlaveState.Begin;
                        return;
                    }
                    break;

                case SlaveState.Begin:
                    _States.CurrentSlaveState = SlaveState.AddPriorityTargets;
                    break;

                case SlaveState.Done:
                    break;

                case SlaveState.SlaveToMaster_WhatIsLocationIDofMaster:
                    //
                    // Broadcast SlaveToMaster_WhatIsLocationIDofMaster Event to all sessions
                    //

                    _States.CurrentSlaveState = SlaveState.Idle;
                    break;

                case SlaveState.SlaveToMaster_WhatIsCurrentMissionAction:
                    //
                    // Broadcast SlaveToMaster_WhatIsCurrentMissionAction Event to all sessions
                    //

                    _States.CurrentSlaveState = SlaveState.Idle;
                    break;

                case SlaveState.SlaveToMaster_WhatIsCoordofMaster:
                    //
                    // Broadcast SlaveToMaster_WhatIsCoordofMaster Event to all sessions
                    //

                    _States.CurrentSlaveState = SlaveState.Idle;
                    break;

                case SlaveState.SlaveToMaster_WhatAmmoShouldILoad:
                    //
                    // Broadcast SlaveToMaster_WhatAmmoShouldILoad Event to all sessions
                    //

                    _States.CurrentSlaveState = SlaveState.Idle;
                    break;

                default:

                    // Unknown state, goto first state
                    _States.CurrentSalvageState = SalvageState.TargetWrecks;
                    break;
            }
        }
    }
}