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
    using System.Net.Mail;
    using System.Net;
    using Questor.Modules.Caching;
    using Questor.Modules.Lookup;
    using Questor.Modules.Logging;
    
    public static class Email
    {
        public static DateTime DateTimeForLogs;
        private static bool ShipLostEmailSent;
        private static bool QuestorRestartedEmailSent;

        public static void Email_ShipLost()
        {
            try
            {
                string _locationName;
                if (Cache.Instance.InStation)
                {
                    _locationName = Cache.Instance.DirectEve.GetLocationName(Cache.Instance.DirectEve.Session.StationId ?? 0);
                }
                else
                {
                    long mySolarSystemlocation = Cache.Instance.ActiveShip.LocationId;
                    _locationName = Cache.Instance.DirectEve.Navigation.GetLocationName(mySolarSystemlocation);
                }
                
                if (!ShipLostEmailSent)
                {
                    string subject = "ShipLostEmail: [" + Settings.CharacterName + "] is in a pod";
                    string body = "ShipLostEmail: [" + Settings.CharacterName + "] is in a pod in [" + _locationName + "]";
                    if (SendEmail(subject, body))
                    {
                        ShipLostEmailSent = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Email_ShipLost", "Exception was [" + exception + "]", Logging.Debug);
            }
        }

        public static void Email_QuestorRestarted()
        {
            if (!QuestorRestartedEmailSent)
            {
                const string subject = "subject test";
                const string body = "body test";
                if (SendEmail(subject, body))
                {
                    QuestorRestartedEmailSent = true;
                }
            }
        }

        public static bool SendEmail(string subject, string body)
        {
            if (Settings.EmailSupport)
            {
                bool _useSSL;
                if (Settings.EmailEnableSSL != null)
                {
                    _useSSL = (bool)Settings.EmailEnableSSL;
                }
                else
                {
                    _useSSL = false;
                }

                if (!String.IsNullOrEmpty(Settings.EmailAddress) &&
                    !String.IsNullOrEmpty(Settings.EmailPassword) &&
                    !String.IsNullOrEmpty(Settings.EmailSMTPServer) &&
                    !String.IsNullOrEmpty(Settings.EmailAddressToSendAlerts)
                    )
                {
                    try
                    {
                        SmtpClient smtpClient = new SmtpClient(Settings.EmailSMTPServer, Settings.EmailSMTPPort) //587
                        {
                            Credentials = new NetworkCredential(Settings.EmailAddress, Settings.EmailPassword),
                            EnableSsl = _useSSL
                        };
                        smtpClient.Send(Settings.EmailAddress, Settings.EmailAddressToSendAlerts, subject, body);
                        Logging.Log("Email.SendEmail", "Sent Email to [" + Settings.EmailAddressToSendAlerts + "]", Logging.Debug);
                    }
                    catch (Exception exception)
                    {
                        Logging.Log("Email.SendEmail", "Sending Email caused an exception [" + exception + "]",Logging.Debug);
                        return false;
                    }
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}