//
// The MIT License (MIT)
//
// Copyright (C) 2012 Gary McNickle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace EVE.Net
{
   public static class EVEConstants
   {
      public static readonly string dateStringFormat = "yyyy-MM-dd HH:mm:ss";
      public static readonly string tranquilityAPIUri = @"https://api.eveonline.com";
      public static readonly string singularityAPIUri = @"https://apitest.eveonline.com";

       /// <summary>
       /// Contains the typeID's returned by some API calls to indicate whether a third party is a character, corporation or alliance.
       /// </summary>
      public static readonly int[] characterTypes = { 1373, 1374, 1375, 1376, 1377, 1378, 1379, 1380, 1381, 1382, 1383, 1384, 1385, 1386 };
       /// <summary>
       /// The typeID returned by some API calls to indicate whether the ID is a character, corporation or alliance.
       /// </summary>
      public static readonly int corporationType = 2;
      /// <summary>
      /// The typeID returned by some API calls to indicate whether the ID is a character, corporation or alliance.
      /// </summary>
      public static readonly int allianceType = 16159;
   }

   public static class BaseUrls
   {
      public static readonly string eveSkillTreeUri = @"/eve/SkillTree.xml.aspx";
      public static readonly string eveCertificateTreeUri = @"/eve/CertificateTree.xml.aspx";
      public static readonly string eveCharInfoUri = @"/eve/CharacterInfo.xml.aspx";
      public static readonly string eveServerStatusUri = @"/server/ServerStatus.xml.aspx";
      public static readonly string eveAllianceListUri = @"/eve/AllianceList.xml.aspx";
      public static readonly string eveCharacterIDUri = @"/eve/CharacterID.xml.aspx";
      public static readonly string eveCharacterNameUri = @"/eve/CharacterName.xml.aspx";
      public static readonly string eveConquerableStationListUri = @"/eve/ConquerableStationList.xml.aspx";
      public static readonly string eveErrorListUri = @"/eve/ErrorList.xml.aspx";
      public static readonly string eveFactionalWarfareStatsUri = @"/eve/FacWarStats.xml.aspx";
      public static readonly string eveFactionalWarfareTopStatsUri = @"/eve/FacWarTopStats.xml.aspx";
      public static readonly string eveRefTypesUri = @"/eve/RefTypes.xml.aspx";
      public static readonly string eveTypeNameUri = @"/eve/TypeName.xml.aspx";
      public static readonly string eveCallListUri = @"/api/calllist.xml.aspx";

      public static readonly string mapKillsUri = @"/map/Kills.xml.aspx";
      public static readonly string mapJumpsUri = @"/map/Jumps.xml.aspx";
      public static readonly string mapSovereigntyUri = @"/map/Sovereignty.xml.aspx";
      public static readonly string mapFacWarSystemsUri = @"/map/FacWarSystems.xml.aspx";
      public static readonly string mapSovereigntyStatusUri = @"/map/SovereigntyStatus.xml.aspx";

      public static readonly string accountStatusUri = @"/account/AccountStatus.xml.aspx";
      public static readonly string accountCharacterListUri = @"/account/Characters.xml.aspx";
      public static readonly string accountApiKeyInfoUri = @"/account/APIKeyInfo.xml.aspx";

      public static readonly string characterSheetUri = @"/char/CharacterSheet.xml.aspx";
      public static readonly string charKillLogUri = @"/char/KillLog.xml.aspx";
      public static readonly string charKillMailsUri = @"/char/KillMails.xml.aspx";
      public static readonly string charAssetListUri = @"/char/AssetList.xml.aspx";
      public static readonly string charWalletJournalUri = @"/char/WalletJournal.xml.aspx";
      public static readonly string charSkillQueueUri = @"/char/SkillQueue.xml.aspx";
      public static readonly string charSkillInTrainingUri = @"/char/SkillInTraining.xml.aspx";
      public static readonly string charAccountBalanceUri = @"/char/AccountBalance.xml.aspx";
      public static readonly string charContactListUri = @"/char/ContactList.xml.aspx";
      public static readonly string charMarketOrdersUri = @"/char/MarketOrders.xml.aspx";
      public static readonly string charContactNotificationsUri = @"/char/ContactNotifications.xml.aspx";
      public static readonly string charUpcomingCalendarEventsUri = @"/char/UpcomingCalendarEvents.xml.aspx";
      public static readonly string charEventAttendeesUri = @"/char/CalendarEventAttendees.xml.aspx";
      public static readonly string charContractsUri = @"/char/Contracts.xml.aspx";
      public static readonly string charNPCStandingsUri = @"/char/Standings.xml.aspx";
      public static readonly string charContractBidsUri = @"/char/ContractBids.xml.aspx";
      public static readonly string charContractItemsUri = @"/char/ContractItems.xml.aspx";
      public static readonly string charFactionalWarfareStatsUri = @"/char/FacWarStats.xml.aspx";
      public static readonly string charIndustryJobsUri = @"/char/IndustryJobs.xml.aspx";
      public static readonly string charLocationsUri = @"/char/Locations.xml.aspx";
      public static readonly string charMailingListsUri = @"/char/mailinglists.xml.aspx";
      public static readonly string charMailMessagesUri = @"/char/MailMessages.xml.aspx";
      public static readonly string charMedalsUri = @"/char/Medals.xml.aspx";
      public static readonly string charResearchUri = @"/char/Research.xml.aspx";
      public static readonly string charNotificationsUri = @"/char/Notifications.xml.aspx";
      public static readonly string charWalletTransactionsUri = @"/char/WalletTransactions.xml.aspx";
      public static readonly string charMailBodiesUri = @"/char/MailBodies.xml.aspx";
      public static readonly string charNotificationTextsUri = @"/char/NotificationTexts.xml.aspx";

      public static readonly string corpSheetUri = @"/corp/CorporationSheet.xml.aspx";
      public static readonly string corpKillLogUri = @"/corp/KillLog.xml.aspx";
      public static readonly string corpKillMailsUri = @"/corp/KillMails.xml.aspx";
      public static readonly string corpContactListUri = @"/corp/ContactList.xml.aspx";
      public static readonly string corpWalletJournalUri = @"/corp/WalletJournal.xml.aspx";
      public static readonly string corpAccountBalanceUri = @"/corp/AccountBalance.xml.aspx";
      public static readonly string containerLogUri = @"/corp/ContainerLog.xml.aspx";
      public static readonly string corpMarketOrdersUri = @"/corp/MarketOrders.xml.aspx";
      public static readonly string corpTitlesUri = @"/corp/Titles.xml.aspx";
      public static readonly string corpAssetListUri = @"/corp/AssetList.xml.aspx";
      public static readonly string corpNPCStandingsUri = @"/corp/Standings.xml.aspx";
      public static readonly string corpContractBidsUri = @"/corp/ContractBids.xml.aspx";
      public static readonly string corpContractItemsUri = @"/corp/ContractItems.xml.aspx";
      public static readonly string corpFactionalWarfareStatsUri = @"/corp/FacWarStats.xml.aspx";
      public static readonly string corpIndustryJobsUri = @"/corp/IndustryJobs.xml.aspx";
      public static readonly string corpLocationsUri = @"/corp/Locations.xml.aspx";
      public static readonly string corpMedalsUri = @"/corp/Medals.xml.aspx";
      public static readonly string corpMemberMedalsUri = @"/corp/MemberMedals.xml.aspx";
      public static readonly string corpWalletTransactionsUri = @"/corp/WalletTransactions.xml.aspx";
      public static readonly string corpMemberSecurityUri = @"/corp/MemberSecurity.xml.aspx";
      public static readonly string corpMemberSecurityLogUri = @"/corp/MemberSecurityLog.xml.aspx";
      public static readonly string corpMemberTrackingUri = @"/corp/MemberTracking.xml.aspx";
      public static readonly string corpOutpostsListUri = @"/corp/OutpostList.xml.aspx";
      public static readonly string corpOutpostServiceDetailUri = @"/corp/OutpostServiceDetail.xml.aspx";
      public static readonly string corpShareholdersUri = @"/corp/Shareholders.xml.aspx";
      public static readonly string corpStarbaseListUri = @"/corp/StarbaseList.xml.aspx";
      public static readonly string corpStarbaseDetailUri = @"/corp/StarbaseDetail.xml.aspx";
   }
}
