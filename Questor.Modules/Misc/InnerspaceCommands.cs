
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
                LavishScript.Commands.AddCommand("QuestorCommands", ListQuestorCommands);
                LavishScript.Commands.AddCommand("QuestorEvents", ListQuestorEvents);
                LavishScript.Commands.AddCommand("IfInPodSwitchToNoobShiporShuttle", IfInPodSwitchToNoobShiporShuttle);
                LavishScript.Commands.AddCommand("SetDestToSystem", SetDestToSystem);
                LavishScript.Commands.AddCommand("LogAllEntities", LogAllEntities);
            }
        }

        private static int LogAllEntities(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("QuestorUI", "SetQuestorStatetoCloseQuestor - Changes the QuestorState to CloseQuestor which will GotoBase and then Exit", Logging.White);
                return -1;
            }

            _States.CurrentStatisticsState = StatisticsState.PocketLog;
            return 0;
        }

        private static int SetAutoStart(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("QuestorUI", "SetAutoStart true|false", Logging.White);
                return -1;
            }

            Settings.Instance.AutoStart = value;

            Logging.Log("QuestorUI", "AutoStart is turned " + (value ? "[on]" : "[off]"), Logging.White);
            return 0;
        }

        private static int SetDisable3D(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("QuestorUI", "SetDisable3D true|false", Logging.White);
                return -1;
            }

            Settings.Instance.Disable3D = value;

            Logging.Log("QuestorUI", "Disable3D is turned " + (value ? "[on]" : "[off]"), Logging.White);
            return 0;
        }

        private static int SetExitWhenIdle(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("QuestorUI", "SetExitWhenIdle true|false", Logging.White);
                Logging.Log("QuestorUI", "Note: AutoStart is automatically turned off when ExitWhenIdle is turned on", Logging.White);
                return -1;
            }

            Cache.Instance.ExitWhenIdle = value;

            Logging.Log("QuestorUI", "ExitWhenIdle is turned " + (value ? "[on]" : "[off]"), Logging.White);

            if (value && Settings.Instance.AutoStart)
            {
                Settings.Instance.AutoStart = false;
                Logging.Log("QuestorUI", "AutoStart is turned [off]", Logging.White);
            }
            return 0;
        }

        private static int SetQuestorStatetoCloseQuestor(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("QuestorUI", "SetQuestorStatetoCloseQuestor - Changes the QuestorState to CloseQuestor which will GotoBase and then Exit", Logging.White);
                return -1;
            }

            Settings.Instance.AutoStart = false;
            _States.CurrentQuestorState = QuestorState.CloseQuestor;

            Logging.Log("QuestorUI", "QuestorState is now: CloseQuestor ", Logging.White);
            return 0;
        }

        private static int SetQuestorStatetoIdle(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("QuestorUI", "SetQuestorStatetoIdle - Changes the QuestorState to Idle", Logging.White);
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


            Logging.Log("QuestorUI", "QuestorState is now: Idle ", Logging.White);
            return 0;
        }

        private static int SetDestToSystem(string[] args)
        {
            long value;
            if (args.Length != 2 || !long.TryParse(args[1], out value))
            {
                Logging.Log("QuestorUI", "SetDestToSystem - Sets destination to the locationID specified", Logging.White);
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
                Logging.Log("QuestorUI", "SetCombatMissionsBehaviorStatetoGotoBase - Changes the CombatMissionsBehaviorState to GotoBase", Logging.White);
                return -1;
            }

            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;

            Logging.Log("QuestorUI", "CombatMissionsBehaviorState is now: GotoBase ", Logging.White);
            return 0;
        }

        private static int SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("QuestorUI", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase - Changes the SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase to GotoBase", Logging.White);
                return -1;
            }

            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;

            Logging.Log("QuestorUI", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase is now: GotoBase ", Logging.White);
            return 0;
        }

        private static int ListQuestorCommands(string[] args)
        {
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", "Questor commands you can run from innerspace", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", "SetAutoStart                                 - SetAutoStart true|false", Logging.White);
            Logging.Log("QuestorUI", "SetDisable3D                                 - SetDisable3D true|false", Logging.White);
            Logging.Log("QuestorUI", "SetExitWhenIdle                              - SetExitWhenIdle true|false", Logging.White);
            Logging.Log("QuestorUI", "SetQuestorStatetoCloseQuestor                - SetQuestorStatetoCloseQuestor true", Logging.White);
            Logging.Log("QuestorUI", "SetQuestorStatetoIdle                        - SetQuestorStatetoIdle true", Logging.White);
            Logging.Log("QuestorUI", "SetCombatMissionsBehaviorStatetoGotoBase     - SetCombatMissionsBehaviorStatetoGotoBase true", Logging.White);
            Logging.Log("QuestorUI", "SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase     - SetDedicatedBookmarkSalvagerBehaviorStatetoGotoBase true", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", "QuestorCommands                              - (this command) ", Logging.White);
            Logging.Log("QuestorUI", "QuestorEvents                                - Lists the available InnerSpace Events you can listen for ", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            return 0;
        }

        private static int ListQuestorEvents(string[] args)
        {
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", "Questor Events you can listen for from an innerspace script", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            Logging.Log("QuestorUI", "QuestorIdle                                   - This Event fires when entering the QuestorState Idle ", Logging.White);
            Logging.Log("QuestorUI", "QuestorState                                  - This Event fires when the State changes", Logging.White);
            Logging.Log("QuestorUI", "QuestorCombatMissionsBehaviorState            - This Event fires when the State changes", Logging.White);
            Logging.Log("QuestorUI", "QuestorDedicatedBookmarkSalvagerBehaviorState - This Event fires when the State changes", Logging.White);
            Logging.Log("QuestorUI", "QuestorAutoStartState                         - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorExitWhenIdleState                      - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorDisable3DState                         - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorPanicState                             - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorPausedState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorDronesState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorCombatState                            - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", "QuestorTravelerState                          - This Event fires when the State changes ", Logging.White);
            Logging.Log("QuestorUI", " ", Logging.White);
            return 0;
        }

        private static int IfInPodSwitchToNoobShiporShuttle(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("QuestorUI", "IfInPodSwitchToNoobShiporShuttle - If the toon is in a pod switch to a noobship or shuttle if avail (otherwise do nothing)", Logging.White);
                return -1;
            }

            //_States.CurrentBackgroundBehaviorState = BackgroundBehaviorState.SwitchToNoobShip1;

            Logging.Log("QuestorUI", "CurrentBackgroundBehaviorState is now: SwitchToNoobShip1 ", Logging.White);
            return 0;
        }

    }
}