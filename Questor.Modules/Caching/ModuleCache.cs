// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------


using System.Linq;

namespace Questor.Modules.Caching
{
    using System;
    //using System.Collections.Generic;
    using DirectEve;
    using Questor.Modules.Lookup;
    using Questor.Modules.Logging;

    public class ModuleCache
    {
        private readonly DirectModule _module;
        
        private DateTime ThisModuleCacheCreated = DateTime.UtcNow;
        public ModuleCache(DirectModule module)
        {
            //
            // reminder: this class and all the info within it is created (and destroyed!) each frame for each module!
            //
            _module = module;
            ThisModuleCacheCreated = DateTime.UtcNow;
        }

        public int TypeId
        {
            get { return _module.TypeId; }
        }

        public string TypeName
        {
            get { return _module.TypeName; }
        }

        public int GroupId
        {
            get { return _module.GroupId; }
        }

        public double Damage
        {
            get { return _module.Damage; }
        }

        public bool ActivatePlex //do we need to make sure this is ONLY valid on a PLEX?
        {
            get { return _module.ActivatePLEX(); }
        }

        public bool AssembleShip // do we need to make sure this is ONLY valid on a packaged ship?
        {
            get { return _module.AssembleShip(); }
        }

        //public double Attributes
        //{
        //    get { return _module.Attributes; }
        //}

        public double AveragePrice
        {
            get { return _module.AveragePrice(); }
        }

        public double Duration
        {
            get { return _module.Duration ?? 0; }
        }

        public double FallOff
        {
            get { return _module.FallOff ?? 0; }
        }

        public double MaxRange
        {
            get
            {
                try
                {
                    double? _maxRange = null;
                    //_maxRange = _module.Attributes.TryGet<double>("maxRange");

                    if (_maxRange == null || _maxRange == 0)
                    {
                        //
                        // if we could not find the max range via EVE use the XML setting for RemoteRepairers
                        //
                        if (_module.GroupId == (int)Group.RemoteArmorRepairer || _module.GroupId == (int)Group.RemoteShieldRepairer || _module.GroupId == (int)Group.RemoteHullRepairer)
                        {
                            return Settings.Instance.RemoteRepairDistance;
                        }
                        //
                        // if we could not find the max range via EVE use the XML setting for Nos/Neuts
                        //
                        if (_module.GroupId == (int)Group.NOS || _module.GroupId == (int)Group.Neutralizer)
                        {
                            return Settings.Instance.NosDistance;
                        }
                        //
                        // Add other types of modules here?
                        //
                        return 0;
                    }

                    return (double)_maxRange;
                }
                catch(Exception ex)
                {
                    Logging.Log("ModuleCache.RemoteRepairDistance", "Exception [ " + ex + " ]", Logging.Debug);
                }

                return 0;
            }            
        }

        public double Hp
        {
            get { return _module.Hp; }
        }

        public bool IsOverloaded
        {
            get { return _module.IsOverloaded; }
        }

        public bool IsPendingOverloading
        {
            get { return _module.IsPendingOverloading; }
        }

        public bool IsPendingStopOverloading
        {
            get { return _module.IsPendingStopOverloading; }
        }

        public bool ToggleOverload
        {
            get { return _module.ToggleOverload(); }
        }

        public bool IsActivatable
        {
            get { return _module.IsActivatable; }
        }

        public long ItemId
        {
            get { return _module.ItemId; }
        }

        public bool IsActive
        {
            get { return _module.IsActive; }
        }

        public bool IsOnline
        {
            get { return _module.IsOnline; }
        }

        public bool IsGoingOnline
        {
            get { return _module.IsGoingOnline; }
        }

