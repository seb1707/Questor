// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.BackgroundTasks
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;
    using Questor.Modules.States;
    using DirectEve;

    public class ManageFleet
    {
        private readonly DateTime _lastSessionChange = Cache.Instance.StartTime;

        private DateTime _lastPulse = DateTime.UtcNow;
        private int _fleetPulseTime = 1000;
        private List<DirectCharacter> _characterNamesForMasterToInviteToFleetInLocal { get; set; }
        private List<DirectFleetMember> _MyCurrentFleetMembers;
        private Dictionary<DirectCharacter, DateTime> _InvitesAlreadySentToTheseCharacters { get; set; }
        
        private DateTime _nextManageFleetAttempt = DateTime.UtcNow;
        
        public ManageFleet()
        {
            _States.CurrentManageFleetState = ManageFleetState.Idle;
            _InvitesAlreadySentToTheseCharacters = null;
        }

        private bool HowManyFleetMembers()
        {
            try
            {
                if (!Cache.Instance.InSpace && !Cache.Instance.InStation)
                {
                    return false;
                }

                if (Cache.Instance.InSpace && DateTime.UtcNow.AddSeconds(5) < Cache.Instance.LastInStation)
                {
                    return false;
                }

                if (Cache.Instance.InStation && DateTime.UtcNow.AddSeconds(5) < Cache.Instance.LastInSpace)
                {
                    return false;
                }

                if (Cache.Instance.DirectEve.Session.FleetId != null)
                {
                    _MyCurrentFleetMembers = Cache.Instance.DirectEve.GetFleetMembers;
                    Logging.Log("HowManyFleetMembers", "_MyCurrentFleetMembers [" + _MyCurrentFleetMembers.Count() + "]", Logging.Debug);

                    int i = 0;
                    foreach (DirectFleetMember _fleetMember in _MyCurrentFleetMembers)
                    {
                        i++;
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.CharacterId + "]", Logging.Debug);
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.Job + "]", Logging.Debug);
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.Role + "]", Logging.Debug);
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.ShipTypeID + "]", Logging.Debug);
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.SolarSystemID + "]", Logging.Debug);
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.SquadID + "]", Logging.Debug);
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.WingID + "]", Logging.Debug);
                        //if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.Skills + " HUGE list]", Logging.Debug);
                        //if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("FleetMemberDetails", "[" + _fleetMember.Name + "]  [" + _fleetMember.WarpToMember() + " not a string ]", Logging.Debug);
                        continue;
                    }
                    return true;
                }
            }
            catch (Exception exception)
            {
                Logging.Log("HowManyFleetMembers", "Exception: [" + exception + "]", Logging.Debug); 
            }
            
            //Logging.Log("HowManyFleetMembers", "We are not yet in a fleet. FleetId was null]", Logging.Debug);
            return true;
        }

        private bool HowManyConfiguredFleetMembersInChannel(string WindowTitle)
        {
            if (DateTime.UtcNow < Cache.Instance.LastSessionChange.AddSeconds(10)) return false;

            DirectWindow _window = Cache.Instance.Windows.FirstOrDefault(t => t.Name.Contains("chatchannel_") && t.Caption.ToLower().Contains(WindowTitle.ToLower()));
            if (_window != null)
            {
                if (Settings.Instance.DebugChat)
                {
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.Caption    [" + _window.Caption + "]", Logging.Debug);
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.Id         [" + _window.Id + "]", Logging.Debug);
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.IsDialog   [" + _window.IsDialog + "]", Logging.Debug);
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.IsKillable [" + _window.IsKillable + "]", Logging.Debug);
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.IsModal    [" + _window.IsModal + "]", Logging.Debug);
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.Type       [" + _window.Type + "]", Logging.Debug);
                    Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.ViewMode   [" + _window.ViewMode + "]", Logging.Debug);
                    if (_window.Html != null) Logging.Log("_window", "_window.Name [" + WindowTitle + "] _window.Html [" + _window.Html + "]", Logging.Debug);    
                }

                DirectChatWindow _chatWindow = (DirectChatWindow)_window;

                List<DirectCharacter> chatMembers = _chatWindow.Members;
                //List<DirectChatMessage> corp_messages = _chatWindow.Messages;
                if (Settings.Instance.DebugChat)
                {
                    Logging.Log("_window", "_chatWindow.Caption           [" + _chatWindow.Caption + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.ChannelId         [" + _chatWindow.ChannelId + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.DustMemberCount   [" + _chatWindow.DustMemberCount + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.EveMemberCount    [" + _chatWindow.EveMemberCount + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.Id                [" + _chatWindow.Id + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.MemberCount       [" + _chatWindow.MemberCount + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.Members.Count()   [" + _chatWindow.Members.Count() + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.Messages.Count()  [" + _chatWindow.Messages.Count() + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.Messages.Type     [" + _chatWindow.Type + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.Messages.Usermode [" + _chatWindow.Usermode + "]", Logging.Debug);
                    Logging.Log("_window", "_chatWindow.Messages.Viewmode [" + _chatWindow.ViewMode + "]", Logging.Debug);
                }

                if (_chatWindow.Members.Count() > 1)
                {
                    _characterNamesForMasterToInviteToFleetInLocal = chatMembers.Where(m => Settings.Instance.CharacterNamesForMasterToInviteToFleet.Contains(m.Name)).ToList();
                    if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet._characterNamesForMasterToInviteToFleetInLocal", "Count [" + _characterNamesForMasterToInviteToFleetInLocal.Count() + "]", Logging.White);

                    if (_characterNamesForMasterToInviteToFleetInLocal.Any())
                    {
                        int i = 0;
                        foreach (DirectCharacter _CharacterToInvite in _characterNamesForMasterToInviteToFleetInLocal)
                        {
                            i++;
                            if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet._CharacterToInvite", "[" + i + "] " + _CharacterToInvite.Name, Logging.Debug);
                            continue;
                        }

                        return true;    
                    }
                    //
                    // no characters in local in our configured invite list
                    //
                }

                Logging.Log("ManageFleet.HowManyConfiguredFleetMembersInChannel", "Channel [" + _chatWindow.Name + "] does not contain more than 1 (self) members.", Logging.Debug);
            }

            Logging.Log("ManageFleet.HowManyConfiguredFleetMembersInChannel", "Channel [" + WindowTitle + "] was not found", Logging.Debug);
            return false;
        }

        private bool InviteConfiguredFleetMembersInLocal()
        {
            if (_characterNamesForMasterToInviteToFleetInLocal.Any())
            {
                int i = 0;
                foreach (DirectCharacter _CharacterToInvite in _characterNamesForMasterToInviteToFleetInLocal)
                {
                    i++;
                    if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet._CharacterToInvite", "[" + i + "] " + _CharacterToInvite.Name, Logging.Debug);
                    
                    if (Cache.Instance.DirectEve.Session.FleetId == null) //fleet does not yet exist, invite someone to form the fleet
                    {
                        if (_InvitesAlreadySentToTheseCharacters.ContainsKey(_CharacterToInvite))
                        {
                            
                        }
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "[" + i + "] Fleet does not yet exist. Forming Fleet with [" + _CharacterToInvite.Name + "]" , Logging.Debug);
                        _InvitesAlreadySentToTheseCharacters.Add(_CharacterToInvite,DateTime.UtcNow);
                        Cache.Instance.DirectEve.InviteToFleet(_CharacterToInvite.CharacterId);
                        return false;
                    }

                    if (Cache.Instance.DirectEve.Session.FleetId != null) //we are in a fleet
                    {
                        if (_MyCurrentFleetMembers.FirstOrDefault(n => n.Name == _CharacterToInvite.Name) == null) //the character _CharacterToInvite.Name is not yet in fleet
                        {
                            if (_InvitesAlreadySentToTheseCharacters.ContainsKey(_CharacterToInvite)) //We have already sent an invite to this toon
                            {
                                if (DateTime.UtcNow > _InvitesAlreadySentToTheseCharacters[_CharacterToInvite].AddSeconds(120)) //the invite as sent over 120 sec ago, resend
                                {
                                    if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "[" + i + "] Inviting [" + _CharacterToInvite.Name + "] to fleet [" + Settings.Instance.FleetName + "]", Logging.Debug);
                                    _InvitesAlreadySentToTheseCharacters.Remove(_CharacterToInvite);
                                    _InvitesAlreadySentToTheseCharacters.Add(_CharacterToInvite, DateTime.UtcNow);
                                    Cache.Instance.DirectEve.InviteToFleet(_CharacterToInvite.CharacterId);
                                    return false;
                                }

                                //
                                // we have already sent an invite and it was not aged to 120 sec yet, assume they will accept the invite when ready (do nothing until over 120sec)
                                //
                                return false;
                            }

                            //
                            // they are not yet in fleet and  we havent yet sent them an invite to fleet. Do so now.
                            //
                            if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "[" + i + "] Inviting [" + _CharacterToInvite.Name + "] to fleet [" + Settings.Instance.FleetName + "]", Logging.Debug);
                            _InvitesAlreadySentToTheseCharacters.Add(_CharacterToInvite, DateTime.UtcNow);
                            Cache.Instance.DirectEve.InviteToFleet(_CharacterToInvite.CharacterId);
                            return false;
                        }

                        continue; //the _CharacterToInvite is already in our fleet, try the next character
                    }

                    continue; //we should never get here, if we do assume we should try the next character.
                }

                return true; //we have made it through the list of character to try to invite to fleet, we are done (for now)
            }

            return true; //no characters in local are in your configured list of characters we can invite to fleet.
        }

        public void ProcessState()
        {
            // Only pulse state changes every 1.5s
            if (DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < _fleetPulseTime)
                return;
            _lastPulse = DateTime.UtcNow;

            if (DateTime.UtcNow.Subtract(_lastSessionChange).TotalSeconds < 10)
            {
                if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "we just completed a session change less than 10 seconds ago... waiting.", Logging.White);
                _nextManageFleetAttempt = DateTime.UtcNow.AddSeconds(3);
                return;
            }
            
            switch (_States.CurrentManageFleetState)
            {
                case ManageFleetState.Idle:
                    if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "ManageFleetState.Idle", Logging.Debug);
                    _States.CurrentManageFleetState = ManageFleetState.HowManyConfiguredFleetMembers;
                    break;

                case ManageFleetState.HowManyConfiguredFleetMembers:
                    if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet","HowManyConfiguredFleetMembers [" + Settings.Instance.CharacterNamesForMasterToInviteToFleet.Count() + "]",Logging.Debug);
                    _States.CurrentManageFleetState = ManageFleetState.HowManyFleetMembers;
                    break;

                case ManageFleetState.HowManyFleetMembers:
                    if (!HowManyFleetMembers()) return;
                    if (Cache.Instance.DirectEve.Session.FleetId != null)
                    {
                        if (_MyCurrentFleetMembers.Any() && _MyCurrentFleetMembers.Count() >= Settings.Instance.CharacterNamesForMasterToInviteToFleet.Count())
                        {
                            //
                            // if everyone is already online and in fleet then idle for 30 sec before checking again
                            //
                            if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "HowManyFleetMembers: Fleet# [" + _MyCurrentFleetMembers.Count() + "] Configured# [" + Settings.Instance.CharacterNamesForMasterToInviteToFleet.Count() + "] waiting 30 sec before retrying", Logging.Debug);
                            _nextManageFleetAttempt = DateTime.UtcNow.AddSeconds(30);
                            _States.CurrentManageFleetState = ManageFleetState.Done;
                            return;
                        }
                        //
                        // here a fleet would exist but have no members?!
                        //
                    }
                    //
                    // no fleet exists yet
                    //
                    _States.CurrentManageFleetState = ManageFleetState.HowManyConfiguredFleetMembersInLocal;
                    break;

                case ManageFleetState.HowManyConfiguredFleetMembersInLocal:
                    if (!HowManyConfiguredFleetMembersInChannel("local")) return;
                    if (!_characterNamesForMasterToInviteToFleetInLocal.Any())
                    {
                        //
                        // if no one in the list of characters to invite to fleet is in local then idle for 15 sec before checking again
                        //
                        if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "HowManyConfiguredFleetMembersInLocal [" + _characterNamesForMasterToInviteToFleetInLocal.Count() + "] waiting 15 sec before retrying", Logging.Debug);
                        _nextManageFleetAttempt = DateTime.UtcNow.AddSeconds(15);
                        _States.CurrentManageFleetState = ManageFleetState.Done;
                    }

                    _States.CurrentManageFleetState = ManageFleetState.InviteConfiguredFleetMembersInLocal;
                    break;
                    
                case ManageFleetState.InviteConfiguredFleetMembersInLocal:
                    if (!InviteConfiguredFleetMembersInLocal()) return;
                    if (Cache.Instance.DirectEve.Session.FleetId != null)
                    {
                        if (_MyCurrentFleetMembers.Any() && _MyCurrentFleetMembers.Count() >= Settings.Instance.CharacterNamesForMasterToInviteToFleet.Count())
                        {
                            //
                            // if everyone is already online and in fleet then idle for 30 sec before checking again
                            //
                            if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("ManageFleet", "HowManyFleetMembers: Fleet# [" + _MyCurrentFleetMembers.Count() + "] Configured# [" + Settings.Instance.CharacterNamesForMasterToInviteToFleet.Count() + "] waiting 30 sec before retrying", Logging.Debug);
                            _nextManageFleetAttempt = DateTime.UtcNow.AddSeconds(30);
                            _States.CurrentManageFleetState = ManageFleetState.Done;
                            return;
                        }
                        //
                        // here a fleet would exist but have no members?!
                        //
                    }

                    _States.CurrentManageFleetState = ManageFleetState.Idle;
                    break;

                case ManageFleetState.KickSpecificFleetMember:
                    //if (!KickSpecificFleetMember()) return;
                    break;

                case ManageFleetState.KickUnauthorizedFleetMembers:
                    //if (!KickUnauthorizedFleetMembers()) return;
                    break;

                case ManageFleetState.LeaveFleet:
                    //if (!LeaveFleet()) return;
                    break;

                case ManageFleetState.PassBossToMaster:
                    //if (!PassBossToMaster()) return;
                    break;

                case ManageFleetState.Done:
                    //
                    // this is fleet management there is no done, ever.
                    //
                    _fleetPulseTime = 1000;
                    _States.CurrentManageFleetState = ManageFleetState.Idle;
                    break;

            }
        }
    }
}