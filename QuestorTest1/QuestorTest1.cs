// ------------------------------------------------------------------------------
// <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
// Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
// Please look in the accompanying license.htm file for the license that
// applies to this source code. (a copy can also be found at:
// http://www.thehackerwithin.com/license.htm)
// </copyright>
// -------------------------------------------------------------------------------


/*
namespace QuestorTest
{
    using System;
    using Questor.Modules.Logging;
    
    public class QuestorTest : IEntryPoint
    {
        //private Timer dispatcherTimer;

        public static void Main(EasyHook.RemoteHooking.IContext InContext, string args)
        {

        }
        //public void Run(EasyHook.RemoteHooking.IContext InContext, string args) //used during as the  Questor.dll entry point
        //{
        //    try
        //    {
        //        //IEnumerable<string> questorParametersfromLauncher = args.Cast<string>();
        //        //Program_Start(questorParametersfromLauncher);
        //        Logging.Log("Startup", "Starting...", Logging.White);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.Log("Startup", "exception [" + ex + "]", Logging.White);
        //    }
        //
        //    Logging.Log("Startup", "Done.", Logging.White);
        //    while (true)
        //        Thread.Sleep(50);
        //}

        public void Run(EasyHook.RemoteHooking.IContext InContext, string args)
        {
            //dispatcherTimer = new System.Timers.Timer(20000);
            //dispatcherTimer.Elapsed += new System.Timers.ElapsedEventHandler(HookOnframe);
            //dispatcherTimer.Start();

            RemoteHooking.WakeUpProcess();

            try
            {
                //IEnumerable<string> questorParametersfromLauncher = args.Cast<string>();
                //Program_Start(questorParametersfromLauncher);
                Logging.Log("Startup", "Starting...", Logging.White);
            }
            catch (Exception ex)
            {
                Logging.Log("Startup", "exception [" + ex + "]", Logging.White);
            }
            
            Logging.Log("Startup", "Done.", Logging.White);
            while (true)
            {
                Thread.Sleep(50);
            }
            
            Cache.Instance.DirectEve.Dispose();
        }
    } 
}
*/