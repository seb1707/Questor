//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that
//    applies to this source code. (a copy can also be found at:
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Questor.Modules.Logging;

namespace Questor
{
    using System;
    using EasyHook;
    using System.Threading;

    /*
    public class Main : EasyHook.IEntryPoint
    {
        [STAThread]
        public Main(RemoteHooking.IContext InContext, string questorParameters)
        {
            // todo: add initialization code here.
            RemoteHooking.WakeUpProcess();
        }

        public void Run(RemoteHooking.IContext InContext, string questorParameters)
        {

            Console.WriteLine("QuestorDLL.dll: QuestorDLL.dll is running");

            while (true)
                Thread.Sleep(50);

            Console.WriteLine("Start: we will never get here... ");
        }
    }
    */

    public class Main : IEntryPoint
    {
        private BeforeLogin _beforeLogin;

        public Main(RemoteHooking.IContext InContext, string questorParameters)
        {
            try
            {
                Logging.Log("Startup", "Initialization Code Goes Here", Logging.White);
            }
            catch (Exception ex)
            {
                Logging.Log("Startup", "Exception [" + ex + "]", Logging.White);
            }
            finally
            {
                RemoteHooking.WakeUpProcess();     
            }
        }
        
        public void Run(RemoteHooking.IContext InContext, string questorParameters)
        {
            Logging.Log("Startup", "QuestorDLL.dll: QuestorDLL.dll is running", Logging.White);

            try
            {
                //RemoteHooking.WakeUpProcess();
                _beforeLogin = new BeforeLogin();
                IEnumerable<string> questorParametersfromLauncher = SplitArguments(questorParameters);
                BeforeLogin.Program_Start(questorParametersfromLauncher);

                while (true)
                {
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Startup", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                Logging.Log("Startup", "done.", Logging.White);
            }
        }

        public static IEnumerable<string> SplitArguments(string commandLine)
        {
            var parmChars = commandLine.ToCharArray();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            for (var index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    parmChars[index] = '\n';
                }
                if (parmChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    parmChars[index] = '\n';
                }
                if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
