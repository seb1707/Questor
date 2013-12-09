// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using Questor.Modules.Activities;

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

    public class Master
    {
        public Master()
        {
            nextTargetBroadcast = DateTime.UtcNow;
        }

        private DateTime nextTargetBroadcast;
        private DateTime _lastMasterProcessState;
        private static DateTime _lastSetDestToSystem;
        private static long _locationid;

        private static bool MasterToSlaveDestinationLocationID()
        {
            if (_States.CurrentTravelerState == TravelerState.Traveling)
            {
                _locationid = Traveler._location.LocationId;
                return true;
            }
            
            return false;
        }

        private static void MasterToSlaveSetDestToSystemviaInnerspace(long locationid)
        {
            if (DateTime.UtcNow < _lastSetDestToSystem.AddSeconds(30))
            {
                return;
            }

            _lastSetDestToSystem = DateTime.UtcNow;
            const string RelayToWhere = "all";
            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "SetDestToSystem" + " " + locationid;
            if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
        }

        private static void MasterToSlaveSetDestToSystem(long locationid)
        {
            MasterToSlaveSetDestToSystemviaInnerspace(locationid);
        }

        private static bool MasterToSlaveInStationorInSpaceviaInnerspace()
        {
            if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
            {
                string InSpaceOrInStation = "";
                if (Cache.Instance.InSpace)
                {
                    InSpaceOrInStation = "MasterIsInSpace";
                }
                else if (Cache.Instance.InStation)
                {
                    InSpaceOrInStation = "MasterIsInStation";
                }

                const string RelayToWhere = "all";
                string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + InSpaceOrInStation;
                if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
                LavishScript.ExecuteCommand(LavishCommandToBroadcast);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaveInStationorInSpace()
        {
            if (!MasterToSlaveInStationorInSpaceviaInnerspace()) return false;
            return true;
        }

        private static void MasterToSlaveWarpOutStationviaInnerspace()
        {
            const string RelayToWhere = "all";
            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "WarpOutStation";
            if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
        }

        private static void MasterToSlaveWarpOutStation()
        {
            MasterToSlaveWarpOutStationviaInnerspace();
        }

        //
        // Targeting (not yet targeted)
        //
        public static void MasterToSlaveCurrentTargetingInfo(int TargetNumber, long TargetID, int? _fleetNumber, long locationID, string TargetName)
        {
            MasterToSlaveCurrentTargetingInfoviaInnerspace(TargetNumber, TargetID, _fleetNumber, locationID, TargetName);
        }

        private static void MasterToSlaveCurrentTargetingInfoviaInnerspace(int TargetNumber, long TargetID, int? _fleetNumber, long locationID, string TargetName)
        {
            const string RelayToWhere = "all";
            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "MasterToSlaveTargetingInfo " + TargetNumber + " " + TargetID + " " + _fleetNumber + " [" + TargetName + "]";
            if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
        }

        //
        // Targeted (done targeting)
        //
        public static void MasterToSlaveCurrentTargetsInfo(int TargetNumber, long TargetID, int? _fleetNumber, long locationID, string TargetName)
        {
            MasterToSlaveCurrentTargetsInfoviaInnerspace(TargetNumber, TargetID, _fleetNumber, locationID, TargetName);
        }

        private static void MasterToSlaveCurrentTargetsInfoviaInnerspace(int TargetNumber, long TargetID, int? _fleetNumber, long locationID, string TargetName)
        {
            const string RelayToWhere = "all";
            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "MasterToSlaveTargetsInfo " + TargetNumber + " " + TargetID + " " + _fleetNumber + " [" + TargetName + "]";
            if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
        }

        private bool BroadcastCurrentTargetingInfo()
        {
            if (nextTargetBroadcast > DateTime.UtcNow && Cache.Instance.LastInStation > DateTime.UtcNow.AddSeconds(10) && Cache.Instance.InSpace)
            {
                //if (Settings.Instance.DebugMaster) 
                Logging.Log("BroadcastCurrentTargets", "if (Cache.Instance.NextTargetBroadcast > DateTime.UtcNow && Cache.Instance.LastInStation > DateTime.UtcNow.AddSeconds(10) && Cache.Instance.InSpace)", Logging.Teal);
                return false;
            }

            nextTargetBroadcast = DateTime.UtcNow.AddSeconds(10);

            if (!Cache.Instance.Targeting.Any())
            {
                if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("Master.BroadcastCurrentTargetingInfo", "if (!Cache.Instance.Targeting.Any())", Logging.Debug);
                return true;
            }

            int TargetNum = 0;
            foreach (EntityCache target in Cache.Instance.Targeting)
            {
                TargetNum++;
                MasterToSlaveCurrentTargetingInfo(TargetNum, target.Id, Settings.Instance.FleetNumber, Cache.Instance.DirectEve.Session.SolarSystemId ?? 0, target.Name);
                continue;
            }

            return true;
        }

        private bool BroadcastCurrentTargetsInfo()
        {
            if (DateTime.UtcNow > nextTargetBroadcast && Cache.Instance.LastInStation > DateTime.UtcNow.AddSeconds(10) && Cache.Instance.InSpace)
            {
                if (Settings.Instance.DebugTractorBeams) Logging.Log("BroadcastCurrentTargets", "if (Cache.Instance.NextTargetBroadcast > DateTime.UtcNow && Cache.Instance.LastInStation > DateTime.UtcNow.AddSeconds(10) && Cache.Instance.InSpace)", Logging.Teal);
                return false;
            }

            

            if (!Cache.Instance.Targets.Any())
            {
                if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("Master.BroadcastCurrentTargetsInfo", "if (!Cache.Instance.Targets.Any())", Logging.Debug);
                nextTargetBroadcast = DateTime.UtcNow.AddSeconds(10);
                return true;
            }

            long _locationID = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
            if (_locationID == -1)
            {
                return false;
            }

            int TargetNum = 0;
            foreach (EntityCache target in Cache.Instance.Targets)
            {
                TargetNum++;
                MasterToSlaveCurrentTargetsInfo(TargetNum, target.Id, Settings.Instance.FleetNumber, _locationID, target.Name);
                
                continue;
            }

            if (Cache.Instance.TargetedBy.Count() >= Cache.Instance.Targets.Count())
            {
                nextTargetBroadcast = DateTime.UtcNow.AddSeconds(3);
            }
            
            return true;
        }

       public void ProcessState()
        {
            if (DateTime.UtcNow < _lastMasterProcessState.AddMilliseconds(300)) //if it has not been 100ms since the last time we ran this ProcessState return. We can't do anything that close together anyway
                return;

            _lastMasterProcessState = DateTime.UtcNow;

            if (!Settings.Instance.FleetSupportMaster)
            {
                Logging.Log("Master.ProcessState", "if (!Settings.Instance.FleetSupportMaster)", Logging.Debug);
                return;
            }

            // Nothing to salvage in stations
            if (Cache.Instance.InStation)
            {
                _States.CurrentMasterState = MasterState.Idle;
                return;
            }

            if (!Cache.Instance.InSpace)
            {
                _States.CurrentMasterState = MasterState.Idle;
                return;
            }

            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
            {
                _States.CurrentMasterState = MasterState.Idle;
                return;
            }

            // There are no targets when cloaked -
            if (Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
            {
                _States.CurrentMasterState = MasterState.Idle;
                return;
            }

            switch (_States.CurrentMasterState)
            {
               case MasterState.Begin:
                    
                    _States.CurrentMasterState = MasterState.BroadcastTargets;
                    break;

               case MasterState.InStationOrInSpace:
                    if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
                    {
                        if (!MasterToSlaveInStationorInSpace()) return;
                        _States.CurrentMasterState = MasterState.Idle;
                    }
                    break;

               case MasterState.BroadcastTargets:

                    //if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("Master", "State changed to: [" + _States.CurrentMasterState.ToString() + "]", Logging.Debug);
                    //Logging.Log("Master", "BroadcastTargets", Logging.White);

                    // When in warp there's nothing we can do, so ignore everything
                    if (!Cache.Instance.InWarp && DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(15))
                    {
                        if (!BroadcastCurrentTargetingInfo()) return;
                        if (!BroadcastCurrentTargetsInfo()) return;
                    }

                    _States.CurrentMasterState = MasterState.Idle;
                    break;

               case MasterState.DestinationLocationID:
                    if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
                    {
                        if (!MasterToSlaveDestinationLocationID()) return;
                        _States.CurrentMasterState = MasterState.Idle;
                    }

                    break;

               case MasterState.Other2:
                    //Logging.Log("Master", "Other2", Logging.White);
                    _States.CurrentMasterState = MasterState.BroadcastTargets;
                    break;

               case MasterState.Idle:
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.DirectEve.ActiveShip.Entity != null &&
                        !Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked &&
                        (Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.CombatShipName.ToLower() ||
                        Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.SalvageShipName.ToLower()) &&
                        !Cache.Instance.InWarp)
                    {
                        //_States.CurrentMasterState = MasterState.Begin;
                        return;
                    }
                    break;

                default:

                    // Unknown state, goto first state
                    _States.CurrentSalvageState = SalvageState.TargetWrecks;
                    break;
            }
        }
    }
}