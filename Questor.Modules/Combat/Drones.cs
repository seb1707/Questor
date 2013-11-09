// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using System.Threading;
using System.Windows.Forms;

namespace Questor.Modules.Combat
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    /// <summary>
    ///   The drones class will manage any and all drone related combat
    /// </summary>
    /// <remarks>
    ///   Drones will always work their way from lowest value target to highest value target and will only attack entities (not structures)
    /// </remarks>
    public static class Drones
    {
        public static int DronesInstances = 0;

        static Drones()
        {
            Interlocked.Increment(ref DronesInstances);
        }

        //~Drones()
        //{
        //    Interlocked.Decrement(ref DronesInstances);
        //}

        private static double _armorPctTotal;
        private static int _lastDroneCount;
        private static DateTime _lastEngageCommand;
        private static DateTime _lastRecallCommand;

        private static int _recallCount;
        private static DateTime _lastLaunch;
        private static DateTime _lastRecall;

        private static DateTime _launchTimeout;
        private static int _launchTries;
        private static double _shieldPctTotal;
        private static double _structurePctTotal;
        public static bool Recall; //false
        public static bool WarpScrambled; //false
        private static DateTime _nextDroneAction = DateTime.UtcNow;
        private static DateTime _nextWarpScrambledWarning = DateTime.MinValue;

        private static void GetDamagedDrones()
        {
            foreach (EntityCache drone in Cache.Instance.ActiveDrones)
            {
                if (Settings.Instance.DebugDroneHealth) Logging.Log("Drones: GetDamagedDrones", "Health[" + drone.Health + "]" + "S[" + Math.Round(drone.ShieldPct, 3) + "]" + "A[" + Math.Round(drone.ArmorPct, 3) + "]" + "H[" + Math.Round(drone.StructurePct, 3) + "][ID" + drone.Id + "]", Logging.White);
            }
            Cache.Instance.DamagedDrones = Cache.Instance.ActiveDrones.Where(d => d.Health < Settings.Instance.BelowThisHealthLevelRemoveFromDroneBay);
        }

        private static double GetShieldPctTotal()
        {
            if (!Cache.Instance.ActiveDrones.Any())
                return 0;

            return Cache.Instance.ActiveDrones.Sum(d => d.ShieldPct);
        }

        private static double GetArmorPctTotal()
        {
            if (!Cache.Instance.ActiveDrones.Any())
                return 0;

            return Cache.Instance.ActiveDrones.Sum(d => d.ArmorPct);
        }

        private static double GetStructurePctTotal()
        {
            if (!Cache.Instance.ActiveDrones.Any())
                return 0;

            return Cache.Instance.ActiveDrones.Sum(d => d.StructurePct);
        }

        /// <summary>
        ///   Engage the target
        /// </summary>
        private static void EngageTarget()
        {
            if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "Entering EngageTarget()", Logging.Debug);
                    
            // Find the first active weapon's target
            //TargetingCache.CurrentDronesTarget = Cache.Instance.EntityById(_lastTarget);

            if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "GetBestDroneTarget: MaxDroneRange [" + Cache.Instance.MaxDroneRange + "]);", Logging.Debug);
            // Return best possible low value target

            if (Cache.Instance.PreferredDroneTarget == null || !Cache.Instance.PreferredDroneTarget.IsFrigate)
            {
                Cache.Instance.GetBestDroneTarget(Cache.Instance.MaxDroneRange, !Cache.Instance.DronesKillHighValueTargets, "Drones");
            }    
            
            EntityCache DroneToShoot = Cache.Instance.PreferredDroneTarget;

            if (DroneToShoot == null)
            {
                if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "GetBestDroneTarget: PreferredDroneTarget is null, picking a target using a simple ruleset...", Logging.Debug);
                if (Cache.Instance.Targets.Any(i => !i.IsContainer && !i.IsBadIdea))
                {
                    DroneToShoot = Cache.Instance.Targets.Where(i => !i.IsContainer && !i.IsBadIdea && i.Distance < Cache.Instance.MaxDroneRange).OrderByDescending(i => i.IsWarpScramblingMe).ThenByDescending(i => i.IsFrigate).ThenBy(i => i.Distance).FirstOrDefault();
                }
            }

            if (DroneToShoot != null)
            {

                if (DroneToShoot.IsReadyToShoot && DroneToShoot.Distance < Cache.Instance.MaxDroneRange)
                {
                    if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (DroneToShoot != null && DroneToShoot.IsReadyToShoot && DroneToShoot.Distance < Cache.Instance.MaxDroneRange)", Logging.Debug);

                     // Nothing to engage yet, probably retargeting
                    if (!DroneToShoot.IsTarget)
                    {
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (!DroneToShoot.IsTarget)", Logging.Debug);
                        return;
                    }

                    if (DroneToShoot.IsBadIdea) //&& !DroneToShoot.IsAttacking)
                    {
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (DroneToShoot.IsBadIdea && !DroneToShoot.IsAttacking) return;", Logging.Debug);
                        return;
                    }

                    // Is our current target still the same and is the last Engage command no longer then 15s ago?
                    if (Cache.Instance.LastDroneTargetID == DroneToShoot.Id && DateTime.UtcNow.Subtract(_lastEngageCommand).TotalSeconds < 15)
                    {
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (_lastTarget == target.Id && DateTime.UtcNow.Subtract(_lastEngageCommand).TotalSeconds < 15)", Logging.Debug);
                        return;
                    }

                    // Are we still actively shooting at the target?
                    bool mustEngage = false;
                    foreach (EntityCache drone in Cache.Instance.ActiveDrones)
                    {
                        mustEngage |= drone.FollowId != Cache.Instance.PreferredDroneTargetID;
                    }

                    if (!mustEngage)
                    {
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (!mustEngage)", Logging.Debug);
                        return;
                    }

                    // Is the last target our current active target?
                    if (DroneToShoot.IsActiveTarget)
                    {
                        // Save target id (so we do not constantly switch)
                        Cache.Instance.LastDroneTargetID = DroneToShoot.Id;

                        // Engage target
                        Logging.Log("Drones", "Engaging [ " + Cache.Instance.ActiveDrones.Count() + " ] drones on [" + DroneToShoot.Name + "][ID: " + Cache.Instance.MaskedID(DroneToShoot.Id) + "]" + Math.Round(DroneToShoot.Distance / 1000, 0) + "k away]", Logging.Magenta);
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdDronesEngage);
                        _lastEngageCommand = DateTime.UtcNow;
                    }
                    else // Make the target active
                    {
                        if (DateTime.UtcNow > Cache.Instance.NextMakeActiveTargetAction)
                        {
                            DroneToShoot.MakeActiveTarget();
                            Logging.Log("Drones", "[" + DroneToShoot.Name + "][ID: " + Cache.Instance.MaskedID(DroneToShoot.Id) + "]IsActiveTarget[" + DroneToShoot.IsActiveTarget + "][" + Math.Round(DroneToShoot.Distance / 1000, 0) + "k away] has been made the ActiveTarget (needed for drones)", Logging.Magenta);
                            Cache.Instance.NextMakeActiveTargetAction = DateTime.UtcNow.AddSeconds(5 + Cache.Instance.RandomNumber(0, 3));
                        }
                    }
                }

                if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (DroneToShoot != null && DroneToShoot.IsReadyToShoot && DroneToShoot.Distance < Cache.Instance.MaxDroneRange)", Logging.Debug);
                return;
            }

            if (Settings.Instance.DebugDrones) Logging.Log("Drones.EngageTarget", "if (Cache.Instance.PreferredDroneTargetID != null)", Logging.Debug);
            return;
        }

        public static void ProcessState()
        {
            if (_nextDroneAction > DateTime.UtcNow || Settings.Instance.DebugDisableDrones) return;

            if (Settings.Instance.DebugDrones) Logging.Log("Drones.ProcessState", "Entering Drones.ProcessState", Logging.Debug);
            _nextDroneAction = DateTime.UtcNow.AddMilliseconds(800);

            if (Cache.Instance.InStation ||                             // There is really no combat in stations (yet)
                !Cache.Instance.InSpace ||                              // if we are not in space yet, wait...
                Cache.Instance.MyShipEntity == null ||   // What? No ship entity?
                Cache.Instance.ActiveShip.Entity.IsCloaked || // There is no combat when cloaked
                !Cache.Instance.UseDrones                               // if UseDrones is false
                )
            {
                _States.CurrentDroneState = DroneState.Idle;
                return;
            }

            if (Cache.Instance.MyShipEntity.IsShipWithNoDroneBay)
            {
                _States.CurrentDroneState = DroneState.Idle;
                return;
            }

            if ((!Cache.Instance.ActiveDrones.Any() && Cache.Instance.InWarp) || !Cache.Instance.EntitiesOnGrid.Any())
            {
                Cache.Instance.RemoveDronePriorityTargets(Cache.Instance.DronePriorityEntities);
                _States.CurrentDroneState = DroneState.Idle;
                return;
            }

            switch (_States.CurrentDroneState)
            {
                case DroneState.WaitingForTargets:

                    // Are we in the right state ?
                    if (Cache.Instance.ActiveDrones.Any())
                    {
                        // Apparently not, we have drones out, go into fight mode
                        _States.CurrentDroneState = DroneState.Fighting;
                        break;
                    }

                    if (Cache.Instance.Targets.Any() || Settings.Instance.DronesDontNeedTargetsBecauseWehaveThemSetOnAggressive)
                    {
                        // Should we launch drones?
                        bool launch = true;

                        // Always launch if we're scrambled
                        if (!Cache.Instance.PotentialCombatTargets.Any(pt => pt.IsWarpScramblingMe))
                        {
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "Launch is [" + launch + "]", Logging.Debug);
                            launch &= Cache.Instance.UseDrones;
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", " launch &= Cache.Instance.UseDrones; Launch is [" + launch + "]", Logging.Debug);
                            // Are we done with this mission pocket?
                            launch &= !Cache.Instance.IsMissionPocketDone;
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "!Cache.Instance.IsMissionPocketDone; Launch is [" + launch + "]", Logging.Debug);
                            // If above minimums
                            launch &= Cache.Instance.ActiveShip.ShieldPercentage >= Settings.Instance.DroneMinimumShieldPct;
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "ActiveShip.ShieldPercentage; Launch is [" + launch + "]", Logging.Debug);
                            launch &= Cache.Instance.ActiveShip.ArmorPercentage >= Settings.Instance.DroneMinimumArmorPct;
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "ActiveShip.ArmorPercentage; Launch is [" + launch + "]", Logging.Debug);
                            launch &= Cache.Instance.ActiveShip.CapacitorPercentage >= Settings.Instance.DroneMinimumCapacitorPct;
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "ActiveShip.CapacitorPercentage; Launch is [" + launch + "]", Logging.Debug);

                            // yes if there are targets to kill
                            launch &= (Cache.Instance.Aggressed.Count(e => e.Distance < Cache.Instance.MaxDroneRange && !e.IsSentry) > 0 || Cache.Instance.Targets.Count(e => e.IsLargeCollidable) > 0);
                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "Cache.Instance.Aggressed.Count; Launch is [" + launch + "] MaxDroneRange [" + Cache.Instance.MaxDroneRange + "] DroneControlrange [" + Settings.Instance.DroneControlRange + "] TargetingRange [" + Cache.Instance.MaxTargetRange + "]", Logging.Debug);

                            if (_States.CurrentQuestorState != QuestorState.CombatMissionsBehavior)
                            {
                                launch &= Cache.Instance.EntitiesOnGrid.Count(e => !e.IsSentry && !e.IsBadIdea && e.CategoryId == (int)CategoryID.Entity && e.IsNpc && !e.IsContainer && !e.IsLargeCollidable && e.Distance < Cache.Instance.MaxDroneRange) > 0;
                                if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "Cache.Instance.Entities.Count; Launch is [" + launch + "]", Logging.Debug);
                            }

                            if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "Launch is [" + launch + "]", Logging.Debug);
                            // If drones get aggro'd within 30 seconds, then wait (5 * _recallCount + 5) seconds since the last recall
                            if (_lastLaunch < _lastRecall && _lastRecall.Subtract(_lastLaunch).TotalSeconds < 30)
                            {
                                if (_lastRecall.AddSeconds(5 * _recallCount + 5) < DateTime.UtcNow)
                                {
                                    // Increase recall count and allow the launch
                                    _recallCount++;

                                    // Never let _recallCount go above 5
                                    if (_recallCount > 5)
                                        _recallCount = 5;
                                }
                                else
                                {
                                    // Do not launch the drones until the delay has passed
                                    if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "We are still in _lastRecall delay. Launch is [" + launch + "]", Logging.Debug);
                                    launch = false;
                                }
                            }
                            else // Drones have been out for more then 30s
                                _recallCount = 0;
                        }
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.WaitingForTargets", "Launch is [" + launch + "]", Logging.Debug);
                        if (launch)
                        {
                            // Reset launch tries
                            _launchTries = 0;
                            _lastLaunch = DateTime.UtcNow;
                            _States.CurrentDroneState = DroneState.Launch;
                        }
                    }
                    break;

                case DroneState.Launch:
                    if (Settings.Instance.DebugDrones) Logging.Log("Drones.Launch", "LaunchAllDrones", Logging.Debug);
                    // Launch all drones
                    Recall = false;
                    _launchTimeout = DateTime.UtcNow;
                    Cache.Instance.ActiveShip.LaunchAllDrones();
                    _States.CurrentDroneState = DroneState.Launching;
                    break;

                case DroneState.Launching:
                    if (Settings.Instance.DebugDrones) Logging.Log("Drones.Launching", "Entering Launching State...", Logging.Debug);
                    // We haven't launched anything yet, keep waiting
                    if (!Cache.Instance.ActiveDrones.Any())
                    {
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.Launching", "No Drones in space yet. waiting", Logging.Debug);
                        if (DateTime.UtcNow.Subtract(_launchTimeout).TotalSeconds > 10)
                        {
                            // Relaunch if tries < 5
                            if (_launchTries < 5)
                            {
                                _launchTries++;
                                _States.CurrentDroneState = DroneState.Launch;
                                break;
                            }

                            _States.CurrentDroneState = DroneState.OutOfDrones;
                        }
                        break;
                    }

                    // Are we done launching?
                    if (_lastDroneCount == Cache.Instance.ActiveDrones.Count())
                    {
                        Logging.Log("Drones", "[" + Cache.Instance.ActiveDrones.Count() + "] Drones Launched", Logging.Magenta);
                        _States.CurrentDroneState = DroneState.Fighting;
                    }
                    break;

                case DroneState.OutOfDrones:

                    if (Cache.Instance.UseDrones && Settings.Instance.CharacterMode == "CombatMissions" && _States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.ExecuteMission)
                    {
                        if (Statistics.Instance.OutOfDronesCount >= 3)
                        {
                            Logging.Log("Drones", "We are Out of Drones! AGAIN - Headed back to base to stay!", Logging.Red);
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                            Statistics.Instance.MissionCompletionErrors = 10; //this effectively will stop questor in station so we do not try to do this mission again, this needs human intervention if we have lots this many drones
                            Statistics.Instance.OutOfDronesCount++;
                        }

                        Logging.Log("Drones","We are Out of Drones! - Headed back to base to Re-Arm",Logging.Red);
                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                        Statistics.Instance.OutOfDronesCount++;
                        return;
                    }

                    Cache.Instance.LastDroneTargetID = 0;
                    break;

                case DroneState.Fighting:
                    if (Settings.Instance.DebugDrones) Logging.Log("Drones.Fighting", "Should we recall our drones? This is a possible list of reasons why we should", Logging.Debug);

                    if (!Cache.Instance.ActiveDrones.Any())
                    {
                        Logging.Log("Drones", "Apparently we have lost all our drones", Logging.Orange);
                        Recall = true;
                    }
                    else
                    {
                        if (Cache.Instance.PotentialCombatTargets.Any(pt => pt.IsWarpScramblingMe))
                        {
                            EntityCache WarpScrambledBy = Cache.Instance.Targets.OrderBy(d => d.Distance).ThenByDescending(i => i.IsWarpScramblingMe).FirstOrDefault();
                            if (WarpScrambledBy != null && DateTime.UtcNow > _nextWarpScrambledWarning)
                            {
                                _nextWarpScrambledWarning = DateTime.UtcNow.AddSeconds(20);
                                Logging.Log("Drones", "We are scrambled by: [" + Logging.White + WarpScrambledBy.Name + Logging.Orange + "][" + Logging.White + Math.Round(WarpScrambledBy.Distance, 0) + Logging.Orange + "][" + Logging.White + WarpScrambledBy.Id + Logging.Orange + "]", Logging.Orange);
                                Recall = false;
                                WarpScrambled = true;
                            }
                        }
                        else
                        {
                            //Logging.Log("Drones: We are not warp scrambled at the moment...");
                            WarpScrambled = false;
                        }
                    }

                    if (!Recall)
                    {
                        // Are we done (for now) ?
                        if (
                            Cache.Instance.TargetedBy.Count(e => !e.IsSentry 
                                                               && (e.IsNpc || e.IsNpcByGroupID)
                                                               && e.Distance < Cache.Instance.MaxDroneRange) == 0)
                        {
                            int TargtedByCount = 0;
                            if (Cache.Instance.TargetedBy.Any())
                            {
                                TargtedByCount = Cache.Instance.TargetedBy.Count();
                            }
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because no NPC is targeting us within [" + Cache.Instance.MaxDroneRange + "] DroneControlRange Is [" + Settings.Instance.DroneControlRange + "] Targeting Range Is [" + Cache.Instance.MaxTargetRange + "] We have [" + TargtedByCount + "] total things targeting us", Logging.Magenta);
                            Recall = true;
                        }

                        if (!Recall & (Cache.Instance.IsMissionPocketDone) && !WarpScrambled)
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because we are done with this pocket.", Logging.Magenta);
                            Recall = true;
                        }
                        else if (!Recall & (_shieldPctTotal > GetShieldPctTotal()))
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because drones have lost some shields! [Old: " +
                                        _shieldPctTotal.ToString("N2") + "][New: " + GetShieldPctTotal().ToString("N2") +
                                        "]", Logging.Magenta);
                            Recall = true;
                        }
                        else if (!Recall & (_armorPctTotal > GetArmorPctTotal()))
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because drones have lost some armor! [Old:" +
                                        _armorPctTotal.ToString("N2") + "][New: " + GetArmorPctTotal().ToString("N2") +
                                        "]", Logging.Magenta);
                            Recall = true;
                        }
                        else if (!Recall & (_structurePctTotal > GetStructurePctTotal()))
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because drones have lost some structure! [Old:" +
                                        _structurePctTotal.ToString("N2") + "][New: " +
                                        GetStructurePctTotal().ToString("N2") + "]", Logging.Magenta);
                            Recall = true;
                        }
                        else if (!Recall & (Cache.Instance.ActiveDrones.Count() < _lastDroneCount))
                        {
                            // Did we lose a drone? (this should be covered by total's as well though)
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because we have lost a drone! [Old:" + _lastDroneCount +
                                        "][New: " + Cache.Instance.ActiveDrones.Count() + "]", Logging.Orange);
                            Recall = true;
                        }
                        else if (!Recall)
                        {
                            // Default to long range recall
                            int lowShieldWarning = Settings.Instance.LongRangeDroneRecallShieldPct;
                            int lowArmorWarning = Settings.Instance.LongRangeDroneRecallArmorPct;
                            int lowCapWarning = Settings.Instance.LongRangeDroneRecallCapacitorPct;

                            if (Cache.Instance.ActiveDrones.Average(d => d.Distance) <
                                (Cache.Instance.MaxDroneRange / 2d))
                            {
                                lowShieldWarning = Settings.Instance.DroneRecallShieldPct;
                                lowArmorWarning = Settings.Instance.DroneRecallArmorPct;
                                lowCapWarning = Settings.Instance.DroneRecallCapacitorPct;
                            }

                            if (!Cache.Instance.Targets.Any() && !Settings.Instance.DronesDontNeedTargetsBecauseWehaveThemSetOnAggressive)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to [" + Cache.Instance.Targets.Count() + "] targets being locked. Locking [" + Cache.Instance.Targeting.Count() + "] targets atm", Logging.Orange);
                                Recall = true;
                            }

                            if (Cache.Instance.ActiveShip.ShieldPercentage < lowShieldWarning && !WarpScrambled)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to shield [" +
                                            Math.Round(Cache.Instance.ActiveShip.ShieldPercentage, 0) + "%] below [" +
                                            lowShieldWarning + "%] minimum", Logging.Orange);
                                Recall = true;
                            }
                            else if (Cache.Instance.ActiveShip.ArmorPercentage < lowArmorWarning && !WarpScrambled)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to armor [" +
                                            Math.Round(Cache.Instance.ActiveShip.ArmorPercentage, 0) + "%] below [" +
                                            lowArmorWarning + "%] minimum", Logging.Orange);
                                Recall = true;
                            }
                            else if (Cache.Instance.ActiveShip.CapacitorPercentage < lowCapWarning && !WarpScrambled)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to capacitor [" +
                                            Math.Round(Cache.Instance.ActiveShip.CapacitorPercentage, 0) + "%] below [" +
                                            lowCapWarning + "%] minimum", Logging.Orange);
                                Recall = true;
                            }
                            else if (_States.CurrentQuestorState == QuestorState.CombatMissionsBehavior && !WarpScrambled)
                            {
                                if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoBase && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to gotobase state", Logging.Orange);
                                    Recall = true;
                                }
                                else if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoMission && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to gotomission state", Logging.Orange);
                                    Recall = true;
                                }
                                else if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Panic && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to panic state", Logging.Orange);
                                    Recall = true;
                                }
                            }
                            else if (_States.CurrentQuestorState == QuestorState.CombatHelperBehavior && !WarpScrambled)
                            {
                                if (_States.CurrentCombatHelperBehaviorState == CombatHelperBehaviorState.Panic && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to panic state", Logging.Orange);
                                    Recall = true;
                                }
                                else if (_States.CurrentCombatHelperBehaviorState == CombatHelperBehaviorState.GotoBase && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to panic state", Logging.Orange);
                                    Recall = true;
                                }
                            }
                            else if (_States.CurrentQuestorState == QuestorState.DedicatedBookmarkSalvagerBehavior && !WarpScrambled)
                            {
                                if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.GotoBase && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to gotobase state", Logging.Orange);
                                    Recall = true;
                                }
                                else if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Panic && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to panic state", Logging.Orange);
                                    Recall = true;
                                }
                                else if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation && !WarpScrambled)
                                {
                                    Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to GotoNearestStation state", Logging.Orange);
                                    Recall = true;
                                }
                            }
                        }
                    }

                    // Recall or engage
                    if (Recall)
                    {
                        Statistics.Instance.DroneRecalls++;
                        _States.CurrentDroneState = DroneState.Recalling;
                    }
                    else
                    {
                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.Fighting", "EngageTarget(); - before", Logging.Debug);
                    
                        EngageTarget();

                        if (Settings.Instance.DebugDrones) Logging.Log("Drones.Fighting", "EngageTarget(); - after", Logging.Debug);
                        // We lost a drone and did not recall, assume panicking and launch (if any) additional drones
                        if (Cache.Instance.ActiveDrones.Count() < _lastDroneCount)
                        {
                            _States.CurrentDroneState = DroneState.Launch;
                        }
                    }
                    break;

                case DroneState.Recalling:

                    // Are we done?
                    if (!Cache.Instance.ActiveDrones.Any())
                    {
                        _lastRecall = DateTime.UtcNow;
                        Recall = false;
                        Cache.Instance.LastDroneTargetID = 0;
                        _nextDroneAction = DateTime.UtcNow.AddSeconds(3);
                        _States.CurrentDroneState = DroneState.WaitingForTargets;
                        break;
                    }

                    // Give recall command every 15 seconds
                    if (DateTime.UtcNow.Subtract(_lastRecallCommand).TotalSeconds > 15)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdDronesReturnToBay);
                        _lastRecallCommand = DateTime.UtcNow;
                    }
                    break;

                case DroneState.Idle:

                    //
                    // below is the reasons we will start the combat state(s) - if the below is not met do nothing
                    //
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.ActiveShip.Entity != null &&
                        !Cache.Instance.ActiveShip.Entity.IsCloaked &&
                        Cache.Instance.ActiveShip.GivenName.ToLower() != Settings.Instance.CombatShipName &&
                        Cache.Instance.UseDrones &&
                        !Cache.Instance.InWarp)
                    {
                        _States.CurrentDroneState = DroneState.WaitingForTargets;
                        return;
                    }

                    Cache.Instance.LastDroneTargetID = 0;
                    break;
            }

            // Update health values
            _shieldPctTotal = GetShieldPctTotal();
            _armorPctTotal = GetArmorPctTotal();
            _structurePctTotal = GetStructurePctTotal();
            _lastDroneCount = Cache.Instance.ActiveDrones.Count();
            GetDamagedDrones();
        }
    }
}