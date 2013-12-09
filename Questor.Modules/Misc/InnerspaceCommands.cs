
using System;
using System.Collections.Generic;
using System.Linq;
using InnerSpaceAPI;

namespace Questor.Modules.Misc
{
    using Questor.Modules.Caching;
    using Questor.Modules.Logging;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;
    using LavishScriptAPI;

    public class InnerspaceCommands
    {
        //public InnerspaceCommands() { }

        #region Create Innerspace Commands
        public static void CreateLavishCommands()
        {
            if (Settings.Instance.UseInnerspace)
            {
                //
                // Slaves To Master Requests
                //
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatIsLocationIDofMaster", SlaveToMaster_WhatIsLocationIDofMaster_InnerspaceCommand);           //Master should reply: MasterToSlaves_SetDestinationLocationID
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatIsCoordofMaster", SlaveToMaster_WhatIsCoordofMaster_InnerspaceCommand);                     //Master should reply: MasterToSlaves_MasterCoordinatesAre_InnerspaceCommand
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatMissionIsCurrentMissionAction", SlaveToMaster_WhatIsCurrentMissionAction_InnerspaceCommand);//Master should reply: MasterToSlaves_DoThisMissionAction_InnerspaceCommand
                LavishScript.Commands.AddCommand("SlaveToMaster_WhatAmmoShouldILoad", SlaveToMaster_WhatAmmoShouldILoad_InnerspaceCommand);                     //Master should reply: 

                //
                // Master To Slaves Requests
                //
                LavishScript.Commands.AddCommand("MasterToSlaves_SetDestinationLocationID", MasterToSlaves_SetDestinationLocationID_InnerspaceCommand);         //answer to: SlaveToMaster_WhatIsLocationIDofMaster
                LavishScript.Commands.AddCommand("MasterToSlaves_MasterCoordinatesAre", MasterToSlaves_MasterCoordinatesAre_InnerspaceCommand);                 //answer to: SlaveToMaster_WhatIsCoordofMaster_InnerspaceCommand
                LavishScript.Commands.AddCommand("MasterToSlaves_DoThisMissionAction", MasterToSlaves_DoThisMissionAction_InnerspaceCommand);                   //answer to: SlaveToMaster_WhatMissionIsCurrentMissionAction
                LavishScript.Commands.AddCommand("MasterToSlaves_MasterIsWarpingTo", MasterToSlaves_MasterIsWarpingTo_InnerspaceCommand);                       //needs no response
                LavishScript.Commands.AddCommand("MasterToSlaves_SlavesGotoBase", MasterToSlaves_SlavesGotoBase_InnerspaceCommand);                             //needs no response
                LavishScript.Commands.AddCommand("MasterToSlaves_DoNotLootItemName", MasterToSlaves_DoNotLootItemName_InnerspaceCommand);                       //needs no response
                LavishScript.Commands.AddCommand("MasterToSlaves_SetAutoStart", MasterToSlaves_SetAutoStart_InnerspaceCommand);                                 //needs no response
                LavishScript.Commands.AddCommand("MasterToSlaves_WhereAreYou", MasterToSlaves_WhereAreYou_InnerspaceCommand);                                   //
                LavishScript.Commands.AddCommand("MasterToSlaves_WhatAreYouShooting", MasterToSlaves_WhatAreYouShooting_InnerspaceCommand);                     //
                LavishScript.Commands.AddCommand("MasterToSlaves_ShootMyTarget", MasterToSlaves_ShootThisEntityID_InnerspaceCommand);                           //needs no response
                
                //LavishScript.Commands.AddCommand("MastersMissionXMLActions", MastersMissionXMLActionsInnerspaceCommand);
                //LavishScript.Commands.AddCommand("RemoteRepairShields", RemoteRepairShieldsInnerspaceCommand);
                //LavishScript.Commands.AddCommand("RemoteArmorRepair", RemoteArmorRepairInnerspaceCommand);

                //Autostart on/off
                LavishScript.Commands.AddCommand("SetAutoStart", SetAutoStart);
                LavishScript.Commands.AddCommand("AutoStart", SetAutoStart);
                //Direct3d on/off
                LavishScript.Commands.AddCommand("SetDisable3D", SetDisable3D);
                LavishScript.Commands.AddCommand("Disable3D", SetDisable3D);
                //GotoBase
                LavishScript.Commands.AddCommand("SetCombatMissionsBehaviorStatetoGotoBase", SetCombatMissionsBehaviorStatetoGotoBase);
                LavishScript.Commands.AddCommand("GotoBase", SetCombatMissionsBehaviorStatetoGotoBase);
                //misc other commands
                LavishScript.Commands.AddCommand("SetQuestorStatetoIdle", SetQuestorStatetoIdle);
                LavishScript.Commands.AddCommand("Idle", SetQuestorStatetoIdle);
                LavishScript.Commands.AddCommand("SetExitWhenIdle", SetExitWhenIdle);
                LavishScript.Commands.AddCommand("SetQuestorStatetoCloseQuestor", SetQuestorStatetoCloseQuestor);
                LavishScript.Commands.AddCommand("SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase", SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase);
                LavishScript.Commands.AddCommand("QuestorEvents", ListQuestorEvents);
                LavishScript.Commands.AddCommand("IfInPodSwitchToNoobShiporShuttle", IfInPodSwitchToNoobShiporShuttle);
                LavishScript.Commands.AddCommand("SetDestToSystem", SetDestToSystem);
                //LavishScript.Commands.AddCommand("FindEntities", FindEntitiesNamed);
                LavishScript.Commands.AddCommand("ListEntitiesThatHaveUsLocked", ListEntitiesThatHaveUsLockedInnerspaceCommand);
                LavishScript.Commands.AddCommand("ListAllEntities", ListAllEntities);
                LavishScript.Commands.AddCommand("ListEntities", ListAllEntities);
                LavishScript.Commands.AddCommand("Entities", ListAllEntities);
                LavishScript.Commands.AddCommand("ListPotentialCombatTargets", ListPotentialCombatTargets);
                LavishScript.Commands.AddCommand("ListHighValueTargets", ListHighValueTargets);
                LavishScript.Commands.AddCommand("ListLowValueTargets", ListLowValueTargets);
                LavishScript.Commands.AddCommand("ModuleInfo", ModuleInfo);
                LavishScript.Commands.AddCommand("ListModules", ModuleInfo);
                LavishScript.Commands.AddCommand("ListIgnoredTargets", ListIgnoredTargets);
                LavishScript.Commands.AddCommand("ListPrimaryWeaponPriorityTargets", ListPrimaryWeaponPriorityTargetsInnerspaceCommand);
                LavishScript.Commands.AddCommand("ListPWPT", ListPrimaryWeaponPriorityTargetsInnerspaceCommand);
                LavishScript.Commands.AddCommand("PWPT", ListPrimaryWeaponPriorityTargetsInnerspaceCommand);
                LavishScript.Commands.AddCommand("ListDronePriorityTargets", ListDronePriorityTargetsInnerspaceCommand);
                LavishScript.Commands.AddCommand("ListDPT", ListDronePriorityTargetsInnerspaceCommand);
                LavishScript.Commands.AddCommand("DPT", ListDronePriorityTargetsInnerspaceCommand);
                LavishScript.Commands.AddCommand("ListTargets", ListTargetedandTargeting);
                LavishScript.Commands.AddCommand("ListItemHangarItems", ListItemHangarItems);
                LavishScript.Commands.AddCommand("ListItemHangar", ListItemHangarItems);
                //LavishScript.Commands.AddCommand("ListAmmoHangarItems", ListAmmoHangarItems);
                LavishScript.Commands.AddCommand("ListLootHangarItems", ListLootHangarItems);
                LavishScript.Commands.AddCommand("ListLootHangar", ListLootHangarItems);
                LavishScript.Commands.AddCommand("ListLootContainerItems", ListLootContainerItems);
                LavishScript.Commands.AddCommand("ListCachedPocketInfo", ListCachedPocketInfoInnerspaceCommand);
                LavishScript.Commands.AddCommand("AddWarpScramblerByName", AddWarpScramblerByName);
                LavishScript.Commands.AddCommand("AddWarpScrambler", AddWarpScramblerByName);
                LavishScript.Commands.AddCommand("AddWebifierByName", AddWebifierByName);
                LavishScript.Commands.AddCommand("AddWebifier", AddWebifierByName);
                LavishScript.Commands.AddCommand("AddIgnoredTarget", AddIgnoredTarget);
                LavishScript.Commands.AddCommand("AddIgnored", AddIgnoredTarget);
                LavishScript.Commands.AddCommand("RemoveIgnoredTarget", RemoveIgnoredTarget);
                LavishScript.Commands.AddCommand("RemoveIgnored", RemoveIgnoredTarget);
                LavishScript.Commands.AddCommand("AddDronePriorityTargetsByName", AddDronePriorityTargetsByName);
                LavishScript.Commands.AddCommand("AddDPT", AddDronePriorityTargetsByName);
                LavishScript.Commands.AddCommand("RemovedDronePriorityTargetsByName", RemovedDronePriorityTargetsByName);
                LavishScript.Commands.AddCommand("AddPrimaryWeaponPriorityTargetsByName", AddPrimaryWeaponPriorityTargetsByName);
                LavishScript.Commands.AddCommand("AddPWPT", AddPrimaryWeaponPriorityTargetsByName);
                LavishScript.Commands.AddCommand("RemovePrimaryWeaponPriorityTargetsByName", RemovePrimaryWeaponPriorityTargetsByName);
                LavishScript.Commands.AddCommand("RemovePWPTByName", RemovePrimaryWeaponPriorityTargetsByName);
                LavishScript.Commands.AddCommand("RemovePWPT", RemovePrimaryWeaponPriorityTargetsByName);
                LavishScript.Commands.AddCommand("ListClassInstanceInfo", ListClassInstanceInfo);
                LavishScript.Commands.AddCommand("ListQuestorCommands", ListQuestorCommands);
                LavishScript.Commands.AddCommand("QuestorCommands", ListQuestorCommands);
                LavishScript.Commands.AddCommand("Help", ListQuestorCommands);
                LavishScript.ExecuteCommand("alias " + Settings.Instance.LoadQuestorDebugInnerspaceCommandAlias + " " + Settings.Instance.LoadQuestorDebugInnerspaceCommand);  //"dotnet q1 questor.exe");
                LavishScript.ExecuteCommand("alias " + Settings.Instance.UnLoadQuestorDebugInnerspaceCommandAlias + " " + Settings.Instance.UnLoadQuestorDebugInnerspaceCommand);  //"dotnet -unload q1");
            }
        }
        #endregion Create Innerspace Commands

