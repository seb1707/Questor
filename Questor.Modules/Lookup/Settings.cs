﻿// ------------------------------------------------------------------------------
// <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
// Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
// Please look in the accompanying license.htm file for the license that
// applies to this source code. (a copy can also be found at:
// http://www.thehackerwithin.com/license.htm)
// </copyright>
// -------------------------------------------------------------------------------

using System.Linq;
using System.Threading;

namespace Questor.Modules.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;
    using System.Globalization;
    using Questor.Modules.Actions;
    using Questor.Modules.BackgroundTasks;
    using Questor.Modules.Caching;
    using Questor.Modules.Logging;
    using Questor.Modules.States;
    using System.Xml;

    public class Settings
    {
        /// <summary>
        /// Singleton implementation
        /// </summary>
        public static Settings Instance = new Settings();
        public string CharacterName;
        private DateTime _lastModifiedDateOfMySettingsFile;
        private DateTime _lastModifiedDateOfCommonSettingsFile;
        private int SettingsLoadedICount = 0;

        public static int SettingsInstances = 0;

        public Settings()
        {
            try
            {
                Ammo = new List<Ammo>();
                ItemsBlackList = new List<int>();
                WreckBlackList = new List<int>();
                ListOfAgents = new List<AgentsList>();
                ListofFactionFittings = new List<FactionFitting>();
                ListOfMissionFittings = new List<MissionFitting>();
                MissionBlacklist = new List<string>();
                MissionGreylist = new List<string>();
                CharacterNamesForMasterToInviteToFleet = new List<string>();

                FactionBlacklist = new List<string>();
                UseFittingManager = true;
                DefaultFitting = new FactionFitting();
                Interlocked.Increment(ref SettingsInstances);
            }
            catch (Exception exception)
            {
                Logging.Log("Settings Initialization", "Exception: [" + exception + "]", Logging.Red);
                return;
            }
        }

        ~Settings()
        {
            Interlocked.Decrement(ref SettingsInstances);
        }

        public bool AtLoginScreen { get; set; }
        public string LoginUsername;
        public string LoginCharacter;

        public bool CharacterXMLExists = true;
        public bool CommonXMLExists = false;
        public bool SchedulesXMLExists = true;
        public bool EVEMemoryManager = false;
        public long MemoryManagerTrimThreshold = 524288000;
        public bool FactionXMLExists = true;
        public bool QuestorStatisticsExists = true;
        public bool QuestorSettingsExists = true;
        public bool QuestorManagerExists = true;

        public string characterNameForLogs = "NoCharacterNameDefinedYet";
            
        //
        // Debug Variables
        //
        public bool DebugActivateGate { get; set; }
        public bool DebugActivateWeapons { get; set; }
        public bool DebugActivateBastion { get; set; }
        public bool DebugAddDronePriorityTarget { get; set; }
        public bool DebugAddPrimaryWeaponPriorityTarget { get; set; }
        public bool DebugAgentInteractionReplyToAgent { get; set; }
        public bool DebugAllMissionsOnBlackList { get; set; }
        public bool DebugAllMissionsOnGreyList { get; set; }
        public bool DebugAmmo { get; set; }
        public bool DebugArm { get; set; }
        public bool DebugAttachVSDebugger { get; set; }
        public bool DebugAutoStart { get; set; }
        public bool DebugBlackList { get; set; }
        public bool DebugCargoHold { get; set; }
        public bool DebugChat { get; set; }
        public bool DebugCleanup { get; set; }
        public bool DebugClearPocket { get; set; }
        public bool DebugCourierMissions { get; set; }
        public bool DebugDecline { get; set; }
        public bool DebugDefense { get; set; }
        public bool DebugDisableCleanup { get; set; }
        public bool DebugDisableCombatMissionsBehavior { get; set; }
        public bool DebugDisableCombatMissionCtrl { get; set; }
        public bool DebugDisableCombat { get; set; }
        public bool DebugDisableDrones { get; set; }
        public bool DebugDisablePanic { get; set; }
        public bool DebugDisableSalvage { get; set; }
        public bool DebugDisableTargetCombatants { get; set; }
        public bool DebugDisableGetBestTarget { get; set; }
        public bool DebugDisableGetBestDroneTarget { get; set; }
        public bool DebugDisableNavigateIntoRange { get; set; }
        public bool DebugDoneAction { get; set; }
        public bool DebugDrones { get; set; }
        public bool DebugDroneHealth { get; set; }
        public bool DebugEachWeaponsVolleyCache { get; set; }
        public bool DebugEntityCache { get; set; }
        public bool DebugExceptions { get; set; }
        public bool DebugFittingMgr { get; set; }
        public bool DebugFleetSupportSlave { get; set; }
        public bool DebugFleetSupportMaster { get; set; }
        public bool DebugGetBestTarget { get; set; }
        public bool DebugGetBestDroneTarget { get; set; }
        public bool DebugGotobase { get; set; }
        public bool DebugGreyList { get; set; }
        public bool DebugHangars { get; set; }
        public bool DebugHasExploded { get; set; }
        public bool DebugIdle { get; set; }
        public bool DebugInSpace { get; set; }
        public bool DebugInStation { get; set; }
        public bool DebugInWarp { get; set; }
        public bool DebugIsReadyToShoot { get; set; }
        public bool DebugItemHangar { get; set; }
        public bool DebugKillTargets { get; set; }
        public bool DebugKillAction { get; set; }
        public bool DebugLoadScripts { get; set; }
        public bool DebugLogging { get; set; }
        public bool DebugLootWrecks { get; set; }
        public bool DebugLootValue { get; set; }
        public bool DebugNavigateOnGrid { get; set; }
        public bool DebugMaintainConsoleLogs { get; set; }
        public bool DebugMiningBehavior { get; set; }
        public bool DebugMissionFittings { get; set; }
        public bool DebugMoveTo { get; set; }
        public bool DebugOnframe { get; set; }
        public bool DebugOverLoadWeapons { get; set; }
        public bool DebugPanic { get; set; }
        public bool DebugPerformance { get; set; }
        public bool DebugPotentialCombatTargets { get; set; }
        public bool DebugPreferredPrimaryWeaponTarget { get; set; }
        public bool DebugQuestorManager { get; set; }
        public bool DebugReloadAll { get; set; }
        public bool DebugReloadorChangeAmmo { get; set; }
        public bool DebugRemoteRepair { get; set; }
        public bool DebugSalvage { get; set; }
        public bool DebugScheduler { get; set; }
        public bool DebugSettings { get; set; }
        public bool DebugShipTargetValues { get; set; }
        public bool DebugSkillTraining { get; set; }
        public bool DebugSpeedMod { get; set; }
        public bool DebugStatistics { get; set; }
        public bool DebugStorylineMissions { get; set; }
        public bool DebugTargetCombatants { get; set; }
        public bool DebugTargetWrecks { get; set; }
        public bool DebugTractorBeams { get; set; }
        public bool DebugTraveler { get; set; }
        public bool DebugUI { get; set; }
        public bool DebugUndockBookmarks { get; set; }
        public bool DebugUnloadLoot { get; set; }
        public bool DebugValuedump { get; set; }
        public bool DebugWalletBalance { get; set; }
        public bool DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange { get; set; }
        public bool DebugWatchForActiveWars { get; set; }
        public string TargetSelectionMethod { get; set; }
        public bool DetailedCurrentTargetHealthLogging { get; set; }
        public bool DebugStates { get; set; }
        public bool DefendWhileTraveling { get; set; }
        public bool UseInnerspace { get; set; }
        public bool setEveClientDestinationWhenTraveling { get; set; }
        public string EveServerName { get; set; }
        public int EnforcedDelayBetweenModuleClicks { get; set; }
        public bool AvoidShootingTargetsWithMissilesIfweKNowTheyAreAboutToBeHitWithAPreviousVolley { get; set; }
        public string CharacterToAcceptInvitesFrom { get; set; }

        //
        // Misc Settings
        //
        public string CharacterMode { get; set; }
        public bool AutoStart { get; set; }
        public bool Disable3D { get; set; }
        public int MinimumDelay { get; set; }
        public int RandomDelay { get; set; }

        //
        // Console Log Settings
        //
        public bool SaveConsoleLog { get; set; }
        public int MaxLineConsole { get; set; }

        //
        // Enable / Disable Major Features that do not have categories of their own below
        //
        public bool EnableStorylines { get; set; }
        public bool DeclineStorylinesInsteadofBlacklistingfortheSession { get; set; }
        public bool UseLocalWatch { get; set; }
        public bool UseFittingManager { get; set; }

        public bool WatchForActiveWars { get; set; }

        public bool FleetSupportSlave { get; set; }

        public bool FleetSupportMaster { get; set; }

        public string FleetName { get; set; }
        public List<string> CharacterNamesForMasterToInviteToFleet { get; set; }

        //
        // Agent and mission settings
        //
        public string MissionName { get; set; }
        public float MinAgentBlackListStandings { get; set; }
        public float MinAgentGreyListStandings { get; set; }
        public string MissionsPath { get; set; }
        public bool RequireMissionXML { get; set; }

        public string StoryLineBaseBookmark { get; set; }
        public bool AllowNonStorylineCourierMissionsInLowSec { get; set; }

        public bool WaitDecline { get; set; }

        //
        // KillSentries Setting
        //
        private bool _killSentries;
        public int NumberOfModulesToActivateInCycle = 4;
        public int NoOfBookmarksDeletedAtOnce = 3;
        public int NumberOfTriesToDeleteBookmarks = 3;
        public bool KillSentries
        {
            get
            {
                if (Cache.Instance.MissionKillSentries != null)
                    return (bool)Cache.Instance.MissionKillSentries;
                return _killSentries;
            }
            set
            {
                _killSentries = value;
            }
        }

        //
        // Local Watch settings - if enabled
        //
        public int LocalBadStandingPilotsToTolerate { get; set; }
        public double LocalBadStandingLevelToConsiderBad { get; set; }
        public bool FinishWhenNotSafe { get; set; }

        //
        // Invasion Settings
        //
        public int BattleshipInvasionLimit { get; set; }
        public int BattlecruiserInvasionLimit { get; set; }
        public int CruiserInvasionLimit { get; set; }
        public int FrigateInvasionLimit { get; set; }
        public int InvasionMinimumDelay { get; set; }
        public int InvasionRandomDelay { get; set; }

        //
        // Ship Names
        //
        public string CombatShipName { get; set; }
        public string SalvageShipName { get; set; }
        public string TransportShipName { get; set; }
        public string TravelShipName { get; set; }
        public string MiningShipName { get; set; }

        //
        //Use HomeBookmark
        //
        public bool UseHomebookmark { get; set; }

        //
        // Storage location for loot, ammo, and bookmarks
        //
        public string HomeBookmarkName { get; set; }
        public string LootHangarTabName { get; set; }
        public string AmmoHangarTabName { get; set; }
        public string BookmarkHangar { get; set; }
        public string LootContainerName { get; set; }

        public string HighTierLootContainer { get; set; }

        //
        // Salvage and Loot settings
        //
        public bool CreateSalvageBookmarks { get; set; }
        public string CreateSalvageBookmarksIn { get; set; }
        public bool SalvageMultipleMissionsinOnePass { get; set; }
        public bool FirstSalvageBookmarksInSystem { get; set; }
        public string BookmarkPrefix { get; set; }
        public string SafeSpotBookmarkPrefix { get; set; }
        public string BookmarkFolder { get; set; }

        public string TravelToBookmarkPrefix { get; set; }

        public string UndockBookmarkPrefix { get; set; }
        public int UndockDelay { get; set; }
        public int MinimumWreckCount { get; set; }
        public bool AfterMissionSalvaging { get; set; }
        public bool UnloadLootAtStation { get; set; }
        public bool UseGatesInSalvage { get; set; }
        public bool LootEverything { get; set; }
        public int ReserveCargoCapacity { get; set; }
        public int MaximumWreckTargets { get; set; }
        public int AgeofBookmarksForSalvageBehavior { get; set; } //in minutes
        public int AgeofSalvageBookmarksToExpire { get; set; } //in minutes
        public bool DeleteBookmarksWithNPC { get; set; }

        public bool LootOnlyWhatYouCanWithoutSlowingDownMissionCompletion { get; set; }
        public int TractorBeamMinimumCapacitor { get; set; }
        public int SalvagerMinimumCapacitor { get; set; }
        public bool DoNotDoANYSalvagingOutsideMissionActions { get; set; }
        public bool LootItemRequiresTarget { get; set; }

        //
        // EVE Process Memory Ceiling and EVE wallet balance Change settings
        //
        public int WalletBalanceChangeLogOffDelay { get; set; }

        public string WalletBalanceChangeLogOffDelayLogoffOrExit { get; set; }

        public Int64 EVEProcessMemoryCeiling { get; set; }
        public string EVEProcessMemoryCeilingLogofforExit { get; set; }
        public bool CloseQuestorCMDUplinkInnerspaceProfile { get; set; }
        public bool CloseQuestorCMDUplinkIsboxerCharacterSet { get; set; }
        public bool CloseQuestorAllowRestart { get; set; }
        public bool CloseQuestorArbitraryOSCmd { get; set; }
        public string CloseQuestorOSCmdContents { get; set; }
        public bool LoginQuestorArbitraryOSCmd { get; set; }
        public string LoginQuestorOSCmdContents { get; set; }
        public bool LoginQuestorLavishScriptCmd { get; set; }
        public string LoginQuestorLavishScriptContents { get; set; }
        public bool MinimizeEveAfterStartingUp { get; set; }
        public int SecondstoWaitAfterExitingCloseQuestorBeforeExitingEVE = 240;

        public string LavishIsBoxerCharacterSet { get; set; }
        public string LavishInnerspaceProfile { get; set; }
        public string LavishGame { get; set; }

        public List<int> ItemsBlackList { get; set; }
        public List<int> WreckBlackList { get; set; }
        public bool WreckBlackListSmallWrecks { get; set; }
        public bool WreckBlackListMediumWrecks { get; set; }
        public string CachePath { get; set; }
        public string Logpath { get; set; }
        public bool SessionsLog { get; set; }
        public string SessionsLogPath { get; set; }
        public string SessionsLogFile { get; set; }

        public bool InnerspaceGeneratedConsoleLog { get; set; }

        public bool ConsoleLog { get; set; }
        public string ConsoleLogPath { get; set; }
        public string ConsoleLogFile { get; set; }
        public bool ConsoleLogRedacted { get; set; }
        public string ConsoleLogPathRedacted { get; set; }
        public string ConsoleLogFileRedacted { get; set; }
        public bool DroneStatsLog { get; set; }
        public string DroneStatsLogPath { get; set; }
        public string DroneStatslogFile { get; set; }
        public bool VolleyStatsLog { get; set; }
        public string VolleyStatsLogPath { get; set; }
        public string VolleyStatslogFile { get; set; }
        public bool WindowStatsLog { get; set; }
        public string WindowStatsLogPath { get; set; }
        public string WindowStatslogFile { get; set; }
        public bool WreckLootStatistics { get; set; }
        public string WreckLootStatisticsPath { get; set; }
        public string WreckLootStatisticsFile { get; set; }
        public bool MissionStats1Log { get; set; }
        public string MissionStats1LogPath { get; set; }
        public string MissionStats1LogFile { get; set; }
        public bool MissionStats2Log { get; set; }
        public string MissionStats2LogPath { get; set; }
        public string MissionStats2LogFile { get; set; }
        public bool MissionStats3Log { get; set; }
        public string MissionStats3LogPath { get; set; }
        public string MissionStats3LogFile { get; set; }

        public bool MissionDungeonIdLog { get; set; }

        public string MissionDungeonIdLogPath { get; set; }

        public string MissionDungeonIdLogFile { get; set; }

        public bool PocketStatistics { get; set; }
        public string PocketStatisticsPath { get; set; }
        public string PocketStatisticsFile { get; set; }
        public bool PocketObjectStatistics { get; set; }
        public string PocketObjectStatisticsPath { get; set; }
        public string PocketObjectStatisticsFile { get; set; }
        public string MissionDetailsHtmlPath { get; set; }
        public bool PocketStatsUseIndividualFilesPerPocket = true;
        public bool PocketObjectStatisticsLog { get; set; }

        //
        // Fitting Settings - if enabled
        //
        public List<FactionFitting> ListofFactionFittings { get; private set; }
        public List<AgentsList> ListOfAgents { get; set; }
        public List<MissionFitting> ListOfMissionFittings { get; private set; }
        public FactionFitting DefaultFitting { get; set; }

        //
        // Weapon Settings
        //
        public bool DontShootFrigatesWithSiegeorAutoCannons { get; set; }
        public int WeaponGroupId { get; set; }
        public int MaximumHighValueTargets { get; set; }
        public int MaximumLowValueTargets { get; set; }
        public int MinimumAmmoCharges { get; set; }
        public List<Ammo> Ammo { get; private set; }
        public List<MiningCrystals> MiningCrystals { get; private set; }
        public int MinimumTargetValueToConsiderTargetAHighValueTarget { get; set; }
        public int MaximumTargetValueToConsiderTargetALowValueTarget { get; set; }
        public bool ArmLoadCapBoosters { get; set; }
        public bool SelectAmmoToUseBasedOnShipSize { get; set; }

        public int DoNotSwitchTargetsIfTargetHasMoreThanThisArmorDamagePercentage { get; set; }

        public double DistanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons { get; set; } //also requires SpeedFrigatesShouldBeIgnoredByMainWeapons
        public double SpeedNPCFrigatesShouldBeIgnoredByPrimaryWeapons { get; set; } //also requires DistanceFrigatesShouldBeIgnoredByMainWeapons
        
        public bool AddWarpScramblersToPrimaryWeaponsPriorityTargetList { get; set; }
        public bool AddWebifiersToPrimaryWeaponsPriorityTargetList { get; set; }
        public bool AddDampenersToPrimaryWeaponsPriorityTargetList { get; set; }
        public bool AddNeutralizersToPrimaryWeaponsPriorityTargetList { get; set; }
        public bool AddTargetPaintersToPrimaryWeaponsPriorityTargetList { get; set; }
        public bool AddECMsToPrimaryWeaponsPriorityTargetList { get; set; }
        public bool AddTrackingDisruptorsToPrimaryWeaponsPriorityTargetList { get; set; }

        public bool AddWarpScramblersToDronePriorityTargetList { get; set; }
        public bool AddWebifiersToDronePriorityTargetList { get; set; }
        public bool AddDampenersToDronePriorityTargetList { get; set; }
        public bool AddNeutralizersToDronePriorityTargetList { get; set; }
        public bool AddTargetPaintersToDronePriorityTargetList { get; set; }
        public bool AddECMsToDroneTargetList { get; set; }
        public bool AddTrackingDisruptorsToDronePriorityTargetList { get; set; }

        public double ListPriorityTargetsEveryXSeconds { get; set; } 

        public double InsideThisRangeIsHardToTrack { get; set; }
        //
        // Script Settings - TypeIDs for the scripts you would like to use in these modules
        //
        public int TrackingDisruptorScript { get; private set; }
        public int TrackingComputerScript { get; private set; }
        public int TrackingLinkScript { get; private set; }
        public int SensorBoosterScript { get; private set; }
        public int SensorDampenerScript { get; private set; }
        public int AncillaryShieldBoosterScript { get; private set; } //they are not scripts, but they work the same, but are consumable for our purposes that does not matter
        public int CapacitorInjectorScript { get; private set; }      //they are not scripts, but they work the same, but are consumable for our purposes that does not matter
        public int CapBoosterToLoad { get; private set; } 
        //
        // OverLoad Settings (this WILL burn out modules, likely very quickly!
        // If you enable the overloading of a slot it is HIGHLY recommended you actually have something overloadable in that slot =/ 
        //
        public bool OverloadWeapons { get; set; }
        
        //
        // Speed and Movement Settings
        //
        public bool AvoidBumpingThings { get; set; }
        public bool SpeedTank { get; set; }
        public int OrbitDistance { get; set; }
        public bool OrbitStructure { get; set; }
        public int OptimalRange { get; set; }
        public int NosDistance { get; set; }
        public int RemoteRepairDistance { get; set; }
        public int MinimumPropulsionModuleDistance { get; set; }
        public int MinimumPropulsionModuleCapacitor { get; set; }

        //
        // Tank Settings
        //
        public int ActivateRepairModules { get; set; }
        public int DeactivateRepairModules { get; set; }
        public int InjectCapPerc { get; set; }

        //
        // Panic Settings
        //
        public int MinimumShieldPct { get; set; }
        public int MinimumArmorPct { get; set; }
        public int MinimumCapacitorPct { get; set; }
        public int SafeShieldPct { get; set; }
        public int SafeArmorPct { get; set; }
        public int SafeCapacitorPct { get; set; }
        public bool UseStationRepair { get; set; }
        public double IskPerLP { get; set; }

        //
        // Drone Settings
        //
        private bool _useDrones;

        public bool UseDrones
        {
            get
            {
                if (Cache.Instance.MissionUseDrones != null)
                    return (bool)Cache.Instance.MissionUseDrones;
                return _useDrones;
            }
            set
            {
                _useDrones = value;
            }
        }

        public int DroneTypeId { get; set; }
        public int DroneControlRange { get; set; }
        public bool DronesDontNeedTargetsBecauseWehaveThemSetOnAggressive { get; set; }
        public int DroneMinimumShieldPct { get; set; }
        public int DroneMinimumArmorPct { get; set; }
        public int DroneMinimumCapacitorPct { get; set; }
        public int DroneRecallShieldPct { get; set; }
        public int DroneRecallArmorPct { get; set; }
        public int DroneRecallCapacitorPct { get; set; }
        public int BelowThisHealthLevelRemoveFromDroneBay { get; set; }
        public int LongRangeDroneRecallShieldPct { get; set; }
        public int LongRangeDroneRecallArmorPct { get; set; }
        public int LongRangeDroneRecallCapacitorPct { get; set; }
        public bool DronesKillHighValueTargets { get; set; }
        public int MaterialsForWarOreID { get; set; }
        public int MaterialsForWarOreQty { get; set; }

        //
        // number of days of console logs to keep (anything older will be deleted on startup)
        //
        public int ConsoleLogDaysOfLogsToKeep { get; set; }

        //
        // Mission Blacklist / Greylist Settings
        //
        public List<string> MissionBlacklist { get; private set; }
        public List<string> MissionGreylist { get; private set; }
        public List<string> FactionBlacklist { get; private set; }

        //
        // Questor GUI location settings
        //
        public int? WindowXPosition { get; set; }
        public int? WindowYPosition { get; set; }
        public int? EVEWindowXPosition { get; set; }
        public int? EVEWindowYPosition { get; set; }
        public int? EVEWindowXSize { get; set; }
        public int? EVEWindowYSize { get; set; }

        //
        // Email SMTP settings
        //
        public bool EmailSupport { get; set; }
        public string EmailAddress { get; set; }
        public string EmailPassword { get; set; }
        public string EmailSMTPServer { get; set; }
        public int EmailSMTPPort { get; set; }
        public string EmailAddressToSendAlerts { get; set; }
        public bool? EmailEnableSSL { get; set; }

        //
        // Skill Training Settings
        //
        public bool ThisToonShouldBeTrainingSkills { get; set; } //as opposed to another toon on the same account

        public string UserDefinedLavishScriptScript1 { get; set; }
        public string UserDefinedLavishScriptScript1Description { get; set; }
        public string UserDefinedLavishScriptScript2 { get; set; }
        public string UserDefinedLavishScriptScript2Description { get; set; }
        public string UserDefinedLavishScriptScript3 { get; set; }
        public string UserDefinedLavishScriptScript3Description { get; set; }
        public string UserDefinedLavishScriptScript4 { get; set; }
        public string UserDefinedLavishScriptScript4Description { get; set; }

        public string LoadQuestorDebugInnerspaceCommandAlias { get; set; }
        public string LoadQuestorDebugInnerspaceCommand { get; set; }
        public string UnLoadQuestorDebugInnerspaceCommandAlias { get; set; }
        public string UnLoadQuestorDebugInnerspaceCommand { get; set; }

        public float StandingsNeededToAccessLevel1Agent { get; set; }
        public float StandingsNeededToAccessLevel2Agent { get; set; }
        public float StandingsNeededToAccessLevel3Agent { get; set; }
        public float StandingsNeededToAccessLevel4Agent { get; set; }
        public float StandingsNeededToAccessLevel5Agent { get; set; }

        //
        // path information - used to load the XML and used in other modules
        //
        public string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public string CharacterSettingsPath { get; private set; }
        public string CommonSettingsPath { get; private set; }
        public string CommonSettingsFileName { get; private set; }

        public event EventHandler<EventArgs> SettingsLoaded;

        public bool DefaultSettingsLoaded;

        private void LoadMissionBlackList(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                //if (Settings.Instance.CharacterMode.ToLower() == "Combat Missions".ToLower())
                //{
                //
                // Mission Blacklist
                //
                MissionBlacklist.Clear();
                XElement xmlElementBlackListSection = CharacterSettingsXml.Element("blacklist") ?? CommonSettingsXml.Element("blacklist");
                if (xmlElementBlackListSection != null)
                {
                    Logging.Log("Settings", "Loading Mission Blacklist", Logging.White);
                    int i = 1;
                    foreach (XElement BlacklistedMission in xmlElementBlackListSection.Elements("mission"))
                    {
                        MissionBlacklist.Add(Cache.Instance.FilterPath((string)BlacklistedMission));
                        if (Settings.Instance.DebugBlackList) Logging.Log("Settings.LoadBlackList", "[" + i + "] Blacklisted mission Name [" + Cache.Instance.FilterPath((string)BlacklistedMission) + "]", Logging.Teal);
                        i++;
                    }
                    Logging.Log("Settings", "        Mission Blacklist now has [" + MissionBlacklist.Count + "] entries", Logging.White);
                }
                //}

            }
            catch (Exception ex)
            {
                Logging.Log("Settings.LoadMissionBlackList", "Exception: [" + ex + "]", Logging.Debug);
            }
        }

        private void LoadMissionGreyList(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                //if (Settings.Instance.CharacterMode.ToLower() == "Combat Missions".ToLower())
                //{
                //
                // Mission Greylist
                //
                MissionGreylist.Clear();
                XElement xmlElementGreyListSection = CharacterSettingsXml.Element("greylist") ?? CommonSettingsXml.Element("greylist");

                if (xmlElementGreyListSection != null)
                {
                    Logging.Log("Settings", "Loading Mission Greylist", Logging.White);
                    int i = 1;
                    foreach (XElement GreylistedMission in xmlElementGreyListSection.Elements("mission"))
                    {
                        MissionGreylist.Add(Cache.Instance.FilterPath((string)GreylistedMission));
                        if (Settings.Instance.DebugGreyList) Logging.Log("Settings.LoadGreyList", "[" + i + "] Greylisted mission Name [" + Cache.Instance.FilterPath((string)GreylistedMission) + "]", Logging.Teal);
                        i++;
                    }
                    Logging.Log("Settings", "        Mission Greylist now has [" + MissionGreylist.Count + "] entries", Logging.White);
                }
                //}
            }
            catch (Exception ex)
            {
                Logging.Log("Settings.LoadMissionGreyList", "Exception: [" + ex + "]", Logging.Debug);
            }
        }

        private void LoadFactionBlacklist(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                //
                // Faction Blacklist
                //
                FactionBlacklist.Clear();
                XElement factionblacklist = CharacterSettingsXml.Element("factionblacklist") ?? CommonSettingsXml.Element("factionblacklist");
                if (factionblacklist != null)
                {
                    Logging.Log("Settings", "Loading Faction Blacklist", Logging.White);
                    foreach (XElement faction in factionblacklist.Elements("faction"))
                    {
                        Logging.Log("Settings", "        Missions from the faction [" + (string)faction + "] will be declined", Logging.White);
                        FactionBlacklist.Add((string)faction);
                    }

                    Logging.Log("Settings", "        Faction Blacklist now has [" + FactionBlacklist.Count + "] entries", Logging.White);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Settings.LoadMissionGreyList", "Exception: [" + ex + "]", Logging.Debug);
            }
        }

        public void ReadSettingsFromXML(XElement CharacterSettingsXml)
        {
            if (!String.IsNullOrEmpty(Cache.Instance.ScheduleCharacterName))
            {
                characterNameForLogs = Cache.Instance.FilterPath(Cache.Instance.ScheduleCharacterName);
            }
            else
            {
                characterNameForLogs = Cache.Instance.FilterPath(CharacterName);
            }

            Settings.Instance.CommonSettingsFileName = (string)CharacterSettingsXml.Element("commonSettingsFileName") ?? "common.xml";
            Settings.Instance.CommonSettingsPath = System.IO.Path.Combine(Settings.Instance.Path, Settings.Instance.CommonSettingsFileName);

            XElement CommonSettingsXml;
            if (File.Exists(Settings.Instance.CommonSettingsPath))
            {
                Settings.Instance.CommonXMLExists = true;
                CommonSettingsXml = XDocument.Load(Settings.Instance.CommonSettingsPath).Root;
                if (CommonSettingsXml == null)
                {
                    Logging.Log("Settings", "found [" + Settings.Instance.CommonSettingsPath + "] but was unable to load it: FATAL ERROR - use the provided settings.xml to create that file.", Logging.Red);
                }
            }
            else
            {
                Settings.Instance.CommonXMLExists = false;
                //
                // if the common XML does not exist, load the characters XML into the CommonSettingsXml just so we can simplify the XML element loading stuff.
                //
                CommonSettingsXml = XDocument.Load(Settings.Instance.CharacterSettingsPath).Root;
            }

            if (CommonSettingsXml == null) return; // this should never happen as we load the characters xml here if the common xml is missing. adding this does quiet some warnings though

            if (Settings.Instance.CommonXMLExists) Logging.Log("Settings", "Loading Settings from [" + Settings.Instance.CommonSettingsPath + "] and", Logging.Green);
            Logging.Log("Settings", "Loading Settings from [" + Settings.Instance.CharacterSettingsPath + "]", Logging.Green);
            //
            // these are listed by feature and should likely be re-ordered to reflect that
            //

            //
            // Debug Settings
            //
            DebugActivateGate = (bool?)CharacterSettingsXml.Element("debugActivateGate") ?? (bool?)CommonSettingsXml.Element("debugActivateGate") ?? false;
            DebugActivateBastion = (bool?)CharacterSettingsXml.Element("debugActivateBastion") ?? (bool?)CommonSettingsXml.Element("debugActivateBastion") ?? false;
            DebugActivateWeapons = (bool?)CharacterSettingsXml.Element("debugActivateWeapons") ?? (bool?)CommonSettingsXml.Element("debugActivateWeapons") ?? false;
            DebugAddDronePriorityTarget = (bool?)CharacterSettingsXml.Element("debugAddDronePriorityTarget") ?? (bool?)CommonSettingsXml.Element("debugAddDronePriorityTarget") ?? false;
            DebugAddPrimaryWeaponPriorityTarget = (bool?)CharacterSettingsXml.Element("debugAddPrimaryWeaponPriorityTarget") ?? (bool?)CommonSettingsXml.Element("debugAddPrimaryWeaponPriorityTarget") ?? false;
            DebugAgentInteractionReplyToAgent = (bool?)CharacterSettingsXml.Element("debugAgentInteractionReplyToAgent") ?? (bool?)CommonSettingsXml.Element("debugAgentInteractionReplyToAgent") ?? false;
            DebugAllMissionsOnBlackList = (bool?)CharacterSettingsXml.Element("debugAllMissionsOnBlackList") ?? (bool?)CommonSettingsXml.Element("debugAllMissionsOnBlackList") ?? false;
            DebugAllMissionsOnGreyList = (bool?)CharacterSettingsXml.Element("debugAllMissionsOnGreyList") ?? (bool?)CommonSettingsXml.Element("debugAllMissionsOnGreyList") ?? false;
            DebugAmmo = (bool?)CharacterSettingsXml.Element("debugAmmo") ?? (bool?)CommonSettingsXml.Element("debugAmmo") ?? false;
            DebugArm = (bool?)CharacterSettingsXml.Element("debugArm") ?? (bool?)CommonSettingsXml.Element("debugArm") ?? false;
            DebugAttachVSDebugger = (bool?)CharacterSettingsXml.Element("debugAttachVSDebugger") ?? (bool?)CommonSettingsXml.Element("debugAttachVSDebugger") ?? false;
            DebugAutoStart = (bool?)CharacterSettingsXml.Element("debugAutoStart") ?? (bool?)CommonSettingsXml.Element("debugAutoStart") ?? false;
            DebugBlackList = (bool?)CharacterSettingsXml.Element("debugBlackList") ?? (bool?)CommonSettingsXml.Element("debugBlackList") ?? false;
            DebugCargoHold = (bool?)CharacterSettingsXml.Element("debugCargoHold") ?? (bool?)CommonSettingsXml.Element("debugCargoHold") ?? false;
            DebugChat = (bool?)CharacterSettingsXml.Element("debugChat") ?? (bool?)CommonSettingsXml.Element("debugChat") ?? false;
            DebugCleanup = (bool?)CharacterSettingsXml.Element("debugCleanup") ?? (bool?)CommonSettingsXml.Element("debugCleanup") ?? false;
            DebugClearPocket = (bool?)CharacterSettingsXml.Element("debugClearPocket") ?? (bool?)CommonSettingsXml.Element("debugClearPocket") ?? false;
            DebugCourierMissions = (bool?)CharacterSettingsXml.Element("debugCourierMissions") ?? (bool?)CommonSettingsXml.Element("debugCourierMissions") ?? false;
            DebugDecline = (bool?)CharacterSettingsXml.Element("debugDecline") ?? (bool?)CommonSettingsXml.Element("debugDecline") ?? false;
            DebugDefense = (bool?)CharacterSettingsXml.Element("debugDefense") ?? (bool?)CommonSettingsXml.Element("debugDefense") ?? false;
            DebugDisableCleanup = (bool?)CharacterSettingsXml.Element("debugDisableCleanup") ?? (bool?)CommonSettingsXml.Element("debugDisableCleanup") ?? false;
            DebugDisableCombatMissionsBehavior = (bool?)CharacterSettingsXml.Element("debugDisableCombatMissionsBehavior") ?? (bool?)CommonSettingsXml.Element("debugDisableCombatMissionsBehavior") ?? false;
            DebugDisableCombatMissionCtrl = (bool?)CharacterSettingsXml.Element("debugDisableCombatMissionCtrl") ?? (bool?)CommonSettingsXml.Element("debugDisableCombatMissionCtrl") ?? false;
            DebugDisableCombat = (bool?)CharacterSettingsXml.Element("debugDisableCombat") ?? (bool?)CommonSettingsXml.Element("debugDisableCombat") ?? false;
            DebugDisableDrones = (bool?)CharacterSettingsXml.Element("debugDisableDrones") ?? (bool?)CommonSettingsXml.Element("debugDisableDrones") ?? false;
            DebugDisablePanic = (bool?)CharacterSettingsXml.Element("debugDisablePanic") ?? (bool?)CommonSettingsXml.Element("debugDisablePanic") ?? false;
            DebugDisableGetBestTarget = (bool?)CharacterSettingsXml.Element("debugDisableGetBestTarget") ?? (bool?)CommonSettingsXml.Element("debugDisableGetBestTarget") ?? false;
            DebugDisableGetBestDroneTarget = (bool?)CharacterSettingsXml.Element("debugDisableGetBestDroneTarget") ?? (bool?)CommonSettingsXml.Element("debugDisableGetBestTarget") ?? false; 
            DebugDisableSalvage = (bool?)CharacterSettingsXml.Element("debugDisableSalvage") ?? (bool?)CommonSettingsXml.Element("debugDisableSalvage") ?? false;
            DebugDisableGetBestTarget = (bool?)CharacterSettingsXml.Element("debugDisableGetBestTarget") ?? (bool?)CommonSettingsXml.Element("debugDisableGetBestTarget") ?? false;
            DebugDisableTargetCombatants = (bool?)CharacterSettingsXml.Element("debugDisableTargetCombatants") ?? (bool?)CommonSettingsXml.Element("debugDisableTargetCombatants") ?? false;
            DebugDisableNavigateIntoRange = (bool?)CharacterSettingsXml.Element("debugDisableNavigateIntoRange") ?? (bool?)CommonSettingsXml.Element("debugDisableNavigateIntoRange") ?? false;
            DebugDoneAction = (bool?)CharacterSettingsXml.Element("debugDoneAction") ?? (bool?)CommonSettingsXml.Element("debugDoneAction") ?? false;
            DebugDrones = (bool?)CharacterSettingsXml.Element("debugDrones") ?? (bool?)CommonSettingsXml.Element("debugDrones") ?? false;
            DebugDroneHealth = (bool?)CharacterSettingsXml.Element("debugDroneHealth") ?? (bool?)CommonSettingsXml.Element("debugDroneHealth") ?? false;
            DebugEachWeaponsVolleyCache = (bool?)CharacterSettingsXml.Element("debugEachWeaponsVolleyCache") ?? (bool?)CommonSettingsXml.Element("debugEachWeaponsVolleyCache") ?? false;
            DebugEntityCache = (bool?)CharacterSettingsXml.Element("debugEntityCache") ?? (bool?)CommonSettingsXml.Element("debugEntityCache") ?? false;
            DebugExceptions = (bool?)CharacterSettingsXml.Element("debugExceptions") ?? (bool?)CommonSettingsXml.Element("debugExceptions") ?? false;
            DebugFittingMgr = (bool?)CharacterSettingsXml.Element("debugFittingMgr") ?? (bool?)CommonSettingsXml.Element("debugFittingMgr") ?? false;
            DebugFleetSupportSlave = (bool?)CharacterSettingsXml.Element("debugFleetSupportSlave") ?? (bool?)CommonSettingsXml.Element("debugFleetSupportSlave") ?? false;
            DebugFleetSupportMaster = (bool?)CharacterSettingsXml.Element("debugFleetSupportMaster") ?? (bool?)CommonSettingsXml.Element("debugFleetSupportMaster") ?? false;
            DebugGetBestTarget = (bool?)CharacterSettingsXml.Element("debugGetBestTarget") ?? (bool?)CommonSettingsXml.Element("debugGetBestTarget") ?? false;
            DebugGetBestDroneTarget = (bool?)CharacterSettingsXml.Element("debugGetBestDroneTarget") ?? (bool?)CommonSettingsXml.Element("debugGetBestDroneTarget") ?? false;
            DebugGotobase = (bool?)CharacterSettingsXml.Element("debugGotobase") ?? (bool?)CommonSettingsXml.Element("debugGotobase") ?? false;
            DebugGreyList = (bool?)CharacterSettingsXml.Element("debugGreyList") ?? (bool?)CommonSettingsXml.Element("debugGreyList") ?? false;
            DebugHangars = (bool?)CharacterSettingsXml.Element("debugHangars") ?? (bool?)CommonSettingsXml.Element("debugHangars") ?? false;
            DebugHasExploded = (bool?)CharacterSettingsXml.Element("debugHasExploded") ?? (bool?)CommonSettingsXml.Element("debugHasExploded") ?? false;
            DebugIdle = (bool?)CharacterSettingsXml.Element("debugIdle") ?? (bool?)CommonSettingsXml.Element("debugIdle") ?? false;
            DebugInSpace = (bool?)CharacterSettingsXml.Element("debugInSpace") ?? (bool?)CommonSettingsXml.Element("debugInSpace") ?? false;
            DebugInStation = (bool?)CharacterSettingsXml.Element("debugInStation") ?? (bool?)CommonSettingsXml.Element("debugInStation") ?? false;
            DebugInWarp = (bool?)CharacterSettingsXml.Element("debugInWarp") ?? (bool?)CommonSettingsXml.Element("debugInWarp") ?? false;
            DebugIsReadyToShoot = (bool?)CharacterSettingsXml.Element("debugIsReadyToShoot") ?? (bool?)CommonSettingsXml.Element("debugIsReadyToShoot") ?? false;
            DebugItemHangar = (bool?)CharacterSettingsXml.Element("debugItemHangar") ?? (bool?)CommonSettingsXml.Element("debugItemHangar") ?? false;
            DebugKillTargets = (bool?)CharacterSettingsXml.Element("debugKillTargets") ?? (bool?)CommonSettingsXml.Element("debugKillTargets") ?? false;
            DebugKillAction = (bool?)CharacterSettingsXml.Element("debugKillAction") ?? (bool?)CommonSettingsXml.Element("debugKillAction") ?? false;
            DebugLoadScripts = (bool?)CharacterSettingsXml.Element("debugLoadScripts") ?? (bool?)CommonSettingsXml.Element("debugLoadScripts") ?? false;
            DebugLogging = (bool?)CharacterSettingsXml.Element("debugLogging") ?? (bool?)CommonSettingsXml.Element("debugLogging") ?? false;
            DebugLootWrecks = (bool?)CharacterSettingsXml.Element("debugLootWrecks") ?? (bool?)CommonSettingsXml.Element("debugLootWrecks") ?? false;
            DebugLootValue = (bool?)CharacterSettingsXml.Element("debugLootValue") ?? (bool?)CommonSettingsXml.Element("debugLootValue") ?? false;
            DebugMaintainConsoleLogs = (bool?)CharacterSettingsXml.Element("debugMaintainConsoleLogs") ?? (bool?)CommonSettingsXml.Element("debugMaintainConsoleLogs") ?? false;
            DebugMiningBehavior = (bool?)CharacterSettingsXml.Element("debugMiningBehavior") ?? (bool?)CommonSettingsXml.Element("debugMiningBehavior") ?? false;
            DebugMissionFittings = (bool?)CharacterSettingsXml.Element("debugMissionFittings") ?? (bool?)CommonSettingsXml.Element("debugMissionFittings") ?? false;
            DebugMoveTo = (bool?)CharacterSettingsXml.Element("debugMoveTo") ?? (bool?)CommonSettingsXml.Element("debugMoveTo") ?? false;
            DebugNavigateOnGrid = (bool?)CharacterSettingsXml.Element("debugNavigateOnGrid") ?? (bool?)CommonSettingsXml.Element("debugNavigateOnGrid") ?? false;
            DebugOnframe = (bool?)CharacterSettingsXml.Element("debugOnframe") ?? (bool?)CommonSettingsXml.Element("debugOnframe") ?? false;
            DebugOverLoadWeapons = (bool?)CharacterSettingsXml.Element("debugOverLoadWeapons") ?? (bool?)CommonSettingsXml.Element("debugOverLoadWeapons") ?? false;
            DebugPanic = (bool?)CharacterSettingsXml.Element("debugPanic") ?? (bool?)CommonSettingsXml.Element("debugPanic") ?? false;
            DebugPerformance = (bool?)CharacterSettingsXml.Element("debugPerformance") ?? (bool?)CommonSettingsXml.Element("debugPerformance") ?? false;                                     //enables more console logging having to do with the sub-states within each state
            DebugPotentialCombatTargets = (bool?)CharacterSettingsXml.Element("debugPotentialCombatTargets") ?? (bool?)CommonSettingsXml.Element("debugPotentialCombatTargets") ?? false;
            DebugPreferredPrimaryWeaponTarget = (bool?)CharacterSettingsXml.Element("debugPreferredPrimaryWeaponTarget") ?? (bool?)CommonSettingsXml.Element("debugPreferredPrimaryWeaponTarget") ?? false;
            DebugQuestorManager = (bool?)CharacterSettingsXml.Element("debugQuestorManager") ?? (bool?)CommonSettingsXml.Element("debugQuestorManager") ?? false;
            DebugReloadAll = (bool?)CharacterSettingsXml.Element("debugReloadAll") ?? (bool?)CommonSettingsXml.Element("debugReloadAll") ?? false;
            DebugReloadorChangeAmmo = (bool?)CharacterSettingsXml.Element("debugReloadOrChangeAmmo") ?? (bool?)CommonSettingsXml.Element("debugReloadOrChangeAmmo") ?? false;
            DebugRemoteRepair = (bool?)CharacterSettingsXml.Element("debugRemoteRepair") ?? (bool?)CommonSettingsXml.Element("debugRemoteRepair") ?? false;
            DebugSalvage = (bool?)CharacterSettingsXml.Element("debugSalvage") ?? (bool?)CommonSettingsXml.Element("debugSalvage") ?? false;
            DebugScheduler = (bool?)CharacterSettingsXml.Element("debugScheduler") ?? (bool?)CommonSettingsXml.Element("debugScheduler") ?? false;
            DebugSettings = (bool?)CharacterSettingsXml.Element("debugSettings") ?? (bool?)CommonSettingsXml.Element("debugSettings") ?? false;
            DebugShipTargetValues = (bool?)CharacterSettingsXml.Element("debugShipTargetValues") ?? (bool?)CommonSettingsXml.Element("debugShipTargetValues") ?? false;
            DebugSkillTraining = (bool?)CharacterSettingsXml.Element("debugSkillTraining") ?? (bool?)CommonSettingsXml.Element("debugSkillTraining") ?? false;
            DebugSpeedMod = (bool?)CharacterSettingsXml.Element("debugSpeedMod") ?? (bool?)CommonSettingsXml.Element("debugSpeedMod") ?? false;
            DebugStates = (bool?)CharacterSettingsXml.Element("debugStates") ?? (bool?)CommonSettingsXml.Element("debugStates") ?? false;                                               //enables more console logging having to do with the time it takes to execute each state
            DebugStatistics = (bool?)CharacterSettingsXml.Element("debugStatistics") ?? (bool?)CommonSettingsXml.Element("debugStatistics") ?? false;
            DebugStorylineMissions = (bool?)CharacterSettingsXml.Element("debugStorylineMissions") ?? (bool?)CommonSettingsXml.Element("debugStorylineMissions") ?? false;
            DebugTargetCombatants = (bool?)CharacterSettingsXml.Element("debugTargetCombatants") ?? (bool?)CommonSettingsXml.Element("debugTargetCombatants") ?? false;
            DebugTargetWrecks = (bool?)CharacterSettingsXml.Element("debugTargetWrecks") ?? (bool?)CommonSettingsXml.Element("debugTargetWrecks") ?? false;
            DebugTraveler = (bool?)CharacterSettingsXml.Element("debugTraveler") ?? (bool?)CommonSettingsXml.Element("debugTraveler") ?? false;
            DebugTractorBeams = (bool?)CharacterSettingsXml.Element("debugTractorBeams") ?? (bool?)CommonSettingsXml.Element("debugTractorBeams") ?? false;
            DebugUI = (bool?)CharacterSettingsXml.Element("debugUI") ?? (bool?)CommonSettingsXml.Element("debugUI") ?? false;
            DebugUndockBookmarks = (bool?)CharacterSettingsXml.Element("debugUndockBookmarks") ?? (bool?)CommonSettingsXml.Element("debugUndockBookmarks") ?? false;
            DebugUnloadLoot = (bool?)CharacterSettingsXml.Element("debugUnloadLoot") ?? (bool?)CommonSettingsXml.Element("debugUnloadLoot") ?? false;
            DebugValuedump = (bool?)CharacterSettingsXml.Element("debugValuedump") ?? (bool?)CommonSettingsXml.Element("debugValuedump") ?? false;
            DebugWalletBalance = (bool?)CharacterSettingsXml.Element("debugWalletBalance") ?? (bool?)CommonSettingsXml.Element("debugWalletBalance") ?? false;
            DebugWeShouldBeInSpaceORInStationAndOutOfSessionChange = (bool?)CharacterSettingsXml.Element("debugWeShouldBeInSpaceORInStationAndOutOfSessionChange") ?? (bool?)CommonSettingsXml.Element("debugWeShouldBeInSpaceORInStationAndOutOfSessionChange") ?? false;
            DebugWatchForActiveWars = (bool?)CharacterSettingsXml.Element("debugWatchForActiveWars") ?? (bool?)CommonSettingsXml.Element("debugWatchForActiveWars") ?? false;
            DetailedCurrentTargetHealthLogging = (bool?)CharacterSettingsXml.Element("detailedCurrentTargetHealthLogging") ?? (bool?)CommonSettingsXml.Element("detailedCurrentTargetHealthLogging") ?? true;
            DefendWhileTraveling = (bool?)CharacterSettingsXml.Element("defendWhileTraveling") ?? (bool?)CommonSettingsXml.Element("defendWhileTraveling") ?? true;
            UseInnerspace = (bool?)CharacterSettingsXml.Element("useInnerspace") ?? (bool?)CommonSettingsXml.Element("useInnerspace") ?? true;
            setEveClientDestinationWhenTraveling = (bool?)CharacterSettingsXml.Element("setEveClientDestinationWhenTraveling") ?? (bool?)CommonSettingsXml.Element("setEveClientDestinationWhenTraveling") ?? false;
            TargetSelectionMethod = (string)CharacterSettingsXml.Element("targetSelectionMethod") ?? (string)CommonSettingsXml.Element("targetSelectionMethod") ?? "isdp"; //other choice is "old"
            CharacterToAcceptInvitesFrom = (string)CharacterSettingsXml.Element("characterToAcceptInvitesFrom") ?? (string)CommonSettingsXml.Element("characterToAcceptInvitesFrom") ?? Settings.Instance.CharacterName;
            MemoryManagerTrimThreshold = (long?)CharacterSettingsXml.Element("memoryManagerTrimThreshold") ?? (long?)CommonSettingsXml.Element("memoryManagerTrimThreshold") ?? 524288000;
            EveServerName = (string)CharacterSettingsXml.Element("eveServerName") ?? (string)CommonSettingsXml.Element("eveServerName") ?? "Tranquility";
            EnforcedDelayBetweenModuleClicks = (int?)CharacterSettingsXml.Element("enforcedDelayBetweenModuleClicks") ?? (int?)CommonSettingsXml.Element("enforcedDelayBetweenModuleClicks") ?? 3000;
            AvoidShootingTargetsWithMissilesIfweKNowTheyAreAboutToBeHitWithAPreviousVolley = (bool?)CharacterSettingsXml.Element("avoidShootingTargetsWithMissilesIfweKNowTheyAreAboutToBeHitWithAPreviousVolley") ?? (bool?)CommonSettingsXml.Element("AvoidShootingTargetsWithMissilesIfweKNowTheyAreAboutToBeHitWithAPreviousVolley") ?? false;
            //
            // Misc Settings
            //
            CharacterMode = (string)CharacterSettingsXml.Element("characterMode") ?? (string)CommonSettingsXml.Element("characterMode") ?? "Combat Missions".ToLower();

            //other option is "salvage"

            if (Settings.Instance.CharacterMode.ToLower() == "dps".ToLower())
            {
                Settings.Instance.CharacterMode = "Combat Missions".ToLower();
            }

            AutoStart = (bool?)CharacterSettingsXml.Element("autoStart") ?? (bool?)CommonSettingsXml.Element("autoStart") ?? false; // auto Start enabled or disabled by default?
            MaxLineConsole = (int?)CharacterSettingsXml.Element("maxLineConsole") ?? (int?)CommonSettingsXml.Element("maxLineConsole") ?? 1000;
            // maximum console log lines to show in the GUI
            Disable3D = (bool?)CharacterSettingsXml.Element("disable3D") ?? (bool?)CommonSettingsXml.Element("disable3D") ?? false; // Disable3d graphics while in space
            RandomDelay = (int?)CharacterSettingsXml.Element("randomDelay") ?? (int?)CommonSettingsXml.Element("randomDelay") ?? 0;
            MinimumDelay = (int?)CharacterSettingsXml.Element("minimumDelay") ?? (int?)CommonSettingsXml.Element("minimumDelay") ?? 0;

            //
            // Enable / Disable Major Features that do not have categories of their own below
            //
            UseFittingManager = (bool?)CharacterSettingsXml.Element("UseFittingManager") ?? (bool?)CommonSettingsXml.Element("UseFittingManager") ?? true;
            EnableStorylines = (bool?)CharacterSettingsXml.Element("enableStorylines") ?? (bool?)CommonSettingsXml.Element("enableStorylines") ?? false;
            DeclineStorylinesInsteadofBlacklistingfortheSession = (bool?)CharacterSettingsXml.Element("declineStorylinesInsteadofBlacklistingfortheSession") ?? (bool?)CommonSettingsXml.Element("declineStorylinesInsteadofBlacklistingfortheSession") ?? false;
            UseLocalWatch = (bool?)CharacterSettingsXml.Element("UseLocalWatch") ?? (bool?)CommonSettingsXml.Element("UseLocalWatch") ?? true;
            WatchForActiveWars = (bool?)CharacterSettingsXml.Element("watchForActiveWars") ?? (bool?)CommonSettingsXml.Element("watchForActiveWars") ?? true;

            FleetSupportSlave = (bool?)CharacterSettingsXml.Element("fleetSupportSlave") ?? (bool?)CommonSettingsXml.Element("fleetSupportSlave") ?? true;
            FleetSupportMaster = (bool?)CharacterSettingsXml.Element("fleetSupportMaster") ?? (bool?)CommonSettingsXml.Element("fleetSupportMaster") ?? true;
            FleetName = (string)CharacterSettingsXml.Element("fleetName") ?? (string)CommonSettingsXml.Element("fleetName") ?? "Fleet1";

            //
            //CharacterNamesForMasterToInviteToFleet
            //
            Settings.Instance.CharacterNamesForMasterToInviteToFleet.Clear();
            XElement xmlCharacterNamesForMasterToInviteToFleet = CharacterSettingsXml.Element("characterNamesForMasterToInviteToFleet") ?? CharacterSettingsXml.Element("characterNamesForMasterToInviteToFleet");
            if (xmlCharacterNamesForMasterToInviteToFleet != null)
            {
                Logging.Log("Settings", "Loading CharacterNames For Master To Invite To Fleet", Logging.White);
                int i = 1;
                foreach (XElement CharacterToInvite in xmlCharacterNamesForMasterToInviteToFleet.Elements("character"))
                {
                    Settings.Instance.CharacterNamesForMasterToInviteToFleet.Add((string)CharacterToInvite);
                    if (Settings.Instance.DebugFleetSupportMaster) Logging.Log("Settings.LoadFleetList", "[" + i + "] CharacterName [" + (string)CharacterToInvite + "]", Logging.Teal);
                    i++;
                }
                if (Settings.Instance.FleetSupportMaster) Logging.Log("Settings", "        CharacterNamesForMasterToInviteToFleet now has [" + CharacterNamesForMasterToInviteToFleet.Count + "] entries", Logging.White);
            }

            //
            // Agent Standings and Mission Settings
            //
            //if (Settings.Instance.CharacterMode.ToLower() == "Combat Missions".ToLower())
            //{
            MinAgentBlackListStandings = (float?)CharacterSettingsXml.Element("minAgentBlackListStandings") ?? (float?)CommonSettingsXml.Element("minAgentBlackListStandings") ?? (float)6.0;
            MinAgentGreyListStandings = (float?)CharacterSettingsXml.Element("minAgentGreyListStandings") ?? (float?)CommonSettingsXml.Element("minAgentGreyListStandings") ?? (float)5.0;
            WaitDecline = (bool?)CharacterSettingsXml.Element("waitDecline") ?? (bool?)CommonSettingsXml.Element("waitDecline") ?? false;

            string relativeMissionsPath = (string)CharacterSettingsXml.Element("missionsPath") ?? (string)CommonSettingsXml.Element("missionsPath");
            MissionsPath = System.IO.Path.Combine(Settings.Instance.Path, relativeMissionsPath);
            Logging.Log("Settings", "MissionsPath is: [" + MissionsPath + "]", Logging.White);

            RequireMissionXML = (bool?)CharacterSettingsXml.Element("requireMissionXML") ?? (bool?)CommonSettingsXml.Element("requireMissionXML") ?? false;
            AllowNonStorylineCourierMissionsInLowSec = (bool?)CharacterSettingsXml.Element("LowSecMissions") ?? (bool?)CommonSettingsXml.Element("LowSecMissions") ?? false;
            StoryLineBaseBookmark = (string)CharacterSettingsXml.Element("storyLineBaseBookmark") ?? (string)CommonSettingsXml.Element("storyLineBaseBookmark") ?? "";
            MaterialsForWarOreID = (int?)CharacterSettingsXml.Element("MaterialsForWarOreID") ?? (int?)CommonSettingsXml.Element("MaterialsForWarOreID") ?? 20;
            MaterialsForWarOreQty = (int?)CharacterSettingsXml.Element("MaterialsForWarOreQty") ?? (int?)CommonSettingsXml.Element("MaterialsForWarOreQty") ?? 8000;
            KillSentries = (bool?)CharacterSettingsXml.Element("killSentries") ?? (bool?)CommonSettingsXml.Element("killSentries") ?? false;
            //}

            //
            // Local Watch Settings - if enabled
            //
            LocalBadStandingPilotsToTolerate = (int?)CharacterSettingsXml.Element("LocalBadStandingPilotsToTolerate") ?? (int?)CommonSettingsXml.Element("LocalBadStandingPilotsToTolerate") ?? 1;
            LocalBadStandingLevelToConsiderBad = (double?)CharacterSettingsXml.Element("LocalBadStandingLevelToConsiderBad") ?? (double?)CommonSettingsXml.Element("LocalBadStandingLevelToConsiderBad") ?? -0.1;

            //
            // Invasion Settings
            //
            BattleshipInvasionLimit = (int?)CharacterSettingsXml.Element("battleshipInvasionLimit") ?? (int?)CommonSettingsXml.Element("battleshipInvasionLimit") ?? 0;

            // if this number of BattleShips lands on grid while in a mission we will enter panic
            BattlecruiserInvasionLimit = (int?)CharacterSettingsXml.Element("battlecruiserInvasionLimit") ?? (int?)CommonSettingsXml.Element("battlecruiserInvasionLimit") ?? 0;

            // if this number of BattleCruisers lands on grid while in a mission we will enter panic
            CruiserInvasionLimit = (int?)CharacterSettingsXml.Element("cruiserInvasionLimit") ?? (int?)CommonSettingsXml.Element("cruiserInvasionLimit") ?? 0;

            // if this number of Cruisers lands on grid while in a mission we will enter panic
            FrigateInvasionLimit = (int?)CharacterSettingsXml.Element("frigateInvasionLimit") ?? (int?)CommonSettingsXml.Element("frigateInvasionLimit") ?? 0;

            // if this number of Frigates lands on grid while in a mission we will enter panic
            InvasionRandomDelay = (int?)CharacterSettingsXml.Element("invasionRandomDelay") ?? (int?)CommonSettingsXml.Element("invasionRandomDelay") ?? 0; // random relay to stay docked
            InvasionMinimumDelay = (int?)CharacterSettingsXml.Element("invasionMinimumDelay") ?? (int?)CommonSettingsXml.Element("invasionMinimumDelay") ?? 0;

            // minimum delay to stay docked

            //
            // Value - Used in calculations
            //
            IskPerLP = (double?)CharacterSettingsXml.Element("IskPerLP") ?? (double?)CommonSettingsXml.Element("IskPerLP") ?? 600; //used in value calculations

            //
            // Undock settings
            //
            UndockDelay = (int?)CharacterSettingsXml.Element("undockdelay") ?? (int?)CommonSettingsXml.Element("undockdelay") ?? 10; //Delay when undocking - not in use
            UndockBookmarkPrefix = (string)CharacterSettingsXml.Element("undockprefix") ?? (string)CommonSettingsXml.Element("undockprefix") ?? (string)CharacterSettingsXml.Element("bookmarkWarpOut") ?? (string)CommonSettingsXml.Element("bookmarkWarpOut") ?? "";

            //
            // Location of the Questor GUI on startup (default is off the screen)
            //
            //X Questor GUI window position (needs to be changed, default is off screen)
            WindowXPosition = (int?)CharacterSettingsXml.Element("windowXPosition") ?? (int?)CommonSettingsXml.Element("windowXPosition") ?? 1;

            //Y Questor GUI window position (needs to be changed, default is off screen)
            WindowYPosition = (int?)CharacterSettingsXml.Element("windowYPosition") ?? (int?)CommonSettingsXml.Element("windowYPosition") ?? 1;

            //
            // Location of the EVE Window on startup (default is to leave the window alone)
            //
            try
            {
                //EVE Client window position
                EVEWindowXPosition = (int?)CharacterSettingsXml.Element("eveWindowXPosition") ?? (int?)CommonSettingsXml.Element("eveWindowXPosition") ?? 0;

                //EVE Client window position
                EVEWindowYPosition = (int?)CharacterSettingsXml.Element("eveWindowYPosition") ?? (int?)CommonSettingsXml.Element("eveWindowYPosition") ?? 0;

                //
                // Size of the EVE Window on startup (default is to leave the window alone)
                // This CAN and WILL distort the proportions of the EVE client if you configure it to do so.
                // ISBOXER arguably does this with more elegance...
                //
                //EVE Client window position
                EVEWindowXSize = (int?)CharacterSettingsXml.Element("eveWindowXSize") ?? (int?)CommonSettingsXml.Element("eveWindowXSize") ?? 0;

                //EVE Client window position
                EVEWindowYSize = (int?)CharacterSettingsXml.Element("eveWindowYSize") ?? (int?)CommonSettingsXml.Element("eveWindowYSize") ?? 0;
            }
            catch
            {
                Logging.Log("Settings", "Invalid Format for eveWindow Settings - skipping", Logging.Teal);
            }

            try
            {
                //
                // Ship Names
                //
                CombatShipName = (string)CharacterSettingsXml.Element("combatShipName") ?? (string)CommonSettingsXml.Element("combatShipName") ?? "My frigate of doom";
                SalvageShipName = (string)CharacterSettingsXml.Element("salvageShipName") ?? (string)CommonSettingsXml.Element("salvageShipName") ?? "My Destroyer of salvage";
                TransportShipName = (string)CharacterSettingsXml.Element("transportShipName") ?? (string)CommonSettingsXml.Element("transportShipName") ?? "My Hauler of transportation";
                TravelShipName = (string)CharacterSettingsXml.Element("travelShipName") ?? (string)CommonSettingsXml.Element("travelShipName") ?? "My Shuttle of traveling";
                MiningShipName = (string)CharacterSettingsXml.Element("miningShipName") ?? (string)CommonSettingsXml.Element("miningShipName") ?? "My Exhumer of Destruction";
            }
            catch (Exception exception)
            {
                Logging.Log("Settings", "Error Loading Ship Name Settings [" + exception + "]", Logging.Teal);
            }

            try
            {
                //
                // Storage Location for Loot, Ammo, Bookmarks
                //
                UseHomebookmark = (bool?)CharacterSettingsXml.Element("UseHomebookmark") ?? (bool?)CommonSettingsXml.Element("UseHomebookmark") ?? false;
            }
            catch (Exception exception)
            {
                Logging.Log("Settings", "Error Loading UseHomebookmark [" + exception + "]", Logging.Teal);
            }

            try
            {
                //
                // Storage Location for Loot, Ammo, Bookmarks
                //
                HomeBookmarkName = (string)CharacterSettingsXml.Element("homeBookmarkName") ?? (string)CommonSettingsXml.Element("homeBookmarkName") ?? "myHomeBookmark";
                LootHangarTabName = (string)CharacterSettingsXml.Element("lootHangar") ?? (string)CommonSettingsXml.Element("lootHangar");
                if (string.IsNullOrEmpty(Settings.Instance.LootHangarTabName))
                {
                    Logging.Log("Settings", "Loothangar [" + "ItemsHangar" + "]", Logging.White);
                }
                else
                {
                    Logging.Log("Settings", "Loothangar [" + Settings.Instance.LootHangarTabName + "]", Logging.White);
                }
                AmmoHangarTabName = (string)CharacterSettingsXml.Element("ammoHangar") ?? (string)CommonSettingsXml.Element("ammoHangar");
                if (string.IsNullOrEmpty(Settings.Instance.AmmoHangarTabName))
                {
                    Logging.Log("Settings", "AmmoHangar [" + "ItemHangar" + "]", Logging.White);
                }
                else
                {
                    Logging.Log("Settings", "AmmoHangar [" + Settings.Instance.AmmoHangarTabName + "]", Logging.White);
                }
                BookmarkHangar = (string)CharacterSettingsXml.Element("bookmarkHangar") ?? (string)CommonSettingsXml.Element("bookmarkHangar");
                LootContainerName = (string)CharacterSettingsXml.Element("lootContainer") ?? (string)CommonSettingsXml.Element("lootContainer");
                if (LootContainerName != null)
                {
                    LootContainerName = LootContainerName.ToLower();
                }
                HighTierLootContainer = (string)CharacterSettingsXml.Element("highValueLootContainer") ?? (string)CommonSettingsXml.Element("highValueLootContainer");
                if (HighTierLootContainer != null)
                {
                    HighTierLootContainer = HighTierLootContainer.ToLower();
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Settings", "Error Loading Hangar Settings [" + exception + "]", Logging.Teal);
            }

            try
            {
                //
                // Loot and Salvage Settings
                //
                LootEverything = (bool?)CharacterSettingsXml.Element("lootEverything") ?? (bool?)CommonSettingsXml.Element("lootEverything") ?? true;
                UseGatesInSalvage = (bool?)CharacterSettingsXml.Element("useGatesInSalvage") ?? (bool?)CommonSettingsXml.Element("useGatesInSalvage") ?? false;

                // if our mission does not DeSpawn (likely someone in the mission looting our stuff?) use the gates when salvaging to get to our bookmarks
                CreateSalvageBookmarks = (bool?)CharacterSettingsXml.Element("createSalvageBookmarks") ?? (bool?)CommonSettingsXml.Element("createSalvageBookmarks") ?? false;
                CreateSalvageBookmarksIn = (string)CharacterSettingsXml.Element("createSalvageBookmarksIn") ?? (string)CommonSettingsXml.Element("createSalvageBookmarksIn") ?? "Player";

                //Player or Corp
                //other setting is "Corp"
                BookmarkPrefix = (string)CharacterSettingsXml.Element("bookmarkPrefix") ?? (string)CommonSettingsXml.Element("bookmarkPrefix") ?? "Salvage:";
                SafeSpotBookmarkPrefix = (string)CharacterSettingsXml.Element("safeSpotBookmarkPrefix") ?? (string)CommonSettingsXml.Element("safeSpotBookmarkPrefix") ?? "safespot";
                BookmarkFolder = (string)CharacterSettingsXml.Element("bookmarkFolder") ?? (string)CommonSettingsXml.Element("bookmarkFolder") ?? "Salvage:";
                TravelToBookmarkPrefix = (string)CharacterSettingsXml.Element("travelToBookmarkPrefix") ?? (string)CommonSettingsXml.Element("travelToBookmarkPrefix") ?? "MeetHere:";
                MinimumWreckCount = (int?)CharacterSettingsXml.Element("minimumWreckCount") ?? (int?)CommonSettingsXml.Element("minimumWreckCount") ?? 1;
                AfterMissionSalvaging = (bool?)CharacterSettingsXml.Element("afterMissionSalvaging") ?? (bool?)CommonSettingsXml.Element("afterMissionSalvaging") ?? false;
                FirstSalvageBookmarksInSystem = (bool?)CharacterSettingsXml.Element("FirstSalvageBookmarksInSystem") ?? (bool?)CommonSettingsXml.Element("FirstSalvageBookmarksInSystem") ?? false;
                SalvageMultipleMissionsinOnePass = (bool?)CharacterSettingsXml.Element("salvageMultpleMissionsinOnePass") ?? (bool?)CommonSettingsXml.Element("salvageMultpleMissionsinOnePass") ?? false;
                UnloadLootAtStation = (bool?)CharacterSettingsXml.Element("unloadLootAtStation") ?? (bool?)CommonSettingsXml.Element("unloadLootAtStation") ?? false;
                ReserveCargoCapacity = (int?)CharacterSettingsXml.Element("reserveCargoCapacity") ?? (int?)CommonSettingsXml.Element("reserveCargoCapacity") ?? 0;
                MaximumWreckTargets = (int?)CharacterSettingsXml.Element("maximumWreckTargets") ?? (int?)CommonSettingsXml.Element("maximumWreckTargets") ?? 0;
                WreckBlackListSmallWrecks = (bool?)CharacterSettingsXml.Element("WreckBlackListSmallWrecks") ?? (bool?)CommonSettingsXml.Element("WreckBlackListSmallWrecks") ?? false;
                WreckBlackListMediumWrecks = (bool?)CharacterSettingsXml.Element("WreckBlackListMediumWrecks") ?? (bool?)CommonSettingsXml.Element("WreckBlackListMediumWrecks") ?? false;
                AgeofBookmarksForSalvageBehavior = (int?)CharacterSettingsXml.Element("ageofBookmarksForSalvageBehavior") ?? (int?)CommonSettingsXml.Element("ageofBookmarksForSalvageBehavior") ?? 45;
                AgeofSalvageBookmarksToExpire = (int?)CharacterSettingsXml.Element("ageofSalvageBookmarksToExpire") ?? (int?)CommonSettingsXml.Element("ageofSalvageBookmarksToExpire") ?? 120;
                LootOnlyWhatYouCanWithoutSlowingDownMissionCompletion = (bool?)CharacterSettingsXml.Element("lootOnlyWhatYouCanWithoutSlowingDownMissionCompletion") ?? (bool?)CommonSettingsXml.Element("lootOnlyWhatYouCanWithoutSlowingDownMissionCompletion") ?? false;
                TractorBeamMinimumCapacitor = (int?)CharacterSettingsXml.Element("tractorBeamMinimumCapacitor") ?? (int?)CommonSettingsXml.Element("tractorBeamMinimumCapacitor") ?? 0;
                SalvagerMinimumCapacitor = (int?)CharacterSettingsXml.Element("salvagerMinimumCapacitor") ?? (int?)CommonSettingsXml.Element("salvagerMinimumCapacitor") ?? 0;
                DoNotDoANYSalvagingOutsideMissionActions = (bool?)CharacterSettingsXml.Element("doNotDoANYSalvagingOutsideMissionActions") ?? (bool?)CommonSettingsXml.Element("doNotDoANYSalvagingOutsideMissionActions") ?? false;
                LootItemRequiresTarget = (bool?)CharacterSettingsXml.Element("lootItemRequiresTarget") ?? (bool?)CommonSettingsXml.Element("lootItemRequiresTarget") ?? false;
            }
            catch (Exception exception)
            {
                Logging.Log("Settings", "Error Loading Loot and Salvage Settings [" + exception + "]", Logging.Teal);
            }

            //
            // at what memory usage do we need to restart this session?
            //
            EVEProcessMemoryCeiling = (int?)CharacterSettingsXml.Element("EVEProcessMemoryCeiling") ?? (int?)CommonSettingsXml.Element("EVEProcessMemoryCeiling") ?? 2048;
            EVEProcessMemoryCeilingLogofforExit = (string)CharacterSettingsXml.Element("EVEProcessMemoryCeilingLogofforExit") ?? (string)CommonSettingsXml.Element("EVEProcessMemoryCeilingLogofforExit") ?? "exit";

            CloseQuestorCMDUplinkInnerspaceProfile = (bool?)CharacterSettingsXml.Element("CloseQuestorCMDUplinkInnerspaceProfile") ?? (bool?)CommonSettingsXml.Element("CloseQuestorCMDUplinkInnerspaceProfile") ?? true;
            CloseQuestorCMDUplinkIsboxerCharacterSet = (bool?)CharacterSettingsXml.Element("CloseQuestorCMDUplinkIsboxerCharacterSet") ?? (bool?)CommonSettingsXml.Element("CloseQuestorCMDUplinkIsboxerCharacterSet") ?? false;
            CloseQuestorAllowRestart = (bool?)CharacterSettingsXml.Element("CloseQuestorAllowRestart") ?? (bool?)CommonSettingsXml.Element("CloseQuestorAllowRestart") ?? true;
            CloseQuestorArbitraryOSCmd = (bool?)CharacterSettingsXml.Element("CloseQuestorArbitraryOSCmd") ?? (bool?)CommonSettingsXml.Element("CloseQuestorArbitraryOSCmd") ?? false;

            //true or false
            CloseQuestorOSCmdContents = (string)CharacterSettingsXml.Element("CloseQuestorOSCmdContents") ?? (string)CommonSettingsXml.Element("CloseQuestorOSCmdContents") ?? "cmd /k (date /t && time /t && echo. && echo. && echo Questor is configured to use the feature: CloseQuestorArbitraryOSCmd && echo But No actual command was specified in your characters settings xml! && pause)";

            LoginQuestorArbitraryOSCmd = (bool?)CharacterSettingsXml.Element("LoginQuestorArbitraryOSCmd") ?? (bool?)CommonSettingsXml.Element("LoginQuestorArbitraryOSCmd") ?? false;

            //true or false
            LoginQuestorOSCmdContents = (string)CharacterSettingsXml.Element("LoginQuestorOSCmdContents") ?? (string)CommonSettingsXml.Element("LoginQuestorOSCmdContents") ?? "cmd /k (date /t && time /t && echo. && echo. && echo Questor is configured to use the feature: LoginQuestorArbitraryOSCmd && echo But No actual command was specified in your characters settings xml! && pause)";
            LoginQuestorLavishScriptCmd = (bool?)CharacterSettingsXml.Element("LoginQuestorLavishScriptCmd") ?? (bool?)CommonSettingsXml.Element("LoginQuestorLavishScriptCmd") ?? false;

            //true or false
            LoginQuestorLavishScriptContents = (string)CharacterSettingsXml.Element("LoginQuestorLavishScriptContents") ?? (string)CommonSettingsXml.Element("LoginQuestorLavishScriptContents") ?? "echo Questor is configured to use the feature: LoginQuestorLavishScriptCmd && echo But No actual command was specified in your characters settings xml! && pause)";

            MinimizeEveAfterStartingUp = (bool?)CharacterSettingsXml.Element("MinimizeEveAfterStartingUp") ?? (bool?)CommonSettingsXml.Element("MinimizeEveAfterStartingUp") ?? false;

            //the above setting can be set to any script or commands available on the system. make sure you test it from a command prompt while in your .net programs directory

            WalletBalanceChangeLogOffDelay = (int?)CharacterSettingsXml.Element("walletbalancechangelogoffdelay") ?? (int?)CommonSettingsXml.Element("walletbalancechangelogoffdelay") ?? 30;
            WalletBalanceChangeLogOffDelayLogoffOrExit = (string)CharacterSettingsXml.Element("walletbalancechangelogoffdelayLogofforExit") ?? (string)CommonSettingsXml.Element("walletbalancechangelogoffdelayLogofforExit") ?? "exit";
            SecondstoWaitAfterExitingCloseQuestorBeforeExitingEVE = 240;

            //
            // Enable / Disable the different types of logging that are available
            //
            InnerspaceGeneratedConsoleLog = (bool?)CharacterSettingsXml.Element("innerspaceGeneratedConsoleLog") ?? (bool?)CommonSettingsXml.Element("innerspaceGeneratedConsoleLog") ?? false; // save the innerspace generated console log to file
            SaveConsoleLog = (bool?)CharacterSettingsXml.Element("saveLog") ?? (bool?)CommonSettingsXml.Element("saveLog") ?? true; // save the console log to file
            ConsoleLogRedacted = (bool?)CharacterSettingsXml.Element("saveLogRedacted") ?? (bool?)CommonSettingsXml.Element("saveLogRedacted") ?? true; // save the console log redacted to file
            SessionsLog = (bool?)CharacterSettingsXml.Element("SessionsLog") ?? (bool?)CommonSettingsXml.Element("SessionsLog") ?? true;
            DroneStatsLog = (bool?)CharacterSettingsXml.Element("DroneStatsLog") ?? (bool?)CommonSettingsXml.Element("DroneStatsLog") ?? true;
            WreckLootStatistics = (bool?)CharacterSettingsXml.Element("WreckLootStatistics") ?? (bool?)CommonSettingsXml.Element("WreckLootStatistics") ?? true;
            MissionStats1Log = (bool?)CharacterSettingsXml.Element("MissionStats1Log") ?? (bool?)CommonSettingsXml.Element("MissionStats1Log") ?? true;
            MissionStats2Log = (bool?)CharacterSettingsXml.Element("MissionStats2Log") ?? (bool?)CommonSettingsXml.Element("MissionStats2Log") ?? true;
            MissionStats3Log = (bool?)CharacterSettingsXml.Element("MissionStats3Log") ?? (bool?)CommonSettingsXml.Element("MissionStats3Log") ?? true;
            MissionDungeonIdLog = (bool?)CharacterSettingsXml.Element("MissionDungeonIdLog") ?? (bool?)CommonSettingsXml.Element("MissionDungeonIdLog") ?? true;
            PocketStatistics = (bool?)CharacterSettingsXml.Element("PocketStatistics") ?? (bool?)CommonSettingsXml.Element("PocketStatistics") ?? true;
            PocketStatsUseIndividualFilesPerPocket = (bool?)CharacterSettingsXml.Element("PocketStatsUseIndividualFilesPerPocket") ?? (bool?)CommonSettingsXml.Element("PocketStatsUseIndividualFilesPerPocket") ?? true;
            PocketObjectStatisticsLog = (bool?)CharacterSettingsXml.Element("PocketObjectStatisticsLog") ?? (bool?)CommonSettingsXml.Element("PocketObjectStatisticsLog") ?? true;
            VolleyStatsLog = (bool?)CharacterSettingsXml.Element("VolleyStatsLog") ?? (bool?)CommonSettingsXml.Element("VolleyStatsLog") ?? true;
            WindowStatsLog = (bool?)CharacterSettingsXml.Element("WindowStatsLog") ?? (bool?)CommonSettingsXml.Element("WindowStatsLog") ?? true;

            //
            // Weapon and targeting Settings
            //
            WeaponGroupId = (int?)CharacterSettingsXml.Element("weaponGroupId") ?? (int?)CommonSettingsXml.Element("weaponGroupId") ?? 0;
            DontShootFrigatesWithSiegeorAutoCannons = (bool?)CharacterSettingsXml.Element("DontShootFrigatesWithSiegeorAutoCannons") ?? (bool?)CommonSettingsXml.Element("DontShootFrigatesWithSiegeorAutoCannons") ?? false;
            MaximumHighValueTargets = (int?)CharacterSettingsXml.Element("maximumHighValueTargets") ?? (int?)CommonSettingsXml.Element("maximumHighValueTargets") ?? 2;
            MaximumLowValueTargets = (int?)CharacterSettingsXml.Element("maximumLowValueTargets") ?? (int?)CommonSettingsXml.Element("maximumLowValueTargets") ?? 2;
            DoNotSwitchTargetsIfTargetHasMoreThanThisArmorDamagePercentage = (int?)CharacterSettingsXml.Element("doNotSwitchTargetsIfTargetHasMoreThanThisArmorDamagePercentage") ?? (int?)CommonSettingsXml.Element("doNotSwitchTargetsIfTargetHasMoreThanThisArmorDamagePercentage") ?? 60;
            DistanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons = (int?)CharacterSettingsXml.Element("distanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons") ?? (int?)CommonSettingsXml.Element("distanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons") ?? 7000; //also requires SpeedFrigatesShouldBeIgnoredByMainWeapons
            SpeedNPCFrigatesShouldBeIgnoredByPrimaryWeapons = (int?)CharacterSettingsXml.Element("speedNPCFrigatesShouldBeIgnoredByPrimaryWeapons") ?? (int?)CommonSettingsXml.Element("speedNPCFrigatesShouldBeIgnoredByPrimaryWeapons") ?? 300; //also requires DistanceFrigatesShouldBeIgnoredByMainWeapons
            ArmLoadCapBoosters = (bool?)CharacterSettingsXml.Element("armLoadCapBoosters") ?? (bool?)CommonSettingsXml.Element("armLoadCapBoosters") ?? false;
            SelectAmmoToUseBasedOnShipSize = (bool?)CharacterSettingsXml.Element("selectAmmoToUseBasedOnShipSize") ?? (bool?)CommonSettingsXml.Element("selectAmmoToUseBasedOnShipSize") ?? false;

            MinimumTargetValueToConsiderTargetAHighValueTarget = (int?)CharacterSettingsXml.Element("minimumTargetValueToConsiderTargetAHighValueTarget") ?? (int?)CommonSettingsXml.Element("minimumTargetValueToConsiderTargetAHighValueTarget") ?? 2;
            MaximumTargetValueToConsiderTargetALowValueTarget = (int?)CharacterSettingsXml.Element("maximumTargetValueToConsiderTargetALowValueTarget") ?? (int?)CommonSettingsXml.Element("maximumTargetValueToConsiderTargetALowValueTarget") ?? 1;

            AddDampenersToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addDampenersToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addDampenersToPrimaryWeaponsPriorityTargetList") ?? true;
            AddECMsToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addECMsToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addECMsToPrimaryWeaponsPriorityTargetList") ?? true;
            AddNeutralizersToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addNeutralizersToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addNeutralizersToPrimaryWeaponsPriorityTargetList") ?? true;
            AddTargetPaintersToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addTargetPaintersToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addTargetPaintersToPrimaryWeaponsPriorityTargetList") ?? true;
            AddTrackingDisruptorsToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addTrackingDisruptorsToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addTrackingDisruptorsToPrimaryWeaponsPriorityTargetList") ?? true;
            AddWarpScramblersToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addWarpScramblersToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addWarpScramblersToPrimaryWeaponsPriorityTargetList") ?? true;
            AddWebifiersToPrimaryWeaponsPriorityTargetList = (bool?)CharacterSettingsXml.Element("addWebifiersToPrimaryWeaponsPriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addWebifiersToPrimaryWeaponsPriorityTargetList") ?? true;

            AddDampenersToDronePriorityTargetList = (bool?)CharacterSettingsXml.Element("addDampenersToDronePriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addDampenersToDronePriorityTargetList") ?? true;
            AddECMsToDroneTargetList = (bool?)CharacterSettingsXml.Element("addECMsToDroneTargetList") ?? (bool?)CommonSettingsXml.Element("addECMsToDroneTargetList") ?? true;
            AddNeutralizersToDronePriorityTargetList = (bool?)CharacterSettingsXml.Element("addNeutralizersToDronePriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addNeutralizersToDronePriorityTargetList") ?? true;
            AddTargetPaintersToDronePriorityTargetList = (bool?)CharacterSettingsXml.Element("addTargetPaintersToDronePriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addTargetPaintersToDronePriorityTargetList") ?? true;
            AddTrackingDisruptorsToDronePriorityTargetList = (bool?)CharacterSettingsXml.Element("addTrackingDisruptorsToDronePriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addTrackingDisruptorsToDronePriorityTargetList") ?? true;
            AddWarpScramblersToDronePriorityTargetList = (bool?)CharacterSettingsXml.Element("addWarpScramblersToDronePriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addWarpScramblersToDronePriorityTargetList") ?? true;
            AddWebifiersToDronePriorityTargetList = (bool?)CharacterSettingsXml.Element("addWebifiersToDronePriorityTargetList") ?? (bool?)CommonSettingsXml.Element("addWebifiersToDronePriorityTargetList") ?? true;

            ListPriorityTargetsEveryXSeconds = (double?)CharacterSettingsXml.Element("listPriorityTargetsEveryXSeconds") ?? (double?)CommonSettingsXml.Element("listPriorityTargetsEveryXSeconds") ?? 900;

            InsideThisRangeIsHardToTrack = (double?)CharacterSettingsXml.Element("insideThisRangeIsHardToTrack") ?? (double?)CommonSettingsXml.Element("insideThisRangeIsHardToTrack") ?? 15000;

            //
            // Script Settings - TypeIDs for the scripts you would like to use in these modules
            //
            // 29003 Focused Warp Disruption Script   // hictor and InfiniPoint
            //
            // 29007 Tracking Speed Disruption Script // tracking disruptor
            // 29005 Optimal Range Disruption Script  // tracking disruptor
            // 29011 Scan Resolution Script           // sensor booster
            // 29009 Targeting Range Script           // sensor booster
            // 29015 Targeting Range Dampening Script // sensor dampener
            // 29013 Scan Resolution Dampening Script // sensor dampener
            // 29001 Tracking Speed Script            // tracking enhancer and tracking computer
            // 28999 Optimal Range Script             // tracking enhancer and tracking computer

            // 3554  Cap Booster 100
            // 11283 Cap Booster 150
            // 11285 Cap Booster 200
            // 263   Cap Booster 25
            // 11287 Cap Booster 400
            // 264   Cap Booster 50
            // 3552  Cap Booster 75
            // 11289 Cap Booster 800
            // 31982 Navy Cap Booster 100
            // 31990 Navy Cap Booster 150
            // 31998 Navy Cap Booster 200
            // 32006 Navy Cap Booster 400
            // 32014 Navy Cap Booster 800

            TrackingDisruptorScript = (int?)CharacterSettingsXml.Element("trackingDisruptorScript") ?? (int?)CommonSettingsXml.Element("trackingDisruptorScript") ?? (int)TypeID.TrackingSpeedDisruptionScript;
            TrackingComputerScript = (int?)CharacterSettingsXml.Element("trackingComputerScript") ?? (int?)CommonSettingsXml.Element("trackingComputerScript") ?? (int)TypeID.TrackingSpeedScript;
            TrackingLinkScript = (int?)CharacterSettingsXml.Element("trackingLinkScript") ?? (int?)CommonSettingsXml.Element("trackingLinkScript") ?? (int)TypeID.TrackingSpeedScript;
            SensorBoosterScript = (int?)CharacterSettingsXml.Element("sensorBoosterScript") ?? (int?)CommonSettingsXml.Element("sensorBoosterScript") ?? (int)TypeID.TargetingRangeScript;
            SensorDampenerScript = (int?)CharacterSettingsXml.Element("sensorDampenerScript") ?? (int?)CommonSettingsXml.Element("sensorDampenerScript") ?? (int)TypeID.TargetingRangeDampeningScript;
            AncillaryShieldBoosterScript = (int?)CharacterSettingsXml.Element("ancillaryShieldBoosterScript") ?? (int?)CommonSettingsXml.Element("ancillaryShieldBoosterScript") ?? (int)TypeID.AncillaryShieldBoosterScript;
            CapacitorInjectorScript = (int?)CharacterSettingsXml.Element("capacitorInjectorScript") ?? (int?)CommonSettingsXml.Element("capacitorInjectorScript") ?? (int)TypeID.CapacitorInjectorScript;
            CapBoosterToLoad = (int?)CharacterSettingsXml.Element("capacitorInjectorToLoad") ?? (int?)CommonSettingsXml.Element("capacitorInjectorToLoad") ?? (int?)CharacterSettingsXml.Element("capBoosterToLoad") ?? (int?)CommonSettingsXml.Element("capBoosterToLoad") ?? 15;

            //
            // OverLoad Settings (this WILL burn out modules, likely very quickly!
            // If you enable the overloading of a slot it is HIGHLY recommended you actually have something overloadable in that slot =/ 
            //
            OverloadWeapons = (bool?)CharacterSettingsXml.Element("overloadWeapons") ?? (bool?)CommonSettingsXml.Element("overloadWeapons") ?? false;

            //
            // Speed and Movement Settings
            //
            AvoidBumpingThings = (bool?)CharacterSettingsXml.Element("avoidBumpingThings") ?? (bool?)CommonSettingsXml.Element("avoidBumpingThings") ?? true;
            SpeedTank = (bool?)CharacterSettingsXml.Element("speedTank") ?? (bool?)CommonSettingsXml.Element("speedTank") ?? false;
            OrbitDistance = (int?)CharacterSettingsXml.Element("orbitDistance") ?? (int?)CommonSettingsXml.Element("orbitDistance") ?? 0;
            OrbitStructure = (bool?)CharacterSettingsXml.Element("orbitStructure") ?? (bool?)CommonSettingsXml.Element("orbitStructure") ?? false;
            OptimalRange = (int?)CharacterSettingsXml.Element("optimalRange") ?? (int?)CommonSettingsXml.Element("optimalRange") ?? 0;
            NosDistance = (int?)CharacterSettingsXml.Element("NosDistance") ?? (int?)CommonSettingsXml.Element("NosDistance") ?? 38000;
            RemoteRepairDistance = (int?)CharacterSettingsXml.Element("remoteRepairDistance") ?? (int?)CommonSettingsXml.Element("remoteRepairDistance") ?? 2000;
            MinimumPropulsionModuleDistance = (int?)CharacterSettingsXml.Element("minimumPropulsionModuleDistance") ?? (int?)CommonSettingsXml.Element("minimumPropulsionModuleDistance") ?? 5000;
            MinimumPropulsionModuleCapacitor = (int?)CharacterSettingsXml.Element("minimumPropulsionModuleCapacitor") ?? (int?)CommonSettingsXml.Element("minimumPropulsionModuleCapacitor") ?? 0;

            //
            // Tanking Settings
            //
            ActivateRepairModules = (int?)CharacterSettingsXml.Element("activateRepairModules") ?? (int?)CommonSettingsXml.Element("activateRepairModules") ?? 65;
            DeactivateRepairModules = (int?)CharacterSettingsXml.Element("deactivateRepairModules") ?? (int?)CommonSettingsXml.Element("deactivateRepairModules") ?? 95;
            InjectCapPerc = (int?)CharacterSettingsXml.Element("injectcapperc") ?? (int?)CommonSettingsXml.Element("injectcapperc") ?? 60;

            //
            // Panic Settings
            //
            MinimumShieldPct = (int?)CharacterSettingsXml.Element("minimumShieldPct") ?? (int?)CommonSettingsXml.Element("minimumShieldPct") ?? 100;
            MinimumArmorPct = (int?)CharacterSettingsXml.Element("minimumArmorPct") ?? (int?)CommonSettingsXml.Element("minimumArmorPct") ?? 100;
            MinimumCapacitorPct = (int?)CharacterSettingsXml.Element("minimumCapacitorPct") ?? (int?)CommonSettingsXml.Element("minimumCapacitorPct") ?? 50;
            SafeShieldPct = (int?)CharacterSettingsXml.Element("safeShieldPct") ?? (int?)CommonSettingsXml.Element("safeShieldPct") ?? 90;
            SafeArmorPct = (int?)CharacterSettingsXml.Element("safeArmorPct") ?? (int?)CommonSettingsXml.Element("safeArmorPct") ?? 90;
            SafeCapacitorPct = (int?)CharacterSettingsXml.Element("safeCapacitorPct") ?? (int?)CommonSettingsXml.Element("safeCapacitorPct") ?? 80;
            UseStationRepair = (bool?)CharacterSettingsXml.Element("useStationRepair") ?? (bool?)CommonSettingsXml.Element("useStationRepair") ?? true;

            //
            // Drone Settings
            //
            UseDrones = (bool?)CharacterSettingsXml.Element("useDrones") ?? (bool?)CommonSettingsXml.Element("useDrones") ?? true;
            DroneTypeId = (int?)CharacterSettingsXml.Element("droneTypeId") ?? (int?)CommonSettingsXml.Element("droneTypeId") ?? 0;
            DroneControlRange = (int?)CharacterSettingsXml.Element("droneControlRange") ?? (int?)CommonSettingsXml.Element("droneControlRange") ?? 0;
            DronesDontNeedTargetsBecauseWehaveThemSetOnAggressive = (bool?)CharacterSettingsXml.Element("dronesDontNeedTargetsBecauseWehaveThemSetOnAggressive") ?? (bool?)CommonSettingsXml.Element("dronesDontNeedTargetsBecauseWehaveThemSetOnAggressive") ?? true;
            DroneMinimumShieldPct = (int?)CharacterSettingsXml.Element("droneMinimumShieldPct") ?? (int?)CommonSettingsXml.Element("droneMinimumShieldPct") ?? 50;
            DroneMinimumArmorPct = (int?)CharacterSettingsXml.Element("droneMinimumArmorPct") ?? (int?)CommonSettingsXml.Element("droneMinimumArmorPct") ?? 50;
            DroneMinimumCapacitorPct = (int?)CharacterSettingsXml.Element("droneMinimumCapacitorPct") ?? (int?)CommonSettingsXml.Element("droneMinimumCapacitorPct") ?? 0;
            DroneRecallShieldPct = (int?)CharacterSettingsXml.Element("droneRecallShieldPct") ?? (int?)CommonSettingsXml.Element("droneRecallShieldPct") ?? 0;
            DroneRecallArmorPct = (int?)CharacterSettingsXml.Element("droneRecallArmorPct") ?? (int?)CommonSettingsXml.Element("droneRecallArmorPct") ?? 0;
            DroneRecallCapacitorPct = (int?)CharacterSettingsXml.Element("droneRecallCapacitorPct") ?? (int?)CommonSettingsXml.Element("droneRecallCapacitorPct") ?? 0;
            LongRangeDroneRecallShieldPct = (int?)CharacterSettingsXml.Element("longRangeDroneRecallShieldPct") ?? (int?)CommonSettingsXml.Element("longRangeDroneRecallShieldPct") ?? 0;
            LongRangeDroneRecallArmorPct = (int?)CharacterSettingsXml.Element("longRangeDroneRecallArmorPct") ?? (int?)CommonSettingsXml.Element("longRangeDroneRecallArmorPct") ?? 0;
            LongRangeDroneRecallCapacitorPct = (int?)CharacterSettingsXml.Element("longRangeDroneRecallCapacitorPct") ?? (int?)CommonSettingsXml.Element("longRangeDroneRecallCapacitorPct") ?? 0;
            DronesKillHighValueTargets = (bool?)CharacterSettingsXml.Element("dronesKillHighValueTargets") ?? (bool?)CommonSettingsXml.Element("dronesKillHighValueTargets") ?? false;
            BelowThisHealthLevelRemoveFromDroneBay = (int?)CharacterSettingsXml.Element("belowThisHealthLevelRemoveFromDroneBay") ?? (int?)CommonSettingsXml.Element("belowThisHealthLevelRemoveFromDroneBay") ?? 150;

            //
            // Email Settings
            //
            EmailSupport = (bool?)CharacterSettingsXml.Element("emailSupport") ?? (bool?)CommonSettingsXml.Element("emailSupport") ?? false;
            EmailAddress = (string)CharacterSettingsXml.Element("emailAddress") ?? (string)CommonSettingsXml.Element("emailAddress") ?? "";
            EmailPassword = (string)CharacterSettingsXml.Element("emailPassword") ?? (string)CommonSettingsXml.Element("emailPassword") ?? "";
            EmailSMTPServer = (string)CharacterSettingsXml.Element("emailSMTPServer") ?? (string)CommonSettingsXml.Element("emailSMTPServer") ?? "";
            EmailSMTPPort = (int?)CharacterSettingsXml.Element("emailSMTPPort") ?? (int?)CommonSettingsXml.Element("emailSMTPPort") ?? 25;
            EmailAddressToSendAlerts = (string)CharacterSettingsXml.Element("emailAddressToSendAlerts") ?? (string)CommonSettingsXml.Element("emailAddressToSendAlerts") ?? "";
            EmailEnableSSL = (bool?)CharacterSettingsXml.Element("emailEnableSSL") ?? (bool?)CommonSettingsXml.Element("emailEnableSSL") ?? false;

            //
            // Skill Training Settings
            //
            ThisToonShouldBeTrainingSkills = (bool?)CharacterSettingsXml.Element("thisToonShouldBeTrainingSkills") ?? (bool?)CommonSettingsXml.Element("thisToonShouldBeTrainingSkills") ?? true;
            
            //
            // User Defined LavishScript Scripts that tie to buttons in the UI
            //
            UserDefinedLavishScriptScript1 = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript1") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript1") ?? "";
            UserDefinedLavishScriptScript1Description = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript1Description") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript1Description") ?? "";
            UserDefinedLavishScriptScript2 = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript2") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript2") ?? "";
            UserDefinedLavishScriptScript2Description = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript2Description") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript2Description") ?? "";
            UserDefinedLavishScriptScript3 = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript3") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript3") ?? "";
            UserDefinedLavishScriptScript3Description = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript3Description") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript3Description") ?? "";
            UserDefinedLavishScriptScript4 = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript4") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript4") ?? "";
            UserDefinedLavishScriptScript4Description = (string)CharacterSettingsXml.Element("userDefinedLavishScriptScript4Description") ?? (string)CommonSettingsXml.Element("userDefinedLavishScriptScript4Description") ?? "";

            LoadQuestorDebugInnerspaceCommandAlias = (string)CharacterSettingsXml.Element("loadQuestorDebugInnerspaceCommandAlias") ?? (string)CommonSettingsXml.Element("loadQuestorDebugInnerspaceCommandAlias") ?? "1";
            LoadQuestorDebugInnerspaceCommand = (string)CharacterSettingsXml.Element("loadQuestorDebugInnerspaceCommand") ?? (string)CommonSettingsXml.Element("loadQuestorDebugInnerspaceCommand") ?? "dotnet q1 questor.exe";
            UnLoadQuestorDebugInnerspaceCommandAlias = (string)CharacterSettingsXml.Element("unLoadQuestorDebugInnerspaceCommandAlias") ?? (string)CommonSettingsXml.Element("unLoadQuestorDebugInnerspaceCommandAlias") ?? "2";
            UnLoadQuestorDebugInnerspaceCommand = (string)CharacterSettingsXml.Element("unLoadQuestorDebugInnerspaceCommand") ?? (string)CommonSettingsXml.Element("unLoadQuestorDebugInnerspaceCommand") ?? "dotnet -unload q1";
            
            //
            // agent standing requirements
            //
            StandingsNeededToAccessLevel1Agent = (float?)CharacterSettingsXml.Element("standingsNeededToAccessLevel1Agent") ?? (float?)CommonSettingsXml.Element("standingsNeededToAccessLevel1Agent") ?? -11;
            StandingsNeededToAccessLevel2Agent = (float?)CharacterSettingsXml.Element("standingsNeededToAccessLevel2Agent") ?? (float?)CommonSettingsXml.Element("standingsNeededToAccessLevel2Agent") ?? 1;
            StandingsNeededToAccessLevel3Agent = (float?)CharacterSettingsXml.Element("standingsNeededToAccessLevel3Agent") ?? (float?)CommonSettingsXml.Element("standingsNeededToAccessLevel3Agent") ?? 3;
            StandingsNeededToAccessLevel4Agent = (float?)CharacterSettingsXml.Element("standingsNeededToAccessLevel4Agent") ?? (float?)CommonSettingsXml.Element("standingsNeededToAccessLevel4Agent") ?? 5;
            StandingsNeededToAccessLevel5Agent = (float?)CharacterSettingsXml.Element("standingsNeededToAccessLevel5Agent") ?? (float?)CommonSettingsXml.Element("standingsNeededToAccessLevel5Agent") ?? 7;

            //
            // number of days of console logs to keep (anything older will be deleted on startup)
            //
            ConsoleLogDaysOfLogsToKeep = (int?)CharacterSettingsXml.Element("consoleLogDaysOfLogsToKeep") ?? (int?)CommonSettingsXml.Element("consoleLogDaysOfLogsToKeep") ?? 14;

            //
            // Ammo settings
            //
            Ammo.Clear();
            XElement ammoTypes = CharacterSettingsXml.Element("ammoTypes") ?? CommonSettingsXml.Element("ammoTypes");

            if (ammoTypes != null)
            {
                foreach (XElement ammo in ammoTypes.Elements("ammoType"))
                {
                    Ammo.Add(new Ammo(ammo));
                }
            }

            MinimumAmmoCharges = (int?)CharacterSettingsXml.Element("minimumAmmoCharges") ?? (int?)CommonSettingsXml.Element("minimumAmmoCharges") ?? 2;
            if (MinimumAmmoCharges < 2) MinimumAmmoCharges = 2; //do not allow MinimumAmmoCharges to be set lower than 1. We always want to reload before the weapon is empty!

            //
            // List of Agents we should use
            //
            //if (Settings.Instance.CharacterMode.ToLower() == "Combat Missions".ToLower())
            //{
            ListOfAgents.Clear();
            XElement agentList = CharacterSettingsXml.Element("agentsList") ?? CommonSettingsXml.Element("agentsList");

            if (agentList != null)
            {
                if (agentList.HasElements)
                {
                    foreach (XElement agent in agentList.Elements("agentList"))
                    {
                        ListOfAgents.Add(new AgentsList(agent));
                    }
                }
                else
                {
                    Logging.Log("Settings", "agentList exists in your characters config but no agents were listed.", Logging.Red);
                }
            }
            else
                Logging.Log("Settings", "Error! No Agents List specified.", Logging.Red);

            //}

            //
            // Fittings chosen based on the faction of the mission
            //
            ListofFactionFittings.Clear();
            
            if (UseFittingManager) //no need to look for or load these settings if FittingManager is disabled
            {
                XElement factionFittings = CharacterSettingsXml.Element("factionFittings") ?? 
                                           CharacterSettingsXml.Element("factionfittings") ??
                                           CommonSettingsXml.Element("factionFittings") ?? 
                                           CommonSettingsXml.Element("factionfittings");

                if (factionFittings != null)
                {
                    string factionFittingXMLElementName = "";
                    if (factionFittings.Elements("factionFitting").Any())
                    {
                        factionFittingXMLElementName = "factionFitting";
                    }
                    else
                    {
                        factionFittingXMLElementName = "factionfitting";
                    }

                    int i = 0;
                    foreach (XElement factionfitting in factionFittings.Elements(factionFittingXMLElementName))
                    {
                        i++;
                        ListofFactionFittings.Add(new FactionFitting(factionfitting));
                        if (Settings.Instance.DebugFittingMgr) Logging.Log("Settings.LoadMFactionFitting", "[" + i + "] Faction Fitting [" + factionfitting + "]", Logging.Teal);
                        
                    }

                    //
                    // if we have no default fitting do not use fitting manager
                    //
                    if (ListofFactionFittings.Exists(m => m.FactionName.ToLower() == "default"))
                    {
                        DefaultFitting = ListofFactionFittings.Find(m => m.FactionName.ToLower() == "default");
                        if (string.IsNullOrEmpty(DefaultFitting.FittingName))
                        {
                            UseFittingManager = false;
                            Logging.Log("Settings", "Error! No default fitting specified or fitting is incorrect.  Fitting manager will not be used.", Logging.Orange);
                        }

                        Logging.Log("Settings", "Faction Fittings defined. Fitting manager will be used when appropriate.", Logging.White);
                    }
                    else
                    {
                        UseFittingManager = false;
                        Logging.Log("Settings", "Error! No default fitting specified or fitting is incorrect.  Fitting manager will not be used.", Logging.Orange);
                    }
                }
                else
                {
                    UseFittingManager = false;
                    Logging.Log("Settings", "No faction fittings specified.  Fitting manager will not be used.", Logging.Orange);
                }
            }

            //
            // Fitting based on the name of the mission
            //
            ListOfMissionFittings.Clear();
            XElement xmlElementMissionFittingsSection = CharacterSettingsXml.Element("missionfittings") ?? CommonSettingsXml.Element("missionfittings");
            if (UseFittingManager) //no need to look for or load these settings if FittingManager is disabled
            {
                if (xmlElementMissionFittingsSection != null)
                {
                    Logging.Log("Settings", "Loading Mission Fittings", Logging.White);
                    int i = 0;
                    foreach (XElement missionfitting in xmlElementMissionFittingsSection.Elements("missionfitting"))
                    {
                        i++; 
                        ListOfMissionFittings.Add(new MissionFitting(missionfitting));
                        if (Settings.Instance.DebugFittingMgr) Logging.Log("Settings.LoadMissionFittings", "[" + i + "] Mission Fitting [" + missionfitting + "]", Logging.Teal);
                        
                    }
                    Logging.Log("Settings", "        Mission Fittings now has [" + ListOfMissionFittings.Count + "] entries", Logging.White);
                }
            }

            LoadMissionBlackList(CharacterSettingsXml, CommonSettingsXml);

            LoadMissionGreyList(CharacterSettingsXml, CommonSettingsXml);
            
            LoadFactionBlacklist(CharacterSettingsXml, CommonSettingsXml);
        }

        public void LoadSettings()
        {
            try
            {
                if (Cache.Instance.ScheduleCharacterName != null)
                {
                    Settings.Instance.CharacterName = Cache.Instance.ScheduleCharacterName;
                    //Logging.Log("Settings", "CharacterName was pulled from the Scheduler: [" + Settings.Instance.CharacterName + "]", Logging.White);
                }
                else
                {
                    Settings.Instance.CharacterName = Cache.Instance.DirectEve.Me.Name;
                    //Logging.Log("Settings", "CharacterName was pulled from your live EVE session: [" + Settings.Instance.CharacterName + "]", Logging.White);
                }
            }
            catch (Exception)
            {
                Settings.Instance.CharacterName = "AtLoginScreenNoCharactersLoggedInYet";
            }

            Settings.Instance.CharacterSettingsPath = System.IO.Path.Combine(Settings.Instance.Path, Cache.Instance.FilterPath(Settings.Instance.CharacterName) + ".xml");
            //Settings.Instance.CommonSettingsPath = System.IO.Path.Combine(Settings.Instance.Path, Settings.Instance.CommonSettingsFileName);

            if (Settings.Instance.CharacterSettingsPath == System.IO.Path.Combine(Settings.Instance.Path, ".xml"))
            {
                if (DateTime.UtcNow > Time.Instance.LastSessionChange.AddSeconds(30))
                {
                    Cache.Instance.ReasonToStopQuestor = "CharacterName not defined! - Are we still logged in? Did we lose connection to eve? Questor should be restarting here.";
                    Logging.Log("Settings", "CharacterName not defined! - Are we still logged in? Did we lose connection to eve? Questor should be restarting here.", Logging.White);
                    Settings.Instance.CharacterName = "NoCharactersLoggedInAnymore";
                    Cache.Instance.EnteredCloseQuestor_DateTime = DateTime.UtcNow;
                    Cache.Instance.SessionState = "Quitting";
                    _States.CurrentQuestorState = QuestorState.CloseQuestor;
                    Cleanup.CloseQuestor(Cache.Instance.ReasonToStopQuestor);
                    return;
                }

                Logging.Log("Settings", "CharacterName not defined! - Are we logged in yet? Did we lose connection to eve?", Logging.White);
                Settings.Instance.CharacterName = "AtLoginScreenNoCharactersLoggedInYet";
                //Cache.Instance.SessionState = "Quitting";
            }

            bool reloadSettings = true;
            if (File.Exists(Settings.Instance.CharacterSettingsPath))
            {
                reloadSettings = _lastModifiedDateOfMySettingsFile != File.GetLastWriteTime(Settings.Instance.CharacterSettingsPath);
                _lastModifiedDateOfMySettingsFile = File.GetLastWriteTime(CharacterSettingsPath);
            }

            if (File.Exists(Settings.Instance.CommonSettingsPath))
            {
                reloadSettings = _lastModifiedDateOfCommonSettingsFile != File.GetLastWriteTime(Settings.Instance.CommonSettingsPath);
                _lastModifiedDateOfCommonSettingsFile = File.GetLastWriteTime(Settings.Instance.CommonSettingsPath);
            }

            if (!reloadSettings)
                return;

            Settings.Instance.EVEMemoryManager = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "MemManager.exe")); //https://github.com/VendanAndrews/EveMemManager
            Settings.Instance.FactionXMLExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "faction.XML"));
            Settings.Instance.SchedulesXMLExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "schedules.XML"));
            Settings.Instance.QuestorManagerExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "QuestorManager.exe"));
            Settings.Instance.QuestorSettingsExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "QuestorSettings.exe"));
            Settings.Instance.QuestorStatisticsExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "QuestorStatistics.exe"));

            if (!File.Exists(Settings.Instance.CharacterSettingsPath) && !Settings.Instance.DefaultSettingsLoaded) //if the settings file does not exist initialize these values. Should we not halt when missing the settings XML?
            {
                Settings.Instance.CharacterXMLExists = false;
                DefaultSettingsLoaded = true;
                //LavishScript.ExecuteCommand("log " + Cache.Instance.DirectEve.Me.Name + ".log");
                //LavishScript.ExecuteCommand("uplink echo Settings: unable to find [" + Settings.Instance.SettingsPath + "] loading default (bad! bad! bad!) settings: you should fix this! NOW.");
                Logging.Log("Settings", "WARNING! unable to find [" + Settings.Instance.CharacterSettingsPath + "] loading default generic, and likely incorrect, settings: WARNING!", Logging.Orange);
                DebugActivateGate = false;
                DebugActivateWeapons = false;
                DebugAddDronePriorityTarget = false;
                DebugAddPrimaryWeaponPriorityTarget = false;
                DebugAgentInteractionReplyToAgent = false;
                DebugAllMissionsOnBlackList = false;
                DebugAllMissionsOnGreyList = false;
                DebugArm = false;
                DebugAttachVSDebugger = false;
                DebugAutoStart = false;
                DebugBlackList = false;
                DebugCargoHold = false;
                DebugChat = false;
                DebugCleanup = false;
                DebugClearPocket = false;
                DebugCourierMissions = false;
                DebugDecline = false;
                DebugDefense = false;
                DebugDisableCleanup = false;
                DebugDisableCombatMissionsBehavior = false;
                DebugDisableCombatMissionCtrl = false;
                DebugDisableCombat = false;
                DebugDisableDrones = false;
                DebugDisablePanic = false;
                DebugDisableSalvage = false;
                DebugDisableGetBestTarget = false;
                DebugDisableTargetCombatants = false;
                DebugDisableNavigateIntoRange = false;
                DebugDrones = false;
                DebugDroneHealth = false;
                DebugExceptions = false;
                DebugFittingMgr = false;
                DebugFleetSupportSlave = false;
                DebugFleetSupportMaster = false;
                DebugGetBestTarget = false;
                DebugGetBestDroneTarget = false;
                DebugGotobase = false;
                DebugGreyList = false;
                DebugHangars = false;
                DebugHasExploded = false;
                DebugIdle = false;
                DebugInWarp = false;
                DebugItemHangar = false;
                DebugKillTargets = false;
                DebugKillAction = false;
                DebugLoadScripts = false;
                DebugLogging = false;
                DebugLootWrecks = false;
                DebugLootValue = false;
                DebugMaintainConsoleLogs = false;
                DebugMiningBehavior = false;
                DebugMissionFittings = false;
                DebugMoveTo = false;
                DebugNavigateOnGrid = false;
                DebugOnframe = false;
                DebugOverLoadWeapons = false;
                DebugPerformance = false;
                DebugPotentialCombatTargets = false;
                DebugQuestorManager = false;
                DebugReloadAll = false;
                DebugReloadorChangeAmmo = false;
                DebugRemoteRepair = false;
                DebugSalvage = false;
                DebugScheduler = false;
                DebugSettings = false;
                DebugShipTargetValues = false;
                DebugSkillTraining = true;
                DebugStates = false;
                DebugStatistics = false;
                DebugStorylineMissions = false;
                DebugTargetCombatants = false;
                DebugTargetWrecks = false;
                DebugTractorBeams = false;
                DebugTraveler = false;
                DebugUI = false;
                DebugUnloadLoot = false;
                DebugValuedump = false;
                DebugWalletBalance = false;
                DebugWatchForActiveWars = true;
                DetailedCurrentTargetHealthLogging = false;
                DefendWhileTraveling = true;
                UseInnerspace = true;
                setEveClientDestinationWhenTraveling = false;

                CharacterToAcceptInvitesFrom = Settings.Instance.CharacterName;
                //
                // Misc Settings
                //
                CharacterMode = "none";
                AutoStart = false; // auto Start enabled or disabled by default
                // maximum console log lines to show in the GUI
                Disable3D = false; // Disable3d graphics while in space
                RandomDelay = 15;
                MinimumDelay = 20;
                //
                // Enable / Disable Major Features that do not have categories of their own below
                //
                UseFittingManager = false;
                EnableStorylines = false;
                DeclineStorylinesInsteadofBlacklistingfortheSession = false;
                UseLocalWatch = false;
                WatchForActiveWars = true;

                FleetSupportSlave = false;
                FleetSupportMaster = false;
                FleetName = "Fleet1";
                CharacterNamesForMasterToInviteToFleet.Clear();

                // Console Log Settings
                //
                SaveConsoleLog = true; // save the console log to file
                MaxLineConsole = 1000;
                //
                // Agent Standings and Mission Settings
                //
                MinAgentBlackListStandings = 1;
                MinAgentGreyListStandings = (float)-1.7;
                WaitDecline = false;
                const string relativeMissionsPath = "Missions";
                MissionsPath = System.IO.Path.Combine(Settings.Instance.Path, relativeMissionsPath);
                //Logging.Log("Settings","Default MissionXMLPath is: [" + MissionsPath + "]",Logging.White);
                RequireMissionXML = false;
                AllowNonStorylineCourierMissionsInLowSec = false;
                MaterialsForWarOreID = 20;
                MaterialsForWarOreQty = 8000;
                KillSentries = false;
                //
                // Local Watch Settings - if enabled
                //
                LocalBadStandingPilotsToTolerate = 1;
                LocalBadStandingLevelToConsiderBad = -0.1;
                //
                // Invasion Settings
                //
                BattleshipInvasionLimit = 2;
                // if this number of BattleShips lands on grid while in a mission we will enter panic
                BattlecruiserInvasionLimit = 2;
                // if this number of BattleCruisers lands on grid while in a mission we will enter panic
                CruiserInvasionLimit = 2;
                // if this number of cruisers lands on grid while in a mission we will enter panic
                FrigateInvasionLimit = 2;
                // if this number of frigates lands on grid while in a mission we will enter panic
                InvasionRandomDelay = 30; // random relay to stay docked
                InvasionMinimumDelay = 30; // minimum delay to stay docked

                //
                // Questor GUI Window Position
                //
                WindowXPosition = 400;
                WindowYPosition = 600;
                //
                // Salvage and loot settings
                //
                ReserveCargoCapacity = 0;
                MaximumWreckTargets = 0;

                //
                // at what memory usage do we need to restart this session?
                //
                EVEProcessMemoryCeiling = 2048;
                EVEProcessMemoryCeilingLogofforExit = "exit";

                CloseQuestorCMDUplinkInnerspaceProfile = true;
                CloseQuestorCMDUplinkIsboxerCharacterSet = false;
                CloseQuestorAllowRestart = true;

                CloseQuestorArbitraryOSCmd = false; //true or false
                CloseQuestorOSCmdContents = string.Empty;
                //the above setting can be set to any script or commands available on the system. make sure you test it from a command prompt while in your .net programs directory

                LoginQuestorArbitraryOSCmd = false;
                LoginQuestorOSCmdContents = String.Empty;
                LoginQuestorLavishScriptCmd = false;
                LoginQuestorLavishScriptContents = string.Empty;
                MinimizeEveAfterStartingUp = false;

                WalletBalanceChangeLogOffDelay = 30;
                WalletBalanceChangeLogOffDelayLogoffOrExit = "exit";
                SecondstoWaitAfterExitingCloseQuestorBeforeExitingEVE = 240;

                //
                // Value - Used in calculations
                //
                IskPerLP = 600; //used in value calculations

                //
                // Undock settings
                //
                UndockDelay = 10; //Delay when undocking - not in use
                UndockBookmarkPrefix = "Insta";

                //
                // Location of the Questor GUI on startup (default is off the screen)
                //
                WindowXPosition = 0;

                //windows position (needs to be changed, default is off screen)
                WindowYPosition = 0;

                //windows position (needs to be changed, default is off screen)
                EVEWindowXPosition = 0;
                EVEWindowYPosition = 0;
                EVEWindowXSize = 0;
                EVEWindowYSize = 0;

                //
                // Ship Names
                //
                CombatShipName = "Raven";
                SalvageShipName = "Noctis";
                TransportShipName = "Transport";
                TravelShipName = "Travel";
                MiningShipName = "Hulk";

                //
                // Usage of HomeBookmark @ dedicated salvager
                UseHomebookmark = false;
                //
                // Storage Location for Loot, Ammo, Bookmarks
                //
                HomeBookmarkName = "myHomeBookmark";
                LootHangarTabName = String.Empty;
                AmmoHangarTabName = String.Empty;
                BookmarkHangar = String.Empty;
                LootContainerName = String.Empty;

                //
                // Loot and Salvage Settings
                //
                LootEverything = true;
                UseGatesInSalvage = false;
                // if our mission does not DeSpawn (likely someone in the mission looting our stuff?) use the gates when salvaging to get to our bookmarks
                CreateSalvageBookmarks = false;
                CreateSalvageBookmarksIn = "Player"; //Player or Corp
                //other setting is "Corp"
                BookmarkPrefix = "Salvage:";
                SafeSpotBookmarkPrefix = "safespot";
                BookmarkFolder = "Salvage";
                TravelToBookmarkPrefix = "MeetHere:";
                MinimumWreckCount = 1;
                AfterMissionSalvaging = false;
                FirstSalvageBookmarksInSystem = false;
                SalvageMultipleMissionsinOnePass = false;
                UnloadLootAtStation = false;
                ReserveCargoCapacity = 100;
                MaximumWreckTargets = 0;
                WreckBlackListSmallWrecks = false;
                WreckBlackListMediumWrecks = false;
                AgeofBookmarksForSalvageBehavior = 60;
                AgeofSalvageBookmarksToExpire = 120;
                DeleteBookmarksWithNPC = false;
                LootOnlyWhatYouCanWithoutSlowingDownMissionCompletion = false;
                TractorBeamMinimumCapacitor = 0;
                SalvagerMinimumCapacitor = 0;
                DoNotDoANYSalvagingOutsideMissionActions = false;
                LootItemRequiresTarget = false;

                //
                // Enable / Disable the different types of logging that are available
                //
                SessionsLog = false;
                DroneStatsLog = false;
                WreckLootStatistics = false;
                MissionStats1Log = false;
                MissionStats2Log = false;
                MissionStats3Log = false;
                PocketStatistics = false;
                PocketStatsUseIndividualFilesPerPocket = false;
                PocketObjectStatisticsLog = false;

                //
                // Weapon and targeting Settings
                //
                WeaponGroupId = 506; //cruise
                DontShootFrigatesWithSiegeorAutoCannons = false;
                MaximumHighValueTargets = 2;
                MaximumLowValueTargets = 2;
                DoNotSwitchTargetsIfTargetHasMoreThanThisArmorDamagePercentage = 60;
                DistanceNPCFrigatesShouldBeIgnoredByPrimaryWeapons = 7000; //also requires SpeedFrigatesShouldBeIgnoredByMainWeapons
                SpeedNPCFrigatesShouldBeIgnoredByPrimaryWeapons = 300; //also requires DistanceFrigatesShouldBeIgnoredByMainWeapons


                // (IsNPCBattleship) return 4;
                // (IsNPCBattlecruiser) return 3;
                // (IsNPCCruiser) return 2;
                // (IsNPCFrigate) return 0;
                MinimumTargetValueToConsiderTargetAHighValueTarget = 2; 
                MaximumTargetValueToConsiderTargetALowValueTarget = 1;

                AddDampenersToPrimaryWeaponsPriorityTargetList = true;
                AddNeutralizersToPrimaryWeaponsPriorityTargetList = true;
                AddWarpScramblersToPrimaryWeaponsPriorityTargetList = true;
                AddWebifiersToPrimaryWeaponsPriorityTargetList = true;
                AddTargetPaintersToPrimaryWeaponsPriorityTargetList = true;
                AddECMsToPrimaryWeaponsPriorityTargetList = true;
                AddTrackingDisruptorsToPrimaryWeaponsPriorityTargetList = true;

                AddDampenersToDronePriorityTargetList = true;
                AddNeutralizersToDronePriorityTargetList = true;
                AddWarpScramblersToDronePriorityTargetList = true;
                AddWebifiersToDronePriorityTargetList = true;
                AddTargetPaintersToDronePriorityTargetList = true;
                AddECMsToDroneTargetList = true;
                AddTrackingDisruptorsToDronePriorityTargetList = true;

                InsideThisRangeIsHardToTrack = 15000;
                //
                // Script Settings - TypeIDs for the scripts you would like to use in these modules
                //
                // 29003 Focused Warp Disruption Script   // Hictor and InfiniPoint
                //
                // 29007 Tracking Speed Disruption Script // tracking disruptor
                // 29005 Optimal Range Disruption Script  // tracking disruptor
                // 29011 Scan Resolution Script           // sensor booster
                // 29009 Targeting Range Script           // sensor booster
                // 29015 Targeting Range Dampening Script // sensor dampener
                // 29013 Scan Resolution Dampening Script // sensor dampener
                // 29001 Tracking Speed Script            // tracking enhancer and tracking computer
                // 28999 Optimal Range Script             // tracking enhancer and tracking computer

                // 3554  Cap Booster 100
                // 11283 Cap Booster 150
                // 11285 Cap Booster 200
                // 263   Cap Booster 25
                // 11287 Cap Booster 400
                // 264   Cap Booster 50
                // 3552  Cap Booster 75
                // 11289 Cap Booster 800
                // 31982 Navy Cap Booster 100
                // 31990 Navy Cap Booster 150
                // 31998 Navy Cap Booster 200
                // 32006 Navy Cap Booster 400
                // 32014 Navy Cap Booster 800

                TrackingDisruptorScript = 29007;
                TrackingComputerScript = 29001;
                TrackingLinkScript = 29001;
                SensorBoosterScript = 29009;
                SensorDampenerScript = 29015;
                AncillaryShieldBoosterScript = 11289;
                CapacitorInjectorScript = 11289;
                CapBoosterToLoad = 15;

                //
                // OverLoad Settings (this WILL burn out modules, likely very quickly!
                // If you enable the overloading of a slot it is HIGHLY recommended you actually have something overloadable in that slot =/ 
                //
                OverloadWeapons = false;

                //
                // Speed and Movement Settings
                //
                AvoidBumpingThings = true;
                SpeedTank = false;
                OrbitDistance = 0;
                OrbitStructure = false;
                OptimalRange = 0;
                NosDistance = 38000;
                RemoteRepairDistance = 2000;
                MinimumPropulsionModuleDistance = 5000;
                MinimumPropulsionModuleCapacitor = 0;

                //
                // Tanking Settings
                //
                ActivateRepairModules = 65;
                DeactivateRepairModules = 95;
                InjectCapPerc = 60;

                //
                // Panic Settings
                //
                MinimumShieldPct = 50;
                MinimumArmorPct = 50;
                MinimumCapacitorPct = 50;
                SafeShieldPct = 0;
                SafeArmorPct = 0;
                SafeCapacitorPct = 0;
                UseStationRepair = true;

                //
                // Drone Settings
                //
                UseDrones = true;
                DroneTypeId = 2488;
                DroneControlRange = 25000;
                DroneMinimumShieldPct = 50;
                DroneMinimumArmorPct = 50;
                DroneMinimumCapacitorPct = 0;
                DroneRecallShieldPct = 0;
                DroneRecallArmorPct = 0;
                DroneRecallCapacitorPct = 0;
                LongRangeDroneRecallShieldPct = 0;
                LongRangeDroneRecallArmorPct = 0;
                LongRangeDroneRecallCapacitorPct = 0;
                DronesKillHighValueTargets = false;
                BelowThisHealthLevelRemoveFromDroneBay = 150;

                //
                // number of days of console logs to keep (anything older will be deleted on startup)
                //
                ConsoleLogDaysOfLogsToKeep = 14;

                MaximumHighValueTargets = 0;
                MaximumLowValueTargets = 0;

                //
                // Email Settings
                //
                EmailSupport = false;
                EmailAddress = "";
                EmailPassword = "";
                EmailSMTPServer = "";
                EmailSMTPPort = 25;
                EmailAddressToSendAlerts = "";
                EmailEnableSSL = false;

                //
                // Skill Training Settings
                //
                ThisToonShouldBeTrainingSkills = true;

                UserDefinedLavishScriptScript1 = "";
                UserDefinedLavishScriptScript1Description = "";
                UserDefinedLavishScriptScript2 = "";
                UserDefinedLavishScriptScript2Description = "";
                UserDefinedLavishScriptScript3 = "";
                UserDefinedLavishScriptScript3Description = "";
                UserDefinedLavishScriptScript4 = "";
                UserDefinedLavishScriptScript4Description = "";

                StandingsNeededToAccessLevel1Agent = -11;
                StandingsNeededToAccessLevel2Agent = 1;
                StandingsNeededToAccessLevel3Agent = 3;
                StandingsNeededToAccessLevel4Agent = 5;
                StandingsNeededToAccessLevel5Agent = 7;
                //
                // Clear various lists
                //
                Ammo.Clear();
                ItemsBlackList.Clear();
                WreckBlackList.Clear();
                ListofFactionFittings.Clear();
                ListOfAgents.Clear();
                ListOfMissionFittings.Clear();

                //
                // Clear the Blacklist
                //
                MissionBlacklist.Clear();
                MissionGreylist.Clear();
                FactionBlacklist.Clear();

                MissionName = null;
            }
            else //if the settings file exists - load the characters settings XML
            {
                Settings.Instance.CharacterXMLExists = true;
                XElement CharacterSettingsXml;
                using (XmlTextReader reader = new XmlTextReader(Settings.Instance.CharacterSettingsPath))
                {
                    reader.EntityHandling = EntityHandling.ExpandEntities;
                    CharacterSettingsXml = XDocument.Load(reader).Root;
                }

                if (CharacterSettingsXml == null)
                {
                    Logging.Log("Settings", "unable to find [" + Settings.Instance.CharacterSettingsPath + "] FATAL ERROR - use the provided settings.xml to create that file.", Logging.Red);
                }
                else
                {
                    ReadSettingsFromXML(CharacterSettingsXml);
                }
            }

            //
            // if enabled the following would keep you from looting or salvaging small wrecks
            //
            //list of small wreck
            if (WreckBlackListSmallWrecks)
            {
                WreckBlackList.Add(26557);
                WreckBlackList.Add(26561);
                WreckBlackList.Add(26564);
                WreckBlackList.Add(26567);
                WreckBlackList.Add(26570);
                WreckBlackList.Add(26573);
                WreckBlackList.Add(26576);
                WreckBlackList.Add(26579);
                WreckBlackList.Add(26582);
                WreckBlackList.Add(26585);
                WreckBlackList.Add(26588);
                WreckBlackList.Add(26591);
                WreckBlackList.Add(26594);
                WreckBlackList.Add(26935);
            }

            //
            // if enabled the following would keep you from looting or salvaging medium wrecks
            //
            //list of medium wreck
            if (WreckBlackListMediumWrecks)
            {
                WreckBlackList.Add(26558);
                WreckBlackList.Add(26562);
                WreckBlackList.Add(26568);
                WreckBlackList.Add(26574);
                WreckBlackList.Add(26580);
                WreckBlackList.Add(26586);
                WreckBlackList.Add(26592);
                WreckBlackList.Add(26934);
            }

            //"-RandomName-" + Cache.Instance.RandomNumber(1,500) + "-of-500"
            if (characterNameForLogs == "AtLoginScreenNoCharactersLoggedInYet")
            {
                characterNameForLogs = characterNameForLogs + "-randomName-" + Cache.Instance.RandomNumber(1, 500) + "-of-500";
            }

            //
            // Log location and log names defined here
            //
            Logpath = (System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log\\" + characterNameForLogs + "\\");
            CachePath = (System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache\\" + characterNameForLogs + "\\");

            //logpath_s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log\\";
            ConsoleLogPath = System.IO.Path.Combine(Logpath, "Console\\");
            ConsoleLogFile = System.IO.Path.Combine(ConsoleLogPath, string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-" + characterNameForLogs + "-" + "console" + ".log");
            ConsoleLogPathRedacted = System.IO.Path.Combine(Logpath, "Console\\");
            ConsoleLogFileRedacted = System.IO.Path.Combine(ConsoleLogPath, string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-" + "redacted" + "-" + "console" + ".log");
            SessionsLogPath = Logpath;
            SessionsLogFile = System.IO.Path.Combine(SessionsLogPath, characterNameForLogs + ".Sessions.log");
            DroneStatsLogPath = Logpath;
            DroneStatslogFile = System.IO.Path.Combine(DroneStatsLogPath, characterNameForLogs + ".DroneStats.log");
            VolleyStatsLogPath = System.IO.Path.Combine(Logpath, "VolleyStats\\");
            VolleyStatslogFile = System.IO.Path.Combine(VolleyStatsLogPath, characterNameForLogs + ".VolleyStats-DayOfYear[" + DateTime.UtcNow.DayOfYear + "].log");
            WindowStatsLogPath = System.IO.Path.Combine(Logpath, "WindowStats\\");
            WindowStatslogFile = System.IO.Path.Combine(WindowStatsLogPath, characterNameForLogs + ".WindowStats-DayOfYear[" + DateTime.UtcNow.DayOfYear + "].log");
            WreckLootStatisticsPath = Logpath;
            WreckLootStatisticsFile = System.IO.Path.Combine(WreckLootStatisticsPath, characterNameForLogs + ".WreckLootStatisticsDump.log");
            MissionStats1LogPath = System.IO.Path.Combine(Logpath, "MissionStats\\");
            MissionStats1LogFile = System.IO.Path.Combine(MissionStats1LogPath, characterNameForLogs + ".Statistics.log");
            MissionStats2LogPath = System.IO.Path.Combine(Logpath, "MissionStats\\");
            MissionStats2LogFile = System.IO.Path.Combine(MissionStats2LogPath, characterNameForLogs + ".DatedStatistics.log");
            MissionStats3LogPath = System.IO.Path.Combine(Logpath, "MissionStats\\");
            MissionStats3LogFile = System.IO.Path.Combine(MissionStats3LogPath, characterNameForLogs + ".CustomDatedStatistics.csv");
            MissionDungeonIdLogPath = System.IO.Path.Combine(Logpath, "MissionStats\\");
            MissionDungeonIdLogFile = System.IO.Path.Combine(MissionDungeonIdLogPath, characterNameForLogs + "Mission-DungeonId-list.csv");
            PocketStatisticsPath = System.IO.Path.Combine(Logpath, "PocketStats\\");
            PocketStatisticsFile = System.IO.Path.Combine(PocketStatisticsPath, characterNameForLogs + "pocketstats-combined.csv");
            PocketObjectStatisticsPath = System.IO.Path.Combine(Logpath, "PocketObjectStats\\");
            PocketObjectStatisticsFile = System.IO.Path.Combine(PocketObjectStatisticsPath, characterNameForLogs + "PocketObjectStats-combined.csv");
            MissionDetailsHtmlPath = System.IO.Path.Combine(Logpath, "MissionDetailsHTML\\");

            try
            {
                Directory.CreateDirectory(Logpath);
                Directory.CreateDirectory(CachePath);
                Directory.CreateDirectory(ConsoleLogPath);
                Directory.CreateDirectory(SessionsLogPath);
                Directory.CreateDirectory(DroneStatsLogPath);
                Directory.CreateDirectory(WreckLootStatisticsPath);
                Directory.CreateDirectory(MissionStats1LogPath);
                Directory.CreateDirectory(MissionStats2LogPath);
                Directory.CreateDirectory(MissionStats3LogPath);
                Directory.CreateDirectory(MissionDungeonIdLogPath);
                Directory.CreateDirectory(PocketStatisticsPath);
                Directory.CreateDirectory(PocketObjectStatisticsPath);
                Directory.CreateDirectory(VolleyStatsLogPath);
                Directory.CreateDirectory(WindowStatsLogPath);
            }
            catch (Exception exception)
            {
                Logging.Log("Settings", "Problem creating directories for logs [" + exception + "]", Logging.Debug);
            }
            //create all the logging directories even if they are not configured to be used - we can adjust this later if it really bugs people to have some potentially empty directories.
            
            if (!Settings.Instance.DefaultSettingsLoaded)
            {
                if (SettingsLoaded != null)
                {
                    SettingsLoadedICount++;
                    if (Settings.Instance.CommonXMLExists) Logging.Log("Settings", "[" + SettingsLoadedICount + "] Done Loading Settings from [" + Settings.Instance.CommonSettingsPath + "] and", Logging.Green);
                    Logging.Log("Settings", "[" + SettingsLoadedICount + "] Done Loading Settings from [" + Settings.Instance.CharacterSettingsPath + "]", Logging.Green);

                    SettingsLoaded(this, new EventArgs());
                }
            }
        }

        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}
