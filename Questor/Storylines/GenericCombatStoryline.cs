﻿
using System;
using System.Collections.Generic;
using System.Linq;
using DirectEve;
using Questor.Modules.Actions;
using Questor.Modules.Activities;
using Questor.Modules.BackgroundTasks;
using Questor.Modules.Caching;
using Questor.Modules.Combat;
using Questor.Modules.Logging;
using Questor.Modules.Lookup;
using Questor.Modules.States;

namespace Questor.Storylines
{
    public class GenericCombatStoryline : IStoryline
    {
        private long _agentId;
        private readonly List<Ammo> _neededAmmo;

        //private readonly AgentInteraction _agentInteraction;
        //private readonly Arm _arm;
        private readonly CombatMissionCtrl _combatMissionCtrl;
        //private readonly Combat _combat;
        //private readonly Drones _drones;
        //private readonly Salvage _salvage;
        private readonly Statistics _statistics;

        private GenericCombatStorylineState _state;

        public GenericCombatStorylineState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public GenericCombatStoryline()
        {
            _neededAmmo = new List<Ammo>();

            //_agentInteraction = new AgentInteraction();
            //_arm = new Arm();
            //_combat = new Combat();
            //_drones = new Drones();
            //_salvage = new Salvage();
            _statistics = new Statistics();
            _combatMissionCtrl = new CombatMissionCtrl();

            Settings.Instance.SettingsLoaded += ApplySettings;
        }

        /// <summary>
        ///   Apply settings to the salvager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplySettings(object sender, EventArgs e)
        {
            Salvage.Ammo = Settings.Instance.Ammo;
            Salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
            Salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
            Salvage.LootEverything = Settings.Instance.LootEverything;
        }

        /// <summary>
        ///   We check what ammo we need by starting a conversation with the agent and load the appropriate ammo
        /// </summary>
        /// <returns></returns>
        public StorylineState Arm(Storyline storyline)
        {
            if (_agentId != Cache.Instance.CurrentStorylineAgentId)
            {
                _neededAmmo.Clear();
                _agentId = Cache.Instance.CurrentStorylineAgentId;

                AgentInteraction.AgentId = _agentId;
                AgentInteraction.ForceAccept = true; // This makes agent interaction skip the offer-check
                _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                AgentInteraction.Purpose = AgentInteractionPurpose.AmmoCheck;

                Modules.Actions.Arm.AgentId = _agentId;
                _States.CurrentArmState = ArmState.Idle;
                Modules.Actions.Arm.AmmoToLoad.Clear();

                //Questor.AgentID = _agentId;

                _statistics.AgentID = _agentId;

                CombatMissionCtrl.AgentId = _agentId;
                _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Start;

                //_States.CurrentCombatState = CombatState.CheckTargets;

                _States.CurrentDroneState = DroneState.WaitingForTargets;
            }

            try
            {
                if (!Interact())
                    return StorylineState.Arm;

                if (!LoadAmmo())
                    return StorylineState.Arm;

                // We are done, reset agent id
                _agentId = 0;

                return StorylineState.GotoAgent;
            }
            catch (Exception ex)
            {
                // Something went wrong!
                Logging.Log("GenericCombatStoryline", "Something went wrong, blacklist this agent [" + ex.Message + "]", Logging.Orange);
                return StorylineState.BlacklistAgent;
            }
        }

