// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;

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
        private readonly DirectEntity _directEntity;
        public static int EntityCacheInstances = 0;
        private DateTime ThisEntityCacheCreated = DateTime.UtcNow;

        public EntityCache(DirectEntity entity)
        {
            _directEntity = entity;
            Interlocked.Increment(ref EntityCacheInstances);
            ThisEntityCacheCreated = DateTime.UtcNow;
        }

        ~EntityCache()
        {
            Interlocked.Decrement(ref EntityCacheInstances);
        }

        public bool BookmarkThis(string NameOfBookmark = "bookmark", string Comment = "")
        {
            try
            {
                if (Cache.Instance.BookmarksByLabel(NameOfBookmark).Any(i => i.LocationId == Cache.Instance.DirectEve.Session.LocationId))
                {
                    List<DirectBookmark> PreExistingBookmarks = Cache.Instance.BookmarksByLabel(NameOfBookmark);
                    if (PreExistingBookmarks.Any())
                    {
                        foreach (DirectBookmark _PreExistingBookmark in PreExistingBookmarks)
                        {

                            if (_PreExistingBookmark.X == _directEntity.X 
                             && _PreExistingBookmark.Y == _directEntity.Y 
                             && _PreExistingBookmark.Z == _directEntity.Z)
                            {
                                Logging.Log("EntityCache.BookmarkThis", "We already have a bookmark for [" + _directEntity.Name + "] and do not need another.", Logging.Debug);
                                return true;
                            }
                            continue;
                        }
                    }
                }

                if (IsLargeCollidable || IsStation || IsAsteroid || IsAsteroidBelt)
                {
                    Cache.Instance.DirectEve.BookmarkEntity(_directEntity, NameOfBookmark, Comment, 0, false);
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
            
            return false;
        }

        private int? _groupID;

        public int GroupId
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_groupID == null)
                        {
                            _groupID = _directEntity.GroupId;
                        }

                        return _groupID ?? _directEntity.GroupId;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private int? _categoryId;

        public int CategoryId
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_categoryId == null)
                        {
                            _categoryId = _directEntity.CategoryId;
                        }

                        return _categoryId ?? _directEntity.CategoryId;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private long? _id;

        public long Id
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_id == null)
                        {
                            _id = _directEntity.Id;
                        }

                        return _id ?? _directEntity.Id;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private int? _TypeId;

        public int TypeId
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_TypeId == null)
                        {
                            _TypeId = _directEntity.TypeId;
                        }

                        return _TypeId ?? _directEntity.TypeId;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private long? _followId;

        public long FollowId
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_followId == null)
                        {
                            _followId = _directEntity.FollowId;
                        }

                        return _followId ?? _directEntity.FollowId;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private string _name;

        public string Name
        {
            get
            {
                try
                {

                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                        {
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }

                        if (_name == null)
                        {
                            _name = _directEntity.Name;
                        }

                        return _name ?? _directEntity.Name ?? string.Empty;
                    }

                    return string.Empty;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return string.Empty;
                }

            }
        }

        private double? _distance;

        public double Distance
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                        {
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }
                        if (_distance == null)
                        {
                            _distance = _directEntity.Distance;
                        }

                        return _distance ?? _directEntity.Distance;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private double? _nearest5kDistance;

        public double Nearest5kDistance
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_nearest5kDistance == null)
                        {
                            if (_directEntity.Distance > 0 && _directEntity.Distance < 900000000)
                            {
                                _nearest5kDistance = Math.Round((_directEntity.Distance / 1000) * 2, MidpointRounding.AwayFromZero) / 2;
                            }
                        }

                        return _nearest5kDistance ?? _directEntity.Distance;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private double? _shieldPct;

        public double ShieldPct
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_shieldPct == null)
                        {
                            _shieldPct = _directEntity.ShieldPct;
                        }

                        return _shieldPct ?? _directEntity.ShieldPct;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private double? _armorPct;

        public double ArmorPct
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_armorPct == null)
                        {
                            _armorPct = _directEntity.ArmorPct;
                        }

                        return _armorPct ?? _directEntity.ArmorPct;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private double? _structurePct;

        public double StructurePct
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_structurePct == null)
                        {
                            _structurePct = _directEntity.StructurePct;
                        }

                        return _structurePct ?? _directEntity.StructurePct;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private bool? _isNpc;

        public bool IsNpc
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isNpc == null)
                        {
                            _isNpc = _directEntity.IsNpc;
                        }

                        return _isNpc ?? _directEntity.IsNpc;

                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private double? _velocity;

        public double Velocity
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_velocity == null)
                        {
                            _velocity = _directEntity.Velocity;
                        }

                        return _velocity ?? _directEntity.Velocity;
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private bool? _isTarget;

        public bool IsTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isTarget == null)
                        {
                            if (!_directEntity.HasExploded && Cache.Instance.Entities.Any(t => t.Id == _directEntity.Id))
                            {
                                _isTarget = _directEntity.IsTarget;
                            }
                        }

                        return _isTarget ?? _directEntity.IsTarget;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isCorrectSizeForMyWeapons;

        public bool IsCorrectSizeForMyWeapons
        {
            get
            {
                try
                {
                    if (_isCorrectSizeForMyWeapons == null)
                    {
                        if (Cache.Instance.MyShipEntity.IsFrigate)
                        {
                            if (IsFrigate)
                            {
                                _isCorrectSizeForMyWeapons = true;
                                return _isCorrectSizeForMyWeapons ?? true;
                            }
                        }

                        if (Cache.Instance.MyShipEntity.IsCruiser)
                        {
                            if (IsCruiser)
                            {
                                _isCorrectSizeForMyWeapons = true;
                                return _isCorrectSizeForMyWeapons ?? true;
                            }
                        }

                        if (Cache.Instance.MyShipEntity.IsBattlecruiser || Cache.Instance.MyShipEntity.IsBattleship)
                        {
                            if (IsBattleship || IsBattlecruiser)
                            {
                                _isCorrectSizeForMyWeapons = true;
                                return _isCorrectSizeForMyWeapons ?? true;
                            }
                        }

                        return false;
                    }

                    return _isCorrectSizeForMyWeapons ?? false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                }

                return false;
            }

        }

        private bool? _isPreferredPrimaryWeaponTarget;

        public bool isPreferredPrimaryWeaponTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isPreferredPrimaryWeaponTarget == null)
                        {
                            if (Cache.Instance.PreferredPrimaryWeaponTarget != null && Cache.Instance.PreferredPrimaryWeaponTarget.Id == _directEntity.Id)
                            {
                                _isPreferredPrimaryWeaponTarget = true;
                                return _isPreferredPrimaryWeaponTarget ?? true;
                            }

                            _isPreferredPrimaryWeaponTarget = false;
                            return _isPreferredPrimaryWeaponTarget ?? false;
                        }

                        return _isPreferredPrimaryWeaponTarget ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isPrimaryWeaponKillPriority;

        public bool IsPrimaryWeaponKillPriority
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isPrimaryWeaponKillPriority == null)
                        {
                            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(e => e.Entity.Id == _directEntity.Id))
                            {
                                _isPrimaryWeaponKillPriority = true;
                            }
                        }

                        return _isPrimaryWeaponKillPriority ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isPreferredDroneTarget;

        public bool isPreferredDroneTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isPreferredDroneTarget == null)
                        {
                            if (Cache.Instance.PreferredDroneTarget != null && Cache.Instance.PreferredDroneTarget.Id == _directEntity.Id)
                            {
                                _isPreferredDroneTarget = true;
                                return _isPreferredDroneTarget ?? true;
                            }

                            _isPreferredDroneTarget = false;
                            return _isPreferredDroneTarget ?? true;
                        }

                        return _isPreferredDroneTarget ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _IsDroneKillPriority;

        public bool IsDroneKillPriority
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_IsDroneKillPriority == null)
                        {
                            if (Cache.Instance.DronePriorityTargets.Any(e => e.Entity.Id == _directEntity.Id))
                            {
                                _IsDroneKillPriority = true;
                            }
                        }

                        return _IsDroneKillPriority ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _IsTooCloseTooFastTooSmallToHit;

        public bool IsTooCloseTooFastTooSmallToHit
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _IsReadyToShoot;

        public bool IsReadyToShoot
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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

                    if (Settings.Instance.DebugIsReadyToShoot) Logging.Log("IsReadyToShoot", "_directEntity is null or invalid", Logging.Debug);
                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _IsReadyToTarget;

        public bool IsReadyToTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isHigherPriorityPresent;

        public bool IsHigherPriorityPresent
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isHigherPriorityPresent == null)
                        {
                            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any() || Cache.Instance.DronePriorityTargets.Any())
                            {
                                if (Cache.Instance.PrimaryWeaponPriorityTargets.Any())
                                {
                                    if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.EntityID == _directEntity.Id))
                                    {
                                        PrimaryWeaponPriority _currentPrimaryWeaponPriority = Cache.Instance.PrimaryWeaponPriorityEntities.Where(t => t.Id == _directEntity.Id).Select(pt => pt.PrimaryWeaponPriorityLevel).FirstOrDefault();

                                        if (!Cache.Instance.PrimaryWeaponPriorityEntities.All(pt => pt.PrimaryWeaponPriorityLevel < _currentPrimaryWeaponPriority && pt.Distance < Cache.Instance.MaxRange))
                                        {
                                            _isHigherPriorityPresent = true;
                                            return _isHigherPriorityPresent ?? true;
                                        }

                                        _isHigherPriorityPresent = false;
                                        return _isHigherPriorityPresent ?? false;
                                    }

                                    if (Cache.Instance.PrimaryWeaponPriorityEntities.Any(e => e.Distance < Cache.Instance.MaxRange))
                                    {
                                        _isHigherPriorityPresent = true;
                                        return _isHigherPriorityPresent ?? true;
                                    }

                                    _isHigherPriorityPresent = false;
                                    return _isHigherPriorityPresent ?? false;
                                }

                                if (Cache.Instance.DronePriorityTargets.Any())
                                {
                                    if (Cache.Instance.DronePriorityTargets.Any(pt => pt.EntityID == _directEntity.Id))
                                    {
                                        DronePriority _currentEntityDronePriority = Cache.Instance.DronePriorityEntities.Where(t => t.Id == _directEntity.Id).Select(pt => pt.DronePriorityLevel).FirstOrDefault();

                                        if (!Cache.Instance.DronePriorityEntities.All(pt => pt.DronePriorityLevel < _currentEntityDronePriority && pt.Distance < Settings.Instance.DroneControlRange))
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    if (Cache.Instance.DronePriorityEntities.Any(e => e.Distance < Settings.Instance.DroneControlRange))
                                    {
                                        _isHigherPriorityPresent = true;
                                        return _isHigherPriorityPresent ?? true;
                                    }

                                    _isHigherPriorityPresent = false;
                                    return _isHigherPriorityPresent ?? false;
                                }

                                _isHigherPriorityPresent = false;
                                return _isHigherPriorityPresent ?? false;
                            }
                        }

                        return _isHigherPriorityPresent ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isActiveTarget;

        public bool IsActiveTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isActiveTarget == null)
                        {
                            if (_directEntity.IsActiveTarget)
                            {
                                _isActiveTarget = true;
                                return _isActiveTarget ?? true;
                            }

                            _isActiveTarget = false;
                            return _isActiveTarget ?? false;
                        }

                        return _isActiveTarget ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isInOptimalRange;

        public bool IsInOptimalRange
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isInOptimalRange == null)
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

                                if (optimal > Cache.Instance.ActiveShip.MaxTargetRange)
                                {
                                    optimal = Cache.Instance.ActiveShip.MaxTargetRange - 500;
                                }

                                if (Cache.Instance.DoWeCurrentlyHaveTurretsMounted()) //Lasers, Projectile, and Hybrids
                                {
                                    if (Distance > Settings.Instance.InsideThisRangeIsHardToTrack)
                                    {
                                        if (Distance < (optimal * 1.5) && Distance < Cache.Instance.ActiveShip.MaxTargetRange)
                                        {
                                            _isInOptimalRange = true;
                                            return _isInOptimalRange ?? true;
                                        }
                                    }
                                }
                                else //missile boats - use max range
                                {
                                    optimal = Cache.Instance.MaxRange;
                                    if (Distance < optimal)
                                    {
                                        _isInOptimalRange = true;
                                        return _isInOptimalRange ?? true;
                                    }
                                }

                                _isInOptimalRange = false;
                                return _isInOptimalRange ?? false;
                            }

                            // If you have no optimal you have to assume the entity is within Optimal... (like missiles)
                            _isInOptimalRange = true;
                            return _isInOptimalRange ?? true;
                        }

                        return _isInOptimalRange ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsInOptimalRangeOrNothingElseAvail
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (IsInOptimalRange)
                        {
                            return true;
                        }

                        if (!Cache.Instance.Targets.Any())
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsInDroneRange
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (Settings.Instance.DroneControlRange > 0) //&& Cache.Instance.UseDrones)
                        {
                            if (_directEntity.Distance < Settings.Instance.DroneControlRange)
                            {
                                return true;
                            }

                            return false;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsDronePriorityTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {

                        if (Cache.Instance.DronePriorityTargets.All(i => i.EntityID != _directEntity.Id))
                        {
                            return false;
                        }

                        return true;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsPriorityWarpScrambler
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.EntityID == Id))
                        {
                            EntityCache __entity = new EntityCache(_directEntity);
                            if (__entity.PrimaryWeaponPriorityLevel == PrimaryWeaponPriority.WarpScrambler)
                            {
                                return true;
                            }
                        }

                        if (Cache.Instance.DronePriorityTargets.Any(pt => pt.EntityID == Id))
                        {
                            EntityCache __entity = new EntityCache(_directEntity);
                            if (__entity.DronePriorityLevel == DronePriority.WarpScrambler)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsPrimaryWeaponPriorityTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(i => i.EntityID == Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public PrimaryWeaponPriority PrimaryWeaponPriorityLevel
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.EntityID == Id))
                        {
                            PrimaryWeaponPriority currentTargetPriority = Cache.Instance.PrimaryWeaponPriorityTargets.Where(t => t.Entity.IsTarget
                                                                                                                                && t.EntityID == Id)
                                                                                                                        .Select(pt => pt.PrimaryWeaponPriority)
                                                                                                                        .FirstOrDefault();
                            return currentTargetPriority;
                        }

                        return PrimaryWeaponPriority.NotUsed;
                    }

                    return PrimaryWeaponPriority.NotUsed;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return PrimaryWeaponPriority.NotUsed;
                }
            }
        }

        public DronePriority DronePriorityLevel
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {

                        if (Cache.Instance.DronePriorityTargets.Any(pt => pt.EntityID == _directEntity.Id))
                        {
                            DronePriority currentTargetPriority = Cache.Instance.DronePriorityTargets.Where(t => t.Entity.IsTarget
                                                                                                                        && t.EntityID == Id)
                                                                                                                        .Select(pt => pt.DronePriority)
                                                                                                                        .FirstOrDefault();

                            return currentTargetPriority;
                        }

                        return DronePriority.NotUsed;
                    }

                    return DronePriority.NotUsed;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return DronePriority.NotUsed;
                }
            }
        }

        private bool? _isTargeting;

        public bool IsTargeting
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isTargeting == null)
                        {
                            _isTargeting = _directEntity.IsTargeting;
                            return _isTargeting ?? false;
                        }

                        return _isTargeting ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isTargetedBy;

        public bool IsTargetedBy
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isTargetedBy == null)
                        {
                            _isTargetedBy = _directEntity.IsTargetedBy;
                            return _isTargetedBy ?? false;
                        }

                        return _isTargetedBy ?? false; 
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsAttacking
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        return _directEntity.IsAttacking;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsWreckEmpty
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        return _directEntity.IsEmpty;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool HasReleased
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        return _directEntity.HasReleased;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool HasExploded
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        return _directEntity.HasExploded;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isEwarTarget;

        public bool IsEwarTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isEwarTarget == null)
                        {
                            bool result = false;
                            result |= IsWarpScramblingMe;
                            result |= IsWebbingMe;
                            result |= IsNeutralizingMe;
                            result |= IsJammingMe;
                            result |= IsSensorDampeningMe;
                            result |= IsTargetPaintingMe;
                            result |= IsTrackingDisruptingMe;
                            _isEwarTarget = result;
                            return _isEwarTarget ?? false;
                        }

                        return _isEwarTarget ?? false;
                    }

                    return _isEwarTarget ?? false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public DronePriority IsActiveDroneEwarType
        {
            get
            {
                try
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
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return DronePriority.NotUsed;
                }
            }
        }

        public PrimaryWeaponPriority IsActivePrimaryWeaponEwarType
        {
            get
            {
                try
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
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return PrimaryWeaponPriority.NotUsed;
                }
            }
        }

        public bool IsWarpScramblingMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (Cache.Instance.WarpScrambler.Contains(Id))
                        {
                            return true;
                        }

                        if (_directEntity.Attacks.Contains("effects.WarpScramble"))
                        {
                            if (!Cache.Instance.WarpScrambler.Contains(Id))
                            {
                                Cache.Instance.WarpScrambler.Add(Id);
                            }

                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsWebbingMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {

                        if (_directEntity.Attacks.Contains("effects.ModifyTargetSpeed"))
                        {
                            if (!Cache.Instance.Webbing.Contains(Id)) Cache.Instance.Webbing.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.Webbing.Contains(Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsNeutralizingMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewEnergyNeut"))
                        {
                            if (!Cache.Instance.Neuting.Contains(Id)) Cache.Instance.Neuting.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.Neuting.Contains(Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsJammingMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("electronic"))
                        {
                            if (!Cache.Instance.Jammer.Contains(Id)) Cache.Instance.Jammer.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.Jammer.Contains(Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsSensorDampeningMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewRemoteSensorDamp"))
                        {
                            if (!Cache.Instance.Dampening.Contains(Id)) Cache.Instance.Dampening.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.Dampening.Contains(Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsTargetPaintingMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_directEntity.ElectronicWarfare.Contains("ewTargetPaint"))
                        {
                            if (!Cache.Instance.TargetPainting.Contains(Id)) Cache.Instance.TargetPainting.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.TargetPainting.Contains(Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsTrackingDisruptingMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {

                        if (_directEntity.ElectronicWarfare.Contains("ewTrackingDisrupt"))
                        {
                            if (!Cache.Instance.TrackingDisrupter.Contains(Id)) Cache.Instance.TrackingDisrupter.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.TrackingDisrupter.Contains(Id))
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public int Health
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        return (int)((ShieldPct + ArmorPct + StructurePct) * 100);
                    }

                    return 0;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return 0;
                }
            }
        }

        private bool? _isEntityIShouldKeepShooting;

        public bool IsEntityIShouldKeepShooting
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isEntityIShouldKeepShooting == null)
                        {
                            //
                            // Is our current target already in armor? keep shooting the same target if so...
                            //
                            if (IsReadyToShoot
                                && IsInOptimalRange && !IsLargeCollidable
                                && (((!IsFrigate && !IsNPCFrigate) || !IsTooCloseTooFastTooSmallToHit))
                                    && ArmorPct * 100 < Settings.Instance.DoNotSwitchTargetsIfTargetHasMoreThanThisArmorDamagePercentage)
                            {
                                if (Settings.Instance.DebugGetBestTarget) Logging.Log("EntityCache.IsEntityIShouldKeepShooting", "[" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + " GroupID [" + GroupId + "]] has less than 60% armor, keep killing this target", Logging.Debug);
                                _isEntityIShouldKeepShooting = true;
                                return _isEntityIShouldKeepShooting ?? true;
                            }
                        }

                        return _isEntityIShouldKeepShooting ?? false;
                    }
                    
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.Log("EntityCache.IsEntityIShouldKeepShooting", "Exception: [" + ex + "]", Logging.Debug);
                }

                return false;
            }
        }

        private bool? _isSentry;

        public bool IsSentry
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isSentry == null)
                        {
                            bool result = false;
                            //if (GroupId == (int)Group.SentryGun) return true;
                            result |= (GroupId == (int)Group.ProtectiveSentryGun);
                            result |= (GroupId == (int)Group.MobileSentryGun);
                            result |= (GroupId == (int)Group.DestructibleSentryGun);
                            result |= (GroupId == (int)Group.MobileMissileSentry);
                            result |= (GroupId == (int)Group.MobileProjectileSentry);
                            result |= (GroupId == (int)Group.MobileLaserSentry);
                            result |= (GroupId == (int)Group.MobileHybridSentry);
                            result |= (GroupId == (int)Group.DeadspaceOverseersSentry);
                            result |= (GroupId == (int)Group.StasisWebificationBattery);
                            result |= (GroupId == (int)Group.EnergyNeutralizingBattery);
                            _isSentry = result;
                            return _isSentry ?? false;
                        }

                        return _isSentry ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public double IsIgnoredRefreshes;

        private bool? _isIgnored;

        public bool IsIgnored
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isIgnored == null)
                        {
                            IsIgnoredRefreshes++;
                            if (Cache.Instance.Entities.All(t => t.Id != _directEntity.Id))
                            {
                                IsIgnoredRefreshes = IsIgnoredRefreshes + 1000;
                                _isIgnored = true;
                                return _isIgnored ?? true;
                            }

                            if (Cache.Instance.IgnoreTargets.Any())
                            {
                                _isIgnored = Cache.Instance.IgnoreTargets.Contains(_directEntity.Name.Trim());
                                if ((bool)_isIgnored)
                                {
                                    if (Cache.Instance.PreferredPrimaryWeaponTarget != null && Cache.Instance.PreferredPrimaryWeaponTarget.Id != Id)
                                    {
                                        Cache.Instance.PreferredPrimaryWeaponTarget = null;
                                    }
                                }
                                return _isIgnored ?? false;
                            }

                            return _isIgnored ?? false;
                        }

                        return _isIgnored ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool HaveLootRights
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {

                        if (GroupId == (int)Group.SpawnContainer)
                        {
                            return true;
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
                            return result;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private int? _targetValue;

        public int? TargetValue
        {
            get
            {
                try
                {
                    int result = -1;

                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_targetValue == null)
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
                                    _targetValue = 4;
                                }
                                else if (IsNPCBattlecruiser)
                                {
                                    _targetValue = 3;
                                }
                                else if (IsNPCCruiser)
                                {
                                    _targetValue = 2;
                                }
                                else if (IsNPCFrigate)
                                {
                                    _targetValue = 0;
                                }

                                return _targetValue ?? -1;
                            }

                            _targetValue = value.TargetValue;
                            return _targetValue;
                        }

                        return _targetValue ?? -1;
                    }

                    return result;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return -1;
                }
            }
        }

        private bool? _isHighValueTarget;

        public bool IsHighValueTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isHighValueTarget == null)
                        {
                            if (TargetValue != null)
                            {
                                if (!IsIgnored || !IsContainer || !IsBadIdea || !IsCustomsOffice || !IsFactionWarfareNPC || !IsPlayer)
                                {
                                    if (TargetValue >= Settings.Instance.MinimumTargetValueToConsiderTargetAHighValueTarget)
                                    {
                                        if (IsSentry && !Settings.Instance.KillSentries)
                                        {
                                            _isHighValueTarget = false;
                                            return _isHighValueTarget ?? false;
                                        }

                                        _isHighValueTarget = true;
                                        return _isHighValueTarget ?? true;
                                    }

                                    //if (IsLargeCollidable)
                                    //{
                                    //    return true;
                                    //}    
                                }

                                _isHighValueTarget = false;
                                return _isHighValueTarget ?? false;
                            }

                            _isHighValueTarget = false;
                            return _isHighValueTarget ?? false;
                        }

                        return _isHighValueTarget ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isLowValueTarget;

        public bool IsLowValueTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isLowValueTarget == null)
                        {
                            if (!IsIgnored || !IsContainer || !IsBadIdea || !IsCustomsOffice || !IsFactionWarfareNPC || !IsPlayer)
                            {
                                if (TargetValue != null && TargetValue <= Settings.Instance.MaximumTargetValueToConsiderTargetALowValueTarget)
                                {
                                    if (IsSentry && !Settings.Instance.KillSentries)
                                    {
                                        _isLowValueTarget = false;
                                        return _isLowValueTarget ?? false;
                                    }

                                    if (TargetValue < 0 && _directEntity.Velocity == 0)
                                    {
                                        _isLowValueTarget = false;
                                        return _isLowValueTarget ?? false;
                                    }

                                    _isLowValueTarget = true;
                                    return _isLowValueTarget ?? true;
                                }

                                _isLowValueTarget = false;
                                return _isLowValueTarget ?? false;
                            }

                            _isLowValueTarget = false;
                            return _isLowValueTarget ?? false;
                        }

                        return _isLowValueTarget ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public DirectContainerWindow CargoWindow
        {
            get
            {
                try
                {
                    return Cache.Instance.Windows.OfType<DirectContainerWindow>().FirstOrDefault(w => w.ItemId == Id);
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return null;
                }
            }
        }

        public bool IsValid
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {

                        if (!HasExploded)
                        {
                            return _directEntity.IsValid;
                        }

                        return false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isContainer;

        public bool IsContainer
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isContainer == null)
                        {
                            bool result = false;
                            result |= (GroupId == (int)Group.Wreck);
                            result |= (GroupId == (int)Group.CargoContainer);
                            result |= (GroupId == (int)Group.SpawnContainer);
                            result |= (GroupId == (int)Group.MissionContainer);
                            _isContainer = result;
                            return _isContainer ?? false;
                        }

                        return _isContainer ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isPlayer;

        public bool IsPlayer
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isPlayer == null)
                        {
                            _isPlayer = _directEntity.IsPc;
                            return _isPlayer ?? false;
                        }

                        return _isPlayer ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsTargetingMeAndNotYetTargeted
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= (((IsNpc || IsNpcByGroupID) || IsAttacking)
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

                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsNotYetTargetingMeAndNotYetTargeted
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= (((IsNpc || IsNpcByGroupID) || IsAttacking || Cache.Instance.InMission)
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

                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsTargetWeCanShootButHaveNotYetTargeted
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= (CategoryId == (int)CategoryID.Entity
                                    && !IsTarget
                                    && !IsTargeting
                                    && Distance < Cache.Instance.MaxTargetRange
                                    && !IsIgnored
                                    && !IsStation);

                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Frigate includes all elite-variants - this does NOT need to be limited to players, as we check for players specifically everywhere this is used
        /// </summary>
        /// 

        public bool IsFrigate
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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

                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Frigate includes all elite-variants - this does NOT need to be limited to players, as we check for players specifically everywhere this is used
        /// </summary>
        public bool IsNPCFrigate
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        if (IsPlayer)
                        {
                            //
                            // if it is a player it is by definition not an NPC
                            //
                            return false;
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
                        result |= GroupId == (int)Group.deadspace_rogue_drone_frigate;
                        result |= GroupId == (int)Group.asteroid_angel_cartel_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_blood_raiders_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_guristas_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_sanshas_nation_commander_frigate;
                        result |= GroupId == (int)Group.asteroid_serpentis_commander_frigate;
                        result |= GroupId == (int)Group.mission_generic_frigates;
                        result |= GroupId == (int)Group.mission_thukker_frigate;
                        result |= GroupId == (int)Group.asteroid_rouge_drone_commander_frigate;
                        result |= GroupId == (int)Group.TutorialDrone;
                        //result |= Name.Contains("Spider Drone"); //we *really* need to find out the GroupID of this one. 
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Cruiser includes all elite-variants
        /// </summary>
        public bool IsCruiser
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Cruiser;
                        result |= GroupId == (int)Group.HeavyAssaultShip;
                        result |= GroupId == (int)Group.Logistics;
                        result |= GroupId == (int)Group.ForceReconShip;
                        result |= GroupId == (int)Group.CombatReconShip;
                        result |= GroupId == (int)Group.HeavyInterdictor;

                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Cruiser includes all elite-variants
        /// </summary>
        public bool IsNPCCruiser
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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
                        result |= GroupId == (int)Group.Mission_Faction_Industrials;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Battlecruiser includes all elite-variants
        /// </summary>
        public bool IsBattlecruiser
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Battlecruiser;
                        result |= GroupId == (int)Group.CommandShip;
                        result |= GroupId == (int)Group.StrategicCruiser; // Technically a cruiser, but hits hard enough to be a BC :)
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Battlecruiser includes all elite-variants
        /// </summary>
        public bool IsNPCBattlecruiser
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Battleship includes all elite-variants
        /// </summary>
        public bool IsBattleship
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Battleship;
                        result |= GroupId == (int)Group.EliteBattleship;
                        result |= GroupId == (int)Group.BlackOps;
                        result |= GroupId == (int)Group.Marauder;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        /// <summary>
        /// Battleship includes all elite-variants
        /// </summary>
        public bool IsNPCBattleship
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isLargeCollidable;

        public bool IsLargeCollidable
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isLargeCollidable == null)
                        {
                            bool result = false;
                            result |= GroupId == (int)Group.LargeColidableObject;
                            result |= GroupId == (int)Group.LargeColidableShip;
                            result |= GroupId == (int)Group.LargeColidableStructure;
                            result |= GroupId == (int)Group.DeadSpaceOverseersStructure;
                            result |= GroupId == (int)Group.DeadSpaceOverseersBelongings;
                            _isLargeCollidable = result;
                            return _isLargeCollidable ?? false;
                        }

                        return _isLargeCollidable ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsMiscJunk
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (Cache.Instance.Entities.All(t => t.Id != Id))
                        {
                            return false;
                        }

                        bool result = false;
                        result |= GroupId == (int)Group.PlayerDrone;
                        result |= GroupId == (int)Group.Wreck;
                        result |= GroupId == (int)Group.AccelerationGate;
                        result |= GroupId == (int)Group.GasCloud;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _IsBadIdea;

        public bool IsBadIdea
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_IsBadIdea == null)
                        {
                            if (Cache.Instance.Entities.All(t => t.Id != Id))
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
                            result |= GroupId == (int)Group.ConcordBillboard;
                            result |= IsFrigate;
                            result |= IsCruiser;
                            result |= IsBattlecruiser;
                            result |= IsBattleship;
                            result |= IsPlayer;
                            _IsBadIdea =  result;
                            return _IsBadIdea ?? false;
                        }

                        return _IsBadIdea ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsFactionWarfareNPC
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.FactionWarfareNPC;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isNpcByGroupID;

        public bool IsNpcByGroupID
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isNpcByGroupID == null)
                        {
                            bool result = false;
                            result |= IsLargeCollidable;
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
                            result |= GroupId == (int)Group.Mission_Faction_Industrials;
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
                            result |= GroupId == (int)Group.TutorialDrone;
                            result |= GroupId == (int)Group.asteroid_angel_cartel_frigate;
                            result |= GroupId == (int)Group.asteroid_blood_raiders_frigate;
                            result |= GroupId == (int)Group.asteroid_guristas_frigate;
                            result |= GroupId == (int)Group.asteroid_sanshas_nation_frigate;
                            result |= GroupId == (int)Group.asteroid_serpentis_frigate;
                            result |= GroupId == (int)Group.deadspace_angel_cartel_frigate;
                            result |= GroupId == (int)Group.deadspace_blood_raiders_frigate;
                            result |= GroupId == (int)Group.deadspace_guristas_frigate;
                            result |= GroupId == (int)Group.Deadspace_Overseer_Frigate;
                            result |= GroupId == (int)Group.Deadspace_Rogue_Drone_Swarm;
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
                            result |= GroupId == (int)Group.deadspace_rogue_drone_frigate;
                            result |= GroupId == (int)Group.asteroid_angel_cartel_commander_frigate;
                            result |= GroupId == (int)Group.asteroid_blood_raiders_commander_frigate;
                            result |= GroupId == (int)Group.asteroid_guristas_commander_frigate;
                            result |= GroupId == (int)Group.asteroid_sanshas_nation_commander_frigate;
                            result |= GroupId == (int)Group.asteroid_serpentis_commander_frigate;
                            result |= GroupId == (int)Group.mission_generic_frigates;
                            result |= GroupId == (int)Group.mission_thukker_frigate;
                            result |= GroupId == (int)Group.asteroid_rouge_drone_commander_frigate;
                            _isNpcByGroupID = result;
                            return _isNpcByGroupID ?? false;
                        }

                        return _isNpcByGroupID ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsEntityIShouldLeaveAlone
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Merchant;            // Merchant, Convoy?
                        result |= GroupId == (int)Group.Mission_Merchant;    // Merchant, Convoy? - Dread Pirate Scarlet
                        result |= IsOreOrIce;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        private bool? _isOnGridWithMe;

        public bool IsOnGridWithMe
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isOnGridWithMe == null)
                        {
                            bool result = false;
                            result |= Distance < (double)Distances.OnGridWithMe;
                            _isOnGridWithMe = result;
                            return result;
                        }

                        return _isOnGridWithMe ?? false;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsStation
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Station;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsCustomsOffice
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.CustomsOffice;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsCelestial
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= CategoryId == (int)CategoryID.Celestial;
                        result |= CategoryId == (int)CategoryID.Station;
                        result |= GroupId == (int)Group.Moon;
                        result |= GroupId == (int)Group.AsteroidBelt;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsAsteroidBelt
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.AsteroidBelt;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsPlanet
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Planet;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsMoon
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Moon;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsAsteroid
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= CategoryId == (int)CategoryID.Asteroid;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsShipWithNoDroneBay
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= TypeId == (int)TypeID.Tengu;
                        result |= GroupId == (int)Group.Shuttle;
                        return result;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool IsOreOrIce
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
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

                    return false;
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return false;
                }
            }
        }

        public bool LockTarget(string module)
        {
            try
            {
                if (_directEntity != null && _directEntity.IsValid)
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
                                        if (Cache.Instance.Entities.Any(i => i.Id == _directEntity.Id))
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

                                            if (_directEntity.LockTarget())
                                            {
                                                Cache.Instance.TargetingIDs[Id] = DateTime.UtcNow;
                                                return true;
                                            }

                                            Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + _directEntity.Name + "][" + Math.Round(_directEntity.Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget failed (unknown reason)", Logging.White);
                                        }

                                        Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + _directEntity.Name + "][" + Math.Round(_directEntity.Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget failed: target was not in Entities List", Logging.White);
                                    }

                                    Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + _directEntity.Name + "][" + Math.Round(_directEntity.Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget aborted: target is already being targeted", Logging.White);
                                }
                                else
                                {
                                    Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + _directEntity.Name + "][" + Math.Round(_directEntity.Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, we only have [" + Cache.Instance.MaxLockedTargets + "] slots!", Logging.White);
                                }
                            }
                            else
                            {
                                Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + _directEntity.Name + "][" + Math.Round(_directEntity.Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, my targeting range is only [" + Cache.Instance.MaxTargetRange + "]!", Logging.White);
                            }
                        }
                        else
                        {
                            Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Cache.Instance.Targets.Count() + "] targets already, target is already dead!", Logging.White);
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
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }

        public bool UnlockTarget(string module)
        {
            try
            {
                if (_directEntity != null && _directEntity.IsValid)
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
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }

        public void Jump()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextJumpAction)
                {
                    if (Cache.Instance.LastInSpace.AddSeconds(2) > DateTime.UtcNow && Cache.Instance.InSpace)
                    {
                        if (_directEntity != null && _directEntity.IsValid)
                        {
                            if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                            {
                                Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                            }

                            Cache.Instance.WehaveMoved = DateTime.UtcNow.AddDays(-7);
                            Cache.Instance.NextJumpAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(8, 12));
                            _directEntity.Jump();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void Activate()
        {
            try
            {
                if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextActivateAction)
                {
                    if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                    {
                        Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                    }
                    _directEntity.Activate();
                    Cache.Instance.LastInWarp = DateTime.UtcNow;
                    Cache.Instance.NextActivateAction = DateTime.UtcNow.AddSeconds(15);
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void Approach()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextApproachAction)
                {
                    Cache.Instance.Approaching = this;

                    if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextApproachAction)
                    {
                        if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                        {
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }
                        Cache.Instance.NextApproachAction = DateTime.UtcNow.AddSeconds(Time.Instance.ApproachDelay_seconds);
                        _directEntity.Approach();
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void Approach(int range)
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextApproachAction)
                {
                    Cache.Instance.Approaching = this;
                    if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextApproachAction)
                    {
                        if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                        {
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }
                        Cache.Instance.NextApproachAction = DateTime.UtcNow.AddSeconds(Time.Instance.ApproachDelay_seconds);
                        _directEntity.KeepAtRange(range);
                        //_directEntity.Approach();
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void Orbit(int range)
        {
            try
            {
                Cache.Instance.Approaching = this;

                if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextOrbit)
                {
                    if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                    {
                        Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                    }
                    Logging.Log("EntityCache", "Initiating Orbit [" + Name + "][at " + Math.Round((double)Cache.Instance.OrbitDistance / 1000, 0) + "k][ID: " + Cache.Instance.MaskedID(Id) + "]", Logging.Teal);
                    Cache.Instance.NextOrbit = DateTime.UtcNow.AddSeconds(10 + Cache.Instance.RandomNumber(1, 15));
                    _directEntity.Orbit(range);
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void WarpTo()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextWarpAction)
                {
                    if (Cache.Instance.LastInSpace.AddSeconds(2) > DateTime.UtcNow && Cache.Instance.InSpace)
                    {
                        if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextWarpTo)
                        {
                            if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                            {
                                Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                            }

                            Cache.Instance.WehaveMoved = DateTime.UtcNow;
                            Cache.Instance.LastInWarp = DateTime.UtcNow;
                            Cache.Instance.NextWarpTo = DateTime.UtcNow.AddSeconds(Time.Instance.WarptoDelay_seconds);
                            _directEntity.WarpTo();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void AlignTo()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextAlign)
                {
                    if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextAlign)
                    {
                        if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                        {
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }

                        Cache.Instance.WehaveMoved = DateTime.UtcNow;
                        Cache.Instance.NextAlign = DateTime.UtcNow.AddMinutes(Time.Instance.AlignDelay_minutes);
                        _directEntity.AlignTo();
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void WarpToAndDock()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextDockAction)
                {
                    if (Cache.Instance.LastInSpace.AddSeconds(2) > DateTime.UtcNow && Cache.Instance.InSpace)
                    {
                        if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextWarpTo && DateTime.UtcNow > Cache.Instance.NextDockAction)
                        {
                            if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                            {
                                Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                            }

                            Cache.Instance.WehaveMoved = DateTime.UtcNow;
                            Cache.Instance.LastInWarp = DateTime.UtcNow;
                            Cache.Instance.NextWarpTo = DateTime.UtcNow.AddSeconds(Time.Instance.WarptoDelay_seconds);
                            Cache.Instance.NextDockAction = DateTime.UtcNow.AddSeconds(Time.Instance.DockingDelay_seconds);
                            _directEntity.WarpToAndDock();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void Dock()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.NextDockAction)
                {
                    if (Cache.Instance.LastInSpace.AddSeconds(2) > DateTime.UtcNow && Cache.Instance.InSpace)
                    {
                        if (_directEntity != null && _directEntity.IsValid && DateTime.UtcNow > Cache.Instance.NextDockAction)
                        {
                            if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                            {
                                Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(_directEntity.Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                            }

                            Cache.Instance.WehaveMoved = DateTime.UtcNow;
                            Cache.Instance.NextDockAction = DateTime.UtcNow.AddSeconds(Time.Instance.DockingDelay_seconds);
                            _directEntity.Dock();

                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void OpenCargo()
        {
            try
            {
                if (_directEntity != null && _directEntity.IsValid)
                {
                    _directEntity.OpenCargo();
                    Cache.Instance.NextOpenCargoAction = DateTime.UtcNow.AddSeconds(2 + Cache.Instance.RandomNumber(1, 3));
                }
            }
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        public void MakeActiveTarget()
        {
            try
            {
                if (_directEntity != null && _directEntity.IsValid)
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
            catch (Exception exception)
            {
                Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
            }
        }
    }
}