        #region List Innerspace Commands
        private static int ListQuestorCommands(string[] args)
        {
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "Questor commands you can run from innerspace", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "SetAutoStart                                 - SetAutoStart true|false", Logging.White);
            Logging.Log("InnerspaceCommands", "SetDisable3D                                 - SetDisable3D true|false", Logging.White);
            Logging.Log("InnerspaceCommands", "SetExitWhenIdle                              - SetExitWhenIdle true|false", Logging.White);
            Logging.Log("InnerspaceCommands", "SetQuestorStatetoCloseQuestor                - SetQuestorStatetoCloseQuestor true", Logging.White);
            Logging.Log("InnerspaceCommands", "SetQuestorStatetoIdle                        - SetQuestorStatetoIdle true", Logging.White);
            Logging.Log("InnerspaceCommands", "SetCombatMissionsBehaviorStatetoGotoBase     - SetCombatMissionsBehaviorStatetoGotoBase true", Logging.White);
            Logging.Log("InnerspaceCommands", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase     - SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase true", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorCommands                              - (this command) ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorEvents                                - Lists the available InnerSpace Events you can listen for ", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "AddWarpScramblerByName                       - Add NPCs by name to the WarpScramblers List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddWebifierByName                            - Add NPCs by name to the Webifiers List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddIgnoredTarget                             - Add name to the IgnoredTarget List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemoveIgnoredTarget                          - Remove name to the IgnoredTarget List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddDronePriorityTargetsByName                - Add NPCs by name to the DPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemoveDronePriorityTargetsByName             - Remove NPCs name from the DPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddPrimaryWeaponPriorityTargetsByName        - Add NPCs by name to the PWPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemovePrimaryWeaponPriorityTargetsByName     - Remove NPCs name from the PWPT List", Logging.White);
            Logging.Log("InnerspaceCommands", "ListItemHangarItems                          - Logs All Items in the ItemHangar", Logging.White);
            //Logging.Log("InnerspaceCommands", "ListAmmoHangarItems - missing                - Logs All Items in the (optionally configured) AmmoHangar", Logging.White);
            Logging.Log("InnerspaceCommands", "ListLootHangarItems                          - Logs All Items in the (optionally configured) LootHangar", Logging.White);
            Logging.Log("InnerspaceCommands", "ListLootContainerItems                       - Logs All Items in the (optionally configured) LootContainer", Logging.White);
            Logging.Log("InnerspaceCommands", "ListAllEntities                              - Logs All Entities on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListPotentialCombatTargets                   - Logs ListPotentialCombatTargets on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListHighValueTargets                         - Logs ListHighValueTargets on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListLowValueTargets                          - Logs ListLowValueTargets on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListIgnoredTargets                           - Logs the contents of the IgnoredTargets List", Logging.White);
            Logging.Log("InnerspaceCommands", "ListModules                                  - Logs Module List of My Current Ship", Logging.White);
            Logging.Log("InnerspaceCommands", "ListPrimaryWeaponPriorityTargets             - Logs PrimaryWeaponPriorityTargets", Logging.White);
            Logging.Log("InnerspaceCommands", "ListDronePriorityTargets                     - Logs DronePriorityTargets", Logging.White);
            Logging.Log("InnerspaceCommands", "ListTargets                                  - Logs ListTargets", Logging.White);
            Logging.Log("InnerspaceCommands", "ListEntitiesThatHaveUsLocked                 - Logs ListEntitiesThatHaveUsLocked", Logging.White);
            Logging.Log("InnerspaceCommands", "ListClassInstanceInfo                        - Logs Class Instance Info", Logging.White);
            Logging.Log("InnerspaceCommands", "ListCachedPocketInfo                         - Logs Cached Pocket Information", Logging.White);
            //
            // Slaves To Master Communication
            //
            Logging.Log("InnerspaceCommands", "                    Slave To Master Fleet Related Innerspace Commands", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsLocationIDofMaster       - Ask Master: What Is the LocationID of the Master (systems and stations are both locationIDs)", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsCoordofMaster            - Ask Master: What x,y,z coordinates is the Master at? (assumes you are already in local)", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsCurrentMissionAction     - Ask Master: What is the current mission action (if on grid w master)", Logging.White);
            Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatAmmoShouldILoad            - Ask Master: What Ammo DamageType should I load during ARM...", Logging.White);
            //
            // Master To Slaves Communication
            //
            Logging.Log("InnerspaceCommands", "                    Master To Slave Fleet Related Innerspace Commands", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_SetDestinationLocationID      - Tell slaves where to go", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_MasterCoordinatesAre          - Tell slaves where Master is x,y,z coordinates", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_DoThisMissionAction           - Tell slaves to do this mission action", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_MasterIsWarpingTo             - Tell slaves where Master is warping", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_SlavesGotoBase                - Tell slaves to set State to GotoBase", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_DoNotLootItemName             - Tell slaves not to loot this ItemName", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_SetAutoStart                  - Tell slaves to turn autostart on or off", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_WhereAreYou                   - Tell slaves to report locationIDs and coordinates", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_WhatAreYouShooting            - Tell slaves to report what they are shooting currently", Logging.White);
            Logging.Log("InnerspaceCommands", "MasterToSlaves_ShootThisEntityID             - Tell slaves to Add masters Target as Kill Priority Target", Logging.White);
            
            return 0;
        }
        #endregion List Innerspace Commands

        #region Slave to Master Innerspace Commands
        private static int SlaveToMaster_WhatIsLocationIDofMaster_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsLocationIDofMaster - What is LocationID of Master", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering SlaveState.SlaveToMaster_WhatIsLocationIDofMaster", Logging.Debug);
            _States.CurrentSlaveState = SlaveState.SlaveToMaster_WhatIsLocationIDofMaster;
            return 0;
        }

