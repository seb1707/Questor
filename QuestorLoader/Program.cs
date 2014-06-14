
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
        public static string _appDomainNameToUse;
        public static string _pathToQuestorEXE;
        public EXEBootStrapper _exeBootStrapper;

        public Main(RemoteHooking.IContext InContext, string questorParameters)
        {
            RemoteHooking.WakeUpProcess(); 
        }

        public void Run(RemoteHooking.IContext InContext, string questorParameters)
        {

            Logging.Log("QuestorLauncher", "QuestorLauncher has started", Logging.White);

            OptionSet p = new OptionSet
            {
                "Usage: QuestorLauncher [OPTIONS]",
                "",
                "Options:",
                {"a|AppDomain", "the AppDomain name to use to load questor.exe into.", v => _appDomainNameToUse = v},
                {"q|PathToQuestorEXE", "the location of questor.exe", v => _pathToQuestorEXE = v},
                {"h|help", "show this message and exit", v => _showHelp = v != null}
            };

            try
            {
                Logging._QuestorParamaters = p.Parse(SplitArguments(questorParameters));
            }
            catch (OptionException ex)
            {
                Logging.Log("QuestorLauncher", "QuestorLauncher: ", Logging.White);
                Logging.Log("QuestorLauncher", ex.Message, Logging.White);
                Logging.Log("QuestorLauncher", "Try `QuestorLauncher --help' for more information.", Logging.White);
                return;
            }

            if (_showHelp)
            {
                System.IO.StringWriter sw = new System.IO.StringWriter();
                p.WriteOptionDescriptions(sw);
                Logging.Log("QuestorLauncher", sw.ToString(), Logging.White);
                return;
            }

            try
            {
                _exeBootStrapper = new EXEBootStrapper();
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
    
    

    public class EXEBootStrapper : MarshalByRefObject
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static EXEBootStrapper() //used during as the  Questor.exe entry point
        {
            try
            {
                // Create a new AppDomain (what happens if this AppDomain already exists!?!)
                System.AppDomain NewAppDomain = System.AppDomain.CreateDomain(Main._appDomainNameToUse);
                Logging.Log("EXEBootStrapper", "AppDomain [" + Main._appDomainNameToUse + "] created", Logging.White);
                // Load the assembly and call the default entry point:
                NewAppDomain.ExecuteAssembly(Main._pathToQuestorEXE);
                Logging.Log("EXEBootStrapper", "ExecuteAssembly [" + Main._pathToQuestorEXE + "] finished", Logging.White);
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
