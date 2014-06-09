// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Lookup
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using DirectEve;
	using global::Questor.Modules.Caching;
	using global::Questor.Modules.Logging;
	//using global::Questor.Modules.Lookup;

	public static class SkillPlan
	{
		/// <summary>
		/// Singleton implementation
		/// </summary>

		private static int iCount = 1;
		public static int injectSkillBookAttempts = 0;
		private static DateTime _nextSkillTrainingAction = DateTime.MinValue;
		private static DateTime _nextRetrieveSkillQueueInfoAction = DateTime.MinValue;
		private static DateTime _nextRetrieveCharactersheetInfoAction = DateTime.MinValue;
		private static List<DirectSkill> MyCharacterSheetSkills { get; set; }
		private static List<DirectSkill> MySkillQueue { get; set; }

		private static readonly List<string> myRawSkillPlan = new List<string>();
		private static readonly Dictionary<string, int> mySkillPlan = new Dictionary<string, int>();
		public static bool buyingSkill = false;
		public static int buyingIterator = 0;
		public static int buyingSkillTypeID = 0;
		public static bool doneWithAllPlannedSKills = false;
		public static int attemptsToDoSomethingWithNonInjectedSkills = 0;
		private static XDocument skillPreReqs;
		public static bool skillWasInjected = false;
		private static readonly Dictionary<char, int> RomanDictionary = new Dictionary<char, int>
		
		{
			{'I', 1},
			{'V', 5},
		};
		
		public static bool RetrieveSkillQueueInfo()
		{
			if (DateTime.UtcNow > _nextRetrieveSkillQueueInfoAction)
			{
				MySkillQueue = Cache.Instance.DirectEve.Skills.MySkillQueue;
				_nextRetrieveSkillQueueInfoAction = DateTime.UtcNow.AddSeconds(10);
				if (MySkillQueue != null)
				{
					if (Settings.Instance.DebugSkillTraining) Logging.Log("RetrieveSkillQueueInfo", "MySkillQueue is not null, continue", Logging.Debug);
					return true;
				}

				if (Settings.Instance.DebugSkillTraining) Logging.Log("RetrieveSkillQueueInfo", "MySkillQueue is null, how? retry in 10 sec", Logging.Debug);
				return true;
			}

			if (Settings.Instance.DebugSkillTraining) Logging.Log("RetrieveSkillQueueInfo", "Waiting...", Logging.Debug);
			return false;
		}

		public static bool InjectSkillBook(int skillID)
		{
			IEnumerable<DirectItem> items = Cache.Instance.ItemHangar.Items.Where(k => k.TypeId == skillID).ToList();
			if (DoWeHaveThisSkillAlreadyInOurItemHangar(skillID))
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skillID.ToString() + "] found in ItemHangar", Logging.Debug);
				DirectItem SkillBookToInject = items.FirstOrDefault(s => s.TypeId == skillID);
				if (SkillBookToInject != null)
				{
					if (MyCharacterSheetSkills != null && !MyCharacterSheetSkills.Any(i => i.TypeName == SkillBookToInject.TypeName || i.GivenName == SkillBookToInject.TypeName))
					{
						if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  GivenName [" + SkillBookToInject.GivenName + "] TypeName [" + SkillBookToInject.TypeName + "] is being injected", Logging.Debug);
						if (DoWeHaveTheRightPrerequisites(SkillBookToInject.TypeId)) {
							SkillBookToInject.InjectSkill();
							skillWasInjected = true;
							
							return true;
						} else {
							if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "Skillbook: We don't have the right Prerequisites for " + SkillBookToInject.GivenName, Logging.Debug);
						}
						
					}
					
					if (MyCharacterSheetSkills != null && MyCharacterSheetSkills.Any(i => i.TypeName == SkillBookToInject.TypeName))
					{
						if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  TypeName [" + SkillBookToInject.TypeName + "] is already injected, why are we trying to do so again? aborting injection attempt ", Logging.Debug);
						return true;
					}
				}
				
				return false;
			}
			Logging.Log("InjectSkillBook", "We don't have this skill in our hangar", Logging.Debug);
			return false;
		}
		public static bool DoWeHaveTheRightPrerequisites(int skillID){
			try {
				if (skillPreReqs == null) {
					skillPreReqs = XDocument.Load(Settings.Instance.Path + "\\Skill_Prerequisites.xml");
					if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites","Skill_Prerequisites.xml Loaded.", Logging.Debug);
				}
			} catch (Exception) {
				
				if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites","Skill_Prerequisites.xml exception -- does the file exist?", Logging.Debug);
				return false;
			}
			
			foreach (var skills in skillPreReqs.Descendants("skill")){
				
				if(skillID.ToString().Equals(skills.Attribute("id").Value)){
					
					if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites", "skillID.ToString().Equals(skills.Attribute(\"id\").Value == TRUE", Logging.Debug);
					foreach(var preRegs in skills.Descendants("preqskill")){
						
						if (MyCharacterSheetSkills.Any(i => i.TypeId.ToString().Equals(preRegs.Attribute("id").Value))){
							
							if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites", "We have this Prerequisite: " + preRegs.Attribute("id").Value, Logging.Debug);
							if(MyCharacterSheetSkills.Any(i => i.TypeId.ToString().Equals(preRegs.Attribute("id").Value) && i.Level < Convert.ToInt32(preRegs.Value))){
								
								if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites", "We don't meet the required level on this skill: " + preRegs.Attribute("id").Value, Logging.Debug);
								
								return false;
							} else {
								
								if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites", "We meet the required skill level on this skill: " + preRegs.Attribute("id").Value, Logging.Debug);
							}
						} else {
							
							if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites", "We don't have this prerequisite: " + preRegs.Attribute("id").Value, Logging.Debug);
						}
					}
					// this is also good for skills with no pre requirements
					return true;
				}
			}
			return false;
			// not in list which is unlikely
		}
		public static bool DoWeHaveThisSkillAlreadyInOurItemHangar(int skillID){
			if (!Cache.Instance.InStation) return false;
			IEnumerable<DirectItem> items = Cache.Instance.ItemHangar.Items.Where(k => k.TypeId == skillID).ToList();
			if (items.Any())
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.DoWeHaveThisSkillAlreadyInOurItemHangar:", "We already have this skill in our hangar " + skillID.ToString(), Logging.White);
				return true;
			} else {
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.DoWeHaveThisSkillAlreadyInOurItemHangar:", "We don't have this skill in our hangar " + skillID.ToString(), Logging.White);
				return false;
			}
		}
		public static bool BuySkill(int skillID){
			
			if (!Cache.Instance.InStation) return false;
			if (DateTime.UtcNow < _nextSkillTrainingAction)
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.buySkill:", "Next Skill Training Action is set to continue in [" + Math.Round(_nextSkillTrainingAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] seconds", Logging.White);
				return false;
			}
			
			if (buyingSkill == true && buyingSkillTypeID != 0){
				buyingIterator++;
				
				if(buyingIterator>20){
					Logging.Log("buySkill", "buying iterator < 20 with SkillID" + skillID, Logging.White);
					buyingSkill = false;
					buyingSkillTypeID = 0;
					buyingIterator = 0;
					return true;
				}
				// only buy if we do not have it already in our itemhangar
				if (DoWeHaveThisSkillAlreadyInOurItemHangar(skillID))
				{
					Logging.Log("buySkill", "We already purchased this skill" + skillID, Logging.White);
					buyingSkill = false;
					buyingSkillTypeID = 0;
					buyingIterator = 0;
					return true;
				}
				
				DirectMarketWindow marketWindow = Cache.Instance.DirectEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
				if (Cache.Instance.DirectEve.HasSupportInstances())
				{
					if (marketWindow == null)
					{
						_nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(10);

						Logging.Log("buySkill", "Opening market window", Logging.White);

						Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Statistics.LogWindowActionToWindowLog("MarketWindow", "MarketWindow Opened");
						return false;
						
					}
					if (!marketWindow.IsReady)
					{
						_nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(3);
						return false;
					}

					if (marketWindow.DetailTypeId != skillID)
					{
						// No, load the right order
						marketWindow.LoadTypeId(skillID);
						Logging.Log("buySkill", "Loading market with right typeid ", Logging.White);
						_nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(6);
						return false;
					}

					// Get the median sell price
				    DirectInvType type;
				    Cache.Instance.DirectEve.InvTypes.TryGetValue(skillID, out type);
					double? maxPrice = type.AveragePrice * 10;
					Logging.Log("buySkill", "maxPrice "  + maxPrice.ToString(), Logging.White);

					// Do we have orders?
					IEnumerable<DirectOrder> orders = marketWindow.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId && o.Price < maxPrice).ToList();
					if (orders.Any())
					{
						DirectOrder order = orders.OrderBy(o => o.Price).FirstOrDefault();
						if (order != null)
							order.Buy(1, DirectOrderRange.Station);
						Logging.Log("buySkill", "Buying skill with typeid & waiting 20 seconds ( to ensure we don't buy the skills twice ) " + skillID, Logging.White);
						buyingSkill = false;
						buyingSkillTypeID = 0;
						buyingIterator = 0;
						// Wait for the order to go through
						_nextRetrieveCharactersheetInfoAction = DateTime.MinValue; // ensure we get the character sheet update
						_nextSkillTrainingAction  = DateTime.UtcNow.AddSeconds(20);
						return true;
					} else {
						Logging.Log("buySkill", "No skill could be found with median price ", Logging.White);
						buyingSkill = false;
						buyingSkillTypeID = 0;
						buyingIterator = 0;
						return false;
						
					}
				}
			}
			return false;
		}

		public static bool ImportSkillPlan()
		{
			iCount = 1;
			myRawSkillPlan.Clear();
			try
			{
				string SkillPlanFile = Settings.Instance.Path + "\\skillPlan-" + Cache.Instance.DirectEve.Me.Name + ".txt".ToLower() ;

				if (!File.Exists(SkillPlanFile))
				{
					string GenericSkillPlanFile = Settings.Instance.Path + "\\" + "skillPlan.txt".ToLower();
					Logging.Log("importSkillPlan", "Missing Character Specific skill plan file [" + SkillPlanFile + "], trying generic file ["  + GenericSkillPlanFile +  "]", Logging.Teal);
					SkillPlanFile = GenericSkillPlanFile;
				}

				if (!File.Exists(SkillPlanFile))
				{
					Logging.Log("importSkillPlan", "Missing Generic skill plan file [" + SkillPlanFile + "]", Logging.Teal);
					return false;
				}

				Logging.Log("importSkillPlan", "Loading SkillPlan from [" + SkillPlanFile + "]", Logging.Teal);

				// Use using StreamReader for disposing.
				using (StreamReader readTextFile = new StreamReader(SkillPlanFile))
				{
					string line;
					while ((line = readTextFile.ReadLine()) != null)
					{
						myRawSkillPlan.Add(line);
					}
				}
				
			}
			catch (Exception exception)
			{
				Logging.Log("importSkillPlan", "Exception was: [" + exception +"]", Logging.Teal);
				return false;
			}
			return true;
		}

		public static void ReadySkillPlan()
		{
			mySkillPlan.Clear();
			//int i = 1;
			foreach (string imported_skill in myRawSkillPlan)
			{
				string RomanNumeral = ParseRomanNumeral(imported_skill);
				string SkillName = imported_skill.Substring(0,imported_skill.Length - CountNonSpaceChars(RomanNumeral));
				SkillName = SkillName.Trim();
				int LevelPlanned = Decode(RomanNumeral);
				if (mySkillPlan.ContainsKey(SkillName))
				{
					if (mySkillPlan.FirstOrDefault(x => x.Key == SkillName).Value < LevelPlanned)
					{
						mySkillPlan.Remove(SkillName);
						mySkillPlan.Add(SkillName, LevelPlanned);
					}
					continue;
				}
				

				mySkillPlan.Add(SkillName, LevelPlanned);
				//if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.readySkillPlan", "[" + i + "]" + imported_skill + "] LevelPlanned[" + LevelPlanned + "][" + RomanNumeral + "]", Logging.Teal);
				continue;
			}
		}

		static int CountNonSpaceChars(string value)
		{
			return value.Count(c => !char.IsWhiteSpace(c));
		}

		public static bool CheckTrainingQueue(string module)
		{
			if (DateTime.UtcNow < _nextSkillTrainingAction)
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", "Next Skill Training Action is set to continue in [" + Math.Round(_nextSkillTrainingAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] seconds", Logging.White);
				return false;
			}
			if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:","Current iCount is: " + iCount.ToString(), Logging.White);
			iCount++;

			if (Cache.Instance.DirectEve.Skills.AreMySkillsReady)
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", "if (Cache.Instance.DirectEve.Skills.AreMySkillsReady)", Logging.White);
				_nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(3);

				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", "Current Training Queue Length is [" + Cache.Instance.DirectEve.Skills.SkillQueueLength.ToString() + "]", Logging.White);
				if (Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalMinutes < 1337 && Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalMinutes >= 0) // 1440 = 60*24
				{
					Logging.Log("SkillPlan.CheckTrainingQueue:", "Training Queue currently has room. [" + Math.Round(24 - Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalHours, 2) + " hours free]", Logging.White);
					if(doneWithAllPlannedSKills) return true;
					if (!AddPlannedSkillToQueue("SkillPlan")) return false;
					if (iCount > 30) return true; //this should only happen if the actual adding of items to the skill queue fails or if we can't add enough skills to the queue <24h
				} else {
					Logging.Log("SkillPlan.CheckTrainingQueue:", "Training Queue is full. [" + Math.Abs(Math.Round(Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalHours, 2)) + " is more than 24 hours]", Logging.White);
					return true;
				}
			} else {
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", " false: if (Cache.Instance.DirectEve.Skills.AreMySkillsReady)", Logging.White);
			}
			return false;
		}
		
		public static bool SkillAlreadyQueued(KeyValuePair<string, int> skill)
        {
		    foreach (DirectSkill queuedskill in MySkillQueue)
			{
				if (queuedskill.TypeName == skill.Key && queuedskill.Level >= skill.Value)
				{
					if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillAlreadyQueued", "Skill already in queue [" + skill.Key + "]", Logging.White);
					return true;
				}
			}

			if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillAlreadyQueued", "Skill not in queue  [" + skill.Key + "]", Logging.White);
			return false;
		}
		
		public static bool SkillAlreadyInCharacterSheet(KeyValuePair<string, int> skill)
        {
			foreach (DirectSkill knownskill in MyCharacterSheetSkills)
			{
				if (knownskill.TypeName == skill.Key)
				{
					if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillAlreadyInCharacterSheet", "We already have this skill injected:  [" + skill.Key + "]", Logging.White);
					return true;
				}
			}

			if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillAlreadyInCharacterSheet", "We DON'T have this skill already injected:  [" + skill.Key + "]", Logging.White);
			return false;
		}
		
		public static bool SkillIsBelowPlannedLevel(KeyValuePair<string, int> skill)
        {
		    foreach (DirectSkill knownskill in MyCharacterSheetSkills)
			{
				if (knownskill.TypeName == skill.Key && knownskill.Level < skill.Value)
				{
					if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillIsBelowPlannedLevel", "Skill is below planned level:  [" + skill.Key + "]", Logging.White);
					return true;
				}
			}
			
            if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillIsBelowPlannedLevel", "Skill is not below planned level:  [" + skill.Key + "]", Logging.White);
			return false;
		}
		
		public static bool TrainSkillNow(KeyValuePair<string, int> skill)
        {
			
			foreach (DirectSkill knownskill in MyCharacterSheetSkills)
			{
				if (knownskill.TypeName == skill.Key && knownskill.Level < skill.Value)
				{
					if (Settings.Instance.DebugSkillTraining) Logging.Log("TrainSkillNow", "Training Skill now:  [" + skill.Key + "]", Logging.White);
					knownskill.AddToEndOfQueue();
					return true;
				}
			}

			if (Settings.Instance.DebugSkillTraining) Logging.Log("TrainSkillNow", "This skill couldn't be trained:  [" + skill.Key + "]", Logging.White);
			return false;
		}
		
		public static int getInvTypeID(string moduleName)
        {
			try 
            {
				
				if (skillPreReqs == null) 
                {
					skillPreReqs = XDocument.Load(Settings.Instance.Path + "\\Skill_Prerequisites.xml");
					if (Settings.Instance.DebugSkillTraining) Logging.Log("DoWeHaveTheRightPrerequisites","Skill_Prerequisites.xml Loaded.", Logging.Debug);
				}

				return Convert.ToInt32(skillPreReqs.Element("document").Elements("skill").Where(i=> i.Attribute("name").Value.ToLower() == moduleName.ToLower()).Select(e=> e.Attribute("id").Value).FirstOrDefault().ToString());
				
			} 
            catch (Exception e) 
            {
				
				if (Settings.Instance.DebugSkillTraining) Logging.Log("getInvTypeID", "Exception:  [" + e.Message + "]", Logging.White);
				return 0;
			}
		}
		

		public static bool AddPlannedSkillToQueue(string module)
		{
			foreach (KeyValuePair<string, int> skill in mySkillPlan)
			{
				//if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "Currently working with [" + skill.Key + "] level [" + skill.Value + "]", Logging.White);
				if (!SkillAlreadyQueued(skill) && SkillIsBelowPlannedLevel(skill) && SkillAlreadyInCharacterSheet(skill)) // not queued && below planned level && in our character sheet
				{
					TrainSkillNow(skill);
					_nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(2, 5));
					return false;
				}
				
				if(!SkillAlreadyInCharacterSheet(skill) && attemptsToDoSomethingWithNonInjectedSkills <= 15)
                {	
                    // skill not already in our character sheet
					attemptsToDoSomethingWithNonInjectedSkills++;
					Logging.Log("AddPlannedSkillToQueue","Skill [" + skill.Key  + "] will be bought(if in station & price < average sell * 10) and/or injected if in itemhangar",Logging.Red);
					buyingSkillTypeID = getInvTypeID(skill.Key);
					if (buyingSkillTypeID != 0)
                    {
						if (DoWeHaveThisSkillAlreadyInOurItemHangar(buyingSkillTypeID))
                        {
							InjectSkillBook(buyingSkillTypeID);
							return false;
						} 
                        
						if (Cache.Instance.InStation) 
                        {
							buyingSkill = true;
						}

						return false;
					}
					return false;
				}
			}
			if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "Done with all planned skills", Logging.White);
			doneWithAllPlannedSKills = true;
			return true;
		}

		private static string ParseRomanNumeral(string importedSkill)
		{
			string subString = importedSkill.Substring(importedSkill.Length - 3);
			
			try
			{
				bool startsWithWhiteSpace = char.IsWhiteSpace(subString, 0); // 0 = first character
				if (startsWithWhiteSpace || char.IsLower(subString, 0))
				{
					subString = importedSkill.Substring(importedSkill.Length - 2);
					startsWithWhiteSpace = char.IsWhiteSpace(subString, 0); // 0 = first character
					if (startsWithWhiteSpace)
					{
						subString = importedSkill.Substring(importedSkill.Length - 1);
						startsWithWhiteSpace = char.IsWhiteSpace(subString, 0); // 0 = first character
						if (startsWithWhiteSpace)
						{
							return subString;
						}
						return subString;
					}
					return subString;
				}
				return subString;
			}
			catch (Exception exception)
			{
				Logging.Log("ParseRomanNumeral", "Exception was [" + exception + "]", Logging.Debug);
			}
			return subString;
		}

		private static int Decode(string roman)
		{

			roman = roman.ToUpper();
			int total = 0, minus = 0;

			for (int icount2 = 0; icount2 < roman.Length; icount2++) // Iterate through characters.
			{
				int thisNumeral = RomanDictionary[roman[icount2]] - minus;

				if (icount2 >= roman.Length - 1 ||
				    thisNumeral + minus >= RomanDictionary[roman[icount2 + 1]])
				{
					total += thisNumeral;
					minus = 0;
				}
				else
				{
					minus = thisNumeral;
				}
			}
			return total;
		}

		public static bool ReadMyCharacterSheetSkills()
		{
			if (DateTime.UtcNow < _nextSkillTrainingAction)
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.ReadMyCharacterSheetSkills:", "Next Skill Training Action is set to continue in [" + Math.Round(_nextSkillTrainingAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] seconds", Logging.White);
				return false;
			}
			if (MyCharacterSheetSkills == null || !MyCharacterSheetSkills.Any())
			{
				if (Settings.Instance.DebugSkillTraining) Logging.Log("readMyCharacterSheetSkills", "if (!MyCharacterSheetSkills.Any())", Logging.Teal);
				
				MyCharacterSheetSkills = Cache.Instance.DirectEve.Skills.MySkills;
				return false;
			}

			int iCount = 1;

			
			if (!Cache.Instance.DirectEve.Skills.AreMySkillsReady)
			{
				if(Settings.Instance.DebugSkillTraining) Logging.Log("readMyCharacterSheetSkills", "if (!Cache.Instance.DirectEve.Skills.AreMySkillsReady)", Logging.Teal);
				return false;
			}
			
			if (DateTime.UtcNow > _nextRetrieveCharactersheetInfoAction) {
				if(Settings.Instance.DebugSkillTraining) Logging.Log("readMyCharacterSheetSkills", "Updating Character sheet again", Logging.Teal);
				MyCharacterSheetSkills = Cache.Instance.DirectEve.Skills.MySkills;
				_nextRetrieveCharactersheetInfoAction.AddSeconds(13);
				return true;
			}

			foreach (DirectSkill trainedskill in MyCharacterSheetSkills)
			{
				iCount++;
				//if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.MyCharacterSheetSkills", "[" + iCount + "] SkillName [" + trainedskill.TypeName + "] lvl [" + trainedskill.Level + "] SkillPoints [" + trainedskill.SkillPoints + "] inTraining [" + trainedskill.InTraining + "]", Logging.Teal);
			}
			return true;
		}
	}
}