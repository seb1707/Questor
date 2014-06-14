// ------------------------------------------------------------------------------
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
    using System.Threading;
    using InnerSpaceAPI;
    
    public static class Logging
    {
        public static int LoggingInstances = 0;

        static Logging()
        {
            Interlocked.Increment(ref LoggingInstances);
        }

        //~Logging()
        //{
        //    Interlocked.Decrement(ref LoggingInstances);
        //}

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

        public const string DebugHangars = White;

        public static string _username;
        public static string _password;
        public static string _character;
        public static bool standaloneInstance;
        public static bool tryToLogToFile;
        public static List<string> _QuestorParamaters;

        //public  void Log(string line)
        //public static void Log(string module, string line, string color = Logging.White)
        public static void Log(string module, string line, string color, bool verbose = false)
        {
            DateTimeForLogs = DateTime.Now;
            //colorLogLine contains color and is for the InnerSpace console
            string colorLogLine = line;

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
            //Cache.Instance.ConsoleLogRedacted = String.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "[" + module + "] " + FilterSensitiveInfo(plainLogLine) + "\r\n");  //In memory Console Log with sensitive info redacted
            plainLogLine = FilterColorsFromLogs(line);
            if (Logging.standaloneInstance)
            {
                Console.WriteLine(plainLogLine);    
            }
        }

        //path = path.Replace(Environment.CommandLine, "");
        //path = path.Replace(Environment.GetCommandLineArgs(), "");

        
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
    }
}