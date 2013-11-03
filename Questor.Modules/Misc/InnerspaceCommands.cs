
using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public static void CreateLavishCommands()
        {
            if (Settings.Instance.UseInnerspace)
            {
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
            Logging.Log("InnerspaceCommands", "ListClassInstanceInfo                        - Logs Class Instance Info", Logging.White);
            return 0;
        }

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
                if (Cache.Instance.Entities.Any())
                {
                    if (Cache.Instance.Entities.Any(i => i.Name == stringEntitiesToAdd))
                    {
                        IEnumerable<EntityCache> entitiesToAdd = Cache.Instance.Entities.Where(i => i.Name == stringEntitiesToAdd).ToList();
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
                    if (Cache.Instance.Entities.Any(i => i.IsOnGridWithMe))
                    {
                        EntitiesOnGridCount = Cache.Instance.Entities.Count(i => i.IsOnGridWithMe);
                    }

                    int EntitiesCount = 0;
                    if (Cache.Instance.Entities.Any())
                    {
                        EntitiesCount = Cache.Instance.Entities.Count();
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

            Logging.Log("Statistics", "Entering StatisticsState.LogAllEntities", Logging.Debug);
            _States.CurrentInnerspaceCommandsState = InnerspaceCommandsState.LogAllEntities;
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
                    Logging.Log(icount.ToString(), thing.Name + "[" + Math.Round(thing.Distance / 1000, 0) + "k] GroupID[" + thing.GroupId + "] ID[" + Cache.Instance.MaskedID(thing.Id) + "] isSentry[" + thing.IsSentry + "] IsHVT[" + thing.IsHighValueTarget + "] IsLVT[" + thing.IsLowValueTarget + "] IsIgnored[" + thing.IsIgnored + "]", Logging.Debug);
                }
            }
            Logging.Log("Entities", "--------------------------- Done  (listed above)-----------------------------", Logging.Yellow);

            return true;
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
                        InnerspaceCommands.LogEntities(Cache.Instance.Entities.Where(i => i.IsOnGridWithMe).ToList());
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