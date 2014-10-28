﻿
using System.Reflection;
using LavishSettingsAPI;

namespace Questor.Modules.Logging
{
    using System;
    using System.Linq;
    using DirectEve;
    using System.IO;
    using System.Globalization;
    using System.Collections.Generic;
    using Questor.Modules.Actions;
    using Questor.Modules.BackgroundTasks;
    using Questor.Modules.Caching;
    using Questor.Modules.Combat;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;

    public class Statistics
    {
        public StatisticsState State { get; set; }

        //private DateTime _lastStatisticsAction;
        public DateTime MissionLoggingStartedTimestamp { get; set; }

        public DateTime StartedMission = DateTime.UtcNow;
        public DateTime FinishedMission = DateTime.UtcNow;
        public DateTime StartedSalvaging = DateTime.UtcNow;
        public DateTime FinishedSalvaging = DateTime.UtcNow;
        public DateTime StartedPocket = DateTime.UtcNow;
        public int LootValue { get; set; }
        public int LoyaltyPoints { get; set; }
        public int LostDrones { get; set; }
        public int DroneRecalls { get; set; }
        public int AmmoConsumption { get; set; }
        public int AmmoValue { get; set; }
        public int MissionsThisSession { get; set; }
        public int MissionCompletionErrors { get; set; }

        public int OutOfDronesCount { get; set; }

        public static int AgentLPRetrievalAttempts { get; set; }

        public bool MissionLoggingCompleted; //false
        public bool DroneLoggingCompleted; //false
        public long AgentID { get; set; }
        //private bool PocketLoggingCompleted = false;
        //private bool SessionLoggingCompleted = false;

        public bool MissionLoggingStarted = true;

        public static DateTime DateTimeForLogs;

        /// <summary>
        ///   Singleton implementation
        /// </summary>
        private static readonly Statistics _instance = new Statistics();

        public DateTime LastMissionCompletionError;

        public static Statistics Instance
        {
            get { return _instance; }
        }

        public double TimeInCurrentMission()
        {
            double missiontimeMinutes = Math.Round(DateTime.UtcNow.Subtract(Statistics.Instance.StartedMission).TotalMinutes, 0);
            return missiontimeMinutes;
        }

