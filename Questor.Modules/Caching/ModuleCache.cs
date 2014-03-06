// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using System.Diagnostics.Eventing.Reader;

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

                if (Cache.Instance.LastReloadedTimeStamp != null && Cache.Instance.LastReloadedTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Cache.Instance.LastReloadedTimeStamp[ItemId].AddSeconds(reloadDelayToUseForThisWeapon))
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

                if (Cache.Instance.LastChangedAmmoTimeStamp != null && Cache.Instance.LastChangedAmmoTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Cache.Instance.LastChangedAmmoTimeStamp[ItemId].AddSeconds(reloadDelayToUseForThisWeapon))
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
                        Cache.Instance.LastReloadedTimeStamp[ItemId] = DateTime.UtcNow;
                        if (Cache.Instance.ReloadTimePerModule.ContainsKey(ItemId))
                        {
                            Cache.Instance.ReloadTimePerModule[ItemId] = Cache.Instance.ReloadTimePerModule[ItemId] + Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                        }
                        else
                        {
                            Cache.Instance.ReloadTimePerModule[ItemId] = Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
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
                        Cache.Instance.LastChangedAmmoTimeStamp[ItemId] = DateTime.UtcNow;
                        if (Cache.Instance.ReloadTimePerModule.ContainsKey(ItemId))
                        {
                            Cache.Instance.ReloadTimePerModule[ItemId] = Cache.Instance.ReloadTimePerModule[ItemId] + Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
                        }
                        else
                        {
                            Cache.Instance.ReloadTimePerModule[ItemId] = Time.Instance.ReloadWeaponDelayBeforeUsable_seconds;
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
                    result |= Cache.Instance.LastInStation.AddSeconds(7) > DateTime.UtcNow;
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

                if (Cache.Instance.LastClickedTimeStamp != null && Cache.Instance.LastClickedTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Cache.Instance.LastClickedTimeStamp[ItemId].AddMilliseconds(Settings.Instance.EnforcedDelayBetweenModuleClicks))
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
                        Cache.Instance.LastActivatedTimeStamp[ItemId] = DateTime.UtcNow;
                    }

                    if (Cache.Instance.LastClickedTimeStamp != null) Cache.Instance.LastClickedTimeStamp[ItemId] = DateTime.UtcNow;
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

        public bool Activate(EntityCache target)
        {
            try
            {
                if (InLimboState || IsActive || ActivateCountThisFrame > 0)
                    return false;

                ActivateCountThisFrame++;

                if (Cache.Instance.LastReloadedTimeStamp != null && Cache.Instance.LastReloadedTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Cache.Instance.LastReloadedTimeStamp[ItemId].AddSeconds(Time.Instance.ReloadWeaponDelayBeforeUsable_seconds))
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Activate", "TypeName: [" + _module.TypeName + "] This module is likely still reloading! aborting activating this module.", Logging.Debug);
                        return false;
                    }
                }

                if (Cache.Instance.LastChangedAmmoTimeStamp != null && Cache.Instance.LastChangedAmmoTimeStamp.ContainsKey(ItemId))
                {
                    if (DateTime.UtcNow < Cache.Instance.LastChangedAmmoTimeStamp[ItemId].AddSeconds(Time.Instance.ReloadWeaponDelayBeforeUsable_seconds))
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

                _module.Activate(target.Id);
                Cache.Instance.LastActivatedTimeStamp[ItemId] = DateTime.UtcNow;
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