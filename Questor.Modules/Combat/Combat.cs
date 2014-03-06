// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using System.Runtime.Remoting;
using System.Threading;
using Questor.Modules.Activities;
using Questor.Modules.BackgroundTasks;
using Questor.Modules.Caching;

namespace Questor.Modules.Combat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using Questor.Modules.Logging;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;

    /// <summary>
    ///   The combat class will target and kill any NPC that is targeting the questor.
    ///   It will also kill any NPC that is targeted but not aggressive  toward the questor.
    /// </summary>
    public static class Combat
    {
        private static bool _isJammed;
        private static int _weaponNumber;

        private static int MaxCharges { get; set; }

        private static DateTime _lastCombatProcessState;

        //private static DateTime _lastReloadAll;
        private static int _reloadAllIteration;

        private static IEnumerable<EntityCache> __highValueTargets;
        private static IEnumerable<EntityCache> __lowValueTargets;
        private static IEnumerable<EntityCache> __highValueTargetsTargeted;
        private static IEnumerable<EntityCache> __lowValueTargetsTargeted;
        private static int maxHighValueTargets;
        private static int maxLowValueTargets;
        private static int maxTotalTargets;
        public static int CombatInstances = 0;
        private static int i = 0;
        public static List<EntityCache> TargetingMe { get; set; }
        public static List<EntityCache> NotYetTargetingMe { get; set; }

        static Combat()
        {
            maxLowValueTargets = Settings.Instance.MaximumLowValueTargets;
            maxHighValueTargets = Settings.Instance.MaximumHighValueTargets;
            maxTotalTargets = maxHighValueTargets + maxLowValueTargets;
            
            //maxTargetingSlotsAvailable = Cache.Instance.MaxLockedTargets;

            Interlocked.Increment(ref CombatInstances);
        }

        //~Combat()
        //{
        //    Interlocked.Decrement(ref CombatInstances);
        //}

        // Reload correct (tm) ammo for the NPC
        // (enough/correct) ammo is loaded, false if wrong/not enough ammo is loaded
        private static bool ReloadNormalAmmo(ModuleCache weapon, EntityCache entity, int weaponNumber, bool force = false)
        {
            if (Settings.Instance.WeaponGroupId == 53) return true;
            if (entity == null)
            {
                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "if (entity == null)", Logging.Orange);
                return false;
            }

            List<Ammo> correctAmmoToUse = null;
            List<Ammo> correctAmmoInCargo = null;

            //
            // NOTE: This new setting is NOT ready for use!
            // NOTE: when we are finished molesting ReloadNormalAmmo we should do the same to the routine used for lasers... 
            //
            // ammo selection based on target size
            //
            if (Settings.Instance.SelectAmmoToUseBasedOnShipSize)
            {
                if (Cache.Instance.LastChangedAmmoTimeStamp != null && Cache.Instance.LastChangedAmmoTimeStamp.ContainsKey(weapon.ItemId))
                {
                    //
                    // Do not allow the changing of ammo if we already changed ammo in the last 60 seconds AND we do not need to reload yet
                    //
                    if (DateTime.UtcNow < Cache.Instance.LastChangedAmmoTimeStamp[weapon.ItemId].AddSeconds(Time.Instance.EnforcedDelayBetweenArbitraryAmmoChanges))
                    {
                        if (weapon.CurrentCharges >= Settings.Instance.MinimumAmmoCharges && !force)
                        {
                            if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "[" + weapon.TypeName + "] last reloaded [" + DateTime.UtcNow.Subtract(Cache.Instance.LastChangedAmmoTimeStamp[weapon.ItemId]).TotalSeconds + "sec ago] [ " + weapon.CurrentCharges + " ] charges in in [" + Cache.Instance.Weapons.Count() + "] total weapons, minimum of [" + Settings.Instance.MinimumAmmoCharges + "] charges, MaxCharges is [" + weapon.MaxCharges + "]", Logging.Orange);
                            return true;
                        }
                    }
                }

                if (Settings.Instance.Ammo.Any(a => a.DamageType == Cache.Instance.MissionDamageType))
                {
                    // Get ammo based on damage type
                    correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.MissionDamageType).ToList();

                    //
                    // get Ammo Based on entity we are shooting's size class, default to normal ammo if we cant determine its size
                    //
                    if (entity.IsBattleship)
                    {
                        // this needs one more layer somewhere to determine the right damage type for battleships, etc (EM, kinetic, etc) and it needs to do it for 
                        // default, faction and mission specific layers
                        //
                        //correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType.BattleShip_EM).ToList(); 
                    }
                    else if (entity.IsBattlecruiser)
                    {
                        //correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType.BattleShip_EM).ToList();
                    }
                    else if (entity.IsCruiser)
                    {
                        //correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType.BattleShip_EM).ToList();
                    }
                    else if (entity.IsFrigate)
                    {
                        //correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType.Frigate_EM).ToList();
                    }
                    else if (entity.IsLargeCollidable)
                    {
                        //correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType.LargeCollidable_EM).ToList();
                    }
                    else if (entity.IsPlayer)
                    {
                        //correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType.PVP_EM).ToList();
                    }

                    // Check if we still have that ammo in our cargo
                    correctAmmoInCargo = correctAmmoToUse.Where(a => Cache.Instance.CurrentShipsCargo.Items.Any(i => i.TypeId == a.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges)).ToList();

                    //check if mission specific ammo is defined
                    if (Cache.Instance.MissionAmmo.Count() != 0)
                    {
                        correctAmmoInCargo = Cache.Instance.MissionAmmo.Where(a => a.DamageType == Cache.Instance.MissionDamageType).ToList();
                    }

                    // Check if we still have that ammo in our cargo
                    correctAmmoInCargo = correctAmmoInCargo.Where(a => Cache.Instance.CurrentShipsCargo.Items.Any(i => i.TypeId == a.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges)).ToList();
                    if (Cache.Instance.MissionAmmo.Count() != 0)
                    {
                        correctAmmoInCargo = Cache.Instance.MissionAmmo;
                    }

                    // We are out of ammo! :(
                    if (!correctAmmoInCargo.Any())
                    {
                        Logging.Log("Combat", "ReloadNormalAmmo: not enough [" + Cache.Instance.MissionDamageType + "] ammo in cargohold: MinimumCharges: [" + Settings.Instance.MinimumAmmoCharges + "]", Logging.Orange);
                        _States.CurrentCombatState = CombatState.OutOfAmmo;
                        return false;
                    }

                    
                }
                else
                {
                    Logging.Log("Combat", "ReloadNormalAmmo: ammo is not defined properly in the ammo section of this characters settings xml", Logging.Orange);
                    _States.CurrentCombatState = CombatState.OutOfAmmo;
                    return false;
                }

            }
            else
            {
                //
                // normal ammo selection - ignore target attributes and uses the right ammo for the pocket
                //
                if (Settings.Instance.Ammo.Any(a => a.DamageType == Cache.Instance.MissionDamageType))
                {
                    // Get ammo based on damage type
                    correctAmmoToUse = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.MissionDamageType).ToList();

                    // Check if we still have that ammo in our cargo
                    correctAmmoInCargo = correctAmmoToUse.Where(a => Cache.Instance.CurrentShipsCargo.Items.Any(i => i.TypeId == a.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges)).ToList();

                    //check if mission specific ammo is defined
                    if (Cache.Instance.MissionAmmo.Count() != 0)
                    {
                        correctAmmoInCargo = Cache.Instance.MissionAmmo.Where(a => a.DamageType == Cache.Instance.MissionDamageType).ToList();
                    }

                    // Check if we still have that ammo in our cargo
                    correctAmmoInCargo = correctAmmoInCargo.Where(a => Cache.Instance.CurrentShipsCargo.Items.Any(i => i.TypeId == a.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges)).ToList();
                    if (Cache.Instance.MissionAmmo.Count() != 0)
                    {
                        correctAmmoInCargo = Cache.Instance.MissionAmmo;
                    }

                    // We are out of ammo! :(
                    if (!correctAmmoInCargo.Any())
                    {
                        Logging.Log("Combat", "ReloadNormalAmmo: not enough [" + Cache.Instance.MissionDamageType + "] ammo in cargohold: MinimumCharges: [" + Settings.Instance.MinimumAmmoCharges + "]", Logging.Orange);
                        _States.CurrentCombatState = CombatState.OutOfAmmo;
                        return false;
                    }
                }
                else
                {
                    Logging.Log("Combat", "ReloadNormalAmmo: ammo is not defined properly in the ammo section of this characters settings xml", Logging.Orange);
                    _States.CurrentCombatState = CombatState.OutOfAmmo;
                    return false;
                }
            }
            

            /******
            if (weapon.Charge != null)
            {
                IEnumerable<Ammo> areWeMissingAmmo = correctAmmo.Where(a => a.TypeId == weapon.Charge.TypeId);
                if (!areWeMissingAmmo.Any())
                {
                    if (DateTime.UtcNow.Subtract(Cache.Instance.LastLoggingAction).TotalSeconds > 4)
                    {
                        Logging.Log("Combat", "ReloadNormalAmmo: We have ammo loaded that does not have a full reload available, checking cargo for other ammo", Logging.Orange);
                        Cache.Instance.LastLoggingAction = DateTime.UtcNow;
                        try
                        {
                            if (Settings.Instance.Ammo.Any())
                            {
                                DirectItem availableAmmo = cargo.Items.OrderByDescending(i => i.Quantity).Where(a => Settings.Instance.Ammo.Any(i => i.TypeId == a.TypeId)).ToList().FirstOrDefault();
                                if (availableAmmo != null)
                                {
                                    Cache.Instance.DamageType = Settings.Instance.Ammo.ToList().OrderByDescending(i => i.Quantity).Where(a => a.TypeId == availableAmmo.TypeId).ToList().FirstOrDefault().DamageType;
                                    Logging.Log("Combat", "ReloadNormalAmmo: found [" + availableAmmo.Quantity + "] units of  [" + availableAmmo.TypeName + "] changed DamageType to [" + Cache.Instance.DamageType.ToString() + "]", Logging.Orange);
                                    return false;
                                }

                                Logging.Log("Combat", "ReloadNormalAmmo: unable to find any alternate ammo in your cargo", Logging.teal);
                                _States.CurrentCombatState = CombatState.OutOfAmmo;
                                return false;
                            }
                        }
                        catch (Exception)
                        {
                            Logging.Log("Combat", "ReloadNormalAmmo: unable to find any alternate ammo in your cargo", Logging.teal);
                            _States.CurrentCombatState = CombatState.OutOfAmmo;
                        }
                        return false;
                    }
                }
            }
            *****/

            // Get the best possible ammo
            Ammo ammo = correctAmmoInCargo.FirstOrDefault();
            try
            {
                if (ammo != null && entity != null)
                {
                    ammo = correctAmmoInCargo.Where(a => a.Range > entity.Distance).OrderBy(a => a.Range).FirstOrDefault();
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Combat", "ReloadNormalAmmo: Unable to find the correct ammo: waiting [" + exception + "]", Logging.Teal);
                return false;
            }

            // We do not have any ammo left that can hit targets at that range!
            if (ammo == null)
            {
                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "We do not have any ammo left that can hit targets at that range!", Logging.Orange);
                return false;
            }

            // Do We have enough ammo loaded?
            if (weapon.Charge != null && weapon.Charge.TypeId == ammo.TypeId)
            {
                if (weapon.CurrentCharges >= Settings.Instance.MinimumAmmoCharges && !force)
                {
                    if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "[ " + weapon.CurrentCharges + " ] charges in in [" + Cache.Instance.Weapons.Count() + "] total weapons, minimum of [" + Settings.Instance.MinimumAmmoCharges + "] charges, MaxCharges is [" + weapon.MaxCharges + "]", Logging.Orange);
                    return true;
                }

                if (weapon.CurrentCharges >= weapon.MaxCharges && force)
                {
                    //
                    // even if force is true do not reload a weapon that is already full!
                    //
                    if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "[ " + weapon.CurrentCharges + " ] charges in [" + Cache.Instance.Weapons.Count() + "] total weapons, MaxCharges [" + weapon.MaxCharges + "]", Logging.Orange);
                    return true;
                }

                if (force && weapon.CurrentCharges < weapon.MaxCharges)
                {
                    //
                    // allow the reload (and log it!)
                    //
                    if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "[ " + weapon.CurrentCharges + " ] charges in [" + Cache.Instance.Weapons.Count() + "] total weapons, MaxCharges [" + weapon.MaxCharges + "] - forced reloading proceeding", Logging.Orange);
                }

            }

            // Retry later, assume its ok now
            //if (!weapon.MatchingAmmo.Any())
            //{
            //    LastWeaponReload[weapon.ItemId] = DateTime.UtcNow; //mark this weapon as reloaded... by the time we need to reload this timer will have aged enough...
            //    return true;
            //}

            DirectItem charge = Cache.Instance.CurrentShipsCargo.Items.FirstOrDefault(e => e.TypeId == ammo.TypeId && e.Quantity >= Settings.Instance.MinimumAmmoCharges);

            // This should have shown up as "out of ammo"
            if (charge == null)
            {
                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "This should have shown up as out of ammo", Logging.Orange);
                return false;
            }

            // If we are reloading, wait Time.ReloadWeaponDelayBeforeUsable_seconds (see time.cs)
            if (weapon.IsReloadingAmmo)
            {
                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "We are already reloading, wait - weapon.IsReloadingAmmo [" + weapon.IsReloadingAmmo + "]", Logging.Orange);
                return true;
            }

            // If we are changing ammo, wait Time.ReloadWeaponDelayBeforeUsable_seconds (see time.cs)
            if (weapon.IsChangingAmmo)
            {
                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "We are already changing ammo, wait - weapon.IsReloadingAmmo [" + weapon.IsReloadingAmmo + "]", Logging.Orange);
                return true;
            }

            try
            {
                // Reload or change ammo
                if (weapon.Charge != null && weapon.Charge.TypeId == charge.TypeId && !weapon.IsChangingAmmo)
                {
                    if (weapon.ReloadAmmo(charge, weaponNumber, (double) ammo.Range))
                    {
                        return true;
                    }

                    return false;
                }
                
                if (weapon.ChangeAmmo(charge, weaponNumber, (double) ammo.Range, entity.Name, entity.Distance))
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logging.Log("Combat.ReloadNormalAmmo", "Exception [" + exception + "]", Logging.Debug);
            }

            // Return true as we are reloading ammo, assume it is the correct ammo...
            return true;
        }

        private static bool ReloadEnergyWeaponAmmo(ModuleCache weapon, EntityCache entity, int weaponNumber)
        {
            // Get ammo based on damage type
            IEnumerable<Ammo> correctAmmo = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.MissionDamageType).ToList();

            // Check if we still have that ammo in our cargo
            IEnumerable<Ammo> correctAmmoInCargo = correctAmmo.Where(a => Cache.Instance.CurrentShipsCargo.Items.Any(e => e.TypeId == a.TypeId)).ToList();

            //check if mission specific ammo is defined
            if (Cache.Instance.MissionAmmo.Count() != 0)
            {
                correctAmmoInCargo = Cache.Instance.MissionAmmo.Where(a => a.DamageType == Cache.Instance.MissionDamageType).ToList();
            }

            // Check if we still have that ammo in our cargo
            correctAmmoInCargo = correctAmmoInCargo.Where(a => Cache.Instance.CurrentShipsCargo.Items.Any(e => e.TypeId == a.TypeId && e.Quantity >= Settings.Instance.MinimumAmmoCharges)).ToList();
            if (Cache.Instance.MissionAmmo.Count() != 0)
            {
                correctAmmoInCargo = Cache.Instance.MissionAmmo;
            }

            // We are out of ammo! :(
            if (!correctAmmoInCargo.Any())
            {
                Logging.Log("Combat", "ReloadEnergyWeapon: not enough [" + Cache.Instance.MissionDamageType + "] ammo in cargohold: MinimumCharges: [" + Settings.Instance.MinimumAmmoCharges + "]", Logging.Orange);
                _States.CurrentCombatState = CombatState.OutOfAmmo;
                return false;
            }

            if (weapon.Charge != null)
            {
                IEnumerable<Ammo> areWeMissingAmmo = correctAmmoInCargo.Where(a => a.TypeId == weapon.Charge.TypeId);
                if (!areWeMissingAmmo.Any())
                {
                    Logging.Log("Combat", "ReloadEnergyWeaponAmmo: We have ammo loaded that does not have a full reload available in the cargo.", Logging.Orange);
                }
            }

            // Get the best possible ammo - energy weapons change ammo near instantly
            Ammo ammo = correctAmmoInCargo.Where(a => a.Range > (entity.Distance)).OrderBy(a => a.Range).FirstOrDefault(); //default

            // We do not have any ammo left that can hit targets at that range!
            if (ammo == null)
            {
                if (Settings.Instance.DebugReloadorChangeAmmo) Logging.Log("Combat", "ReloadEnergyWeaponAmmo: best possible ammo: [ ammo == null]", Logging.White);
                return false;
            }

            if (Settings.Instance.DebugReloadorChangeAmmo) Logging.Log("Combat", "ReloadEnergyWeaponAmmo: best possible ammo: [" + ammo.TypeId + "][" + ammo.DamageType + "]", Logging.White);
            if (Settings.Instance.DebugReloadorChangeAmmo) Logging.Log("Combat", "ReloadEnergyWeaponAmmo: best possible ammo: [" + entity.Name + "][" + Math.Round(entity.Distance / 1000, 0) + "]", Logging.White);

            DirectItem charge = Cache.Instance.CurrentShipsCargo.Items.OrderBy(e => e.Quantity).FirstOrDefault(e => e.TypeId == ammo.TypeId);

            // We do not have any ammo left that can hit targets at that range!
            if (charge == null)
            {
                if (Settings.Instance.DebugReloadorChangeAmmo) Logging.Log("Combat", "ReloadEnergyWeaponAmmo: We do not have any ammo left that can hit targets at that range!", Logging.Orange);
                return false;
            }

            if (Settings.Instance.DebugReloadorChangeAmmo) Logging.Log("Combat", "ReloadEnergyWeaponAmmo: charge: [" + charge.TypeName + "][" + charge.TypeId + "]", Logging.White);

            // We have enough ammo loaded
            if (weapon.Charge != null && weapon.Charge.TypeId == ammo.TypeId)
            {
                if (Settings.Instance.DebugReloadorChangeAmmo) Logging.Log("Combat", "ReloadEnergyWeaponAmmo: We have Enough Ammo of that type Loaded Already", Logging.White);
                return true;
            }

            // We are reloading, wait 
            if (weapon.IsReloadingAmmo)
                return true;

            // We are reloading, wait 
            if (weapon.IsChangingAmmo)
                return true;

            // Reload or change ammo
            if (weapon.Charge != null && weapon.Charge.TypeId == charge.TypeId)
            {
                //
                // reload
                //
                if (weapon.ReloadAmmo(charge, weaponNumber, (double) ammo.Range))
                {
                    return true;
                }

                return false;
            }
            
            //
            // change ammo
            //
            if (weapon.ChangeAmmo(charge, weaponNumber, (double) ammo.Range, entity.Name, entity.Distance))
            {
                return true;
            }

            return false;
            

            
        }

        /// <summary> Reload correct (tm) ammo for the NPC
        /// </summary>
        /// <param name = "weapon"></param>
        /// <param name = "entity"></param>
        /// <param name = "weaponNumber"></param>
        /// <returns>True if the (enough/correct) ammo is loaded, false if wrong/not enough ammo is loaded</returns>
        private static bool ReloadAmmo(ModuleCache weapon, EntityCache entity, int weaponNumber)
        {
            // We need the cargo bay open for both reload actions
            //if (!Cache.Instance.OpenCargoHold("Combat: ReloadAmmo")) return false;

            return weapon.IsEnergyWeapon ? ReloadEnergyWeaponAmmo(weapon, entity, weaponNumber) : ReloadNormalAmmo(weapon, entity, weaponNumber);
        }

        public static bool ReloadAll(EntityCache entity, bool force = false)
        {
            if (Settings.Instance.DebugReloadAll)
            {
                if (Cache.Instance.MyShipEntity.Name == Settings.Instance.TransportShipName)
                {
                    Logging.Log("ReloadAll", "You are in your TransportShip named [" + Settings.Instance.TransportShipName + "], no need to reload ammo!", Logging.Debug);
                    return true;
                }

                if (Cache.Instance.MyShipEntity.Name == Settings.Instance.TravelShipName)
                {
                    Logging.Log("ReloadAll", "You are in your TravelShipName named [" + Settings.Instance.TravelShipName + "], no need to reload ammo!", Logging.Debug);
                    return true;
                }

                if (Cache.Instance.MyShipEntity.GroupId == (int)Group.Shuttle)
                {
                    Logging.Log("ReloadAll", "You are in a Shuttle, no need to reload ammo!", Logging.Debug);
                    return true;
                }
            }

            _reloadAllIteration++;
            if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "Entering reloadAll function (again) - it iterates through all weapon stacks [" + _reloadAllIteration + "]", Logging.White);
            if (_reloadAllIteration > 12)
            {
                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "reset iteration counter", Logging.Orange);
                return true;
            }

            _weaponNumber = 0;
            if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "Weapons (or stacks of weapons?): [" + Cache.Instance.Weapons.Count() + "]", Logging.Orange);

            if (Cache.Instance.Weapons.Any())
            {
                foreach (ModuleCache weapon in Cache.Instance.Weapons)
                {
                    _weaponNumber++;
                    // Reloading energy weapons prematurely just results in unnecessary error messages, so let's not do that
                    if (weapon.IsEnergyWeapon)
                    {
                        if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll:", "if (weapon.IsEnergyWeapon) continue (energy weapons do not really need to reload)", Logging.Orange);
                        continue;
                    }

                    if (weapon.IsReloadingAmmo)
                    {
                        if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "[" + weapon.TypeName + "][" + _weaponNumber + "] is still reloading, moving on to next weapon", Logging.White);
                        continue;
                    }

                    if (weapon.IsDeactivating)
                    {
                        if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "[" + weapon.TypeName + "][" + _weaponNumber + "] is still Deactivating, moving on to next weapon", Logging.White);
                        continue;
                    }

                    if (weapon.IsChangingAmmo)
                    {
                        if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "[" + weapon.TypeName + "][" + _weaponNumber + "] is still Changing Ammo, moving on to next weapon", Logging.White);
                        continue;
                    }

                    if (weapon.IsActive)
                    {
                        if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "[" + weapon.TypeName + "][" + _weaponNumber + "] is Active, moving on to next weapon", Logging.White);
                        continue;
                    }

                    if (Cache.Instance.LastReloadedTimeStamp != null && Cache.Instance.LastReloadedTimeStamp.ContainsKey(weapon.ItemId))
                    {
                        if (DateTime.UtcNow < Cache.Instance.LastReloadedTimeStamp[weapon.ItemId].AddSeconds(Time.Instance.ReloadWeaponDelayBeforeUsable_seconds))
                        {
                            if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "Weapon [" + _weaponNumber + "] was just reloaded [" + Math.Round(DateTime.UtcNow.Subtract(Cache.Instance.LastReloadedTimeStamp[weapon.ItemId]).TotalSeconds, 0) + "] seconds ago , moving on to next weapon", Logging.White);
                            continue;
                        }    
                    }
                    
                    if (Cache.Instance.CurrentShipsCargo != null && Cache.Instance.CurrentShipsCargo.Items.Any())
                    {
                        if (!ReloadAmmo(weapon, entity, _weaponNumber)) continue; //by returning false here we make sure we only reload one gun (or stack) per iteration (basically per second)    
                    }

                    return false;
                }

                if (Settings.Instance.DebugReloadAll) Logging.Log("debug ReloadAll", "completely reloaded all weapons", Logging.White);
                _reloadAllIteration = 0;
                return true;
            }
            
            _reloadAllIteration = 0;
            return true;
        }

        /// <summary> Returns true if it can activate the weapon on the target
        /// </summary>
        /// <remarks>
        ///   The idea behind this function is that a target that explodes is not being fired on within 5 seconds
        /// </remarks>
        /// <param name = "module"></param>
        /// <param name = "entity"></param>
        /// <param name = "isWeapon"></param>
        /// <returns></returns>
        private static bool CanActivate(ModuleCache module, EntityCache entity, bool isWeapon)
        {
            if (!module.IsOnline)
            {
                return false;
            }

            if (module.IsActive || !module.IsActivatable)
            {
                return false;
            }

            if (isWeapon && !entity.IsTarget)
            {
                Logging.Log("Combat.CanActivate", "We attempted to shoot [" + entity.Name + "][" + Math.Round(entity.Distance/1000, 2) + "] which is currently not locked!", Logging.Debug);
                return false;
            }

            if (isWeapon && entity.Distance > Cache.Instance.MaxRange)
            {
                Logging.Log("Combat.CanActivate", "We attempted to shoot [" + entity.Name + "][" + Math.Round(entity.Distance / 1000, 2) + "] which is out of weapons range!", Logging.Debug);
                return false;
            }

            if (module.IsReloadingAmmo)
                return false;

            if (module.IsChangingAmmo)
                return false;

            if (module.IsDeactivating)
                return false;

            // We have changed target, allow activation
            if (entity.Id != module.LastTargetId)
                return true;

            // We have reloaded, allow activation
            if (isWeapon && module.CurrentCharges == MaxCharges)
                return true;

            // if the module is not already active, we have a target, it is in range, we are not reloading then ffs shoot it... 
            return true;
        }

        /// <summary> Activate weapons
        /// </summary>
        private static void ActivateWeapons(EntityCache target)
        {
            // When in warp there's nothing we can do, so ignore everything
            if (Cache.Instance.InSpace && Cache.Instance.InWarp)
            {
                if (Cache.Instance.PrimaryWeaponPriorityEntities != null && Cache.Instance.PrimaryWeaponPriorityEntities.Any())
                {
                    Cache.Instance.RemovePrimaryWeaponPriorityTargets(Cache.Instance.PrimaryWeaponPriorityEntities.ToList());
                }

                if (Cache.Instance.UseDrones && Cache.Instance.DronePriorityEntities != null && Cache.Instance.DronePriorityEntities.Any())
                {
                    Cache.Instance.RemoveDronePriorityTargets(Cache.Instance.DronePriorityEntities.ToList());
                }

                Cache.Instance.ClearPerPocketCache();
                if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: we are in warp! doing nothing", Logging.Teal);
                return;
            }

            if (!Cache.Instance.Weapons.Any())
            {
                if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: you have no weapons?", Logging.Teal);
                return;
            }

            //
            // Do we really want a non-mission action moving the ship around at all!! (other than speed tanking)?
            // If you are not in a mission by all means let combat actions move you around as needed
            /*
            if (!Cache.Instance.InMission)
            {
                if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: we are NOT in a mission: NavigateInToRange", Logging.Teal);
                NavigateOnGrid.NavigateIntoRange(target, "Combat");
            }
            if (Settings.Instance.SpeedTank)
            {
                if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: We are Speed Tanking: NavigateInToRange", Logging.Teal);
                NavigateOnGrid.NavigateIntoRange(target, "Combat");
            }
            */
            if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: after navigate into range...", Logging.Teal);

            // Get the weapons
            
            // TODO: Add check to see if there is better ammo to use! :)
            // Get distance of the target and compare that with the ammo currently loaded

            //Deactivate weapons that needs to be deactivated for this list of reasons...
            _weaponNumber = 0;
            if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: Do we need to deactivate any weapons?", Logging.Teal);

            if (Cache.Instance.Weapons.Any())
            {
                foreach (ModuleCache weapon in Cache.Instance.Weapons)
                {
                    _weaponNumber++;
                    if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: for each weapon [" + _weaponNumber + "] in weapons", Logging.Teal);

                    if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(weapon.ItemId))
                    {
                        if (Cache.Instance.LastActivatedTimeStamp[weapon.ItemId].AddMilliseconds(Time.Instance.WeaponDelay_milliseconds) > DateTime.UtcNow)
                        {
                            continue;
                        }
                    }

                    if (!weapon.IsActive)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: [" + weapon.TypeName + "][" + _weaponNumber + "] is not active: no need to do anything", Logging.Teal);
                        continue;
                    }

                    if (weapon.IsReloadingAmmo)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: [" + weapon.TypeName + "][" + _weaponNumber + "] is reloading ammo: waiting", Logging.Teal);
                        continue;
                    }
                    
                    if (weapon.IsDeactivating)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: [" + weapon.TypeName + "][" + _weaponNumber + "] is deactivating: waiting", Logging.Teal);
                        continue;
                    }
                    
                    if (weapon.IsChangingAmmo)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: [" + weapon.TypeName + "][" + _weaponNumber + "] is changing ammo: waiting", Logging.Teal);
                        continue;
                    }
                    
                    // No ammo loaded
                    if (weapon.Charge == null)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: no ammo loaded? [" + weapon.TypeName + "][" + _weaponNumber + "] reload will happen elsewhere", Logging.Teal);
                        continue;
                    }

                    Ammo ammo = Settings.Instance.Ammo.FirstOrDefault(a => a.TypeId == weapon.Charge.TypeId);

                    //use mission specific ammo
                    if (Cache.Instance.MissionAmmo.Count() != 0)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: MissionAmmocount is not 0", Logging.Teal);
                        ammo = Cache.Instance.MissionAmmo.FirstOrDefault(a => a.TypeId == weapon.Charge.TypeId);
                    }

                    // How can this happen? Someone manually loaded ammo
                    if (ammo == null)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: ammo == null [" + weapon.TypeName + "][" + _weaponNumber + "] someone manually loaded ammo?", Logging.Teal);
                        continue;
                    }

                    if (weapon.CurrentCharges >= 2)
                    {
                        // If we have already activated warp, deactivate the weapons
                        if (!Cache.Instance.ActiveShip.Entity.IsWarping)
                        {
                            // Target is in range
                            if (target.Distance <= ammo.Range)
                            {
                                if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: target is in range: do nothing, wait until it is dead", Logging.Teal);
                                continue;
                            }
                        }
                    }

                    // Target is out of range, stop firing
                    if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: deactivate: target is out of range, stop firing", Logging.Teal);
                    if (weapon.Click()) return;
                    return;
                }

                // Hack for max charges returning incorrect value
                if (!Cache.Instance.Weapons.Any(w => w.IsEnergyWeapon))
                {
                    MaxCharges = Math.Max(MaxCharges, Cache.Instance.Weapons.Max(l => l.MaxCharges));
                    MaxCharges = Math.Max(MaxCharges, Cache.Instance.Weapons.Max(l => l.CurrentCharges));
                }

                int weaponsActivatedThisTick = 0;
                int weaponsToActivateThisTick = Cache.Instance.RandomNumber(1, 4);

                // Activate the weapons (it not yet activated)))
                _weaponNumber = 0;
                if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: activate: Do we need to activate any weapons?", Logging.Teal);
                foreach (ModuleCache weapon in Cache.Instance.Weapons)
                {
                    _weaponNumber++;

                    if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(weapon.ItemId))
                    {
                        if (Cache.Instance.LastActivatedTimeStamp[weapon.ItemId].AddMilliseconds(Time.Instance.WeaponDelay_milliseconds) > DateTime.UtcNow)
                        {
                            continue;
                        }
                    }

                    // Are we reloading, deactivating or changing ammo?
                    if (weapon.IsReloadingAmmo)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] is reloading, waiting.", Logging.Teal);
                        continue;
                    }

                    if (weapon.IsDeactivating)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] is deactivating, waiting.", Logging.Teal);
                        continue;
                    }

                    if (weapon.IsChangingAmmo)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] is changing ammo, waiting.", Logging.Teal);
                        continue;
                    }

                    if (!target.IsTarget)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] is [" + target.Name + "] is not locked, waiting.", Logging.Teal);
                        continue;
                    }

                    // Are we on the right target?
                    if (weapon.IsActive)
                    {
                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] is active already", Logging.Teal);
                        if (weapon.TargetId != target.Id && target.IsTarget)
                        {
                            if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] is shooting at the wrong target: deactivating", Logging.Teal);
                            if (weapon.Click()) return;

                            return;
                        }
                        continue;
                    }

                    // No, check ammo type and if that is correct, activate weapon
                    if (ReloadAmmo(weapon, target, _weaponNumber) && CanActivate(weapon, target, true))
                    {
                        if (weaponsActivatedThisTick > weaponsToActivateThisTick)
                        {
                            if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: if we have already activated x number of weapons return, which will wait until the next ProcessState", Logging.Teal);
                            return;
                        }

                        if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: Activate: [" + weapon.TypeName + "][" + _weaponNumber + "] has the correct ammo: activate", Logging.Teal);
                        if (weapon.Activate(target))
                        {
                            weaponsActivatedThisTick++; //increment the number of weapons we have activated this ProcessState so that we might optionally activate more than one module per tick
                            Logging.Log("Combat", "Activating weapon  [" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.Teal);
                            continue;
                        }
                        
                        continue;
                    }

                    if (Settings.Instance.DebugActivateWeapons) Logging.Log("Combat", "ActivateWeapons: ReloadReady [" + ReloadAmmo(weapon, target, _weaponNumber) + "] CanActivateReady [" + CanActivate(weapon, target, true) + "]", Logging.Teal);
                }
            }
            else
            {
                Logging.Log("Combat","ActivateWeapons: you have no weapons with groupID: [ " + Settings.Instance.WeaponGroupId + " ]",Logging.Debug);
                i = 0;
                foreach (ModuleCache __module in Cache.Instance.Modules.Where(e => e.IsOnline && e.IsActivatable))
                {
                    i++;
                    Logging.Log("Fitted Modules", "[" + i + "] Module TypeID [ " + __module.TypeId + " ] ModuleGroupID [ " + __module.GroupId + " ] EveCentral Link [ http://eve-central.com/home/quicklook.html?typeid=" + __module.TypeId + " ]", Logging.Debug);
                }
            }
        }

        /// <summary> Activate target painters
        /// </summary>
        private static void ActivateTargetPainters(EntityCache target)
        {
            List<ModuleCache> targetPainters = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.TargetPainter).ToList();

            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            foreach (ModuleCache painter in targetPainters)
            {
                if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(painter.ItemId))
                {
                    if (Cache.Instance.LastActivatedTimeStamp[painter.ItemId].AddMilliseconds(Time.Instance.PainterDelay_milliseconds) > DateTime.UtcNow)
                    {
                        continue;
                    }    
                }
                
                _weaponNumber++;

                // Are we on the right target?
                if (painter.IsActive)
                {
                    if (painter.TargetId != target.Id)
                    {
                        if (painter.Click()) return;

                        return;
                    }

                    continue;
                }

                // Are we deactivating?
                if (painter.IsDeactivating)
                    continue;

                if (CanActivate(painter, target, false))
                {
                    if (painter.Activate(target))
                    {
                        Logging.Log("Combat", "Activating [" + painter.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.Teal);
                        return;
                    }

                    continue;
                }
            }
        }

        /// <summary> Activate target painters
        /// </summary>
        private static void ActivateSensorDampeners(EntityCache target)
        {
            List<ModuleCache> sensorDampeners = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.SensorDampener).ToList();

            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            foreach (ModuleCache sensorDampener in sensorDampeners)
            {
                if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(sensorDampener.ItemId))
                {
                    if (Cache.Instance.LastActivatedTimeStamp[sensorDampener.ItemId].AddMilliseconds(Time.Instance.PainterDelay_milliseconds) > DateTime.UtcNow)
                    {
                        continue;
                    }
                }

                _weaponNumber++;

                // Are we on the right target?
                if (sensorDampener.IsActive)
                {
                    if (sensorDampener.TargetId != target.Id)
                    {
                        if (sensorDampener.Click()) return;
                        return;
                    }

                    continue;
                }

                // Are we deactivating?
                if (sensorDampener.IsDeactivating)
                    continue;

                if (CanActivate(sensorDampener, target, false))
                {
                    if (sensorDampener.Activate(target))
                    {
                        Logging.Log("Combat", "Activating [" + sensorDampener.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.Teal);
                        return;    
                    }

                    continue;
                }
            }
        }

        /// <summary> Activate Nos
        /// </summary>
        private static void ActivateNos(EntityCache target)
        {
            List<ModuleCache> noses = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.NOS || m.GroupId == (int)Group.Neutralizer).ToList();

            //Logging.Log("Combat: we have " + noses.Count.ToString() + " Nos modules");
            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            foreach (ModuleCache nos in noses)
            {
                _weaponNumber++;

                if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(nos.ItemId))
                {
                    if (Cache.Instance.LastActivatedTimeStamp[nos.ItemId].AddMilliseconds(Time.Instance.NosDelay_milliseconds) > DateTime.UtcNow)
                    {
                        continue;
                    }
                }

                // Are we on the right target?
                if (nos.IsActive)
                {
                    if (nos.TargetId != target.Id)
                    {
                        if (nos.Click()) return;

                        return;
                    }

                    continue;
                }

                // Are we deactivating?
                if (nos.IsDeactivating)
                    continue;

                //Logging.Log("Combat: Distances Target[ " + Math.Round(target.Distance,0) + " Optimal[" + nos.OptimalRange.ToString()+"]");
                // Target is out of Nos range
                if (target.Distance >= nos.MaxRange)
                    continue;

                if (CanActivate(nos, target, false))
                {
                    if (nos.Activate(target))
                    {
                        Logging.Log("Combat", "Activating [" + nos.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.Teal);
                        return;    
                    }

                    continue;
                }

                Logging.Log("Combat", "Cannot Activate [" + nos.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.Teal);
            }
        }

        /// <summary> Activate StasisWeb
        /// </summary>
        private static void ActivateStasisWeb(EntityCache target)
        {
            List<ModuleCache> webs = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.StasisWeb).ToList();

            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            foreach (ModuleCache web in webs)
            {
                _weaponNumber++;

                if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(web.ItemId))
                {
                    if (Cache.Instance.LastActivatedTimeStamp[web.ItemId].AddMilliseconds(Time.Instance.WebDelay_milliseconds) > DateTime.UtcNow)
                    {
                        continue;
                    }
                }

                // Are we on the right target?
                if (web.IsActive)
                {
                    if (web.TargetId != target.Id)
                    {
                        if (web.Click()) return;
                        
                        return;
                    }

                    continue;
                }

                // Are we deactivating?
                if (web.IsDeactivating)
                    continue;

                // Target is out of web range
                if (target.Distance >= web.OptimalRange)
                    continue;

                if (CanActivate(web, target, false))
                {
                    if (web.Activate(target))
                    {
                        Logging.Log("Combat", "Activating [" + web.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "]", Logging.Teal);
                        return;    
                    }

                    continue;
                }
            }
        }

        public static bool ActivateBastion(bool activate = false)
        {
            List<ModuleCache> bastionModules = null;
            bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
            if (!bastionModules.Any()) return true;
            if (bastionModules.Any(i => i.IsActive && i.IsDeactivating)) return true;

            if (!Cache.Instance.PotentialCombatTargets.Where(e => e.Distance < Cache.Instance.WeaponRange).Any(e => e.IsTarget || e.IsTargeting) && CombatMissionCtrl.DeactivateIfNothingTargetedWithinRange)
            {
                if (Settings.Instance.DebugActivateBastion) Logging.Log("ActivateBastion", "NextBastionModeDeactivate set to 2 sec ago: We have no targets in range and DeactivateIfNothingTargetedWithinRange [ " + CombatMissionCtrl.DeactivateIfNothingTargetedWithinRange + " ]", Logging.Debug);
                Cache.Instance.NextBastionModeDeactivate = DateTime.UtcNow.AddSeconds(-2);
            }

            if (_States.CurrentPanicState == PanicState.Panicking || _States.CurrentPanicState == PanicState.StartPanicking)
            {
                if (Settings.Instance.DebugActivateBastion) Logging.Log("ActivateBastion", "NextBastionModeDeactivate set to 2 sec ago: We are in panic!", Logging.Debug);
                Cache.Instance.NextBastionModeDeactivate = DateTime.UtcNow.AddSeconds(-2);
            }

            if (DateTime.UtcNow < Cache.Instance.NextBastionAction)
            {
                if (Settings.Instance.DebugActivateBastion) Logging.Log("ActivateBastion", "NextBastionAction [" + Cache.Instance.NextBastionAction.Subtract(DateTime.UtcNow).TotalSeconds + "] seconds, waiting...", Logging.Debug);
                return false;
            }

            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            foreach (ModuleCache bastionMod in bastionModules)
            {
                _weaponNumber++;

                if (Settings.Instance.DebugActivateBastion) Logging.Log("ActivateBastion", "[" + _weaponNumber + "] BastionModule: IsActive [" + bastionMod.IsActive + "] IsDeactivating [" + bastionMod.IsDeactivating + "] InLimboState [" + bastionMod.InLimboState + "] Duration [" + bastionMod.Duration + "] TypeId [" + bastionMod.TypeId + "]", Logging.Debug);

                //
                // Deactivate (if needed)
                //
                // Are we on the right target?
                if (bastionMod.IsActive && !bastionMod.IsDeactivating)
                {
                    if (Settings.Instance.DebugActivateBastion) Logging.Log("ActivateBastion", "IsActive and Is not yet deactivating (we only want one cycle), attempting to Click...", Logging.Debug);
                    if (bastionMod.Click()) return true;
                    return false;
                }

                if (bastionMod.IsActive)
                {
                    if (Settings.Instance.DebugActivateBastion) Logging.Log("ActivateBastion", "IsActive: assuming it is deactivating on the next cycle.", Logging.Debug);
                    return true;
                }

                //
                // Activate (if needed)
                //

                // Are we deactivating?
                if (bastionMod.IsDeactivating)
                    continue;

                if (!bastionMod.IsActive)
                {
                    Logging.Log("Combat", "Activating bastion [" + _weaponNumber + "]", Logging.Teal);
                    if (bastionMod.Click())
                    {
                        Cache.Instance.NextBastionAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 20));
                        return true;        
                    }

                    return false;
                }
            }

            return true; //if we got  this far we have done all we can do.
        }

        private static void ActivateWarpDisruptor(EntityCache target)
        {
            List<ModuleCache> WarpDisruptors = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.WarpDisruptor).ToList();

            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            foreach (ModuleCache WarpDisruptor in WarpDisruptors)
            {
                _weaponNumber++;

                if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(WarpDisruptor.ItemId))
                {
                    if (Cache.Instance.LastActivatedTimeStamp[WarpDisruptor.ItemId].AddMilliseconds(Time.Instance.WebDelay_milliseconds) > DateTime.UtcNow)
                    {
                        continue;
                    }
                }

                // Are we on the right target?
                if (WarpDisruptor.IsActive)
                {
                    if (WarpDisruptor.TargetId != target.Id)
                    {
                        if (WarpDisruptor.Click()) return;

                        return;
                    }

                    continue;
                }

                // Are we deactivating?
                if (WarpDisruptor.IsDeactivating)
                    continue;

                // Target is out of web range
                if (target.Distance >= WarpDisruptor.OptimalRange)
                    continue;

                if (CanActivate(WarpDisruptor, target, false))
                {
                    if (WarpDisruptor.Activate(target))
                    {
                        Logging.Log("Combat", "Activating [" + WarpDisruptor.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "]", Logging.Teal);
                        return;
                    }

                    continue;
                }
            }
        }

        private static void ActivateRemoteRepair(EntityCache target)
        {
            List<ModuleCache> RemoteRepairers = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.RemoteArmorRepairer 
                                                                               || m.GroupId == (int)Group.RemoteShieldRepairer 
                                                                               || m.GroupId == (int)Group.RemoteHullRepairer
                                                                               ).ToList();

            // Find the first active weapon
            // Assist this weapon
            _weaponNumber = 0;
            if (RemoteRepairers.Any())
            {
                if (Settings.Instance.DebugRemoteRepair) Logging.Log("ActivateRemoteRepair", "RemoteRepairers [" + RemoteRepairers.Count() + "] Target Distance [" + Math.Round(target.Distance / 1000, 0) + "] RemoteRepairDistance [" + Math.Round(((double)Settings.Instance.RemoteRepairDistance / 1000), digits: 0) + "]", Logging.Debug);
                foreach (ModuleCache RemoteRepairer in RemoteRepairers)
                {
                    _weaponNumber++;

                    if (Cache.Instance.LastActivatedTimeStamp != null && Cache.Instance.LastActivatedTimeStamp.ContainsKey(RemoteRepairer.ItemId))
                    {
                        if (Cache.Instance.LastActivatedTimeStamp[RemoteRepairer.ItemId].AddMilliseconds(Time.Instance.RemoteRepairerDelay_milliseconds) > DateTime.UtcNow)
                        {
                            continue;
                        }
                    }

                    // Are we on the right target?
                    if (RemoteRepairer.IsActive)
                    {
                        if (RemoteRepairer.TargetId != target.Id)
                        {
                            if (RemoteRepairer.Click()) return;

                            return;
                        }

                        continue;
                    }

                    // Are we deactivating?
                    if (RemoteRepairer.IsDeactivating)
                        continue;

                    // Target is out of RemoteRepair range
                    if (target.Distance >= RemoteRepairer.MaxRange)
                        continue;

                    if (CanActivate(RemoteRepairer, target, false))
                    {
                        if (RemoteRepairer.Activate(target))
                        {
                            Logging.Log("Combat", "Activating [" + RemoteRepairer.TypeName + "][" + _weaponNumber + "] on [" + target.Name + "][" + Cache.Instance.MaskedID(target.Id) + "]", Logging.Teal);
                            return;    
                        }

                        continue;
                    }
                }
            }
            
        }

        private static bool UnlockHighValueTarget(string module, string reason, bool OutOfRangeOnly = false)
        {
            EntityCache unlockThisHighValueTarget = null;
            long preferredId = Cache.Instance.PreferredPrimaryWeaponTarget != null ? Cache.Instance.PreferredPrimaryWeaponTarget.Id : -1;
                    
            if (!OutOfRangeOnly)
            {
                if (__lowValueTargetsTargeted.Count() > maxLowValueTargets && maxTotalTargets <= __lowValueTargetsTargeted.Count() + __highValueTargetsTargeted.Count())
                {
                    return UnlockLowValueTarget(module, reason, OutOfRangeOnly);    // We are using HighValueSlots for lowvaluetarget (which is ok)
                                                                                    // but we now need 1 slot back to target our PreferredTarget
                }

                try
                {
                    if (__highValueTargetsTargeted.Count(t => t.Id != preferredId) >= maxHighValueTargets)
                    {
                        //unlockThisHighValueTarget = Cache.Instance.GetBestWeaponTargets((double)Distances.OnGridWithMe).Where(t => t.IsTarget && highValueTargetsTargeted.Any(e => t.Id == e.Id)).LastOrDefault();

                        unlockThisHighValueTarget = __highValueTargetsTargeted.Where(h =>  (h.IsTarget && h.IsIgnored)
                                                                                        || (h.IsTarget && (!h.isPreferredDroneTarget && !h.IsDronePriorityTarget && !h.isPreferredPrimaryWeaponTarget && !h.IsPrimaryWeaponPriorityTarget && !h.IsPriorityWarpScrambler && !h.IsInOptimalRange && Cache.Instance.PotentialCombatTargets.Count() >= 3))
                                                                                        || (h.IsTarget && (!h.isPreferredPrimaryWeaponTarget && !h.IsDronePriorityTarget && h.IsHigherPriorityPresent && !h.IsPrimaryWeaponPriorityTarget && __highValueTargetsTargeted.Count() == maxHighValueTargets) && !h.IsPriorityWarpScrambler))
                                                                                        .OrderByDescending(t => t.Distance > Cache.Instance.MaxRange)
                                                                                        .ThenByDescending(t => t.Distance)
                                                                                        .FirstOrDefault();
                    }
                } 
                catch (NullReferenceException) { }

            }
            else
            {
                try
                {
                    unlockThisHighValueTarget = __highValueTargetsTargeted.Where(h => h.IsTarget && h.IsIgnored && !h.IsPriorityWarpScrambler)
                                                                        .OrderByDescending(t => t.Distance > Cache.Instance.MaxRange)
                                                                        .ThenByDescending(t => t.Distance)
                                                                        .FirstOrDefault();
                }
                catch (NullReferenceException) { }
            }
                
            if (unlockThisHighValueTarget != null)
            {
                Logging.Log("Combat [TargetCombatants]" + module, "Unlocking HighValue " + unlockThisHighValueTarget.Name + "[" + Math.Round(unlockThisHighValueTarget.Distance/1000,0) + "k] myTargtingRange:[" + Cache.Instance.MaxTargetRange + "] myWeaponRange[:" + Cache.Instance.WeaponRange + "] to make room for [" + reason + "]", Logging.Orange);
                unlockThisHighValueTarget.UnlockTarget("Combat [TargetCombatants]");
                //Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                return false;
            }

            if (!OutOfRangeOnly)
            {
                //Logging.Log("Combat [TargetCombatants]" + module, "We don't have a spot open to target [" + reason + "], this could be a problem", Logging.Orange);
                //Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);    
            }

            return true;
            
        }

        private static bool UnlockLowValueTarget(string module, string reason, bool OutOfWeaponsRange = false)
        {
            EntityCache unlockThisLowValueTarget = null;
            if (!OutOfWeaponsRange)
            {
                try
                {
                    unlockThisLowValueTarget = __lowValueTargetsTargeted.Where(h => (h.IsTarget && h.IsIgnored)
                                                                                 || (h.IsTarget && (!h.isPreferredDroneTarget && !h.IsDronePriorityTarget && !h.isPreferredPrimaryWeaponTarget && !h.IsPrimaryWeaponPriorityTarget && !h.IsPriorityWarpScrambler && !h.IsInOptimalRange && Cache.Instance.PotentialCombatTargets.Count() >= 3))
                                                                                 || (h.IsTarget && (!h.isPreferredDroneTarget && !h.IsDronePriorityTarget && !h.isPreferredPrimaryWeaponTarget && !h.IsPrimaryWeaponPriorityTarget && !h.IsPriorityWarpScrambler && __lowValueTargetsTargeted.Count() == maxLowValueTargets))
                                                                                 || (h.IsTarget && (!h.isPreferredDroneTarget && !h.IsDronePriorityTarget && !h.isPreferredPrimaryWeaponTarget && !h.IsPrimaryWeaponPriorityTarget && h.IsHigherPriorityPresent && !h.IsPriorityWarpScrambler && __lowValueTargetsTargeted.Count() == maxLowValueTargets)))
                                                                                 .OrderByDescending(t => t.Distance < (Cache.Instance.UseDrones ? Cache.Instance.MaxDroneRange : Cache.Instance.WeaponRange))
                                                                                .ThenByDescending(t => t.Nearest5kDistance)
                                                                                .FirstOrDefault();
                }
                catch (NullReferenceException) { }
            }
            else
            {
                try
                {
                    unlockThisLowValueTarget = __lowValueTargetsTargeted.Where(h => (h.IsTarget && h.IsIgnored)
                                                                                 || (h.IsTarget && (!h.isPreferredDroneTarget && !h.IsDronePriorityTarget && !h.isPreferredPrimaryWeaponTarget  && !h.IsPrimaryWeaponPriorityTarget && h.IsHigherPriorityPresent && !h.IsPriorityWarpScrambler && !h.IsReadyToShoot  && __lowValueTargetsTargeted.Count() == maxLowValueTargets)))
                                                                                 .OrderByDescending(t => t.Distance < (Cache.Instance.UseDrones ? Cache.Instance.MaxDroneRange : Cache.Instance.WeaponRange))
                                                                                 .ThenByDescending(t => t.Nearest5kDistance)
                                                                                 .FirstOrDefault();
                }
                catch (NullReferenceException) { }
            }

            if (unlockThisLowValueTarget != null)
            {
                Logging.Log("Combat [TargetCombatants]" + module, "Unlocking LowValue " + unlockThisLowValueTarget.Name + "[" + Math.Round(unlockThisLowValueTarget.Distance / 1000, 0) + "k] myTargtingRange:[" + Cache.Instance.MaxTargetRange + "] myWeaponRange[:" + Cache.Instance.WeaponRange + "] to make room for [" + reason + "]", Logging.Orange);
                unlockThisLowValueTarget.UnlockTarget("Combat [TargetCombatants]");
                //Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                return false;
            }

            if (!OutOfWeaponsRange)
            {
                //Logging.Log("Combat [TargetCombatants]" + module, "We don't have a spot open to target [" + reason + "], this could be a problem", Logging.Orange);
                //Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);    
            }

            return true;
        }

        private static void TargetCombatants2()
        {
            if ( DateTime.UtcNow < Cache.Instance.NextTargetAction //if we just did something wait a fraction of a second
              || Settings.Instance.DebugDisableTargetCombatants)
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("InSpace [ " + Cache.Instance.InSpace + " ] InWarp [ " + Cache.Instance.InWarp + " ] InStation [ " + Cache.Instance.InStation + " ] NextTargetAction [ " + Cache.Instance.NextTargetAction.Subtract(DateTime.UtcNow).TotalSeconds + " seconds] DebugDisableTargetCombatants [ " + Settings.Instance.DebugDisableTargetCombatants + " ]", "", Logging.Debug);
                return;
            }

            maxLowValueTargets = Settings.Instance.MaximumLowValueTargets;
            maxHighValueTargets = Settings.Instance.MaximumHighValueTargets;
            

            #region ECM Jamming checks
            //
            // First, can we even target?
            // We are ECM'd / jammed, forget targeting anything...
            //
            if (Cache.Instance.MaxLockedTargets == 0)
            {
                if (!_isJammed)
                {
                    Logging.Log("Combat", "We are jammed and can not target anything", Logging.Orange);
                }

                _isJammed = true;
                return;
            }

            if (_isJammed)
            {
                // Clear targeting list as it does not apply
                Cache.Instance.TargetingIDs.Clear();
                Logging.Log("Combat", "We are no longer jammed, reTargeting", Logging.Teal);
            }

            _isJammed = false;
            #endregion

            #region Current active targets/targeting
            //
            // What do we currently have targeted?
            // Get our current targets/targeting
            //

            // Get lists of the current high and low value targets
            try
            {
                __highValueTargets = Cache.Instance.EntitiesOnGrid.Where(t => t.CategoryId != (int)CategoryID.Asteroid && t.CategoryId == (int)CategoryID.Entity && (t.IsHighValueTarget)).ToList();
                __highValueTargetsTargeted = __highValueTargets.Where(i => i.IsTarget || i.IsTargeting);
            }
            catch (NullReferenceException) { }

            int __highValueTargetsTargetedCount = 0;
            if (__highValueTargetsTargeted.Any())
            {
                __highValueTargetsTargetedCount = __highValueTargetsTargeted.Count();
            }

            try
            {
                __lowValueTargets = Cache.Instance.EntitiesOnGrid.Where(t => t.CategoryId != (int)CategoryID.Asteroid && t.CategoryId == (int)CategoryID.Entity && (t.IsLowValueTarget)).ToList();
                __lowValueTargetsTargeted = __lowValueTargets.Where(i => i.IsTarget || i.IsTargeting);
            }
            catch (NullReferenceException) { }

            int __lowValueTargetsTargetedCount = 0;
            if (__lowValueTargetsTargeted.Any())
            {
                __lowValueTargetsTargetedCount = __lowValueTargetsTargeted.Count();
            }

            int targetsTargeted = __highValueTargetsTargetedCount + __lowValueTargetsTargetedCount;
            #endregion

            #region Targeting using priority
            if (Cache.Instance.EntitiesOnGrid.Any())
            {
                IEnumerable<EntityCache> primaryWeaponTargetsToLock = Cache.Instance.__GetBestWeaponTargets((double) Distances.OnGridWithMe).Take(maxHighValueTargets); //.ToList();
                int primaryWeaponTargetsToTargetCount = 0;
                if (primaryWeaponTargetsToLock.Any())
                {
                    primaryWeaponTargetsToTargetCount = primaryWeaponTargetsToLock.Count();
                }
                else
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "!primaryWeaponTargetsToTarget.Any()", Logging.Debug);
                }

                IEnumerable<EntityCache> droneTargetsToLock = null;
                int droneTargetsToTargetCount = 0;
                if (Cache.Instance.UseDrones)
                {
                    droneTargetsToLock = Cache.Instance.__GetBestDroneTargets((double)Distances.OnGridWithMe).Take(maxLowValueTargets); //.ToList();
                    //droneTargetsToLock = droneTargetsToLock.ToList();
                    if (droneTargetsToLock.Any())
                    {
                        droneTargetsToTargetCount = droneTargetsToLock.Count();
                    }
                    else
                    {
                        //if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "!primaryWeaponTargetsToTarget.Any()", Logging.Debug);
                    }    
                }
                else
                {
                    droneTargetsToLock = primaryWeaponTargetsToLock;
                    //droneTargetsToLock = droneTargetsToLock.ToList();
                }

                IEnumerable<EntityCache> activeTargets = Cache.Instance.EntitiesOnGrid.Where(e => (e.IsTarget && !e.HasExploded)); //.ToList();
                int activeTargetsCount = 0;
                if (activeTargets.Any())
                {
                    activeTargetsCount = activeTargets.Count();
                }

                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "primaryWeaponTargetsToTarget [" + primaryWeaponTargetsToTargetCount + "] droneTargetsToTarget [" + droneTargetsToTargetCount + "] activeTargets [" + activeTargetsCount + "]", Logging.Debug);

                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "UnTarget stuff (if needed)", Logging.Debug);

                foreach (EntityCache target in activeTargets)
                {
                    if (primaryWeaponTargetsToLock.All(e => e.Id != target.Id) && droneTargetsToLock.All(e => e.Id != target.Id) && !target.IsContainer)
                    {
                        Logging.Log("Combat", "Target [" + target.Name + "] does not need to be shot at the moment, unLocking", Logging.Green);
                        target.UnlockTarget("Combat");
                        return;
                    }
                }

                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "Target a weapon target)", Logging.Debug);
                EntityCache toTarget = primaryWeaponTargetsToLock.FirstOrDefault(e => !e.IsTarget && !e.IsTargeting && e.Distance < Cache.Instance.MaxTargetRange);
                if ((!__highValueTargets.Any() || __highValueTargetsTargetedCount >= maxHighValueTargets) && (__lowValueTargets.Any() || __lowValueTargetsTargetedCount <= maxHighValueTargets))
                {
                    // If we targeted all highValueTargets already take a lowvaluetarget
                    if (toTarget == null)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "if (toTarget == null)", Logging.Debug);
                        toTarget = droneTargetsToLock.FirstOrDefault(e => !e.IsTarget && !e.IsTargeting && e.Distance < Cache.Instance.MaxTargetRange);
                    }    
                }

                if (toTarget != null && Cache.Instance.TotalTargetsandTargetingCount < Cache.Instance.TargetingSlotsNotBeingUsedBySalvager)
                {
                    Logging.Log("Combat", "Lock Target [" + toTarget.Name + "][" + Math.Round(toTarget.Distance / 1000, 2) + "k][" + Cache.Instance.MaskedID(toTarget.Id) + "] PreferredPWPT [" + Cache.Instance.MaskedID(Cache.Instance.PreferredPrimaryWeaponTargetID) + "] PreferedDPT [" + Cache.Instance.MaskedID(Cache.Instance.PreferredDroneTargetID) + "]", Logging.Green);
                    toTarget.LockTarget("Combat");
                    return;
                }
                else
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("TargetCombatants2", "We have [" + Cache.Instance.TotalTargetsandTargetingCount + "] Locked Targets and a Max Number Of Total Targets of [" + Cache.Instance.MaxLockedTargets + "]", Logging.Debug);
                }
            }
            #endregion
        }
        /// <summary> Target combatants
        /// </summary>
        private static void TargetCombatants()
        {
            
            if ((Cache.Instance.InSpace && Cache.Instance.InWarp) // When in warp we should not try to target anything
                    || Cache.Instance.InStation //How can we target if we are in a station?
                    || DateTime.UtcNow < Cache.Instance.NextTargetAction //if we just did something wait a fraction of a second
                    //|| !Cache.Instance.OpenCargoHold("Combat.TargetCombatants") //If we can't open our cargohold then something MUST be wrong
                    || Settings.Instance.DebugDisableTargetCombatants
                )
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("InSpace [ " + Cache.Instance.InSpace + " ] InWarp [ " + Cache.Instance.InWarp + " ] InStation [ " + Cache.Instance.InStation + " ] NextTargetAction [ " + Cache.Instance.NextTargetAction.Subtract(DateTime.UtcNow).TotalSeconds + " seconds] DebugDisableTargetCombatants [ " + Settings.Instance.DebugDisableTargetCombatants + " ]", "", Logging.Debug);
                return;
            }

            #region ECM Jamming checks
            //
            // First, can we even target?
            // We are ECM'd / jammed, forget targeting anything...
            //
            if (Cache.Instance.MaxLockedTargets == 0)
            {
                if (!_isJammed)
                {
                    Logging.Log("Combat", "We are jammed and can not target anything", Logging.Orange);
                }

                _isJammed = true;
                return;
            }

            if (_isJammed)
            {
                // Clear targeting list as it does not apply
                Cache.Instance.TargetingIDs.Clear();
                Logging.Log("Combat", "We are no longer jammed, reTargeting", Logging.Teal);
            }

            _isJammed = false;
            #endregion

            #region Current active targets/targeting
            //
            // What do we currently have targeted?
            // Get our current targets/targeting
            //
            
            // Get lists of the current high and low value targets
            try
            {
                __highValueTargetsTargeted = Cache.Instance.EntitiesOnGrid.Where(t => (t.IsTarget || t.IsTargeting) && (t.IsHighValueTarget)).ToList();
            }
            catch (NullReferenceException) { }

            try
            {
                __lowValueTargetsTargeted = Cache.Instance.EntitiesOnGrid.Where(t => (t.IsTarget || t.IsTargeting) && (t.IsLowValueTarget)).ToList();
            }
            catch (NullReferenceException) { }

            int targetsTargeted = __highValueTargetsTargeted.Count() + __lowValueTargetsTargeted.Count();
            #endregion 

            #region Remove any target that is out of range (lower of Weapon Range or targeting range, definitely matters if damped)
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Remove any target that is out of range", Logging.Debug);
            //
            // If it is currently out of our weapon range unlock it for now, unless it is one of our preferred targets which should technically only happen during kill type actions
            //
            if (Cache.Instance.Targets.Any() && Cache.Instance.Targets.Count() > 1)
            {
                //
                // unlock low value targets that are out of range or ignored
                //
                if (!UnlockLowValueTarget("Combat.TargetCombatants", "[lowValue]OutOfRange or Ignored", true)) return;
                //
                // unlock high value targets that are out of range or ignored
                //
                if (!UnlockHighValueTarget("Combat.TargetCombatants", "[highValue]OutOfRange or Ignored", true)) return;
            }
            #endregion Remove any target that is too far out of range (Weapon Range)

            #region Priority Target Handling
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Priority Target Handling", Logging.Debug);
            //
            // Now lets deal with the priority targets
            //
            if (Cache.Instance.PrimaryWeaponPriorityEntities.Any())
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "We have [" + Cache.Instance.PrimaryWeaponPriorityEntities.Count() + "] PWPT. We have [" + Cache.Instance.TotalTargetsandTargeting.Count() + "] TargetsAndTargeting. We have [" + Cache.Instance.PrimaryWeaponPriorityEntities.Count(i => i.IsTarget) + "] PWPT that are already targeted", Logging.Debug);
                int PrimaryWeaponsPriorityTargetUnTargeted = Cache.Instance.PrimaryWeaponPriorityEntities.Count() - Cache.Instance.TotalTargetsandTargeting.Count(t => Cache.Instance.PrimaryWeaponPriorityEntities.Contains(t));

                if (PrimaryWeaponsPriorityTargetUnTargeted > 0)
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "if (PrimaryWeaponsPriorityTargetUnTargeted > 0)", Logging.Debug);
                    //
                    // unlock a lower priority entity if needed
                    //
                    if (!UnlockHighValueTarget("Combat.TargetCombatants", "PrimaryWeaponPriorityTargets")) return;

                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "if (!UnlockHighValueTarget(Combat.TargetCombatants, PrimaryWeaponPriorityTargets return;", Logging.Debug);

                    IEnumerable<EntityCache> _primaryWeaponPriorityEntities = Cache.Instance.PrimaryWeaponPriorityEntities.Where(t => t.IsTargetWeCanShootButHaveNotYetTargeted)
                                                                                                                     .OrderByDescending(c => c.IsLastTargetPrimaryWeaponsWereShooting)
                                                                                                                     .ThenBy(c => c.IsInOptimalRange)
                                                                                                                     .ThenBy(c => c.Distance);

                    if (_primaryWeaponPriorityEntities.Any())
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: [" + _primaryWeaponPriorityEntities.Count() + "] primaryWeaponPriority targets", Logging.Debug);

                        foreach (EntityCache primaryWeaponPriorityEntity in _primaryWeaponPriorityEntities)
                        {
                            // Have we reached the limit of high value targets?
                            if (__highValueTargetsTargeted.Count() >= maxHighValueTargets)
                            {
                                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __highValueTargetsTargeted [" + __highValueTargetsTargeted.Count() + "] >= maxHighValueTargets [" + maxHighValueTargets + "]", Logging.Debug);
                                break;
                            }

                            if (primaryWeaponPriorityEntity.Distance < Cache.Instance.MaxRange
                                && primaryWeaponPriorityEntity.IsReadyToTarget)
                            {
                                if (Cache.Instance.TotalTargetsandTargetingCount < Cache.Instance.TargetingSlotsNotBeingUsedBySalvager)
                                {
                                    if (primaryWeaponPriorityEntity.LockTarget("TargetCombatants.PrimaryWeaponPriorityEntity"))
                                    {
                                        Logging.Log("Combat", "Targeting primary weapon priority target [" + primaryWeaponPriorityEntity.Name + "][" + Cache.Instance.MaskedID(primaryWeaponPriorityEntity.Id) + "][" + Math.Round(primaryWeaponPriorityEntity.Distance / 1000, 0) + "k away]", Logging.Teal);
                                        Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                                        if (Cache.Instance.TotalTargetsandTargeting.Any() && (Cache.Instance.TotalTargetsandTargeting.Count() >= Cache.Instance.MaxLockedTargets))
                                        {
                                            Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                                        }

                                        return;
                                    }
                                }

                                if (Cache.Instance.TotalTargetsandTargetingCount >= Cache.Instance.TargetingSlotsNotBeingUsedBySalvager)
                                {
                                    if (__lowValueTargetsTargeted.Any())
                                    {
                                        Combat.UnlockLowValueTarget("TargetCombatants", "PriorityTarget Needs to be targeted");
                                        return;
                                    }

                                    if (__highValueTargetsTargeted.Any())
                                    {
                                        Combat.UnlockHighValueTarget("TargetCombatants", "PriorityTarget Needs to be targeted");
                                        return;
                                    }
                                    
                                    //
                                    // if we have nothing to unlock just continue...
                                    //
                                }
                            }
                            
                            continue;
                        }
                    }
                    else
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: 0 primaryWeaponPriority targets", Logging.Debug);
                    }
                }
            }
            #endregion

            #region Drone Priority Target Handling
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Drone Priority Target Handling", Logging.Debug);
            //
            // Now lets deal with the priority targets
            //
            if (Cache.Instance.DronePriorityTargets.Any())
            {
                int DronesPriorityTargetUnTargeted = Cache.Instance.DronePriorityEntities.Count() - Cache.Instance.TotalTargetsandTargeting.Count(t => Cache.Instance.DronePriorityEntities.Contains(t));

                if (DronesPriorityTargetUnTargeted > 0)
                {
                    if (!UnlockLowValueTarget("Combat.TargetCombatants", "DronePriorityTargets")) return;

                    IEnumerable<EntityCache> _dronePriorityTargets = Cache.Instance.DronePriorityEntities.Where(t => t.IsTargetWeCanShootButHaveNotYetTargeted)
                                                                                                                         .OrderByDescending(c => c.IsInDroneRange)
                                                                                                                         .ThenBy(c => c.IsLastTargetPrimaryWeaponsWereShooting)
                                                                                                                         .ThenBy(c => c.Nearest5kDistance);

                    if (_dronePriorityTargets.Any())
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: [" + _dronePriorityTargets.Count() + "] dronePriority targets", Logging.Debug);

                        foreach (EntityCache dronePriorityEntity in _dronePriorityTargets)
                        {
                            // Have we reached the limit of low value targets?
                            if (__lowValueTargetsTargeted.Count() >= maxLowValueTargets)
                            {
                                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __lowValueTargetsTargeted [" + __lowValueTargetsTargeted.Count() + "] >= maxLowValueTargets [" + maxLowValueTargets + "]", Logging.Debug);
                                break;
                            }

                            if (dronePriorityEntity.Nearest5kDistance < Cache.Instance.MaxDroneRange
                                && dronePriorityEntity.IsReadyToTarget
                                && dronePriorityEntity.Nearest5kDistance < Cache.Instance.LowValueTargetsHaveToBeWithinDistance
                                && !dronePriorityEntity.IsIgnored)
                            {
                                if (Cache.Instance.TotalTargetsandTargetingCount < Cache.Instance.TargetingSlotsNotBeingUsedBySalvager)
                                {
                                    if (dronePriorityEntity.LockTarget("TargetCombatants.PrimaryWeaponPriorityEntity"))
                                    {
                                        Logging.Log("Combat", "Targeting primary weapon priority target [" + dronePriorityEntity.Name + "][" + Cache.Instance.MaskedID(dronePriorityEntity.Id) + "][" + Math.Round(dronePriorityEntity.Distance / 1000, 0) + "k away]", Logging.Teal);
                                        return;
                                    }
                                }

                                if (Cache.Instance.TotalTargetsandTargetingCount >= Cache.Instance.TargetingSlotsNotBeingUsedBySalvager)
                                {
                                    if (__lowValueTargetsTargeted.Any())
                                    {
                                        Combat.UnlockLowValueTarget("TargetCombatants", "PriorityTarget Needs to be targeted");
                                        return;
                                    }

                                    if (__highValueTargetsTargeted.Any())
                                    {
                                        Combat.UnlockHighValueTarget("TargetCombatants", "PriorityTarget Needs to be targeted");
                                        return;
                                    }

                                    Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                                    //
                                    // if we have nothing to unlock just continue...
                                    //
                                }
                            }

                            continue;
                        }
                    }
                    else
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: 0 primaryWeaponPriority targets", Logging.Debug);
                    }
                }
            }
            #endregion

            #region Preferred Primary Weapon target handling
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Preferred Primary Weapon target handling", Logging.Debug);
            //
            // Lets deal with our preferred targets next (in other words what Q is actively trying to shoot or engage drones on)
            //
            
            if (Cache.Instance.PreferredPrimaryWeaponTarget != null)
            {

                if (Cache.Instance.PreferredPrimaryWeaponTarget.IsIgnored)
                {
                    Logging.Log("TargetCombatants", "if (Cache.Instance.PreferredPrimaryWeaponTarget.IsIgnored) Cache.Instance.PreferredPrimaryWeaponTarget = null;", Logging.Red);
                    //Cache.Instance.PreferredPrimaryWeaponTarget = null;
                }

                if (Cache.Instance.PreferredPrimaryWeaponTarget != null)
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("TargetCombatants", "if (Cache.Instance.PreferredPrimaryWeaponTarget != null)", Logging.Debug);
                    if (Cache.Instance.EntitiesOnGrid.Any(e => e.Id == Cache.Instance.PreferredPrimaryWeaponTarget.Id))
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("TargetCombatants", "if (Cache.Instance.Entities.Any(i => i.Id == Cache.Instance.PreferredPrimaryWeaponTarget.Id))", Logging.Debug);
                        
                        if (Settings.Instance.DebugTargetCombatants)
                        {
                            Logging.Log("[" + Cache.Instance.PreferredPrimaryWeaponTarget.Name + "] Distance [" + Math.Round(Cache.Instance.PreferredPrimaryWeaponTarget.Distance / 1000, 0) + "] HasExploded:" + Cache.Instance.PreferredPrimaryWeaponTarget.HasExploded + " IsTarget: [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsTarget + "] IsTargeting: [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsTargeting + "] IsReady [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsReadyToTarget + "]", "", Logging.Debug); 
                        }
                        
                        if (Cache.Instance.PreferredPrimaryWeaponTarget.IsReadyToTarget)
                        {
                            if (Settings.Instance.DebugTargetCombatants) Logging.Log("TargetCombatants", "if (Cache.Instance.PreferredPrimaryWeaponTarget.IsReadyToTarget)", Logging.Debug);
                            if (Cache.Instance.PreferredPrimaryWeaponTarget.Distance <= Cache.Instance.MaxRange)
                            {
                                if (Settings.Instance.DebugTargetCombatants) Logging.Log("TargetCombatants", "if (Cache.Instance.PreferredPrimaryWeaponTarget.Distance <= Cache.Instance.MaxRange)", Logging.Debug);
                                //
                                // unlock a lower priority entity if needed
                                //
                                if (__highValueTargetsTargeted.Count() >= maxHighValueTargets && maxHighValueTargets > 1)
                                {
                                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: we have enough targets targeted [" + Cache.Instance.TotalTargetsandTargeting.Count() + "]", Logging.Debug);
                                    if (!UnlockLowValueTarget("Combat.TargetCombatants", "PreferredPrimaryWeaponTarget")
                                        || !UnlockHighValueTarget("Combat.TargetCombatants", "PreferredPrimaryWeaponTarget"))
                                    {
                                        return;
                                    }

                                    return;
                                }

                                if (Cache.Instance.PreferredPrimaryWeaponTarget.LockTarget("TargetCombatants.PreferredPrimaryWeaponTarget"))
                                {
                                    Logging.Log("Combat", "Targeting preferred primary weapon target [" + Cache.Instance.PreferredPrimaryWeaponTarget.Name + "][" + Cache.Instance.MaskedID(Cache.Instance.PreferredPrimaryWeaponTarget.Id) + "][" + Math.Round(Cache.Instance.PreferredPrimaryWeaponTarget.Distance / 1000, 0) + "k away]", Logging.Teal);
                                    Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                                    if (Cache.Instance.TotalTargetsandTargeting.Any() && (Cache.Instance.TotalTargetsandTargeting.Count() >= Cache.Instance.MaxLockedTargets))
                                    {
                                        Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                                    }

                                    return;
                                }
                            }

                            return;
                        }
                    }
                }
            }

            #endregion

            //if (Settings.Instance.DebugTargetCombatants)
            //{
            //    Logging.Log("Combat.TargetCombatants", "LCOs [" + Cache.Instance.Entities.Count(i => i.IsLargeCollidable) + "]", Logging.Debug);
            //    if (Cache.Instance.Entities.Any(i => i.IsLargeCollidable))
            //    {
            //        foreach (EntityCache LCO in Cache.Instance.Entities.Where(i => i.IsLargeCollidable))
            //        {
            //            Logging.Log("Combat.TargetCombatants", "LCO name [" + LCO.Name + "] Distance [" + Math.Round(LCO.Distance /1000,2) + "] TypeID [" + LCO.TypeId + "] GroupID [" + LCO.GroupId + "]", Logging.Debug);
            //        }
            //    }
            //}


            #region Preferred Drone target handling
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Preferred Drone target handling", Logging.Debug);
            //
            // Lets deal with our preferred targets next (in other words what Q is actively trying to shoot or engage drones on)
            //

            if (Cache.Instance.PreferredDroneTarget != null)
            {
                if (Cache.Instance.PreferredDroneTarget.IsIgnored)
                {
                    Cache.Instance.PreferredDroneTarget = null;
                }

                if (Cache.Instance.PreferredDroneTarget != null
                    && Cache.Instance.EntitiesOnGrid.Any(I => I.Id == Cache.Instance.PreferredDroneTarget.Id)
                    && Cache.Instance.UseDrones
                    && Cache.Instance.PreferredDroneTarget.IsReadyToTarget
                    && Cache.Instance.PreferredDroneTarget.Distance < Cache.Instance.WeaponRange
                    && Cache.Instance.PreferredDroneTarget.Nearest5kDistance <= Cache.Instance.MaxDroneRange)
                {
                    //
                    // unlock a lower priority entity if needed
                    //
                    if (__lowValueTargetsTargeted.Count() >= maxLowValueTargets)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: we have enough targets targeted [" + Cache.Instance.TotalTargetsandTargeting.Count() + "]", Logging.Debug);
                        if (!UnlockLowValueTarget("Combat.TargetCombatants", "PreferredPrimaryWeaponTarget")
                            || !UnlockHighValueTarget("Combat.TargetCombatants", "PreferredPrimaryWeaponTarget"))
                        {
                            return;
                        }

                        return;
                    }

                    if (Cache.Instance.PreferredDroneTarget.LockTarget("TargetCombatants.PreferredDroneTarget"))
                    {
                        Logging.Log("Combat", "Targeting preferred drone target [" + Cache.Instance.PreferredDroneTarget.Name + "][" + Cache.Instance.MaskedID(Cache.Instance.PreferredDroneTarget.Id) + "][" + Math.Round(Cache.Instance.PreferredDroneTarget.Distance / 1000, 0) + "k away]", Logging.Teal);
                        //highValueTargets.Add(primaryWeaponPriorityEntity);
                        Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                        if (Cache.Instance.TotalTargetsandTargeting.Any() && (Cache.Instance.TotalTargetsandTargeting.Count() >= Cache.Instance.MaxLockedTargets))
                        {
                            Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                        }

                        return;
                    }
                }    
            }
            
            #endregion
         
            #region Do we have enough targets?
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Do we have enough targets? Locked [" + Cache.Instance.Targets.Count() + "] Locking [" + Cache.Instance.Targeting.Count() + "] Total [" + Cache.Instance.TotalTargetsandTargeting.Count() + "] Slots Total [" + Cache.Instance.MaxLockedTargets + "]", Logging.Debug);
            //
            // OK so now that we are done dealing with preferred and priorities for now, lets see if we can target anything else
            // First lets see if we have enough targets already
            //

            int highValueSlotsreservedForPriorityTargets = 1;
            int lowValueSlotsreservedForPriorityTargets = 1;

            if (Cache.Instance.MaxLockedTargets <= 4)
            {
                //
                // With a ship/toon combination that has 4 or less slots you really do not have room to reserve 2 slots for priority targets
                //
                highValueSlotsreservedForPriorityTargets = 0;
                lowValueSlotsreservedForPriorityTargets = 0;
            }

            if (maxHighValueTargets <= 2)
            {
                //
                // do not reserve targeting slots if we have none to spare
                //
                highValueSlotsreservedForPriorityTargets = 0;
            }

            if (maxLowValueTargets <= 2)
            {
                //
                // do not reserve targeting slots if we have none to spare
                //
                lowValueSlotsreservedForPriorityTargets = 0;
            }


            if ((__highValueTargetsTargeted.Count() >= maxHighValueTargets - highValueSlotsreservedForPriorityTargets)
                && __lowValueTargetsTargeted.Count() >= maxLowValueTargets - lowValueSlotsreservedForPriorityTargets)
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: we have enough targets targeted [" + Cache.Instance.TotalTargetsandTargeting.Count() + "] __highValueTargetsTargeted [" + __highValueTargetsTargeted.Count() + "] __lowValueTargetsTargeted [" + __lowValueTargetsTargeted.Count() + "] maxHighValueTargets [" + maxHighValueTargets + "] maxLowValueTargets [" + maxLowValueTargets + "]", Logging.Debug);
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __highValueTargetsTargeted [" + __highValueTargetsTargeted.Count() + "] maxHighValueTargets [" + maxHighValueTargets + "] highValueSlotsreservedForPriorityTargets [" + highValueSlotsreservedForPriorityTargets + "]", Logging.Debug);
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __lowValueTargetsTargeted [" + __lowValueTargetsTargeted.Count() + "] maxLowValueTargets [" + maxLowValueTargets + "] lowValueSlotsreservedForPriorityTargets [" + lowValueSlotsreservedForPriorityTargets + "]", Logging.Debug);
                //Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                return;
            }

            #endregion

            #region Aggro Handling
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: Aggro Handling", Logging.Debug);
            //
            // OHHHH We are still here? OK Cool lets deal with things that are already targeting me
            //
            TargetingMe = Cache.Instance.TargetedBy.Where(t => t.Distance < (double)Distances.OnGridWithMe 
                                                            && t.CategoryId != (int)CategoryID.Asteroid
                                                            && t.IsTargetingMeAndNotYetTargeted
                                                            && (!t.IsSentry || (t.IsSentry && Settings.Instance.KillSentries) || (t.IsSentry && t.IsEwarTarget))
                                                            && t.Nearest5kDistance < Cache.Instance.MaxRange)
                                                            .ToList();

            List<EntityCache> highValueTargetingMe = TargetingMe.Where(t => (t.IsHighValueTarget))
                                                                .OrderByDescending(t => !t.IsNPCCruiser) //prefer battleships
                                                                .ThenByDescending(t => t.IsBattlecruiser && t.IsLastTargetPrimaryWeaponsWereShooting)
                                                                .ThenByDescending(t => t.IsBattleship && t.IsLastTargetPrimaryWeaponsWereShooting)
                                                                .ThenByDescending(t => t.IsBattlecruiser)
                                                                .ThenByDescending(t => t.IsBattleship)
                                                                .ThenBy(t => t.Nearest5kDistance).ToList();

            int LockedTargetsThatHaveHighValue = Cache.Instance.Targets.Count(t => (t.IsHighValueTarget));

            List<EntityCache> lowValueTargetingMe = TargetingMe.Where(t => t.IsLowValueTarget)
                                                               .OrderByDescending(t => !t.IsNPCCruiser) //prefer frigates
                                                               .ThenBy(t => t.Nearest5kDistance).ToList();

            int LockedTargetsThatHaveLowValue = Cache.Instance.Targets.Count(t => (t.IsLowValueTarget));

            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "TargetingMe [" + TargetingMe.Count() + "] lowValueTargetingMe [" + lowValueTargetingMe.Count() + "] targeted [" + LockedTargetsThatHaveLowValue + "] :::  highValueTargetingMe [" + highValueTargetingMe.Count() + "] targeted [" + LockedTargetsThatHaveHighValue + "] LCOs [" + Cache.Instance.EntitiesOnGrid.Count(e => e.IsLargeCollidable) + "]", Logging.Debug);

            // High Value
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: foreach (EntityCache entity in highValueTargetingMe)", Logging.Debug);

            if (highValueTargetingMe.Any())
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: [" + highValueTargetingMe.Count() + "] highValueTargetingMe targets", Logging.Debug);

                int HighValueTargetsTargetedThisCycle = 1;
                foreach (EntityCache highValueTargetingMeEntity in highValueTargetingMe.Where(t => t.IsReadyToTarget && t.Nearest5kDistance < Cache.Instance.MaxRange))
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: [" + HighValueTargetsTargetedThisCycle + "][" + highValueTargetingMeEntity.Name + "][" + Math.Round(highValueTargetingMeEntity.Distance / 1000, 2) + "k][groupID" + highValueTargetingMeEntity.GroupId + "]", Logging.Debug);
                    // Have we reached the limit of high value targets?
                    if (__highValueTargetsTargeted.Count() >= maxHighValueTargets)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __highValueTargetsTargeted.Count() [" + __highValueTargetsTargeted.Count() + "] maxHighValueTargets [" + maxHighValueTargets + "], done for this iteration", Logging.Debug);
                        break;
                    }

                    if (HighValueTargetsTargetedThisCycle >= 4)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: HighValueTargetsTargetedThisCycle [" + HighValueTargetsTargetedThisCycle + "], done for this iteration", Logging.Debug);
                        break;
                    }

                    //We need to make sure we do not have too many low value targets filling our slots
                    if (__highValueTargetsTargeted.Count() < maxHighValueTargets && __lowValueTargetsTargeted.Count() > maxLowValueTargets)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __highValueTargetsTargeted [" + __highValueTargetsTargeted.Count() + "] < maxHighValueTargets [" + maxHighValueTargets + "] && __lowValueTargetsTargeted [" + __lowValueTargetsTargeted.Count() + "] > maxLowValueTargets [" + maxLowValueTargets + "], try to unlock a low value target, and return.", Logging.Debug);
                        UnlockLowValueTarget("Combat.TargetCombatants", "HighValueTarget");
                        return;
                    }

                    if (highValueTargetingMeEntity != null
                        && highValueTargetingMeEntity.Distance < Cache.Instance.MaxRange
                        && highValueTargetingMeEntity.IsReadyToTarget
                        && highValueTargetingMeEntity.IsInOptimalRangeOrNothingElseAvail
                        && !highValueTargetingMeEntity.IsIgnored
                        && highValueTargetingMeEntity.LockTarget("TargetCombatants.HighValueTargetingMeEntity"))
                    {
                        HighValueTargetsTargetedThisCycle++;
                        Logging.Log("Combat", "Targeting high value target [" + highValueTargetingMeEntity.Name + "][" + Cache.Instance.MaskedID(highValueTargetingMeEntity.Id) + "][" + Math.Round(highValueTargetingMeEntity.Distance / 1000, 0) + "k away] highValueTargets.Count [" + __highValueTargetsTargeted.Count() + "]", Logging.Teal);
                        Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                        if (Cache.Instance.TotalTargetsandTargeting.Any() && (Cache.Instance.TotalTargetsandTargeting.Count() >= Cache.Instance.MaxLockedTargets))
                        {
                            Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                        }

                        if (HighValueTargetsTargetedThisCycle > 2)
                        {
                            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: HighValueTargetsTargetedThisCycle [" + HighValueTargetsTargetedThisCycle + "] > 3, return", Logging.Debug);
                            return;
                        }
                    }

                    continue;
                }

                if (HighValueTargetsTargetedThisCycle > 1)
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: HighValueTargetsTargetedThisCycle [" + HighValueTargetsTargetedThisCycle + "] > 1, return", Logging.Debug);
                    return;
                }
            }
            else
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: 0 highValueTargetingMe targets", Logging.Debug);
            }

            // Low Value
            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: foreach (EntityCache entity in lowValueTargetingMe)", Logging.Debug);

            if (lowValueTargetingMe.Any())
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: [" + lowValueTargetingMe.Count() + "] lowValueTargetingMe targets", Logging.Debug);

                int LowValueTargetsTargetedThisCycle = 1;
                foreach (EntityCache lowValueTargetingMeEntity in lowValueTargetingMe.Where(t => !t.IsTarget && !t.IsTargeting && t.Nearest5kDistance < Cache.Instance.LowValueTargetsHaveToBeWithinDistance).OrderByDescending(i => i.IsLastTargetDronesWereShooting).ThenBy(i => i.IsLastTargetPrimaryWeaponsWereShooting))
                {

                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: lowValueTargetingMe [" + LowValueTargetsTargetedThisCycle + "][" + lowValueTargetingMeEntity.Name + "][" + Math.Round(lowValueTargetingMeEntity.Distance / 1000, 2) + "k] groupID [ " + lowValueTargetingMeEntity.GroupId + "]", Logging.Debug);

                    // Have we reached the limit of low value targets?
                    if (__lowValueTargetsTargeted.Count() >= maxLowValueTargets)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: __lowValueTargetsTargeted.Count() [" + __lowValueTargetsTargeted.Count() + "] maxLowValueTargets [" + maxLowValueTargets + "], done for this iteration", Logging.Debug);
                        break;
                    }

                    if (LowValueTargetsTargetedThisCycle >= 3)
                    {
                        if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: LowValueTargetsTargetedThisCycle [" + LowValueTargetsTargetedThisCycle + "], done for this iteration", Logging.Debug);
                        break;
                    }

                    //We need to make sure we do not have too many high value targets filling our slots
                    if (__lowValueTargetsTargeted.Count() < maxLowValueTargets && __highValueTargetsTargeted.Count() > maxHighValueTargets)
                    {
                        UnlockLowValueTarget("Combat.TargetCombatants", "HighValueTarget");
                        return;
                    }

                    if (lowValueTargetingMeEntity != null
                        && lowValueTargetingMeEntity.Distance < Cache.Instance.WeaponRange
                        && lowValueTargetingMeEntity.IsReadyToTarget
                        && lowValueTargetingMeEntity.IsInOptimalRangeOrNothingElseAvail
                        && lowValueTargetingMeEntity.Nearest5kDistance < Cache.Instance.LowValueTargetsHaveToBeWithinDistance
                        && !lowValueTargetingMeEntity.IsIgnored
                        && lowValueTargetingMeEntity.LockTarget("TargetCombatants.LowValueTargetingMeEntity"))
                    {
                        LowValueTargetsTargetedThisCycle++;
                        Logging.Log("Combat", "Targeting low  value target [" + lowValueTargetingMeEntity.Name + "][" + Cache.Instance.MaskedID(lowValueTargetingMeEntity.Id) + "][" + Math.Round(lowValueTargetingMeEntity.Distance / 1000, 0) + "k away] lowValueTargets.Count [" + __lowValueTargetsTargeted.Count() + "]", Logging.Teal);
                        //lowValueTargets.Add(lowValueTargetingMeEntity);
                        Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.TargetDelay_milliseconds);
                        if (Cache.Instance.TotalTargetsandTargeting.Any() && (Cache.Instance.TotalTargetsandTargeting.Count() >= Cache.Instance.MaxLockedTargets))
                        {
                            Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                        }
                        if (LowValueTargetsTargetedThisCycle > 2)
                        {
                            if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: LowValueTargetsTargetedThisCycle [" + LowValueTargetsTargetedThisCycle + "] > 2, return", Logging.Debug);
                            return;
                        }
                    }

                    continue;
                }

                if (LowValueTargetsTargetedThisCycle > 1)
                {
                    if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: if (LowValueTargetsTargetedThisCycle > 1)", Logging.Debug);
                    return;
                }
            }
            else
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: 0 lowValueTargetingMe targets", Logging.Debug);
            }

            //
            // If we have 2 PotentialCombatTargets targeted at this point return... we do not want to target anything that is not yet aggressed if we have something aggressed. 
            // or are in the middle of attempting to aggro something
            // 
            if (Cache.Instance.PotentialCombatTargets.Count(e => e.IsTarget) > 1 || (Cache.Instance.MaxLockedTargets < 2 && Cache.Instance.Targets.Any()))
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: We already have [" + Cache.Instance.PotentialCombatTargets.Count(e => e.IsTarget) + "] PotentialCombatTargets Locked. Do not aggress non aggressed NPCs until we have no targets", Logging.Debug);
                return;
            }

            #endregion

            #region All else fails grab an unlocked target that is not yet targeting me
            //
            // Ok, now that that is all handled lets grab the closest non aggressed mob and pew
            // Build a list of things not yet targeting me and not yet targeted
            //
            
            NotYetTargetingMe = Cache.Instance.PotentialCombatTargets.Where(e => e.IsNotYetTargetingMeAndNotYetTargeted)
                                                                        .OrderBy(t => t.Nearest5kDistance)
                                                                        .ToList();

            if (NotYetTargetingMe.Any())
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: [" + NotYetTargetingMe.Count() + "] NotYetTargetingMe targets", Logging.Debug);

                foreach (EntityCache TargetThisNotYetAggressiveNPC in NotYetTargetingMe)
                {
                    if (TargetThisNotYetAggressiveNPC != null
                     && TargetThisNotYetAggressiveNPC.IsReadyToTarget
                     && TargetThisNotYetAggressiveNPC.IsInOptimalRangeOrNothingElseAvail
                     && TargetThisNotYetAggressiveNPC.Nearest5kDistance < Cache.Instance.MaxRange
                     && !TargetThisNotYetAggressiveNPC.IsIgnored
                     && TargetThisNotYetAggressiveNPC.LockTarget("TargetCombatants.TargetThisNotYetAggressiveNPC"))
                    {
                        Logging.Log("Combat", "Targeting non-aggressed NPC target [" + TargetThisNotYetAggressiveNPC.Name + "][GroupID: " + TargetThisNotYetAggressiveNPC.GroupId + "][TypeID: " + TargetThisNotYetAggressiveNPC.TypeId + "][" + Cache.Instance.MaskedID(TargetThisNotYetAggressiveNPC.Id) + "][" + Math.Round(TargetThisNotYetAggressiveNPC.Distance / 1000, 0) + "k away]", Logging.Teal);
                        Cache.Instance.NextTargetAction = DateTime.UtcNow.AddMilliseconds(4000);
                        if (Cache.Instance.TotalTargetsandTargeting.Any() && (Cache.Instance.TotalTargetsandTargeting.Count() >= Cache.Instance.MaxLockedTargets))
                        {
                            Cache.Instance.NextTargetAction = DateTime.UtcNow.AddSeconds(Time.Instance.TargetsAreFullDelay_seconds);
                        }

                        return;
                    }
                }
            }
            else
            {
                if (Settings.Instance.DebugTargetCombatants) Logging.Log("Combat.TargetCombatants", "DebugTargetCombatants: 0 NotYetTargetingMe targets", Logging.Debug);
            }
            
            return;
            #endregion
        }

        public static void ProcessState()
        {
            try
            {
                if (DateTime.UtcNow < _lastCombatProcessState.AddMilliseconds(350) || Settings.Instance.DebugDisableCombat) //if it has not been 500ms since the last time we ran this ProcessState return. We can't do anything that close together anyway
                {
                    return;
                }

                _lastCombatProcessState = DateTime.UtcNow;

                if (Cache.Instance.InSpace && Cache.Instance.InWarp)
                {
                    i = 0;
                }

                if ((_States.CurrentCombatState != CombatState.Idle ||
                    _States.CurrentCombatState != CombatState.OutOfAmmo) &&
                    (Cache.Instance.InStation ||// There is really no combat in stations (yet)
                    !Cache.Instance.InSpace || // if we are not in space yet, wait...
                    Cache.Instance.ActiveShip.Entity == null || // What? No ship entity?
                    Cache.Instance.ActiveShip.Entity.IsCloaked))  // There is no combat when cloaked
                {
                    _States.CurrentCombatState = CombatState.Idle;
                    return;
                }

                if (Cache.Instance.InStation)
                {
                    _States.CurrentCombatState = CombatState.Idle;
                    return;
                }

                try
                {
                    if (!Cache.Instance.MyShipEntity.IsFrigate && !Cache.Instance.MyShipEntity.IsCruiser && Cache.Instance.ActiveShip.GivenName != Settings.Instance.SalvageShipName)
                    {
                        //
                        // we are not in something light and fast so assume we need weapons and assume we should be in the defined combatship
                        //
                        if (!Cache.Instance.Weapons.Any() && Cache.Instance.InSpace && DateTime.UtcNow > Cache.Instance.LastInStation.AddSeconds(30))
                        {
                            Logging.Log("Combat", "Your Current ship [" + Cache.Instance.ActiveShip.GivenName + "] has no weapons!", Logging.Red);
                            _States.CurrentCombatState = CombatState.OutOfAmmo;
                        }

                        if (Cache.Instance.ActiveShip.GivenName != Settings.Instance.CombatShipName)
                        {
                            Logging.Log("Combat", "Your Current ship [" + Cache.Instance.ActiveShip.GivenName + "] GroupID [" + Cache.Instance.MyShipEntity.GroupId + "] TypeID [" + Cache.Instance.MyShipEntity.TypeId + "] is not the CombatShipName [" + Settings.Instance.CombatShipName + "]", Logging.Red);
                            _States.CurrentCombatState = CombatState.OutOfAmmo;
                        }
                    }

                    //
                    // we are in something light and fast so assume we do not need weapons and assume we do not need to be in the defined combatship
                    //
                }
                catch (Exception exception)
                {
                    if (Settings.Instance.DebugExceptions) Logging.Log("Combat", "if (!Cache.Instance.Weapons.Any() && Cache.Instance.ActiveShip.GivenName == Settings.Instance.CombatShipName ) - exception [" + exception + "]", Logging.White);
                }

                switch (_States.CurrentCombatState)
                {
                    case CombatState.CheckTargets:
                        _States.CurrentCombatState = CombatState.KillTargets; //this MUST be before TargetCombatants() or the combat state will potentially get reset (important for the OutOfAmmo state)
                        //if (Settings.Instance.TargetSelectionMethod == "isdp")
                        //{
                            TargetCombatants();    
                        //}
                        //else //use new target selection method
                        //{
                        //    TargetCombatants2();    
                        //}
                        
                        break;

                    case CombatState.KillTargets:

                        if (Cache.Instance.CurrentShipsCargo == null)
                        {
                            Logging.Log("Combat.KillTargets", "if (Cache.Instance.CurrentShipsCargo == null)", Logging.Teal);
                            return;
                        }
                        _States.CurrentCombatState = CombatState.CheckTargets;

                        if (Settings.Instance.DebugPreferredPrimaryWeaponTarget || Settings.Instance.DebugKillTargets)
                        {
                            if (Cache.Instance.Targets.Any())
                            {
                                if (Cache.Instance.PreferredPrimaryWeaponTarget != null)
                                {
                                    Logging.Log("Combat.KillTargets", "PreferredPrimaryWeaponTarget [" + Cache.Instance.PreferredPrimaryWeaponTarget.Name + "][" + Math.Round(Cache.Instance.PreferredPrimaryWeaponTarget.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Cache.Instance.PreferredPrimaryWeaponTargetID) + "]", Logging.Teal);
                                }
                                else
                                {
                                    Logging.Log("Combat.KillTargets", "PreferredPrimaryWeaponTarget [ null ]", Logging.Teal);
                                }

                                //if (Cache.Instance.PreferredDroneTarget != null) Logging.Log("Combat.KillTargets", "PreferredPrimaryWeaponTarget [" + Cache.Instance.PreferredDroneTarget.Name + "][" + Math.Round(Cache.Instance.PreferredDroneTarget.Distance / 1000, 0) + "k][" + Cache.Instance.MaskedID(Cache.Instance.PreferredDroneTargetID) + "]", Logging.Teal);        
                            }
                        }
                        
                        //lets at the least make sure we have a fresh entity this frame to check against so we are not trying to navigate to things that no longer exist
                        EntityCache killTarget = null;
                        if (Cache.Instance.PreferredPrimaryWeaponTarget != null)
                        {
                            if (Cache.Instance.Targets.Any(t => t.Id == Cache.Instance.PreferredPrimaryWeaponTarget.Id))
                            {
                                killTarget = Cache.Instance.Targets.FirstOrDefault(t => t.Id == Cache.Instance.PreferredPrimaryWeaponTarget.Id && t.Distance < Cache.Instance.MaxRange);    
                            }
                            else
                            {
                                //Logging.Log("Combat.Killtargets", "Unable to find the PreferredPrimaryWeaponTarget in the Entities list... PPWT.Name[" + Cache.Instance.PreferredPrimaryWeaponTarget.Name + "] PPWTID [" + Cache.Instance.MaskedID(Cache.Instance.PreferredPrimaryWeaponTargetID) + "]", Logging.Debug);
                                //Cache.Instance.PreferredPrimaryWeaponTarget = null;
                                //Cache.Instance.NextGetBestCombatTarget = DateTime.UtcNow;
                            }
                        }

                        if (killTarget == null)
                        {
                            if (Cache.Instance.Targets.Any(i => !i.IsContainer && !i.IsBadIdea))
                            {
                                killTarget = Cache.Instance.Targets.Where(i => !i.IsContainer && !i.IsBadIdea && i.Distance < Cache.Instance.MaxRange).OrderByDescending(i => i.IsInOptimalRange).ThenByDescending(i => i.IsCorrectSizeForMyWeapons).ThenBy(i => i.Distance).FirstOrDefault();
                            }
                        }

                        if (killTarget != null)
                        {
                            if (!Cache.Instance.InMission || Settings.Instance.SpeedTank)
                            {
                                if (Settings.Instance.DebugNavigateOnGrid) Logging.Log("Combat.KillTargets", "Navigate Toward the Closest Preferred PWPT", Logging.Debug);
                                NavigateOnGrid.NavigateIntoRange(killTarget, "Combat", Cache.Instance.normalNav);    
                            }

                            if (killTarget.IsReadyToShoot)
                            {
                                i++;
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating Bastion", Logging.Debug);
                                ActivateBastion(); //by default this will deactivate bastion when needed, but NOT activate it, activation needs activate = true
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating Painters", Logging.Debug);
                                ActivateTargetPainters(killTarget);
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating Webs", Logging.Debug);
                                ActivateStasisWeb(killTarget);
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating WarpDisruptors", Logging.Debug);
                                ActivateWarpDisruptor(killTarget);
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating RemoteRepairers", Logging.Debug);
                                ActivateRemoteRepair(killTarget);
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating NOS/Neuts", Logging.Debug);
                                ActivateNos(killTarget);
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating SensorDampeners", Logging.Debug);
                                ActivateSensorDampeners(killTarget);
                                if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "[" + i + "] Activating Weapons", Logging.Debug);
                                ActivateWeapons(killTarget);
                                return;
                            }

                            if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "killTarget [" + killTarget.Name + "][" + Math.Round(killTarget.Distance/1000,0) + "k][" + Cache.Instance.MaskedID(killTarget.Id) + "] is not yet ReadyToShoot, LockedTarget [" + killTarget.IsTarget + "] My MaxRange [" + Math.Round(Cache.Instance.MaxRange/1000,0) + "]", Logging.Debug);
                            return;
                        }
                        
                        if (Settings.Instance.DebugKillTargets) Logging.Log("Combat.KillTargets", "We do not have a killTarget targeted, waiting", Logging.Debug);

                        //ok so we do need this, but only use it if we actually have some potential targets
                        if (Cache.Instance.PrimaryWeaponPriorityTargets.Any() || (Cache.Instance.PotentialCombatTargets.Any() && Cache.Instance.Targets.Any() && (!Cache.Instance.InMission || Settings.Instance.SpeedTank)))
                        {
                            //if (Settings.Instance.TargetSelectionMethod == "isdp")
                            //{
                                Cache.Instance.GetBestPrimaryWeaponTarget(Cache.Instance.MaxRange, false, "Combat");
                            //}
                            //else //use new target selection method
                            //{
                            //    Cache.Instance.__GetBestWeaponTargets(Cache.Instance.MaxDroneRange);
                            //}

                            i = 0;
                        }
                        
                        break;

                    case CombatState.OutOfAmmo:
                        break;

                    case CombatState.Idle:

                        //
                        // below is the reasons we will start the combat state(s) - if the below is not met do nothing
                        //
                        //Logging.Log("Cache.Instance.InSpace: " + Cache.Instance.InSpace);
                        //Logging.Log("Cache.Instance.ActiveShip.Entity.IsCloaked: " + Cache.Instance.ActiveShip.Entity.IsCloaked);
                        //Logging.Log("Cache.Instance.ActiveShip.GivenName.ToLower(): " + Cache.Instance.ActiveShip.GivenName.ToLower());
                        //Logging.Log("Cache.Instance.InSpace: " + Cache.Instance.InSpace);
                        if (Cache.Instance.InSpace && //we are in space (as opposed to being in station or in limbo between systems when jumping)
                            (Cache.Instance.ActiveShip.Entity != null &&  // we are in a ship!
                            !Cache.Instance.ActiveShip.Entity.IsCloaked && //we are not cloaked anymore
                            Cache.Instance.ActiveShip.GivenName.ToLower() == Settings.Instance.CombatShipName.ToLower() && //we are in our combat ship
                            !Cache.Instance.InWarp)) // no longer in warp
                        {
                            _States.CurrentCombatState = CombatState.CheckTargets;
                            return;
                        }
                        break;

                    default:

                        // Next state
                        Logging.Log("Combat", "CurrentCombatState was not set thus ended up at default", Logging.Orange);
                        _States.CurrentCombatState = CombatState.CheckTargets;
                        break;
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Combat.ProcessState", "Exception [" + exception + "]", Logging.Debug);
            }
        }
    }
}