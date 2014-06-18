﻿// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------


namespace QuestorLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    //using Questor;
    //using Questor.Modules.Caching;
    //using Questor.Modules.Lookup;
    using InnerSpaceAPI;
    using LavishScriptAPI;
    
    public static class Logging
    {
        public static int LoggingInstances = 0;

        static Logging()
        {
            Interlocked.Increment(ref LoggingInstances);
            string characterNameForLogs = "QuestorLoader";
            Logging.Logpath = (System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log\\" + characterNameForLogs + "\\");
            //logpath_s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log\\";
            Logging.ConsoleLogPath = System.IO.Path.Combine(Logging.Logpath, "Console\\");
            Logging.ConsoleLogFile = System.IO.Path.Combine(Logging.ConsoleLogPath, string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-" + characterNameForLogs + "-" + "console" + ".log");
            Logging.ConsoleLogPathRedacted = System.IO.Path.Combine(Logging.Logpath, "Console\\");
            Logging.ConsoleLogFileRedacted = System.IO.Path.Combine(Logging.ConsoleLogPath, string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-" + "redacted" + "-" + "console" + ".log");
        }

        //~Logging()
        //{
        //    Interlocked.Decrement(ref LoggingInstances);
        //}

        public static bool DebugMaintainConsoleLogs { get; set; }
        
        public static DateTime DateTimeForLogs;
        //list of colors
        public const string Green = "\ag";    //traveler mission control
        public const string Yellow = "\ay";
        public const string Blue = "\ab";     //DO NOT USE - blends into default lavish GUIs background.
        public const string Red = "\ar";      //error panic
        public const string Orange = "\ao";   //error can fix
        public const string Purple = "\ap";   //combat
        public const string Magenta = "\am";  //drones
        public const string Teal = "\at";     //log debug
        public const string White = "\aw";    //questor


        public const string Debug = Teal;     //log debug

        public const string CombatUserCorrectableErrors = Orange;
        public const string CombatFatalErrors = Red;
        public const string CombatGenericLogging = White;

        public const string DronesUserCorrectableErrors = Orange;
        public const string DronesFatalErrors = Red;
        public const string DronesGenericLogging = White;

        public const string TravelerUserCorrectableErrors = Orange;
        public const string TravelerFatalErrors = Red;
        public const string TravelerGenericLogging = White;
        public const string TravelerDestinationColor = White;

        public static string _username;
        public static string _password;
        public static string _character;
        public static string CharacterSettingsPath;

        public static bool standaloneInstance;
        public static bool tryToLogToFile;
        public static List<string> _QuestorParamaters;

        private static string colorLogLine;
        private static string ConsoleLogText;
        public static bool SaveConsoleLog;
        public static bool ConsoleLogOpened = false;
        public static string ExtConsole { get; set; }
        //public static string ConsoleLog { get; set; }
        //public static string ConsoleLogRedacted { get; set; }
        public static string Logpath { get; set; }
        public static bool InnerspaceGeneratedConsoleLog { get; set; }
        public static bool UseInnerspace { get; set; }
        //public static bool ConsoleLog { get; set; }
        public static string ConsoleLogPath { get; set; }
        public static string ConsoleLogFile { get; set; }
        public static bool SaveLogRedacted { get; set; }
        //public static bool ConsoleLogRedacted { get; set; }
        public static string redactedLogLine { get; set; }
        public static string ConsoleLogPathRedacted { get; set; }
        public static string ConsoleLogFileRedacted { get; set; }
        
        //
        // number of days of console logs to keep (anything older will be deleted on startup)
        //
        public static int ConsoleLogDaysOfLogsToKeep { get; set; }


        //public  void Log(string line)
        //public static void Log(string module, string line, string color = Logging.White)
        public static void Log(string module, string line, string color, bool verbose = false)
        {
            DateTimeForLogs = DateTime.Now;
            //colorLogLine contains color and is for the InnerSpace console
            colorLogLine = line;

            //Logging when using Innerspace
            if (!Logging.standaloneInstance)
            {
                InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, Logging.Orange + "[" + Logging.Yellow + module + Logging.Orange + "] " + color + colorLogLine));
            }
          	// probably want some sort of extra logging if using the standalone version? we dont have any output until q window is up
           
            string plainLogLine = FilterColorsFromLogs(line);

            //
            // plainLogLine contains plain text and is for the log file and the GUI console (why cant the GUI be made to use color too?)
            // we now filter sensitive info by default
            //
            //redactedLogLine = String.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "[" + module + "] " + FilterSensitiveInfo(plainLogLine) + "\r\n");  //In memory Console Log with sensitive info redacted
            plainLogLine = FilterColorsFromLogs(line);
            if (Logging.standaloneInstance)
            {
                Console.WriteLine(plainLogLine);
            }

            if (Logging.tryToLogToFile)
            {
                if (Logging.SaveConsoleLog)//(Settings.Instance.SaveConsoleLog)
                {
                    if (!Logging.ConsoleLogOpened)
                    {
                        //
                        // begin logging to file
                        //
                        if (Logging.ConsoleLogPath != null && Logging.ConsoleLogFile != null)
                        {
                            module = "Logging";
                            if (Logging.InnerspaceGeneratedConsoleLog && Logging.UseInnerspace)
                            {
                                InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "log " + Logging.ConsoleLogFile + "-innerspace-generated.log"));
                                LavishScript.ExecuteCommand("log " + Logging.ConsoleLogFile + "-innerspace-generated.log");
                            }

                            Logging.ExtConsole = string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, plainLogLine + "\r\n");

                            if (!string.IsNullOrEmpty(Logging.ConsoleLogFile))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(Logging.ConsoleLogFile));
                                if (Directory.Exists(Path.GetDirectoryName(Logging.ConsoleLogFile)))
                                {
                                    Logging.ConsoleLogText = string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "[" + module + "]" + plainLogLine + "\r\n");
                                    Logging.ConsoleLogOpened = true;
                                }
                                else
                                {
                                    if (Logging.UseInnerspace) InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "Logging: Unable to find (or create): " + Logging.ConsoleLogPath));
                                }
                                line = "";
                            }
                            else
                            {
                                line = "Logging: Unable to write log to file yet as: ConsoleLogFile is not yet defined";
                                if (Logging.UseInnerspace) InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, colorLogLine));
                                Logging.ExtConsole = string.Format("{0:HH:mm:ss} {1}", DateTime.UtcNow, "[" + module + "] " + plainLogLine + "\r\n");
                            }
                        }
                    }

                    if (Logging.ConsoleLogOpened)
                    {
                        //
                        // log file ready: add next logging entry...
                        //
                        if (Logging.ConsoleLogFile != null && !verbose) //normal
                        {

                            //
                            // normal text logging
                            //
                            Logging.ConsoleLogText = string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "[" + module + "]" + plainLogLine + "\r\n");
                            File.AppendAllText(Logging.ConsoleLogFile, Logging.ConsoleLogText); //Write In Memory Console log to File
                        }

                        if (Logging.ConsoleLogFile != null && verbose) //tons of info
                        {
                            //
                            // verbose text logging - with line numbers, filenames and Methods listed ON EVERY LOGGING LINE - this is ALOT more detail
                            //
                            System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1, true);
                            module += "-[line" + sf.GetFileLineNumber().ToString() + "]in[" + System.IO.Path.GetFileName(sf.GetFileName()) + "][" + sf.GetMethod().Name + "]";
                            Logging.ConsoleLogText = string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "[" + module + "]" + plainLogLine + "\r\n");
                            File.AppendAllText(Logging.ConsoleLogFile, Logging.ConsoleLogText); //Write In Memory Console log to File
                        }
                        //Cache.Instance.ConsoleLog = null;

                        if (Logging.ConsoleLogFileRedacted != null)
                        {
                            //
                            // redacted text logging - sensitive info removed so you can generally paste the contents of this log publicly w/o fear of easily exposing user identifiable info
                            //
                            Logging.ExtConsole = string.Format("{0:HH:mm:ss} {1}", DateTime.UtcNow, "[" + module + "] " + plainLogLine + "\r\n");
                            File.AppendAllText(Logging.ConsoleLogFileRedacted, Logging.redactedLogLine);               //Write In Memory Console log to File
                        }
                        //Cache.Instance.ConsoleLogRedacted = null;
                    }

                    Logging.ExtConsole = string.Format("{0:HH:mm:ss} {1}", DateTime.UtcNow, "[" + module + "] " + plainLogLine + "\r\n");
                }    
            }
        }

        //path = path.Replace(Environment.CommandLine, "");
        //path = path.Replace(Environment.GetCommandLineArgs(), "");

        public static string FilterSensitiveInfo(string line)
        {
            if (line == null)
                return string.Empty;
            if (!string.IsNullOrEmpty(Logging._character))
            {
                line = line.Replace(Logging._character, Logging._character.Substring(0, 2) + "_MyEVECharacterNameRedacted_");
                line = line.Replace("/" + Logging._character, "/" + Logging._character.Substring(0, 2) + "_MyEVECharacterNameRedacted_");
                line = line.Replace("\\" + Logging._character, "\\" + Logging._character.Substring(0, 2) + "_MyEVECharacterNameRedacted_");
                line = line.Replace("[" + Logging._character + "]", "[" + Logging._character.Substring(0, 2) + "_MyEVECharacterNameRedacted_]");
                line = line.Replace(Logging._character + ".xml", Logging._character.Substring(0, 2) + "_MyEVECharacterNameRedacted_.xml");
                line = line.Replace(Logging.CharacterSettingsPath, Logging.CharacterSettingsPath.Substring(0, 2) + "_MySettingsFileNameRedacted_.xml");
                //line = line.Replace(Cache.Instance._agentName, Cache.Instance._agentName.Substring(0, 2) + "_MyCurrentAgentName_");
                //line = line.Replace(Cache.Instance.AgentId, "_MyCurrentAgentID_");
            }

            //if (!string.IsNullOrEmpty(Cache.Instance.CurrentAgent))
            //{
            //    if (Logging.DebugLogging) InnerSpace.Echo("Logging.Log: FilterSensitiveInfo: CurrentAgent exists [" + Cache.Instance.CurrentAgent + "]");
            //    line = line.Replace(" " + Cache.Instance.CurrentAgent + " ", " _MyCurrentAgentRedacted_ ");
            //    line = line.Replace("[" + Cache.Instance.CurrentAgent + "]", "[_MyCurrentAgentRedacted_]");
            //}
            //if (Cache.Instance.AgentId != -1)
            //{
            //    if(Logging.DebugLogging) InnerSpace.Echo("Logging.Log: FilterSensitiveInfo: AgentId is not -1");
            //    line = line.Replace(" " + Cache.Instance.AgentId + " ", " _MyAgentIdRedacted_ ");
            //    line = line.Replace("[" + Cache.Instance.AgentId + "]", "[_MyAgentIdRedacted_]");
            //}
            if (!String.IsNullOrEmpty(Logging._username))
            {
                line = line.Replace(Logging._username, Logging._username.Substring(0, 2) + "_HiddenEVELoginName_");
            }
            if (!String.IsNullOrEmpty(Logging._password))
            {
                line = line.Replace(Logging._password, "_HiddenPassword_");
            }
            if (!string.IsNullOrEmpty(Environment.UserName))
            {
                line = line.Replace("\\" + Environment.UserName + "\\", "\\_MyWindowsLoginNameRedacted_\\");
                line = line.Replace("/" + Environment.UserName + "/", "/_MyWindowsLoginNameRedacted_/");
            }
            if (!string.IsNullOrEmpty(Environment.UserDomainName))
            {
                line = line.Replace(Environment.UserDomainName, "_MyWindowsDomainNameRedacted_");
            }
            return line;
        }
        
        public static string FilterColorsFromLogs(string line)
        {
            if (line == null)
                return string.Empty;

            line = line.Replace("\ag", "");
            line = line.Replace("\ay", "");
            line = line.Replace("\ab", "");
            line = line.Replace("\ar", "");
            line = line.Replace("\ao", "");
            line = line.Replace("\ap", "");
            line = line.Replace("\am", "");
            line = line.Replace("\at", "");
            line = line.Replace("\aw", "");
            while (line.IndexOf("  ", System.StringComparison.Ordinal) >= 0)
                line = line.Replace("  ", " ");
            return line.Trim();
        }
