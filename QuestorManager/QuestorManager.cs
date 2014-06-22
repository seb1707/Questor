// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace QuestorManager
{
    using System;
    using System.Windows.Forms;
    using Mono.Options;
    using Questor.Modules.Logging;

    internal static class QuestorManager
    {
    	private static bool _showHelp;
    	private static bool _standaloneInstance;
        /// <summary>
        ///   The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
        	OptionSet p = new OptionSet {
        		"Usage: QuestorManager [OPTIONS]",
        		"",
        		"",
        		"Options:",
        		{"i|standalone instance", "Standalone instance, hook D3D w/o Innerspace!", v => _standaloneInstance = v != null},
        		{"h|help", "show this message and exit", v => _showHelp = v != null}
        	};
        	
        	
        	try
        	{
        		p.Parse(args);
        	}
        	catch (OptionException ex)
        	{
        		Logging.Log("Startup", "QuestorManager: ", Logging.White);
        		Logging.Log("Startup", ex.Message, Logging.White);
        		Logging.Log("Startup", "Try `QuestorManager --help' for more information.", Logging.White);
        		return;
        	}
        	
        	if (_showHelp)
        	{
        		System.IO.StringWriter sw = new System.IO.StringWriter();
        		p.WriteOptionDescriptions(sw);
        		Logging.Log("Startup", sw.ToString(), Logging.White);
        		return;
        	}
        	
            try
            {
                Application.Run(new QuestorManagerUI(_standaloneInstance));
            }
            catch (Exception) { }
        }
    }
}