        public static bool WreckStatistics(IEnumerable<ItemCache> items, EntityCache containerEntity)
        {
            //if (Settings.Instance.DateTimeForLogs = EveTime)
            //{
            //    DateTimeForLogs = DateTime.UtcNow;
            //}
            //else //assume LocalTime
            //{
            DateTimeForLogs = DateTime.Now;
            //}

            if (Settings.Instance.WreckLootStatistics)
            {
                if (containerEntity != null)
                {
                    // Log all items found in the wreck
                    File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "TIME: " + string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTimeForLogs) + "\n");
                    File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "NAME: " + containerEntity.Name + "\n");
                    File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "ITEMS:" + "\n");
                    foreach (ItemCache item in items.OrderBy(i => i.TypeId))
                    {
                        File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "TypeID: " + item.TypeId.ToString(CultureInfo.InvariantCulture) + "\n");
                        File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "Name: " + item.Name + "\n");
                        File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "Quantity: " + item.Quantity.ToString(CultureInfo.InvariantCulture) + "\n");
                        File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, "=\n");
                    }
                    File.AppendAllText(Settings.Instance.WreckLootStatisticsFile, ";" + "\n");
                }
            }
            return true;
        }

        public static bool LogWindowActionToWindowLog(string Windowname, string Description)
        {
            try
            {
                string textToLogToFile;
                if (!File.Exists(Settings.Instance.WindowStatslogFile))
                {
                    //
                    // build header
                    //
                    textToLogToFile = "WindowName;Description;Time;Seconds Since LastInSpace;Seconds Since LastInStation;Seconds Since We Started;\r\n";
                    File.AppendAllText(Settings.Instance.WindowStatslogFile, textToLogToFile);
                }

                //
                // header should already be built.
                //
                textToLogToFile = Windowname + ";" + Description +  ";" + DateTime.UtcNow.ToShortTimeString() + ";" + Time.Instance.LastInSpace.Subtract(DateTime.UtcNow).TotalSeconds + ";" + Time.Instance.LastInStation.Subtract(DateTime.UtcNow).TotalSeconds + ";" + Time.Instance.QuestorStarted_DateTime.Subtract(DateTime.UtcNow).TotalSeconds + ";";
                textToLogToFile += "\r\n";

                //Logging.Log("Statistics", ";PocketObjectStatistics;" + objectline, Logging.White);
                File.AppendAllText(Settings.Instance.WindowStatslogFile, textToLogToFile);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log("Statistics", "Exception while logging to file [" + ex.Message + "]", Logging.White);
                return false;
            }
        }

        public static bool PocketObjectStatistics(List<EntityCache> things, bool force = false)
        {
            if (Settings.Instance.PocketObjectStatisticsLog || force)
            {
                string currentPocketName = Cache.Instance.FilterPath("Random-Grid");
                try
                {
                    if (!String.IsNullOrEmpty(Cache.Instance.MissionName))
                    {
                        currentPocketName = Cache.Instance.FilterPath(Cache.Instance.MissionName);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Statistics", "PocketObjectStatistics: is cache.Instance.MissionName null?: exception was [" + ex.Message + "]", Logging.White);
                }

                Settings.Instance.PocketObjectStatisticsFile = Path.Combine(
                        Settings.Instance.PocketObjectStatisticsPath,
                        Cache.Instance.FilterPath(Cache.Instance.DirectEve.Me.Name) + " - " + currentPocketName + " - " +
                        Cache.Instance.PocketNumber + " - ObjectStatistics.csv");
                
                Logging.Log("Statistics.ObjectStatistics", "Logging info on the [" + things.Count + "] objects in this pocket to [" + Settings.Instance.PocketObjectStatisticsFile + "]", Logging.White);

                if (File.Exists(Settings.Instance.PocketObjectStatisticsFile))
                {
                    File.Delete(Settings.Instance.PocketObjectStatisticsFile);
                }

                //
                // build header
                //
                string objectline = "Name;Distance;TypeId;GroupId;CategoryId;IsNPC;IsPlayer;TargetValue;Velocity;ID;\r\n";
                //Logging.Log("Statistics",";PocketObjectStatistics;" + objectline,Logging.White);
                File.AppendAllText(Settings.Instance.PocketObjectStatisticsFile, objectline);

                //
                // iterate through entities
                //
                foreach (EntityCache thing in things.OrderBy(i => i.Distance))
                {
                    objectline = thing.Name + ";";
                    objectline += Math.Round(thing.Distance / 1000, 0) + ";";
                    objectline += thing.TypeId + ";";
                    objectline += thing.GroupId + ";";
                    objectline += thing.CategoryId + ";";
                    objectline += thing.IsNpc + ";";
                    objectline += thing.IsPlayer + ";";
                    objectline += thing.TargetValue + ";";
                    objectline += Math.Round(thing.Velocity, 0) + ";";
                    objectline += thing.Id + ";\r\n";

                    //
                    // can we somehow get the X,Y,Z coord? If we could we could use this info to build some kind of grid layout...
                    // or at least know the distances between all the NPCs... thus be able to infer which NPCs were in which 'groups'
                    //

                    //Logging.Log("Statistics", ";PocketObjectStatistics;" + objectline, Logging.White);
                    File.AppendAllText(Settings.Instance.PocketObjectStatisticsFile, objectline);
                }
            }
            return true;
        }

        public static bool LogEntities(List<EntityCache> things, bool force = false)
        {
            // iterate through entities
            //
            Logging.Log("Entities", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            things = things.ToList();
            if (things.Any())
            {
                int icount = 0;
                foreach (EntityCache thing in things.OrderBy(i => i.Distance))
                {
                    icount++;
                    Logging.Log(icount.ToString(), thing.Name + "[" + Math.Round(thing.Distance / 1000, 0) + "k] GroupID[" + thing.GroupId + "] ID[" + Cache.Instance.MaskedID(thing.Id) + "] isSentry[" + thing.IsSentry + "] IsHVT[" + thing.IsHighValueTarget + "] IsLVT[" + thing.IsLowValueTarget + "] IsIgnored[" + thing.IsIgnored + "]", Logging.Debug);
                }
            }
            Logging.Log("Entities", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);
            
            return true;
        }

        public static bool ListItems(IEnumerable<ItemCache> ItemsToList)
        {
            Logging.Log("Items", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            ItemsToList = ItemsToList.ToList();
            if (ItemsToList.Any())
            {

                int icount = 0;
                foreach (ItemCache item in ItemsToList.OrderBy(i => i.TypeId).ThenBy(i => i.GroupId))
                {
                    icount++;
                    Logging.Log(icount.ToString(), "[" + item.Name + "] GroupID [" + item.GroupId + "], IsContraband [" + item.IsContraband + "]", Logging.Debug);
                }
            }
            Logging.Log("Items", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);
            
            return true;
        }

        public static bool ModuleInfo(IEnumerable<ModuleCache> _modules)
        {
            Logging.Log("ModuleInfo", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            _modules = _modules.ToList();
            if (_modules != null && _modules.Any())
            {

                int icount = 0;
                foreach (ModuleCache _module in _modules.OrderBy(i => i.TypeId).ThenBy(i => i.GroupId))
                {
                    icount++;
                    Logging.Log(icount.ToString(), "TypeID [" + _module.TypeId + "] GroupID [" + _module.GroupId + "] isOnline [" + _module.IsOnline + "] isActivatable [" + _module.IsActivatable + "] IsActive [" + _module.IsActive + "] OptimalRange [" + _module.OptimalRange + "] Falloff [" + _module.FallOff + "] Duration [" + _module.Duration + "] IsActive [" + _module.IsActive + "]", Logging.Debug);
                }
            }
            Logging.Log("ModuleInfo", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);
            //Logging.Log("WeaponInfo", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            //
            //if (Cache.Instance.Weapons != null && Cache.Instance.Weapons.Any())
            //{
            //
            //    int iCount = 0;
            //    foreach (ModuleCache weapon in Cache.Instance.Weapons.OrderBy(i => i.TypeId).ThenBy(i => i.GroupId))
            //    {
            //        iCount++;
            //        Logging.Log(icount.ToString(), "TypeID [" + weapon.TypeId + "] GroupID [" + weapon.GroupId + "] isOnline [" + weapon.IsOnline + "] isActivatable [" + weapon.IsActivatable + "] IsActive [" + weapon.IsActive + "] OptimalRange [" + weapon.OptimalRange + "] Falloff [" + weapon.FallOff + "] Duration [" + weapon.Duration + "] LastReload [" + Math.Round(DateTime.UtcNow.Subtract(Combat.LastWeaponReload[weapon.ItemId]).TotalSeconds, 0) + "]", Logging.Debug);
            //    }
            //}
            //Logging.Log("WeaponInfo", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);


            return true;
        }

        public static bool ListClassInstanceInfo()
        {
            Logging.Log("debug", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            if (Cache.Instance.EntitiesOnGrid.Any())
            {
                //Logging.Log("debug", "Entities: [" + Cache.Instance.Entities.Count() + "] EntityCache  Class Instances: [" + EntityCache.EntityCacheInstances + "]", Logging.Debug);
                Logging.Log("debug", "InvType Class Instances: [" + InvType.InvTypeInstances + "]", Logging.Debug); 
                Logging.Log("debug", "Defense Class Instances: [" + Defense.DefenseInstances + "]", Logging.Debug);
                Logging.Log("debug", "Arm Class Instances: [" + Arm.ArmInstances + "]", Logging.Debug);
                Logging.Log("debug", "Salvage Class Instances: [" + Salvage.SalvageInstances + "]", Logging.Debug);
                Logging.Log("debug", "Combat Class Instances: [" + Combat.CombatInstances + "]", Logging.Debug);
                Logging.Log("debug", "Drone Class Instances: [" + Drones.DronesInstances + "]", Logging.Debug);
                Logging.Log("debug", "Logging Class Instances: [" + Logging.LoggingInstances + "]", Logging.Debug);
                Logging.Log("debug", "Cache Class Instances: [" + Cache.CacheInstances + "]", Logging.Debug);
                Logging.Log("debug", "Settings Class Instances: [" + Settings.SettingsInstances + "]", Logging.Debug);
            }
            Logging.Log("debug", "--------------------------- Done  (listed above) -----------------------------", Logging.Yellow);
            
            
            return true;
        }

        public static bool ListIgnoredTargets()
        {
            Logging.Log("IgnoreTargets", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            Logging.Log("IgnoreTargets", "Note: Ignore Targets are based on Text Matching. If you ignore: Angel Warlord you ignore all of them on the field!", Logging.Debug); 
            if (Cache.Instance.IgnoreTargets.Any())
            {

                int icount = 0;
                foreach (string ignoreTarget in Cache.Instance.IgnoreTargets)
                {
                    icount++;
                    Logging.Log(icount.ToString(),"[" + ignoreTarget + "] of a total of [" + Cache.Instance.IgnoreTargets.Count() + "]", Logging.Debug);
                }
            }
            Logging.Log("IgnoreTargets", "--------------------------- Done  (listed above) -----------------------------", Logging.Yellow);
            return true;
        }

        public static bool ListDronePriorityTargets(IEnumerable<EntityCache> primaryDroneTargets)
        {
            Logging.Log("DPT", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            if (Cache.Instance.PreferredDroneTarget != null)
            {
                Logging.Log("DPT", "[" + 0 + "] PreferredDroneTarget [" + Cache.Instance.PreferredDroneTarget.Name + "][" + Math.Round(Cache.Instance.PreferredDroneTarget.Distance / 1000, 0) + "k] IsInOptimalRange [" + Cache.Instance.PreferredDroneTarget.IsInOptimalRange + "] IsTarget [" + Cache.Instance.PreferredDroneTarget.IsTarget + "]", Logging.Debug);
            }

            primaryDroneTargets = primaryDroneTargets.ToList();
            if (primaryDroneTargets.Any())
            {
                int icount = 0;
                foreach (EntityCache dronePriorityTarget in primaryDroneTargets.OrderBy(i => i.DronePriorityLevel).ThenBy(i => i.Name))
                {
                    icount++;
                    Logging.Log(icount.ToString(), "[" + dronePriorityTarget.Name + "][" + Math.Round(dronePriorityTarget.Distance / 1000, 0) + "k] IsInOptimalRange [" + dronePriorityTarget.IsInOptimalRange + "] IsTarget [" + dronePriorityTarget.IsTarget + "] DronePriorityLevel [" + dronePriorityTarget.DronePriorityLevel + "]", Logging.Debug);
                }
            }
            Logging.Log("DPT", "--------------------------- Done  (listed above) -----------------------------", Logging.Yellow);
            return true;
        }

        public static bool ListTargetedandTargeting(IEnumerable<EntityCache> targetedandTargeting)
        {
            Logging.Log("List", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            targetedandTargeting = targetedandTargeting.ToList();
            if (targetedandTargeting.Any())
            {
                int icount = 0;
                foreach (EntityCache targetedandTargetingEntity in targetedandTargeting.OrderBy(i => i.Distance).ThenBy(i => i.Name))
                {
                    icount++;
                    Logging.Log(icount.ToString(), "[" + targetedandTargetingEntity.Name + "][" + Math.Round(targetedandTargetingEntity.Distance / 1000, 0) + "k] IsIgnored [" + targetedandTargetingEntity.IsIgnored + "] IsInOptimalRange [" + targetedandTargetingEntity.IsInOptimalRange + "] isTarget [" + targetedandTargetingEntity.IsTarget + "] isTargeting [" + targetedandTargetingEntity.IsTargeting + "] IsPrimaryWeaponPriorityTarget [" + targetedandTargetingEntity.IsPrimaryWeaponPriorityTarget + "] IsDronePriorityTarget [" + targetedandTargetingEntity.IsDronePriorityTarget + "]", Logging.Debug);
                }
            }
            Logging.Log("List", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);
            return true;
        }

        public static bool EntityStatistics(IEnumerable<EntityCache> things)
        {
            string objectline = "Name;Distance;TypeId;GroupId;CategoryId;IsNPC;IsNPCByGroupID;IsPlayer;TargetValue;Velocity;HaveLootRights;IsContainer;ID;\r\n";
            Logging.Log("Statistics", ";EntityStatistics;" + objectline, Logging.White);

            things = things.ToList();

            if (!things.Any()) //if their are no entries, return
            {
                Logging.Log("Statistics", "EntityStatistics: No entries to log", Logging.White);
                return true;
            }

            foreach (EntityCache thing in things.OrderBy(i => i.Distance))
            {
                objectline = thing.Name + ";";
                objectline += Math.Round(thing.Distance / 1000, 0) + ";";
                objectline += thing.TypeId + ";";
                objectline += thing.GroupId + ";";
                objectline += thing.CategoryId + ";";
                objectline += thing.IsNpc + ";";
                objectline += thing.IsNpcByGroupID + ";";
                objectline += thing.IsPlayer + ";";
                objectline += thing.TargetValue + ";";
                objectline += Math.Round(thing.Velocity, 0) + ";";
                objectline += thing.HaveLootRights + ";";
                objectline += thing.IsContainer + ";";
                objectline += thing.Id + ";\r\n";

                //
                // can we somehow get the X,Y,Z coord? If we could we could use this info to build some kinda mission simulator...
                // or at least know the distances between all the NPCs... thus be able to infer which NPCs were in which 'groups'
                //

                Logging.Log("Statistics", ";EntityStatistics;" + objectline, Logging.White);
            }
            return true;
        }

        public static bool IndividualVolleyDataStatistics(List<EachWeaponsVolleyCache> ListOfVolleyDataToCrunch )
        {
            try
            {
                if (Settings.Instance.VolleyStatsLog)
                {
                    if (!Directory.Exists(Settings.Instance.VolleyStatsLogPath))
                    {
                        Directory.CreateDirectory(Settings.Instance.VolleyStatsLogPath);
                    }

                    Settings.Instance.VolleyStatslogFile = System.IO.Path.Combine(Settings.Instance.VolleyStatsLogPath, Settings.Instance.characterNameForLogs + ".VolleyStats[" + DateTime.UtcNow.DayOfYear + "].log");

                    if (!File.Exists(Settings.Instance.VolleyStatslogFile))
                    {
                        File.AppendAllText(Settings.Instance.VolleyStatslogFile, "VolleyNumber;TimeOfTheVolley;targetName;targetDistance;moduleName;moduleAmmoTypeName;myShipName;pocketNumber;missionName;systemName;moduleCurrentCharges;moduleFalloff;moduleItemID;moduleOptimal;moduleTargetID;moduleTypeID;myShipShieldPercentage;myShipArmorPercentage;myShipHullPercentage;myShipCapacitorPercentage;myShipVelocity;myShipXCoordinate;myShipYCoordinate;myShipZCoordinate;targetTransversalVelocity;targetAngularVelocity;targetXCoordinate;targetYCoordinate;targetZCoordinate\r\n");
                    }

                    foreach (EachWeaponsVolleyCache individualVolleyDataEntry in ListOfVolleyDataToCrunch)
                    {
                        string objectline = individualVolleyDataEntry.thisWasVolleyNumber + ";";
                        objectline += individualVolleyDataEntry.ThisVolleyCacheCreated.ToShortTimeString() + ";";
                        objectline += individualVolleyDataEntry.targetName + ";";
                        objectline += Math.Round(individualVolleyDataEntry.targetDistance / 1000, 2) + ";";
                        objectline += individualVolleyDataEntry.targetVelocity + ";";
                        objectline += individualVolleyDataEntry.moduleName + ";";
                        objectline += individualVolleyDataEntry.moduleAmmoTypeName + ";";
                        objectline += individualVolleyDataEntry.myShipName + ";";
                        objectline += individualVolleyDataEntry.pocketNumber + ";";
                        objectline += individualVolleyDataEntry.missionName + ";";
                        objectline += individualVolleyDataEntry.systemName + ";";
                        objectline += individualVolleyDataEntry.moduleCurrentCharges + ";";
                        objectline += individualVolleyDataEntry.moduleFalloff + ";";
                        objectline += individualVolleyDataEntry.moduleItemID + ";";
                        objectline += individualVolleyDataEntry.moduleOptimal + ";";
                        objectline += individualVolleyDataEntry.moduleTargetID + ";";
                        objectline += individualVolleyDataEntry.moduleTypeID + ";";
                        objectline += individualVolleyDataEntry.myShipShieldPercentage + ";";
                        objectline += individualVolleyDataEntry.myShipArmorPercentage + ";";
                        objectline += individualVolleyDataEntry.myShipHullPercentage + ";";
                        objectline += individualVolleyDataEntry.myShipCapacitorPercentage + ";";
                        objectline += individualVolleyDataEntry.myShipVelocity + ";";
                        objectline += individualVolleyDataEntry.myShipXCoordinate + ";";
                        objectline += individualVolleyDataEntry.myShipYCoordinate + ";";
                        objectline += individualVolleyDataEntry.myShipZCoordinate + ";";
                        objectline += individualVolleyDataEntry.targetTransversalVelocity + ";";
                        objectline += individualVolleyDataEntry.targetAngularVelocity + ";";
                        objectline += individualVolleyDataEntry.targetXCoordinate + ";";
                        objectline += individualVolleyDataEntry.targetYCoordinate + ";";
                        objectline += individualVolleyDataEntry.targetZCoordinate + ";";
                        objectline += "\r\n";

                        File.AppendAllText(Settings.Instance.VolleyStatslogFile, objectline);
                    }    
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Statistics", "IndividualVolleyDataStatistics - Exception: [" + exception + "]", Logging.Red);
                return false;
            }
            
            return true;
        }

        public static bool AmmoConsumptionStatistics()
        {
            // Ammo Consumption statistics
            // Is cargo open?
            if (Cache.Instance.CurrentShipsCargo == null)
            {
                Logging.Log("AmmoConsumptionStatistics", "if (Cache.Instance.CurrentShipsCargo == null)", Logging.Teal);
                return false;
            }

            IEnumerable<Ammo> correctAmmo1 = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.MissionDamageType);
            IEnumerable<DirectItem> ammoCargo = Cache.Instance.CurrentShipsCargo.Items.Where(i => correctAmmo1.Any(a => a.TypeId == i.TypeId));
            try
            {
                foreach (DirectItem item in ammoCargo)
                {
                    Ammo ammo1 = Settings.Instance.Ammo.FirstOrDefault(a => a.TypeId == item.TypeId);
                    DirectInvType ammoType;
                    Cache.Instance.DirectEve.InvTypes.TryGetValue(item.TypeId, out ammoType);
                    if (ammo1 != null) Statistics.Instance.AmmoConsumption = (ammo1.Quantity - item.Quantity);

                    //TODO: disable this piece of code since AveragePrice is not provided by DirectEve anymore
                    //Statistics.Instance.AmmoValue = ((int?)ammoType.AveragePrice ?? 0) * Statistics.Instance.AmmoConsumption;
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Statistics.AmmoConsumptionStatistics","Exception: " + exception,Logging.Debug);
            }

            return true;
        }

        public static bool WriteDroneStatsLog()
        {
            //if (Settings.Instance.DateTimeForLogs = EveTime)
            //{
            //    DateTimeForLogs = DateTime.UtcNow;
            //}
            //else //assume LocalTime
            //{
            DateTimeForLogs = DateTime.Now;

            //}

            if (Settings.Instance.DroneStatsLog && !Statistics.Instance.DroneLoggingCompleted)
            {
                // Lost drone statistics
                if (Settings.Instance.UseDrones &&
                     Cache.Instance.ActiveShip.GroupId != (int)Group.Capsule &&
                     Cache.Instance.ActiveShip.GroupId != (int)Group.Shuttle &&
                     Cache.Instance.ActiveShip.GroupId != (int)Group.Frigate &&
                     Cache.Instance.ActiveShip.GroupId != (int)Group.Industrial &&
                     Cache.Instance.ActiveShip.GroupId != (int)Group.TransportShip &&
                     Cache.Instance.ActiveShip.GroupId != (int)Group.Freighter)
                {
                    if (!File.Exists(Settings.Instance.DroneStatslogFile))
                    {
                        File.AppendAllText(Settings.Instance.DroneStatslogFile, "Date;Mission;Number of lost drones;# of Recalls\r\n");
                    }

                    string droneline = DateTimeForLogs.ToShortDateString() + ";";
                    droneline += DateTimeForLogs.ToShortTimeString() + ";";
                    droneline += Cache.Instance.MissionName + ";";
                    droneline += Statistics.Instance.LostDrones + ";";
                    droneline += +Statistics.Instance.DroneRecalls + ";\r\n";
                    File.AppendAllText(Settings.Instance.DroneStatslogFile, droneline);
                    Statistics.Instance.DroneLoggingCompleted = true;
                }
                else
                {
                    Logging.Log("DroneStats", "We do not use drones in this type of ship, skipping drone stats", Logging.White);
                    Statistics.Instance.DroneLoggingCompleted = true;
                }
            }

            // Lost drone statistics stuff ends here
            return true;
        }

        public static void WriteSessionLogStarting()
        {
            //if (Settings.Instance.DateTimeForLogs = EVETIme)
            //{
            //    DateTimeForLogs = DateTime.UtcNow;
            //}
            //else //assume LocalTime
            //{
            DateTimeForLogs = DateTime.Now;

            //}

            if (Settings.Instance.SessionsLog)
            {
                if (Cache.Instance.MyWalletBalance != 0 || Cache.Instance.MyWalletBalance != -2147483648) // this hopefully resolves having negative max int in the session logs occasionally
                {
                    //
                    // prepare the Questor Session Log - keeps track of starts, restarts and exits, and hopefully the reasons
                    //
                    // Get the path
                    if (!Directory.Exists(Settings.Instance.SessionsLogPath))
                    {
                        Directory.CreateDirectory(Settings.Instance.SessionsLogPath);
                    }

                    // Write the header
                    if (!File.Exists(Settings.Instance.SessionsLogFile))
                    {
                        File.AppendAllText(Settings.Instance.SessionsLogFile, "Date;RunningTime;SessionState;LastMission;WalletBalance;MemoryUsage;Reason;IskGenerated;LootGenerated;LPGenerated;Isk/Hr;Loot/Hr;LP/HR;Total/HR;\r\n");
                    }

                    // Build the line
                    string line = DateTimeForLogs + ";";                           //Date
                    line += "0" + ";";                                          //RunningTime
                    line += Cache.Instance.SessionState + ";";                  //SessionState
                    line += "" + ";";                                           //LastMission
                    line += Cache.Instance.MyWalletBalance + ";";               //WalletBalance
                    line += Cache.Instance.TotalMegaBytesOfMemoryUsed + ";";    //MemoryUsage
                    line += "Starting" + ";";                                   //Reason
                    line += ";";                                                //IskGenerated
                    line += ";";                                                //LootGenerated
                    line += ";";                                                //LPGenerated
                    line += ";";                                                //Isk/Hr
                    line += ";";                                                //Loot/Hr
                    line += ";";                                                //LP/HR
                    line += ";\r\n";                                            //Total/HR

                    // The mission is finished
                    File.AppendAllText(Settings.Instance.SessionsLogFile, line);

                    Cache.Instance.SessionState = "";
                    Logging.Log("Statistics: WriteSessionLogStarting", "Writing session data to [ " + Settings.Instance.SessionsLogFile + " ]", Logging.White);
                }
            }
        }

        public static bool WriteSessionLogClosing()
        {
            //if (Settings.Instance.DateTimeForLogs = EVETIme)
            //{
            //    DateTimeForLogs = DateTime.UtcNow;
            //}
            //else //assume LocalTime
            //{
            DateTimeForLogs = DateTime.Now;

            //}

            if (Settings.Instance.SessionsLog) // if false we do not write a session log, doubles as a flag so we don't write the session log more than once
            {
                //
                // prepare the Questor Session Log - keeps track of starts, restarts and exits, and hopefully the reasons
                //

                // Get the path

                if (!Directory.Exists(Settings.Instance.SessionsLogPath))
                {
                    Directory.CreateDirectory(Settings.Instance.SessionsLogPath);
                }

                Cache.Instance.SessionIskPerHrGenerated = ((int)Cache.Instance.SessionIskGenerated / (DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalMinutes / 60));
                Cache.Instance.SessionLootPerHrGenerated = ((int)Cache.Instance.SessionLootGenerated / (DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalMinutes / 60));
                Cache.Instance.SessionLPPerHrGenerated = (((int)Cache.Instance.SessionLPGenerated * (int)Settings.Instance.IskPerLP) / (DateTime.UtcNow.Subtract(Time.Instance.QuestorStarted_DateTime).TotalMinutes / 60));
                Cache.Instance.SessionTotalPerHrGenerated = ((int)Cache.Instance.SessionIskPerHrGenerated + (int)Cache.Instance.SessionLootPerHrGenerated + (int)Cache.Instance.SessionLPPerHrGenerated);
                Logging.Log("QuestorState.CloseQuestor", "Writing Session Data [1]", Logging.White);

                // Write the header
                if (!File.Exists(Settings.Instance.SessionsLogFile))
                {
                    File.AppendAllText(Settings.Instance.SessionsLogFile, "Date;RunningTime;SessionState;LastMission;WalletBalance;MemoryUsage;Reason;IskGenerated;LootGenerated;LPGenerated;Isk/Hr;Loot/Hr;LP/HR;Total/HR;\r\n");
                }

                // Build the line
                string line = DateTimeForLogs + ";";                               // Date
                line += Cache.Instance.SessionRunningTime + ";";                // RunningTime
                line += Cache.Instance.SessionState + ";";                      // SessionState
                line += Cache.Instance.MissionName + ";";                       // LastMission
                line += Cache.Instance.MyWalletBalance + ";";                   // WalletBalance
                line += ((int)Cache.Instance.TotalMegaBytesOfMemoryUsed + ";"); // MemoryUsage
                line += Cache.Instance.ReasonToStopQuestor + ";";               // Reason to Stop Questor
                line += Cache.Instance.SessionIskGenerated + ";";               // Isk Generated This Session
                line += Cache.Instance.SessionLootGenerated + ";";              // Loot Generated This Session
                line += Cache.Instance.SessionLPGenerated + ";";                // LP Generated This Session
                line += Cache.Instance.SessionIskPerHrGenerated + ";";          // Isk Generated per hour this session
                line += Cache.Instance.SessionLootPerHrGenerated + ";";         // Loot Generated per hour This Session
                line += Cache.Instance.SessionLPPerHrGenerated + ";";           // LP Generated per hour This Session
                line += Cache.Instance.SessionTotalPerHrGenerated + ";\r\n";    // Total Per Hour This Session

                // The mission is finished
                Logging.Log("Statistics: WriteSessionLogClosing", line, Logging.White);
                File.AppendAllText(Settings.Instance.SessionsLogFile, line);

                Logging.Log("Statistics: WriteSessionLogClosing", "Writing to session log [ " + Settings.Instance.SessionsLogFile, Logging.White);
                Logging.Log("Statistics: WriteSessionLogClosing", "Questor is stopping because: " + Cache.Instance.ReasonToStopQuestor, Logging.White);
                Settings.Instance.SessionsLog = false; //so we don't write the session log more than once per session
            }
            return true;
        }

        public static void WritePocketStatistics()
        {
            //if (Settings.Instance.DateTimeForLogs = EVETIme)
            //{
            //    DateTimeForLogs = DateTime.UtcNow;
            //}
            //else //assume LocalTime
            //{
            DateTimeForLogs = DateTime.Now;

            //}

            // We are not supposed to create bookmarks
            //if (!Settings.Instance.LogBounties)
            //    return;

            //agentID needs to change if its a storyline mission - so its assigned in storyline.cs to the various modules directly.
            string currentPocketName = Cache.Instance.FilterPath(Cache.Instance.MissionName);
            if (Settings.Instance.PocketStatistics)
            {
                if (Settings.Instance.PocketStatsUseIndividualFilesPerPocket)
                {
                    Settings.Instance.PocketStatisticsFile = Path.Combine(Settings.Instance.PocketStatisticsPath, Cache.Instance.FilterPath(Cache.Instance.DirectEve.Me.Name) + " - " + currentPocketName + " - " + Cache.Instance.PocketNumber + " - PocketStatistics.csv");
                }
                if (!Directory.Exists(Settings.Instance.PocketStatisticsPath))
                    Directory.CreateDirectory(Settings.Instance.PocketStatisticsPath);

                //
                // this is writing down stats from the PREVIOUS pocket (if any?!)
                //

                // Write the header
                if (!File.Exists(Settings.Instance.PocketStatisticsFile))
                    File.AppendAllText(Settings.Instance.PocketStatisticsFile, "Date and Time;Mission Name ;Pocket;Time to complete;Isk;panics;LowestShields;LowestArmor;LowestCapacitor;RepairCycles;Wrecks\r\n");

                // Build the line
                string pocketstatsLine = DateTimeForLogs + ";";                                                            //Date
                pocketstatsLine += currentPocketName + ";";                                                                //Mission Name
                pocketstatsLine += "pocket" + (Cache.Instance.PocketNumber) + ";";                                         //Pocket number
                pocketstatsLine += ((int)DateTime.UtcNow.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";"; //Time to Complete
                pocketstatsLine += Cache.Instance.MyWalletBalance - Cache.Instance.WealthatStartofPocket + ";";            //Isk
                pocketstatsLine += Cache.Instance.PanicAttemptsThisPocket + ";";                                           //Panics
                pocketstatsLine += ((int)Cache.Instance.LowestShieldPercentageThisPocket) + ";";                           //LowestShields
                pocketstatsLine += ((int)Cache.Instance.LowestArmorPercentageThisPocket) + ";";                            //LowestArmor
                pocketstatsLine += ((int)Cache.Instance.LowestCapacitorPercentageThisPocket) + ";";                        //LowestCapacitor
                pocketstatsLine += Cache.Instance.RepairCycleTimeThisPocket + ";";                                         //repairCycles
                pocketstatsLine += Cache.Instance.WrecksThisPocket + ";";                                                  //wrecksThisPocket
                pocketstatsLine += "\r\n";

                // The old pocket is finished
                Logging.Log("Statistics: WritePocketStatistics", "Writing pocket statistics to [ " + Settings.Instance.PocketStatisticsFile + " ] and clearing stats for next pocket", Logging.White);
                File.AppendAllText(Settings.Instance.PocketStatisticsFile, pocketstatsLine);
            }

            // Update statistic values for next pocket stats
            Cache.Instance.WealthatStartofPocket = Cache.Instance.MyWalletBalance;
            Statistics.Instance.StartedPocket = DateTime.UtcNow;
            Cache.Instance.PanicAttemptsThisPocket = 0;
            Cache.Instance.LowestShieldPercentageThisPocket = 101;
            Cache.Instance.LowestArmorPercentageThisPocket = 101;
            Cache.Instance.LowestCapacitorPercentageThisPocket = 101;
            Cache.Instance.RepairCycleTimeThisPocket = 0;
            Cache.Instance.WrecksThisMission += Cache.Instance.WrecksThisPocket;
            Cache.Instance.WrecksThisPocket = 0;
            Cache.Instance.OrbitEntityNamed = null;
        }

        public static void SaveMissionHTMLDetails(string MissionDetailsHtml, string missionName)
        {
            DateTimeForLogs = DateTime.Now;

            string missionDetailsHtmlFile = Path.Combine(Settings.Instance.MissionDetailsHtmlPath, missionName + " - " + "mission-description-html.txt");
            
            if (!Directory.Exists(Settings.Instance.MissionDetailsHtmlPath))
            {
                Directory.CreateDirectory(Settings.Instance.MissionDetailsHtmlPath);
            }

            // Write the file
            if (!File.Exists(missionDetailsHtmlFile))
            {
                Logging.Log("Statistics: SaveMissionHTMLDetails", "Writing mission details HTML [ " + missionDetailsHtmlFile + " ]", Logging.White);
                File.AppendAllText(missionDetailsHtmlFile, MissionDetailsHtml);    
            }
        }

        public static void WriteMissionStatistics(long statisticsForThisAgent)
        {
            //if (Settings.Instance.DateTimeForLogs = EveTime)
            //{
            //    DateTimeForLogs = DateTime.UtcNow;
            //}
            //else //assume LocalTime
            //{
            DateTimeForLogs = DateTime.Now;

            //}

            if (Cache.Instance.InSpace)
            {
                Logging.Log("Statistics", "We have started questor in space, assume we do not need to write any statistics at the moment.", Logging.Teal);
                Statistics.Instance.MissionLoggingCompleted = true; //if the mission was completed more than 10 min ago assume the logging has been done already.
                return;
            }

            Cache.Instance.Mission = Cache.Instance.GetAgentMission(statisticsForThisAgent, true);
            if (Settings.Instance.DebugStatistics) // we only need to see the following wall of comments if debugging mission statistics
            {
                Logging.Log("Statistics", "...Checking to see if we should create a mission log now...", Logging.White);
                Logging.Log("Statistics", " ", Logging.White);
                Logging.Log("Statistics", " ", Logging.White);
                Logging.Log("Statistics", "The Rules for After Mission Logging are as Follows...", Logging.White);
                Logging.Log("Statistics", "1)  we must have loyalty points with the current agent (disabled at the moment)", Logging.White); //which we already verified if we got this far
                Logging.Log("Statistics", "2) Cache.Instance.MissionName must not be empty - we must have had a mission already this session", Logging.White);
                Logging.Log("Statistics", "AND", Logging.White);
                Logging.Log("Statistics", "3a Cache.Instance.mission == null - their must not be a current mission OR", Logging.White);
                Logging.Log("Statistics", "3b Cache.Instance.mission.State != (int)MissionState.Accepted) - the mission state is not 'Accepted'", Logging.White);
                Logging.Log("Statistics", " ", Logging.White);
                Logging.Log("Statistics", " ", Logging.White);
                Logging.Log("Statistics", "If those are all met then we get to create a log for the previous mission.", Logging.White);

                if (!string.IsNullOrEmpty(Cache.Instance.MissionName)) //condition 1
                {
                    Logging.Log("Statistics", "1 We must have a mission because MissionName is filled in", Logging.White);
                    Logging.Log("Statistics", "1 Mission is: " + Cache.Instance.MissionName, Logging.White);

                    if (Cache.Instance.Mission != null) //condition 2
                    {
                        Logging.Log("Statistics", "2 Cache.Instance.mission is: " + Cache.Instance.Mission, Logging.White);
                        Logging.Log("Statistics", "2 Cache.Instance.mission.Name is: " + Cache.Instance.Mission.Name, Logging.White);
                        Logging.Log("Statistics", "2 Cache.Instance.mission.State is: " + Cache.Instance.Mission.State, Logging.White);

                        if (Cache.Instance.Mission.State != (int)MissionState.Accepted) //condition 3
                        {
                            Logging.Log("Statistics", "MissionState is NOT Accepted: which is correct if we want to do logging", Logging.White);
                        }
                        else
                        {
                            Logging.Log("Statistics", "MissionState is Accepted: which means the mission is not yet complete", Logging.White);
                            Statistics.Instance.MissionLoggingCompleted = true; //if it is not true - this means we should not be trying to log mission stats atm
                        }
                    }
                    else
                    {
                        Logging.Log("Statistics", "mission is NULL - which means we have no current mission", Logging.White);
                        Statistics.Instance.MissionLoggingCompleted = true; //if it is not true - this means we should not be trying to log mission stats atm
                    }
                }
                else
                {
                    Logging.Log("Statistics", "1 We must NOT have had a mission yet because MissionName is not filled in", Logging.White);
                    Statistics.Instance.MissionLoggingCompleted = true; //if it is not true - this means we should not be trying to log mission stats atm
                }
            }

            if (AgentLPRetrievalAttempts > 10)
            {
                Logging.Log("Statistics", "WriteMissionStatistics: We do not have loyalty points with the current agent yet, still -1, attempt # [" + AgentLPRetrievalAttempts + "] giving up", Logging.White);
                AgentLPRetrievalAttempts = 0;
                Statistics.Instance.MissionLoggingCompleted = true; //if it is not true - this means we should not be trying to log mission stats atm
                return;
            }

            // Seeing as we completed a mission, we will have loyalty points for this agent
            if (Cache.Instance.Agent.LoyaltyPoints == -1)
            {
                AgentLPRetrievalAttempts++;
                Logging.Log("Statistics", "WriteMissionStatistics: We do not have loyalty points with the current agent yet, still -1, attempt # [" + AgentLPRetrievalAttempts + "] retrying...", Logging.White);
                return;
            }

            AgentLPRetrievalAttempts = 0;

            Statistics.Instance.MissionsThisSession++;
            if (Settings.Instance.DebugStatistics) Logging.Log("Statistics", "We jumped through all the hoops: now do the mission logging", Logging.White);
            Cache.Instance.SessionIskGenerated = (Cache.Instance.SessionIskGenerated + (Cache.Instance.MyWalletBalance - Cache.Instance.Wealth));
            Cache.Instance.SessionLootGenerated = (Cache.Instance.SessionLootGenerated + Statistics.Instance.LootValue);
            Cache.Instance.SessionLPGenerated = (Cache.Instance.SessionLPGenerated + (Cache.Instance.Agent.LoyaltyPoints - Statistics.Instance.LoyaltyPoints));
            Logging.Log("Statistics", "Printing All Statistics Related Variables to the console log:", Logging.White);
            Logging.Log("Statistics", "Mission Name: [" + Cache.Instance.MissionName + "]", Logging.White);
            Logging.Log("Statistics", "Faction: [" + Cache.Instance.FactionName + "]", Logging.White);
            Logging.Log("Statistics", "System: [" + Cache.Instance.MissionSolarSystem + "]", Logging.White);
            Logging.Log("Statistics", "Total Missions completed this session: [" + Statistics.Instance.MissionsThisSession + "]", Logging.White);
            Logging.Log("Statistics", "StartedMission: [ " + Statistics.Instance.StartedMission + "]", Logging.White);
            Logging.Log("Statistics", "FinishedMission: [ " + Statistics.Instance.FinishedMission + "]", Logging.White);
            Logging.Log("Statistics", "StartedSalvaging: [ " + Statistics.Instance.StartedSalvaging + "]", Logging.White);
            Logging.Log("Statistics", "FinishedSalvaging: [ " + Statistics.Instance.FinishedSalvaging + "]", Logging.White);
            Logging.Log("Statistics", "Wealth before mission: [ " + Cache.Instance.Wealth + "]", Logging.White);
            Logging.Log("Statistics", "Wealth after mission: [ " + Cache.Instance.MyWalletBalance + "]", Logging.White);
            Logging.Log("Statistics", "Value of Loot from the mission: [" + Statistics.Instance.LootValue + "]", Logging.White);
            Logging.Log("Statistics", "Total LP after mission:  [" + Cache.Instance.Agent.LoyaltyPoints + "]", Logging.White);
            Logging.Log("Statistics", "Total LP before mission: [" + Statistics.Instance.LoyaltyPoints + "]", Logging.White);
            Logging.Log("Statistics", "LostDrones: [" + Statistics.Instance.LostDrones + "]", Logging.White);
            Logging.Log("Statistics", "DroneRecalls: [" + Statistics.Instance.DroneRecalls + "]", Logging.White);
            Logging.Log("Statistics", "AmmoConsumption: [" + Statistics.Instance.AmmoConsumption + "]", Logging.White);
            Logging.Log("Statistics", "AmmoValue: [" + Statistics.Instance.AmmoConsumption + "]", Logging.White);
            Logging.Log("Statistics", "Panic Attempts: [" + Cache.Instance.PanicAttemptsThisMission + "]", Logging.White);
            Logging.Log("Statistics", "Lowest Shield %: [" + Math.Round(Cache.Instance.LowestShieldPercentageThisMission, 0) + "]", Logging.White);
            Logging.Log("Statistics", "Lowest Armor %: [" + Math.Round(Cache.Instance.LowestArmorPercentageThisMission, 0) + "]", Logging.White);
            Logging.Log("Statistics", "Lowest Capacitor %: [" + Math.Round(Cache.Instance.LowestCapacitorPercentageThisMission, 0) + "]", Logging.White);
            Logging.Log("Statistics", "Repair Cycle Time: [" + Cache.Instance.RepairCycleTimeThisMission + "]", Logging.White);
            Logging.Log("Statistics", "MissionXMLIsAvailable: [" + Cache.Instance.MissionXMLIsAvailable + "]", Logging.White);
            Logging.Log("Statistics", "MissionCompletionerrors: [" + Statistics.Instance.MissionCompletionErrors + "]", Logging.White);
            Logging.Log("Statistics", "the stats below may not yet be correct and need some TLC", Logging.White);
            int weaponNumber = 0;
            foreach (ModuleCache weapon in Cache.Instance.Weapons)
            {
                weaponNumber++;
                if (Time.Instance.ReloadTimePerModule != null && Time.Instance.ReloadTimePerModule.ContainsKey(weapon.ItemId))
                {
                    Logging.Log("Statistics", "Time Spent Reloading: [" + weaponNumber + "][" + Time.Instance.ReloadTimePerModule[weapon.ItemId] + "]", Logging.White);    
                }
            }
            Logging.Log("Statistics", "Time Spent IN Mission: [" + Cache.Instance.TimeSpentInMission_seconds + "sec]", Logging.White);
            Logging.Log("Statistics", "Time Spent In Range: [" + Cache.Instance.TimeSpentInMissionInRange + "]", Logging.White);
            Logging.Log("Statistics", "Time Spent Out of Range: [" + Cache.Instance.TimeSpentInMissionOutOfRange + "]", Logging.White);

            if (Settings.Instance.MissionStats1Log)
            {
                if (!Directory.Exists(Settings.Instance.MissionStats1LogPath))
                {
                    Directory.CreateDirectory(Settings.Instance.MissionStats1LogPath);
                }

                // Write the header
                if (!File.Exists(Settings.Instance.MissionStats1LogFile))
                {
                    File.AppendAllText(Settings.Instance.MissionStats1LogFile, "Date;Mission;TimeMission;TimeSalvage;TotalTime;Isk;Loot;LP;\r\n");
                }

                // Build the line
                string line = DateTimeForLogs + ";";                                                                                        // Date
                line += Cache.Instance.MissionName + ";";                                                                                   // Mission
                line += ((int)Statistics.Instance.FinishedMission.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";";         // TimeMission
                line += ((int)Statistics.Instance.FinishedSalvaging.Subtract(Statistics.Instance.StartedSalvaging).TotalMinutes) + ";";     // Time Doing After Mission Salvaging
                line += ((int)DateTime.UtcNow.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";";                             // Total Time doing Mission
                line += ((int)(Cache.Instance.MyWalletBalance - Cache.Instance.Wealth)) + ";";                                              // Isk (balance difference from start and finish of mission: is not accurate as the wallet ticks from bounty kills are every x minutes)
                line += Statistics.Instance.LootValue + ";";                                                                                // Loot
                line += (Cache.Instance.Agent.LoyaltyPoints - Statistics.Instance.LoyaltyPoints) + ";\r\n";                                 // LP

                // The mission is finished
                File.AppendAllText(Settings.Instance.MissionStats1LogFile, line);
                Logging.Log("Statistics", "writing mission log1 to  [ " + Settings.Instance.MissionStats1LogFile + " ]", Logging.White);

                //Logging.Log("Date;Mission;TimeMission;TimeSalvage;TotalTime;Isk;Loot;LP;");
                //Logging.Log(line);
            }
            if (Settings.Instance.MissionStats2Log)
            {
                if (!Directory.Exists(Settings.Instance.MissionStats2LogPath))
                {
                    Directory.CreateDirectory(Settings.Instance.MissionStats2LogPath);
                }

                // Write the header
                if (!File.Exists(Settings.Instance.MissionStats2LogFile))
                {
                    File.AppendAllText(Settings.Instance.MissionStats2LogFile, "Date;Mission;Time;Isk;Loot;LP;LostDrones;AmmoConsumption;AmmoValue\r\n");
                }

                // Build the line
                string line2 = string.Format("{0:MM/dd/yyyy HH:mm:ss}", DateTimeForLogs) + ";";                                      // Date
                line2 += Cache.Instance.MissionName + ";";                                                                           // Mission
                line2 += ((int)Statistics.Instance.FinishedMission.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";"; // TimeMission
                line2 += ((int)(Cache.Instance.MyWalletBalance - Cache.Instance.Wealth)) + ";";                                      // Isk
                line2 += Statistics.Instance.LootValue + ";";                                                                        // Loot
                line2 += (Cache.Instance.Agent.LoyaltyPoints - Statistics.Instance.LoyaltyPoints) + ";";                             // LP
                line2 += Statistics.Instance.LostDrones + ";";                                                                       // Lost Drones
                line2 += Statistics.Instance.AmmoConsumption + ";";                                                                  // Ammo Consumption
                line2 += Statistics.Instance.AmmoValue + ";\r\n";                                                                    // Ammo Value

                // The mission is finished
                Logging.Log("Statistics", "writing mission log2 to [ " + Settings.Instance.MissionStats2LogFile + " ]", Logging.White);
                File.AppendAllText(Settings.Instance.MissionStats2LogFile, line2);

                //Logging.Log("Date;Mission;Time;Isk;Loot;LP;LostDrones;AmmoConsumption;AmmoValue;");
                //Logging.Log(line2);
            }
            if (Settings.Instance.MissionStats3Log)
            {
                if (!Directory.Exists(Settings.Instance.MissionStats3LogPath))
                {
                    Directory.CreateDirectory(Settings.Instance.MissionStats3LogPath);
                }

                // Write the header
                if (!File.Exists(Settings.Instance.MissionStats3LogFile))
                {
                    File.AppendAllText(Settings.Instance.MissionStats3LogFile, "Date;Mission;Time;Isk;Loot;LP;DroneRecalls;LostDrones;AmmoConsumption;AmmoValue;Panics;LowestShield;LowestArmor;LowestCap;RepairCycles;AfterMissionsalvageTime;TotalMissionTime;MissionXMLAvailable;Faction;SolarSystem;DungeonID;OutOfDronesCount;\r\n");
                }

                // Build the line
                string line3 = DateTimeForLogs + ";";                                                                                        // Date
                line3 += Cache.Instance.MissionName + ";";                                                                                   // Mission
                line3 += ((int)Statistics.Instance.FinishedMission.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";";         // TimeMission
                line3 += ((long)(Cache.Instance.MyWalletBalance - Cache.Instance.Wealth)) + ";";                                             // Isk
                line3 += ((long)Statistics.Instance.LootValue) + ";";                                                                        // Loot
                line3 += ((long)Cache.Instance.Agent.LoyaltyPoints - Statistics.Instance.LoyaltyPoints) + ";";                               // LP
                line3 += Statistics.Instance.DroneRecalls + ";";                                                                             // Lost Drones
                line3 += "LostDrones:" + Statistics.Instance.LostDrones + ";";                                                               // Lost Drones
                line3 += Statistics.Instance.AmmoConsumption + ";";                                                                          // Ammo Consumption
                line3 += Statistics.Instance.AmmoValue + ";";                                                                                // Ammo Value
                line3 += "Panics:" + Cache.Instance.PanicAttemptsThisMission + ";";                                                          // Panics
                line3 += ((int)Cache.Instance.LowestShieldPercentageThisMission) + ";";                                                      // Lowest Shield %
                line3 += ((int)Cache.Instance.LowestArmorPercentageThisMission) + ";";                                                       // Lowest Armor %
                line3 += ((int)Cache.Instance.LowestCapacitorPercentageThisMission) + ";";                                                   // Lowest Capacitor %
                line3 += Cache.Instance.RepairCycleTimeThisMission + ";";                                                                    // repair Cycle Time
                line3 += ((int)Statistics.Instance.FinishedSalvaging.Subtract(Statistics.Instance.StartedSalvaging).TotalMinutes) + ";";     // After Mission Salvaging Time
                line3 += ((int)Statistics.Instance.FinishedSalvaging.Subtract(Statistics.Instance.StartedSalvaging).TotalMinutes) + ((int)Statistics.Instance.FinishedMission.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";"; // Total Time, Mission + After Mission Salvaging (if any)
                line3 += Cache.Instance.MissionXMLIsAvailable.ToString(CultureInfo.InvariantCulture) + ";";
                line3 += Cache.Instance.FactionName + ";";                                                                                   // FactionName that the mission is against
                line3 += Cache.Instance.MissionSolarSystem + ";";                                                                            // SolarSystem the mission was located in
                line3 += Cache.Instance.DungeonId + ";";                                                                                     // DungeonID - the unique identifier for this mission
                line3 += Statistics.Instance.OutOfDronesCount + ";";                                                                         // OutOfDronesCount - number of times we totally ran out of drones and had to go re-arm
                line3 += "\r\n";

                // The mission is finished
                Logging.Log("Statistics", "writing mission log3 to  [ " + Settings.Instance.MissionStats3LogFile + " ]", Logging.White);
                File.AppendAllText(Settings.Instance.MissionStats3LogFile, line3);

                //Logging.Log("Date;Mission;Time;Isk;Loot;LP;LostDrones;AmmoConsumption;AmmoValue;Panics;LowestShield;LowestArmor;LowestCap;RepairCycles;AfterMissionsalvageTime;TotalMissionTime;");
                //Logging.Log(line3);
            }
            if (Settings.Instance.MissionDungeonIdLog)
            {
                if (!Directory.Exists(Settings.Instance.MissionDungeonIdLogPath))
                {
                    Directory.CreateDirectory(Settings.Instance.MissionDungeonIdLogPath);
                }

                // Write the header
                if (!File.Exists(Settings.Instance.MissionDungeonIdLogFile))
                {
                    File.AppendAllText(Settings.Instance.MissionDungeonIdLogFile, "Mission;Faction;DungeonID;\r\n");
                }

                // Build the line
                string line4 = DateTimeForLogs + ";";              // Date
                line4 += Cache.Instance.MissionName + ";";      // Mission
                line4 += Cache.Instance.FactionName + ";";      // FactionName that the mission is against
                line4 += Cache.Instance.DungeonId + ";";        // DungeonID - the unique identifier for this mission (parsed from the mission HTML)
                line4 += "\r\n";

                // The mission is finished
                Logging.Log("Statistics", "writing mission dungeonID log to  [ " + Settings.Instance.MissionDungeonIdLogFile + " ]", Logging.White);
                File.AppendAllText(Settings.Instance.MissionDungeonIdLogFile, line4);

                //Logging.Log("Date;Mission;Time;Isk;Loot;LP;LostDrones;AmmoConsumption;AmmoValue;Panics;LowestShield;LowestArmor;LowestCap;RepairCycles;AfterMissionsalvageTime;TotalMissionTime;");
                //Logging.Log(line3);
            }

            // Disable next log line
            Statistics.Instance.MissionLoggingCompleted = true;
            Statistics.Instance.LootValue = 0;
            Statistics.Instance.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
            Statistics.Instance.StartedMission = DateTime.UtcNow;
            Statistics.Instance.FinishedMission = DateTime.UtcNow; //this may need to be reset to DateTime.MinValue, but that was causing other issues...
            Cache.Instance.MissionName = string.Empty;
            Statistics.Instance.DroneRecalls = 0;
            Statistics.Instance.LostDrones = 0;
            Statistics.Instance.AmmoConsumption = 0;
            Statistics.Instance.AmmoValue = 0;
            Statistics.Instance.DroneLoggingCompleted = false;
            Statistics.Instance.MissionCompletionErrors = 0;
            Statistics.Instance.OutOfDronesCount = 0;
            foreach (ModuleCache weapon in Cache.Instance.Weapons)
            {
                if (Time.Instance.ReloadTimePerModule != null && Time.Instance.ReloadTimePerModule.ContainsKey(weapon.ItemId))
                {
                    Time.Instance.ReloadTimePerModule[weapon.ItemId] = 0;    
                }
            }

            Cache.Instance.PanicAttemptsThisMission = 0;
            Cache.Instance.LowestShieldPercentageThisMission = 101;
            Cache.Instance.LowestArmorPercentageThisMission = 101;
            Cache.Instance.LowestCapacitorPercentageThisMission = 101;
            Cache.Instance.RepairCycleTimeThisMission = 0;
            Cache.Instance.TimeSpentReloading_seconds = 0;             // this will need to be added to whenever we reload or switch ammo
            Cache.Instance.TimeSpentInMission_seconds = 0;             // from landing on grid (loading mission actions) to going to base (changing to gotobase state)
            Cache.Instance.TimeSpentInMissionInRange = 0;              // time spent totally out of range, no targets
            Cache.Instance.TimeSpentInMissionOutOfRange = 0;           // time spent in range - with targets to kill (or no targets?!)
            Cache.Instance.MissionSolarSystem = null;
            Cache.Instance.DungeonId = "n/a";
            Cache.Instance.OrbitEntityNamed = null;
        }

        public void ProcessState()
        {
            switch (_States.CurrentStatisticsState)
            {
                case StatisticsState.Idle:
                    break;

                case StatisticsState.LogAllEntities:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.LogAllEntities", Logging.Debug);
                        Statistics.LogEntities(Cache.Instance.EntitiesOnGrid.ToList());
                    }
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;

                case StatisticsState.ListPotentialCombatTargets:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.LogAllEntities", Logging.Debug);
                        Statistics.LogEntities(Cache.Instance.PotentialCombatTargets.Where(i => i.IsOnGridWithMe).ToList());
                    }
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;

                case StatisticsState.ListHighValueTargets:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.LogAllEntities", Logging.Debug);
                        Statistics.LogEntities(Cache.Instance.PotentialCombatTargets.Where(i => i.IsHighValueTarget).ToList());
                    }
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;

                case StatisticsState.ListLowValueTargets:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.LogAllEntities", Logging.Debug);
                        Statistics.LogEntities(Cache.Instance.PotentialCombatTargets.Where(i => i.IsLowValueTarget).ToList());
                    }
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;

                case StatisticsState.SessionLog:
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;

                case StatisticsState.ModuleInfo:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Cache.Instance.InSpace || Cache.Instance.InStation)
                        {
                            _States.CurrentStatisticsState = StatisticsState.Idle;
                            Logging.Log("Statistics", "StatisticsState.ModuleInfo", Logging.Debug);
                            Statistics.ModuleInfo(Cache.Instance.Modules);
                        }
                    }
                    break;

                    //ListClassInstanceInfo

                case StatisticsState.ListClassInstanceInfo:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Cache.Instance.InSpace)
                        {
                            _States.CurrentStatisticsState = StatisticsState.Idle;
                            Logging.Log("Statistics", "StatisticsState.ListClassInstanceInfo", Logging.Debug);
                            Statistics.ListClassInstanceInfo();
                        }
                    }
                    break;

                case StatisticsState.ListIgnoredTargets:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Cache.Instance.InSpace)
                        {
                            _States.CurrentStatisticsState = StatisticsState.Idle;
                            Logging.Log("Statistics", "StatisticsState.ListIgnoredTargets", Logging.Debug);
                            Statistics.ListIgnoredTargets();
                        }
                    }
                    break;

                case StatisticsState.ListDronePriorityTargets:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Cache.Instance.InSpace)
                        {
                            _States.CurrentStatisticsState = StatisticsState.Idle;
                            Logging.Log("Statistics", "StatisticsState.ListDronePriorityTargets", Logging.Debug);
                            Statistics.ListDronePriorityTargets(Cache.Instance.DronePriorityEntities);
                        }
                    }
                    break;

                case StatisticsState.ListTargetedandTargeting:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Cache.Instance.InSpace)
                        {
                            _States.CurrentStatisticsState = StatisticsState.Idle;
                            Logging.Log("Statistics", "StatisticsState.ListTargetedandTargeting", Logging.Debug);
                            Statistics.ListTargetedandTargeting(Cache.Instance.TotalTargetsandTargeting);
                        }
                    }
                    break;

                case StatisticsState.PocketObjectStatistics:
                    if (!Cache.Instance.InWarp)
                    {
                        if (Cache.Instance.EntitiesOnGrid.Any())
                        {
                            _States.CurrentStatisticsState = StatisticsState.Idle;
                            Logging.Log("Statistics", "StatisticsState.PocketObjectStatistics", Logging.Debug);
                            Statistics.PocketObjectStatistics(Cache.Instance.EntitiesOnGrid.ToList(), true);
                        }
                    }
                    break;

                case StatisticsState.ListItemHangarItems:
                    if (Cache.Instance.InStation && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(20))
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.ListItemHangarItems", Logging.Debug);
                        List<ItemCache> ItemsToList;
                        if (Cache.Instance.ItemHangar != null && Cache.Instance.ItemHangar.Items.Any())
                        {
                            ItemsToList = Cache.Instance.ItemHangar.Items.Select(i => new ItemCache(i)).ToList();
                        }
                        else
                        {
                            ItemsToList = new List<ItemCache>();
                        }

                        Statistics.ListItems(ItemsToList);
                    }
                    break;

                case StatisticsState.ListLootHangarItems:
                    if (Cache.Instance.InStation && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(20))
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.ListLootHangarItems", Logging.Debug);
                        List<ItemCache> ItemsToList;
                        if (Cache.Instance.LootHangar != null && Cache.Instance.LootHangar.Items.Any())
                        {
                            ItemsToList = Cache.Instance.LootHangar.Items.Select(i => new ItemCache(i)).ToList();
                        }
                        else
                        {
                            ItemsToList = new List<ItemCache>();
                        }

                        Statistics.ListItems(ItemsToList);
                    }
                    break;

                case StatisticsState.ListLootContainerItems:
                    if (Cache.Instance.InStation && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(20))
                    {
                        _States.CurrentStatisticsState = StatisticsState.Idle;
                        Logging.Log("Statistics", "StatisticsState.ListLootContainerItems", Logging.Debug);
                        List<ItemCache> ItemsToList;
                        if (Cache.Instance.LootContainer != null && Cache.Instance.LootContainer.Items.Any())
                        {
                            ItemsToList = Cache.Instance.LootContainer.Items.Select(i => new ItemCache(i)).ToList();
                        }
                        else
                        {
                            ItemsToList = new List<ItemCache>();    
                        }
                        
                        Statistics.ListItems(ItemsToList);
                    }
                    break;


                case StatisticsState.Done:

                    //_lastStatisticsAction = DateTime.UtcNow;
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;

                default:

                    // Next state
                    _States.CurrentStatisticsState = StatisticsState.Idle;
                    break;
            }
        }
    }
}