        private static int SlaveToMaster_WhatIsCoordofMaster_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsCoordofMaster - What are the coordinates of Master", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering SlaveState.SlaveToMaster_WhatIsCoordofMaster", Logging.Debug);
            _States.CurrentSlaveState = SlaveState.SlaveToMaster_WhatIsCoordofMaster;
            return 0;
        }

        private static int SlaveToMaster_WhatIsCurrentMissionAction_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatIsCurrentMissionAction - What Mission Action is the Master Running (if any)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering SlaveState.SlaveToMaster_WhatIsCurrentMissionAction", Logging.Debug);
            _States.CurrentSlaveState = SlaveState.SlaveToMaster_WhatIsCurrentMissionAction;
            return 0;
        }
        
        private static int SlaveToMaster_WhatAmmoShouldILoad_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SlaveToMaster_WhatAmmoShouldILoad - What Ammo should be loaded during Arm?", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering SlaveState.SlaveToMaster_WhatIsCurrentMissionAction", Logging.Debug);
            _States.CurrentSlaveState = SlaveState.SlaveToMaster_WhatIsCurrentMissionAction;
            return 0;
        }
        #endregion Slave to Master Innerspace Commands

        #region Master To Slave Innerspace Commands
        private static int MasterToSlaves_SetDestinationLocationID_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_SetDestinationLocationID - Slaves set destination to LocationID (system or station)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_SetDestinationLocationID", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_SetDestinationLocationID;
            return 0;
        }

        private static int MasterToSlaves_MasterCoordinatesAre_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_MasterCoordinatesAre - Masters x,y,z Coordinates are...", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_MasterCoordinatesAre", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_MasterCoordinatesAre;
            return 0;
        }

        private static int MasterToSlaves_MasterIsWarpingTo_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_MasterIsWarpingTo - Master is warping to EntityName (or bookmark)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_MasterIsWarpingTo", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_MasterIsWarpingTo;
            return 0;
        }

        private static int MasterToSlaves_SlavesGotoBase_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_SlavesGotoBase - Slaves should set state to GotoBase", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_SlavesGotoBase", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_SlavesGotoBase;
            return 0;
        }

        private static int MasterToSlaves_DoThisMissionAction_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_DoThisMissionAction - Slaves should execute this mission action", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_DoThisMissionAction", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_DoThisMissionAction;
            return 0;
        }

        private static int MasterToSlaves_DoNotLootItemName_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_DoNotLootItemName - Slaves should not loot this ItemName", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_DoNotLootItemName", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_DoNotLootItemName;
            return 0;
        }

        private static int MasterToSlaves_SetAutoStart_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_SetAutoStart - Slaves should Set AutoStart on or off", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_SetAutoStart", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_SetAutoStart;
            return 0;
        }

        private static int MasterToSlaves_WhereAreYou_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_WhereAreYou - Slaves should report where they are located, LocationID and Coordinates", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_WhereAreYou", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_WhereAreYou;
            return 0;
        }

        private static int MasterToSlaves_WhatAreYouShooting_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_WhatAreYouShooting - Slaves should report what they are shooting: Name and EntityID (if any)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_WhatAreYouShooting", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_WhatAreYouShooting;
            return 0;
        }

        private static int MasterToSlaves_ShootThisEntityID_InnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "MasterToSlaves_ShootThisEntityID - Slaves should Add this EntityID as a Primary Weapon Priority Kill Target (if any)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommands.MasterToSlaves_ShootThisEntityID", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.MasterToSlaves_ShootThisEntityID;
            return 0;
        }
        #endregion Master To Slave Innerspace Commands

        #region Slave to Master Routines

        private static bool SlaveToMaster_WhatIsLocationIDofMaster()
        {
            try
            {
                if (Settings.Instance.FleetSupportSlave)
                {
                    //
                    // 
                    //
                    if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
                    {
                        int? _locationID = Cache.Instance.DirectEve.Session.LocationId;
                        if (_locationID != null)
                        {
                            const string RelayToWhere = "all";
                            string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "-event EVENT_SlaveToMaster_WhatIsLocationIDofMaster";
                            if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
                            LavishScript.ExecuteCommand(LavishCommandToBroadcast);
                            return true;
                        }

                        return false;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatIsLocationIDofMaster", "[" + exception + "]", Logging.Teal);
                return true;
            }
        }

        private static bool SlaveToMaster_WhatIsCoordofMaster()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatIsCoordofMaster", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool SlaveToMaster_WhatIsCurrentMissionAction()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatIsCurrentMissionAction", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool SlaveToMaster_WhatAmmoShouldILoad()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("SlaveToMaster_WhatAmmoShouldILoad", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }
        
        #endregion Slave to Master Routines

        #region Master to Slave Routines

        private static bool MasterToSlaves_SetDestinationLocationID()
        {
            try
            {
                if (DateTime.UtcNow > Cache.Instance.LastSessionChange.AddSeconds(10))
                {
                    int? _locationID = Cache.Instance.DirectEve.Session.LocationId;
                    if (_locationID != null)
                    {
                        const string RelayToWhere = "all";
                        string LavishCommandToBroadcast = "relay " + RelayToWhere + " " + "-event BlahNewEventHere";
                        if (Settings.Instance.DebugFleetSupportMaster) InnerSpace.Echo(string.Format("[BroadcastViaInnerspace] " + LavishCommandToBroadcast));
                        LavishScript.ExecuteCommand(LavishCommandToBroadcast);
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_SetDestinationLocationID", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_MasterIsWarpingTo()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_MasterIsWarpingTo", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_SlavesGotoBase()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_SlavesGotoBase", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_DoThisMissionAction()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_DoThisMissionAction", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_DoNotLootItemName()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_DoNotLootItemName", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_SetAutoStart()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_SetAutoStart", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_WhereAreYou()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_WhereAreYou", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_WhatAreYouShooting()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_WhatAreYouShooting", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        private static bool MasterToSlaves_ShootThisEntityID()
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logging.Log("MasterToSlaves_ShootThisEntityID", "[" + exception + "]", Logging.Teal);
                return true;
            }

            return false;
        }

        #endregion Master to Slave Routines

        private static int ListQuestorEvents(string[] args)
        {
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "Questor Events you can listen for from an innerspace script", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorIdle                                   - This Event fires when entering the QuestorState Idle ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorState                                  - This Event fires when the State changes", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorCombatMissionsBehaviorState            - This Event fires when the State changes", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorDedicatedBookmarkSalvagerBehaviorState - This Event fires when the State changes", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorAutoStartState                         - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorExitWhenIdleState                      - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorDisable3DState                         - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorPanicState                             - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorPausedState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorDronesState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorCombatState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", "QuestorTravelerState                          - This Event fires when the State changes ", Logging.White);
            Logging.Log("InnerspaceCommands", " ", Logging.White);
            return 0;
        }

        private static int ListClassInstanceInfo(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListClassInstanceInfo - Lists Class Instance Count for These Classes", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListClassInstanceInfo", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListClassInstanceInfo;
            return 0;
        }

        private static int ListCachedPocketInfoInnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListCachedPocketInfo - Lists Cached Pocket Info (Size of Dictionaries)", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommands", "Entering InnerspaceCommandsState.ListCachedPocketInfo", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.ListCachedPocketInfo;
            return 0;
        }
        

        private static int ListTargetedandTargeting(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListTargets - Lists Entities Targeted and Targeting", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListTargetedandTargeting", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListTargetedandTargeting;
            return 0;
        }

        private static int ListItemHangarItems(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListItemHangarItems - Lists Items in the ItemHangar", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListItemHangarItems", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListItemHangarItems;
            return 0;
        }

        private static int ListLootHangarItems(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListLootHangarItems - Lists Items in the LootHangar", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListLootHangarItems", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListLootHangarItems;
            return 0;
        }

        private static int ListLootContainerItems(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListLootContainerItems - Lists Items in the LootContainer", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListLootContainerItems", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListLootContainerItems;
            return 0;
        }

        private static int AddDronePriorityTargetsByName(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "AddDronePriorityTargetsByName NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            AddThese = args[1];

            Logging.Log("InnerspaceCommands", "Processing Command as: AddDronePriorityTargetsByName " + AddThese, Logging.White);
            Cache.Instance.AddDronePriorityTargetsByName(AddThese);
            return 0;
        }

        //AddWarpScramblerByName
        private static int AddWarpScramblerByName(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "AddWarpScramblerByName NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            AddThese = args[1];

            Logging.Log("InnerspaceCommands", "Processing Command as: AddWarpScramblerByName " + AddThese, Logging.White);
            Cache.Instance.AddWarpScramblerByName(AddThese);
            return 0;
        }

        private static int AddWebifierByName(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "AddWebifierByName NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            AddThese = args[1];

            Logging.Log("InnerspaceCommands", "Processing Command as: AddWebifierByName " + AddThese, Logging.White);
            Cache.Instance.AddWebifierByName(AddThese);
            return 0;
        }

        private static int RemovedDronePriorityTargetsByName(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "RemovedDronePriorityTargetsByName NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            string RemoveThese = args[1];

            Cache.Instance.RemovedDronePriorityTargetsByName(RemoveThese);
            return 0;
        }

        private static string AddThese;

        private static int AddPrimaryWeaponPriorityTargetsByName(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "AddPrimaryWeaponPriorityTargetsByName NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            AddThese = args[1];

            Logging.Log("InnerspaceCommands", "Processing Command as: AddPrimaryWeaponPriorityTargetsByName " + AddThese, Logging.White);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.AddPWPT;
            return 0;
        }

        public static void AddPrimaryWeaponPriorityTargetsByName(string stringEntitiesToAdd)
        {
            try
            {
                if (Cache.Instance.EntitiesOnGrid.Any())
                {
                    if (Cache.Instance.EntitiesOnGrid.Any(i => i.Name == stringEntitiesToAdd))
                    {
                        IEnumerable<EntityCache> entitiesToAdd = Cache.Instance.EntitiesOnGrid.Where(i => i.Name == stringEntitiesToAdd).ToList();
                        if (entitiesToAdd.Any())
                        {

                            foreach (EntityCache entityToAdd in entitiesToAdd)
                            {
                                Cache.Instance.AddPrimaryWeaponPriorityTarget(entityToAdd, PrimaryWeaponPriority.PriorityKillTarget, "AddPWPTByName");
                                continue;
                            }

                            return;
                        }

                        Logging.Log("Adding PWPT", "[" + stringEntitiesToAdd + "] was not found.", Logging.Debug);
                        return;
                    }

                    int EntitiesOnGridCount = 0;
                    if (Cache.Instance.EntitiesOnGrid.Any())
                    {
                        EntitiesOnGridCount = Cache.Instance.EntitiesOnGrid.Count(i => i.IsOnGridWithMe);
                    }

                    int EntitiesCount = 0;
                    if (Cache.Instance.EntitiesOnGrid.Any())
                    {
                        EntitiesCount = Cache.Instance.EntitiesOnGrid.Count();
                    }

                    Logging.Log("Adding PWPT", "[" + stringEntitiesToAdd + "] was not found. [" + EntitiesOnGridCount + "] entities on grid [" + EntitiesCount + "] entities", Logging.Debug);
                    return;
                }

                Logging.Log("Adding PWPT", "[" + stringEntitiesToAdd + "] was not found. no entities on grid", Logging.Debug);
                return;

            }
            catch (Exception ex)
            {
                Logging.Log("AddPrimaryWeaponPriorityTargets", "Exception [" + ex + "]", Logging.Debug);
            }

            return;
        }

        private static int RemovePrimaryWeaponPriorityTargetsByName(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "RemovedPrimaryWeaponPriorityTargetsByName NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            string RemoveThese = args[1];

            Cache.Instance.RemovePrimaryWeaponPriorityTargetsByName(RemoveThese);
            return 0;
        }

        private static int AddIgnoredTarget(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "AddIgnoredTarget NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            string ignoreThese = args[1];
            if (!Cache.Instance.IgnoreTargets.Contains(ignoreThese))
            {
                Cache.Instance.IgnoreTargets.Add(ignoreThese.Trim());    
            }
            int IgnoreTargetsCount = 0;
            if (Cache.Instance.IgnoreTargets.Any())
            {
                IgnoreTargetsCount = Cache.Instance.IgnoreTargets.Count();
            }
            Logging.Log("InnerspaceCommands", "Added [" + ignoreThese + "] to Ignored Targets List. IgnoreTargets Contains [" + IgnoreTargetsCount + "] items", Logging.White);
            return 0;
        }

        private static int RemoveIgnoredTarget(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "RemoveIgnoredTarget NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            string unIgnoreThese = args[1];
            if (Cache.Instance.IgnoreTargets.Contains(unIgnoreThese))
            {
                Cache.Instance.IgnoreTargets.Remove(unIgnoreThese.Trim());
            }

            int IgnoreTargetsCount = 0;
            if ( Cache.Instance.IgnoreTargets.Any())
            {
                IgnoreTargetsCount = Cache.Instance.IgnoreTargets.Count;
            }
            Logging.Log("InnerspaceCommands", "Removed [" + unIgnoreThese + "] from Ignored Targets List. IgnoreTargets Contains [" + IgnoreTargetsCount + "] items", Logging.White);
            return 0;
        }

        private static int ListIgnoredTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListIgnoredTargets - Lists Ignored Targets", Logging.White);
                return -1;
            }
            Logging.Log("Statistics", "Entering StatisticsState.ListIgnoredTargets", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListIgnoredTargets;
            return 0;
        }

        private static int ListPrimaryWeaponPriorityTargetsInnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListPrimaryWeaponPriorityTargets - Lists Primary Weapon Priority Targets", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListPrimaryWeaponPriorityTargets", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.ListPrimaryWeaponPriorityTargets;
            return 0;
        }

        public static bool ListPrimaryWeaponPriorityTargets()
        {
            Logging.Log("PWPT", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            if (Cache.Instance.PreferredPrimaryWeaponTarget != null && Cache.Instance.PreferredPrimaryWeaponTarget.IsOnGridWithMe)
            {
                Logging.Log("PWPT", "[" + 0 + "] PreferredPrimaryWeaponTarget [" + Cache.Instance.PreferredPrimaryWeaponTarget.Name + "][" + Math.Round(Cache.Instance.PreferredPrimaryWeaponTarget.Distance / 1000, 0) + "k] IsInOptimalRange [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsInOptimalRange + "] IsTarget [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsTarget + "]", Logging.Debug);
            }

            if (Cache.Instance.PrimaryWeaponPriorityTargets.Any())
            {
                int icount = 0;
                foreach (PriorityTarget PrimaryWeaponPriorityTarget in Cache.Instance.PrimaryWeaponPriorityTargets)
                {
                    icount++;
                    Logging.Log(icount.ToString(), "[" + PrimaryWeaponPriorityTarget.Name + "] PrimaryWeaponPriorityLevel [" + PrimaryWeaponPriorityTarget.PrimaryWeaponPriority + "] EntityID [" + Cache.Instance.MaskedID(PrimaryWeaponPriorityTarget.EntityID) + "]", Logging.Debug);
                    //Logging.Log(icount.ToString(), "[" + PrimaryWeaponPriorityTarget.Name + "][" + Math.Round(primaryWeaponPriorityEntity.Distance / 1000, 0) + "k] IsInOptimalRange [" + primaryWeaponPriorityEntity.IsInOptimalRange + "] IsTarget [" + primaryWeaponPriorityEntity.IsTarget + "] PrimaryWeaponPriorityLevel [" + primaryWeaponPriorityEntity.PrimaryWeaponPriorityLevel + "]", Logging.Debug);
                }
            }
            Logging.Log("PWPT", "--------------------------- Done  (listed above) -----------------------------", Logging.Yellow);
            Logging.Log("PWPT", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            if (Cache.Instance.PreferredPrimaryWeaponTarget != null && Cache.Instance.PreferredPrimaryWeaponTarget.IsOnGridWithMe)
            {
                Logging.Log("PWPT", "[" + 0 + "] PreferredPrimaryWeaponTarget [" + Cache.Instance.PreferredPrimaryWeaponTarget.Name + "][" + Math.Round(Cache.Instance.PreferredPrimaryWeaponTarget.Distance / 1000, 0) + "k] IsInOptimalRange [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsInOptimalRange + "] IsTarget [" + Cache.Instance.PreferredPrimaryWeaponTarget.IsTarget + "]", Logging.Debug);
            }

            if (Cache.Instance.PrimaryWeaponPriorityEntities.Any())
            {
                int icount = 0;
                foreach (EntityCache PrimaryWeaponPriorityEntity in Cache.Instance.PrimaryWeaponPriorityEntities)
                {
                    icount++;
                    Logging.Log(icount.ToString(), "[" + PrimaryWeaponPriorityEntity.Name + "] PrimaryWeaponPriorityLevel [" + PrimaryWeaponPriorityEntity.PrimaryWeaponPriorityLevel + "][" + PrimaryWeaponPriorityEntity.MaskedId + "]", Logging.Debug);
                    //Logging.Log(icount.ToString(), "[" + PrimaryWeaponPriorityTarget.Name + "][" + Math.Round(primaryWeaponPriorityEntity.Distance / 1000, 0) + "k] IsInOptimalRange [" + primaryWeaponPriorityEntity.IsInOptimalRange + "] IsTarget [" + primaryWeaponPriorityEntity.IsTarget + "] PrimaryWeaponPriorityLevel [" + primaryWeaponPriorityEntity.PrimaryWeaponPriorityLevel + "]", Logging.Debug);
                }
            }
            Logging.Log("PWPT", "--------------------------- Done  (listed above) -----------------------------", Logging.Yellow);
            return true;
        }

        private static int ListDronePriorityTargetsInnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListDronePriorityTargets - Lists DronePriorityTargets", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListDronePriorityTargets", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListDronePriorityTargets;
            return 0;
        }

        private static int ModuleInfo(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ModuleInfo - Lists ModuleInfo of current ship", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ModuleInfo:", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ModuleInfo;
            return 0;
        }

        /*
        private static int FindEntitiesNamed(string[] args)
        {
            if (args.Length < 2)
            {
                Logging.Log("InnerspaceCommands", "FindEntitiesNamed NameOfNPCInQuotes", Logging.White);
                return -1;
            }

            string FindTheseEntityNames = args[1];
            Logging.Log("InnerspaceCommands", "Processing Command as: FindEntitiesNamed " + FindTheseEntityNames, Logging.White);
            _States.CurrentStatisticsState = StatisticsState.FindEntitiesnamed;
            return 0;
        }
        */

        private static int ListAllEntities(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListAllEntities - Logs Entities on grid", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommand", "Entering InnerspaceCommandState.LogAllEntities", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.LogAllEntities;
            return 0;
        }

        private static int ListEntitiesThatHaveUsLockedInnerspaceCommand(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListEntitiesThatHaveUsLocked - Logs Entities on grid that have us targeted", Logging.White);
                return -1;
            }

            Logging.Log("InnerspaceCommand", "Entering InnerspaceCommandState.ListEntitiesThatHaveUsLocked", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.ListEntitiesThatHaveUsLocked;
            return 0;
        }
        
        private static int ListLowValueTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListLowValueTargets - Logs ListLowValueTargets on grid", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListLowValueTargets", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListLowValueTargets;
            return 0;
        }

        private static int ListHighValueTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListHighValueTargets - Logs ListHighValueTargets on grid", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListHighValueTargets", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListHighValueTargets;
            return 0;
        }

        private static int ListPotentialCombatTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListPotentialCombatTargets - Logs PotentialCombatTargets on grid", Logging.White);
                return -1;
            }

            Logging.Log("Statistics", "Entering StatisticsState.ListPotentialCombatTargets", Logging.Debug);
            _States.CurrentStatisticsState = StatisticsState.ListPotentialCombatTargets;
            return 0;
        }

        private static int SetAutoStart(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("InnerspaceCommands", "SetAutoStart true|false", Logging.White);
                return -1;
            }

            Settings.Instance.AutoStart = value;

            Logging.Log("InnerspaceCommands", "AutoStart is turned " + (value ? "[on]" : "[off]"), Logging.White);
            return 0;
        }

        private static int SetDisable3D(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("InnerspaceCommands", "SetDisable3D true|false", Logging.White);
                return -1;
            }

            Settings.Instance.Disable3D = value;

            Logging.Log("InnerspaceCommands", "Disable3D is turned " + (value ? "[on]" : "[off]"), Logging.White);
            return 0;
        }

        private static int SetExitWhenIdle(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("InnerspaceCommands", "SetExitWhenIdle true|false", Logging.White);
                Logging.Log("InnerspaceCommands", "Note: AutoStart is automatically turned off when ExitWhenIdle is turned on", Logging.White);
                return -1;
            }

            Cache.Instance.ExitWhenIdle = value;

            Logging.Log("InnerspaceCommands", "ExitWhenIdle is turned " + (value ? "[on]" : "[off]"), Logging.White);

            if (value && Settings.Instance.AutoStart)
            {
                Settings.Instance.AutoStart = false;
                Logging.Log("InnerspaceCommands", "AutoStart is turned [off]", Logging.White);
            }
            return 0;
        }

        private static int SetQuestorStatetoCloseQuestor(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SetQuestorStatetoCloseQuestor - Changes the QuestorState to CloseQuestor which will GotoBase and then Exit", Logging.White);
                return -1;
            }

            Settings.Instance.AutoStart = false;
            _States.CurrentQuestorState = QuestorState.CloseQuestor;

            Logging.Log("InnerspaceCommands", "QuestorState is now: CloseQuestor ", Logging.White);
            return 0;
        }

        private static int SetQuestorStatetoIdle(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SetQuestorStatetoIdle - Changes the QuestorState to Idle", Logging.White);
                return -1;
            }

            Cache.Instance.Paused = false;
            _States.CurrentQuestorState = QuestorState.Idle;

            _States.CurrentPanicState = PanicState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            _States.CurrentTravelerState = TravelerState.Idle;
            _States.CurrentMiningState = MiningState.Idle;

            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Idle;
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
            //_States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Idle;
            _States.CurrentCombatHelperBehaviorState = CombatHelperBehaviorState.Idle;


            Logging.Log("InnerspaceCommands", "QuestorState is now: Idle ", Logging.White);
            return 0;
        }

        private static int SetDestToSystem(string[] args)
        {
            long value;
            if (args.Length != 2 || !long.TryParse(args[1], out value))
            {
                Logging.Log("InnerspaceCommands", "SetDestToSystem - Sets destination to the locationID specified", Logging.White);
                return -1;
            }

            long _locationid = value;

            Cache.Instance.DirectEve.Navigation.SetDestination(_locationid);
            switch (_States.CurrentQuestorState)
            {
                case QuestorState.Idle:
                    return -1;
                case QuestorState.CombatMissionsBehavior:
                    _States.CurrentTravelerState = TravelerState.Idle;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Traveler;
                    return 0;

                case QuestorState.DedicatedBookmarkSalvagerBehavior:
                    _States.CurrentTravelerState = TravelerState.Idle;
                    _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Traveler;
                    return 0;

                case QuestorState.CombatHelperBehavior:
                    _States.CurrentTravelerState = TravelerState.Idle;
                    _States.CurrentCombatHelperBehaviorState = CombatHelperBehaviorState.Traveler;
                    return 0;

                //case QuestorState.BackgroundBehavior:
                //    _States.CurrentTravelerState = TravelerState.Idle;
                //    _States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.Traveler;
                //    return 0;
            }
            return 0;
        }

        private static int SetCombatMissionsBehaviorStatetoGotoBase(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SetCombatMissionsBehaviorStatetoGotoBase - Changes the CombatMissionsBehaviorState to GotoBase", Logging.White);
                return -1;
            }

            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;

            Logging.Log("InnerspaceCommands", "CombatMissionsBehaviorState is now: GotoBase ", Logging.White);
            return 0;
        }

        private static int SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase - Changes the SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase to GotoBase", Logging.White);
                return -1;
            }

            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;

            Logging.Log("InnerspaceCommands", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase is now: GotoBase ", Logging.White);
            return 0;
        }

        private static int IfInPodSwitchToNoobShiporShuttle(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "IfInPodSwitchToNoobShiporShuttle - If the toon is in a pod switch to a noobship or shuttle if avail (otherwise do nothing)", Logging.White);
                return -1;
            }

            //_States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.SwitchToNoobShip1;

            Logging.Log("InnerspaceCommands", "CurrentBackgroundBehaviorState is now: SwitchToNoobShip1 ", Logging.White);
            return 0;
        }

        public static bool LogEntities(List<EntityCache> things, bool force = false)
        {
            // iterate through entities
            //
            Logging.Log("Entities", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            things = things.ToList();
            if (things.Any())
            {
                int icount = 0;
                foreach (EntityCache thing in things.OrderBy(i => i.Distance))
                {
                    icount++;
                    Logging.Log(icount.ToString(), thing.Name + "[" + Math.Round(thing.Distance / 1000, 0) + "k] GroupID[" + thing.GroupId + "] ID[" + Cache.Instance.MaskedID(thing.Id) + "] isSentry[" + thing.IsSentry + "] IsHVT[" + thing.IsHighValueTarget + "] IsLVT[" + thing.IsLowValueTarget + "] IsIgnored[" + thing.IsIgnored + "] isTarget [" + thing.IsTarget + "] isTargeting [" + thing.IsTargeting + "]", Logging.Debug);
                }
            }
            Logging.Log("Entities", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);

            return true;
        }

        private static int ListCachedPocketInfo()
        {
            Logging.Log("ListCachedPocketInfo", "Entering InnerspaceCommandsState.ListCachedPocketInfo", Logging.Debug);
            Logging.Log("ListCachedPocketInfo", "--------------------------- Start (listed below)-----------------------------", Logging.Yellow);
            
            int ListofWebbingEntitiesCount = 0;
            if (Cache.Instance.ListofWebbingEntities.Any())
            {
                ListofWebbingEntitiesCount = Cache.Instance.ListofWebbingEntities.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + ListofWebbingEntitiesCount + "] entries in ListofWebbingEntities", Logging.Yellow);

            int ListOfDampenuingEntitiesCount = 0;
            if (Cache.Instance.ListOfDampenuingEntities.Any())
            {
                ListOfDampenuingEntitiesCount = Cache.Instance.ListOfDampenuingEntities.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + ListOfDampenuingEntitiesCount + "] entries in ListOfDampenuingEntities", Logging.Yellow);

            int ListOfJammingEntitiesCount = 0;
            if (Cache.Instance.ListOfJammingEntities.Any())
            {
                ListOfJammingEntitiesCount = Cache.Instance.ListOfJammingEntities.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + ListOfJammingEntitiesCount + "] entries in ListOfJammingEntities", Logging.Yellow);

            int ListOfTargetPaintingEntitiesCount = 0;
            if (Cache.Instance.ListOfTargetPaintingEntities.Any())
            {
                ListOfTargetPaintingEntitiesCount = Cache.Instance.ListOfTargetPaintingEntities.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + ListOfTargetPaintingEntitiesCount + "] entries in ListOfTargetPaintingEntities", Logging.Yellow);

            int ListOfTrackingDisruptingEntitiesCount = 0;
            if (Cache.Instance.ListOfTrackingDisruptingEntities.Any())
            {
                ListOfTrackingDisruptingEntitiesCount = Cache.Instance.ListOfTrackingDisruptingEntities.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + ListOfTargetPaintingEntitiesCount + "] entries in ListOfTrackingDisruptingEntities", Logging.Yellow);

            int ListOfWarpScramblingEntitiesCount = 0;
            if (Cache.Instance.ListOfWarpScramblingEntities.Any())
            {
                ListOfWarpScramblingEntitiesCount = Cache.Instance.ListOfWarpScramblingEntities.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + ListOfWarpScramblingEntitiesCount + "] entries in ListOfWarpScramblingEntities", Logging.Yellow);

            int EntityNamesCount = 0;
            if (Cache.Instance.EntityNames.Any())
            {
                EntityNamesCount = Cache.Instance.EntityNames.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityNamesCount + "] entries in EntityNames", Logging.Yellow);

            int EntityTypeIDCount = 0;
            if (Cache.Instance.EntityTypeID.Any())
            {
                EntityTypeIDCount = Cache.Instance.EntityTypeID.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityTypeIDCount + "] entries in EntityTypeID", Logging.Yellow);

            int EntityGroupIDCount = 0;
            if (Cache.Instance.EntityGroupID.Any())
            {
                EntityGroupIDCount = Cache.Instance.EntityGroupID.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityGroupIDCount + "] entries in EntityGroupID", Logging.Yellow);

            int EntityIsFrigateCount = 0;
            if (Cache.Instance.EntityIsFrigate.Any())
            {
                EntityIsFrigateCount = Cache.Instance.EntityIsFrigate.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsFrigateCount + "] entries in EntityIsFrigate", Logging.Yellow);

            int EntityIsNPCFrigateCount = 0;
            if (Cache.Instance.EntityIsNPCFrigate.Any())
            {
                EntityIsNPCFrigateCount = Cache.Instance.EntityIsNPCFrigate.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsNPCFrigateCount + "] entries in EntityIsNPCFrigate", Logging.Yellow);

            int EntityIsCruiserCount = 0;
            if (Cache.Instance.EntityIsCruiser.Any())
            {
                EntityIsCruiserCount = Cache.Instance.EntityIsCruiser.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsCruiserCount + "] entries in EntityIsCruiser", Logging.Yellow);

            int EntityIsNPCCruiserCount = 0;
            if (Cache.Instance.EntityIsNPCCruiser.Any())
            {
                EntityIsNPCCruiserCount = Cache.Instance.EntityIsNPCCruiser.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsNPCCruiserCount + "] entries in EntityIsNPCCruiser", Logging.Yellow);

            int EntityIsBattleCruiserCount = 0;
            if (Cache.Instance.EntityIsBattleCruiser.Any())
            {
                EntityIsBattleCruiserCount = Cache.Instance.EntityIsBattleCruiser.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsBattleCruiserCount + "] entries in EntityIsBattleCruiser", Logging.Yellow);

            int EntityIsNPCBattleCruiserCount = 0;
            if (Cache.Instance.EntityIsNPCBattleCruiser.Any())
            {
                EntityIsNPCBattleCruiserCount = Cache.Instance.EntityIsNPCBattleCruiser.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsNPCBattleCruiserCount + "] entries in EntityIsNPCBattleCruiser", Logging.Yellow);

            int EntityIsBattleShipCount = 0;
            if (Cache.Instance.EntityIsBattleShip.Any())
            {
                EntityIsBattleShipCount = Cache.Instance.EntityIsBattleShip.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsBattleShipCount + "] entries in EntityIsBattleShip", Logging.Yellow);

            int EntityIsNPCBattleShipCount = 0;
            if (Cache.Instance.EntityIsNPCBattleShip.Any())
            {
                EntityIsNPCBattleShipCount = Cache.Instance.EntityIsNPCBattleShip.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsNPCBattleShipCount + "] entries in EntityIsNPCBattleShip", Logging.Yellow);

            int EntityIsHighValueTargetCount = 0;
            if (Cache.Instance.EntityIsHighValueTarget.Any())
            {
                EntityIsHighValueTargetCount = Cache.Instance.EntityIsHighValueTarget.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsHighValueTargetCount + "] entries in EntityIsHighValueTarget", Logging.Yellow);

            int EntityIsLowValueTargetCount = 0;
            if (Cache.Instance.EntityIsLowValueTarget.Any())
            {
                EntityIsLowValueTargetCount = Cache.Instance.EntityIsLowValueTarget.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsLowValueTargetCount + "] entries in EntityIsLowValueTarget", Logging.Yellow);

            int EntityIsLargeCollidableCount = 0;
            if (Cache.Instance.EntityIsLargeCollidable.Any())
            {
                EntityIsLargeCollidableCount = Cache.Instance.EntityIsLargeCollidable.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsLargeCollidableCount + "] entries in EntityIsLargeCollidable", Logging.Yellow);

            int EntityIsSentryCount = 0;
            if (Cache.Instance.EntityIsSentry.Any())
            {
                EntityIsSentryCount = Cache.Instance.EntityIsSentry.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsSentryCount + "] entries in EntityIsSentry", Logging.Yellow);

            int EntityIsMiscJunkCount = 0;
            if (Cache.Instance.EntityIsMiscJunk.Any())
            {
                EntityIsMiscJunkCount = Cache.Instance.EntityIsMiscJunk.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsMiscJunkCount + "] entries in EntityIsMiscJunk", Logging.Yellow);

            int EntityIsBadIdeaCount = 0;
            if (Cache.Instance.EntityIsBadIdea.Any())
            {
                EntityIsBadIdeaCount = Cache.Instance.EntityIsBadIdea.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsBadIdeaCount + "] entries in EntityIsBadIdea", Logging.Yellow);

            int EntityIsFactionWarfareNPCCount = 0;
            if (Cache.Instance.EntityIsFactionWarfareNPC.Any())
            {
                EntityIsFactionWarfareNPCCount = Cache.Instance.EntityIsFactionWarfareNPC.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsFactionWarfareNPCCount + "] entries in EntityIsFactionWarfareNPC", Logging.Yellow);

            int EntityIsNPCByGroupIDCount = 0;
            if (Cache.Instance.EntityIsNPCByGroupID.Any())
            {
                EntityIsNPCByGroupIDCount = Cache.Instance.EntityIsNPCByGroupID.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsNPCByGroupIDCount + "] entries in EntityIsNPCByGroupID", Logging.Yellow);

            int EntityIsEntutyIShouldLeaveAloneCount = 0;
            if (Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Any())
            {
                EntityIsEntutyIShouldLeaveAloneCount = Cache.Instance.EntityIsEntutyIShouldLeaveAlone.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsEntutyIShouldLeaveAloneCount + "] entries in EntityIsEntutyIShouldLeaveAlone", Logging.Yellow);

            int EntityHaveLootRightsCount = 0;
            if (Cache.Instance.EntityHaveLootRights.Any())
            {
                EntityHaveLootRightsCount = Cache.Instance.EntityHaveLootRights.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityHaveLootRightsCount + "] entries in EntityHaveLootRights", Logging.Yellow);

            int EntityIsStargateCount = 0;
            if (Cache.Instance.EntityIsStargate.Any())
            {
                EntityIsStargateCount = Cache.Instance.EntityIsStargate.Count();
            }
            Logging.Log("ListCachedPocketInfo", "[" + EntityIsStargateCount + "] entries in EntityIsStargate", Logging.Yellow);
            Logging.Log("ListCachedPocketInfo", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);
            Logging.Log("ListCachedPocketInfo", "--- Note: pausing or warping / jumping will clear the above dictionaries  ---", Logging.Yellow);
            return 0;
        }
        
        public void ProcessState()
        {
            switch (_States.CurrentInnerspaceCommandsState)
            {
                case InnerspaceCommandsState.Idle:
                    break;

                case InnerspaceCommandsState.LogAllEntities:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.LogAllEntities", Logging.Debug);
                        InnerspaceCommands.LogEntities(Cache.Instance.EntitiesOnGrid.ToList());
                    }
                    break;

                case InnerspaceCommandsState.ListEntitiesThatHaveUsLocked:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.ListEntitiesThatHaveUsLocked", Logging.Debug);
                        InnerspaceCommands.LogEntities(Cache.Instance.EntitiesOnGrid.Where(i => i.IsTargetedBy).ToList());
                    }
                    break;

                case InnerspaceCommandsState.ListPrimaryWeaponPriorityTargets:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.ListPrimaryWeaponPriorityTargets", Logging.Debug);
                        InnerspaceCommands.ListPrimaryWeaponPriorityTargets();
                    }
                    break;

                case InnerspaceCommandsState.AddPWPT:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.AddPWPT", Logging.Debug);
                        InnerspaceCommands.AddPrimaryWeaponPriorityTargetsByName(AddThese);
                    }
                    break;

                case InnerspaceCommandsState.ListCachedPocketInfo:
                    if (!Cache.Instance.InWarp)
                    {
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.ListCachedPocketInfo", Logging.Debug);
                        InnerspaceCommands.ListCachedPocketInfo();
                    }
                    break;

                case InnerspaceCommandsState.SlaveToMaster_WhatIsLocationIDofMaster:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.SlaveToMaster_WhatIsLocationIDofMaster", Logging.Debug);
                        if (!InnerspaceCommands.SlaveToMaster_WhatIsLocationIDofMaster()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.SlaveToMaster_WhatIsCoordofMaster:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.SlaveToMaster_WhatIsCoordofMaster", Logging.Debug);
                        if (!InnerspaceCommands.SlaveToMaster_WhatIsCoordofMaster()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        
                    }
                    break;

                case InnerspaceCommandsState.SlaveToMaster_WhatIsCurrentMissionAction:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.SlaveToMaster_WhatIsCurrentMissionAction", Logging.Debug);
                        if (!InnerspaceCommands.SlaveToMaster_WhatIsCurrentMissionAction()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.SlaveToMaster_WhatAmmoShouldILoad:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.SlaveToMaster_WhatAmmoShouldILoad", Logging.Debug);
                        if (!InnerspaceCommands.SlaveToMaster_WhatAmmoShouldILoad()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_SetDestinationLocationID:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_SetDestinationLocationID", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_SetDestinationLocationID()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_MasterIsWarpingTo:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_MasterIsWarpingTo", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_MasterIsWarpingTo()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_SlavesGotoBase:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_SlavesGotoBase", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_SlavesGotoBase()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_DoThisMissionAction:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_DoThisMissionAction", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_DoThisMissionAction()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_DoNotLootItemName:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_DoNotLootItemName", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_DoNotLootItemName()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                        
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_SetAutoStart:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_SetAutoStart", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_SetAutoStart()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_WhereAreYou:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_WhereAreYou", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_WhereAreYou()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_WhatAreYouShooting:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_WhatAreYouShooting", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_WhatAreYouShooting()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.MasterToSlaves_ShootThisEntityID:
                    if (!Cache.Instance.InWarp)
                    {
                        Logging.Log("InnerspaceCommands", "InnerspaceCommandsState.MasterToSlaves_ShootThisEntityID", Logging.Debug);
                        if (!InnerspaceCommands.MasterToSlaves_ShootThisEntityID()) return;
                        _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    }
                    break;

                case InnerspaceCommandsState.Done:

                    //_lastStatisticsAction = DateTime.UtcNow;
                    _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    break;

                default:

                    // Next state
                    _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.Idle;
                    break;
            }
        }
    }
}