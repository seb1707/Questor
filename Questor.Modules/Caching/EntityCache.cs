// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Caching
{
    using System;
    using System.Linq;
    using DirectEve;
    //using System.Collections.Generic;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;

    public class EntityCache
    {
        //
        // ALL of thses (well most) need _equivalents so we can cache the results per frame
        // _GroupID
        //

        private readonly DirectEntity _directEntity;

        public EntityCache(DirectEntity entity)
        {
            _directEntity = entity;
        }

        public void InvalidateCache()
        {
            try
            {
                //
                // this list of Entitycache Attributes we want to clear every pulse. (where is this running that does that atm?)
                //
                // thre are some attributes that will never change and can be cached across frames - some of those are not cleared here
                //
                _GroupID = null;
                _CategoryId = null;
                _Id = null;
                _TypeId = null;
                _FollowId = null;
                _Name = null;
                _Distance = null;
                _Nearest5kDistance = null;
                _ShieldPct = null;
                _ArmorPct = null;
                _StructurePct = null;
                _IsNpc = null;
                _Velocity = null;
                _IsTarget = null;
                _IsPrimaryWeaponKillPriority = null;
                _IsDroneKillPriority = null;
                _IsTooCloseTooFastTooSmallToHit = null;
                _IsReadyToShoot = null;
                _IsReadyToTarget = null;
                _IsHigherPriorityPresent = null;
                _IsLowerPriorityPresent = null;
                _IsActiveTarget = null;
                _IsInOptimalRange = null;
                _isPreferredDroneTarget = null;
                _IsDronePriorityTarget = null;
                _IsPriorityWarpScrambler = null;
                _isPreferredPrimaryWeaponTarget = null;
                _IsPrimaryWeaponPriorityTarget = null;
                _PrimaryWeaponPriorityLevel = null;
                _DronePriorityLevel = null;
                _IsTargeting = null;
                _IsTargetedBy = null;
                _IsAttacking = null;
                _IsWreckEmpty = null;
                _HasReleased = null;
                _HasExploded = null;
                _IsWarpScramblingMe = null;
                _IsWebbingMe = null;
                _IsNeutralizingMe = null;
                _IsJammingMe = null;
                _IsSensorDampeningMe = null;
                _IsTargetPaintingMe = null;
                _IsTrackingDisruptingMe = null;
                _Health = null;
                _IsSentry = null;
                _IsIgnored = null;
                _HaveLootRights = null;
                _TargetValue = null;
                _IsHighValueTarget = null;
                _IsLowValueTarget = null;
                _IsValid = null;
                _IsContainer = null;
                _IsPlayer = null;
                _IsTargetingMeAndNotYetTargeted = null;
                _IsNotYetTargetingMeAndNotYetTargeted = null;
                _IsTargetWeCanShootButHaveNotYetTargeted = null;
                _IsFrigate = null;
                _IsNPCFrigate = null;
                _IsCruiser = null;
                _IsNPCCruiser = null;
                _IsBattlecruiser = null;
                _IsNPCBattlecruiser = null;
                _IsBattleship = null;
                _IsNPCBattleship = null;
                _IsLargeCollidable = null;
                _IsMiscJunk = null;
                _IsBadIdea = null;
                _IsFactionWarfareNPC = null;
                _IsNpcByGroupID = null;
                _IsEntityIShouldLeaveAlone = null;
                _IsOnGridWithMe = null;
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache.InvalidateCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public int? _GroupID;

        public int GroupId
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_GroupID == null)
                    {
                        _GroupID = _directEntity.GroupId;    
                    }

                    return _GroupID ?? _directEntity.GroupId;
                }
                    
                return 0;
            }
        }

        public int? _CategoryId;

        public int CategoryId
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_CategoryId == null)
                    {
                        _CategoryId = _directEntity.CategoryId;
                    }

                    return _CategoryId ?? _directEntity.CategoryId;
                }

                return 0;
            }
        }

        public long? _Id;
        
        public long Id
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_Id == null)
                    {
                        _Id = _directEntity.Id;
                    }

                    return _Id ?? _directEntity.Id;
                }

                return 0;
            }
        }

        public int? _TypeId;

        public int TypeId
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_TypeId == null)
                    {
                        _TypeId = _directEntity.TypeId;
                    }

                    return _TypeId ?? _directEntity.TypeId;
                }

                return 0;
            }
        }

        public long? _FollowId;

        public long FollowId
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_FollowId == null)
                    {
                        _FollowId = _directEntity.FollowId;
                    }

                    return _FollowId ?? _directEntity.FollowId;
                }

                return 0;
            }
        }

        public string _Name;

        public string Name
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_Name == null)
                    {
                        _Name = _directEntity.Name;
                    }

                    return _Name ?? _directEntity.Name ?? string.Empty;
                }

                return string.Empty;
            }
        }

        public double? _Distance;

        public double Distance
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_Distance == null)
                    {
                        _Distance = _directEntity.Distance;
                    }

                    return _Distance ?? _directEntity.Distance;
                }

                return 0;
            }
        }

        public double? _Nearest5kDistance;

        public double Nearest5kDistance
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_Nearest5kDistance == null)
                    {
                        if (_directEntity.Distance > 0 && _directEntity.Distance < 900000000)
                        {
                            _Nearest5kDistance = Math.Round((_directEntity.Distance / 1000) * 2, MidpointRounding.AwayFromZero) / 2;
                        }
                    }

                    return _Nearest5kDistance ?? _directEntity.Distance;
                }

                return 0;
            }
        }

        public double? _ShieldPct;

        public double ShieldPct
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_ShieldPct == null)
                    {
                        _ShieldPct = _directEntity.ShieldPct;
                    }

                    return _ShieldPct ?? _directEntity.ShieldPct;
                }

                return 0;
            }
        }

        public double? _ArmorPct;

        public double ArmorPct
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_ArmorPct == null)
                    {
                        _ArmorPct = _directEntity.ArmorPct;
                    }

                    return _ArmorPct ?? _directEntity.ArmorPct;
                }

                return 0;
            }
        }

        public double? _StructurePct;

        public double StructurePct
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_StructurePct == null)
                    {
                        _StructurePct = _directEntity.StructurePct;
                    }

                    return _StructurePct ?? _directEntity.StructurePct;
                }

                return 0;
            }
        }

        public bool? _IsNpc;

        public bool IsNpc
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNpc == null)
                    {
                        _IsNpc = _directEntity.IsNpc;
                    }

                    return _IsNpc ?? _directEntity.IsNpc;
                }

                return false;
            }
        }

        public double? _Velocity;

        public double Velocity
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_Velocity == null)
                    {
                        _Velocity = _directEntity.Velocity;
                    }

                    return _Velocity ?? _directEntity.Velocity;
                }

                return 0;
            }
        }

        public bool? _IsTarget;
        
        public bool IsTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsTarget == null)
                    {
                        if (!_directEntity.HasExploded && Cache.Instance.Entities.Any(t => t.Id == _directEntity.Id))
                        {
                            _IsTarget = _directEntity.IsTarget;
                        }
                    }

                    return _IsTarget ?? _directEntity.IsTarget;
                }

                return false;
            }
        }

        public bool? _isPreferredPrimaryWeaponTarget;

        public bool isPreferredPrimaryWeaponTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_isPreferredPrimaryWeaponTarget == null)
                    {
                        if (Cache.Instance.PreferredPrimaryWeaponTarget.Id == _directEntity.Id)
                        {
                            _isPreferredPrimaryWeaponTarget = true;
                        }
                    }

                    return _isPreferredPrimaryWeaponTarget ?? false;
                }

                return false;
            }
        }

        public bool? _IsPrimaryWeaponKillPriority;
        
        public bool IsPrimaryWeaponKillPriority
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsPrimaryWeaponKillPriority == null)
                    {
                        if (Cache.Instance._primaryWeaponPriorityTargets.Any(e => e.Entity.Id == _directEntity.Id))
                        {
                            _IsPrimaryWeaponKillPriority = true;
                        }
                    }

                    return _IsPrimaryWeaponKillPriority ?? false;
                }

                return false;
            }
        }

        public bool? _isPreferredDroneTarget;

        public bool isPreferredDroneTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_isPreferredDroneTarget == null)
                    {
                        if (Cache.Instance.PreferredDroneTarget.Id == _directEntity.Id)
                        {
                            _isPreferredDroneTarget = true;
                        }
                    }

                    return _isPreferredDroneTarget ?? false;
                }

                return false;
            }
        }

        public bool? _IsDroneKillPriority;

        public bool IsDroneKillPriority
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsDroneKillPriority == null)
                    {
                        if (Cache.Instance._dronePriorityTargets.Any(e => e.Entity.Id == _directEntity.Id))
                        {
                            _IsDroneKillPriority = true;
                        }
                    }

                    return _IsDroneKillPriority ?? false;
                }

                return false;
            }
        }

        public bool? _IsTooCloseTooFastTooSmallToHit;

        public bool IsTooCloseTooFastTooSmallToHit
        {
            get
            {
                if (_directEntity != null)
                {
                    

                    return false;
                }


                if (_directEntity != null)
                {
                    if (_IsTooCloseTooFastTooSmallToHit == null)
                    {
                        if (IsNPCFrigate || IsFrigate)
                        {
                            if (Cache.Instance.DoWeCurrentlyHaveTurretsMounted() && Cache.Instance.UseDrones)
                            {
                                if (_directEntity.Distance < Settings.Instance.DistanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons
                                 && _directEntity.Velocity > Settings.Instance.SpeedNPCFrigatesShouldBeIgnoredByPrimaryWeapons)
                                {
                                    _IsTooCloseTooFastTooSmallToHit = true;
                                    return _IsTooCloseTooFastTooSmallToHit ?? true;
                                }

                                _IsTooCloseTooFastTooSmallToHit = false;
                                return _IsTooCloseTooFastTooSmallToHit ?? false;
                            }

                            _IsTooCloseTooFastTooSmallToHit = false;
                            return _IsTooCloseTooFastTooSmallToHit ?? false;
                        }
                    }

                    return _IsTooCloseTooFastTooSmallToHit ?? false;
                }

                return false;
            }
        }

        public bool? _IsReadyToShoot;

        public bool IsReadyToShoot
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsReadyToShoot == null)
                    {
                        if (!HasExploded && IsTarget && !IsIgnored)
                        {
                            if (_directEntity.Distance < Cache.Instance.MaxRange)
                            {
                                if (Cache.Instance.Entities.Any(t => t.Id == _directEntity.Id))
                                {
                                    _IsReadyToShoot = true;
                                    return _IsReadyToShoot ?? true;
                                }

                                _IsReadyToShoot = false;
                                return _IsReadyToShoot ?? false;
                            }

                            _IsReadyToShoot = false;
                            return _IsReadyToShoot ?? false;
                        }
                    }

                    return _IsReadyToShoot ?? false;
                }

                return false;
            }
        }

        public bool? _IsReadyToTarget;
        
        public bool IsReadyToTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsReadyToTarget == null)
                    {
                        if (!HasExploded && !IsTarget && !IsTargeting)
                        {
                            _IsReadyToTarget = true;
                            return _IsReadyToTarget ?? true;
                        }
                    }

                    return _IsReadyToTarget ?? false;
                }

                return false;
            }
        }

        public bool? _IsHigherPriorityPresent;

        public bool IsHigherPriorityPresent
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsHigherPriorityPresent == null)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any() || Cache.Instance.DronePriorityTargets.Any())
                        {

                            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any())
                            {
                                if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.Id == _directEntity.Id))
                                {
                                    PrimaryWeaponPriority _currentPrimaryWeaponPriority = Cache.Instance.PrimaryWeaponPriorityTargets.Where(t => t.Id == _directEntity.Id).Select(pt => pt.PrimaryWeaponPriorityLevel).FirstOrDefault();

                                    if (!Cache.Instance.PrimaryWeaponPriorityTargets.All(pt => pt.PrimaryWeaponPriorityLevel < _currentPrimaryWeaponPriority && pt.Distance < Cache.Instance.MaxRange))
                                    {
                                        _IsHigherPriorityPresent = true;
                                        return _IsHigherPriorityPresent ?? true;
                                    }

                                    _IsHigherPriorityPresent = false;
                                    return _IsHigherPriorityPresent ?? false;
                                }

                                if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(e => e.Distance < Cache.Instance.MaxRange))
                                {
                                    _IsHigherPriorityPresent = true;
                                    return _IsHigherPriorityPresent ?? true;
                                }

                                _IsHigherPriorityPresent = false;
                                return _IsHigherPriorityPresent ?? false;
                            }

                            if (Cache.Instance.DronePriorityTargets.Any())
                            {
                                if (Cache.Instance.DronePriorityTargets.Any(pt => pt.Id == _directEntity.Id))
                                {
                                    DronePriority _currentEntityDronePriority = Cache.Instance.DronePriorityTargets.Where(t => t.Id == _directEntity.Id).Select(pt => pt.DronePriorityLevel).FirstOrDefault();

                                    if (!Cache.Instance.DronePriorityTargets.All(pt => pt.DronePriorityLevel < _currentEntityDronePriority && pt.Distance < Settings.Instance.DroneControlRange))
                                    {
                                        _IsHigherPriorityPresent = true;
                                        return _IsHigherPriorityPresent ?? true;
                                    }

                                    _IsHigherPriorityPresent = false;
                                    return _IsHigherPriorityPresent ?? false;
                                }

                                if (Cache.Instance.DronePriorityTargets.Any(e => e.Distance < Settings.Instance.DroneControlRange))
                                {
                                    _IsHigherPriorityPresent = true;
                                    return _IsHigherPriorityPresent ?? true;
                                }

                                _IsHigherPriorityPresent = false;
                                return _IsHigherPriorityPresent ?? false;
                            }

                            _IsHigherPriorityPresent = false;
                            return _IsHigherPriorityPresent ?? false;
                        }
                    }

                    return _IsHigherPriorityPresent ?? false;
                }

                return false;
            }
        }

        public bool? _IsLowerPriorityPresent;

        public bool IsLowerPriorityPresent
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsLowerPriorityPresent == null)
                    {
                        if (Cache.Instance._primaryWeaponPriorityTargets.Any() || Cache.Instance._dronePriorityTargets.Any())
                        {
                            if (Cache.Instance._primaryWeaponPriorityTargets.Any())
                            {
                                if (Cache.Instance._primaryWeaponPriorityTargets.Any(pt => pt.EntityID == _directEntity.Id))
                                {
                                    PrimaryWeaponPriority _currentPrimaryWeaponPriority = Cache.Instance._primaryWeaponPriorityTargets.Where(t => t.EntityID == _directEntity.Id).Select(pt => pt.PrimaryWeaponPriority).FirstOrDefault();

                                    if (!Cache.Instance._primaryWeaponPriorityTargets.All(pt => pt.PrimaryWeaponPriority > _currentPrimaryWeaponPriority && pt.Entity.Distance < Cache.Instance.MaxRange))
                                    {
                                        _IsLowerPriorityPresent = true;
                                        return _IsLowerPriorityPresent ?? true;
                                    }

                                    _IsLowerPriorityPresent = false;
                                    return _IsLowerPriorityPresent ?? false;
                                }

                                if (Cache.Instance._primaryWeaponPriorityTargets.Any(e => e.Entity.Distance < Cache.Instance.MaxRange))
                                {
                                    _IsLowerPriorityPresent = true;
                                    return _IsLowerPriorityPresent ?? true;
                                }

                                _IsLowerPriorityPresent = false;
                                return _IsLowerPriorityPresent ?? false;
                            }

                            if (Cache.Instance._dronePriorityTargets.Any())
                            {
                                if (Cache.Instance._dronePriorityTargets.Any(pt => pt.EntityID == _directEntity.Id))
                                {
                                    DronePriority _currentEntityDronePriority = Cache.Instance._dronePriorityTargets.Where(t => t.EntityID == _directEntity.Id).Select(pt => pt.DronePriority).FirstOrDefault();

                                    if (!Cache.Instance._dronePriorityTargets.All(pt => pt.DronePriority > _currentEntityDronePriority && pt.Entity.Distance < Settings.Instance.DroneControlRange))
                                    {
                                        _IsLowerPriorityPresent = true;
                                        return _IsLowerPriorityPresent ?? true;
                                    }

                                    _IsLowerPriorityPresent = false;
                                    return _IsLowerPriorityPresent ?? false;
                                }

                                if (Cache.Instance._dronePriorityTargets.Any(e => e.Entity.Distance < Settings.Instance.DroneControlRange))
                                {
                                    _IsLowerPriorityPresent = true;
                                    return _IsLowerPriorityPresent ?? true;
                                }

                                _IsLowerPriorityPresent = false;
                                return _IsLowerPriorityPresent ?? false;
                            }

                            _IsLowerPriorityPresent = false;
                            return _IsLowerPriorityPresent ?? false;
                        }
                    }

                    return _IsLowerPriorityPresent ?? false;
                }

                return false;
            }
        }

        public bool? _IsActiveTarget;

        public bool IsActiveTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if(_IsActiveTarget == null)
                    {
                        if (IsTarget)
                        {
                            _IsActiveTarget = _directEntity.IsActiveTarget;
                            return _IsActiveTarget ?? false;
                        }
                    }

                    return _IsActiveTarget ?? false;
                }

                return false;
            }
        }

        public bool? _IsInOptimalRange;

        public bool IsInOptimalRange
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsInOptimalRange == null)
                    {
                        if (Settings.Instance.SpeedTank && Settings.Instance.OrbitDistance != 0)
                        {
                            if (Settings.Instance.OptimalRange == 0)
                            {
                                Cache.Instance.OptimalRange = Settings.Instance.OrbitDistance + 5000;
                            }
                        }

                        if (Cache.Instance.OptimalRange != 0 || Settings.Instance.OptimalRange != 0)
                        {
                            double optimal = 0;

                            if (Cache.Instance.OptimalRange != 0)
                            {
                                optimal = Cache.Instance.OptimalRange;
                            }
                            else if (Settings.Instance.OptimalRange != 0) //do we really need this condition? we cant even get in here if one of them isnt != 0, that is the idea, if its 0 we sure as hell dont want to use it as the optimal
                            {
                                optimal = Settings.Instance.OptimalRange;
                            }

                            if (Cache.Instance.DoWeCurrentlyHaveTurretsMounted()) //Lasers, Projectile, and Hybrids
                            {
                                if (Distance > Settings.Instance.InsideThisRangeIsHardToTrack)
                                {
                                    if (Distance < (optimal * 1.5))
                                    {
                                        _IsInOptimalRange = true;
                                        return _IsInOptimalRange ?? true;
                                    }
                                }
                            }
                            else //missile boats - use max range
                            {
                                optimal = Cache.Instance.MaxRange;
                                if (Distance < optimal)
                                {
                                    _IsInOptimalRange = true;
                                    return _IsInOptimalRange ?? true;
                                }
                            }

                            _IsInOptimalRange = false;
                            return _IsInOptimalRange ?? false;
                        }

                        // If you have no optimal you have to assume the entity is within Optimal... (like missiles)
                        _IsInOptimalRange = true;
                        return _IsInOptimalRange ?? true;
                    }

                    return _IsInOptimalRange ?? true;
                }

                return false;
            }
        }

        public bool? _IsDronePriorityTarget;

        public bool IsDronePriorityTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsDronePriorityTarget == null)
                    {
                        if (Cache.Instance.DronePriorityTargets.All(i => i.Id != _directEntity.Id))
                        {
                            _IsDronePriorityTarget = false;
                            return _IsDronePriorityTarget ?? false;
                        }

                        _IsDronePriorityTarget = true;
                        return _IsDronePriorityTarget ?? true;
                    }

                    return _IsDronePriorityTarget ?? false;
                }

                return false;
            }
        }

        public bool? _IsPriorityWarpScrambler;

        public bool IsPriorityWarpScrambler
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsPriorityWarpScrambler == null)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.Id == Id))
                        {
                            EntityCache __entity = new EntityCache(_directEntity);
                            if (__entity.PrimaryWeaponPriorityLevel == PrimaryWeaponPriority.WarpScrambler)
                            {
                                _IsPriorityWarpScrambler = true;
                                return _IsPriorityWarpScrambler ?? true;
                            }
                        }

                        if (Cache.Instance.DronePriorityTargets.Any(pt => pt.Id == Id))
                        {
                            EntityCache __entity = new EntityCache(_directEntity);
                            if (__entity.DronePriorityLevel == DronePriority.WarpScrambler)
                            {
                                _IsPriorityWarpScrambler = true;
                                return _IsPriorityWarpScrambler ?? true;
                            }
                        }

                        _IsPriorityWarpScrambler = false;
                        return _IsPriorityWarpScrambler ?? false;
                    }

                    return _IsPriorityWarpScrambler ?? false;
                }

                return false;
            }
        }

        public bool? _IsPrimaryWeaponPriorityTarget;

        public bool IsPrimaryWeaponPriorityTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsPrimaryWeaponPriorityTarget == null)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.All(i => i.Id != _directEntity.Id))
                        {
                            _IsPrimaryWeaponPriorityTarget = false;
                            return _IsPrimaryWeaponPriorityTarget ?? false;
                        }

                        _IsPrimaryWeaponPriorityTarget = true;
                        return _IsPrimaryWeaponPriorityTarget ?? true;
                    }

                    return _IsPrimaryWeaponPriorityTarget ?? false;
                }

                return false;
            }
        }

        public PrimaryWeaponPriority? _PrimaryWeaponPriorityLevel;

        public PrimaryWeaponPriority PrimaryWeaponPriorityLevel
        {
            get
            {
                if (_directEntity != null)
                {
                    if(_PrimaryWeaponPriorityLevel == null)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.Id == Id))
                        {
                            PrimaryWeaponPriority currentTargetPriority = Cache.Instance._primaryWeaponPriorityTargets.Where(t => t.Entity.IsTarget
                                                                                                                               && t.EntityID == Id)
                                                                                                                      .Select(pt => pt.PrimaryWeaponPriority)
                                                                                                                      .FirstOrDefault();
                            _PrimaryWeaponPriorityLevel = currentTargetPriority;
                            return _PrimaryWeaponPriorityLevel ?? PrimaryWeaponPriority.NotUsed;
                        }

                        _PrimaryWeaponPriorityLevel = PrimaryWeaponPriority.NotUsed;
                        return _PrimaryWeaponPriorityLevel ?? PrimaryWeaponPriority.NotUsed;
                    }
                    
                }

                _PrimaryWeaponPriorityLevel = PrimaryWeaponPriority.NotUsed;
                return _PrimaryWeaponPriorityLevel ?? PrimaryWeaponPriority.NotUsed;
            }
        }

        public DronePriority? _DronePriorityLevel;
        
        public DronePriority DronePriorityLevel
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_DronePriorityLevel ==null)
                    {
                        if (Cache.Instance._dronePriorityTargets.Any(pt => pt.EntityID == _directEntity.Id))
                        {
                            DronePriority currentTargetPriority = Cache.Instance._dronePriorityTargets.Where(t => t.Entity.IsTarget
                                                                                                                      && t.EntityID == Id)
                                                                                                                      .Select(pt => pt.DronePriority)
                                                                                                                      .FirstOrDefault();

                            _DronePriorityLevel = currentTargetPriority;
                            return _DronePriorityLevel ?? DronePriority.NotUsed;
                        }

                        _DronePriorityLevel = DronePriority.NotUsed;
                        return _DronePriorityLevel ?? DronePriority.NotUsed;
                    }
                }

                _DronePriorityLevel = DronePriority.NotUsed;
                return _DronePriorityLevel ?? DronePriority.NotUsed;
            }
        }

        public bool? _IsTargeting;

        public bool IsTargeting
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsTargeting == null)
                    {
                        _IsTargeting = _directEntity.IsTargeting;
                        return _IsTargeting ?? false;
                    }

                    return _IsTargeting ?? false;
                }

                return false;
            }
        }

        public bool? _IsTargetedBy;

        public bool IsTargetedBy
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsTargetedBy == null)
                    {
                        _IsTargetedBy = _directEntity.IsTargetedBy;
                        return _IsTargetedBy ?? false;
                    }

                    return _IsTargetedBy ?? false;
                }

                return false;
            }
        }

        public bool? _IsAttacking;

        public bool IsAttacking
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsAttacking == null)
                    {
                        _IsAttacking = _directEntity.IsAttacking;
                        return _IsAttacking ?? false;
                    }

                    return _IsAttacking ?? false;
                }

                return false;
            }
        }


        public bool? _IsWreckEmpty;
        
        public bool IsWreckEmpty
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsWreckEmpty == null)
                    {
                        _IsWreckEmpty = _directEntity.IsEmpty;
                        return _IsWreckEmpty ?? false;
                    }

                    return _IsWreckEmpty ?? false;
                }

                return false;
            }
        }

        public bool? _HasReleased;
        
        public bool HasReleased
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_HasReleased == null)
                    {
                        _HasReleased = _directEntity.HasReleased;
                        return _HasReleased ?? false;
                    }

                    return _HasReleased ?? false;
                }

                return false;
            }
        }

        public bool? _HasExploded;

        public bool HasExploded
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_HasExploded == null)
                    {
                        _HasExploded = _directEntity.HasExploded;
                        return _HasExploded ?? false;
                    }

                    return _HasExploded ?? false;
                }

                return false;
            }
        }

        public bool IsEwarTarget()
        {
            bool result = false;
            result |= IsWarpScramblingMe;
            result |= IsWebbingMe;
            result |= IsNeutralizingMe;
            result |= IsJammingMe;
            result |= IsSensorDampeningMe;
            result |= IsTargetPaintingMe;
            result |= IsTrackingDisruptingMe;
            return result;
        }

        public DronePriority IsActiveDroneEwarType()
        {
            if (IsWarpScramblingMe)
            {
                return DronePriority.WarpScrambler;
            }

            if (IsWebbingMe)
            {
                return DronePriority.Webbing;
            }

            if (IsNeutralizingMe)
            {
                return DronePriority.PriorityKillTarget;
            }

            if (IsJammingMe)
            {
                return DronePriority.PriorityKillTarget;
            }

            if (IsSensorDampeningMe)
            {
                return DronePriority.PriorityKillTarget;
            }

            if (IsTargetPaintingMe)
            {
                return DronePriority.PriorityKillTarget;
            }

            if (IsTrackingDisruptingMe)
            {
                return DronePriority.PriorityKillTarget;
            }
            
            return DronePriority.NotUsed;
        }

        public PrimaryWeaponPriority IsActivePrimaryWeaponEwarType()
        {
            if (IsWarpScramblingMe)
            {
                return PrimaryWeaponPriority.WarpScrambler;
            }

            if (IsWebbingMe)
            {
                return PrimaryWeaponPriority.Webbing;
            }

            if (IsNeutralizingMe)
            {
                return PrimaryWeaponPriority.Neutralizing;
            }

            if (IsJammingMe)
            {
                return PrimaryWeaponPriority.Jamming;
            }

            if (IsSensorDampeningMe)
            {
                return PrimaryWeaponPriority.Dampening;
            }

            if (IsTargetPaintingMe)
            {
                return PrimaryWeaponPriority.TargetPainting;
            }

            if (IsTrackingDisruptingMe)
            {
                return PrimaryWeaponPriority.TrackingDisrupting;
            }

            return PrimaryWeaponPriority.NotUsed;
        }

        public bool? _IsWarpScramblingMe;

        public bool IsWarpScramblingMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if(_IsWarpScramblingMe == null)
                    {
                        if (_directEntity.Attacks.Contains("effects.WarpScramble"))
                        {
                            if (!Cache.Instance.WarpScrambler.Contains(_directEntity.Id)) Cache.Instance.WarpScrambler.Add(_directEntity.Id);
                            _IsWarpScramblingMe = true;
                            return _IsWarpScramblingMe ?? true;
                        }
                        if (Cache.Instance.WarpScrambler.Contains(_directEntity.Id))
                        {
                            _IsWarpScramblingMe = true;
                            return _IsWarpScramblingMe ?? true;
                        }
                    }

                    return _IsWarpScramblingMe ?? false;
                }

                return false;
            }
        }

        public bool? _IsWebbingMe;
        
        public bool IsWebbingMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsWebbingMe == null)
                    {
                        if (_directEntity.Attacks.Contains("effects.ModifyTargetSpeed"))
                        {
                            if (!Cache.Instance.Webbing.Contains(Id)) Cache.Instance.Webbing.Add(Id);
                            _IsWebbingMe = true;
                            return _IsWebbingMe ?? true;
                        }

                        if (Cache.Instance.Webbing.Contains(Id))
                        {
                            _IsWebbingMe = true;
                            return _IsWebbingMe ?? true;
                        }
                    }

                    return _IsWebbingMe ?? false;
                }

                return false;
            }
        }

        public bool? _IsNeutralizingMe;

        public bool IsNeutralizingMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNeutralizingMe == null)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewEnergyNeut"))
                        {
                            if (!Cache.Instance.Neuting.Contains(Id)) Cache.Instance.Neuting.Add(Id);
                            _IsNeutralizingMe = true;
                            return _IsNeutralizingMe ?? true;
                        }

                        if (Cache.Instance.Neuting.Contains(Id))
                        {
                            _IsNeutralizingMe = true;
                            return _IsNeutralizingMe ?? true;
                        }
                    }

                    return _IsNeutralizingMe ?? false;
                }

                return false;
            }
        }

        public bool? _IsJammingMe;

        public bool IsJammingMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsJammingMe == null)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("electronic"))
                        {
                            if (!Cache.Instance.Jammer.Contains(Id)) Cache.Instance.Jammer.Add(Id);
                            _IsJammingMe = true;
                            return _IsJammingMe ?? true;
                        }

                        if (Cache.Instance.Jammer.Contains(Id))
                        {
                            _IsJammingMe = true;
                            return _IsJammingMe ?? true;
                        }
                    }

                    return _IsJammingMe ?? false;
                }

                return false;
            }
        }

        public bool? _IsSensorDampeningMe;

        public bool IsSensorDampeningMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsSensorDampeningMe == null)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewRemoteSensorDamp"))
                        {
                            if (!Cache.Instance.Dampening.Contains(Id)) Cache.Instance.Dampening.Add(Id);
                            _IsSensorDampeningMe = true;
                            return _IsSensorDampeningMe ?? true;
                        }

                        if (Cache.Instance.Dampening.Contains(Id))
                        {
                            _IsSensorDampeningMe = true;
                            return _IsSensorDampeningMe ?? true;
                        }    
                    }

                    return _IsSensorDampeningMe ?? false;
                }

                return false;
            }
        }

        public bool? _IsTargetPaintingMe;

        public bool IsTargetPaintingMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if(_IsTargetPaintingMe == null)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewTargetPaint"))
                        {
                            if (!Cache.Instance.TargetPainting.Contains(Id)) Cache.Instance.TargetPainting.Add(Id);
                            _IsTargetPaintingMe = true;
                            return _IsTargetPaintingMe ?? true;
                        }

                        if (Cache.Instance.TargetPainting.Contains(Id))
                        {
                            _IsTargetPaintingMe = true;
                            return _IsTargetPaintingMe ?? true;
                        }    
                    }

                    return _IsTargetPaintingMe ?? false;
                }

                return false;
            }
        }

        public bool? _IsTrackingDisruptingMe;

        public bool IsTrackingDisruptingMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsTrackingDisruptingMe == null)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewTrackingDisrupt"))
                        {
                            if (!Cache.Instance.TrackingDisrupter.Contains(Id)) Cache.Instance.TrackingDisrupter.Add(Id);
                            _IsTrackingDisruptingMe = true;
                            return _IsTrackingDisruptingMe ?? true;
                        }

                        if (Cache.Instance.TrackingDisrupter.Contains(Id))
                        {
                            _IsTrackingDisruptingMe = true;
                            return _IsTrackingDisruptingMe ?? true;
                        }
                    }

                    return _IsTrackingDisruptingMe ?? false;
                }

                return false;
            }
        }

        public int? _Health;

        public int Health
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_Health == null)
                    {
                        _Health = (int)((ShieldPct + ArmorPct + StructurePct) * 100);
                        return _Health ?? 0;
                    }

                    return _Health ?? 0;
                }

                return 0;
            }
        }

        public bool? _IsSentry;

        public bool IsSentry
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsSentry == null)
                    {
                        bool result = false;
                        //if (GroupId == (int)Group.SentryGun) return true;
                        result |= (GroupId == (int) Group.ProtectiveSentryGun);
                        result |= (GroupId == (int) Group.MobileSentryGun);
                        result |= (GroupId == (int) Group.DestructibleSentryGun);
                        result |= (GroupId == (int) Group.MobileMissileSentry);
                        result |= (GroupId == (int) Group.MobileProjectileSentry);
                        result |= (GroupId == (int) Group.MobileLaserSentry);
                        result |= (GroupId == (int) Group.MobileHybridSentry);
                        result |= (GroupId == (int) Group.DeadspaceOverseersSentry);
                        result |= (GroupId == (int) Group.StasisWebificationBattery);
                        result |= (GroupId == (int) Group.EnergyNeutralizingBattery);
                        _IsSentry = result;
                        return _IsSentry ?? false;
                    }

                    return _IsSentry ?? false;
                }

                return false;
            }
        }

        public bool? _IsIgnored;

        public bool IsIgnored
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsIgnored == null)
                    {
                        if (Cache.Instance.Entities.Any(t => t.Id != Id))
                        {
                            _IsIgnored = false;
                            return _IsIgnored ?? false;
                        }

                        bool result = false;
                        result |= Cache.Instance.IgnoreTargets.Contains(Name.Trim());
                        _IsIgnored = result;
                        return _IsIgnored ?? false;
                    }

                    return _IsIgnored ?? false;
                }

                return false;
            }
        }

        public bool? _HaveLootRights;

        public bool HaveLootRights
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_HaveLootRights == null)
                    {
                        if (GroupId == (int)Group.SpawnContainer)
                        {
                            _HaveLootRights = true;
                            return _HaveLootRights ?? true;
                        }
                        
                        bool result = false;
                        if (Cache.Instance.ActiveShip.Entity != null)
                        {
                            result |= _directEntity.CorpId == Cache.Instance.ActiveShip.Entity.CorpId;
                            result |= _directEntity.OwnerId == Cache.Instance.ActiveShip.Entity.CharId;
                            //
                            // It would be nice if this were eventually extended to detect and include 'abandoned' wrecks (blue ones). 
                            // I do not yet know what attributed actually change when that happens. We should collect some data. 
                            //
                        }

                        _HaveLootRights = result;
                        return _HaveLootRights ?? false;
                    }

                    return _HaveLootRights ?? false;
                }

                return false;
            }
        }

        public int? _TargetValue;

        public int? TargetValue
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_TargetValue == null)
                    {
                        ShipTargetValue value = null;

                        try
                        {
                            value = Cache.Instance.ShipTargetValues.FirstOrDefault(v => v.GroupId == GroupId);
                        }
                        catch (Exception exception)
                        {
                            if (Settings.Instance.DebugShipTargetValues) Logging.Log("TargetValue", "exception [" + exception + "]", Logging.Debug);
                        }

                        if (value == null)
                        {

                            if (IsNPCBattleship)
                            {
                                _TargetValue = 4;
                            }
                            if (IsNPCBattlecruiser)
                            {
                                _TargetValue = 3;
                            }
                            if (IsNPCCruiser)
                            {
                                _TargetValue = 2;
                            }
                            if (IsNPCFrigate)
                            {
                                _TargetValue = 0;
                            }
                            _TargetValue = 2;
                            return _TargetValue ?? 0;
                        }

                        _TargetValue = value.TargetValue;
                        return _TargetValue ?? 0;    
                    }

                    return _TargetValue ?? 0;
                }

                return 0;
            }
        }

        public bool? _IsHighValueTarget;

        public bool IsHighValueTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsHighValueTarget == null)
                    {
                        if (TargetValue != null)
                        {
                            if (TargetValue >= Settings.Instance.MinimumTargetValueToConsiderTargetAHighValueTarget)
                            {
                                _IsHighValueTarget = true;
                                return _IsHighValueTarget ?? true;
                            }

                            if (IsLargeCollidable)
                            {
                                _IsHighValueTarget = true;
                                return _IsHighValueTarget ?? true;
                            }
                        }
                    }

                    return _IsHighValueTarget ?? false;
                }

                return false;
            }
        }

        public bool? _IsLowValueTarget;

        public bool IsLowValueTarget
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsLowValueTarget == null)
                    {
                        if (TargetValue != null && TargetValue <= Settings.Instance.MaximumTargetValueToConsiderTargetALowValueTarget)
                        {
                            _IsLowValueTarget = true;
                            return _IsLowValueTarget ?? true;
                        }

                        _IsLowValueTarget = false;
                        return _IsLowValueTarget ?? false;
                    }

                    return _IsLowValueTarget ?? true;
                }

                return false;
            }
        }

        public DirectContainerWindow CargoWindow
        {
            get { return Cache.Instance.Windows.OfType<DirectContainerWindow>().FirstOrDefault(w => w.ItemId == Id); }
        }

        public bool? _IsValid;

        public bool IsValid
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsValid == null)
                    {
                        if (!HasExploded)
                        {
                            _IsValid = _directEntity.IsValid;
                            return _IsValid ?? true;
                        }

                        _IsValid = false;
                        return _IsValid ?? false;
                    }

                    return _IsValid ?? true;
                }

                return false;
            }
        }

        public bool? _IsContainer;

        public bool IsContainer
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsContainer == null)
                    {
                        bool result = false;
                        result |= (GroupId == (int)Group.Wreck);
                        result |= (GroupId == (int)Group.CargoContainer);
                        result |= (GroupId == (int)Group.SpawnContainer);
                        result |= (GroupId == (int)Group.MissionContainer);

                        _IsContainer = result;
                        return _IsContainer ?? false;
                    }

                    return _IsContainer ?? false;
                }

                return false;
                
            }
        }

        public bool? _IsPlayer;

        public bool IsPlayer
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsPlayer == null)
                    {
                        _IsPlayer = _directEntity.IsPc;
                        return _IsPlayer ?? false;
                    }

                    return _IsPlayer ?? false;
                }
                
                return false;
            }
        }

        public bool? _IsTargetingMeAndNotYetTargeted;

        public bool IsTargetingMeAndNotYetTargeted
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsTargetingMeAndNotYetTargeted == null)
                    {
                        bool result = false;
                        result |= (((IsNpc || IsNpcByGroupID) || IsAttacking)
                            //&& (!IsSentry || (IsSentry && Settings.Instance.KillSentries))
                                   && (!IsTargeting && !IsTarget && IsTargetedBy)
                                   && !IsContainer
                                   && CategoryId == (int)CategoryID.Entity
                                   && Distance < Cache.Instance.MaxTargetRange
                                   && !IsIgnored
                                   && (!IsBadIdea || IsAttacking)
                                   && !IsEntityIShouldLeaveAlone
                                   && !IsFactionWarfareNPC
                                   && !IsLargeCollidable
                                   && !IsStation);
                        _IsTargetingMeAndNotYetTargeted = result;
                        return _IsTargetingMeAndNotYetTargeted ?? false;
                    }

                    return _IsTargetingMeAndNotYetTargeted ?? false;
                }
                
                return false;
            }
        }

        public bool? _IsNotYetTargetingMeAndNotYetTargeted;

        public bool IsNotYetTargetingMeAndNotYetTargeted
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNotYetTargetingMeAndNotYetTargeted == null)
                    {
                        bool result = false;
                        result |= (((IsNpc || IsNpcByGroupID) || IsAttacking || Cache.Instance.InMission)
                            //&& (!IsSentry || (IsSentry && Settings.Instance.KillSentries))
                                   && (!IsTargeting && !IsTarget)
                                   && !IsContainer
                                   && CategoryId == (int)CategoryID.Entity
                                   && Distance < Cache.Instance.MaxTargetRange
                                   && !IsIgnored
                                   && (!IsBadIdea || IsAttacking)
                                   && !IsEntityIShouldLeaveAlone
                                   && !IsFactionWarfareNPC
                                   && !IsLargeCollidable
                                   && !IsStation);
                        _IsNotYetTargetingMeAndNotYetTargeted = result;
                        return _IsNotYetTargetingMeAndNotYetTargeted ?? false;
                    }

                    return _IsNotYetTargetingMeAndNotYetTargeted ?? false;
                }

                return false;
            }
        }

        public bool? _IsTargetWeCanShootButHaveNotYetTargeted;

        public bool IsTargetWeCanShootButHaveNotYetTargeted
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsTargetWeCanShootButHaveNotYetTargeted == null)
                    {
                        bool result = false;
                        result |= (CategoryId == (int)CategoryID.Entity
                                   && !IsTarget
                                   && !IsTargeting
                                   && Distance < Cache.Instance.MaxTargetRange
                                   && !IsIgnored
                                   && !IsStation);
                        _IsTargetWeCanShootButHaveNotYetTargeted = result;
                        return _IsTargetWeCanShootButHaveNotYetTargeted ?? false;    
                    }

                    return _IsTargetWeCanShootButHaveNotYetTargeted ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Frigate includes all elite-variants - this does NOT need to be limited to players, as we check for players specifically everywhere this is used
        /// </summary>
        /// 
        public bool? _IsFrigate;

        public bool IsFrigate
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsFrigate == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Frigate;
                        result |= GroupId == (int)Group.AssaultShip;
                        result |= GroupId == (int)Group.StealthBomber;
                        result |= GroupId == (int)Group.ElectronicAttackShip;
                        result |= GroupId == (int)Group.PrototypeExplorationShip;

                        // Technically not frigs, but for our purposes they are
                        result |= GroupId == (int)Group.Destroyer;
                        result |= GroupId == (int)Group.Interdictor;
                        result |= GroupId == (int)Group.Interceptor;
                        _IsFrigate = result;
                        return _IsFrigate ?? false;
                    }

                    return _IsFrigate ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Frigate includes all elite-variants - this does NOT need to be limited to players, as we check for players specifically everywhere this is used
        /// </summary>
        private bool? _IsNPCFrigate;

        public bool IsNPCFrigate
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNPCFrigate == null)
                    {
                        bool result = false;
                        if (IsPlayer)
                        {
                            //
                            // if it is a player it is by definition not an NPC
                            //
                            _IsNPCFrigate = false;
                            return _IsNPCFrigate ?? false;
                        }
                        result |= GroupId == (int)Group.Frigate;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Guristas_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_Destroyer;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Destroyer;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Destroyer;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Destroyer;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Destroyer;
                        result |= GroupId == (int)Group.Mission_Khanid_Destroyer;
                        result |= GroupId == (int)Group.Mission_CONCORD_Destroyer;
                        result |= GroupId == (int)Group.Mission_Mordu_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_Destroyer;
                        result |= GroupId == (int)Group.Mission_Thukker_Destroyer;
                        result |= GroupId == (int)Group.Mission_Generic_Destroyers;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_Destroyer;
                        result |= GroupId == (int)Group.asteroid_angel_cartel_frigate;
                        result |= GroupId == (int)Group.asteroid_blood_raiders_frigate;
                        result |= GroupId == (int)Group.asteroid_guristas_frigate;
                        result |= GroupId == (int)Group.asteroid_sanshas_nation_frigate;
                        result |= GroupId == (int)Group.asteroid_serpentis_frigate;
                        result |= GroupId == (int)Group.deadspace_angel_cartel_frigate;
                        result |= GroupId == (int)Group.deadspace_blood_raiders_frigate;
                        result |= GroupId == (int)Group.deadspace_guristas_frigate;
                        result |= GroupId == (int)Group.deadspace_sanshas_nation_frigate;
                        result |= GroupId == (int)Group.deadspace_serpentis_frigate;
                        result |= GroupId == (int)Group.mission_amarr_empire_frigate;
                        result |= GroupId == (int)Group.mission_caldari_state_frigate;
                        result |= GroupId == (int)Group.mission_gallente_federation_frigate;
                        result |= GroupId == (int)Group.mission_minmatar_republic_frigate;
                        result |= GroupId == (int)Group.mission_khanid_frigate;
                        result |= GroupId == (int)Group.mission_concord_frigate;
                        result |= GroupId == (int)Group.mission_mordu_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_frigate2;
                        result |= GroupId == (int)Group.asteroid_angel_cartel_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_blood_raiders_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_guristas_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_sanshas_nation_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_serpentis_commander_frigate;
                        result |= GroupId == (int)Group.mission_generic_frigates;
                        result |= GroupId == (int)Group.mission_thukker_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_commander_frigate;
                        result |= Name.Contains("Spider Drone"); //we *really* need to find out the GroupID of this one. 
                        _IsNPCFrigate = result;
                        return _IsNPCFrigate ?? false;    
                    }

                    return _IsNPCFrigate ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Cruiser includes all elite-variants
        /// </summary>
        public bool? _IsCruiser;
        
        public bool IsCruiser
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsCruiser == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Cruiser;
                        result |= GroupId == (int)Group.HeavyAssaultShip;
                        result |= GroupId == (int)Group.Logistics;
                        result |= GroupId == (int)Group.ForceReconShip;
                        result |= GroupId == (int)Group.CombatReconShip;
                        result |= GroupId == (int)Group.HeavyInterdictor;
                        _IsCruiser = result;
                        return _IsCruiser ?? false;
                    }

                    return _IsCruiser ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Cruiser includes all elite-variants
        /// </summary>
        public bool? _IsNPCCruiser;

        public bool IsNPCCruiser
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNPCCruiser == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Storyline_Cruiser;
                        result |= GroupId == (int)Group.Storyline_Mission_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Guristas_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_Cruiser;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Cruiser;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Cruiser;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Cruiser;
                        result |= GroupId == (int)Group.Mission_Khanid_Cruiser;
                        result |= GroupId == (int)Group.Mission_CONCORD_Cruiser;
                        result |= GroupId == (int)Group.Mission_Mordu_Cruiser;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_Cruiser;
                        result |= GroupId == (int)Group.Mission_Generic_Cruisers;
                        result |= GroupId == (int)Group.Deadspace_Overseer_Cruiser;
                        result |= GroupId == (int)Group.Mission_Thukker_Cruiser;
                        result |= GroupId == (int)Group.Mission_Generic_Battle_Cruisers;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_Cruiser;
                        result |= GroupId == (int)Group.Mission_Faction_Cruiser;
                        _IsNPCCruiser = result;
                        return _IsNPCCruiser ?? false;
                    }

                    return _IsNPCCruiser ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Battlecruiser includes all elite-variants
        /// </summary>
        public bool? _IsBattlecruiser;

        public bool IsBattlecruiser
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsBattlecruiser == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Battlecruiser;
                        result |= GroupId == (int)Group.CommandShip;
                        result |= GroupId == (int)Group.StrategicCruiser; // Technically a cruiser, but hits hard enough to be a BC :)
                        _IsBattlecruiser = result;
                        return _IsBattlecruiser ?? false;
                    }

                    return _IsBattlecruiser ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Battlecruiser includes all elite-variants
        /// </summary>
        public bool? _IsNPCBattlecruiser;

        public bool IsNPCBattlecruiser
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNPCBattlecruiser == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Guristas_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_BattleCruiser;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Khanid_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_CONCORD_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Mordu_Battlecruiser;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Mission_Thukker_Battlecruiser;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_BattleCruiser;
                        _IsNPCBattlecruiser = result;
                        return _IsNPCBattlecruiser ?? false;    
                    }

                    return _IsNPCBattlecruiser ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Battleship includes all elite-variants
        /// </summary>
        public bool? _IsBattleship;

        public bool IsBattleship
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsBattleship == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Battleship;
                        result |= GroupId == (int)Group.EliteBattleship;
                        result |= GroupId == (int)Group.BlackOps;
                        result |= GroupId == (int)Group.Marauder;
                        _IsBattleship = result;
                        return _IsBattleship ?? false;
                    }

                    return _IsBattleship ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// Battleship includes all elite-variants
        /// </summary>
        public bool? _IsNPCBattleship;
        
        public bool IsNPCBattleship
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNPCBattleship == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Storyline_Battleship;
                        result |= GroupId == (int)Group.Storyline_Mission_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Guristas_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_Battleship;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Battleship;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Battleship;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Battleship;
                        result |= GroupId == (int)Group.Mission_Khanid_Battleship;
                        result |= GroupId == (int)Group.Mission_CONCORD_Battleship;
                        result |= GroupId == (int)Group.Mission_Mordu_Battleship;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Battleship;
                        result |= GroupId == (int)Group.Mission_Generic_Battleships;
                        result |= GroupId == (int)Group.Deadspace_Overseer_Battleship;
                        result |= GroupId == (int)Group.Mission_Thukker_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_Battleship;
                        result |= GroupId == (int)Group.Mission_Faction_Battleship;
                        _IsNPCBattleship = result;
                        return _IsNPCBattleship ?? false;
                    }

                    return _IsNPCBattleship ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// A bad idea to attack these targets
        /// </summary>
        public bool? _IsLargeCollidable;
        
        public bool IsLargeCollidable
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsLargeCollidable == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.LargeColidableObject;
                        result |= GroupId == (int)Group.LargeColidableShip;
                        result |= GroupId == (int)Group.LargeColidableStructure;
                        result |= GroupId == (int)Group.DeadSpaceOverseersStructure;
                        result |= GroupId == (int)Group.DeadSpaceOverseersBelongings;
                        _IsLargeCollidable = result;
                        return _IsLargeCollidable ?? false;    
                    }

                    return _IsLargeCollidable ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// A bad idea to attack these targets
        /// </summary>
        public bool? _IsMiscJunk;

        public bool IsMiscJunk
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsMiscJunk == null)
                    {
                        if (Cache.Instance.Entities.Any(t => t.Id != Id))
                        {
                            return false;
                        }

                        bool result = false;
                        result |= GroupId == (int)Group.PlayerDrone;
                        result |= GroupId == (int)Group.Wreck;
                        result |= GroupId == (int)Group.AccelerationGate;
                        result |= GroupId == (int)Group.GasCloud;
                        _IsMiscJunk = result;
                        return _IsMiscJunk ?? false;
                    }

                    return _IsMiscJunk ?? false;
                }

                return false;
            }
        }

        /// <summary>
        /// A bad idea to attack these targets
        /// </summary>
        public bool? _IsBadIdea;

        public bool IsBadIdea
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsBadIdea == null)
                    {
                        if (Cache.Instance.Entities.Any(t => t.Id != Id))
                        {
                            return false;
                        }

                        bool result = false;
                        result |= GroupId == (int)Group.ConcordDrone;
                        result |= GroupId == (int)Group.PoliceDrone;
                        result |= GroupId == (int)Group.CustomsOfficial;
                        result |= GroupId == (int)Group.Billboard;
                        result |= GroupId == (int)Group.Stargate;
                        result |= GroupId == (int)Group.Station;
                        result |= GroupId == (int)Group.SentryGun;
                        result |= GroupId == (int)Group.Capsule;
                        result |= GroupId == (int)Group.MissionContainer;
                        result |= GroupId == (int)Group.CustomsOffice;
                        result |= GroupId == (int)Group.GasCloud;
                        result |= IsFrigate;
                        result |= IsCruiser;
                        result |= IsBattlecruiser;
                        result |= IsBattleship;
                        result |= IsPlayer;
                        _IsBadIdea = result;
                        return _IsBadIdea ?? false;
                    }

                    return _IsBadIdea ?? false;
                }

                return false;
            }
        }

        public bool? _IsFactionWarfareNPC;

        public bool IsFactionWarfareNPC
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsFactionWarfareNPC == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.FactionWarfareNPC;
                        _IsFactionWarfareNPC = result;
                        return _IsFactionWarfareNPC ?? false;
                    }

                    return _IsFactionWarfareNPC ?? false;
                }
                
                return false;
            }
        }


        public bool? _IsNpcByGroupID;

        public bool IsNpcByGroupID
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsNpcByGroupID == null)
                    {
                        bool result = false;
                        result |= IsSentry;
                        result |= GroupId == (int)Group.DeadSpaceOverseersStructure;
                        //result |= GroupId == (int)Group.DeadSpaceOverseersBelongings;
                        result |= GroupId == (int)Group.Storyline_Battleship;
                        result |= GroupId == (int)Group.Storyline_Mission_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Guristas_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_Battleship;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Battleship;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Battleship;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Battleship;
                        result |= GroupId == (int)Group.Mission_Khanid_Battleship;
                        result |= GroupId == (int)Group.Mission_CONCORD_Battleship;
                        result |= GroupId == (int)Group.Mission_Mordu_Battleship;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Battleship;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Battleship;
                        result |= GroupId == (int)Group.Mission_Generic_Battleships;
                        result |= GroupId == (int)Group.Deadspace_Overseer_Battleship;
                        result |= GroupId == (int)Group.Mission_Thukker_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_Battleship;
                        result |= GroupId == (int)Group.Mission_Faction_Battleship;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Guristas_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_BattleCruiser;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Khanid_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_CONCORD_Battlecruiser;
                        result |= GroupId == (int)Group.Mission_Mordu_Battlecruiser;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Mission_Thukker_Battlecruiser;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_BattleCruiser;
                        result |= GroupId == (int)Group.Storyline_Cruiser;
                        result |= GroupId == (int)Group.Storyline_Mission_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Guristas_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_Cruiser;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Cruiser;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Cruiser;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Cruiser;
                        result |= GroupId == (int)Group.Mission_Khanid_Cruiser;
                        result |= GroupId == (int)Group.Mission_CONCORD_Cruiser;
                        result |= GroupId == (int)Group.Mission_Mordu_Cruiser;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_Cruiser;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_Cruiser;
                        result |= GroupId == (int)Group.Mission_Generic_Cruisers;
                        result |= GroupId == (int)Group.Deadspace_Overseer_Cruiser;
                        result |= GroupId == (int)Group.Mission_Thukker_Cruiser;
                        result |= GroupId == (int)Group.Mission_Generic_Battle_Cruisers;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_Cruiser;
                        result |= GroupId == (int)Group.Mission_Faction_Cruiser;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Angel_Cartel_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Blood_Raiders_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Guristas_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Sanshas_Nation_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Serpentis_Destroyer;
                        result |= GroupId == (int)Group.Mission_Amarr_Empire_Destroyer;
                        result |= GroupId == (int)Group.Mission_Caldari_State_Destroyer;
                        result |= GroupId == (int)Group.Mission_Gallente_Federation_Destroyer;
                        result |= GroupId == (int)Group.Mission_Minmatar_Republic_Destroyer;
                        result |= GroupId == (int)Group.Mission_Khanid_Destroyer;
                        result |= GroupId == (int)Group.Mission_CONCORD_Destroyer;
                        result |= GroupId == (int)Group.Mission_Mordu_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Angel_Cartel_Commander_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Blood_Raiders_Commander_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Guristas_Commander_Destroyer;
                        result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Sanshas_Nation_Commander_Destroyer;
                        result |= GroupId == (int)Group.Asteroid_Serpentis_Commander_Destroyer;
                        result |= GroupId == (int)Group.Mission_Thukker_Destroyer;
                        result |= GroupId == (int)Group.Mission_Generic_Destroyers;
                        result |= GroupId == (int)Group.Asteroid_Rogue_Drone_Commander_Destroyer;
                        result |= GroupId == (int)Group.asteroid_angel_cartel_frigate;
                        result |= GroupId == (int)Group.asteroid_blood_raiders_frigate;
                        result |= GroupId == (int)Group.asteroid_guristas_frigate;
                        result |= GroupId == (int)Group.asteroid_sanshas_nation_frigate;
                        result |= GroupId == (int)Group.asteroid_serpentis_frigate;
                        result |= GroupId == (int)Group.deadspace_angel_cartel_frigate;
                        result |= GroupId == (int)Group.deadspace_blood_raiders_frigate;
                        result |= GroupId == (int)Group.deadspace_guristas_frigate;
                        result |= GroupId == (int)Group.deadspace_sanshas_nation_frigate;
                        result |= GroupId == (int)Group.deadspace_serpentis_frigate;
                        result |= GroupId == (int)Group.mission_amarr_empire_frigate;
                        result |= GroupId == (int)Group.mission_caldari_state_frigate;
                        result |= GroupId == (int)Group.mission_gallente_federation_frigate;
                        result |= GroupId == (int)Group.mission_minmatar_republic_frigate;
                        result |= GroupId == (int)Group.mission_khanid_frigate;
                        result |= GroupId == (int)Group.mission_concord_frigate;
                        result |= GroupId == (int)Group.mission_mordu_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_frigate2;
                        result |= GroupId == (int)Group.asteroid_angel_cartel_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_blood_raiders_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_guristas_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_sanshas_nation_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_serpentis_commander_frigate;
                        result |= GroupId == (int)Group.mission_generic_frigates;
                        result |= GroupId == (int)Group.mission_thukker_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_commander_frigate;
                        _IsNpcByGroupID = result;
                        return _IsNpcByGroupID ?? false;
                    }

                    return _IsNpcByGroupID ?? false;
                }

                return false;
            }
        }

        public bool? _IsEntityIShouldLeaveAlone;

        public bool IsEntityIShouldLeaveAlone
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsEntityIShouldLeaveAlone == null)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Merchant;            // Merchant, Convoy?
                        result |= GroupId == (int)Group.Mission_Merchant;    // Merchant, Convoy? - Dread Pirate Scarlet
                        result |= IsOreOrIce;
                        _IsEntityIShouldLeaveAlone = result;
                        return _IsEntityIShouldLeaveAlone ?? false;
                    }

                    return _IsEntityIShouldLeaveAlone ?? false;
                }

                return false;
            }
        }

        public bool? _IsOnGridWithMe;

        public bool IsOnGridWithMe
        {
            get
            {
                if (_directEntity != null)
                {
                    if (_IsOnGridWithMe == null)
                    {
                        bool result = false;
                        result |= Distance < (double)Distances.OnGridWithMe;
                        _IsOnGridWithMe = result;
                        return _IsOnGridWithMe ?? false;
                    }

                    return _IsOnGridWithMe ?? false;
                }

                return false;
            }
        }

        public bool IsStation
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Station;
                return result;
            }
        }

        public bool IsCustomsOffice
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.CustomsOffice;
                return result;
            }
        }

        public bool IsCelestial
        {
            get
            {
                bool result = false;
                result |= CategoryId == (int) CategoryID.Celestial;
                result |= CategoryId == (int) CategoryID.Station;
                result |= GroupId == (int) Group.Moon;
                result |= GroupId == (int) Group.AsteroidBelt;
                return result;
            }
        }

        public bool IsAsteroidBelt
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.AsteroidBelt;
                return result;
            }
        }

        public bool IsPlanet
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Planet;
                return result;
            }
        }

        public bool IsMoon
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Moon;
                return result;
            }
        }

        public bool IsAsteroid
        {
            get
            {
                bool result = false;
                result |= CategoryId == (int)CategoryID.Asteroid;
                return result;
            }
        }

        public bool IsShipWithNoDroneBay
        {
            get
            {
                bool result = false;
                result |= TypeId == (int)TypeID.Tengu;
                result |= GroupId == (int)Group.Shuttle;
                return result;
            }
        }

        public bool IsOreOrIce
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Plagioclase;
                result |= GroupId == (int)Group.Spodumain;
                result |= GroupId == (int)Group.Kernite;
                result |= GroupId == (int)Group.Hedbergite;
                result |= GroupId == (int)Group.Arkonor;
                result |= GroupId == (int)Group.Bistot;
                result |= GroupId == (int)Group.Pyroxeres;
                result |= GroupId == (int)Group.Crokite;
                result |= GroupId == (int)Group.Jaspet;
                result |= GroupId == (int)Group.Omber;
                result |= GroupId == (int)Group.Scordite;
                result |= GroupId == (int)Group.Gneiss;
                result |= GroupId == (int)Group.Veldspar;
                result |= GroupId == (int)Group.Hemorphite;
                result |= GroupId == (int)Group.DarkOchre;
                result |= GroupId == (int)Group.Ice;
                return result;
            }
        }
        

        public bool LockTarget(string module)
        {
            // If the bad idea is attacking, attack back
            if (IsBadIdea && !IsAttacking)
            {
                Logging.Log("EntityCache.LockTarget", "[" + module + "] Attempted to target a player or concord entity! [" + Name + "] - aborting", Logging.White);
                return false;
            }

            if (Distance >= 250001 || Distance > Cache.Instance.MaxTargetRange) //250k is the MAX targeting range in eve. 
            {
                Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "] which is [" + Math.Round(Distance / 1000, 2) + "k] away. Do not try to lock things that you cant possibly target", Logging.Debug);
                return false;
            }

            // Remove the target info (its been targeted)
            foreach (EntityCache target in Cache.Instance.Entities.Where(e => e.IsTarget && Cache.Instance.TargetingIDs.ContainsKey(e.Id)))
            {
                Cache.Instance.TargetingIDs.Remove(target.Id);
            }

            if (Cache.Instance.TargetingIDs.ContainsKey(Id))
            {
                DateTime lastTargeted = Cache.Instance.TargetingIDs[Id];

                // Ignore targeting request
                double seconds = DateTime.UtcNow.Subtract(lastTargeted).TotalSeconds;
                if (seconds < 20)
                {
                    Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, can retarget in [" + Math.Round(20 - seconds, 0) + "]", Logging.White);
                    return false;
                }
            }

            // Only add targeting id's when its actually being targeted
            if (_directEntity != null)
            {
                if (!IsTarget)
                {
                    if (!HasExploded)
                    {
                        if (Distance < Cache.Instance.MaxTargetRange)
                        {
                            if (Cache.Instance.Targets.Count() < Cache.Instance.MaxLockedTargets)
                            {
                                if (!IsTargeting)
                                {
                                    if (Cache.Instance.Entities.Any(i => i.Id == Id))
                                    {
                                        if (_directEntity.LockTarget())
                                        {
                                            Cache.Instance.TargetingIDs[Id] = DateTime.UtcNow;
                                            return true;
                                        }

                                        Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget failed (unknown reason)", Logging.White);
                                    }

                                    Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget failed: target was not in Entities List", Logging.White);
                                }

                                Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget aborted: target is already being targeted", Logging.White);
                            }
                            else
                            {
                                Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, we are out of targeting slots!", Logging.White);
                            }
                        }
                        else
                        {
                            Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, target is out of range!", Logging.White);
                        }
                    }
                    else
                    {
                        Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Cache.Instance.Targets.Count() + "] targets already, target is alread dead!", Logging.White);
                    }
                }
                else
                {
                    Logging.Log("EntityCache.LockTarget", "[" + module + "] LockTarget req has been ignored for [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, target is already locked!", Logging.White);
                }

                return false;
            }

            return false;
        }

        public bool UnlockTarget(string module)
        {
            if (_directEntity != null)
            {
                //if (Distance > 250001)
                //{
                //    return false;
                //}

                Cache.Instance.TargetingIDs.Remove(Id);

                if (IsTarget)
                {
                    _directEntity.UnlockTarget();
                    return true;
                }
                
                return false;
            }

            return false;
        }

        public void Jump()
        {
            if (_directEntity != null)

                //Cache.Instance._lastDockedorJumping = DateTime.UtcNow;
                _directEntity.Jump();
        }

        public void Activate()
        {
            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextActivateAction)
            {
                _directEntity.Activate();
                Cache.Instance.LastInWarp = DateTime.UtcNow;
                Cache.Instance.NextActivateAction = DateTime.UtcNow.AddSeconds(15);
            }
        }

        public void Approach()
        {
            Cache.Instance.Approaching = this;

            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextApproachAction)
            {
                Cache.Instance.NextApproachAction = DateTime.UtcNow.AddSeconds(Time.Instance.ApproachDelay_seconds);
                _directEntity.Approach();
            }
        }

        public void Approach(int range)
        {
            Cache.Instance.Approaching = this;

            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextApproachAction)
            {
                Cache.Instance.NextApproachAction = DateTime.UtcNow.AddSeconds(Time.Instance.ApproachDelay_seconds);
                _directEntity.KeepAtRange(range);
            }
        }

        public void Orbit(int range)
        {
            Cache.Instance.Approaching = this;

            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextOrbit)
            {
                Cache.Instance.NextOrbit = DateTime.UtcNow.AddSeconds(Time.Instance.OrbitDelay_seconds);
                _directEntity.Orbit(range);
            }
        }

        public void WarpTo()
        {
            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextWarpTo)
            {
                Cache.Instance.LastInWarp = DateTime.UtcNow;
                Cache.Instance.NextWarpTo = DateTime.UtcNow.AddSeconds(Time.Instance.WarptoDelay_seconds);
                _directEntity.WarpTo();
            }
        }

        public void AlignTo()
        {
            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextAlign)
            {
                Cache.Instance.NextAlign = DateTime.UtcNow.AddMinutes(Time.Instance.AlignDelay_minutes);
                _directEntity.AlignTo();
            }
        }

        public void WarpToAndDock()
        {
            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextWarpTo && DateTime.UtcNow > Cache.Instance.NextDockAction)
            {
                Cache.Instance.LastInWarp = DateTime.UtcNow;
                Cache.Instance.NextWarpTo = DateTime.UtcNow.AddSeconds(Time.Instance.WarptoDelay_seconds);
                Cache.Instance.NextDockAction = DateTime.UtcNow.AddSeconds(Time.Instance.DockingDelay_seconds);
                _directEntity.WarpToAndDock();
            }
        }

        public void Dock()
        {
            if (_directEntity != null && DateTime.UtcNow > Cache.Instance.NextDockAction)
            {
                _directEntity.Dock();
                Cache.Instance.NextDockAction = DateTime.UtcNow.AddSeconds(Time.Instance.DockingDelay_seconds);
            }
        }

        public void OpenCargo()
        {
            if (_directEntity != null)
            {
                _directEntity.OpenCargo();
                Cache.Instance.NextOpenCargoAction = DateTime.UtcNow.AddSeconds(2 + Cache.Instance.RandomNumber(1, 3));
            }
        }

        public void MakeActiveTarget()
        {
            if (_directEntity != null)
            {                
                if (_directEntity.IsTarget)
                {
                    _directEntity.MakeActiveTarget();
                    Cache.Instance.NextMakeActiveTargetAction = DateTime.UtcNow.AddSeconds(1 + Cache.Instance.RandomNumber(2, 3));
                }
                
                return;
            }

            return;
        }
    }
}