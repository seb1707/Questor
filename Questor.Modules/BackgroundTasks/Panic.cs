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
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    //using global::Questor.Modules.Actions;
    //using global::Questor.Modules.Activities;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class Panic
    {
        private readonly Random _random = new Random();

        private double _lastNormalX;
        private double _lastNormalY;
        private double _lastNormalZ;

        private DateTime _resumeTime;
        private DateTime _nextWarpScrambledWarning = DateTime.UtcNow;
        private DateTime _nextPanicProcessState;

        private DateTime _lastWarpScrambled = DateTime.UtcNow;
        private DateTime _lastPriorityTargetLogging = DateTime.UtcNow;
        private bool _delayedResume;
        private int _randomDelay;
        private int BookmarkMyWreckAttempts;
        private int icount = 1;

        //public bool InMission { get; set; }

        public void ProcessState()
        {
            // Only pulse state changes every 500ms
            if (DateTime.UtcNow < _nextPanicProcessState || Settings.Instance.DebugDisablePanic) //default: 500ms
                return;

            _nextPanicProcessState = DateTime.UtcNow.AddMilliseconds(500);

            switch (_States.CurrentPanicState)
            {
                case PanicState.Idle:

                    //
                    // below is the reasons we will start the panic state(s) - if the below is not met do nothing
                    //
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.ActiveShip.Entity != null &&
                        !Cache.Instance.ActiveShip.Entity.IsCloaked)
                    {
                        _States.CurrentPanicState = PanicState.Normal;
                        return;
                    }
                    break;

                case PanicState.Normal:
                    if (Cache.Instance.InStation)
                    {
                        _States.CurrentPanicState = PanicState.Idle;
                    }

                    if (Cache.Instance.ActiveShip.Entity != null)
                    {
                        _lastNormalX = Cache.Instance.ActiveShip.Entity.X;
                        _lastNormalY = Cache.Instance.ActiveShip.Entity.Y;
                        _lastNormalZ = Cache.Instance.ActiveShip.Entity.Z;
                    }

                    if (Cache.Instance.ActiveShip.Entity == null)
                        return;

                    if (DateTime.UtcNow < Time.Instance.LastSessionChange.AddSeconds(10))
                        return;

                    if ((long)Cache.Instance.ActiveShip.StructurePercentage == 0) //if your hull is 0 you are dead or bugged, wait.
                        return;

                    if (Settings.Instance.WatchForActiveWars && Cache.Instance.IsCorpInWar)
                    {
                        Logging.Log("Cache", "Your corp is involved in a war [" + Cache.Instance.IsCorpInWar + "] and WatchForActiveWars [" + Settings.Instance.WatchForActiveWars + "], Starting panic!", Logging.Orange);
                        _States.CurrentPanicState = PanicState.StartPanicking;
                        //return;
                    }

                    if (Cache.Instance.InSpace)
                    {
                        if (!Cache.Instance.InMission && Cache.Instance.ActiveShip.GroupId == (int)Group.Capsule)
                        {
                            Logging.Log("Panic", "You are in a Capsule, you must have died :(", Logging.Red);
                            _States.CurrentPanicState = PanicState.BookmarkMyWreck;
                            //_States.CurrentPanicState = PanicState.StartPanicking;
                            return;
                        }

                        if (Cache.Instance.TargetedBy.Any())
                        {
                            if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been locked by [" + Cache.Instance.TargetedBy.Count() + "] Entities", Logging.Debug);
                            List<EntityCache> EntitiesThatAreWarpScramblingMe = Cache.Instance.TargetedBy.Where(t => t.IsWarpScramblingMe).ToList();
                            if (EntitiesThatAreWarpScramblingMe.Any())
                            {
                                if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been warp scrambled by [" + EntitiesThatAreWarpScramblingMe.Count() + "] Entities", Logging.Debug);
                                if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreWarpScramblingMe, DronePriority.WarpScrambler, "Panic", Settings.Instance.AddWarpScramblersToDronePriorityTargetList);
                                Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreWarpScramblingMe, PrimaryWeaponPriority.WarpScrambler, "Panic", Settings.Instance.AddWarpScramblersToDronePriorityTargetList);
                            }

                            if (Settings.Instance.SpeedTank)
                            {
                                List<EntityCache> EntitiesThatAreWebbingMe = Cache.Instance.TargetedBy.Where(t => t.IsWebbingMe).ToList();
                                if (EntitiesThatAreWebbingMe.Any())
                                {
                                    if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been webbed by [" + EntitiesThatAreWebbingMe.Count() + "] Entities", Logging.Debug);
                                    if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreWebbingMe, DronePriority.Webbing, "Panic", Settings.Instance.AddWebifiersToDronePriorityTargetList);
                                    Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreWebbingMe, PrimaryWeaponPriority.Webbing, "Panic", Settings.Instance.AddWebifiersToPrimaryWeaponsPriorityTargetList);
                                }

                                List<EntityCache> EntitiesThatAreTargetPaintingMe = Cache.Instance.TargetedBy.Where(t => t.IsTargetPaintingMe).ToList();
                                if (EntitiesThatAreTargetPaintingMe.Any())
                                {
                                    if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been target painted by [" + EntitiesThatAreTargetPaintingMe.Count() + "] Entities", Logging.Debug);
                                    if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreTargetPaintingMe, DronePriority.PriorityKillTarget, "Panic", Settings.Instance.AddTargetPaintersToDronePriorityTargetList);
                                    Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreTargetPaintingMe, PrimaryWeaponPriority.TargetPainting, "Panic", Settings.Instance.AddTargetPaintersToPrimaryWeaponsPriorityTargetList);
                                }
                            }

                            List<EntityCache> EntitiesThatAreNeutralizingMe = Cache.Instance.TargetedBy.Where(t => t.IsNeutralizingMe).ToList();
                            if (EntitiesThatAreNeutralizingMe.Any())
                            {
                                if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been neuted by [" + EntitiesThatAreNeutralizingMe.Count() + "] Entities", Logging.Debug);
                                if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreNeutralizingMe, DronePriority.PriorityKillTarget, "Panic", Settings.Instance.AddNeutralizersToDronePriorityTargetList);
                                Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreNeutralizingMe, PrimaryWeaponPriority.Neutralizing, "Panic", Settings.Instance.AddNeutralizersToPrimaryWeaponsPriorityTargetList);
                            }

                            List<EntityCache> EntitiesThatAreJammingMe = Cache.Instance.TargetedBy.Where(t => t.IsJammingMe).ToList();
                            if (EntitiesThatAreJammingMe.Any())
                            {
                                if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been ECMd by [" + EntitiesThatAreJammingMe.Count() + "] Entities", Logging.Debug);
                                if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreJammingMe, DronePriority.PriorityKillTarget, "Panic", Settings.Instance.AddECMsToDroneTargetList);
                                Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreJammingMe, PrimaryWeaponPriority.Jamming, "Panic", Settings.Instance.AddECMsToPrimaryWeaponsPriorityTargetList);
                            }

                            List<EntityCache> EntitiesThatAreSensorDampeningMe = Cache.Instance.TargetedBy.Where(t => t.IsSensorDampeningMe).ToList();
                            if (EntitiesThatAreSensorDampeningMe.Any())
                            {
                                if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been Sensor Damped by [" + EntitiesThatAreSensorDampeningMe.Count() + "] Entities", Logging.Debug);
                                if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreSensorDampeningMe, DronePriority.PriorityKillTarget, "Panic", Settings.Instance.AddDampenersToDronePriorityTargetList);
                                Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreSensorDampeningMe, PrimaryWeaponPriority.Dampening, "Panic", Settings.Instance.AddDampenersToPrimaryWeaponsPriorityTargetList);
                            }

                            if (Cache.Instance.Modules.Any(m => m.IsTurret))
                            {
                                //
                                // tracking disrupting targets
                                //
                                List<EntityCache> EntitiesThatAreTrackingDisruptingMe = Cache.Instance.TargetedBy.Where(t => t.IsTrackingDisruptingMe).ToList();
                                if (EntitiesThatAreTrackingDisruptingMe.Any())
                                {
                                    if (Settings.Instance.DebugPanic) Logging.Log("Panic.Normal", "We have been Tracking Disrupted by [" + EntitiesThatAreTrackingDisruptingMe.Count() + "] Entities", Logging.Debug);
                                    if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(EntitiesThatAreTrackingDisruptingMe, DronePriority.PriorityKillTarget, "Panic", Settings.Instance.AddTrackingDisruptorsToDronePriorityTargetList);
                                    Cache.Instance.AddPrimaryWeaponPriorityTargets(EntitiesThatAreTrackingDisruptingMe, PrimaryWeaponPriority.Dampening, "Panic", Settings.Instance.AddTrackingDisruptorsToPrimaryWeaponsPriorityTargetList);
                                }
                            }
                        }

                        if (Math.Round(DateTime.UtcNow.Subtract(_lastPriorityTargetLogging).TotalSeconds) > Settings.Instance.ListPriorityTargetsEveryXSeconds)
                        {
                            _lastPriorityTargetLogging = DateTime.UtcNow;

                            icount = 1;
                            foreach (EntityCache target in Cache.Instance.DronePriorityEntities)
                            {
                                icount++;
                                Logging.Log("Panic.ListDronePriorityTargets", "[" + icount + "][" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away] WARP[" + target.IsWarpScramblingMe + "] ECM[" + target.IsJammingMe + "] Damp[" + target.IsSensorDampeningMe + "] TP[" + target.IsTargetPaintingMe + "] NEUT[" + target.IsNeutralizingMe + "]", Logging.Teal);
                                continue;
                            }

                            icount = 1;
                            foreach (EntityCache target in Cache.Instance.PrimaryWeaponPriorityEntities)
                            {
                                icount++;
                                Logging.Log("Panic.ListPrimaryWeaponPriorityTargets", "[" + icount + "][" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away] WARP[" + target.IsWarpScramblingMe + "] ECM[" + target.IsJammingMe + "] Damp[" + target.IsSensorDampeningMe + "] TP[" + target.IsTargetPaintingMe + "] NEUT[" + target.IsNeutralizingMe + "]", Logging.Teal);
                                continue;
                            }
                        }

                        if (Cache.Instance.ActiveShip.ArmorPercentage < 100)
                        {
                            Cache.Instance.NeedRepair = true;
                            //
                            // do not return here, we are just setting a flag for use by arm to repair or not repair...
                            //
                        }
                        else 
                        {
                            Cache.Instance.NeedRepair = false;
                        }

                        if (Cache.Instance.InMission && Cache.Instance.ActiveShip.CapacitorPercentage < Settings.Instance.MinimumCapacitorPct && Cache.Instance.ActiveShip.GroupId != 31)
                        {
                            // Only check for cap-panic while in a mission, not while doing anything else
                            Logging.Log("Panic", "Start panicking, capacitor [" + Math.Round(Cache.Instance.ActiveShip.CapacitorPercentage, 0) + "%] below [" + Settings.Instance.MinimumCapacitorPct + "%] S[" + Math.Round(Cache.Instance.ActiveShip.ShieldPercentage, 0) + "%] A[" + Math.Round(Cache.Instance.ActiveShip.ArmorPercentage, 0) + "%] C[" + Math.Round(Cache.Instance.ActiveShip.CapacitorPercentage, 0) + "%]", Logging.Red);

                            //Questor.panic_attempts_this_mission;
                            Cache.Instance.PanicAttemptsThisMission++;
                            Cache.Instance.PanicAttemptsThisPocket++;
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            return;
                        }

                        if (Cache.Instance.ActiveShip.ShieldPercentage < Settings.Instance.MinimumShieldPct)
                        {
                            Logging.Log("Panic", "Start panicking, shield [" + Math.Round(Cache.Instance.ActiveShip.ShieldPercentage, 0) + "%] below [" + Settings.Instance.MinimumShieldPct + "%] S[" + Math.Round(Cache.Instance.ActiveShip.ShieldPercentage, 0) + "%] A[" + Math.Round(Cache.Instance.ActiveShip.ArmorPercentage, 0) + "%] C[" + Math.Round(Cache.Instance.ActiveShip.CapacitorPercentage, 0) + "%]", Logging.Red);
                            Cache.Instance.PanicAttemptsThisMission++;
                            Cache.Instance.PanicAttemptsThisPocket++;
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            return;
                        }

                        if (Cache.Instance.ActiveShip.ArmorPercentage < Settings.Instance.MinimumArmorPct)
                        {
                            Logging.Log("Panic", "Start panicking, armor [" + Math.Round(Cache.Instance.ActiveShip.ArmorPercentage, 0) + "%] below [" + Settings.Instance.MinimumArmorPct + "%] S[" + Math.Round(Cache.Instance.ActiveShip.ShieldPercentage, 0) + "%] A[" + Math.Round(Cache.Instance.ActiveShip.ArmorPercentage, 0) + "%] C[" + Math.Round(Cache.Instance.ActiveShip.CapacitorPercentage, 0) + "%]", Logging.Red);
                            Cache.Instance.PanicAttemptsThisMission++;
                            Cache.Instance.PanicAttemptsThisPocket++;
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            return;
                        }

                        BookmarkMyWreckAttempts = 1; // reset to 1 when we are known to not be in a pod anymore

                        _delayedResume = false;
                        if (Cache.Instance.InMission)
                        {
                            if (Cache.Instance.ActiveShip.GroupId == (int)Group.Capsule)
                            {
                                Logging.Log("Panic", "You are in a Capsule, you must have died in a mission :(", Logging.Red);
                                _States.CurrentPanicState = PanicState.BookmarkMyWreck;
                            }

                            int frigates = Cache.Instance.EntitiesNotSelf.Count(e => e.IsFrigate && e.IsPlayer);
                            int cruisers = Cache.Instance.EntitiesNotSelf.Count(e => e.IsCruiser && e.IsPlayer);
                            int battlecruisers = Cache.Instance.EntitiesNotSelf.Count(e => e.IsBattlecruiser && e.IsPlayer);
                            int battleships = Cache.Instance.EntitiesNotSelf.Count(e => e.IsBattleship && e.IsPlayer);
                            if (Settings.Instance.FrigateInvasionLimit > 0 && frigates >= Settings.Instance.FrigateInvasionLimit)
                            {
                                _delayedResume = true;

                                Cache.Instance.PanicAttemptsThisMission++;
                                Cache.Instance.PanicAttemptsThisPocket++;
                                _States.CurrentPanicState = PanicState.StartPanicking;
                                Logging.Log("Panic", "Start panicking, mission invaded by [" + frigates + "] Frigates", Logging.Red);
                            }

                            if (Settings.Instance.CruiserInvasionLimit > 0 && cruisers >= Settings.Instance.CruiserInvasionLimit)
                            {
                                _delayedResume = true;

                                Cache.Instance.PanicAttemptsThisMission++;
                                Cache.Instance.PanicAttemptsThisPocket++;
                                _States.CurrentPanicState = PanicState.StartPanicking;
                                Logging.Log("Panic", "Start panicking, mission invaded by [" + cruisers + "] Cruisers", Logging.Red);
                            }

                            if (Settings.Instance.BattlecruiserInvasionLimit > 0 && battlecruisers >= Settings.Instance.BattlecruiserInvasionLimit)
                            {
                                _delayedResume = true;

                                Cache.Instance.PanicAttemptsThisMission++;
                                Cache.Instance.PanicAttemptsThisPocket++;
                                _States.CurrentPanicState = PanicState.StartPanicking;
                                Logging.Log("Panic", "Start panicking, mission invaded by [" + battlecruisers + "] BattleCruisers", Logging.Red);
                            }

                            if (Settings.Instance.BattleshipInvasionLimit > 0 && battleships >= Settings.Instance.BattleshipInvasionLimit)
                            {
                                _delayedResume = true;

                                Cache.Instance.PanicAttemptsThisMission++;
                                Cache.Instance.PanicAttemptsThisPocket++;
                                _States.CurrentPanicState = PanicState.StartPanicking;
                                Logging.Log("Panic", "Start panicking, mission invaded by [" + battleships + "] BattleShips", Logging.Red);
                            }

                            if (_delayedResume)
                            {
                                _randomDelay = (Settings.Instance.InvasionRandomDelay > 0 ? _random.Next(Settings.Instance.InvasionRandomDelay) : 0);
                                _randomDelay += Settings.Instance.InvasionMinimumDelay;
                                foreach (EntityCache enemy in Cache.Instance.EntitiesNotSelf.Where(e => e.IsPlayer))
                                {
                                    Logging.Log("Panic", "Invaded by: PlayerName [" + enemy.Name + "] ShipTypeID [" + enemy.TypeId + "] Distance [" + Math.Round(enemy.Distance, 0) / 1000 + "k] Velocity [" + Math.Round(enemy.Velocity, 0) + "]", Logging.Red);
                                }
                            }
                        }
                    }
                   
                    break;

                // NOTE: The difference between Panicking and StartPanicking is that the bot will move to "Panic" state once in warp & Panicking
                //       and the bot wont go into Panic mode while still "StartPanicking"
                case PanicState.StartPanicking:
                case PanicState.Panicking:

                    //
                    // Add any warp scramblers to the priority list
                    // Use the same rules here as you do before you panic, as we probably want to keep killing DPS if configured to do so
                    //

                    EntityCache EntityIsWarpScramblingMeWhilePanicing = Cache.Instance.TargetedBy.FirstOrDefault(t => t.IsWarpScramblingMe);
                    if (EntityIsWarpScramblingMeWhilePanicing != null)
                    {
                        if (Cache.Instance.UseDrones) Cache.Instance.AddDronePriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsWarpScramblingMe), DronePriority.WarpScrambler, "Panic", Settings.Instance.AddWarpScramblersToDronePriorityTargetList);
                        Cache.Instance.AddPrimaryWeaponPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsWarpScramblingMe), PrimaryWeaponPriority.WarpScrambler, "Panic", Settings.Instance.AddWarpScramblersToPrimaryWeaponsPriorityTargetList);
                    }
                    
                    // Failsafe, in theory would/should never happen
                    if (_States.CurrentPanicState == PanicState.Panicking && Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                    {
                        // Resume is the only state that will make Questor revert to combat mode
                        _States.CurrentPanicState = PanicState.Resume;
                        return;
                    }

                    if (Cache.Instance.InStation)
                    {
                        Logging.Log("Panic", "Entered a station, lower panic mode", Logging.White);
                        Settings.Instance.LoadSettings();
                        _States.CurrentPanicState = PanicState.Panic;
                        return;
                    }

                    // Once we have warped off 500km, assume we are "safer"
                    if (_States.CurrentPanicState == PanicState.StartPanicking && Cache.Instance.DistanceFromMe(_lastNormalX, _lastNormalY, _lastNormalZ) > (int)Distances.PanicDistanceToConsiderSafelyWarpedOff)
                    {
                        Logging.Log("Panic", "We have warped off:  My ShipType: [" + Logging.Yellow + Cache.Instance.ActiveShip.TypeName + Logging.White + "] My ShipName [" + Logging.Yellow + Cache.Instance.ActiveShip.GivenName + Logging.White + "]", Logging.White);
                        _States.CurrentPanicState = PanicState.Panicking;
                    }

                    // We leave the panicking state once we actually start warping off

                    EntityCache station = null;
                    if (Cache.Instance.Stations != null && Cache.Instance.Stations.Any())
                    {
                        station = Cache.Instance.Stations.FirstOrDefault();    
                    }
                    
                    if (station != null && Cache.Instance.InSpace)
                    {
                        if (Cache.Instance.InWarp)
                        {
                            if (Cache.Instance.PrimaryWeaponPriorityEntities != null && Cache.Instance.PrimaryWeaponPriorityEntities.Any())
                            {
                                Cache.Instance.RemovePrimaryWeaponPriorityTargets(Cache.Instance.PrimaryWeaponPriorityEntities.ToList());
                            }

                            if (Cache.Instance.UseDrones && Cache.Instance.DronePriorityEntities != null && Cache.Instance.DronePriorityEntities.Any())
                            {
                                Cache.Instance.RemoveDronePriorityTargets(Cache.Instance.DronePriorityEntities.ToList());    
                            }
                            
                            break;
                        }

                        if (station.Distance > (int)Distances.WarptoDistance)
                        {
                            NavigateOnGrid.AvoidBumpingThings(Cache.Instance.BigObjectsandGates.FirstOrDefault(),"Panic");
                            if (Cache.Instance.DronePriorityEntities.Any(pt => pt.IsWarpScramblingMe) || Cache.Instance.PrimaryWeaponPriorityEntities.Any(pt => pt.IsWarpScramblingMe))
                            {
                                EntityCache WarpScrambledBy = Cache.Instance.DronePriorityEntities.FirstOrDefault(pt => pt.IsWarpScramblingMe) ?? Cache.Instance.PrimaryWeaponPriorityEntities.FirstOrDefault(pt => pt.IsWarpScramblingMe);
                                if (WarpScrambledBy != null && DateTime.UtcNow > _nextWarpScrambledWarning)
                                {
                                    _nextWarpScrambledWarning = DateTime.UtcNow.AddSeconds(20);
                                    Logging.Log("Panic", "We are scrambled by: [" + Logging.White + WarpScrambledBy.Name + Logging.Orange + "][" + Logging.White + Math.Round(WarpScrambledBy.Distance, 0) + Logging.Orange + "][" + Logging.White + WarpScrambledBy.Id + Logging.Orange + "]", Logging.Orange);
                                    _lastWarpScrambled = DateTime.UtcNow;
                                }
                            }

                            if (DateTime.UtcNow > Time.Instance.NextWarpAction || DateTime.UtcNow.Subtract(_lastWarpScrambled).TotalSeconds < Time.Instance.WarpScrambledNoDelay_seconds) //this will effectively spam warpto as soon as you are free of warp disruption if you were warp disrupted in the past 10 seconds)
                            {
                                if (station.WarpTo())
                                {
                                    Logging.Log("Panic", "Warping to [" + Logging.Yellow + station.Name + Logging.Red + "][" + Logging.Yellow + Math.Round((station.Distance / 1000) / 149598000, 2) + Logging.Red + " AU away]", Logging.Red);
                                    Cache.Instance.IsMissionPocketDone = true;    
                                }
                            }
                            else Logging.Log("Panic", "Warping will be attempted again after [" + Math.Round(Time.Instance.NextWarpAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "sec]", Logging.Red);
                                
                            //if (Cache.Instance.ActiveShip.GroupId == (int)Group.Capsule)
                            //{
                            //    Logging.Log("Panic", "You are in a Capsule, you must have died :(", Logging.Red);
                            //}
                            return;
                        }

                        if (station.Distance < (int)Distances.DockingRange)
                        {
                            if (station.Dock())
                            {
                                Logging.Log("Panic", "Docking with [" + Logging.Yellow + station.Name + Logging.Red + "][" + Logging.Yellow + Math.Round((station.Distance / 1000) / 149598000, 2) + Logging.Red + " AU away]", Logging.Red);    
                            }

                            return;
                        }

                        if (DateTime.UtcNow > Time.Instance.NextTravelerAction)
                        {
                            if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id || Cache.Instance.MyShipEntity.Velocity < 50)
                            {
                                if (station.Approach())
                                {
                                    Logging.Log("Panic", "Approaching to [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.Red);
                                    return;
                                }

                                return;
                            }
                                    
                            Logging.Log("Panic", "Already Approaching to: [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.Red);
                            return;
                        }

                        Logging.Log("Panic", "Approaching has been delayed for [" + Math.Round(Time.Instance.NextWarpAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "sec]", Logging.Red);
                        return;
                    }

                    if (Cache.Instance.InSpace)
                    {
                        if (DateTime.UtcNow.Subtract(Time.Instance.LastLoggingAction).TotalSeconds > 15)
                        {
                            Logging.Log("Panic", "No station found in local?", Logging.Red);
                        }

                        if (Cache.Instance.SafeSpotBookmarks.Any() && Cache.Instance.SafeSpotBookmarks.Any(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId))
                        {
                            List<DirectBookmark> SafeSpotBookmarksInLocal = new List<DirectBookmark>(Cache.Instance.SafeSpotBookmarks
                                                                            .Where(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId)
                                                                            .OrderBy(b => b.CreatedOn));

                            if (SafeSpotBookmarksInLocal.Any())
                            {
                                DirectBookmark offridSafeSpotBookmark = SafeSpotBookmarksInLocal.OrderBy(i => Cache.Instance.DistanceFromMe(i.X ?? 0, i.Y ?? 0, i.Z ?? 0)).FirstOrDefault();
                                if (offridSafeSpotBookmark != null)
                                {
                                    if (Cache.Instance.InWarp)
                                    {
                                        _States.CurrentPanicState = PanicState.Panic;
                                        return;
                                    }

                                    if (Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                                    {
                                        Logging.Log("Panic", "We are still warp scrambled!", Logging.Red);
                                        //This runs every 'tick' so we should see it every 1.5 seconds or so
                                        _lastWarpScrambled = DateTime.UtcNow;
                                        return;
                                    }

                                    if (DateTime.UtcNow > Time.Instance.NextWarpAction || DateTime.UtcNow.Subtract(_lastWarpScrambled).TotalSeconds < 10)
                                    //this will effectively spam warpto as soon as you are free of warp disruption if you were warp disrupted in the past 10 seconds
                                    {
                                        if (offridSafeSpotBookmark.WarpTo())
                                        {
                                            double DistanceToBm = Cache.Instance.DistanceFromMe(offridSafeSpotBookmark.X ?? 0,
                                                                                                offridSafeSpotBookmark.Y ?? 0,
                                                                                                offridSafeSpotBookmark.Z ?? 0);
                                            Logging.Log("Panic", "Warping to safespot bookmark [" + offridSafeSpotBookmark.Title + "][" + Math.Round((DistanceToBm / 1000) / 149598000, 2) + " AU away]", Logging.Red);
                                            return;
                                        }
                                        
                                        return;
                                    }

                                    Logging.Log("Panic", "Warping has been delayed for [" + Math.Round(Time.Instance.NextWarpAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "sec]", Logging.Red);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // What is this you say?  No star?
                            if (Cache.Instance.Star == null) return;

                            if (Cache.Instance.Star.Distance > (int)Distances.WeCanWarpToStarFromHere)
                            {
                                if (Cache.Instance.InWarp) return;

                                if (Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                                {
                                    Logging.Log("Panic", "We are still warp scrambled!", Logging.Red);
                                    //This runs every 'tick' so we should see it every 1.5 seconds or so
                                    _lastWarpScrambled = DateTime.UtcNow;
                                    return;
                                }

                                //this will effectively spam warpto as soon as you are free of warp disruption if you were warp disrupted in the past 10 seconds
                                if (DateTime.UtcNow > Time.Instance.NextWarpAction || DateTime.UtcNow.Subtract(_lastWarpScrambled).TotalSeconds < 10)
                                {
                                    if (Cache.Instance.Star.WarpTo())
                                    {
                                        Logging.Log("Panic", "Warping to [" + Logging.Yellow + Cache.Instance.Star.Name + Logging.Red + "][" + Logging.Yellow + Math.Round((Cache.Instance.Star.Distance / 1000) / 149598000, 2) + Logging.Red + " AU away]", Logging.Red);
                                        return;
                                    }
                                    
                                    return;
                                }

                                Logging.Log("Panic", "Warping has been delayed for [" + Math.Round(Time.Instance.NextWarpAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "sec]", Logging.Red);
                                return;
                            }
                        }
                    }

                    Logging.Log("Panic", "At a safe location, lower panic mode", Logging.Red);
                    Settings.Instance.LoadSettings();
                    _States.CurrentPanicState = PanicState.Panic;
                    break;

                case PanicState.BookmarkMyWreck:
                    BookmarkMyWreckAttempts++;
                    if(Cache.Instance.Wrecks.Any(i => i.Name.Contains(Settings.Instance.CombatShipName)))
                    {
                        Cache.Instance.CreateBookmark("Wreck: " + Settings.Instance.CombatShipName);
                        _States.CurrentPanicState = PanicState.StartPanicking;
                        break;
                    }

                    if (BookmarkMyWreckAttempts++ > 3)
                    {
                        _States.CurrentPanicState = PanicState.StartPanicking;
                        break;
                    }

                    break;

                case PanicState.Panic:

                    // Do not resume until you're no longer in a capsule
                    if (Cache.Instance.ActiveShip.GroupId == (int)Group.Capsule)
                        break;

                    if (Cache.Instance.InStation)
                    {
                        if (Cache.Instance.IsCorpInWar && Settings.Instance.WatchForActiveWars)
                        {
                            if (Settings.Instance.DebugWatchForActiveWars) Logging.Log("Panic", "Cache.Instance.IsCorpInWar [" + Cache.Instance.IsCorpInWar + "] and Settings.Instance.WatchForActiveWars [" + Settings.Instance.WatchForActiveWars + "] staying in panic (effectively paused in station)", Logging.Debug);
                            Cache.Instance.Paused = true;
                            Settings.Instance.AutoStart = false;
                            return;
                        }

                        if (Cache.Instance.DirectEve.HasSupportInstances() && Settings.Instance.UseStationRepair)
                        {
                            if (!Cache.Instance.RepairItems("Repair Function")) break; //attempt to use repair facilities if avail in station
                        }
                        Logging.Log("Panic", "We're in a station, resume mission", Logging.Red);
                        _States.CurrentPanicState = _delayedResume ? PanicState.DelayedResume : PanicState.Resume;
                    }

                    bool isSafe = Cache.Instance.ActiveShip.CapacitorPercentage >= Settings.Instance.SafeCapacitorPct;
                    isSafe &= Cache.Instance.ActiveShip.ShieldPercentage >= Settings.Instance.SafeShieldPct;
                    isSafe &= Cache.Instance.ActiveShip.ArmorPercentage >= Settings.Instance.SafeArmorPct;
                    if (isSafe)
                    {
                        if (Cache.Instance.InSpace)
                        {
                            Cache.Instance.RepairAll = true;
                        }
                        Logging.Log("Panic", "We have recovered, resume mission", Logging.Red);
                        _States.CurrentPanicState = _delayedResume ? PanicState.DelayedResume : PanicState.Resume;
                    }

                    if (_States.CurrentPanicState == PanicState.DelayedResume)
                    {
                        Logging.Log("Panic", "Delaying resume for " + _randomDelay + " seconds", Logging.Red);
                        Cache.Instance.IsMissionPocketDone = false;
                        _resumeTime = DateTime.UtcNow.AddSeconds(_randomDelay);
                    }
                    break;

                case PanicState.DelayedResume:
                    if (DateTime.UtcNow > _resumeTime)
                        _States.CurrentPanicState = PanicState.Resume;
                    break;

                case PanicState.Resume:

                    // Don't do anything here
                    break;
            }
        }
    }
}