//
        // Debug Variables
        //
        public static bool DebugActivateGate { get; set; }
        public static bool DebugActivateWeapons { get; set; }
        public static bool DebugActivateBastion { get; set; }
        public static bool DebugAddDronePriorityTarget { get; set; }
        public static bool DebugAddPrimaryWeaponPriorityTarget { get; set; }
        public static bool DebugAgentInteractionReplyToAgent { get; set; }
        public static bool DebugAllMissionsOnBlackList { get; set; }
        public static bool DebugAllMissionsOnGreyList { get; set; }
        public static bool DebugAmmo { get; set; }
        public static bool DebugArm { get; set; }
        public static bool DebugAttachVSDebugger { get; set; }
        public static bool DebugAutoStart { get; set; }
        public static bool DebugBlackList { get; set; }
        public static bool DebugCargoHold { get; set; }
        public static bool DebugChat { get; set; }
        public static bool DebugCleanup { get; set; }
        public static bool DebugClearPocket { get; set; }
        public static bool DebugCourierMissions { get; set; }
        public static bool DebugDecline { get; set; }
        public static bool DebugDefense { get; set; }
        public static bool DebugDisableCleanup { get; set; }
        public static bool DebugDisableCombatMissionsBehavior { get; set; }
        public static bool DebugDisableCombatMissionCtrl { get; set; }
        public static bool DebugDisableCombat { get; set; }
        public static bool DebugDisableDrones { get; set; }
        public static bool DebugDisablePanic { get; set; }
        public static bool DebugDisableSalvage { get; set; }
        public static bool DebugDisableTargetCombatants { get; set; }
        public static bool DebugDisableGetBestTarget { get; set; }
        public static bool DebugDisableGetBestDroneTarget { get; set; }
        public static bool DebugDisableNavigateIntoRange { get; set; }
        public static bool DebugDoneAction { get; set; }
        public static bool DebugDrones { get; set; }
        public static bool DebugDroneHealth { get; set; }
        public static bool DebugEachWeaponsVolleyCache { get; set; }
        public static bool DebugEntityCache { get; set; }
        public static bool DebugExceptions { get; set; }
        public static bool DebugFittingMgr { get; set; }
        public static bool DebugFleetSupportSlave { get; set; }
        public static bool DebugFleetSupportMaster { get; set; }
        public static bool DebugGetBestTarget { get; set; }
        public static bool DebugGetBestDroneTarget { get; set; }
        public static bool DebugGotobase { get; set; }
        public static bool DebugGreyList { get; set; }
        public static bool DebugHangars { get; set; }
        public static bool DebugHasExploded { get; set; }
        public static bool DebugIdle { get; set; }
        public static bool DebugInSpace { get; set; }
        public static bool DebugInStation { get; set; }
        public static bool DebugInWarp { get; set; }
        public static bool DebugIsReadyToShoot { get; set; }
        public static bool DebugItemHangar { get; set; }
        public static bool DebugKillTargets { get; set; }
        public static bool DebugKillAction { get; set; }
        public static bool DebugLoadScripts { get; set; }
        public static bool DebugLogging { get; set; }
        public static bool DebugLootWrecks { get; set; }
        public static bool DebugLootValue { get; set; }
        public static bool DebugNavigateOnGrid { get; set; }
        public static bool DebugMiningBehavior { get; set; }
        public static bool DebugMissionFittings { get; set; }
        public static bool DebugMoveTo { get; set; }
        public static bool DebugOnframe { get; set; }
        public static bool DebugOverLoadWeapons { get; set; }
        public static bool DebugPanic { get; set; }
        public static bool DebugPerformance { get; set; }
        public static bool DebugPotentialCombatTargets { get; set; }
        public static bool DebugPreferredPrimaryWeaponTarget { get; set; }
        public static bool DebugQuestorManager { get; set; }
        public static bool DebugReloadAll { get; set; }
        public static bool DebugReloadorChangeAmmo { get; set; }
        public static bool DebugRemoteRepair { get; set; }
        public static bool DebugSalvage { get; set; }
        public static bool DebugScheduler { get; set; }
        public static bool DebugSettings { get; set; }
        public static bool DebugShipTargetValues { get; set; }
        public static bool DebugSkillTraining { get; set; }
        public static bool DebugSpeedMod { get; set; }
        public static bool DebugStatistics { get; set; }
        public static bool DebugStorylineMissions { get; set; }
        public static bool DebugTargetCombatants { get; set; }
        public static bool DebugTargetWrecks { get; set; }
        public static bool DebugTractorBeams { get; set; }
        public static bool DebugTraveler { get; set; }
        public static bool DebugUI { get; set; }
        public static bool DebugUndockBookmarks { get; set; }
        public static bool DebugUnloadLoot { get; set; }
        public static bool DebugValuedump { get; set; }
        public static bool DebugWalletBalance { get; set; }
        public static bool DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange { get; set; }
        public static bool DebugWatchForActiveWars { get; set; }
    }
}