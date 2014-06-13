﻿// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public static class AgentInteraction
    {
        public const string RequestMission = "Request Mission";
        public const string ViewMission = "View Mission";
        public const string CompleteMission = "Complete Mission";
        public const string LocateCharacter = "Locate Character";
        public const string Accept = "Accept";
        public const string Decline = "Decline";
        public const string Close = "Close";
        public const string Delay = "Delay";
        public const string Quit = "Quit";

        public static string MissionName;

        private static bool _agentStandingsCheckFlag;  //false;
        private static bool _waitingOnAgentResponse;
        private static bool _waitingOnMission;
        private static bool loadedAmmo = false;

        private static DateTime _agentWindowTimeStamp = DateTime.MinValue;
        private static DateTime _agentStandingsCheckTimeOut = DateTime.MaxValue;
        private static DateTime _nextAgentAction;
        private static DateTime _waitingOnAgentResponseTimer = DateTime.UtcNow;
        private static DateTime _waitingOnMissionTimer = DateTime.UtcNow;

        private static int LoyaltyPointCounter;

        static AgentInteraction()
        {
            AmmoToLoad = new List<Ammo>();
        }

        public static long AgentId { get; set; }

        public static DirectAgent Agent
        {
            get { return Cache.Instance.DirectEve.GetAgentById(AgentId); }
        }

        public static bool ForceAccept { get; set; }

        public static AgentInteractionPurpose Purpose { get; set; }

        public static List<Ammo> AmmoToLoad { get; private set; }

        private static void LoadSpecificAmmo(IEnumerable<DamageType> damageTypes)
        {
            AmmoToLoad.Clear();
            AmmoToLoad.AddRange(Settings.Instance.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
        }

        private static void MyStandingsAreTooLowSwitchAgentsOrPause()
        {
            Cache.Instance.AgentEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(AgentId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentCorpEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.CorpId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentFactionEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.FactionId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);

            Cache.Instance.StandingUsedToAccessAgent = Math.Max(Cache.Instance.AgentEffectiveStandingtoMe, Math.Max(Cache.Instance.AgentCorpEffectiveStandingtoMe, Cache.Instance.AgentFactionEffectiveStandingtoMe));
            AgentsList currentAgent = Settings.Instance.ListOfAgents.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

            //
            //Change Agents
            //
            if (currentAgent != null) currentAgent.DeclineTimer = DateTime.UtcNow.AddHours(999);
            CloseConversation();
            Cache.Instance.CurrentAgent = Cache.Instance.SwitchAgent();
            Logging.Log("AgentInteraction", "new agent is " + Cache.Instance.CurrentAgent, Logging.Yellow);
            _States.CurrentAgentInteractionState = AgentInteractionState.ChangeAgent;
        }

        private static void StartConversation(string module)
        {
            Cache.Instance.AgentEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(AgentId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentCorpEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.CorpId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentFactionEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.FactionId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);

            Cache.Instance.StandingUsedToAccessAgent = Math.Max(Cache.Instance.AgentEffectiveStandingtoMe,Math.Max(Cache.Instance.AgentCorpEffectiveStandingtoMe,Cache.Instance.AgentFactionEffectiveStandingtoMe));
            AgentsList currentAgent = Settings.Instance.ListOfAgents.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

            Cache.Instance.AgentEffectiveStandingtoMeText = Cache.Instance.StandingUsedToAccessAgent.ToString("0.00");

            //
            // Standings Check: if this is a totally new agent this check will timeout after 20 seconds
            //
            if (DateTime.UtcNow < _agentStandingsCheckTimeOut)
            {
                if (((float)Cache.Instance.StandingUsedToAccessAgent == (float)0.00) && (AgentId == Cache.Instance.AgentId))
                {
                    if (!_agentStandingsCheckFlag)
                    {
                        _agentStandingsCheckTimeOut = DateTime.UtcNow.AddSeconds(15);
                        _agentStandingsCheckFlag = true;
                    }
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(5);
                    Logging.Log("AgentInteraction.StandingsCheck", " Agent [" + Cache.Instance.DirectEve.GetAgentById(AgentId).Name + "] Standings show as [" + Cache.Instance.StandingUsedToAccessAgent + " and must not yet be available. retrying for [" + Math.Round((double)_agentStandingsCheckTimeOut.Subtract(DateTime.UtcNow).TotalSeconds, 0) + " sec]", Logging.Yellow);
                    return;
                }
            }

            switch (Agent.Level)
            {
                //
                // what do tutorial mission agents show as?
                //
                case 1: //lvl1 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < Settings.Instance.StandingsNeededToAccessLevel1Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + Settings.Instance.StandingsNeededToAccessLevel1Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;
             
                case 2: //lvl2 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < Settings.Instance.StandingsNeededToAccessLevel2Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + Settings.Instance.StandingsNeededToAccessLevel2Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;

                case 3: //lvl3 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < Settings.Instance.StandingsNeededToAccessLevel3Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + Settings.Instance.StandingsNeededToAccessLevel3Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;

                case 4: //lvl4 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < Settings.Instance.StandingsNeededToAccessLevel4Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + Settings.Instance.StandingsNeededToAccessLevel4Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;

                case 5: //lvl5 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < Settings.Instance.StandingsNeededToAccessLevel5Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + Settings.Instance.StandingsNeededToAccessLevel5Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;
            }

            if (!Cache.Instance.OpenAgentWindow(module)) return;

            if (Purpose == AgentInteractionPurpose.AmmoCheck)
            {
                Logging.Log("AgentInteraction", "Checking ammo type", Logging.Yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission;
            }
            else
            {
                Logging.Log("AgentInteraction", "Replying to agent", Logging.Yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.ReplyToAgent;
                _nextAgentAction = DateTime.UtcNow.AddSeconds(3);
            }
            return;
        }

        private static void ReplyToAgent(string module)
        {
            if (!Cache.Instance.OpenAgentWindow(module)) return;

            if (Agent.Window.AgentResponses == null || !Agent.Window.AgentResponses.Any())
            {
                if (_waitingOnAgentResponse == false)
                {
                    if (Settings.Instance.DebugAgentInteractionReplyToAgent) Logging.Log("AgentInteraction.ReplyToAgent", "Debug: if (_waitingOnAgentResponse == false)", Logging.Yellow);
                    _waitingOnAgentResponseTimer = DateTime.UtcNow;
                    _waitingOnAgentResponse = true;
                }

                if (DateTime.UtcNow.Subtract(_waitingOnAgentResponseTimer).TotalSeconds > 15)
                {
                    Logging.Log("AgentInteraction.ReplyToAgent", "Debug: agentWindowAgentresponses == null : trying to close the agent window", Logging.Yellow);
                    Agent.Window.Close();
                }

                if (Settings.Instance.DebugAgentInteractionReplyToAgent) Logging.Log("AgentInteraction.ReplyToAgent", "Debug: if (Agent.Window.AgentResponses == null || !Agent.Window.AgentResponses.Any())", Logging.Yellow);
                return;
            }

            if (Agent.Window.AgentResponses.Any())
            {
                if (Settings.Instance.DebugAgentInteractionReplyToAgent) Logging.Log("AgentInteraction.ReplyToAgent", "Debug: we have Agent.Window.AgentResponces", Logging.Yellow);
            }

            _waitingOnAgentResponse = false;

            DirectAgentResponse request = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(RequestMission));
            DirectAgentResponse complete = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(CompleteMission));
            DirectAgentResponse view = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(ViewMission));
            DirectAgentResponse accept = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(Accept));
            DirectAgentResponse decline = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(Decline));
            DirectAgentResponse delay = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(Delay));
            DirectAgentResponse quit = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(Quit));
            DirectAgentResponse close = Agent.Window.AgentResponses.FirstOrDefault(r => r.Text.Contains(Close));

            //
            // Read the possibly responses and make sure we are 'doing the right thing' - set AgentInteractionPurpose to fit the state of the agent window
            //
            if (Purpose != AgentInteractionPurpose.AmmoCheck) //do not change the AgentInteractionPurpose if we are checking which ammo type to use.
            {
                if (Settings.Instance.DebugAgentInteractionReplyToAgent) Logging.Log(module, "if (Purpose != AgentInteractionPurpose.AmmoCheck) //do not change the AgentInteractionPurpose if we are checking which ammo type to use.", Logging.Yellow);
                if (accept != null && decline != null && delay != null)
                {
                    if (Purpose != AgentInteractionPurpose.StartMission)
                    {
                        Logging.Log("AgentInteraction", "ReplyToAgent: Found accept button, Changing Purpose to StartMission", Logging.White);
                        _agentWindowTimeStamp = DateTime.UtcNow;
                        Purpose = AgentInteractionPurpose.StartMission;
                    }
                }

                if (complete != null && quit != null && close != null && Statistics.Instance.MissionCompletionErrors == 0)
                {
                    //
                    // this should run for ANY courier and likely needs to be changed when we implement generic courier support
                    //
                    if (Purpose != AgentInteractionPurpose.CompleteMission)
                    {
                        Logging.Log("AgentInteraction", "ReplyToAgent: Found complete button, Changing Purpose to CompleteMission", Logging.White);

                        //we have a mission in progress here, attempt to complete it
                        if (DateTime.UtcNow > _agentWindowTimeStamp.AddSeconds(30))
                        {
                            Purpose = AgentInteractionPurpose.CompleteMission;
                        }
                    }
                }

                if (request != null && close != null)
                {
                    if (Purpose != AgentInteractionPurpose.StartMission)
                    {
                        Logging.Log("AgentInteraction", "ReplyToAgent: Found request button, Changing Purpose to StartMission", Logging.White);

                        //we do not have a mission yet, request one?
                        if (DateTime.UtcNow > _agentWindowTimeStamp.AddSeconds(30))
                        {
                            Purpose = AgentInteractionPurpose.StartMission;
                        }
                    }
                }
            }

            if (complete != null)
            {
                if (Purpose == AgentInteractionPurpose.CompleteMission)
                {
                    // Complete the mission, close conversation
                    Logging.Log("AgentInteraction", "Saying [Complete Mission]", Logging.Yellow);
                    complete.Say();
                    Cache.Instance.FactionName = string.Empty;

                    Logging.Log("AgentInteraction", "Closing conversation", Logging.Yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                }
                else
                {
                    Logging.Log("AgentInteraction", "Waiting for mission", Logging.Yellow);

                    // Apparently someone clicked "accept" already
                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission;
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
                }
            }
            else if (request != null)
            {
                if (Purpose == AgentInteractionPurpose.StartMission)
                {
                    // Request a mission and wait for it
                    Logging.Log("AgentInteraction", "Saying [Request Mission]", Logging.Yellow);
                    request.Say();

                    Logging.Log("AgentInteraction", "Waiting for mission", Logging.Yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission;
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                }
                else
                {
                    Logging.Log("AgentInteraction", "Unexpected dialog options", Logging.Red);
                    _States.CurrentAgentInteractionState = AgentInteractionState.UnexpectedDialogOptions;
                }
            }
            else if (view != null)
            {
                // View current mission
                Logging.Log("AgentInteraction", "Saying [View Mission]", Logging.Yellow);

                view.Say();
                _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));

                // No state change
            }
            else if (accept != null || decline != null)
            {
                if (Purpose == AgentInteractionPurpose.StartMission)
                {
                    Logging.Log("AgentInteraction", "Waiting for mission", Logging.Yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission; // Do not say anything, wait for the mission
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 15));
                }
                else
                {
                    Logging.Log("AgentInteraction", "Unexpected dialog options", Logging.Red);

                    _States.CurrentAgentInteractionState = AgentInteractionState.UnexpectedDialogOptions;
                }
            }
        }

        public static void LoadMissionXMLData()
        {
            Logging.Log("AgentInteraction", "Loading mission xml [" + MissionName + "] from [" + Cache.Instance.MissionXmlPath + "]", Logging.Yellow);
            Cache.Instance.MissionXMLIsAvailable = true;

            //
            // this loads the settings global to the mission, NOT individual pockets
            //
            XDocument missionXml = null;
            try
            {
                missionXml = XDocument.Load(Cache.Instance.MissionXmlPath);

                //load mission specific ammo and WeaponGroupID if specified in the mission xml
                if (missionXml.Root != null)
                {
                    XElement ammoTypes = missionXml.Root.Element("ammoTypes");
                    if (ammoTypes != null)
                    {
                        Cache.Instance.MissionAmmo.Clear();
                        foreach (XElement ammo in ammoTypes.Elements("ammoType"))
                        {
                            Cache.Instance.MissionAmmo.Add(new Ammo(ammo));
                        }

                        //Cache.Instance.DamageType
                    }

                    ammoTypes = missionXml.Root.Element("missionammo");
                    if (ammoTypes != null)
                    {
                        Cache.Instance.MissionAmmo.Clear();
                        foreach (XElement ammo in ammoTypes.Elements("ammo"))
                        {
                            Cache.Instance.MissionAmmo.Add(new Ammo(ammo));
                        }

                        //Cache.Instance.DamageType
                    }
                    Cache.Instance.MissionWeaponGroupId = (int?)missionXml.Root.Element("weaponGroupId") ?? 0;
                    Cache.Instance.MissionUseDrones = (bool?)missionXml.Root.Element("useDrones"); //do not set default here, use character level settings if avail
                    Cache.Instance.MissionKillSentries = (bool?)missionXml.Root.Element("killSentries"); //do not set default here, use character level settings if avail
                    Cache.Instance.MissionWarpAtDistanceRange = (int?)missionXml.Root.Element("missionWarpAtDistanceRange") ?? 0; //distance in km
                }

                //Cache.Instance.MissionDroneTypeID = (int?)missionXml.Root.Element("DroneTypeId") ?? Settings.Instance.DroneTypeId;
                IEnumerable<DamageType> damageTypesForThisMission = missionXml.XPathSelectElements("//damagetype").Select(e => (DamageType)Enum.Parse(typeof(DamageType), (string)e, true)).ToList();
                if (damageTypesForThisMission.Any())
                {
                    Cache.Instance.MissionDamageType = damageTypesForThisMission.FirstOrDefault();
                    LoadSpecificAmmo(damageTypesForThisMission.Distinct());
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

        private static void GetDungeonId(string html)
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

        private static void GetFactionName(string html)
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
                            Cache.Instance.FactionName = (string)faction.Attribute("name");
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
                Cache.Instance.FactionName = "rogue drones";
                return;
            }
            if (eom)
            {
                Cache.Instance.FactionName = "eom";
                return;
            }
            if (mercenaries)
            {
                Cache.Instance.FactionName = "mercenaries";
                return;
            }
            if (seven)
            {
                Cache.Instance.FactionName = "the seven";
                return;
            }

            Logging.Log("AgentInteraction", "Unable to find the faction for [" + MissionName + "] when searching through the html (listed below)", Logging.Orange);

            Logging.Log("AgentInteraction", html, Logging.White);
            return;
        }

        private static DamageType GetMissionDamageType(string html)
        {
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
                        Cache.Instance.FactionName = (string)faction.Attribute("name");
                        Cache.Instance.MissionDamageType = (DamageType)Enum.Parse(typeof(DamageType), (string)faction.Attribute("damagetype"));
                        return (DamageType)Enum.Parse(typeof(DamageType), (string)faction.Attribute("damagetype"));
                    }
                }
            }
            Cache.Instance.MissionDamageType = DamageType.EM;
            return DamageType.EM;
        }

        private static void WaitForMission(string module)
        {
            if (!Cache.Instance.OpenAgentWindow(module)) return;

            if (!Cache.Instance.OpenJournalWindow(module)) return;

            Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentId, true);
            if (Cache.Instance.Mission == null)
            {
                if (_waitingOnMission == false)
                {
                    _waitingOnMissionTimer = DateTime.UtcNow;
                    _waitingOnMission = true;
                }
                if (DateTime.UtcNow.Subtract(_waitingOnMissionTimer).TotalSeconds > 30)
                {
                    Logging.Log("AgentInteraction", "WaitForMission: Unable to find mission from that agent (yet?) : AgentInteraction.AgentId [" + AgentId + "] regular Mission AgentID [" + Cache.Instance.AgentId + "]", Logging.Yellow);
                    Cache.Instance.JournalWindow.Close();
                    if (DateTime.UtcNow.Subtract(_waitingOnMissionTimer).TotalSeconds > 120)
                    {
                        Cache.Instance.CloseQuestorCMDLogoff = false;
                        Cache.Instance.CloseQuestorCMDExitGame = true;
                        Cache.Instance.ReasonToStopQuestor = "AgentInteraction: WaitforMission: Journal would not open/refresh - mission was null: restarting EVE Session";
                        Logging.Log("ReasonToStopQuestor", Cache.Instance.ReasonToStopQuestor, Logging.Yellow);
                        Cache.Instance.SessionState = "Quitting";
                    }
                }
                return;
            }

            _waitingOnMission = false;

            MissionName = Cache.Instance.FilterPath(Cache.Instance.Mission.Name);

            Logging.Log("AgentInteraction", "[" + Agent.Name + "] standing toward me is [" + Cache.Instance.AgentEffectiveStandingtoMeText + "], minAgentGreyListStandings: [" + Settings.Instance.MinAgentGreyListStandings + "]", Logging.Yellow);
            string html = Agent.Window.Objective;
            if (Settings.Instance.DebugAllMissionsOnBlackList || CheckFaction() || Settings.Instance.MissionBlacklist.Any(m => m.ToLower() == MissionName.ToLower()))
            {
                if (Purpose != AgentInteractionPurpose.AmmoCheck)
                {
                    Logging.Log("AgentInteraction", "Declining blacklisted mission [" + Cache.Instance.Mission.Name + "]", Logging.Yellow);
                }

                if (CheckFaction())
                {
                    Logging.Log("AgentInteraction", "Declining blacklisted mission [" + Cache.Instance.Mission.Name + "] because of faction blacklist", Logging.Yellow);
                }

                //
                // this is tracking declined missions before they are actually declined (bad?)
                // to fix this wed have to move this tracking stuff to the decline state and pass a reason we are
                // declining the mission to that process too... not knowing why we are declining is downright silly
                //
                Cache.Instance.LastBlacklistMissionDeclined = MissionName;
                Cache.Instance.BlackListedMissionsDeclined++;
                _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                return;
            }

            if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction", "[" + MissionName + "] is not on the blacklist and might be on the GreyList we have not checked yet", Logging.White);

            if (Settings.Instance.DebugAllMissionsOnGreyList || Settings.Instance.MissionGreylist.Any(m => m.ToLower() == MissionName.ToLower())) //-1.7
            {
                if (Cache.Instance.StandingUsedToAccessAgent > Settings.Instance.MinAgentGreyListStandings)
                {
                    Cache.Instance.LastGreylistMissionDeclined = MissionName;
                    Cache.Instance.GreyListedMissionsDeclined++;
                    Logging.Log("AgentInteraction", "Declining GreyListed mission [" + MissionName + "]", Logging.Yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                    return;
                }

                Logging.Log("AgentInteraction", "Unable to decline GreyListed mission: AgentEffectiveStandings [" + Cache.Instance.StandingUsedToAccessAgent + "] >  MinGreyListStandings [" + Settings.Instance.MinAgentGreyListStandings + "]", Logging.Orange);
            }
            else
            {
                if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction", "[" + MissionName + "] is not on the GreyList and will likely be run if it is not in lowsec, we have not checked for that yet", Logging.White);
            }

            //public bool RouteIsAllHighSec(long solarSystemId, List<long> currentDestination)
            //Cache.Instance.RouteIsAllHighSec(Cache.Instance.DirectEve.Session.SolarSystemId, );

            //
            // at this point we have not yet accepted the mission, thus we do not have the bookmark in people and places
            // we cannot and should not accept the mission without checking the route first, declining after accepting incurs a much larger penalty to standings
            //
            DirectBookmark missionBookmark = Cache.Instance.Mission.Bookmarks.FirstOrDefault();
            if (missionBookmark != null)
            {
                String missionLocationID = missionBookmark.LocationId.ToString();
                Logging.Log("AgentInteraction", "mission bookmark info: [" + missionLocationID + "]", Logging.White);
            }
            else
            {
                Logging.Log("AgentInteraction", "There are No Bookmarks Associated with " + Cache.Instance.Mission.Name + " yet", Logging.White);
            }

            if (html.Contains("The route generated by current autopilot settings contains low security systems!"))
            {
                bool decline = !Cache.Instance.CourierMission || (Cache.Instance.CourierMission && !Settings.Instance.AllowNonStorylineCourierMissionsInLowSec);

                if (decline)
                {
                    if (Purpose != AgentInteractionPurpose.AmmoCheck)
                    {
                        Logging.Log("AgentInteraction", "Declining [" + MissionName + "] because it was taking us through low-sec", Logging.Yellow);
                    }

                    _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
                    return;
                }
            }
            else
            {
                if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction", "[" + MissionName + "] is not in lowsec so we will do the mission", Logging.White);
            }

            //
            // if MissionName is a Courier Mission set Cache.Instance.CourierMission = true;
            //
            switch(MissionName)
            {
                case "Enemies Abound (2 of 5)":                       //lvl4 courier
                case "In the Midst of Deadspace (2 of 5)":            //lvl4 courier
                case "Pot and Kettle - Delivery (3 of 5)":            //lvl4 courier
                case "Technological Secrets (2 of 3)":                //lvl4 courier
                case "New Frontiers - Toward a Solution (3 of 7)":    //lvl3 courier
                case "New Frontiers - Nanite Express (6 of 7)":       //lvl3 courier
                case "Portal to War (3 of 5)":                        //lvl3 courier
                case "Guristas Strike - The Interrogation (2 of 10)": //lvl3 courier
                case "Guristas Strike - Possible Leads (4 of 10)":    //lvl3 courier
                case "Guristas Strike - The Flu Outbreak (6 of 10)":  //lvl3 courier
                case "Angel Strike - The Interrogation (2 of 10)":    //lvl3 courier
                case "Angel Strike - Possible Leads (4 of 10)":       //lvl3 courier
                case "Angel Strike - The Flu Outbreak (6 of 10)":     //lvl3 courier
                case "Interstellar Railroad (2 of 4)":                //lvl1 courier
                    Cache.Instance.CourierMission = true;
                    break;

                default:
                    Cache.Instance.CourierMission = false;
                    break;
            }

            if (!ForceAccept)
            {
                // Is the mission offered?
                if (Cache.Instance.Mission.State == (int)MissionState.Offered && (Cache.Instance.Mission.Type == "Mining" || Cache.Instance.Mission.Type == "Trade" || (Cache.Instance.Mission.Type == "Courier" && Cache.Instance.CourierMission)))
                {
                    if (!Cache.Instance.Mission.Important) //do not decline courier/mining/trade storylines!
                    {
                        Logging.Log("AgentInteraction", "Declining courier/mining/trade", Logging.Yellow);
                        _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                        _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                        return;    
                    }
                }
            }

            if (!Cache.Instance.CourierMission)
            {
                loadedAmmo = false;
                GetFactionName(html);
                GetDungeonId(html);
                Cache.Instance.SetmissionXmlPath(Cache.Instance.FilterPath(MissionName));

                Cache.Instance.MissionAmmo = new List<Ammo>();
                if (File.Exists(Cache.Instance.MissionXmlPath))
                {
                    LoadMissionXMLData();
                }
                else
                {
                    Logging.Log("AgentInteraction", "Missing mission xml [" + MissionName + "] from [" + Cache.Instance.MissionXmlPath + "] !!!", Logging.Orange);
                    Cache.Instance.MissionXMLIsAvailable = false;
                    if (Settings.Instance.RequireMissionXML)
                    {
                        Logging.Log("AgentInteraction", "Stopping Questor because RequireMissionXML is true in your character XML settings", Logging.Orange);
                        Logging.Log("AgentInteraction", "You will need to create a mission XML for [" + MissionName + "]", Logging.Orange);

                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                        return;
                    }
                }

                if (!loadedAmmo)
                {
                    Cache.Instance.MissionDamageType = GetMissionDamageType(html);
                    LoadSpecificAmmo(new[] { Cache.Instance.MissionDamageType });
                    Logging.Log("AgentInteraction", "Detected configured damage type for [" + MissionName + "] is [" + Cache.Instance.MissionDamageType + "]", Logging.Yellow);
                }

                if (Purpose == AgentInteractionPurpose.AmmoCheck)
                {
                    Logging.Log("AgentInteraction", "Closing conversation", Logging.Yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                    return;
                }
            }

            Cache.Instance.MissionName = MissionName;

            if (Cache.Instance.Mission.State == (int)MissionState.Offered)
            {
                Logging.Log("AgentInteraction", "Accepting mission [" + MissionName + "]", Logging.Yellow);

                _States.CurrentAgentInteractionState = AgentInteractionState.AcceptMission;
                _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
            }
            else // If we already accepted the mission, close the conversation
            {
                Logging.Log("AgentInteraction", "Mission [" + MissionName + "] already accepted", Logging.Yellow);
                Logging.Log("AgentInteraction", "Closing conversation", Logging.Yellow);

                //CheckFaction();
                _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
            }
        }

        private static void AcceptMission(string module)
        {
            if (!Cache.Instance.OpenAgentWindow(module)) return;

            List<DirectAgentResponse> responses = Agent.Window.AgentResponses;
            if (responses == null || responses.Count == 0)
                return;

            DirectAgentResponse accept = responses.FirstOrDefault(r => r.Text.Contains(Accept));
            if (accept == null)
                return;

            if (Cache.Instance.Agent.LoyaltyPoints == -1)
            {
                if (LoyaltyPointCounter < 10)
                {
                    Logging.Log("AgentInteraction", "Loyalty Points still -1; retrying", Logging.Red);
                    _nextAgentAction = DateTime.UtcNow.AddMilliseconds(500);
                    LoyaltyPointCounter++;
                    return;
                }
            }

            LoyaltyPointCounter = 0;
            Statistics.Instance.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;

            Logging.Log("AgentInteraction", "Saying [Accept]", Logging.Yellow);
            Cache.Instance.Wealth = Cache.Instance.DirectEve.Me.Wealth;
            accept.Say();

            Logging.Log("AgentInteraction", "Closing conversation", Logging.Yellow);
            _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
            _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 5));
        }

        private static void DeclineMission(string module)
        {
            // If we are doing an ammo check then Decline Mission is an end-state! (no, it is not... wtf)
            //if (Purpose == AgentInteractionPurpose.AmmoCheck)
            //{
            //    if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (Purpose == AgentInteractionPurpose.AmmoCheck) return", Logging.Debug);
            //    return;
            //}

            if (!Cache.Instance.OpenAgentWindow(module)) return;

            List<DirectAgentResponse> responses = Agent.Window.AgentResponses;
            if (responses == null || responses.Count == 0)
            {
                if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (responses == null || responses.Count == 0) return", Logging.Debug);
                return;
            }

            DirectAgentResponse decline = responses.FirstOrDefault(r => r.Text.Contains(Decline));
            if (decline == null)
            {
                if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (decline == null) return", Logging.Debug);
                return;
            }

            // Check for agent decline timer
            
            string html = Agent.Window.Briefing;
            if (html.Contains("Declining a mission from this agent within the next"))
            {
                //this need to divide by 10 was a remnant of the html scrape method we were using before. this can likely be removed now.
                if (Cache.Instance.StandingUsedToAccessAgent != 0)
                {
                    if (Cache.Instance.StandingUsedToAccessAgent > 10)
                    {
                        Logging.Log("AgentInteraction.DeclineMission", "if (Cache.Instance.StandingUsedToAccessAgent > 10)", Logging.Yellow);
                        Cache.Instance.StandingUsedToAccessAgent = Cache.Instance.StandingUsedToAccessAgent / 10;
                    }

                    if (Settings.Instance.MinAgentBlackListStandings > 10)
                    {
                        Logging.Log("AgentInteraction.DeclineMission", "if (Cache.Instance.StandingUsedToAccessAgent > 10)", Logging.Yellow);
                        Settings.Instance.MinAgentBlackListStandings = Settings.Instance.MinAgentBlackListStandings / 10;
                    }

                    Logging.Log("AgentInteraction.DeclineMission", "Agent decline timer detected. Current standings: " + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + ". Minimum standings: " + Math.Round(Settings.Instance.MinAgentBlackListStandings, 2), Logging.Yellow);
                }

                Regex hourRegex = new Regex("\\s(?<hour>\\d+)\\shour");
                Regex minuteRegex = new Regex("\\s(?<minute>\\d+)\\sminute");
                Match hourMatch = hourRegex.Match(html);
                Match minuteMatch = minuteRegex.Match(html);
                int hours = 0;
                int minutes = 0;
                if (hourMatch.Success)
                {
                    string hourValue = hourMatch.Groups["hour"].Value;
                    hours = Convert.ToInt32(hourValue);
                }
                if (minuteMatch.Success)
                {
                    string minuteValue = minuteMatch.Groups["minute"].Value;
                    minutes = Convert.ToInt32(minuteValue);
                }

                int secondsToWait = ((hours * 3600) + (minutes * 60) + 60);
                AgentsList currentAgent = Settings.Instance.ListOfAgents.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

                //
                // standings are below the blacklist minimum 
                // (any lower and we might lose access to this agent)
                // and no other agents are NOT available (or are also in cool-down)
                //
                if ((Settings.Instance.WaitDecline && Cache.Instance.AllAgentsStillInDeclineCoolDown))
                {
                    //
                    // if true we ALWAYS wait (or switch agents?!?)
                    //
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(secondsToWait);
                    Logging.Log("AgentInteraction.DeclineMission", "Waiting " + (secondsToWait / 60) + " minutes to try decline again because waitDecline setting is set to true", Logging.Yellow);
                    CloseConversation();
                    _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                    return;
                }

                //
                // if WaitDecline is false we only wait if standings are below our configured minimums
                //
                if (Cache.Instance.StandingUsedToAccessAgent <= Settings.Instance.MinAgentBlackListStandings)
                {
                    if (Settings.Instance.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (Cache.Instance.StandingUsedToAccessAgent <= Settings.Instance.MinAgentBlackListStandings)", Logging.Debug);
                    
                    //TODO - We should probably check if there are other agents who's effective standing is above the minAgentBlackListStanding.
                    if (Cache.Instance.AllAgentsStillInDeclineCoolDown)
                    {
                        //
                        // wait.
                        //
                        _nextAgentAction = DateTime.UtcNow.AddSeconds(secondsToWait);
                        Logging.Log("AgentInteraction.DeclineMission", "Current standings [" + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + "] at or below configured minimum of [" + Settings.Instance.MinAgentBlackListStandings + "].  Waiting " + (secondsToWait / 60) + " minutes to try decline again because no other agents were avail for use.", Logging.Yellow);
                        CloseConversation();
                        _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                        return;
                    }
                }

                if (!Cache.Instance.AllAgentsStillInDeclineCoolDown)
                {
                    if (currentAgent != null) currentAgent.DeclineTimer = DateTime.UtcNow.AddSeconds(secondsToWait);
                    CloseConversation();

                    Cache.Instance.CurrentAgent = Cache.Instance.SwitchAgent();
                    Logging.Log("AgentInteraction.DeclineMission", "new agent is " + Cache.Instance.CurrentAgent, Logging.Yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.ChangeAgent;
                    return;
                }

                Logging.Log("AgentInteraction.DeclineMission", "Current standings [" + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + "] is above our configured minimum [" + Settings.Instance.MinAgentBlackListStandings + "].  Declining [" + Cache.Instance.Mission.Name + "] note: WaitDecline is false", Logging.Yellow);
            }

            //
            // this closes the conversation, blacklists the agent for this session and goes back to base.
            //
            if (_States.CurrentStorylineState == StorylineState.DeclineMission || _States.CurrentStorylineState == StorylineState.AcceptMission)
            {
                Logging.Log("AgentInteraction.DeclineMission", "Storyline: Storylines cannot be declined, thus we will add this agent to the blacklist for this session.", Logging.Yellow);
                Logging.Log("AgentInteraction.DeclineMission", "Saying [Decline]", Logging.Yellow);
                Cache.Instance.Mission.RemoveOffer();
                _States.CurrentStorylineState = StorylineState.Idle;
                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                if (Settings.Instance.DeclineStorylinesInsteadofBlacklistingfortheSession)
                {
                    decline.Say();
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
                    return;
                }
                else
                {
                    Cache.Instance.AgentBlacklist.Add(Cache.Instance.CurrentStorylineAgentId);
                    Statistics.Instance.MissionCompletionErrors = 0;
                    return;    
                }
            }

            // Decline and request a new mission
            Logging.Log("AgentInteraction.DeclineMission", "Saying [Decline]", Logging.Yellow);
            decline.Say();

            Logging.Log("AgentInteraction.DeclineMission", "Replying to agent", Logging.Yellow);
            _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
            _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
            Statistics.Instance.MissionCompletionErrors = 0;
            return;
        }

        public static bool CheckFaction()
        {
            DirectAgentWindow agentWindow = Agent.Window;
            string html = agentWindow.Objective;
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

                    //Cache.Instance.FactionFit = "Default";
                    Cache.Instance.FittingToLoad = Settings.Instance.DefaultFitting.ToString();
                    Cache.Instance.FactionName = "Default";
                    if (faction != null)
                    {
                        string factionName = ((string)faction.Attribute("name"));
                        Cache.Instance.FactionName = factionName;
                        Logging.Log("AgentInteraction", "Mission enemy faction: " + factionName, Logging.Yellow);
                        if (Settings.Instance.FactionBlacklist.Any(m => m.ToLower() == factionName.ToLower()))
                        {
                            return true;
                        }

                        if (Settings.Instance.UseFittingManager && Settings.Instance.ListofFactionFittings.Any(m => m.FactionName.ToLower() == factionName.ToLower()))
                        {
                            FactionFitting _factionFittingForThisMissionsFaction = Settings.Instance.ListofFactionFittings.FirstOrDefault(m => m.FactionName.ToLower() == factionName.ToLower());
                            if (_factionFittingForThisMissionsFaction != null)
                            {
                                Cache.Instance.FactionFittingForThisMissionsFaction = _factionFittingForThisMissionsFaction.FittingName;
                                //
                                // if we have the drone type specified in the mission fitting entry use it, otherwise do not overwrite the default or the drone type specified by the faction
                                //
                                if (_factionFittingForThisMissionsFaction.DroneTypeID != null)
                                {
                                    Cache.Instance.DroneTypeID = (int)_factionFittingForThisMissionsFaction.DroneTypeID;
                                }
                                Logging.Log("AgentInteraction", "Faction fitting: " + _factionFittingForThisMissionsFaction.FactionName + "Using DroneTypeID [" + Cache.Instance.DroneTypeID + "]", Logging.Yellow);
                            }
                            else
                            {
                                Cache.Instance.FactionFittingForThisMissionsFaction = null;
                                Logging.Log("AgentInteraction", "Faction fitting: No fittings defined for [ " + factionName + " ]", Logging.Yellow);
                            }

                            //Cache.Instance.Fitting = Cache.Instance.FactionFit;
                            return false;
                        }
                    }
                    else
                    {
                        Logging.Log("AgentInteraction", "Faction fitting: Missing Factions.xml :aborting faction fittings", Logging.Yellow);
                    }
                }
            }
            else if (Settings.Instance.UseFittingManager) //only load the default fitting if we did not find the faction logo in the mission html.
            {
                Cache.Instance.FactionName = "Default";
                FactionFitting factionFitting = Settings.Instance.ListofFactionFittings.FirstOrDefault(m => m.FactionName.ToLower() == "default");
                if (factionFitting != null)
                {
                    Cache.Instance.FactionFittingForThisMissionsFaction = factionFitting.FittingName;
                    Logging.Log("AgentInteraction", "Faction fitting: " + factionFitting.FactionName, Logging.Yellow);
                }
                else
                {
                    Logging.Log("AgentInteraction", "Faction fitting: No fittings defined for [ " + Cache.Instance.FactionName + " ]", Logging.Orange);
                }

                //Cache.Instance.Fitting = Cache.Instance.FactionFit;
            }
            return false;
        }

        public static void CloseConversation()
        {
            if (DateTime.UtcNow < _nextAgentAction)
            {
                //Logging.Log("AgentInteraction.CloseConversation", "will continue in [" + Math.Round(_nextAgentAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "]sec", Logging.Yellow);
                return;
            }

            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow == null)
            {
                Logging.Log("AgentInteraction", "Done", Logging.Yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.Done;
            }

            if (agentWindow != null)
            {
                Logging.Log("AgentInteraction", "Attempting to close Agent Window", Logging.Yellow);
                _nextAgentAction = DateTime.UtcNow.AddSeconds(1);
                agentWindow.Close();
            }

            Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentId, true);
        }

        public static void ProcessState()
        {
            if (!Cache.Instance.InStation)
            {
                return;
            }

            if (Cache.Instance.InSpace)
            {
                return;
            }

            if (!Cache.Instance.Windows.Any())
            {
                return;
            }

            foreach (DirectWindow window in Cache.Instance.Windows)
            {
                if (window.Name == "modal")
                {
                    bool needHumanIntervention = false;
                    bool sayyes = false;

                    if (!string.IsNullOrEmpty(window.Html))
                    {
                        //errors that are repeatable and unavoidable even after a restart of eve/questor
                        needHumanIntervention |= window.Html.Contains("One or more mission objectives have not been completed");
                        needHumanIntervention |= window.Html.Contains("Please check your mission journal for further information");
                        needHumanIntervention |= window.Html.Contains("You have to be at the drop off location to deliver the items in person");

                        sayyes |= window.Html.Contains("objectives requiring a total capacity");
                        sayyes |= window.Html.Contains("your ship only has space for");
                    }

                    if (sayyes)
                    {
                        Logging.Log("AgentInteraction", "Found a window that needs 'yes' chosen...", Logging.Yellow);
                        Logging.Log("AgentInteraction", "Content of modal window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.Yellow);
                        window.AnswerModal("Yes");
                        continue;
                    }

                    if (needHumanIntervention)
                    {
                        Statistics.Instance.MissionCompletionErrors++;
                        Statistics.Instance.LastMissionCompletionError = DateTime.UtcNow;

                        Logging.Log("AgentInteraction", "This window indicates an error completing a mission: [" + Statistics.Instance.MissionCompletionErrors + "] errors already we will stop questor and halt restarting when we reach 3", Logging.White);
                        window.Close();

                        if (Statistics.Instance.MissionCompletionErrors > 3 && Cache.Instance.InStation)
                        {
                            if (Cache.Instance.MissionXMLIsAvailable)
                            {
                                Logging.Log("AgentInteraction", "ERROR: Mission XML is available for [" + Cache.Instance.MissionName + "] but we still did not complete the mission after 3 tries! - ERROR!", Logging.White);
                                Settings.Instance.AutoStart = false;

                                //we purposely disable autostart so that when we quit eve and questor here it stays closed until manually restarted as this error is fatal (and repeating)
                                //Cache.Instance.CloseQuestorCMDLogoff = false;
                                //Cache.Instance.CloseQuestorCMDExitGame = true;
                                //Cache.Instance.ReasonToStopQuestor = "Could not complete the mission: [" + Cache.Instance.MissionName + "] after [" + Statistics.Instance.MissionCompletionErrors + "] attempts: objective not complete or missing mission completion item or ???";
                                //Cache.Instance.SessionState = "Exiting";
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                            }
                            else
                            {
                                Logging.Log("AgentInteraction", "ERROR: Mission XML is missing for [" + Cache.Instance.MissionName + "] and we we unable to complete the mission after 3 tries! - ERROR!", Logging.White);
                                Settings.Instance.AutoStart = false; //we purposely disable autostart so that when we quit eve and questor here it stays closed until manually restarted as this error is fatal (and repeating)

                                //Cache.Instance.CloseQuestorCMDLogoff = false;
                                //Cache.Instance.CloseQuestorCMDExitGame = true;
                                //Cache.Instance.ReasonToStopQuestor = "Could not complete the mission: [" + Cache.Instance.MissionName + "] after [" + Statistics.Instance.MissionCompletionErrors + "] attempts: objective not complete or missing mission completion item or ???";
                                //Cache.Instance.SessionState = "Exiting";
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                            }
                        }
                        continue;
                    }
                }
            }

            // Wait a bit before doing "things"
            if (DateTime.UtcNow < _nextAgentAction)
                return;

            switch (_States.CurrentAgentInteractionState)
            {
                case AgentInteractionState.Idle:
                    break;

                case AgentInteractionState.Done:
                    break;

                case AgentInteractionState.ChangeAgent:
                    Logging.Log("AgentInteraction", "Change Agent", Logging.Yellow);
                    break;

                case AgentInteractionState.StartConversation:
                    StartConversation("AgentInteraction.StartConversation");
                    break;

                case AgentInteractionState.ReplyToAgent:
                    ReplyToAgent("AgentInteraction.ReplyToAgent");
                    break;

                case AgentInteractionState.WaitForMission:
                    WaitForMission("AgentInteraction.WaitForMission");
                    break;

                case AgentInteractionState.AcceptMission:
                    AcceptMission("AgentInteraction.AcceptMission");
                    break;

                case AgentInteractionState.DeclineMission:
                    DeclineMission("AgentInteraction.DeclineMission");
                    break;

                case AgentInteractionState.CloseConversation:
                    CloseConversation();
                    break;
            }
        }
    }
}