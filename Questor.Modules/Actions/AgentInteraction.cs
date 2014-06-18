// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using Questor.Modules.Combat;

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
        
        private static DateTime _agentWindowTimeStamp = DateTime.MinValue;
        private static DateTime _agentStandingsCheckTimeOut = DateTime.MaxValue;
        private static DateTime _nextAgentAction;
        private static DateTime _waitingOnAgentResponseTimer = DateTime.UtcNow;
        private static DateTime _waitingOnMissionTimer = DateTime.UtcNow;

        private static int LoyaltyPointCounter;
        public static float StandingsNeededToAccessLevel1Agent { get; set; }
        public static float StandingsNeededToAccessLevel2Agent { get; set; }
        public static float StandingsNeededToAccessLevel3Agent { get; set; }
        public static float StandingsNeededToAccessLevel4Agent { get; set; }
        public static float StandingsNeededToAccessLevel5Agent { get; set; }

        

        static AgentInteraction()
        {
            MissionSettings.AmmoToLoad = new List<Ammo>();
        }

        public static long AgentId { get; set; }

        public static DirectAgent Agent
        {
            get { return Cache.Instance.DirectEve.GetAgentById(AgentId); }
        }

        public static bool ForceAccept { get; set; }

        public static AgentInteractionPurpose Purpose { get; set; }

        private static void MyStandingsAreTooLowSwitchAgentsOrPause()
        {
            Cache.Instance.AgentEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(AgentId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentCorpEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.CorpId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentFactionEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.FactionId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);

            Cache.Instance.StandingUsedToAccessAgent = Math.Max(Cache.Instance.AgentEffectiveStandingtoMe, Math.Max(Cache.Instance.AgentCorpEffectiveStandingtoMe, Cache.Instance.AgentFactionEffectiveStandingtoMe));
            AgentsList currentAgent = MissionSettings.ListOfAgents.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

            if (MissionSettings.MultiAgentSupport)
            {
                //
                //Change Agents
                //
                if (currentAgent != null) currentAgent.DeclineTimer = DateTime.UtcNow.AddHours(999);
                CloseConversation();
                Cache.Instance.CurrentAgent = Cache.Instance.SwitchAgent();
                Cache.Instance.CurrentAgentText = Cache.Instance.CurrentAgent.ToString(CultureInfo.InvariantCulture);
                Logging.Log("AgentInteraction", "new agent is " + Cache.Instance.CurrentAgent, Logging.Yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.ChangeAgent;
                return;
            }

            Logging.Log("AgentInteraction.StartConversation", "", Logging.Orange);
            Cache.Instance.Paused = true;
        }

        private static void StartConversation(string module)
        {
            Cache.Instance.AgentEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(AgentId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentCorpEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.CorpId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);
            Cache.Instance.AgentFactionEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(Agent.FactionId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);

            Cache.Instance.StandingUsedToAccessAgent = Math.Max(Cache.Instance.AgentEffectiveStandingtoMe,Math.Max(Cache.Instance.AgentCorpEffectiveStandingtoMe,Cache.Instance.AgentFactionEffectiveStandingtoMe));
            AgentsList currentAgent = MissionSettings.ListOfAgents.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

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
                    if (Cache.Instance.StandingUsedToAccessAgent < StandingsNeededToAccessLevel1Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + StandingsNeededToAccessLevel1Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;
             
                case 2: //lvl2 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < StandingsNeededToAccessLevel2Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + StandingsNeededToAccessLevel2Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;

                case 3: //lvl3 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < StandingsNeededToAccessLevel3Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + StandingsNeededToAccessLevel3Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;

                case 4: //lvl4 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < StandingsNeededToAccessLevel4Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + StandingsNeededToAccessLevel4Agent + "]", Logging.Orange);
                        MyStandingsAreTooLowSwitchAgentsOrPause();
                        return;
                    }
                    break;

                case 5: //lvl5 agent
                    if (Cache.Instance.StandingUsedToAccessAgent < StandingsNeededToAccessLevel5Agent)
                    {
                        Logging.Log("AgentInteraction.StartConversation", "Our Standings to [" + Agent.Name + "] are [" + Cache.Instance.StandingUsedToAccessAgent + "] < [" + StandingsNeededToAccessLevel5Agent + "]", Logging.Orange);
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
                    if (Logging.DebugAgentInteractionReplyToAgent) Logging.Log("AgentInteraction.ReplyToAgent", "Debug: if (_waitingOnAgentResponse == false)", Logging.Yellow);
                    _waitingOnAgentResponseTimer = DateTime.UtcNow;
                    _waitingOnAgentResponse = true;
                }

                if (DateTime.UtcNow.Subtract(_waitingOnAgentResponseTimer).TotalSeconds > 15)
                {
                    Logging.Log("AgentInteraction.ReplyToAgent", "Debug: agentWindowAgentresponses == null : trying to close the agent window", Logging.Yellow);
                    Agent.Window.Close();
                }

                if (Logging.DebugAgentInteractionReplyToAgent) Logging.Log("AgentInteraction.ReplyToAgent", "Debug: if (Agent.Window.AgentResponses == null || !Agent.Window.AgentResponses.Any())", Logging.Yellow);
                return;
            }

            if (Agent.Window.AgentResponses.Any())
            {
                if (Logging.DebugAgentInteractionReplyToAgent) Logging.Log("AgentInteraction.ReplyToAgent", "Debug: we have Agent.Window.AgentResponces", Logging.Yellow);
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
                if (Logging.DebugAgentInteractionReplyToAgent) Logging.Log(module, "if (Purpose != AgentInteractionPurpose.AmmoCheck) //do not change the AgentInteractionPurpose if we are checking which ammo type to use.", Logging.Yellow);
                if (accept != null && decline != null && delay != null)
                {
                    if (Purpose != AgentInteractionPurpose.StartMission)
                    {
                        Logging.Log("AgentInteraction", "ReplyToAgent: Found accept button, Changing Purpose to StartMission", Logging.White);
                        _agentWindowTimeStamp = DateTime.UtcNow;
                        Purpose = AgentInteractionPurpose.StartMission;
                    }
                }

                if (complete != null && quit != null && close != null && Statistics.MissionCompletionErrors == 0)
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
                    MissionSettings.FactionName = string.Empty;

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

        private static void WaitForMission(string module)
        {
            if (!Cache.Instance.OpenAgentWindow(module)) return;

            if (!Cache.Instance.OpenJournalWindow(module)) return;

            MissionSettings.Mission = Cache.Instance.GetAgentMission(AgentId, true);
            if (MissionSettings.Mission == null)
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

            MissionName = Cache.Instance.FilterPath(MissionSettings.Mission.Name);

            Logging.Log("AgentInteraction", "[" + Agent.Name + "] standing toward me is [" + Cache.Instance.AgentEffectiveStandingtoMeText + "], minAgentGreyListStandings: [" + MissionSettings.MinAgentGreyListStandings + "]", Logging.Yellow);
            string html = Agent.Window.Objective;
            if (Logging.DebugAllMissionsOnBlackList || CheckFaction() || MissionSettings.MissionBlacklist.Any(m => m.ToLower() == MissionName.ToLower()))
            {
                if (Purpose != AgentInteractionPurpose.AmmoCheck)
                {
                    Logging.Log("AgentInteraction", "Declining blacklisted mission [" + MissionSettings.Mission.Name + "]", Logging.Yellow);
                }

                if (CheckFaction())
                {
                    Logging.Log("AgentInteraction", "Declining blacklisted mission [" + MissionSettings.Mission.Name + "] because of faction blacklist", Logging.Yellow);
                }

                //
                // this is tracking declined missions before they are actually declined (bad?)
                // to fix this wed have to move this tracking stuff to the decline state and pass a reason we are
                // declining the mission to that process too... not knowing why we are declining is downright silly
                //
                MissionSettings.LastBlacklistMissionDeclined = MissionName;
                MissionSettings.BlackListedMissionsDeclined++;
                _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                return;
            }

            if (Logging.DebugDecline) Logging.Log("AgentInteraction", "[" + MissionName + "] is not on the blacklist and might be on the GreyList we have not checked yet", Logging.White);

            if (Logging.DebugAllMissionsOnGreyList || MissionSettings.MissionGreylist.Any(m => m.ToLower() == MissionName.ToLower())) //-1.7
            {
                if (Cache.Instance.StandingUsedToAccessAgent > MissionSettings.MinAgentGreyListStandings)
                {
                    MissionSettings.LastGreylistMissionDeclined = MissionName;
                    MissionSettings.GreyListedMissionsDeclined++;
                    Logging.Log("AgentInteraction", "Declining GreyListed mission [" + MissionName + "]", Logging.Yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                    _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                    return;
                }

                Logging.Log("AgentInteraction", "Unable to decline GreyListed mission: AgentEffectiveStandings [" + Cache.Instance.StandingUsedToAccessAgent + "] >  MinGreyListStandings [" + MissionSettings.MinAgentGreyListStandings + "]", Logging.Orange);
            }
            else
            {
                if (Logging.DebugDecline) Logging.Log("AgentInteraction", "[" + MissionName + "] is not on the GreyList and will likely be run if it is not in lowsec, we have not checked for that yet", Logging.White);
            }

            //public bool RouteIsAllHighSec(long solarSystemId, List<long> currentDestination)
            //Cache.Instance.RouteIsAllHighSec(Cache.Instance.DirectEve.Session.SolarSystemId, );

            //
            // at this point we have not yet accepted the mission, thus we do not have the bookmark in people and places
            // we cannot and should not accept the mission without checking the route first, declining after accepting incurs a much larger penalty to standings
            //
            DirectBookmark missionBookmark = MissionSettings.Mission.Bookmarks.FirstOrDefault();
            if (missionBookmark != null)
            {
                String missionLocationID = missionBookmark.LocationId.ToString();
                Logging.Log("AgentInteraction", "mission bookmark info: [" + missionLocationID + "]", Logging.White);
            }
            else
            {
                Logging.Log("AgentInteraction", "There are No Bookmarks Associated with " + MissionSettings.Mission.Name + " yet", Logging.White);
            }

            if (html.Contains("The route generated by current autopilot settings contains low security systems!"))
            {
                bool decline = !Cache.Instance.CourierMission || (Cache.Instance.CourierMission && !MissionSettings.AllowNonStorylineCourierMissionsInLowSec);

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
                if (Logging.DebugDecline) Logging.Log("AgentInteraction", "[" + MissionName + "] is not in lowsec so we will do the mission", Logging.White);
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
                if (MissionSettings.Mission.State == (int)MissionState.Offered && (MissionSettings.Mission.Type == "Mining" || MissionSettings.Mission.Type == "Trade" || (MissionSettings.Mission.Type == "Courier" && Cache.Instance.CourierMission)))
                {
                    if (!MissionSettings.Mission.Important) //do not decline courier/mining/trade storylines!
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
                MissionSettings.loadedAmmo = false;
                MissionSettings.GetFactionName(html);
                MissionSettings.GetDungeonId(html);
                MissionSettings.SetmissionXmlPath(Cache.Instance.FilterPath(MissionName));

                MissionSettings.MissionAmmo = new List<Ammo>();
                if (File.Exists(MissionSettings.MissionXmlPath))
                {
                    MissionSettings.LoadMissionXMLData();
                }
                else
                {
                    Logging.Log("AgentInteraction", "Missing mission xml [" + MissionName + "] from [" + MissionSettings.MissionXmlPath + "] !!!", Logging.Orange);
                    MissionSettings.MissionXMLIsAvailable = false;
                    if (MissionSettings.RequireMissionXML)
                    {
                        Logging.Log("AgentInteraction", "Stopping Questor because RequireMissionXML is true in your character XML settings", Logging.Orange);
                        Logging.Log("AgentInteraction", "You will need to create a mission XML for [" + MissionName + "]", Logging.Orange);

                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                        return;
                    }
                }

                if (!MissionSettings.loadedAmmo)
                {
                    MissionSettings.MissionDamageType = MissionSettings.GetMissionDamageType(html);
                    MissionSettings.LoadSpecificAmmo(new[] { MissionSettings.MissionDamageType });
                    Logging.Log("AgentInteraction", "Detected configured damage type for [" + MissionName + "] is [" + MissionSettings.MissionDamageType + "]", Logging.Yellow);
                }

                if (Purpose == AgentInteractionPurpose.AmmoCheck)
                {
                    Logging.Log("AgentInteraction", "Closing conversation", Logging.Yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                    return;
                }
            }

            MissionSettings.MissionName = MissionName;

            if (MissionSettings.Mission.State == (int)MissionState.Offered)
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
            Statistics.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;

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
            //    if (Logging.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (Purpose == AgentInteractionPurpose.AmmoCheck) return", Logging.Debug);
            //    return;
            //}

            if (!Cache.Instance.OpenAgentWindow(module)) return;

            List<DirectAgentResponse> responses = Agent.Window.AgentResponses;
            if (responses == null || responses.Count == 0)
            {
                if (Logging.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (responses == null || responses.Count == 0) return", Logging.Debug);
                return;
            }

            DirectAgentResponse decline = responses.FirstOrDefault(r => r.Text.Contains(Decline));
            if (decline == null)
            {
                if (Logging.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (decline == null) return", Logging.Debug);
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

                    if (MissionSettings.MinAgentBlackListStandings > 10)
                    {
                        Logging.Log("AgentInteraction.DeclineMission", "if (Cache.Instance.StandingUsedToAccessAgent > 10)", Logging.Yellow);
                        MissionSettings.MinAgentBlackListStandings = MissionSettings.MinAgentBlackListStandings / 10;
                    }

                    Logging.Log("AgentInteraction.DeclineMission", "Agent decline timer detected. Current standings: " + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + ". Minimum standings: " + Math.Round(MissionSettings.MinAgentBlackListStandings, 2), Logging.Yellow);
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
                AgentsList currentAgent = MissionSettings.ListOfAgents.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

                //
                // standings are below the blacklist minimum 
                // (any lower and we might lose access to this agent)
                // and no other agents are NOT available (or are also in cool-down)
                //
                if ((MissionSettings.WaitDecline && !MissionSettings.MultiAgentSupport) || (MissionSettings.WaitDecline && MissionSettings.MultiAgentSupport && Cache.Instance.AllAgentsStillInDeclineCoolDown))
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
                if (Cache.Instance.StandingUsedToAccessAgent <= MissionSettings.MinAgentBlackListStandings)
                {
                    if (Logging.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (Cache.Instance.StandingUsedToAccessAgent <= Settings.Instance.MinAgentBlackListStandings)", Logging.Debug);
                    
                    //
                    // If we have multiple agents defined - switch agents
                    //
                    if (MissionSettings.MultiAgentSupport)
                    {
                        //TODO - We should probably check if there are other agents who's effective standing is above the minAgentBlackListStanding.
                        if (Logging.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (Settings.Instance.MultiAgentSupport)", Logging.Debug);
                        if (Cache.Instance.AllAgentsStillInDeclineCoolDown)
                        {
                            //
                            // wait.
                            //
                            _nextAgentAction = DateTime.UtcNow.AddSeconds(secondsToWait);
                            Logging.Log("AgentInteraction.DeclineMission", "Current standings [" + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + "] at or below configured minimum of [" + MissionSettings.MinAgentBlackListStandings + "].  Waiting " + (secondsToWait / 60) + " minutes to try decline again because no other agents were avail for use.", Logging.Yellow);
                            CloseConversation();
                            _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                            return;
                        }
                    }

                    _nextAgentAction = DateTime.UtcNow.AddSeconds(secondsToWait);
                    Logging.Log("AgentInteraction.DeclineMission", "Current standings [" + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + "] at or below configured minimum of [" + MissionSettings.MinAgentBlackListStandings + "].  Waiting " + (secondsToWait / 60) + " minutes to try decline again.", Logging.Yellow);
                    CloseConversation();
                    _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                    return;
                }

                if (MissionSettings.MultiAgentSupport && !Cache.Instance.AllAgentsStillInDeclineCoolDown)
                {
                    if (Logging.DebugDecline) Logging.Log("AgentInteraction.DeclineMission", "if (Settings.Instance.MultiAgentSupport)", Logging.Debug);

                    //
                    //Change Agents
                    //
                    if (currentAgent != null) currentAgent.DeclineTimer = DateTime.UtcNow.AddSeconds(secondsToWait);
                    CloseConversation();

                    Cache.Instance.CurrentAgent = Cache.Instance.SwitchAgent();
                    Cache.Instance.CurrentAgentText = Cache.Instance.CurrentAgent.ToString(CultureInfo.InvariantCulture);
                    Logging.Log("AgentInteraction.DeclineMission", "new agent is " + Cache.Instance.CurrentAgent, Logging.Yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.ChangeAgent;
                    return;
                }

                Logging.Log("AgentInteraction.DeclineMission", "Current standings [" + Math.Round(Cache.Instance.StandingUsedToAccessAgent, 2) + "] is above our configured minimum [" + MissionSettings.MinAgentBlackListStandings + "].  Declining [" + MissionSettings.Mission.Name + "] note: WaitDecline is false", Logging.Yellow);
            }

            //
            // this closes the conversation, blacklists the agent for this session and goes back to base.
            //
            if (_States.CurrentStorylineState == StorylineState.DeclineMission || _States.CurrentStorylineState == StorylineState.AcceptMission)
            {
                Logging.Log("AgentInteraction.DeclineMission", "Storyline: Storylines cannot be declined, thus we will add this agent to the blacklist for this session.", Logging.Yellow);
                Logging.Log("AgentInteraction.DeclineMission", "Saying [Decline]", Logging.Yellow);
                MissionSettings.Mission.RemoveOffer();
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
                    Statistics.MissionCompletionErrors = 0;
                    return;    
                }
            }

            // Decline and request a new mission
            Logging.Log("AgentInteraction.DeclineMission", "Saying [Decline]", Logging.Yellow);
            decline.Say();

            Logging.Log("AgentInteraction.DeclineMission", "Replying to agent", Logging.Yellow);
            _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
            _nextAgentAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(3, 7));
            Statistics.MissionCompletionErrors = 0;
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
                    MissionSettings.FittingToLoad = MissionSettings.DefaultFitting.ToString();
                    MissionSettings.FactionName = "Default";
                    if (faction != null)
                    {
                        string factionName = ((string)faction.Attribute("name"));
                        MissionSettings.FactionName = factionName;
                        Logging.Log("AgentInteraction", "Mission enemy faction: " + factionName, Logging.Yellow);
                        if (MissionSettings.FactionBlacklist.Any(m => m.ToLower() == factionName.ToLower()))
                        {
                            return true;
                        }

                        if (Settings.Instance.UseFittingManager && MissionSettings.ListofFactionFittings.Any(m => m.FactionName.ToLower() == factionName.ToLower()))
                        {
                            FactionFitting _factionFittingForThisMissionsFaction = MissionSettings.ListofFactionFittings.FirstOrDefault(m => m.FactionName.ToLower() == factionName.ToLower());
                            if (_factionFittingForThisMissionsFaction != null)
                            {
                                MissionSettings.FactionFittingForThisMissionsFaction = _factionFittingForThisMissionsFaction.FittingName;
                                //
                                // if we have the drone type specified in the mission fitting entry use it, otherwise do not overwrite the default or the drone type specified by the faction
                                //
                                if (_factionFittingForThisMissionsFaction.DroneTypeID != null && _factionFittingForThisMissionsFaction.DroneTypeID != 0)
                                {
                                    Drones.DroneTypeID = (int)_factionFittingForThisMissionsFaction.DroneTypeID;
                                }
                                Logging.Log("AgentInteraction", "Faction fitting: " + _factionFittingForThisMissionsFaction.FactionName + "Using DroneTypeID [" + Drones.DroneTypeID + "]", Logging.Yellow);
                            }
                            else
                            {
                                MissionSettings.FactionFittingForThisMissionsFaction = null;
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
                MissionSettings.FactionName = "Default";
                FactionFitting factionFitting = MissionSettings.ListofFactionFittings.FirstOrDefault(m => m.FactionName.ToLower() == "default");
                if (factionFitting != null)
                {
                    MissionSettings.FactionFittingForThisMissionsFaction = factionFitting.FittingName;
                    Logging.Log("AgentInteraction", "Faction fitting: " + factionFitting.FactionName, Logging.Yellow);
                }
                else
                {
                    Logging.Log("AgentInteraction", "Faction fitting: No fittings defined for [ " + MissionSettings.FactionName + " ]", Logging.Orange);
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

            MissionSettings.Mission = Cache.Instance.GetAgentMission(AgentId, true);
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
                        Statistics.MissionCompletionErrors++;
                        Statistics.LastMissionCompletionError = DateTime.UtcNow;

                        Logging.Log("AgentInteraction", "This window indicates an error completing a mission: [" + Statistics.MissionCompletionErrors + "] errors already we will stop questor and halt restarting when we reach 3", Logging.White);
                        window.Close();

                        if (Statistics.MissionCompletionErrors > 3 && Cache.Instance.InStation)
                        {
                            if (MissionSettings.MissionXMLIsAvailable)
                            {
                                Logging.Log("AgentInteraction", "ERROR: Mission XML is available for [" + MissionSettings.MissionName + "] but we still did not complete the mission after 3 tries! - ERROR!", Logging.White);
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
                                Logging.Log("AgentInteraction", "ERROR: Mission XML is missing for [" + MissionSettings.MissionName + "] and we we unable to complete the mission after 3 tries! - ERROR!", Logging.White);
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