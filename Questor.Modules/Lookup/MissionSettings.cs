// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using LavishScriptAPI;

namespace Questor.Modules.Lookup
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using DirectEve;
    using global::Questor.Modules.Actions;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Combat;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.States;
    

    public static class MissionSettings
    {
        static MissionSettings()
        {
            ChangeMissionShipFittings = false;
            DefaultFitting = new FactionFitting(); 
            FactionBlacklist = new List<string>();
            ListOfAgents = new List<AgentsList>();
            ListofFactionFittings = new List<FactionFitting>();
            ListOfMissionFittings = new List<MissionFitting>();
            MissionAmmoTypesToLoad = new List<Ammo>();
            MissionBlacklist = new List<string>();
            MissionGreylist = new List<string>();
            MissionItems = new List<string>();
            MissionUseDrones = null;
            UseMissionShip = false;
        }

        //
        // Fitting Settings - if enabled
        //
        public static List<FactionFitting> ListofFactionFittings { get; private set; }
        public static List<AgentsList> ListOfAgents { get; set; }
        public static List<MissionFitting> ListOfMissionFittings { get; private set; }
        public static FactionFitting DefaultFitting { get; set; }

        public static DirectAgentMission Mission;
        public static DirectAgentMission FirstAgentMission;
        public static IEnumerable<DirectAgentMission> myAgentMissionList { get; set; }
        public static bool MissionXMLIsAvailable { get; set; }
        public static string MissionXmlPath { get; set; }
        public static string MissionName { get; set; }
        public static float MinAgentBlackListStandings { get; set; }
        public static float MinAgentGreyListStandings { get; set; }
        public static string MissionsPath { get; set; }
        public static bool RequireMissionXML { get; set; }
        public static bool AllowNonStorylineCourierMissionsInLowSec { get; set; }
        public static bool WaitDecline { get; set; }
        public static int NumberOfTriesToDeleteBookmarks = 3;
        public static int MaterialsForWarOreID { get; set; }
        public static int MaterialsForWarOreQty { get; set; }
        public static int StopSessionAfterMissionNumber = int.MaxValue;
        public static int GreyListedMissionsDeclined = 0;
        public static string LastGreylistMissionDeclined = string.Empty;
        public static int BlackListedMissionsDeclined = 0;
        public static string LastBlacklistMissionDeclined = string.Empty;
        

        //
        // Pocket Specific Settings (we should make these ALL settable via the mission XML inside of pockets
        //

        public static int? PocketDroneTypeID { get; set; }
        public static bool? PocketKillSentries { get; set; }
        public static bool? PocketUseDrones { get; set; }
        public static double? PocketOrbitDistance = null;
        public static double? PocketOptimalRange = null;
        public static int? PocketActivateRepairModulesAtThisPerc { get; set; }
        
        //
        // Mission Specific Settings (we should make these ALL settable via the mission XML outside of pockets (just inside the mission tag)
        //
        public static int? MissionDroneTypeID { get; set; }
        public static bool? MissionDronesKillHighValueTargets = null;
        public static bool? MissionKillSentries { get; set; }
        public static bool? MissionUseDrones { get; set; }
        public static double? MissionOrbitDistance = null;
        public static double? MissionOptimalRange = null;
        public static int? MissionActivateRepairModulesAtThisPerc { get; set; }
        public static int MissionWeaponGroupId { get; set; }
        public static string BringMissionItem { get; private set; }
        public static int BringMissionItemQuantity { get; private set; }
        public static string BringOptionalMissionItem { get; private set; }
        public static int BringOptionalMissionItemQuantity { get; set; }
        public static double MissionWarpAtDistanceRange { get; set; } //in km

        //
        // Faction Specific Settings (we should make these ALL settable via some mechanic that I have not come up with yet
        //
        public static int? FactionDroneTypeID { get; set; }
        public static bool? FactionDronesKillHighValueTargets = null;
        public static double? FactionOrbitDistance = null;
        public static double? FactionOptimalRange = null;
        public static int? FactionActivateRepairModulesAtThisPerc { get; set; }

       
        //
        // Mission Blacklist / Greylist Settings
        //
        public static List<string> MissionBlacklist { get; private set; }
        public static List<string> MissionGreylist { get; private set; }
        public static List<string> FactionBlacklist { get; private set; }

        public static void LoadMissionBlackList(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                //if (Settings.Instance.CharacterMode.ToLower() == "Combat Missions".ToLower())
                //{
                //
                // Mission Blacklist
                //
                MissionBlacklist.Clear();
                XElement xmlElementBlackListSection = CharacterSettingsXml.Element("blacklist") ?? CommonSettingsXml.Element("blacklist");
                if (xmlElementBlackListSection != null)
                {
                    Logging.Log("Settings", "Loading Mission Blacklist", Logging.White);
                    int i = 1;
                    foreach (XElement BlacklistedMission in xmlElementBlackListSection.Elements("mission"))
                    {
                        MissionBlacklist.Add(Logging.FilterPath((string)BlacklistedMission));
                        if (Logging.DebugBlackList) Logging.Log("Settings.LoadBlackList", "[" + i + "] Blacklisted mission Name [" + Logging.FilterPath((string)BlacklistedMission) + "]", Logging.Teal);
                        i++;
                    }
                    Logging.Log("Settings", "        Mission Blacklist now has [" + MissionBlacklist.Count + "] entries", Logging.White);
                }
                //}

            }
            catch (Exception ex)
            {
                Logging.Log("Settings.LoadMissionBlackList", "Exception: [" + ex + "]", Logging.Debug);
            }
        }

        public static void LoadMissionGreyList(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                //if (Settings.Instance.CharacterMode.ToLower() == "Combat Missions".ToLower())
                //{
                //
                // Mission Greylist
                //
                MissionGreylist.Clear();
                XElement xmlElementGreyListSection = CharacterSettingsXml.Element("greylist") ?? CommonSettingsXml.Element("greylist");

                if (xmlElementGreyListSection != null)
                {
                    Logging.Log("Settings", "Loading Mission Greylist", Logging.White);
                    int i = 1;
                    foreach (XElement GreylistedMission in xmlElementGreyListSection.Elements("mission"))
                    {
                        MissionGreylist.Add(Logging.FilterPath((string)GreylistedMission));
                        if (Logging.DebugGreyList) Logging.Log("Settings.LoadGreyList", "[" + i + "] Greylisted mission Name [" + Logging.FilterPath((string)GreylistedMission) + "]", Logging.Teal);
                        i++;
                    }
                    Logging.Log("Settings", "        Mission Greylist now has [" + MissionGreylist.Count + "] entries", Logging.White);
                }
                //}
            }
            catch (Exception ex)
            {
                Logging.Log("Settings.LoadMissionGreyList", "Exception: [" + ex + "]", Logging.Debug);
            }
        }

        public static void LoadFactionBlacklist(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                //
                // Faction Blacklist
                //
                FactionBlacklist.Clear();
                XElement factionblacklist = CharacterSettingsXml.Element("factionblacklist") ?? CommonSettingsXml.Element("factionblacklist");
                if (factionblacklist != null)
                {
                    Logging.Log("Settings", "Loading Faction Blacklist", Logging.White);
                    foreach (XElement faction in factionblacklist.Elements("faction"))
                    {
                        Logging.Log("Settings", "        Missions from the faction [" + (string)faction + "] will be declined", Logging.White);
                        FactionBlacklist.Add((string)faction);
                    }

                    Logging.Log("Settings", "        Faction Blacklist now has [" + FactionBlacklist.Count + "] entries", Logging.White);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Settings.LoadMissionGreyList", "Exception: [" + ex + "]", Logging.Debug);
            }
        }

        //public XDocument InvTypes;
        public static XDocument UnloadLootTheseItemsAreLootItems;
        public static XDocument InvIgnore;
        
        /// <summary>
        ///   Returns the mission objectives from
        /// </summary>
        public static List<string> MissionItems { get; private set; }

        public static string FittingToLoad { get; set; } // stores name of the final fitting we want to use
        public static string MissionSpecificShip { get; set; } //stores name of mission specific ship
        public static string FactionSpecificShip { get; set; } //stores name of mission specific ship
        public static string DefaultFittingName { get; set; } //stores name of the default fitting
        public static string CurrentFit { get; set; }
        public static string FactionFittingForThisMissionsFaction { get; set; }
        public static string FactionName { get; set; }
        public static bool UseMissionShip { get; set; } // flags whether we're using a mission specific ship
        public static bool ChangeMissionShipFittings { get; set; } // used for situations in which missionShip's specified, but no faction or mission fittings are; prevents default
        public static List<Ammo> MissionAmmoTypesToLoad { get; set; }
        public static List<Ammo> FactionAmmoTypesToLoad { get; set; }
        
        public static int MissionsThisSession = 0;

        
        /// <summary>
        ///   Returns the first mission bookmark that starts with a certain string
        /// </summary>
        /// <returns></returns>
        public static DirectAgentMissionBookmark GetMissionBookmark(long agentId, string startsWith)
        {
            try
            {
                // Get the missions
                DirectAgentMission missionForBookmarkInfo = Cache.Instance.GetAgentMission(agentId, false);
                if (missionForBookmarkInfo == null)
                {
                    Logging.Log("Cache.DirectAgentMissionBookmark", "missionForBookmarkInfo [null] <---bad  parameters passed to us:  agentid [" + agentId + "] startswith [" + startsWith + "]", Logging.White);
                    return null;
                }

                // Did we accept this mission?
                if (missionForBookmarkInfo.State != (int)MissionState.Accepted)
                {
                    Logging.Log("GetMissionBookmark", "missionForBookmarkInfo.State: [" + missionForBookmarkInfo.State.ToString() + "]", Logging.Debug);
                    return null;
                }

                if (missionForBookmarkInfo.AgentId != agentId)
                {
                    Logging.Log("GetMissionBookmark", "missionForBookmarkInfo.AgentId: [" + missionForBookmarkInfo.AgentId.ToString() + "]", Logging.Debug);
                    Logging.Log("GetMissionBookmark", "agentId: [" + agentId + "]", Logging.Debug);
                    return null;
                }

                return missionForBookmarkInfo.Bookmarks.FirstOrDefault(b => b.Title.ToLower().StartsWith(startsWith.ToLower()));
            }
            catch (Exception exception)
            {
                Logging.Log("Cache.DirectAgentMissionBookmark", "Exception [" + exception + "]", Logging.Debug);
                return null;
            }
        }

        public static void ClearPocketSpecificSettings()
        {
            MissionSettings.PocketActivateRepairModulesAtThisPerc = null;
            MissionSettings.PocketKillSentries = null;
            MissionSettings.PocketOptimalRange = null;
            MissionSettings.PocketOrbitDistance = null;
            MissionSettings.PocketUseDrones = null;
        }

        public static void ClearMissionSpecificSettings()
        {
            MissionSettings.MissionSpecificShip = null;
            MissionSettings.FactionSpecificShip = null;
            MissionSettings.MissionItems.Clear();
            MissionSettings.BringMissionItem = string.Empty;
            MissionSettings.BringOptionalMissionItem = string.Empty;

            MissionSettings.MissionDroneTypeID = null;
            MissionSettings.MissionDronesKillHighValueTargets = null;
            //MissionSettings.MissionWeaponGroupId = null;
            //MissionSettings.BringOptionalMissionItemQuantity = null;
            //MissionSettings.MissionWarpAtDistanceRange = null;
            MissionSettings.MissionXMLIsAvailable = true;
            MissionSettings.MissionDroneTypeID = null;
            MissionSettings.MissionKillSentries = null;
            MissionSettings.MissionUseDrones = null;
            MissionSettings.MissionUseDrones = null;
            MissionSettings.MissionOrbitDistance = null;
            MissionSettings.MissionOptimalRange = null;
            MissionSettings.MissionAmmoTypesToLoad = new List<Ammo>();
        }

        public static void ClearFactionSpecificSettings()
        {
            MissionSettings.FactionActivateRepairModulesAtThisPerc = null;
            MissionSettings.FactionDroneTypeID = null;
            MissionSettings.FactionDronesKillHighValueTargets = null;
            MissionSettings.FactionOptimalRange = null;
            MissionSettings.FactionOrbitDistance = null;
            MissionSettings.FactionAmmoTypesToLoad = new List<Ammo>();
        }

        public static void LoadMissionXMLData()
        {
            Logging.Log("AgentInteraction", "Loading mission xml [" + MissionName + "] from [" + MissionSettings.MissionXmlPath + "]", Logging.Yellow);
            //
            // Clear Mission Specific Settings
            //
            ClearMissionSpecificSettings();

            //
            // this loads the settings global to the mission, NOT individual pockets
            //
            XDocument missionXml = null;
            try
            {
                missionXml = XDocument.Load(MissionXmlPath);

                //load mission specific ammo and WeaponGroupID if specified in the mission xml
                if (missionXml.Root != null)
                {
                    XElement ammoTypes = missionXml.Root.Element("ammoTypes");
                    if (ammoTypes != null)
                    {
                        Logging.Log("LoadMissionXMLData", "Clearing existing list of Ammo To load", Logging.White);
                        MissionAmmoTypesToLoad = new List<Ammo>();
                        foreach (XElement ammo in ammoTypes.Elements("ammoType"))
                        {
                            Logging.Log("LoadSpecificAmmo", "Adding [" + new Ammo(ammo).Name + "] to the list of ammo to load: from: ammoTypes", Logging.White);
                            MissionAmmoTypesToLoad.Add(new Ammo(ammo));
                        }

                        //Cache.Instance.DamageType
                    }

                    ammoTypes = missionXml.Root.Element("missionammo");
                    if (ammoTypes != null)
                    {
                        Logging.Log("LoadMissionXMLData", "Clearing existing list of Ammo To load", Logging.White);
                        MissionAmmoTypesToLoad = new List<Ammo>();
                        foreach (XElement ammo in ammoTypes.Elements("ammo"))
                        {
                            Logging.Log("LoadSpecificAmmo", "Adding [" + new Ammo(ammo).Name + "] to the list of ammo to load: from: missionammo", Logging.White);
                            MissionAmmoTypesToLoad.Add(new Ammo(ammo));
                        }

                        //Cache.Instance.DamageType
                    }

                    MissionWeaponGroupId = (int?)missionXml.Root.Element("weaponGroupId") ?? 0;
                    MissionUseDrones = (bool?) missionXml.Root.Element("useDrones") ?? null; 
                    MissionKillSentries = (bool?)missionXml.Root.Element("killSentries" ?? null);
                    MissionWarpAtDistanceRange = (int?)missionXml.Root.Element("missionWarpAtDistanceRange") ?? 0; //distance in km
                }

                MissionSettings.MissionDroneTypeID = (int?)missionXml.Root.Element("DroneTypeId") ?? null;
                IEnumerable<DamageType> damageTypesForThisMission = missionXml.XPathSelectElements("//damagetype").Select(e => (DamageType)Enum.Parse(typeof(DamageType), (string)e, true)).ToList();
                if (damageTypesForThisMission.Any())
                {
                    MissionDamageType = damageTypesForThisMission.FirstOrDefault();
                    Logging.Log("AgentInteraction", "Mission XML specified the Mission Damagetype for [" + MissionName + "] is [" + MissionDamageType.ToString() + "]", Logging.White);
                    LoadSpecificAmmo(damageTypesForThisMission.Distinct(), false, true, true);
                    loadedAmmo = true;
                }
            }
            catch (Exception ex)
            {
                Logging.Log("AgentInteraction", "Error in mission (not pocket) specific XML tags [" + MissionName + "], " + ex.Message, Logging.Orange);
            }
            finally
            {
                missionXml = null;
                System.GC.Collect();
            }

        }

        public static bool loadedAmmo = false;

        public static void LoadSpecificAmmo(IEnumerable<DamageType> damageTypes, bool FactionAmmo, bool MissionAmmo, bool ClearExisting)
        {
            try
            {
                Logging.Log("LoadSpecificAmmo", "Clearing existing list of Ammo To load", Logging.White);
                Arm._ammoTypesToLoad = new List<Ammo>();
                if (Combat.Ammo.Any(a => damageTypes.Contains(a.DamageType)))
                {
                    Ammo _ammo = Combat.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()).FirstOrDefault();
                    if (_ammo != null)
                    {
                        if (FactionAmmo)
                        {
                            Logging.Log("LoadSpecificAmmo", "Adding [" + _ammo.Name + "] to the list of FactionAmmo to load", Logging.White);
                            if (ClearExisting) MissionSettings.FactionAmmoTypesToLoad = new List<Ammo>();
                            MissionSettings.FactionAmmoTypesToLoad.AddRange(Combat.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
                            return;
                        }

                        if (MissionAmmo)
                        {
                            Logging.Log("LoadSpecificAmmo", "Adding [" + _ammo.Name + "] to the list of MissionAmmo to load", Logging.White);
                            if (ClearExisting) MissionSettings.MissionAmmoTypesToLoad = new List<Ammo>();
                            MissionSettings.MissionAmmoTypesToLoad.AddRange(Combat.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
                            return;
                        }

                        //if (ClearExisting) Arm.AmmoToLoad.Clear();
                        //Logging.Log("LoadSpecificAmmo", "Adding [" + _ammo.Name + "] to the list of ammo to load", Logging.White);
                        //Arm._ammoToLoad.AddRange(Combat.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("LoadSpecificAmmo", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        /*
        public static void GetDungeonId(string html)
        {
            HtmlAgilityPack.HtmlDocument missionHtml = new HtmlAgilityPack.HtmlDocument();
            missionHtml.LoadHtml(html);
            try
            {
                foreach (HtmlAgilityPack.HtmlNode nd in missionHtml.DocumentNode.SelectNodes("//a[@href]"))
                {
                    if (nd.Attributes["href"].Value.Contains("dungeonID="))
                    {
                        Cache.Instance.DungeonId = nd.Attributes["href"].Value;
                        Logging.Log("GetDungeonId", "DungeonID is: " + Cache.Instance.DungeonId, Logging.White);
                    }
                    else
                    {
                        Cache.Instance.DungeonId = "n/a";
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("GetDungeonId", "if (nd.Attributes[href].Value.Contains(dungeonID=)) - Exception: [" + exception + "]", Logging.White);
            }
        }
        */

        public static void GetFactionName(string html)
        {
            Statistics.SaveMissionHTMLDetails(html, MissionName);
            // We are going to check damage types
            Regex logoRegex = new Regex("img src=\"factionlogo:(?<factionlogo>\\d+)");

            Match logoMatch = logoRegex.Match(html);
            if (logoMatch.Success)
            {
                string logo = logoMatch.Groups["factionlogo"].Value;

                // Load faction xml
                string factionsXML = Path.Combine(Settings.Instance.Path, "Factions.xml");
                try
                {
                    XDocument xml = XDocument.Load(factionsXML);
                    if (xml.Root != null)
                    {
                        XElement faction = xml.Root.Elements("faction").FirstOrDefault(f => (string)f.Attribute("logo") == logo);
                        if (faction != null)
                        {
                            FactionName = (string)faction.Attribute("name");
                            return;
                        }
                    }
                    else
                    {
                        Logging.Log("CombatMissionSettings", "ERROR! unable to read [" + factionsXML + "]  no root element named <faction> ERROR!", Logging.Red);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("CombatMissionSettings", "ERROR! unable to find [" + factionsXML + "] ERROR! [" + ex.Message + "]", Logging.Red);
                }
            }

            bool roguedrones = false;
            bool mercenaries = false;
            bool eom = false;
            bool seven = false;
            if (!string.IsNullOrEmpty(html))
            {
                roguedrones |= html.Contains("Destroy the Rogue Drones");
                roguedrones |= html.Contains("Rogue Drone Harassment Objectives");
                roguedrones |= html.Contains("Air Show! Objectives");
                roguedrones |= html.Contains("Alluring Emanations Objectives");
                roguedrones |= html.Contains("Anomaly Objectives");
                roguedrones |= html.Contains("Attack of the Drones Objectives");
                roguedrones |= html.Contains("Drone Detritus Objectives");
                roguedrones |= html.Contains("Drone Infestation Objectives");
                roguedrones |= html.Contains("Evolution Objectives");
                roguedrones |= html.Contains("Infected Ruins Objectives");
                roguedrones |= html.Contains("Infiltrated Outposts Objectives");
                roguedrones |= html.Contains("Mannar Mining Colony");
                roguedrones |= html.Contains("Missing Convoy Objectives");
                roguedrones |= html.Contains("Onslaught Objectives");
                roguedrones |= html.Contains("Patient Zero Objectives");
                roguedrones |= html.Contains("Persistent Pests Objectives");
                roguedrones |= html.Contains("Portal to War Objectives");
                roguedrones |= html.Contains("Rogue Eradication Objectives");
                roguedrones |= html.Contains("Rogue Hunt Objectives");
                roguedrones |= html.Contains("Rogue Spy Objectives");
                roguedrones |= html.Contains("Roving Rogue Drones Objectives");
                roguedrones |= html.Contains("Soothe The Salvage Beast");
                roguedrones |= html.Contains("Wildcat Strike Objectives");
                eom |= html.Contains("Gone Berserk Objectives");
                seven |= html.Contains("The Damsel In Distress Objectives");
            }

            if (roguedrones)
            {
                MissionSettings.FactionName = "rogue drones";
                return;
            }
            if (eom)
            {
                MissionSettings.FactionName = "eom";
                return;
            }
            if (mercenaries)
            {
                MissionSettings.FactionName = "mercenaries";
                return;
            }
            if (seven)
            {
                MissionSettings.FactionName = "the seven";
                return;
            }

            Logging.Log("AgentInteraction", "Unable to find the faction for [" + MissionName + "] when searching through the html (listed below)", Logging.Orange);

            Logging.Log("AgentInteraction", html, Logging.White);
            return;
        }

        /// <summary>
        ///   Best damage type for the mission
        /// </summary>
        public static DamageType MissionDamageType { get; set; }

        public static DamageType GetFactionDamageType(string html)
        {
            DamageType DamageTypeToUse;
            // We are going to check damage types
            Regex logoRegex = new Regex("img src=\"factionlogo:(?<factionlogo>\\d+)");

            Match logoMatch = logoRegex.Match(html);
            if (logoMatch.Success)
            {
                string logo = logoMatch.Groups["factionlogo"].Value;
                
                // Load faction xml
                XDocument xml = XDocument.Load(Path.Combine(Settings.Instance.Path, "Factions.xml"));
                if (xml.Root != null)
                {
                    XElement faction = xml.Root.Elements("faction").FirstOrDefault(f => (string)f.Attribute("logo") == logo);
                    if (faction != null)
                    {
                        FactionName = (string)faction.Attribute("name");
                        Logging.Log("GetMissionDamageType", "[" + MissionName + "] Faction [" + FactionName + "]", Logging.Yellow);
                        if (faction.Attribute("damagetype") != null)
                        {
                            DamageTypeToUse = ((DamageType) Enum.Parse(typeof (DamageType), (string) faction.Attribute("damagetype")));
                            Logging.Log("GetMissionDamageType", "Faction DamageType defined as [" + DamageTypeToUse + "]", Logging.Yellow);
                            return DamageTypeToUse;
                        }

                        DamageTypeToUse = DamageType.EM;
                        Logging.Log("GetMissionDamageType", "DamageType not found for Faction [" + FactionName + "], Defaulting to DamageType  [" + DamageTypeToUse + "]", Logging.Yellow);
                        return DamageTypeToUse;
                    }

                    DamageTypeToUse = DamageType.EM;
                    Logging.Log("GetMissionDamageType", "Faction not found in factions.xml, Defaulting to DamageType  [" + DamageTypeToUse + "]", Logging.Yellow);
                    return DamageTypeToUse;
                }

                DamageTypeToUse = DamageType.EM;
                Logging.Log("GetMissionDamageType", "Factions.xml is missing, Defaulting to DamageType  [" + DamageTypeToUse + "]", Logging.Yellow);
                return DamageTypeToUse;
            }

            DamageTypeToUse = DamageType.EM;
            Logging.Log("GetMissionDamageType", "Faction logo not matched, Defaulting to DamageType  [" + DamageTypeToUse + "]", Logging.Yellow);
            return DamageTypeToUse;
        }

        public static void UpdateMissionName(long AgentID = 0)
        {
            if (AgentID != 0)
            {
                MissionSettings.Mission = Cache.Instance.GetAgentMission(AgentID, true);
                if (MissionSettings.Mission != null && Cache.Instance.Agent != null)
                {
                    // Update loyalty points again (the first time might return -1)
                    Statistics.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
                    MissionSettings.MissionName = MissionSettings.Mission.Name;
                    if (Logging.UseInnerspace)
                    {
                        LavishScript.ExecuteCommand("WindowText EVE - " + Settings.Instance.CharacterName + " - " + MissionSettings.MissionName);
                    }
                }
            }
            else
            {
                if (Logging.UseInnerspace)
                {
                    LavishScript.ExecuteCommand("WindowText EVE - " + Settings.Instance.CharacterName);
                }
            }
        }

        public static void SetmissionXmlPath(string missionName)
        {
            try
            {
                if (!string.IsNullOrEmpty(FactionName))
                {
                    MissionXmlPath = System.IO.Path.Combine(MissionsPath, Logging.FilterPath(missionName) + "-" + FactionName + ".xml");
                    if (!File.Exists(MissionXmlPath))
                    {
                        //
                        // This will always fail for courier missions, can we detect those and suppress these log messages?
                        //
                        Logging.Log("Cache.SetmissionXmlPath", "[" + MissionXmlPath + "] not found.", Logging.White);
                        MissionXmlPath = System.IO.Path.Combine(MissionsPath, Logging.FilterPath(missionName) + ".xml");
                        if (!File.Exists(MissionXmlPath))
                        {
                            Logging.Log("Cache.SetmissionXmlPath", "[" + MissionXmlPath + "] not found", Logging.White);
                        }

                        if (File.Exists(MissionXmlPath))
                        {
                            Logging.Log("Cache.SetmissionXmlPath", "[" + MissionXmlPath + "] found!", Logging.Green);
                        }
                    }
                }
                else
                {
                    MissionXmlPath = System.IO.Path.Combine(MissionsPath, Logging.FilterPath(missionName) + ".xml");
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Cache.SetmissionXmlPath", "Exception [" + exception + "]", Logging.Debug);
            }
        }

        /// <summary>
        ///   Refresh the mission items
        /// </summary>
        public static void RefreshMissionItems(long agentId)
        {
            ClearMissionSpecificSettings();
            
            if (_States.CurrentQuestorState != QuestorState.CombatMissionsBehavior)
            {
                Settings.Instance.UseFittingManager = false;

                //Logging.Log("Cache.RefreshMissionItems", "We are not running missions so we have no mission items to refresh", Logging.Teal);
                return;
            }

            DirectAgentMission missionDetailsForMissionItems = Cache.Instance.GetAgentMission(agentId, false);
            if (missionDetailsForMissionItems == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(FactionName))
            {
                FactionName = "Default";
            }

            if (Settings.Instance.UseFittingManager)
            {
                //Set fitting to default
                DefaultFittingName = DefaultFitting.FittingName;
                FittingToLoad = DefaultFittingName;
                MissionSpecificShip = null;
                FactionSpecificShip = null;
                ChangeMissionShipFittings = false;

                //
                // default to using the faction fitting if defined
                //
                if (!string.IsNullOrEmpty(FactionFittingForThisMissionsFaction))
                {
                    FittingToLoad = FactionFittingForThisMissionsFaction;
                    MissionSettings.MissionDroneTypeID = Drones.FactionDroneTypeID;
                }

                if (ListOfMissionFittings.Any(m => m.Mission.ToLower() == missionDetailsForMissionItems.Name.ToLower())) //priority goes to mission-specific fittings
                {
                    MissionFitting FittingNameTouseForThisMission;

                    // if we have got multiple copies of the same mission, find the one with the matching faction
                    if (ListOfMissionFittings.Any(m => m.Faction.ToLower() == FactionName.ToLower() && (m.Mission.ToLower() == missionDetailsForMissionItems.Name.ToLower())))
                    {
                        FittingNameTouseForThisMission = ListOfMissionFittings.FirstOrDefault(m => m.Faction.ToLower() == FactionName.ToLower() && (m.Mission.ToLower() == missionDetailsForMissionItems.Name.ToLower()));
                    }
                    else //otherwise just use the first copy of that mission
                    {
                        FittingNameTouseForThisMission = ListOfMissionFittings.FirstOrDefault(m => m.Mission.ToLower() == missionDetailsForMissionItems.Name.ToLower());
                    }

                    if (FittingNameTouseForThisMission != null)
                    {
                        //
                        //
                        //
                        // this whole thing should be reworked to use faction fittings, drones, etc and them apply mission specific stuff if avail to overwrite it.
                        //
                        //
                        //
                        MissionSpecificShip = FittingNameTouseForThisMission.Ship;
                        //
                        // if we have the drone type specified in the mission fitting entry use it, otherwise do not overwrite the default or the drone type specified by the faction
                        //
                        if (FittingNameTouseForThisMission.DroneTypeID != null)
                        {
                            MissionSettings.MissionDroneTypeID = (int)FittingNameTouseForThisMission.DroneTypeID;
                        }

                        if (!(FittingNameTouseForThisMission.Fitting == "" && MissionSpecificShip != "")) // if we have both specified a mission specific ship and a fitting, then apply that fitting to the ship
                        {
                            ChangeMissionShipFittings = true;
                            FittingToLoad = FittingNameTouseForThisMission.Fitting;
                        }
                        else if (!string.IsNullOrEmpty(FactionFittingForThisMissionsFaction))
                        {
                            FittingToLoad = FactionFittingForThisMissionsFaction;
                        }

                        Logging.Log("RefreshMissionItems", "Mission: " + MissionSpecificShip + " - Faction: " + FactionName + " - Fitting: " + FittingToLoad + "]", Logging.White);
                        Logging.Log("RefreshMissionItems", "Ship: " + MissionSpecificShip + " - ChangeMissionShipFittings: " + ChangeMissionShipFittings + "Using DroneTypeID [" + Drones.DroneTypeID + "]", Logging.White);
                    }
                }
                else if (!string.IsNullOrEmpty(FactionFittingForThisMissionsFaction)) // if no mission fittings defined, try to match by faction
                {
                    FittingToLoad = FactionFittingForThisMissionsFaction;
                }

                if (FittingToLoad == "") // otherwise use the default
                {
                    FittingToLoad = DefaultFittingName;
                }
            }

            string missionName = Logging.FilterPath(missionDetailsForMissionItems.Name);
            MissionXmlPath = System.IO.Path.Combine(MissionsPath, missionName + ".xml");
            if (!File.Exists(MissionXmlPath))
            {
                return;
            }

            try
            {
                XDocument xdoc = XDocument.Load(MissionXmlPath);
                IEnumerable<string> items = ((IEnumerable)xdoc.XPathEvaluate("//action[(translate(@name, 'LOT', 'lot')='loot') or (translate(@name, 'LOTIEM', 'lotiem')='lootitem')]/parameter[translate(@name, 'TIEM', 'tiem')='item']/@value")).Cast<XAttribute>().Select(a => ((string)a ?? string.Empty).ToLower());
                MissionItems.AddRange(items);

                if (xdoc.Root != null)
                {
                    BringMissionItem = (string)xdoc.Root.Element("bring") ?? string.Empty;
                    BringMissionItem = BringMissionItem.ToLower();
                    if (Logging.DebugArm) Logging.Log("RefreshMissionItems", "bring XML [" + xdoc.Root.Element("bring") + "] BringMissionItem [" + BringMissionItem + "]", Logging.Debug);
                    BringMissionItemQuantity = (int?)xdoc.Root.Element("bringquantity") ?? 1;
                    if (Logging.DebugArm) Logging.Log("RefreshMissionItems", "bringquantity XML [" + xdoc.Root.Element("bringquantity") + "] BringMissionItemQuantity [" + BringMissionItemQuantity + "]", Logging.Debug);

                    BringOptionalMissionItem = (string)xdoc.Root.Element("trytobring") ?? string.Empty;
                    BringOptionalMissionItem = BringOptionalMissionItem.ToLower();
                    if (Logging.DebugArm) Logging.Log("RefreshMissionItems", "trytobring XML [" + xdoc.Root.Element("trytobring") + "] BringOptionalMissionItem [" + BringOptionalMissionItem + "]", Logging.Debug);
                    BringOptionalMissionItemQuantity = (int?)xdoc.Root.Element("trytobringquantity") ?? 1;
                    if (Logging.DebugArm) Logging.Log("RefreshMissionItems", "trytobringquantity XML [" + xdoc.Root.Element("trytobringquantity") + "] BringOptionalMissionItemQuantity [" + BringOptionalMissionItemQuantity + "]", Logging.Debug);

                }

                //load fitting setting from the mission file
                //Fitting = (string)xdoc.Root.Element("fitting") ?? "default";
            }
            catch (Exception ex)
            {
                Logging.Log("RefreshMissionItems", "Error loading mission XML file [" + ex.Message + "]", Logging.Orange);
            }
        }

    }
}