        /// <summary>
        ///   Interact with the agent so we know what ammo to bring
        /// </summary>
        /// <returns>True if interact is done</returns>
        private bool Interact()
        {
            // Are we done?
            if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
                return true;

            if (AgentInteraction.Agent == null)
                throw new Exception("Invalid agent");

            // Start the conversation
            if (_States.CurrentAgentInteractionState == AgentInteractionState.Idle)
                _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;

            // Interact with the agent to find out what ammo we need
            AgentInteraction.ProcessState();

            if (_States.CurrentAgentInteractionState == AgentInteractionState.DeclineMission)
            {
                if (AgentInteraction.Agent.Window != null)
                    AgentInteraction.Agent.Window.Close();
                Logging.Log("GenericCombatStoryline", "Mission offer is in a Low Security System", Logging.Orange); //do storyline missions in lowsec get blacklisted by: "public StorylineState Arm(Storyline storyline)"?
                throw new Exception("Low security systems");
            }

            if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
            {
                Modules.Actions.Arm.AmmoToLoad.Clear();
                Modules.Actions.Arm.AmmoToLoad.AddRange(AgentInteraction.AmmoToLoad);
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Load the appropriate ammo
        /// </summary>
        /// <returns></returns>
        private bool LoadAmmo()
        {
            if (_States.CurrentArmState == ArmState.Done)
                return true;

            if (_States.CurrentArmState == ArmState.Idle)
                _States.CurrentArmState = ArmState.Begin;

            Modules.Actions.Arm.ProcessState();

            if (_States.CurrentArmState == ArmState.Done)
            {
                _States.CurrentArmState = ArmState.Idle;
                return true;
            }

            return false;
        }

        /// <summary>
        ///   We have no pre-accept steps
        /// </summary>
        /// <returns></returns>
        public StorylineState PreAcceptMission(Storyline storyline)
        {
            // Not really a step is it? :)
            _state = GenericCombatStorylineState.WarpOutStation;
            return StorylineState.AcceptMission;
        }

        /// <summary>
        ///   Do a mini-questor here (goto mission, execute mission, goto base)
        /// </summary>
        /// <returns></returns>
        public StorylineState ExecuteMission(Storyline storyline)
        {
            switch (_state)
            {
                case GenericCombatStorylineState.WarpOutStation:
                    DirectBookmark warpOutBookMark = Cache.Instance.BookmarksByLabel(Settings.Instance.UndockBookmarkPrefix ?? "").OrderByDescending(b => b.CreatedOn).FirstOrDefault(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);
                    long solarid = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (warpOutBookMark == null)
                    {
                        Logging.Log("GenericCombatStoryline.WarpOut", "No Bookmark", Logging.Orange);
                        _state = GenericCombatStorylineState.GotoMission;
                        break;
                    }

                    if (warpOutBookMark.LocationId == solarid)
                    {
                        if (Traveler.Destination == null)
                        {
                            Logging.Log("GenericCombatStoryline.WarpOut", "Warp at " + warpOutBookMark.Title, Logging.White);
                            Traveler.Destination = new BookmarkDestination(warpOutBookMark);
                            Cache.Instance.DoNotBreakInvul = true;
                        }

                        Traveler.ProcessState();
                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            Logging.Log("GenericCombatStoryline.WarpOut", "Safe!", Logging.White);
                            Cache.Instance.DoNotBreakInvul = false;
                            _state = GenericCombatStorylineState.GotoMission;
                            Traveler.Destination = null;
                            break;
                        }

                        break;
                    }
                    
                    Logging.Log("GenericCombatStoryline.WarpOut", "No Bookmark in System", Logging.White);
                    _state = GenericCombatStorylineState.GotoMission;
                    break;

                case GenericCombatStorylineState.GotoMission:
                    MissionBookmarkDestination missionDestination = Traveler.Destination as MissionBookmarkDestination;
                    //
                    // if we have no destination yet... OR if missionDestination.AgentId != storyline.CurrentStorylineAgentId
                    //
                    //if (missionDestination != null) Logging.Log("GenericCombatStoryline: missionDestination.AgentId [" + missionDestination.AgentId + "] " + "and storyline.CurrentStorylineAgentId [" + storyline.CurrentStorylineAgentId + "]");
                    //if (missionDestination == null) Logging.Log("GenericCombatStoryline: missionDestination.AgentId [ NULL ] " + "and storyline.CurrentStorylineAgentId [" + storyline.CurrentStorylineAgentId + "]");
                    if (missionDestination == null || missionDestination.AgentId != Cache.Instance.CurrentStorylineAgentId) // We assume that this will always work "correctly" (tm)
                    {
                        string nameOfBookmark ="";
                        if (Settings.Instance.EveServerName == "Tranquility") nameOfBookmark = "Encounter";
                        if (Settings.Instance.EveServerName == "Serenity") nameOfBookmark = "遭遇战";
                        if (nameOfBookmark == "") nameOfBookmark = "Encounter";
                        Logging.Log("GenericCombatStoryline", "Setting Destination to 1st bookmark from AgentID: [" + Cache.Instance.CurrentStorylineAgentId + "] with [" + nameOfBookmark + "] in the title", Logging.White);
                        Traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(Cache.Instance.CurrentStorylineAgentId, nameOfBookmark));
                    }

                    if (Cache.Instance.PotentialCombatTargets.Any())
                    {
                        Logging.Log("GenericCombatStoryline", "Priority targets found while traveling, engaging!", Logging.White);
                        Combat.ProcessState();
                    }

                    Traveler.ProcessState();
                    if (_States.CurrentTravelerState == TravelerState.AtDestination)
                    {
                        _state = GenericCombatStorylineState.ExecuteMission;

                        //_States.CurrentCombatState = CombatState.CheckTargets;
                        Traveler.Destination = null;
                    }
                    break;

                case GenericCombatStorylineState.ExecuteMission:
                    Combat.ProcessState();
                    Drones.ProcessState();
                    Salvage.ProcessState();
                    _combatMissionCtrl.ProcessState();

                    // If we are out of ammo, return to base, the mission will fail to complete and the bot will reload the ship
                    // and try the mission again
                    if (_States.CurrentCombatState == CombatState.OutOfAmmo)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();

                        Logging.Log("GenericCombatStoryline", "Out of Ammo!", Logging.Orange);
                        return StorylineState.ReturnToAgent;
                    }

                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Done)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                        return StorylineState.ReturnToAgent;
                    }

                    // If in error state, just go home and stop the bot
                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();

                        Logging.Log("MissionController", "Error", Logging.Red);
                        return StorylineState.ReturnToAgent;
                    }
                    break;
            }

            return StorylineState.ExecuteMission;
        }
    }
}