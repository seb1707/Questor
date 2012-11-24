// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Alerts
{
    using System;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net.Mail;
    using System.Net;
    using Questor.Modules.Caching;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;
    using InnerSpaceAPI;
    using LavishScriptAPI;

    public static class Email
    {
        public static DateTime DateTimeForLogs;

        public static void SendEmail(string subject, string body)
        {
            if (Settings.Instance.EmailSupport)
            {
                bool _useSSL;
                if (Settings.Instance.EmailEnableSSL != null)
                {
                    _useSSL = (bool)Settings.Instance.EmailEnableSSL;
                }
                else
                {
                    _useSSL = false;
                }

                if (!String.IsNullOrEmpty(Settings.Instance.EmailAddress) &&
                    !String.IsNullOrEmpty(Settings.Instance.EmailPassword) &&
                    !String.IsNullOrEmpty(Settings.Instance.EmailSMTPServer) &&
                    !String.IsNullOrEmpty(Settings.Instance.EmailAddressToSendAlerts)
                    )
                {
                    var client = new SmtpClient(Settings.Instance.EmailSMTPServer, Settings.Instance.EmailSMTPPort) //587
                    {
                        Credentials = new NetworkCredential(Settings.Instance.EmailAddress, Settings.Instance.EmailPassword),
                        EnableSsl = _useSSL
                    };
                    client.Send(Settings.Instance.EmailAddress, Settings.Instance.EmailAddressToSendAlerts, subject, body);
                    Console.WriteLine("Sent Email to [" + Settings.Instance.EmailAddressToSendAlerts + "]");
                    Console.ReadLine();
                }    
            }
        }
    }
}