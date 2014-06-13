namespace Questor.Modules.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using Questor.Modules.Logging;

    public class AgentsList
    {
        private sealed class AgentsDeclineTimers
        {
            private static readonly Lazy<AgentsDeclineTimers> lazy = new Lazy<AgentsDeclineTimers>(() => new AgentsDeclineTimers());
            public static AgentsDeclineTimers Instance { get { return lazy.Value; } }

            private Dictionary<string,DateTime> _timers;
            private string _cacheFilePath;

            private AgentsDeclineTimers()
            {
                _cacheFilePath = Settings.Instance.CachePath + "agents_decline_times.csv";

                _loadFromCacheFile();
            }

            private void _loadFromCacheFile()
            {
                _timers = new Dictionary<string, DateTime>();

                if (File.Exists(_cacheFilePath))
                {
                    Logging.Log("AgentsDeclineTimes", String.Format("Loading agents decline times from cache file : {0}", _cacheFilePath), Logging.White);

                    System.IO.StreamReader file = new System.IO.StreamReader(_cacheFilePath);

                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] lineValues = line.Split(';');

                        string agentName = lineValues[0];
                        DateTime declineTime = new DateTime(long.Parse(lineValues[1]));

                        _timers.Add(agentName, declineTime);

                        Logging.Log("AgentsDeclineTimes", String.Format("Found agent decline time in cache file : {0}, {1}", agentName, declineTime.ToString()), Logging.White);
                    }
                }
                else
                {
                    Logging.Log("AgentsDeclineTimes", String.Format("No decline times loaded because cache file does not exist : {0}", _cacheFilePath), Logging.White);
                }
            }

            private void _writeToCacheFile()
            {
                Logging.Log("AgentsDeclineTimes", String.Format("Writing agents decline times to cache file : {0}", _cacheFilePath), Logging.White);

                System.IO.StreamWriter file = new System.IO.StreamWriter(_cacheFilePath);

                foreach (var entry in _timers)
                    file.WriteLine("{0};{1};{2};{3}", entry.Key, entry.Value.Ticks, entry.Value.ToString(), DateTime.UtcNow.ToString()); 

                file.Close();
            }
                        
            public DateTime getDeclineTimer(string agentName)
            {
                DateTime declineTimer;
                if(!_timers.TryGetValue(agentName, out declineTimer))
                {
                    declineTimer = DateTime.UtcNow;
                }

                return declineTimer;
            }

            public void setDeclineTimer(string agentName, DateTime declineTimer)
            {
                if (_timers.ContainsKey(agentName))
                    _timers["agentName"] = declineTimer;
                else
                    _timers.Add(agentName, declineTimer);

                _writeToCacheFile();
            }
        }

        public AgentsList()
        {
        }

        public AgentsList(XElement agentList)
        {
            Name = (string)agentList.Attribute("name") ?? "";
            Priorit = (int)agentList.Attribute("priority");
        }

        public string Name { get; private set; }

        public int Priorit { get; private set; }

        public DateTime DeclineTimer
        {
            get
            {
                return AgentsDeclineTimers.Instance.getDeclineTimer(Name);
            }
            set
            {
                AgentsDeclineTimers.Instance.setDeclineTimer(Name, value);
            }
        }
    }
}