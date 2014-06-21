
using System.CodeDom;
using System.Linq;

namespace QuestorLoader
{
    using System;
    using EasyHook;
    using Mono.Options;
    using System.Collections.Generic;

    //using System.Runtime.InteropServices;
    //using mscoree;

    public class Main : IEntryPoint
    {
        public static bool _showHelp;
        public static bool UnthawEVEProcess = false;
        public static string _appDomainNameToUse;
        public static string _pathToQuestorEXE;
        public static string[] questorLoaderArgsArray;
        public static string[] ParamatersToPassToQuestorArray;
        public static string TempArgString = string.Empty;
        public static EXEBootStrapper _exeBootStrapper;
        public static string _UseSchedule;
        public static string _EVECharacterName;
        public static string _EVELoginUserName;
        public static string _EVELoginPassword;

        public Main(RemoteHooking.IContext InContext, string[] questorLoaderParameters)
        {
            //RemoteHooking.WakeUpProcess();
        }

        public void Run(RemoteHooking.IContext InContext, string[] questorLoaderParameters)
        {
            Logging.Log("QuestorLauncher", "QuestorLauncher has started", Logging.White);

            //questorLoaderArgsArray = SplitArguments(questorLoaderParameters).ToArray();

            int i = 0;
            foreach (var arg in questorLoaderArgsArray)
            {
                if (questorLoaderArgsArray[i].ToLower() == "-PathToQuestorEXE".ToLower())
                {
                    _pathToQuestorEXE = questorLoaderArgsArray[i + 1];
                }

                if (questorLoaderArgsArray[i].ToLower() == "-AppDomain".ToLower())
                {
                    _appDomainNameToUse = questorLoaderArgsArray[i + 1];
                }

                //if (questorLoaderArgsArray[i].ToLower() == "-x".ToLower())
                //{
                //    _UseSchedule = questorLoaderArgsArray[i];
                //}

                //if (questorLoaderArgsArray[i].ToLower() == "-c".ToLower())
                //{
                //    _EVECharacterName = questorLoaderArgsArray[i + 1];
                //}

                //if (questorLoaderArgsArray[i].ToLower() == "-u".ToLower())
                //{
                //    _EVELoginUserName = questorLoaderArgsArray[i + 1];
                //}

                //if (questorLoaderArgsArray[i].ToLower() == "-p".ToLower())
                //{
                //    _EVELoginPassword = questorLoaderArgsArray[i + 1];
                //}

                if (arg == questorLoaderArgsArray[0]) continue;
                if (arg == questorLoaderArgsArray[1]) continue;
                if (arg == questorLoaderArgsArray[2]) continue;
                if (arg == questorLoaderArgsArray[3]) continue;
                Console.WriteLine("QuestorLoader Parameters we were passed [" + i + "] - [" + arg + "] \r\n");
                TempArgString = TempArgString + " " + arg;
                i++;
            }

            //if (!String.IsNullOrEmpty(_EVELoginUserName)) TempArgList.Add("-u " +  _EVELoginUserName + " ");
            //if (!String.IsNullOrEmpty(_EVELoginPassword)) TempArgList.Add(_EVELoginPassword);
            //if (!String.IsNullOrEmpty(_UseSchedule)) TempArgList.Add("-x");
            //if (_EVECharacterName != null) 
            

            EXEBootstrapper_StartQuestor();

            while (!UnthawEVEProcess)
            {
                System.Threading.Thread.Sleep(1000);
            }

            RemoteHooking.WakeUpProcess();
            
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static void EXEBootstrapper_StartQuestor()
        {
            try
            {
                _exeBootStrapper = new EXEBootStrapper();
                //EXEBootStrapper.StartQuestor(new string[] { TempArgString });
                EXEBootStrapper.StartQuestor(questorLoaderArgsArray);
            }
            catch (Exception ex)
            {
                Logging.Log("QuestorLauncher", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                //while (true)
                //{
                //    Thread.Sleep(50);
                //}
                Logging.Log("QuestorLauncher", "done", Logging.White);
            }
        }
        
        public static IEnumerable<string> SplitArguments(string commandLine)
        {
            try
            {
                char[] parmChars = commandLine.ToCharArray();
                bool inSingleQuote = false;
                bool inDoubleQuote = false;
                for (int index = 0; index < parmChars.Length; index++)
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
            catch (Exception exception)
            {
                Console.WriteLine("Exception [" + exception + "]");
                return null;
            }
        }
    }
    
    

    public class EXEBootStrapper : MarshalByRefObject
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static EXEBootStrapper() //used during as the  Questor.exe entry point
        {
            
        }

        public static void StartQuestor(string[] args)
        {
            try
            {
                Console.WriteLine("------------------------------------------------------] \r\n");
                Console.WriteLine("------------------------------------------------------] \r\n");
                Console.WriteLine("Main._pathToQuestorEXE [" + Main._pathToQuestorEXE + "] \r\n");
                Console.WriteLine("------------------------------------------------------] \r\n");
                Console.WriteLine("------------------------------------------------------] \r\n");
                
                // Create a new AppDomain (what happens if this AppDomain already exists!?!)
                System.AppDomain NewAppDomain = System.AppDomain.CreateDomain(Main._appDomainNameToUse);
                Logging.Log("EXEBootStrapper", "AppDomain [" + Main._appDomainNameToUse + "] created", Logging.White);
                // Load the assembly and call the default entry point:
                NewAppDomain.ExecuteAssembly(Main._pathToQuestorEXE, Main.questorLoaderArgsArray);
                Logging.Log("EXEBootStrapper", "ExecuteAssembly [" + Main._pathToQuestorEXE + "] finished", Logging.White);
                //Main.UnthawEVEProcess = true;
            }
            catch (Exception ex)
            {
                Logging.Log("EXEBootStrapper", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                Logging.Log("EXEBootStrapper", "done.", Logging.White);
            }
        }

        //
        //https://stackoverflow.com/questions/388554/list-appdomains-in-process
        //
        //Remember to reference COM object \WINDOWS\Microsoft.NET\Framework\vXXX\mscoree.tlb, set reference mscoree "Embed Interop Types" as "False".

        //foreach (AppDomain appDomain in EnumAppDomains())
        //{
        //    // use appDomain
        //}
        /*
        public static IEnumerable<AppDomain> EnumAppDomains()
        {
            IList<AppDomain> appDomains = new List<AppDomain>();
            IntPtr enumHandle = IntPtr.Zero;
            ICorRuntimeHost host = null;

            try
            {
                host = new CorRuntimeHostClass();
                host.EnumDomains(out enumHandle);
                object domain = null;

                do
                {
                    host.NextDomain(enumHandle, out domain);
                    if (domain != null)
                    {
                        yield return (AppDomain)domain;
                    }
                }
                while (domain != null);
            }
            finally
            {
                if (host != null)
                {
                    if (enumHandle != IntPtr.Zero)
                    {
                        host.CloseEnum(enumHandle);
                    }

                    Marshal.ReleaseComObject(host);
                }
            }
        }
        */
    }
}
