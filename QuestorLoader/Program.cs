
namespace QuestorLoader
{
    using System;
    using System.IO;
    using System.Linq;
    using EasyHook;
    using System.Collections.Generic;

    
    public class Main : IEntryPoint
    {
        //public static bool UnthawEVEProcess = false;
        public static string _appDomainNameToUse;
        public static string _pathToQuestorEXE;
        public static bool _RestartQuestorIfClosed;
        public static bool DebugAppDomains;
        public static EXEBootStrapper _exeBootStrapper;
        public static string QuestorDLLSettingsINI;
        public static DateTime QuestorLoader_Started;
        
        public Main(RemoteHooking.IContext InContext, string questorLoaderParameters)
        {
            //RemoteHooking.WakeUpProcess();
        }


        public void Run(RemoteHooking.IContext InContext,string questorLoaderParameters)
        {
            Logging.Log("QuestorLauncher", "QuestorLauncher has started", Logging.White);

            
            int i = 0;
            Console.WriteLine("QuestorLoader Parameters we were passed [" + i + "] - [" + questorLoaderParameters + "] \r\n");
            
            if (PrepareToLoadPreLoginSettingsFromINI(questorLoaderParameters))
            {
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
            else
            {
                Console.WriteLine("QuestorLoader: unable to load settings from ini, halting\r\n");
            }

            Console.WriteLine("QuestorLoader: done.\r\n");
        }

        private static bool PrepareToLoadPreLoginSettingsFromINI(string arg)
        {
            //
            // Load PathToQuestorEXE and AppDomainToCreateForQuestor from an ini
            //
            if (arg.ToLower().EndsWith(".ini"))
            {
                QuestorDLLSettingsINI = System.IO.Path.Combine(Directory.GetCurrentDirectory(), arg);
                
                if (!string.IsNullOrEmpty(QuestorDLLSettingsINI) && File.Exists(QuestorDLLSettingsINI))
                {
                    Logging.Log("QuestorLoader", "Found [" + QuestorDLLSettingsINI + "] loading Questor PreLogin Settings", Logging.White);
                    if (!PreLoginSettings(QuestorDLLSettingsINI))
                    {
                        Logging.Log("QuestorLoader", "Failed to load PreLogin settings from [" + QuestorDLLSettingsINI + "]", Logging.Debug);
                        return false;
                    }

                    Logging.Log("QuestorLoader", "_pathToQuestorEXE is [" + _pathToQuestorEXE + "]", Logging.Debug);
                    Logging.Log("QuestorLoader", "_appDomainNameToUse is [" + _appDomainNameToUse + "]", Logging.Debug);
                    return true;
                }

                return false;
            }

            return false;
        }

        public static void EXEBootstrapper_StartQuestor()
        {
            try
            {
                _exeBootStrapper = new EXEBootStrapper();
                EXEBootStrapper.StartQuestor(); 
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

        public static bool PreLoginSettings(string iniFile)
        {
            try
            {
                if (!File.Exists(iniFile))
                {
                    Logging.Log("PreLoginSettings", "Could not find a file named [" + iniFile + "]", Logging.Debug);
                    return false;
                }

                //foreach (string line in File.ReadAllLines(iniFile))
                //{
                //    Logging.Log("PreLoginSettings", "Contents of INI [" + line + "]", Logging.Debug);
                //}

                int index = 0;
                foreach (string line in File.ReadAllLines(iniFile))
                {    
                    index++;
                    if (line.StartsWith(";"))
                        continue;

                    if (line.StartsWith("["))
                        continue;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (string.IsNullOrEmpty(line))
                        continue;

                    //Logging.Log("PreLoginSettings", "Contents of INI Lines we Process [" + line + "]", Logging.Debug);

                    string[] sLine = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    //if (sLine.Count() != 2 && !sLine[0].Equals(ProxyUsername) && !sLine[0].Equals(ProxyPassword) )
                    if (sLine.Count() != 2)
                    {
                        Logging.Log("PreLoginSettings", "IniFile not right format at line: [" + index + "]", Logging.Debug);
                    }

                    //Logging.Log("PreLoginSettings", "Contents of INI Values we Processed [" + sLine[1] + "]", Logging.Debug);
                    switch (sLine[0].ToLower())
                    {
                        case "pathtoquestorexe":
                            _pathToQuestorEXE = sLine[1];
                            break;

                        case "appdomaintocreateforquestor":
                            _appDomainNameToUse = sLine[1];
                            break;

                        case "restartquestorifclosed":
                            _RestartQuestorIfClosed = Boolean.Parse(sLine[1]);
                            break;

                        case "debugappdomains":
                            DebugAppDomains = Boolean.Parse(sLine[1]);
                            break;
                    }
                }

                if (_pathToQuestorEXE == null || string.IsNullOrEmpty(_pathToQuestorEXE))
                {
                    Logging.Log("PreLoginSettings", "Missing: PathToQuestorEXE in [" + iniFile + "]: We cannot launch EVE if we do not know where it is. Ex. PathToQuestorEXE=c:\\eveoffline\\DotNetPrograms\\Questor.exe", Logging.Debug);
                }

                if (_appDomainNameToUse == null || string.IsNullOrEmpty(_appDomainNameToUse))
                {
                    _appDomainNameToUse = "q1";
                }

                return true;
            }
            catch (Exception exception)
            {
                Logging.Log("QuestorLoader", "Exception [" + exception + "]", Logging.Debug);
                return false;
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

        public static void StartQuestor()
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
                NewAppDomain.ExecuteAssembly(Main._pathToQuestorEXE, new string[] { Main.QuestorDLLSettingsINI });
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