        public bool IsReloadingAmmo
        {
            get
            {
                int reloadDelayToUseForThisWeapon;
                if (IsEnergyWeapon)
                {
                    reloadDelayToUseForThisWeapon = 1;
                }
                else
                {
                    reloadDelayToUseForThisWeapon = Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                }

                if (Time.Instance.LastReloadedTimeStamp != null && Time.Instance.LastReloadedTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Time.Instance.LastReloadedTimeStamp[ItemId].AddSeconds(reloadDelayToUseForThisWeapon))
                    {
                        //if (Settings.Instance.DebugActivateWeapons) Logging.Log("ModuleCache", "TypeName: [" + _module.TypeName + "] This module is likely still reloading! aborting activating this module.", Logging.Debug);
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsDeactivating
        {
            get { return _module.IsDeactivating; }
        }

        public bool IsChangingAmmo
        {
            get
            {
                int reloadDelayToUseForThisWeapon;
                if (IsEnergyWeapon)
                {
                    reloadDelayToUseForThisWeapon = 1;
                }
                else
                {
                    reloadDelayToUseForThisWeapon = Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                }

                if (Time.Instance.LastChangedAmmoTimeStamp != null && Time.Instance.LastChangedAmmoTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Time.Instance.LastChangedAmmoTimeStamp[ItemId].AddSeconds(reloadDelayToUseForThisWeapon))
                    {
                        //if (Settings.Instance.DebugActivateWeapons) Logging.Log("ModuleCache", "TypeName: [" + _module.TypeName + "] This module is likely still changing ammo! aborting activating this module.", Logging.Debug);
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsTurret
        {
            get
            {
                if (GroupId == (int)Group.EnergyWeapon) return true;
                if (GroupId == (int)Group.ProjectileWeapon) return true;
                if (GroupId == (int)Group.HybridWeapon) return true;
                return false;
            }
        }

        public bool IsMissileLauncher
        {
            get
            {
                if (GroupId == (int)Group.AssaultMissilelaunchers) return true;
                if (GroupId == (int)Group.CruiseMissileLaunchers) return true;
                if (GroupId == (int)Group.TorpedoLaunchers) return true;
                if (GroupId == (int)Group.StandardMissileLaunchers) return true;
                if (GroupId == (int)Group.AssaultMissilelaunchers) return true;
                if (GroupId == (int)Group.HeavyMissilelaunchers) return true;
                if (GroupId == (int)Group.DefenderMissilelaunchers) return true;
                return false;
            }
        }

        public bool IsEnergyWeapon
        {
            get { return GroupId == (int)Group.EnergyWeapon; }
        }

        public long TargetId
        {
            get { return _module.TargetId ?? -1; }
        }

        public long LastTargetId
        {
            get
            {
                if (Cache.Instance.LastModuleTargetIDs.ContainsKey(ItemId))
                {
                    return Cache.Instance.LastModuleTargetIDs[ItemId];
                }

                return -1;
            }
        }

        //public IEnumerable<DirectItem> MatchingAmmo
        //{
        //    get { return _module.MatchingAmmo; }
        //}

        public DirectItem Charge
        {
            get { return _module.Charge; }
        }

        public int CurrentCharges
        {
            get
            {
                if (_module.Charge != null)
                    return _module.Charge.Quantity;

                return -1;
            }
        }

        public int MaxCharges
        {
            get { return _module.MaxCharges; }
        }

        public double OptimalRange
        {
            get { return _module.OptimalRange ?? 0; }
        }

        public bool AutoReload
        {
            get { return _module.AutoReload; }
        }

        public bool DisableAutoReload
        {
            get
            {
                if (IsActivatable && !InLimboState)
                {
                    if (_module.AutoReload)
                    {
                        _module.SetAutoReload(false);
                        return false;
                    }

                    return true;
                }
                
                return true;
            }
        }
        
        public bool DoesNotRequireAmmo
        {
            get
            {
                if (TypeId == (int)TypeID.CivilianGatlingPulseLaser) return true;
                if (TypeId == (int)TypeID.CivilianGatlingAutocannon) return true;
                if (TypeId == (int)TypeID.CivilianGatlingRailgun) return true;
                if (TypeId == (int)TypeID.CivilianLightElectronBlaster) return true;
                return false;
            }
        }

        public bool ReloadAmmo(DirectItem charge, int weaponNumber, double Range)
        {
            if (!IsReloadingAmmo)
            {
                if (!IsChangingAmmo)
                {
                    if (!InLimboState)
                    {
                        Logging.Log("ReloadAmmo", "Reloading [" + weaponNumber + "] [" + _module.TypeName + "] with [" + charge.TypeName + "][" + Math.Round(Range / 1000, 0) + "]", Logging.Teal);
                        _module.ReloadAmmo(charge);
                        Time.Instance.LastReloadedTimeStamp[ItemId] = DateTime.UtcNow;
                        if (Time.Instance.ReloadTimePerModule.ContainsKey(ItemId))
                        {
                            Time.Instance.ReloadTimePerModule[ItemId] = Time.Instance.ReloadTimePerModule[ItemId] + Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                        }
                        else
                        {
                            Time.Instance.ReloadTimePerModule[ItemId] = Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                        }

                        return true;
                    }

                    Logging.Log("ReloadAmmo", "[" + weaponNumber + "][" + _module.TypeName + "] is currently in a limbo state, waiting", Logging.Teal);
                    return false;
                }

                Logging.Log("ReloadAmmo", "[" + weaponNumber + "][" + _module.TypeName + "] is already changing ammo, waiting", Logging.Teal);
                return false;
            }

            Logging.Log("ReloadAmmo", "[" + weaponNumber + "][" + _module.TypeName + "] is already reloading, waiting", Logging.Teal);
            return false;
        }

        public bool ChangeAmmo(DirectItem charge, int weaponNumber, double Range, String entityName = "n/a", Double entityDistance = 0)
        {
            if (!IsReloadingAmmo)
            {
                if (!IsChangingAmmo)
                {
                    if (!InLimboState)
                    {
                        _module.ChangeAmmo(charge);
                        Logging.Log("ChangeAmmo", "Changing [" + weaponNumber + "][" + _module.TypeName + "] with [" + charge.TypeName + "][" + Math.Round(Range / 1000, 0) + "] so we can hit [" + entityName + "][" + Math.Round(entityDistance / 1000, 0) + "k]", Logging.Teal);
                        Time.Instance.LastChangedAmmoTimeStamp[ItemId] = DateTime.UtcNow;
                        if (Time.Instance.ReloadTimePerModule.ContainsKey(ItemId))
                        {
                            Time.Instance.ReloadTimePerModule[ItemId] = Time.Instance.ReloadTimePerModule[ItemId] + Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                        }
                        else
                        {
                            Time.Instance.ReloadTimePerModule[ItemId] = Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                        }

                        return true;    
                    }

                    Logging.Log("ChangeAmmo", "[" + weaponNumber + "][" + _module.TypeName + "] is currently in a limbo state, waiting", Logging.Teal);
                    return false;
                }

                Logging.Log("ChangeAmmo", "[" + weaponNumber + "][" + _module.TypeName + "] is already changing ammo, waiting", Logging.Teal);
                return false;
            }

            Logging.Log("ChangeAmmo", "[" + weaponNumber + "][" + _module.TypeName + "] is already reloading, waiting", Logging.Teal);
            return false;
        }

        public bool InLimboState
        {
            get
            {
                try
                {
                    bool result = false;
                    result |= !IsActivatable;
                    result |= !IsOnline;
                    result |= IsDeactivating;
                    result |= IsGoingOnline;
                    result |= IsReloadingAmmo;
                    result |= IsChangingAmmo;
                    result |= !Cache.Instance.InSpace;
                    result |= Cache.Instance.InStation;
                    result |= Time.Instance.LastInStation.AddSeconds(7) > DateTime.UtcNow;
                    return result;
                }
                catch (Exception exception)
                {
                    Logging.Log("InLimboState", "IterateUnloadLootTheseItemsAreLootItems - Exception: [" + exception + "]", Logging.Red);
                    return false;
                }
            }
        }

        private int ClickCountThisFrame = 0;
        public bool Click()
        {
            try
            {
                if (InLimboState || ClickCountThisFrame > 0)
                    return false;

                if (Time.Instance.LastClickedTimeStamp != null && Time.Instance.LastClickedTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Time.Instance.LastClickedTimeStamp[ItemId].AddMilliseconds(Settings.Instance.EnforcedDelayBetweenModuleClicks))
                    {
                        //if (Settings.Instance.DebugEntityCache) Logging.Log("ModuleCache", "TypeName: [" + _module.TypeName + "] we last clicked this module less than 3 seconds ago, wait.", Logging.Debug);
                        return false;
                    }
                }

                ClickCountThisFrame++;

                if (IsActivatable)
                {
                    if (!IsActive) //it is not yet active, this click should activate it.
                    {
                        Time.Instance.LastActivatedTimeStamp[ItemId] = DateTime.UtcNow;
                    }

                    if (Time.Instance.LastClickedTimeStamp != null) Time.Instance.LastClickedTimeStamp[ItemId] = DateTime.UtcNow;
                    return _module.Click();
                }

                return false;
            }
            catch (Exception exception)
            {
                Logging.Log("Click", "IterateUnloadLootTheseItemsAreLootItems - Exception: [" + exception + "]", Logging.Red);
                return false;
            }
        }

        private int ActivateCountThisFrame = 0;

        public EachWeaponsVolleyCache SnapshotOfVolleyData;
        public bool Activate(EntityCache target)
        {
            try
            {
                if (InLimboState || IsActive || ActivateCountThisFrame > 0)
                    return false;

                if (!DisableAutoReload)
                    return false;

                ActivateCountThisFrame++;

                if (Time.Instance.LastReloadedTimeStamp != null && Time.Instance.LastReloadedTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Time.Instance.LastReloadedTimeStamp[ItemId].AddSeconds(Time.Instance.ReloadWeaponDelayBeforeUsable_seconds))
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Activate", "TypeName: [" + _module.TypeName + "] This module is likely still reloading! aborting activating this module.", Logging.Debug);
                        return false;
                    }
                }

                if (Time.Instance.LastChangedAmmoTimeStamp != null && Time.Instance.LastChangedAmmoTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Time.Instance.LastChangedAmmoTimeStamp[ItemId].AddSeconds(Time.Instance.ReloadWeaponDelayBeforeUsable_seconds))
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Activate", "TypeName: [" + _module.TypeName + "] This module is likely still changing ammo! aborting activating this module.", Logging.Debug);
                        return false;
                    }
                }

                if (!target.IsTarget)
                {
                    Logging.Log("Activate", "Target [" + target.Name + "][" + Math.Round(target.Distance / 1000, 2) + "]IsTargeting[" + target.IsTargeting + "] was not locked, aborting activating module as we cant activate a module on something that is not locked!", Logging.Debug);
                    return false;
                }

                if (IsMissileLauncher && Settings.Instance.AvoidShootingTargetsWithMissilesIfweKNowTheyAreAboutToBeHitWithAPreviousVolley)
                {
                    if (Cache.Instance.ListofEachWeaponsVolleyData != null && Cache.Instance.ListofEachWeaponsVolleyData.Any())
                    {
                        //
                        // sanity check
                        //
                        if (Cache.Instance.ListofEachWeaponsVolleyData.Count() > 100000)
                        {
                            Logging.Log("Activate", "We should be clearing ListofEachWeaponsVolleyData at the end of each mission, how did we get [" + Cache.Instance.ListofEachWeaponsVolleyData.Count() + "] entries already?", Logging.Debug);
                        }

                        foreach (EachWeaponsVolleyCache _volley in Cache.Instance.ListofEachWeaponsVolleyData.Where(i => i.targetItemID == target.Id))
                        {
                            DirectInvType __directInvTypeItem = null;
                            Cache.Instance.DirectEve.InvTypes.TryGetValue((int)_volley.moduleAmmoTypeID, out __directInvTypeItem); //create a new DirectIntType object and assign it
                            if (__directInvTypeItem != null)
                            {
                                DirectItem __directItem = null;
                                __directItem = (DirectItem)__directInvTypeItem; //cast the directInvType Object to a DirectItem
                                ItemCache __item = null;
                                __item = new ItemCache(__directItem); //create an ItemCache object from the DirectItem

                                //TRUE Max Missile Range
                                //
                                //r = Range
                                //v = Velocity of missile
                                //f = Flight time of missile
                                //m = Mass of missile
                                //a = Agility of missile

                                //Quote:
                                //r = v*(f-(10^6/(m*a)
                                //
                                double WeaponTimeToTarget_Seconds = _volley.targetDistance/__item.maxVelocity;
                                if (Cache.Instance.MyMissileProjectionSkillLevel > 0)
                                {                                                                              
                                    WeaponTimeToTarget_Seconds = _volley.targetDistance / (__item.maxVelocity * (1 + ((Cache.Instance.MyMissileProjectionSkillLevel * 10) / 100)));
                                }

                                if (DateTime.UtcNow > _volley.ThisVolleyCacheCreated.AddSeconds(WeaponTimeToTarget_Seconds + .5))
                                {
                                    continue;
                                }

                                if (DateTime.UtcNow < _volley.ThisVolleyCacheCreated.AddSeconds(WeaponTimeToTarget_Seconds - .5))
                                {
                                    continue;
                                }

                                //
                                // we found a volley that should hit this target within the next 1 second: delaying the next volley until the next pulse
                                //
                                Logging.Log("Activate", "Target [" + target.Name + "][" + Math.Round(target.Distance / 1000, 2) + "] has a volley about to hit it, waiting a moment before firing again", Logging.Debug);
                                return false;
                            }
                        }
                    }
                }
                
                //DateTime.UtcNow > i.ThisVolleyCacheCreated.AddSeconds(10)))
                
                _module.Activate(target.Id);
                SnapshotOfVolleyData = new EachWeaponsVolleyCache(_module, target);
                if (IsMissileLauncher || IsTurret)
                {
                    Cache.Instance.ListofEachWeaponsVolleyData.Add(SnapshotOfVolleyData);
                }
                
                Time.Instance.LastActivatedTimeStamp[ItemId] = DateTime.UtcNow;
                Cache.Instance.LastModuleTargetIDs[ItemId] = target.Id;
                return true;
            }
            catch (Exception exception)
            {
                Logging.Log("Activate", "IterateUnloadLootTheseItemsAreLootItems - Exception: [" + exception + "]", Logging.Red);
                return false;
            }
        }

        public void Deactivate()
        {
            if (InLimboState || !IsActive)
                return;

            _module.Deactivate();
        }
    }
}