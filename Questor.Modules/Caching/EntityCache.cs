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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using DirectEve;
    //using System.Collections.Generic;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;

    public class EntityCache
    {
        private readonly DirectEntity _directEntity;
        //public static int EntityCacheInstances = 0;
        private DateTime ThisEntityCacheCreated = DateTime.UtcNow;
        private int DictionaryCountThreshhold = 250;
        public EntityCache(DirectEntity entity)
        {
            _directEntity = entity;
            //Interlocked.Increment(ref EntityCacheInstances);
            ThisEntityCacheCreated = DateTime.UtcNow;
        }

        ~EntityCache()
        {
            //Interlocked.Decrement(ref EntityCacheInstances);
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
                                if (Settings.Instance.DebugEntityCache) Logging.Log("EntityCache.BookmarkThis", "We already have a bookmark for [" + Name + "] and do not need another.", Logging.Debug);
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
                            if (Cache.Instance.EntityGroupID.Any() && Cache.Instance.EntityGroupID.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.GroupID", "We have [" + Cache.Instance.EntityGroupID.Count() + "] Entities in Cache.Instance.EntityGroupID", Logging.Debug);
                            }

                            if (Cache.Instance.EntityGroupID.Any())
                            {
                                int value = 0;
                                if (Cache.Instance.EntityGroupID.TryGetValue(Id, out value))
                                {
                                    _groupID = value;
                                    return _groupID ?? 0;
                                }    
                            }

                            _groupID = _directEntity.GroupId;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.GroupID", "Adding [" + Name + "] to EntityGroupID as [" + _groupID + "]", Logging.Debug);
                            
                            Cache.Instance.EntityGroupID.Add(Id, (int)_groupID);
                            return _groupID ?? _directEntity.GroupId;
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

        public string MaskedId
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        return Cache.Instance.MaskedID(Id);
                    }

                    return "!0!";
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return "!0!";
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
                            if (Cache.Instance.EntityTypeID.Any() && Cache.Instance.EntityTypeID.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.TypeID", "We have [" + Cache.Instance.EntityTypeID.Count() + "] Entities in Cache.Instance.EntityTypeID", Logging.Debug);
                            }

                            if (Cache.Instance.EntityTypeID.Any())
                            {
                                int value = 0;
                                if (Cache.Instance.EntityTypeID.TryGetValue(Id, out value))
                                {
                                    _TypeId = value;
                                    return _TypeId ?? 0;
                                }    
                            }

                            _TypeId = _directEntity.TypeId;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.TypeId", "Adding [" + Name + "] to EntityTypeId as [" + _TypeId + "]", Logging.Debug);
                            Cache.Instance.EntityTypeID.Add(Id, (int)_TypeId);
                            return _TypeId ?? _directEntity.TypeId;
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
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + _directEntity.Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }

                        if (_name == null)
                        {
                            if (Cache.Instance.EntityNames.Any() && Cache.Instance.EntityNames.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.Name", "We have [" + Cache.Instance.EntityNames.Count() + "] Entities in Cache.Instance.EntityNames", Logging.Debug);
                            }

                            if (Cache.Instance.EntityNames.Any())
                            {
                                string value = null;
                                if (Cache.Instance.EntityNames.TryGetValue(Id, out value))
                                {
                                    _name = value;
                                    return _name;
                                }    
                            }

                            _name = _directEntity.Name;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.Name", "Adding [" + MaskedId + "] to EntityName as [" + _name + "]", Logging.Debug);
                            Cache.Instance.EntityNames.Add(Id, _name);
                            return _name ?? string.Empty;
                        }

                        return _name;
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
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(_directEntity.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
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
                            if (Distance > 0 && Distance < 900000000)
                            {
                                //_nearest5kDistance = Math.Round((Distance / 1000) * 2, MidpointRounding.AwayFromZero) / 2;
                                _nearest5kDistance = (double)Math.Ceiling(Math.Round((Distance / 1000)) / 5.0) * 5;
                            }
                        }

                        return _nearest5kDistance ?? Distance;
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
                            return _structurePct ?? _directEntity.StructurePct;
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
                            return _isNpc ?? _directEntity.IsNpc;
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
                            return _velocity ?? _directEntity.Velocity;
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
                            _isTarget = _directEntity.IsTarget;
                            return _isTarget ?? false;
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
                            if (Cache.Instance.PreferredPrimaryWeaponTarget != null && Cache.Instance.PreferredPrimaryWeaponTarget.Id == Id)
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
                            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(e => e.Entity.Id == Id))
                            {
                                _isPrimaryWeaponKillPriority = true;
                                return _isPrimaryWeaponKillPriority ?? false;
                            }

                            _isPrimaryWeaponKillPriority = false;
                            return _isPrimaryWeaponKillPriority ?? false;
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
                                return _IsDroneKillPriority ?? false;
                            }

                            _IsDroneKillPriority = false;
                            return _IsDroneKillPriority ?? false;
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
                                    if (Distance < Settings.Instance.DistanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons
                                     && Velocity > Settings.Instance.SpeedNPCFrigatesShouldBeIgnoredByPrimaryWeapons)
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
                                //if (_directEntity.Distance < Cache.Instance.MaxRange)
                                //{
                                    //if (Cache.Instance.Entities.Any(t => t.Id == Id))
                                    //{
                                        _IsReadyToShoot = true;
                                        return _IsReadyToShoot ?? true;
                                    //}

                                    //_IsReadyToShoot = false;
                                    //return _IsReadyToShoot ?? false;
                                //}

                                //_IsReadyToShoot = false;
                                //return _IsReadyToShoot ?? false;
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

                            _IsReadyToTarget = false;
                            return _IsReadyToTarget ?? false;
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
                                    if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.EntityID == Id))
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

                                        if (!Cache.Instance.DronePriorityEntities.All(pt => pt.DronePriorityLevel < _currentEntityDronePriority && pt.Distance < Cache.Instance.MaxDroneRange))
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    if (Cache.Instance.DronePriorityEntities.Any(e => e.Distance < Cache.Instance.MaxDroneRange))
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

        private bool? _isCurrentTarget;

        public bool IsCurrentTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isCurrentTarget == null)
                        {
                            if (Cache.Instance.CurrentWeaponTarget() != null)
                            {
                                _isCurrentTarget = true;
                                return _isCurrentTarget ?? true;
                            }

                            _isCurrentTarget = false;
                            return _isCurrentTarget ?? false;
                        }

                        return _isCurrentTarget ?? false;
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

        private bool? _isCurrentDroneTarget;

        public bool IsCurrentDroneTarget
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isCurrentDroneTarget == null)
                        {
                            if (Id == Cache.Instance.LastDroneTargetID)
                            {
                                _isCurrentDroneTarget = true;
                                return _isCurrentDroneTarget ?? true;
                            }

                            _isCurrentDroneTarget = false;
                            return _isCurrentDroneTarget ?? false;
                        }

                        return _isCurrentDroneTarget ?? false;
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
                                else if (Settings.Instance.OptimalRange != 0) //do we really need this condition? we cant even get in here if one of them is not != 0, that is the idea, if its 0 we sure as hell do not want to use it as the optimal
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

                                    _isInOptimalRange = false;
                                    return _isInOptimalRange ?? false;
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
                        //if it is in optimal, return true, we want to shoot things that are in optimal!
                        if (IsInOptimalRange)
                        {
                            return true;
                        }

                        //Any targets which are not the current target and is not a wreck or container
                        if (!Cache.Instance.Targets.Any(i => i.Id != Id && !i.IsContainer)) 
                        {
                            return true;
                        }

                        //something else must be available to shoot, and this entity is not in optimal, return false;
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
                        if (Cache.Instance.MaxDroneRange > 0) //&& Cache.Instance.UseDrones)
                        {
                            if (Distance < Cache.Instance.MaxDroneRange)
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

                        if (Cache.Instance.DronePriorityTargets.All(i => i.EntityID != Id))
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
                            if (PrimaryWeaponPriorityLevel == PrimaryWeaponPriority.WarpScrambler)
                            {
                                return true;
                            }

                            //return false; //check for drone priority targets too!
                        }

                        if (Cache.Instance.DronePriorityTargets.Any(pt => pt.EntityID == Id))
                        {
                            if (DronePriorityLevel == DronePriority.WarpScrambler)
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

        private PrimaryWeaponPriority? _primaryWeaponPriorityLevel;
        public PrimaryWeaponPriority PrimaryWeaponPriorityLevel
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_primaryWeaponPriorityLevel == null)
                        {
                            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any(pt => pt.EntityID == Id))
                            {
                                _primaryWeaponPriorityLevel = Cache.Instance.PrimaryWeaponPriorityTargets.Where(t => t.Entity.IsTarget && t.EntityID == Id)
                                                                                                                            .Select(pt => pt.PrimaryWeaponPriority)
                                                                                                                            .FirstOrDefault();
                                return _primaryWeaponPriorityLevel ?? PrimaryWeaponPriority.NotUsed;
                            }

                            return PrimaryWeaponPriority.NotUsed;
                        }

                        return _primaryWeaponPriorityLevel ?? PrimaryWeaponPriority.NotUsed;
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

        private bool? _isAttacking;
        public bool IsAttacking
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isAttacking == null)
                        {
                            _isAttacking = _directEntity.IsAttacking;
                            return _isAttacking ?? false;
                        }

                        return _isAttacking ?? false;
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
                        if (Cache.Instance.ListOfWarpScramblingEntities.Contains(Id))
                        {
                            return true;
                        }

                        if (_directEntity.Attacks.Contains("effects.WarpScramble"))
                        {
                            if (!Cache.Instance.ListOfWarpScramblingEntities.Contains(Id))
                            {
                                Cache.Instance.ListOfWarpScramblingEntities.Add(Id);
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
                            if (!Cache.Instance.ListofWebbingEntities.Contains(Id)) Cache.Instance.ListofWebbingEntities.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.ListofWebbingEntities.Contains(Id))
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
                            if (!Cache.Instance.ListNeutralizingEntities.Contains(Id)) Cache.Instance.ListNeutralizingEntities.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.ListNeutralizingEntities.Contains(Id))
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
                            if (!Cache.Instance.ListOfJammingEntities.Contains(Id)) Cache.Instance.ListOfJammingEntities.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.ListOfJammingEntities.Contains(Id))
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
                            if (!Cache.Instance.ListOfDampenuingEntities.Contains(Id)) Cache.Instance.ListOfDampenuingEntities.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.ListOfDampenuingEntities.Contains(Id))
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
                            if (!Cache.Instance.ListOfTargetPaintingEntities.Contains(Id)) Cache.Instance.ListOfTargetPaintingEntities.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.ListOfTargetPaintingEntities.Contains(Id))
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
                            if (!Cache.Instance.ListOfTrackingDisruptingEntities.Contains(Id)) Cache.Instance.ListOfTrackingDisruptingEntities.Add(Id);
                            return true;
                        }

                        if (Cache.Instance.ListOfTrackingDisruptingEntities.Contains(Id))
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

        private bool? _isEntityIShouldKeepShootingWithDrones;

        public bool IsEntityIShouldKeepShootingWithDrones
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isEntityIShouldKeepShootingWithDrones == null)
                        {
                            //
                            // Is our current target already in armor? keep shooting the same target if so...
                            //
                            if (IsReadyToShoot
                                && IsInDroneRange 
                                && !IsLargeCollidable
                                && ((IsFrigate || IsNPCFrigate) || Settings.Instance.DronesKillHighValueTargets)
                                && ShieldPct * 100 < 80)
                            {
                                if (Settings.Instance.DebugGetBestTarget) Logging.Log("EntityCache.IsEntityIShouldKeepShootingWithDrones", "[" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + " GroupID [" + GroupId + "]] has less than 60% armor, keep killing this target", Logging.Debug);
                                _isEntityIShouldKeepShootingWithDrones = true;
                                return _isEntityIShouldKeepShootingWithDrones ?? true;
                            }
                        }

                        return _isEntityIShouldKeepShootingWithDrones ?? false;
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
                            if (Cache.Instance.EntityIsSentry.Any() && Cache.Instance.EntityIsSentry.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsSentry", "We have [" + Cache.Instance.EntityIsSentry.Count() + "] Entities in Cache.Instance.EntityIsSentry", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsSentry.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsSentry.TryGetValue(Id, out value))
                                {
                                    _isSentry = value;
                                    return _isSentry ?? false;
                                }
                            }

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
                            Cache.Instance.EntityIsSentry.Add(Id, result);
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
                            //IsIgnoredRefreshes++;
                            //if (Cache.Instance.Entities.All(t => t.Id != _directEntity.Id))
                            //{
                            //    IsIgnoredRefreshes = IsIgnoredRefreshes + 1000;
                            //    _isIgnored = true;
                            //    return _isIgnored ?? true;
                            //}
                            if (Cache.Instance.IgnoreTargets.Any())
                            {
                                _isIgnored = Cache.Instance.IgnoreTargets.Contains(Name.Trim());
                                if ((bool)_isIgnored)
                                {
                                    if (Cache.Instance.PreferredPrimaryWeaponTarget != null && Cache.Instance.PreferredPrimaryWeaponTarget.Id != Id)
                                    {
                                        Cache.Instance.PreferredPrimaryWeaponTarget = null;
                                    }
                                    
                                    if (Cache.Instance.EntityIsLowValueTarget.ContainsKey(Id))
                                    {
                                        Cache.Instance.EntityIsLowValueTarget.Remove(Id);
                                    }

                                    if (Cache.Instance.EntityIsHighValueTarget.ContainsKey(Id))
                                    {
                                        Cache.Instance.EntityIsHighValueTarget.Remove(Id);
                                    }

                                    if (Settings.Instance.DebugEntityCache) Logging.Log("EntityCache.IsIgnored", "[" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] isIgnored [" + _isIgnored + "]", Logging.Debug);
                                    return _isIgnored ?? true;
                                }

                                _isIgnored = false;
                                if (Settings.Instance.DebugEntityCache) Logging.Log("EntityCache.IsIgnored", "[" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] isIgnored [" + _isIgnored + "]", Logging.Debug);
                                return _isIgnored ?? false;
                            }

                            _isIgnored = false;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("EntityCache.IsIgnored", "[" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] isIgnored [" + _isIgnored + "]", Logging.Debug);
                            return _isIgnored ?? false;
                        }

                        if (Settings.Instance.DebugEntityCache) Logging.Log("EntityCache.IsIgnored", "[" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] isIgnored [" + _isIgnored + "]", Logging.Debug);
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
                            if (Cache.Instance.EntityIsHighValueTarget.Any() && Cache.Instance.EntityIsHighValueTarget.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsHighValueTarget", "We have [" + Cache.Instance.EntityIsHighValueTarget.Count() + "] Entities in Cache.Instance.EntityIsHighValueTarget", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsHighValueTarget.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsHighValueTarget.TryGetValue(Id, out value))
                                {
                                    _isHighValueTarget = value;
                                    return _isHighValueTarget ?? false;
                                }    
                            }

                            if (TargetValue != null)
                            {
                                if (!IsIgnored || !IsContainer || !IsBadIdea || !IsCustomsOffice || !IsFactionWarfareNPC || !IsPlayer)
                                {
                                    if (TargetValue >= Settings.Instance.MinimumTargetValueToConsiderTargetAHighValueTarget)
                                    {
                                        if (IsSentry && !Settings.Instance.KillSentries && !IsEwarTarget)
                                        {
                                            _isHighValueTarget = false;
                                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsHighValueTarget", "Adding [" + Name + "] to EntityIsHighValueTarget as [" + _isHighValueTarget + "]", Logging.Debug);
                                            Cache.Instance.EntityIsHighValueTarget.Add(Id, (bool)_isHighValueTarget);
                                            return _isHighValueTarget ?? false;
                                        }

                                        _isHighValueTarget = true;
                                        if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsHighValueTarget", "Adding [" + Name + "] to EntityIsHighValueTarget as [" + _isHighValueTarget + "]", Logging.Debug);
                                        Cache.Instance.EntityIsHighValueTarget.Add(Id, (bool)_isHighValueTarget);
                                        return _isHighValueTarget ?? true;
                                    }

                                    //if (IsLargeCollidable)
                                    //{
                                    //    return true;
                                    //}    
                                }

                                _isHighValueTarget = false;
                                //do not cache things that may be ignored temporarily...
                                return _isHighValueTarget ?? false;
                            }

                            _isHighValueTarget = false;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsHighValueTarget", "Adding [" + Name + "] to EntityIsHighValueTarget as [" + _isHighValueTarget + "]", Logging.Debug);
                            Cache.Instance.EntityIsHighValueTarget.Add(Id, (bool)_isHighValueTarget);
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
                            if (Cache.Instance.EntityIsLowValueTarget.Any() && Cache.Instance.EntityIsLowValueTarget.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLowValueTarget", "We have [" + Cache.Instance.EntityIsLowValueTarget.Count() + "] Entities in Cache.Instance.EntityIsLowValueTarget", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsLowValueTarget.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsLowValueTarget.TryGetValue(Id, out value))
                                {
                                    _isLowValueTarget = value;
                                    return _isLowValueTarget ?? false;
                                }   
                            }

                            if (!IsIgnored || !IsContainer || !IsBadIdea || !IsCustomsOffice || !IsFactionWarfareNPC || !IsPlayer)
                            {
                                if (TargetValue != null && TargetValue <= Settings.Instance.MaximumTargetValueToConsiderTargetALowValueTarget)
                                {
                                    if (IsSentry && !Settings.Instance.KillSentries && !IsEwarTarget)
                                    {
                                        _isLowValueTarget = false;
                                        if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLowValueTarget", "Adding [" + Name + "] to EntityIsLowValueTarget as [" + _isLowValueTarget + "]", Logging.Debug);
                                        Cache.Instance.EntityIsLowValueTarget.Add(Id, (bool)_isLowValueTarget);
                                        return _isLowValueTarget ?? false;
                                    }

                                    if (TargetValue < 0 && Velocity == 0)
                                    {
                                        _isLowValueTarget = false;
                                        if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLowValueTarget", "Adding [" + Name + "] to EntityIsLowValueTarget as [" + _isLowValueTarget + "]", Logging.Debug);
                                        Cache.Instance.EntityIsLowValueTarget.Add(Id, (bool)_isLowValueTarget);
                                        return _isLowValueTarget ?? false;
                                    }

                                    _isLowValueTarget = true;
                                    if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLowValueTarget", "Adding [" + Name + "] to EntityIsLowValueTarget as [" + _isLowValueTarget + "]", Logging.Debug);
                                    Cache.Instance.EntityIsLowValueTarget.Add(Id, (bool)_isLowValueTarget);
                                    return _isLowValueTarget ?? true;
                                }

                                _isLowValueTarget = false;
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLowValueTarget", "Adding [" + Name + "] to EntityIsLowValueTarget as [" + _isLowValueTarget + "]", Logging.Debug);
                                Cache.Instance.EntityIsLowValueTarget.Add(Id, (bool)_isLowValueTarget);
                                return _isLowValueTarget ?? false;
                            }

                            _isLowValueTarget = false;
                            //do not cache things that may be ignored temporarily
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
                    if (!Cache.Instance.Windows.Any())
                    {
                        return null;
                    }

                    return Cache.Instance.Windows.OfType<DirectContainerWindow>().FirstOrDefault(w => w.ItemId == Id);
                }
                catch (Exception exception)
                {
                    Logging.Log("EntityCache", "Exception [" + exception + "]", Logging.Debug);
                    return null;
                }
            }
        }

        private bool? _isValid;
        public bool IsValid
        {
            get
            {
                try
                {
                    if (_directEntity != null)
                    {
                        if (_isValid == null)
                        {
                            _isValid = _directEntity.IsValid;
                            return _isValid ?? true;
                        }

                        return _isValid ?? true;
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
                            result |= (GroupId == (int)Group.DeadSpaceOverseersBelongings);
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
                                    && CategoryId == (int)CategoryID.Entity
                                    && Distance < Cache.Instance.MaxTargetRange
                                    && !IsLargeCollidable
                                    && (!IsTargeting && !IsTarget && IsTargetedBy)
                                    && !IsContainer
                                    && !IsIgnored
                                    && (!IsBadIdea || IsAttacking)
                                    && !IsEntityIShouldLeaveAlone
                                    && !IsFactionWarfareNPC
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

        private bool? _isFrigate;
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
                        if (_isFrigate == null)
                        {
                            if (Cache.Instance.EntityIsFrigate.Any() && Cache.Instance.EntityIsFrigate.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsFrigate", "We have [" + Cache.Instance.EntityIsFrigate.Count() + "] Entities in Cache.Instance.EntityIsFrigate", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsFrigate.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsFrigate.TryGetValue(Id, out value))
                                {
                                    _isFrigate = value;
                                    return _isFrigate ?? false;
                                }    
                            }

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

                            _isFrigate = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsFrigate", "Adding [" + Name + "] to EntityIsFrigate as [" + _isFrigate + "]", Logging.Debug);
                            Cache.Instance.EntityIsFrigate.Add(Id, (bool)_isFrigate);
                            return _isFrigate ?? false;
                        }

                        return _isFrigate ?? false;
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

        private bool? _isNPCFrigate;
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
                        if (_isNPCFrigate == null)
                        {
                            if (Cache.Instance.EntityIsNPCFrigate.Any() && Cache.Instance.EntityIsNPCFrigate.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCFrigate", "We have [" + Cache.Instance.EntityIsNPCFrigate.Count() + "] Entities in Cache.Instance.EntityIsNPCFrigate", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsNPCFrigate.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsNPCFrigate.TryGetValue(Id, out value))
                                {
                                    _isNPCFrigate = value;
                                    return _isNPCFrigate ?? false;
                                }    
                            }
                            
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
                            _isNPCFrigate = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCFrigate", "Adding [" + Name + "] to EntityIsNPCFrigate as [" + _isNPCFrigate + "]", Logging.Debug);
                            Cache.Instance.EntityIsNPCFrigate.Add(Id, (bool)_isNPCFrigate);
                            return _isNPCFrigate ?? false;
                        }

                        return _isNPCFrigate ?? false;
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

        private bool? _isCruiser;
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
                        if (_isCruiser == null)
                        {
                            if (Cache.Instance.EntityIsCruiser.Any() && Cache.Instance.EntityIsCruiser.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsCruiser", "We have [" + Cache.Instance.EntityIsCruiser.Count() + "] Entities in Cache.Instance.EntityIsCruiser", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsCruiser.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsCruiser.TryGetValue(Id, out value))
                                {
                                    _isCruiser = value;
                                    return _isCruiser ?? false;
                                }   
                            }

                            bool result = false;
                            result |= GroupId == (int)Group.Cruiser;
                            result |= GroupId == (int)Group.HeavyAssaultShip;
                            result |= GroupId == (int)Group.Logistics;
                            result |= GroupId == (int)Group.ForceReconShip;
                            result |= GroupId == (int)Group.CombatReconShip;
                            result |= GroupId == (int)Group.HeavyInterdictor;

                            _isCruiser = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsCruiser", "Adding [" + Name + "] to EntityIsCruiser as [" + _isCruiser + "]", Logging.Debug);
                            Cache.Instance.EntityIsCruiser.Add(Id, (bool)_isCruiser);
                            return _isCruiser ?? false;
                        }

                        return _isCruiser ?? false;
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

        private bool? _isNPCCruiser;
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
                        if (_isNPCCruiser == null)
                        {
                            if (Cache.Instance.EntityIsNPCCruiser.Any() && Cache.Instance.EntityIsNPCCruiser.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCCruiser", "We have [" + Cache.Instance.EntityIsNPCCruiser.Count() + "] Entities in Cache.Instance.EntityIsNPCCruiser", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsNPCCruiser.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsNPCCruiser.TryGetValue(Id, out value))
                                {
                                    _isNPCCruiser = value;
                                    return _isNPCCruiser ?? false;
                                }    
                            }

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
                            _isNPCCruiser = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCCruiser", "Adding [" + Name + "] to EntityIsNPCCruiser as [" + _isNPCCruiser + "]", Logging.Debug);
                            Cache.Instance.EntityIsNPCCruiser.Add(Id, (bool)_isNPCCruiser);
                            return _isNPCCruiser ?? false;
                        }

                        return _isNPCCruiser ?? false;
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

        private bool? _isBattleCruiser;
        /// <summary>
        /// BattleCruiser includes all elite-variants
        /// </summary>
        public bool IsBattlecruiser
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isBattleCruiser == null)
                        {
                            if (Cache.Instance.EntityIsBattleCruiser.Any() && Cache.Instance.EntityIsBattleCruiser.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsBattleCruiser", "We have [" + Cache.Instance.EntityIsBattleCruiser.Count() + "] Entities in Cache.Instance.EntityIsBattleCruiser", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsBattleCruiser.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsBattleCruiser.TryGetValue(Id, out value))
                                {
                                    _isBattleCruiser = value;
                                    return _isBattleCruiser ?? false;
                                }   
                            }

                            bool result = false;
                            result |= GroupId == (int)Group.Battlecruiser;
                            result |= GroupId == (int)Group.CommandShip;
                            result |= GroupId == (int)Group.StrategicCruiser; // Technically a cruiser, but hits hard enough to be a BC :)
                            _isBattleCruiser = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsBattleCruiser", "Adding [" + Name + "] to EntityIsBattleCruiser as [" + _isBattleCruiser + "]", Logging.Debug);
                            Cache.Instance.EntityIsBattleCruiser.Add(Id, (bool)_isBattleCruiser);
                            return _isBattleCruiser ?? false;
                        }

                        return _isBattleCruiser ?? false;
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

        private bool? _isNPCBattleCruiser;
        /// <summary>
        /// BattleCruiser includes all elite-variants
        /// </summary>
        public bool IsNPCBattlecruiser
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isNPCBattleCruiser == null)
                        {
                            if (Cache.Instance.EntityIsNPCBattleCruiser.Any() && Cache.Instance.EntityIsNPCBattleCruiser.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCBattleCruiser", "We have [" + Cache.Instance.EntityIsNPCBattleCruiser.Count() + "] Entities in Cache.Instance.EntityIsNPCBattleCruiser", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsNPCBattleCruiser.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsNPCBattleCruiser.TryGetValue(Id, out value))
                                {
                                    _isNPCBattleCruiser = value;
                                    return _isNPCBattleCruiser ?? false;
                                }   
                            }

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
                            _isNPCBattleCruiser = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCBattleCruiser", "Adding [" + Name + "] to EntityIsNPCBattleCruiser as [" + _isNPCBattleCruiser + "]", Logging.Debug);
                            Cache.Instance.EntityIsNPCBattleCruiser.Add(Id, (bool)_isNPCBattleCruiser);
                            return _isNPCBattleCruiser ?? false;
                        }

                        return _isNPCBattleCruiser ?? false;
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


        private bool? _isBattleship;
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
                        if (_isBattleship == null)
                        {
                            if (Cache.Instance.EntityIsBattleShip.Any() && Cache.Instance.EntityIsBattleShip.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsBattleShip", "We have [" + Cache.Instance.EntityIsBattleShip.Count() + "] Entities in Cache.Instance.EntityIsBattleShip", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsBattleShip.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsBattleShip.TryGetValue(Id, out value))
                                {
                                    _isBattleship = value;
                                    return _isBattleship ?? false;
                                }   
                            }

                            bool result = false;
                            result |= GroupId == (int)Group.Battleship;
                            result |= GroupId == (int)Group.EliteBattleship;
                            result |= GroupId == (int)Group.BlackOps;
                            result |= GroupId == (int)Group.Marauder;
                            _isBattleship = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsBattleShip", "Adding [" + Name + "] to EntityIsBattleShip as [" + _isBattleship + "]", Logging.Debug);
                            Cache.Instance.EntityIsBattleShip.Add(Id, (bool)_isBattleship);
                            return _isBattleship ?? false;
                        }

                        return _isBattleship ?? false;
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

        private bool? _isNPCBattleship;
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
                        if (_isNPCBattleship == null)
                        {
                            if (Cache.Instance.EntityIsNPCBattleShip.Any() && Cache.Instance.EntityIsNPCBattleShip.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCBattleShip", "We have [" + Cache.Instance.EntityIsNPCBattleShip.Count() + "] Entities in Cache.Instance.EntityIsNPCBattleShip", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsNPCBattleShip.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsNPCBattleShip.TryGetValue(Id, out value))
                                {
                                    _isNPCBattleship = value;
                                    return _isNPCBattleship ?? false;
                                }   
                            }

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
                            _isNPCBattleship = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCBattleShip", "Adding [" + Name + "] to EntityIsNPCBattleShip as [" + _isNPCBattleship + "]", Logging.Debug);
                            Cache.Instance.EntityIsNPCBattleShip.Add(Id, (bool)_isNPCBattleship);
                            return _isNPCBattleship ?? false;
                        }

                        return _isNPCBattleship ?? false;
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
                            if (Cache.Instance.EntityIsLargeCollidable.Any() && Cache.Instance.EntityIsLargeCollidable.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLargeCollidable", "We have [" + Cache.Instance.EntityIsLargeCollidable.Count() + "] Entities in Cache.Instance.EntityIsLargeCollidable", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsLargeCollidable.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsLargeCollidable.TryGetValue(Id, out value))
                                {
                                    _isLargeCollidable = value;
                                    return _isLargeCollidable ?? false;
                                }   
                            }

                            bool result = false;
                            result |= GroupId == (int)Group.LargeColidableObject;
                            result |= GroupId == (int)Group.LargeColidableShip;
                            result |= GroupId == (int)Group.LargeColidableStructure;
                            result |= GroupId == (int)Group.DeadSpaceOverseersStructure;
                            result |= GroupId == (int)Group.DeadSpaceOverseersBelongings;
                            _isLargeCollidable = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsLargeCollidableObject", "Adding [" + Name + "] to EntityIsLargeCollidableObject as [" + _isLargeCollidable + "]", Logging.Debug);
                            Cache.Instance.EntityIsLargeCollidable.Add(Id, (bool)_isLargeCollidable);
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

        private bool? _isMiscJunk;
        public bool IsMiscJunk
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isMiscJunk == null)
                        {
                            if (Cache.Instance.EntityIsMiscJunk.Any() && Cache.Instance.EntityIsMiscJunk.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsMiscJunk", "We have [" + Cache.Instance.EntityIsMiscJunk.Count() + "] Entities in Cache.Instance.EntityIsMiscJunk", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsMiscJunk.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsMiscJunk.TryGetValue(Id, out value))
                                {
                                    _isMiscJunk = value;
                                    return _isMiscJunk ?? false;
                                }
                            }

                            bool result = false;
                            result |= GroupId == (int)Group.PlayerDrone;
                            result |= GroupId == (int)Group.Wreck;
                            result |= GroupId == (int)Group.AccelerationGate;
                            result |= GroupId == (int)Group.GasCloud;
                            _isMiscJunk = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsMiscJunk", "Adding [" + Name + "] to EntityIsMiscJunk as [" + _isMiscJunk + "]", Logging.Debug);
                            Cache.Instance.EntityIsMiscJunk.Add(Id, (bool)_isMiscJunk);
                            return _isMiscJunk ?? false;
                        }

                        return _isMiscJunk ?? false;
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
                            if (Cache.Instance.EntityIsBadIdea.Any() && Cache.Instance.EntityIsBadIdea.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsBadIdea", "We have [" + Cache.Instance.EntityIsBadIdea.Count() + "] Entities in Cache.Instance.EntityIsBadIdea", Logging.Debug);
                            }
                            
                            if (Cache.Instance.EntityIsBadIdea.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsBadIdea.TryGetValue(Id, out value))
                                {
                                    _IsBadIdea = value;
                                    return _IsBadIdea ?? false;
                                }    
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
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsBadIdea", "Adding [" + Name + "] to EntityIsBadIdea as [" + _IsBadIdea + "]", Logging.Debug);
                            Cache.Instance.EntityIsBadIdea.Add(Id, (bool)_IsBadIdea);
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
                            if (Cache.Instance.EntityIsNPCByGroupID.Any() && Cache.Instance.EntityIsNPCByGroupID.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCByGroupID", "We have [" + Cache.Instance.EntityIsNPCByGroupID.Count() + "] Entities in Cache.Instance.EntityIsNPCByGroupID", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsNPCByGroupID.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsNPCByGroupID.TryGetValue(Id, out value))
                                {
                                    _isNpcByGroupID = value;
                                    return _isNpcByGroupID ?? false;
                                }   
                            }

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
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsNPCByGroupID", "Adding [" + Name + "] to EntityIsNPCByGroupID as [" + _isNpcByGroupID + "]", Logging.Debug);
                            Cache.Instance.EntityIsNPCByGroupID.Add(Id, (bool)_isNpcByGroupID);
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

        private bool? _isEntityIShouldLeaveAlone;
        public bool IsEntityIShouldLeaveAlone
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        if (_isEntityIShouldLeaveAlone == null)
                        {
                            if (Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Any() && Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Count() > DictionaryCountThreshhold)
                            {
                                if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsEntutyIShouldLeaveAlone", "We have [" + Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Count() + "] Entities in Cache.Instance.EntityIsEntutyIShouldLeaveAlone", Logging.Debug);
                            }

                            if (Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Any())
                            {
                                bool value = false;
                                if (Cache.Instance.EntityIsEntutyIShouldLeaveAlone.TryGetValue(Id, out value))
                                {
                                    _isEntityIShouldLeaveAlone = value;
                                    return _isEntityIShouldLeaveAlone ?? false;
                                }   
                            }

                            bool result = false;
                            result |= GroupId == (int)Group.Merchant;            // Merchant, Convoy?
                            result |= GroupId == (int)Group.Mission_Merchant;    // Merchant, Convoy? - Dread Pirate Scarlet
                            result |= GroupId == (int)Group.FactionWarfareNPC;
                            result |= IsOreOrIce;
                            _isEntityIShouldLeaveAlone = result;
                            if (Settings.Instance.DebugEntityCache) Logging.Log("Entitycache.IsEntutyIShouldLeaveAlone", "Adding [" + Name + "] to EntityIsEntutyIShouldLeaveAlone as [" + _isEntityIShouldLeaveAlone + "]", Logging.Debug);
                            Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Add(Id, (bool)_isEntityIShouldLeaveAlone);
                            return _isEntityIShouldLeaveAlone ?? false;
                        }

                        return _isEntityIShouldLeaveAlone ?? false;
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
                            if (Distance < (double) Distances.OnGridWithMe)
                            {
                                _isOnGridWithMe = true;
                                return _isOnGridWithMe ?? true;
                            }

                            _isOnGridWithMe = false;
                            return _isOnGridWithMe ?? false;
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

        public bool IsShipWithOreHold
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= TypeId == (int)TypeID.Venture;
                        result |= GroupId == (int)Group.MiningBarge;
                        result |= GroupId == (int)Group.Exhumer;
                        result |= GroupId == (int)Group.IndustrialCommandShip; // Orca
                        result |= GroupId == (int)Group.CapitalIndustrialShip; // Rorqual
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

        public bool IsShipWithNoCargoBay
        {
            get
            {
                try
                {
                    if (_directEntity != null && _directEntity.IsValid)
                    {
                        bool result = false;
                        result |= GroupId == (int)Group.Capsule;
                        //result |= GroupId == (int)Group.Shuttle;
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
                if (DateTime.UtcNow < Cache.Instance.NextTargetAction)
                {
                    return false;
                }

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
                                        if (Cache.Instance.EntitiesOnGrid.Any(i => i.Id == Id))
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
                                            foreach (EntityCache target in Cache.Instance.EntitiesOnGrid.Where(e => e.IsTarget && Cache.Instance.TargetingIDs.ContainsKey(e.Id)))
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
                                                    Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, can reTarget in [" + Math.Round(20 - seconds, 0) + "]", Logging.White);
                                                    return false;
                                                }
                                            }
                                            // Only add targeting id's when its actually being targeted

                                            if (_directEntity.LockTarget())
                                            {
                                                //Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                                                Cache.Instance.TargetingIDs[Id] = DateTime.UtcNow;
                                                return true;
                                            }

                                            Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget failed (unknown reason)", Logging.White);
                                            return false; 
                                        }

                                        Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget failed: target was not in Entities List", Logging.White);
                                        return false;
                                    }

                                    Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, LockTarget aborted: target is already being targeted", Logging.White);
                                    return false;
                                }
                                
                                Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, we only have [" + Cache.Instance.MaxLockedTargets + "] slots!", Logging.White);
                                return false;
                            }
                            
                            Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, my targeting range is only [" + Cache.Instance.MaxTargetRange + "]!", Logging.White);
                            return false;
                        }
                        
                        Logging.Log("EntityCache.LockTarget", "[" + module + "] tried to lock [" + Name + "][" + Cache.Instance.Targets.Count() + "] targets already, target is already dead!", Logging.White);
                        return false;
                    }
                    
                    Logging.Log("EntityCache.LockTarget", "[" + module + "] LockTarget request has been ignored for [" + Name + "][" + Math.Round(Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(Id) + "][" + Cache.Instance.Targets.Count() + "] targets already, target is already locked!", Logging.White);
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
                                Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
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
                        Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                    }

                    //we cant move in bastion mode, do not try
                    List<ModuleCache> bastionModules = null;
                    bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                    if (bastionModules.Any(i => i.IsActive))
                    {
                        Logging.Log("EntityCache.Activate", "BastionMode is active, we cannot move, aborting attempt to Activate Gate", Logging.Debug);
                        return;
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
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }

                        //we cant move in bastion mode, do not try
                        List<ModuleCache> bastionModules = null;
                        bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                        if (bastionModules.Any(i => i.IsActive))
                        {
                            Logging.Log("EntityCache.Approach", "BastionMode is active, we cannot move, aborting attempt to Approach", Logging.Debug);
                            return;
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
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                        }

                        //we cant move in bastion mode, do not try
                        List<ModuleCache> bastionModules = null;
                        bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                        if (bastionModules.Any(i => i.IsActive))
                        {
                            Logging.Log("EntityCache.Approach", "BastionMode is active, we cannot move, aborting attempt to Approach", Logging.Debug);
                            return;
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
                        Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + MaskedId+ "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                    }

                    //we cant move in bastion mode, do not try
                    List<ModuleCache> bastionModules = null;
                    bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                    if (bastionModules.Any(i => i.IsActive))
                    {
                        Logging.Log("EntityCache.Orbit", "BastionMode is active, we cannot move, aborting attempt to Orbit", Logging.Debug);
                        return;
                    }

                    Logging.Log("EntityCache", "Initiating Orbit [" + Name + "][at " + Math.Round((double)Cache.Instance.OrbitDistance / 1000, 0) + "k][" + MaskedId + "]", Logging.Teal);
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
                                Logging.Log("EntityCache.WarpTo", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
                            }

                            //we cant move in bastion mode, do not try
                            List<ModuleCache> bastionModules = null;
                            bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                            if (bastionModules.Any(i => i.IsActive))
                            {
                                Logging.Log("EntityCache.WarpTo", "BastionMode is active, we cannot warp, aborting attempt to warp", Logging.Debug);
                                return;
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
                            Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
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
                if (DateTime.UtcNow > Cache.Instance.NextDockAction && DateTime.UtcNow > Cache.Instance.NextWarpTo)
                {
                    if (Cache.Instance.LastInSpace.AddSeconds(2) > DateTime.UtcNow && Cache.Instance.InSpace && DateTime.UtcNow > Cache.Instance.LastInStation.AddSeconds(20))
                    {
                        if (_directEntity != null && _directEntity.IsValid)
                        {
                            if (DateTime.UtcNow.AddSeconds(-5) > ThisEntityCacheCreated)
                            {
                                Logging.Log("EntityCache.Name", "The EntityCache instance that represents [" + Name + "][" + Math.Round(Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Id) + "] was created more than 5 seconds ago (ugh!)", Logging.Debug);
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
                    if (Cache.Instance.LastInSpace.AddSeconds(2) > DateTime.UtcNow && Cache.Instance.InSpace && DateTime.UtcNow > Cache.Instance.LastInStation.AddSeconds(20))
                    {
                        if (_directEntity != null && _directEntity.IsValid)
                        {
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
                    if (IsTarget)
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