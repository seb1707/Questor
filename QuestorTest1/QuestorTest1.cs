// ------------------------------------------------------------------------------
// <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
// Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
// Please look in the accompanying license.htm file for the license that
// applies to this source code. (a copy can also be found at:
// http://www.thehackerwithin.com/license.htm)
// </copyright>
// -------------------------------------------------------------------------------

using Questor.Modules.Misc;

namespace QuestorTest1
{
    using System;
    using System.Threading;
    using DirectEve;
    using Questor.Modules.Logging;
    using Questor.Modules.Caching;
    using Questor.Modules.BackgroundTasks;

    internal class QuestorTest1
    {
        public static DateTime DateTimeForLogs;
        private const int WaitMillis = 3500;
        private static long _lastLoyaltyPoints;
        private static DateTime _nextAction;
        private static DateTime _loyaltyPointTimeout;
        private static string _type;
        private static int? _quantity;
        private static int? _totalQuantityOfOrders;
        private static DateTime _done = DateTime.UtcNow.AddDays(10);
        private static DateTime _lastPulse;
        private static Cleanup _cleanup;

        private static void Main(string[] args)
        {
            DateTimeForLogs = DateTime.Now;
            //Logging.Log("BuyLPI", "BuyLPI: Test", Logging.White);
            //InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "BuyLPI: Test2"));
            
            if (args.Length == 0)
            {
                ////InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "BuyLPI: 0 arguments"));
                //Logging.Log("QuestorTest1", "Syntax:", Logging.White);
                //Logging.Log("QuestorTest1", "DotNet BuyLPI BuyLPI <TypeName or TypeId> [Quantity]", Logging.White);
                //Logging.Log("QuestorTest1", "(Quantity is optional)", Logging.White);
                //Logging.Log("QuestorTest1", "", Logging.White);
                //Logging.Log("QuestorTest1", "Example:", Logging.White);
                //Logging.Log("QuestorTest1", "DotNet BuyLPI BuyLPI \"Caldari Navy Mjolnir Torpedo\" 10", Logging.White);
                //Logging.Log("QuestorTest1", "*OR*", Logging.White);
                //Logging.Log("QuestorTest1", "DotNet BuyLPI BuyLPI 27339 10", Logging.White);
                //return;
            }

            if (args.Length >= 1)
            {
                //_type = args[0];
            }

            if (args.Length >= 2)
            {
                //int dummy;
                //if (!int.TryParse(args[1], out dummy))
                //{
                //    Logging.Log("QuestorTest1", "Quantity must be an integer, 0 - " + int.MaxValue, Logging.White);
                //    return;
                //}
                //
                //if (dummy < 0)
                //{
                //    Logging.Log("QuestorTest1", "Quantity must be a positive number", Logging.White);
                //    return;
                //}
                //
                //_quantity = dummy;
                //_totalQuantityOfOrders = dummy;
            }

            Cache.Instance.QuestorStarted_DateTime = DateTime.UtcNow;

            InnerspaceCommands.CreateLavishCommands();
            //InnerspaceEvents.CreateLavishEvents();

            //InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "Starting BuyLPI... - innerspace Echo"));
            Logging.Log("QuestorTest1", "Starting QuestorTest1...", Logging.White);
            _cleanup = new Cleanup();
            Cache.Instance.DirectEve = new DirectEve();
            Cache.Instance.DirectEve.OnFrame += OnFrame;

            // Sleep until we're done
            while (_done.AddSeconds(5) > DateTime.UtcNow)
            {
                Thread.Sleep(50);
            }

            Cache.Instance.DirectEve.Dispose();
            Logging.Log("QuestorTest1", "QuestorTest1 finished.", Logging.White);
            //InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTimeForLogs, "BuyLPI: Finished 2"));
        }

        private static void OnFrame(object sender, EventArgs eventArgs)
        {
            // New frame, invalidate old cache
            Cache.Instance.InvalidateCache();

            Cache.Instance.LastFrame = DateTime.UtcNow;

            // Only pulse state changes every 1.5s
            if (DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < 3000)
            {
                return;
            }

            _lastPulse = DateTime.UtcNow;
            
            // Done
            // Cleanup State: ProcessState

            if (DateTime.UtcNow > _done)
                return;

            // Wait for the next action
            if (_nextAction >= DateTime.UtcNow)
            {
                return;
            }
            

            //
            // ......
            //
            Logging.Log("QuestorTest1", "QuestorTest1 [" + DateTime.UtcNow.Subtract(Cache.Instance.QuestorStarted_DateTime).TotalSeconds + "] sec since we started. ", Logging.White);

            return;
        }
    }
}