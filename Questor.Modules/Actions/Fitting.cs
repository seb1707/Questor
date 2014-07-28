// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Actions
{
    using System.Xml.Linq;
    using Questor.Modules.Logging;
    using System;

    public class FactionFitting
    {
        public FactionFitting()
        {
        }

        public FactionFitting(XElement factionfitting)
        {
            try
            {
                FactionName = (string)factionfitting.Attribute("faction") ?? "";
                FittingName = (string)factionfitting.Attribute("fitting") ?? "default";
                DroneTypeID = (int?)factionfitting.Attribute("dronetype") ??
                              (int?)factionfitting.Attribute("drone") ??
                              (int?)factionfitting.Attribute("dronetype") ??
                              null;
            }
            catch (Exception exception)
            {
                Logging.Log("FactionFitting", "Exception: [" + exception + "]", Logging.Red);
            }
        }

        public string FactionName { get; private set; }

        public string FittingName { get; private set; }

        public int? DroneTypeID { get; private set; }
    }

    public class MissionFitting
    {
        public MissionFitting()
        {
        }

        public MissionFitting(XElement missionfitting)
        {
            try
            {
                Mission = (string)missionfitting.Attribute("mission") ?? "";
                Faction = (string)missionfitting.Attribute("faction") ?? "Default";
                Fitting = (string)missionfitting.Attribute("fitting") ?? "";
                Ship = (string)missionfitting.Attribute("ship") ?? "";
                DroneTypeID = (int?) missionfitting.Attribute("droneTypeID") ??
                              (int?) missionfitting.Attribute("drone") ??
                              (int?) missionfitting.Attribute("dronetype") ??
                              null;
            }
            catch (Exception exception)
            {
                Logging.Log("MissionFitting", "Exception: [" + exception + "]", Logging.Red);
            }
        }

        public string Mission { get; private set; }

        public string Faction { get; private set; }

        public string Fitting { get; private set; }

        public string Ship { get; private set; }

        public int? DroneTypeID { get; private set; }
    }
}