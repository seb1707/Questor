
namespace QuestorDLL
{
    using System;
    using EasyHook;
    using System.Threading;
    using System.Collections.Generic;
    using System.Linq;
    using Questor;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;

    public class Main : IEntryPoint
    {
        //private static BeforeLogin _beforeLogin;
        public Main(RemoteHooking.IContext InContext, string questorParameters)
        {
            RemoteHooking.WakeUpProcess(); 
        }

        public void Run(RemoteHooking.IContext InContext, string questorParameters)
        {

            Console.WriteLine("QuestorDLL.dll: QuestorDLL.dll is running");

            try
            {
                //_beforeLogin = new BeforeLogin();
                IEnumerable<string> questorParametersfromLauncher = null; //= args.Cast<string>();
                //BeforeLogin.Program_Start(questorParametersfromLauncher);
            }
            catch (Exception ex)
            {
                Logging.Log("Startup", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                Logging.Log("Startup", "done", Logging.White);
            }

            while (true)
                Thread.Sleep(50);

            Console.WriteLine("Start: we will never get here... ");
        }   
    }
    
    /*
    public class EXEBootStrapper
    {
        private static BeforeLogin _beforeLogin;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) //used during as the  Questor.exe entry point
        {
            try
            {
                _beforeLogin = new BeforeLogin();
                IEnumerable<string> questorParametersfromLauncher = args.Cast<string>();
                BeforeLogin.Program_Start(questorParametersfromLauncher);
            }
            catch (Exception ex)
            {
                Logging.Log("Startup", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                Logging.Log("Startup", "done", Logging.White);
            }
        }
    }
    */
}
