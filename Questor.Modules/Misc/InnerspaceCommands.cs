
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
                LavishScript.Commands.AddCommand("SetAutoStart", SetAutoStart);
                LavishScript.Commands.AddCommand("SetDisable3D", SetDisable3D);
                LavishScript.Commands.AddCommand("SetExitWhenIdle", SetExitWhenIdle);
                LavishScript.Commands.AddCommand("SetQuestorStatetoCloseQuestor", SetQuestorStatetoCloseQuestor);
                LavishScript.Commands.AddCommand("SetQuestorStatetoIdle", SetQuestorStatetoIdle);
                LavishScript.Commands.AddCommand("SetCombatMissionsBehaviorStatetoGotoBase", SetCombatMissionsBehaviorStatetoGotoBase);
                LavishScript.Commands.AddCommand("SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase", SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase);
                LavishScript.Commands.AddCommand("QuestorEvents", ListQuestorEvents);
                LavishScript.Commands.AddCommand("IfInPodSwitchToNoobShiporShuttle", IfInPodSwitchToNoobShiporShuttle);
                LavishScript.Commands.AddCommand("SetDestToSystem", SetDestToSystem);
                LavishScript.Commands.AddCommand("LogAllEntities", LogAllEntities);
                LavishScript.Commands.AddCommand("ModuleInfo", ModuleInfo);
                LavishScript.Commands.AddCommand("ListIgnoredTargets", ListIgnoredTargets);
                LavishScript.Commands.AddCommand("ListPrimaryWeaponPriorityTargets", ListPrimaryWeaponPriorityTargets);
                LavishScript.Commands.AddCommand("ListPWPT", ListPrimaryWeaponPriorityTargets);
                LavishScript.Commands.AddCommand("ListDronePriorityTargets", ListDronePriorityTargets);
                LavishScript.Commands.AddCommand("ListDPT", ListDronePriorityTargets);
                LavishScript.Commands.AddCommand("ListTargets", ListTargetedandTargeting);
                LavishScript.Commands.AddCommand("AddIgnoredTarget", AddIgnoredTarget);
                LavishScript.Commands.AddCommand("RemoveIgnoredTarget", RemoveIgnoredTarget);
                LavishScript.Commands.AddCommand("ListClassInstanceInfo", ListClassInstanceInfo);
                LavishScript.Commands.AddCommand("ListQuestorCommands", ListQuestorCommands);
                LavishScript.Commands.AddCommand("QuestorCommands", ListQuestorCommands);
                LavishScript.Commands.AddCommand("Help", ListQuestorCommands);
                LavishScript.ExecuteCommand("alias 1 " + Settings.Instance.LoadQuestorDebugInnerspaceCommand);  //"dotnet q1 questor.exe");
                LavishScript.ExecuteCommand("alias 2 " + Settings.Instance.UnLoadQuestorDebugInnerspaceCommand);  //"dotnet -unload q1");
            }
        }

        private static int ListClassInstanceInfo(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListClassInstanceInfo - Lists Class Instance Count for These Classes", Logging.White);
                return -1;
            }

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

            _States.CurrentStatisticsState = StatisticsState.ListTargetedandTargeting;
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
            if (Cache.Instance.IgnoreTargets.Contains(ignoreThese))
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
            if (!Cache.Instance.IgnoreTargets.Contains(unIgnoreThese))
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

            _States.CurrentStatisticsState = StatisticsState.ListIgnoredTargets;
            return 0;
        }

        private static int ListPrimaryWeaponPriorityTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListPrimaryWeaponPriorityTargets - Lists Primary Weapon Priority Targets", Logging.White);
                return -1;
            }

            _States.CurrentStatisticsState = StatisticsState.ListPrimaryWeaponPriorityTargets;
            return 0;
        }

        private static int ListDronePriorityTargets(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "ListDronePriorityTargets - Lists DronePriorityTargets", Logging.White);
                return -1;
            }

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

            _States.CurrentStatisticsState = StatisticsState.ModuleInfo;
            return 0;
        }

        private static int LogAllEntities(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("InnerspaceCommands", "LogAllEntities - Logs Entities on grid", Logging.White);
                return -1;
            }

            _States.CurrentStatisticsState = StatisticsState.LogAllEntities;
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
            Logging.Log("InnerspaceCommands", "LogAllEntities                               - Logs Entities on Grid", Logging.White);
            Logging.Log("InnerspaceCommands", "ListIgnoredTargets                           - Logs the contents of the IgnoredTargets List", Logging.White);
            Logging.Log("InnerspaceCommands", "AddIgnoredTarget                             - Add name to the IgnoredTarget List", Logging.White);
            Logging.Log("InnerspaceCommands", "RemoveIgnoredTarget                          - Remove name to the IgnoredTarget List", Logging.White);
            
            Logging.Log("InnerspaceCommands", "ModuleInfo                                   - Logs Module Info of My Current Ship", Logging.White);
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

    }
}