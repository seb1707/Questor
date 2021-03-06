// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------


using Questor.Modules.Actions;

namespace Questor.Modules.BackgroundTasks
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Combat;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;
    using Questor.Modules.States;
    using DirectEve;

    public static class Defense
    {
        public static int DefenseInstances;

        static Defense()
        {
            Interlocked.Increment(ref DefenseInstances);
        }
        
        private static DateTime _lastCloaked = DateTime.UtcNow;

        private static DateTime _lastPulse = DateTime.UtcNow;
        private static int _trackingLinkScriptAttempts;
        private static int _sensorBoosterScriptAttempts;
        private static int _sensorDampenerScriptAttempts;
        private static int _trackingComputerScriptAttempts;
        private static int _trackingDisruptorScriptAttempts;
        //private int _ancillaryShieldBoosterAttempts;
        //private int _capacitorInjectorAttempts;
        private static DateTime _nextOverloadAttempt = DateTime.UtcNow;
        public static bool DoNotBreakInvul;
        public static int MinimumPropulsionModuleDistance { get; set; }
        public static int MinimumPropulsionModuleCapacitor { get; set; }
        public static int ActivateRepairModulesAtThisPerc { get; set; }
        public static int DeactivateRepairModulesAtThisPerc { get; set; }
        public static int InjectCapPerc { get; set; }

        private static int ModuleNumber { get; set; }

        private static readonly Dictionary<long, DateTime> NextScriptReload = new Dictionary<long, DateTime>();

        private static bool LoadthisScript(DirectItem scriptToLoad, ModuleCache module)
        {
            if (scriptToLoad != null)
            {
                if (module.IsReloadingAmmo || module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline)
                    return false;

                // We have enough ammo loaded
                if (module.Charge != null && module.Charge.TypeId == scriptToLoad.TypeId && module.CurrentCharges == module.MaxCharges)
                {
                    Logging.Log("LoadthisScript", "module is already loaded with the script we wanted", Logging.Teal);
                    NextScriptReload[module.ItemId] = DateTime.UtcNow.AddSeconds(15); //mark this weapon as reloaded... by the time we need to reload this timer will have aged enough...
                    return false;
                }

                // We are reloading, wait 15
                if (NextScriptReload.ContainsKey(module.ItemId) && DateTime.UtcNow < NextScriptReload[module.ItemId].AddSeconds(15))
                {
                    Logging.Log("LoadthisScript", "module was reloaded recently... skipping", Logging.Teal);
                    return false;
                }
                NextScriptReload[module.ItemId] = DateTime.UtcNow.AddSeconds(15);

                // Reload or change ammo
                if (module.Charge != null && module.Charge.TypeId == scriptToLoad.TypeId)
                {
                    if (DateTime.UtcNow.Subtract(Time.Instance.LastLoggingAction).TotalSeconds > 10)
                    {
                        Time.Instance.LastLoggingAction = DateTime.UtcNow;
                    }
                    
                    if (module.ReloadAmmo(scriptToLoad, 0, 0))
                    {
                        Logging.Log("Defense", "Reloading [" + module.TypeId + "] with [" + scriptToLoad.TypeName + "][TypeID: " + scriptToLoad.TypeId + "]", Logging.Teal);
                        return true;    
                    }

                    return false;
                }

                if (DateTime.UtcNow.Subtract(Time.Instance.LastLoggingAction).TotalSeconds > 10)
                {
                    Time.Instance.LastLoggingAction = DateTime.UtcNow;
                }
                
                if (module.ChangeAmmo(scriptToLoad, 0, 0))
                {
                    Logging.Log("Defense", "Changing [" + module.TypeId + "] with [" + scriptToLoad.TypeName + "][TypeID: " + scriptToLoad.TypeId + "]", Logging.Teal);
                    return true;    
                }

                return false;
            }
            Logging.Log("LoadthisScript", "script to load was NULL!", Logging.Teal);
            return false;
        }

        private static void ActivateOnce()
        {
            //if (Logging.DebugLoadScripts) Logging.Log("Defense", "spam", Logging.White);
            if (DateTime.UtcNow < Time.Instance.NextActivateSupportModules) //if we just did something wait a fraction of a second
                return;

            ModuleNumber = 0;
            foreach (ModuleCache module in Cache.Instance.Modules)
            {
                if (!module.IsActivatable)
                    continue;

                //if (Logging.DebugLoadScripts) Logging.Log("Defense", "Found Activatable Module [typeid: " + module.TypeId + "][groupID: " + module.GroupId +  "]", Logging.White);

                if (module.GroupId == (int)Group.TrackingDisruptor ||
                    module.GroupId == (int)Group.TrackingComputer ||
                    module.GroupId == (int)Group.TrackingLink ||
                    module.GroupId == (int)Group.SensorBooster ||
                    module.GroupId == (int)Group.SensorDampener ||
                    module.GroupId == (int)Group.CapacitorInjector ||
                    module.GroupId == (int)Group.AncillaryShieldBooster)
                {
                    //if (Logging.DebugLoadScripts) Logging.Log("Defense", "---Found mod that could take a script [typeid: " + module.TypeId + "][groupID: " + module.GroupId + "][module.CurrentCharges [" + module.CurrentCharges + "]", Logging.White);
                    if (module.CurrentCharges < module.MaxCharges)
                    {
                        if (Logging.DebugLoadScripts) Logging.Log("Defense", "Found Activatable Module with no charge[typeID:" + module.TypeId + "]", Logging.White);
                        DirectItem scriptToLoad;

                        if (module.GroupId == (int)Group.TrackingDisruptor && _trackingDisruptorScriptAttempts < 5)
                        {
                            _trackingDisruptorScriptAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "TrackingDisruptor Found", Logging.White);
                            scriptToLoad = Cache.Instance.CheckCargoForItem(Settings.Instance.TrackingDisruptorScript, 1);

                            // this needs a counter and an abort after 10 tries or so... or it will keep checking the cargo for a script that may not exist
                            // every second we are in space!
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "TrackingDisruptor Found", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddSeconds(2);
                                        return;    
                                    }
                                }

                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline)
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                                return;
                            }
                            ModuleNumber++;
                            continue;
                        }

                        if (module.GroupId == (int)Group.TrackingComputer && _trackingComputerScriptAttempts < 5)
                        {
                            _trackingComputerScriptAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "TrackingComputer Found", Logging.White);
                            DirectItem TrackingComputerScript = Cache.Instance.CheckCargoForItem(Settings.Instance.TrackingComputerScript, 1);
                            
                            EntityCache EntityTrackingDisruptingMe = Combat.TargetedBy.FirstOrDefault(t => t.IsTrackingDisruptingMe);
                            if (EntityTrackingDisruptingMe != null || TrackingComputerScript == null)
                            {
                                TrackingComputerScript = Cache.Instance.CheckCargoForItem((int)TypeID.OptimalRangeScript, 1);
                            }

                            scriptToLoad = TrackingComputerScript;
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "Script Found for TrackingComputer", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddSeconds(2);
                                        return;    
                                    }
                                }

                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline)
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                                return;
                            }
                            ModuleNumber++;
                            continue;
                        }

                        if (module.GroupId == (int)Group.TrackingLink && _trackingLinkScriptAttempts < 5)
                        {
                            _trackingLinkScriptAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "TrackingLink Found", Logging.White);
                            scriptToLoad = Cache.Instance.CheckCargoForItem(Settings.Instance.TrackingLinkScript, 1);
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "Script Found for TrackingLink", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddSeconds(2);
                                        return;    
                                    }
                                }

                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline)
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                                return;
                            }
                            ModuleNumber++;
                            continue;
                        }

                        if (module.GroupId == (int)Group.SensorBooster && _sensorBoosterScriptAttempts < 5)
                        {
                            _sensorBoosterScriptAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "SensorBooster Found", Logging.White);
                            scriptToLoad = Cache.Instance.CheckCargoForItem(Settings.Instance.SensorBoosterScript, 1);
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "Script Found for SensorBooster", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddSeconds(2);
                                        return;    
                                    }
                                }

                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline)
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                                return;
                            }
                            ModuleNumber++;
                            continue;
                        }

                        if (module.GroupId == (int)Group.SensorDampener && _sensorDampenerScriptAttempts < 5)
                        {
                            _sensorDampenerScriptAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "SensorDampener Found", Logging.White);
                            scriptToLoad = Cache.Instance.CheckCargoForItem(Settings.Instance.SensorDampenerScript, 1);
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "Script Found for SensorDampener", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddSeconds(2);
                                        return;    
                                    }
                                }

                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline)
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                                return;
                            }
                            ModuleNumber++;
                            continue;
                        }

                        if (module.GroupId == (int)Group.AncillaryShieldBooster)
                        {
                            //_ancillaryShieldBoosterAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "ancillaryShieldBooster Found", Logging.White);
                            scriptToLoad = Cache.Instance.CheckCargoForItem(Settings.Instance.AncillaryShieldBoosterScript, 1);
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "CapBoosterCharges Found for ancillaryShieldBooster", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(500);
                                        return;    
                                    }
                                }

                                bool inCombat = Combat.TargetedBy.Any();
                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline || (inCombat && module.CurrentCharges > 0))
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                                return;
                            }
                            ModuleNumber++;
                            continue;
                        }

                        if (module.GroupId == (int)Group.CapacitorInjector)
                        {
                            //_capacitorInjectorAttempts++;
                            if (Logging.DebugLoadScripts) Logging.Log("Defense", "capacitorInjector Found", Logging.White);
                            scriptToLoad = Cache.Instance.CheckCargoForItem(Settings.Instance.CapacitorInjectorScript, 1);
                            if (scriptToLoad != null)
                            {
                                if (Logging.DebugLoadScripts) Logging.Log("Defense", "CapBoosterCharges Found for capacitorInjector", Logging.White);
                                if (module.IsActive)
                                {
                                    if (module.Click())
                                    {
                                        Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(500);
                                        return;    
                                    }
                                }

                                bool inCombat = Combat.TargetedBy.Any();
                                if (module.IsActive || module.IsDeactivating || module.IsChangingAmmo || module.InLimboState || module.IsGoingOnline || !module.IsOnline || (inCombat && module.CurrentCharges > 0))
                                {
                                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                                    ModuleNumber++;
                                    continue;
                                }

                                if (!LoadthisScript(scriptToLoad, module))
                                {
                                    ModuleNumber++;
                                    continue;
                                }
                            }
                            else if (module.CurrentCharges == 0)
                            {
                                Logging.Log("Defense", "ReloadCapBooster: ran out of cap booster with typeid: [ " + Settings.Instance.CapacitorInjectorScript + " ]", Logging.Orange);
                                _States.CurrentCombatState = CombatState.OutOfAmmo;
                                continue;
                            }
                            ModuleNumber++;
                            continue;
                        }
                    }
                }
                ModuleNumber++;
                Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                continue;
            }

            ModuleNumber = 0;
            foreach (ModuleCache module in Cache.Instance.Modules)
            {
                if (!module.IsActivatable)
                    continue;

                bool activate = false;
                activate |= module.GroupId == (int)Group.CloakingDevice;
                activate |= module.GroupId == (int)Group.ShieldHardeners;
                activate |= module.GroupId == (int)Group.DamageControl;
                activate |= module.GroupId == (int)Group.ArmorHardeners;
                activate |= module.GroupId == (int)Group.SensorBooster;
                activate |= module.GroupId == (int)Group.TrackingComputer;
                activate |= module.GroupId == (int)Group.ECCM;

                ModuleNumber++;

                if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] Module TypeID [" + module.TypeId + "] GroupId [" + module.GroupId + "] Found", Logging.Debug);
                if (!activate)
                {
                    if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] is not activatable, proceed to next module", Logging.Debug);
                    continue;
                }

                if (module.IsActive)
                {
                    if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] is already active", Logging.Debug);
                    continue;
                }

                if (module.InLimboState)
                {
                    if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] is in LimboState (likely being activated or decativated already)", Logging.Debug);
                    continue;
                }

                if (module.GroupId == (int)Group.CloakingDevice)
                {
                    //Logging.Log("Defense: This module has a typeID of: " + module.TypeId + " !!");
                    if (module.TypeId != 11578)  //11578 Covert Ops Cloaking Device - if you don't have a covert ops cloak try the next module
                    {
                        continue;
                    }
                    EntityCache stuffThatMayDecloakMe = Cache.Instance.EntitiesOnGrid.Where(t => t.Name != Cache.Instance.DirectEve.Me.Name || t.IsBadIdea || t.IsContainer || t.IsNpc || t.IsPlayer).OrderBy(t => t.Distance).FirstOrDefault();
                    if (stuffThatMayDecloakMe != null && (stuffThatMayDecloakMe.Distance <= (int)Distances.SafeToCloakDistance)) //if their is anything within 2300m do not attempt to cloak
                    {
                        if ((int)stuffThatMayDecloakMe.Distance != 0)
                        {
                            //Logging.Log(Defense: StuffThatMayDecloakMe.Name + " is very close at: " + StuffThatMayDecloakMe.Distance + " meters");
                            continue;
                        }
                    }
                }
                else
                {
                    //
                    // if capacitor is really really low, do not make it worse
                    //
                    if (Cache.Instance.ActiveShip.Capacitor < 45)
                    {
                        if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] You have less then 45 UNITS of cap: do not make it worse by turning on the hardeners", Logging.Debug);
                        continue;
                    }

                    if (Cache.Instance.ActiveShip.CapacitorPercentage < 3)
                    {
                        if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] You have less then 3% of cap: do not make it worse by turning on the hardeners", Logging.Debug);
                        continue;
                    }

                    //
                    // if total capacitor is really low, do not run stuff unless we are targeted by something
                    // this should only kick in when using frigates as the combatship
                    //
                    if (Cache.Instance.ActiveShip.Capacitor < 400 && !Combat.TargetedBy.Any() &&
                        Cache.Instance.ActiveShip.GivenName.ToLower() == Combat.CombatShipName.ToLower())
                    {
                        if (Logging.DebugDefense) Logging.Log("DefenseActivateOnce", "[" + ModuleNumber + "] You have less then 400 units total cap and nothing is targeting you yet, no need for hardeners yet.", Logging.Debug);
                        continue;
                    }
                }
                //
                // at this point the module should be active but is not: activate it, set the delay and return. The process will resume on the next tick
                //
                if (module.Click())
                {
                    Time.Instance.NextActivateSupportModules = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                    if (Logging.DebugDefense) Logging.Log("Defense", "Defensive module activated: [" + ModuleNumber + "]", Logging.White);
                    return;    
                }
            }

            ModuleNumber = 0;
        }

        private static bool OverLoadWeapons()
        {
            //if (Logging.DebugLoadScripts) Logging.Log("Defense", "spam", Logging.White);
            if (DateTime.UtcNow < _nextOverloadAttempt) //if we just did something wait a bit
                return true;

            if (!Settings.Instance.OverloadWeapons)
            {
                // if we do not have the OverLoadWeapons setting set to true then just return.
                _nextOverloadAttempt = DateTime.UtcNow.AddSeconds(30);
                return true;
            }

            //
            //if we do not have the skill (to at least lvl1) named thermodynamics, return true and do not try to overload
            //


            ModuleNumber = 0;
            foreach (ModuleCache module in Cache.Instance.Modules)
            {
                if (!module.IsActivatable)
                    continue;

                if (module.IsOverloaded || module.IsPendingOverloading || module.IsPendingStopOverloading)
                    continue;

                //if (Logging.DebugLoadScripts) Logging.Log("Defense", "Found Activatable Module [typeid: " + module.TypeId + "][groupID: " + module.GroupId +  "]", Logging.White);

                if (module.GroupId == (int)Group.EnergyWeapon ||
                    module.GroupId == (int)Group.HybridWeapon ||
                    module.GroupId == (int)Group.ProjectileWeapon ||
                    module.GroupId == (int)Group.CruiseMissileLaunchers ||
                    module.GroupId == (int)Group.RocketLaunchers ||
                    module.GroupId == (int)Group.TorpedoLaunchers ||
                    module.GroupId == (int)Group.StandardMissileLaunchers ||
                    module.GroupId == (int)Group.HeavyMissilelaunchers ||
                    module.GroupId == (int)Group.AssaultMissilelaunchers ||
                    module.GroupId == (int)Group.DefenderMissilelaunchers
                    )
                {
                    //if (Logging.DebugLoadScripts) Logging.Log("Defense", "---Found mod that could take a script [typeid: " + module.TypeId + "][groupID: " + module.GroupId + "][module.CurrentCharges [" + module.CurrentCharges + "]", Logging.White);

                    ModuleNumber++;

                    if (module.IsOverloaded)
                    {
                        if (module.IsPendingOverloading || module.IsPendingStopOverloading)
                        {
                            continue;
                        }
                            
                        //double DamageThresholdToStopOverloading = 1;

                        if (Logging.DebugOverLoadWeapons) Logging.Log("Defense.Overload", "IsOverLoaded - HP [" + Math.Round(module.Hp,2) + "] Damage [" + Math.Round(module.Damage, 2) + "][" + module.TypeId + "]", Logging.Debug);

                        //if (module.Damage > DamageThresholdToStopOverloading)
                        //{
                        //    Logging.Log("Defense.Overload","Damage [" + Math.Round(module.Damage,2) + "] Disable Overloading of Module wTypeID[" + module.TypeId + "]",Logging.Debug);
                        //    return module.ToggleOverload;
                        //    return false;
                        //}
                        
                        continue;
                    }
                    
                    if (!module.IsOverloaded)
                    {
                        if (module.IsPendingOverloading || module.IsPendingStopOverloading)
                        {
                            continue;
                        }

                        //double DamageThresholdToAllowOverLoading = 1;

                        if (Logging.DebugOverLoadWeapons) Logging.Log("Defense.Overload", "Is not OverLoaded - HP [" + Math.Round(module.Hp, 2) + "] Damage [" + Math.Round(module.Damage, 2) + "][" + module.TypeId + "]", Logging.Debug);
                        _nextOverloadAttempt = DateTime.UtcNow.AddSeconds(30);

                        //if (module.Damage < DamageThresholdToAllowOverLoading)
                        //{
                        //    Logging.Log("Defense.Overload", "Damage [" + Math.Round(module.Damage, 2) + "] Enable Overloading of Module wTypeID[" + module.TypeId + "]", Logging.Debug);
                              return module.ToggleOverload;
                        //}

                        //continue;
                    }

                    _nextOverloadAttempt = DateTime.UtcNow.AddSeconds(60);
                    return true;
                }
           
                ModuleNumber++;
                continue;
            }
            ModuleNumber = 0;
            return true;
        }

        private static void ActivateRepairModules()
        {
            //var watch = new Stopwatch();
            if (DateTime.UtcNow < Time.Instance.NextRepModuleAction) //if we just did something wait a fraction of a second
                return;

            ModuleNumber = 0;
            foreach (ModuleCache module in Cache.Instance.Modules)
            {
                if (module.InLimboState)
                    continue;

                double perc;
                double cap;
                if (module.GroupId == (int)Group.ShieldBoosters || 
                    module.GroupId == (int)Group.AncillaryShieldBooster || 
                    module.GroupId == (int)Group.CapacitorInjector)
                {
                    ModuleNumber++;
                    perc = Cache.Instance.ActiveShip.ShieldPercentage;
                    cap = Cache.Instance.ActiveShip.CapacitorPercentage;
                }
                else if (module.GroupId == (int)Group.ArmorRepairer)
                {
                    ModuleNumber++;
                    perc = Cache.Instance.ActiveShip.ArmorPercentage;
                    cap = Cache.Instance.ActiveShip.CapacitorPercentage;
                }
                else
                    continue;

                // Module is either for Cap or Tank recharging, so we look at these separated (or random things will happen, like cap recharging when we need to repair but cap is near max) 
                // Cap recharging
                bool inCombat = Cache.Instance.EntitiesOnGrid.Any(i => i.IsTargetedBy) || Combat.PotentialCombatTargets.Any();
                if (!module.IsActive && inCombat && cap < InjectCapPerc && module.GroupId == (int)Group.CapacitorInjector && module.CurrentCharges > 0)
                {
                    if (module.Click())
                    {
                        perc = Cache.Instance.ActiveShip.ShieldPercentage;
                        Logging.Log("Defense", "Cap: [" + Math.Round(cap, 0) + "%] Capacitor Booster: [" + ModuleNumber + "] activated", Logging.White);    
                    }
                }
                // Shield/Armor recharging
                else if (!module.IsActive && ((inCombat && perc < ActivateRepairModulesAtThisPerc) || (!inCombat && perc < DeactivateRepairModulesAtThisPerc && cap > Panic.SafeCapacitorPct)))
                {
                    if (Cache.Instance.ActiveShip.ShieldPercentage < Statistics.LowestShieldPercentageThisPocket)
                    {
                        Statistics.LowestShieldPercentageThisPocket = Cache.Instance.ActiveShip.ShieldPercentage;
                        Statistics.LowestShieldPercentageThisMission = Cache.Instance.ActiveShip.ShieldPercentage;
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                    }
                    if (Cache.Instance.ActiveShip.ArmorPercentage < Statistics.LowestArmorPercentageThisPocket)
                    {
                        Statistics.LowestArmorPercentageThisPocket = Cache.Instance.ActiveShip.ArmorPercentage;
                        Statistics.LowestArmorPercentageThisMission = Cache.Instance.ActiveShip.ArmorPercentage;
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                    }
                    if (Cache.Instance.ActiveShip.CapacitorPercentage < Statistics.LowestCapacitorPercentageThisPocket)
                    {
                        Statistics.LowestCapacitorPercentageThisPocket = Cache.Instance.ActiveShip.CapacitorPercentage;
                        Statistics.LowestCapacitorPercentageThisMission = Cache.Instance.ActiveShip.CapacitorPercentage;
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                    }
                    if ((Cache.Instance.UnlootedContainers != null) && Statistics.WrecksThisPocket != Cache.Instance.UnlootedContainers.Count())
                        Statistics.WrecksThisPocket = Cache.Instance.UnlootedContainers.Count();

                    if (module.GroupId == (int)Group.AncillaryShieldBooster) //this needs to have a huge delay and it currently does not.
                    {
                        if (module.CurrentCharges > 0)
                        {
                            if (module.Click())
                            {
                                return;    
                            }
                        }
                    }

                    //
                    // if capacitor is really really low, do not make it worse
                    //
                    if (Cache.Instance.ActiveShip.Capacitor < 25)
                        continue;

                    if (Cache.Instance.ActiveShip.CapacitorPercentage < 3)
                        continue;

                    if (module.GroupId == (int) Group.ShieldBoosters || module.GroupId == (int) Group.ArmorRepairer)
                    {
                        if (module.Click())
                        {
                            Time.Instance.StartedBoosting = DateTime.UtcNow;
                            Time.Instance.NextRepModuleAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                        }
                    }
                    
                    if (module.GroupId == (int)Group.ShieldBoosters || module.GroupId == (int)Group.AncillaryShieldBooster)
                    {
                        perc = Cache.Instance.ActiveShip.ShieldPercentage;
                        Logging.Log("Defense", "Shields: [" + Math.Round(perc, 0) + "%] Cap: [" + Math.Round(cap, 0) + "%] Shield Booster: [" + ModuleNumber + "] activated", Logging.White);
                        if (Cache.Instance.ActiveShip.ArmorPercentage * 100 < 100)
                        {
                            Arm.NeedRepair = true; //triggers repairing during panic recovery, and arm
                        }
                    }
                    else if (module.GroupId == (int)Group.ArmorRepairer)
                    {
                        perc = Cache.Instance.ActiveShip.ArmorPercentage;
                        Logging.Log("Defense", "Armor: [" + Math.Round(perc, 0) + "%] Cap: [" + Math.Round(cap, 0) + "%] Armor Repairer: [" + ModuleNumber + "] activated", Logging.White);
                        int aggressiveEntities = Cache.Instance.EntitiesOnGrid.Count(e => e.IsAttacking && e.IsPlayer);
                        if (aggressiveEntities == 0 && Cache.Instance.EntitiesOnGrid.Count(e => e.IsStation) == 1)
                        {
                            Time.Instance.NextDockAction = DateTime.UtcNow.AddSeconds(15);
                            Logging.Log("Defense", "Repairing Armor outside station with no aggro (yet): delaying docking for [15]seconds", Logging.White);
                        }
                        if (Cache.Instance.ActiveShip.StructurePercentage * 100 < 100)
                        {
                            Arm.NeedRepair = true; //triggers repairing during panic recovery, and arm
                        }
                    }

                    //Logging.Log("LowestShieldPercentage(pocket) [ " + Cache.Instance.lowest_shield_percentage_this_pocket + " ] ");
                    //Logging.Log("LowestArmorPercentage(pocket) [ " + Cache.Instance.lowest_armor_percentage_this_pocket + " ] ");
                    //Logging.Log("LowestCapacitorPercentage(pocket) [ " + Cache.Instance.lowest_capacitor_percentage_this_pocket + " ] ");
                    //Logging.Log("LowestShieldPercentage(mission) [ " + Cache.Instance.lowest_shield_percentage_this_mission + " ] ");
                    //Logging.Log("LowestArmorPercentage(mission) [ " + Cache.Instance.lowest_armor_percentage_this_mission + " ] ");
                    //Logging.Log("LowestCapacitorPercentage(mission) [ " + Cache.Instance.lowest_capacitor_percentage_this_mission + " ] ");
                    return;
                }

                if (module.IsActive && (perc >= DeactivateRepairModulesAtThisPerc || module.GroupId == (int)Group.CapacitorInjector))
                {
                    if (module.Click())
                    {
                        Time.Instance.NextRepModuleAction = DateTime.UtcNow.AddMilliseconds(Time.Instance.DefenceDelay_milliseconds);
                        Statistics.RepairCycleTimeThisPocket = Statistics.RepairCycleTimeThisPocket + ((int)DateTime.UtcNow.Subtract(Time.Instance.StartedBoosting).TotalSeconds);
                        Statistics.RepairCycleTimeThisMission = Statistics.RepairCycleTimeThisMission + ((int)DateTime.UtcNow.Subtract(Time.Instance.StartedBoosting).TotalSeconds);
                        Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                        if (module.GroupId == (int)Group.ShieldBoosters || module.GroupId == (int)Group.CapacitorInjector)
                        {
                            perc = Cache.Instance.ActiveShip.ShieldPercentage;
                            Logging.Log("Defense", "Shields: [" + Math.Round(perc, 0) + "%] Cap: [" + Math.Round(cap, 0) + "%] Shield Booster: [" + ModuleNumber + "] deactivated [" + Math.Round(Time.Instance.NextRepModuleAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] sec reactivation delay", Logging.White);
                        }
                        else if (module.GroupId == (int)Group.ArmorRepairer)
                        {
                            perc = Cache.Instance.ActiveShip.ArmorPercentage;
                            Logging.Log("Defense", "Armor: [" + Math.Round(perc, 0) + "%] Cap: [" + Math.Round(cap, 0) + "%] Armor Repairer: [" + ModuleNumber + "] deactivated [" + Math.Round(Time.Instance.NextRepModuleAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] sec reactivation delay", Logging.White);
                        }

                        //Cache.Instance.repair_cycle_time_this_pocket = Cache.Instance.repair_cycle_time_this_pocket + ((int)watch.Elapsed);
                        //Cache.Instance.repair_cycle_time_this_mission = Cache.Instance.repair_cycle_time_this_mission + watch.Elapsed.TotalMinutes;
                        return;    
                    }
                    
                }

                continue;
            }
        }

        private static void ActivateSpeedMod()
        {
            ModuleNumber = 0;
            foreach (ModuleCache SpeedMod in Cache.Instance.Modules.Where(i => i.GroupId == (int)Group.Afterburner))
            {
                ModuleNumber++;

                if (Time.Instance.LastActivatedTimeStamp != null && Time.Instance.LastActivatedTimeStamp.ContainsKey(SpeedMod.ItemId))
                {
                    if (Logging.DebugSpeedMod) Logging.Log("Defense.ActivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] was last activated [" + Math.Round(DateTime.UtcNow.Subtract(Time.Instance.LastActivatedTimeStamp[SpeedMod.ItemId]).TotalSeconds, 0) + "] sec ago", Logging.Debug);
                    if (Time.Instance.LastActivatedTimeStamp[SpeedMod.ItemId].AddMilliseconds(Time.Instance.AfterburnerDelay_milliseconds) > DateTime.UtcNow)
                    {
                        //if (Logging.DebugSpeedMod) Logging.Log("Defense.ActivateSpeedMod", "[" + ModuleNumber + "] was last activated [" + Time.Instance.LastActivatedTimeStamp[SpeedMod.ItemId] + "[" + Time.Instance.AfterburnerDelay_milliseconds + "] > [" + DateTime.UtcNow + "], skip this speed mod", Logging.Debug);
                        continue;
                    }
                }

                if (SpeedMod.InLimboState)
                {
                    if (Logging.DebugSpeedMod) Logging.Log("Defense.ActivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] isActive [" + SpeedMod.IsActive + "]", Logging.Debug);
                    continue;
                }

                //
                // Should we deactivate the module?
                //
                if (Logging.DebugSpeedMod) Logging.Log("Defense.DeactivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] isActive [" + SpeedMod.IsActive + "]", Logging.Debug);

                if (SpeedMod.IsActive)
                {
                    bool deactivate = false;

                    //we cant move in bastion mode, do not try
                    List<ModuleCache> bastionModules = null;
                    bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                    if (bastionModules.Any(i => i.IsActive))
                    {
                        if (Logging.DebugSpeedMod) Logging.Log("EntityCache.DeactivateSpeedMod", "BastionMode is active, we cannot move, deactivating speed module", Logging.Debug);
                        deactivate = true;
                    }

                    if (!Cache.Instance.IsApproachingOrOrbiting(0))
                    {
                        deactivate = true;
                        if (Logging.DebugSpeedMod) Logging.Log("Defense.DeactivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] We are not approaching or orbiting anything: Deactivate [" + deactivate + "]", Logging.Debug);
                    }
                    else if (!Combat.PotentialCombatTargets.Any() && DateTime.UtcNow > Statistics.StartedPocket.AddMinutes(10) && Cache.Instance.ActiveShip.GivenName == Combat.CombatShipName)
                    {
                        deactivate = true;
                        if (Logging.DebugSpeedMod) Logging.Log("Defense.DeactivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] Nothing on grid is attacking and it has been more than 60 seconds since we landed in this pocket. Deactivate [" + deactivate + "]", Logging.Debug);
                    }
                    else if (!NavigateOnGrid.SpeedTank)
                    {
                        // This only applies when not speed tanking
                        if (Cache.Instance.IsApproachingOrOrbiting(0) && Cache.Instance.Approaching != null)
                        {
                            // Deactivate if target is too close
                            if (Cache.Instance.Approaching.Distance < Defense.MinimumPropulsionModuleDistance)
                            {
                                deactivate = true;
                                if (Logging.DebugSpeedMod) Logging.Log("Defense.DeactivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] We are approaching... and [" + Math.Round(Cache.Instance.Approaching.Distance / 1000, 0) + "] is within [" + Math.Round((double)Defense.MinimumPropulsionModuleDistance / 1000, 0) + "] Deactivate [" + deactivate + "]", Logging.Debug);
                            }
                        }
                    }
                    else if (Cache.Instance.ActiveShip.CapacitorPercentage < Defense.MinimumPropulsionModuleCapacitor)
                    {
                        deactivate = true;
                        if (Logging.DebugSpeedMod) Logging.Log("Defense.DeactivateSpeedMod", "[" + ModuleNumber + "][" + SpeedMod.TypeName + "] Capacitor is at [" + Cache.Instance.ActiveShip.CapacitorPercentage + "] which is below MinimumPropulsionModuleCapacitor [" + Defense.MinimumPropulsionModuleCapacitor + "] Deactivate [" + deactivate + "]", Logging.Debug);
                    }

                    if (deactivate)
                    {
                        if (SpeedMod.Click())
                        {
                            if (Logging.DebugSpeedMod) Logging.Log("Defense.DeactivateSpeedMod", "[" + ModuleNumber + "] [" + SpeedMod.TypeName + "] Deactivated", Logging.Debug);
                            return;    
                        }
                    }
                }
                
                //
                // Should we activate the module
                //

                if (!SpeedMod.IsActive && !SpeedMod.InLimboState)
                {
                    bool activate = false;

                    //we cant move in bastion mode, do not try
                    List<ModuleCache> bastionModules = null;
                    bastionModules = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.Bastion && m.IsOnline).ToList();
                    if (bastionModules.Any(i => i.IsActive))
                    {
                        if (Logging.DebugSpeedMod) Logging.Log("EntityCache.ActivateSpeedMod", "BastionMode is active, we cannot move, do not attempt to activate speed module", Logging.Debug);
                        activate = false;
                        return;
                    }

                    if (Cache.Instance.IsApproachingOrOrbiting(0) && Cache.Instance.Approaching != null)
                    {
                        // Activate if target is far enough
                        if (Cache.Instance.Approaching.Distance > Defense.MinimumPropulsionModuleDistance)
                        {
                            activate = true;
                            if (Logging.DebugSpeedMod) Logging.Log("Defense.ActivateSpeedMod", "[" + ModuleNumber + "] SpeedTank is [" + NavigateOnGrid.SpeedTank + "] We are approaching or orbiting and [" + Math.Round(Cache.Instance.Approaching.Distance / 1000, 0) + "k] is within MinimumPropulsionModuleDistance [" + Math.Round((double)Defense.MinimumPropulsionModuleDistance/1000,2) + "] Activate [" + activate + "]", Logging.Debug);
                        }

                        if (NavigateOnGrid.SpeedTank)
                        {
                            activate = true;
                            if (Logging.DebugSpeedMod) Logging.Log("Defense.ActivateSpeedMod", "[" + ModuleNumber + "] We are approaching or orbiting: Activate [" + activate + "]", Logging.Debug);      
                        }
                    }
                    
                    // If we have less then x% cap, do not activate the module
                    //Logging.Log("Defense: Current Cap [" + Cache.Instance.ActiveShip.CapacitorPercentage + "]" + "Settings: minimumPropulsionModuleCapacitor [" + Settings.Instance.MinimumPropulsionModuleCapacitor + "]");
                    if (Cache.Instance.ActiveShip.CapacitorPercentage < Defense.MinimumPropulsionModuleCapacitor)
                    {
                        activate = false;
                        if (Logging.DebugSpeedMod) Logging.Log("Defense.ActivateSpeedMod", "[" + ModuleNumber + "] CapacitorPercentage is [" + Cache.Instance.ActiveShip.CapacitorPercentage + "] which is less than MinimumPropulsionModuleCapacitor [" + Defense.MinimumPropulsionModuleCapacitor + "] Activate [" + activate + "]", Logging.Debug);
                    }

                    if (activate)
                    {
                        if (SpeedMod.Click())
                        {
                            return;    
                        }
                    }
                }

                continue;
            }
        }

        public static void ProcessState()
        {
            // Only pulse state changes every 1.5s
            if (DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < 350) //default: 350ms
                return;
            _lastPulse = DateTime.UtcNow;

            // Thank god stations are safe ! :)
            if (Cache.Instance.InStation)
            {
                _trackingLinkScriptAttempts = 0;
                _sensorBoosterScriptAttempts = 0;
                _sensorDampenerScriptAttempts = 0;
                _trackingComputerScriptAttempts = 0;
                _trackingDisruptorScriptAttempts = 0;
                _nextOverloadAttempt = DateTime.UtcNow;
                return;
            }

            if (DateTime.UtcNow.AddSeconds(-2) > Time.Instance.LastInSpace)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "it was more than 2 seconds ago since we thought we were in space", Logging.White);
                return;
            }

            if (!Cache.Instance.InSpace)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "we are not in space (yet?)", Logging.White);
                Time.Instance.LastSessionChange = DateTime.UtcNow;
                return;
            }

            // What? No ship entity?
            if (Cache.Instance.ActiveShip.Entity == null || Cache.Instance.ActiveShip.GroupId == (int) Group.Capsule)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "no ship entity, or we are in a pod...", Logging.White);
                Time.Instance.LastSessionChange = DateTime.UtcNow;
                return;
            }

            if (DateTime.UtcNow.Subtract(Time.Instance.LastSessionChange).TotalSeconds < 15)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "we just completed a session change less than 7 seconds ago... waiting.", Logging.White);
                _nextOverloadAttempt = DateTime.UtcNow;
                return;
            }

            // There is no better defense then being cloaked ;)
            if (Cache.Instance.ActiveShip.Entity.IsCloaked)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "we are cloaked... no defense needed.", Logging.White);
                _lastCloaked = DateTime.UtcNow;
                return;
            }

            if (Defense.DoNotBreakInvul)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "DoNotBreakInvul == true, not running defense yet as that will break invulnerability", Logging.White);
                return;
            }

            if (DateTime.UtcNow.Subtract(_lastCloaked).TotalSeconds < 2)
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "we are cloaked.... waiting.", Logging.White);
                return;
            }

            if (DateTime.UtcNow.AddHours(-10) > Time.Instance.WehaveMoved &&
                DateTime.UtcNow < Time.Instance.LastInStation.AddSeconds(20))
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "we have not moved yet after jumping or undocking... waiting.", Logging.White);
                //
                // we reset this datetime stamp to -7 days when we jump, and set it to DateTime.UtcNow when we move (to deactivate jump cloak!)
                // once we have moved (warp, orbit, dock, etc) this should be false and before that it will be true
                //
                return;
            }

            if (Cache.Instance.ActiveShip.CapacitorPercentage < 10 && !Combat.TargetedBy.Any())
            {
                if (Logging.DebugDefense) Logging.Log("Defense", "Cap is SO low that we should not care about hardeners/boosters as we are not being targeted anyhow)", Logging.White);
                return;
            }

            if (Logging.DebugDefense) Logging.Log("Defense", "Starting ActivateRepairModules();", Logging.White);
            ActivateRepairModules();
            if (Logging.DebugDefense) Logging.Log("Defense", "Starting ActivateOnce();", Logging.White);
            ActivateOnce();

            if (Cache.Instance.InWarp)
            {
                _trackingLinkScriptAttempts = 0;
                _sensorBoosterScriptAttempts = 0;
                _sensorDampenerScriptAttempts = 0;
                _trackingComputerScriptAttempts = 0;
                _trackingDisruptorScriptAttempts = 0;
                return;
            }

            // this allows speed mods only when not paused, which is expected behavior
            if (!Cache.Instance.Paused)
            {
                if (Logging.DebugDefense || Logging.DebugSpeedMod) Logging.Log("Defense", "Starting ActivateSpeedMod();", Logging.White);
                ActivateSpeedMod();
            }

            return;
        }
    }
}