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
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    //using global::Questor.Modules.Lookup;

    public static class SkillPlan
    {
        /// <summary>
        /// Singleton implementation
        /// </summary>
        //private static Skills Instance = new Skills();

        private static int iCount = 1;
        public static int injectSkillBookAttempts; // = 0;
        private static DateTime _nextSkillTrainingAction = DateTime.MinValue;
        private static DateTime _nextRetrieveSkillQueueInfoAction = DateTime.MinValue;
        private static List<DirectSkill> MyCharacterSheetSkills { get; set; }
        private static List<DirectSkill> MySkillQueue { get; set; }

        private static readonly List<string> myRawSkillPlan = new List<string>();
        private static readonly Dictionary<string, int> mySkillPlan = new Dictionary<string, int>();

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

        public static bool InjectSkillBook(string skillNameToFind)
        {
            //"To learn that skill requires having already learned the following skills"
            //00:38:14 [Questor] --------------------------------------------------
            //00:38:14 [Questor] Debug_Window.Name: [modal]
            //00:38:14 [Questor] Debug_Window.Caption: []
            //00:38:14 [Questor] Debug_Window.Type: [form.MessageBox]
            //00:38:14 [Questor] Debug_Window.IsModal: [True]
            //00:38:14 [Questor] Debug_Window.IsDialog: [True]
            //00:38:14 [Questor] Debug_Window.Id: []
            //00:38:14 [Questor] Debug_Window.IsKillable: [True]
            //00:38:14 [Questor] Debug_Window.Html: [To learn that skill requires having already learned the following skills: Marketing : Level 2.]

            // List its items
            IEnumerable<DirectItem> items = Cache.Instance.ItemHangar.Items.Where(k => k.CategoryId == (int)CategoryID.Skill).ToList();
            if (items.Any())
            {
                //SkillBookToInject.
                if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skillNameToFind + "] found in ItemHangar", Logging.Debug);
                DirectItem SkillBookToInject = items.FirstOrDefault(s => s.TypeName == skillNameToFind);
                if (SkillBookToInject != null)
                {
                    if (MyCharacterSheetSkills != null && !MyCharacterSheetSkills.Any(i => i.TypeName == SkillBookToInject.TypeName || i.GivenName == SkillBookToInject.TypeName))
                    {
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  GivenName [" + SkillBookToInject.GivenName + "] TypeName [" + SkillBookToInject.TypeName + "] is being injected", Logging.Debug);
                        SkillBookToInject.InjectSkill();
                        return true;    
                    }
                    
                    if (MyCharacterSheetSkills != null && MyCharacterSheetSkills.Any(i => i.TypeName == SkillBookToInject.TypeName))
                    {
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  TypeName [" + SkillBookToInject.TypeName + "] is already injected, why are we trying to do so again? aborting injection attempt ", Logging.Debug);
                        return true;
                    }
                    
                    if (MyCharacterSheetSkills != null && MyCharacterSheetSkills.Any(i => i.GivenName == SkillBookToInject.TypeName))
                    {
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  GivenName [" + SkillBookToInject.GivenName + "] is already injected, why are we trying to do so again? aborting injection attempt ", Logging.Debug);
                        return true;
                    }

                    if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  GivenName [" + SkillBookToInject.GivenName + "] could not be verified as NOT yet in your head... continue", Logging.Debug);
                    return true;
                }
                
                return false;
            }

            if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skillNameToFind + "] not found in ItemHangar, trying AmmoHangar", Logging.Debug);
            items = Cache.Instance.AmmoHangar.Items.Where(k => k.CategoryId == (int)CategoryID.Skill).ToList();
            if (items.Any())
            {
                //SkillBookToInject.
                DirectItem SkillBookToInject = items.FirstOrDefault(s => s.TypeName == skillNameToFind);
                if (SkillBookToInject != null)
                {
                    if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook:  GivenName [" + SkillBookToInject.GivenName + "] TypeName [" + SkillBookToInject.TypeName + "] is being injected", Logging.Debug);
                    SkillBookToInject.InjectSkill();
                    return true;
                }

                return false;
            }

            if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skillNameToFind + "] not found in AmmoHangar, not found in any hangar", Logging.Debug);
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
                    //
                    // can we assume that the skill plan file being read is is from EveMon (thus valid)?
                    // can we verify that the file we are reading is in fact a skill plan!?! 
                    // err maybe we need to verify that each line contains a skill? 
                    //
                    string line;
                    while ((line = readTextFile.ReadLine()) != null)
                    {
                        // Insert logic here.
                        // ...
                        // "line" is a line in the file. Add it to our List.
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

            iCount++;

            if (Cache.Instance.DirectEve.Skills.AreMySkillsReady)
            {
                if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", "if (Cache.Instance.DirectEve.Skills.AreMySkillsReady)", Logging.White);  
                _nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(3);

                if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", "Current Training Queue Length is [" + Cache.Instance.DirectEve.Skills.SkillQueueLength.ToString() + "]", Logging.White);  
                if (Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalDays < 1)
                {
                    Logging.Log("SkillPlan.CheckTrainingQueue:", "Training Queue currently has room. [" + Math.Round(24 - Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalHours, 2) + " hours free]", Logging.White);
                    if (!InjectSkillsThatAreReadyToInject()) return false;
                    if (!AddPlannedSkillToQueue("SkillPlan")) return false;
                    if (iCount > 30) return true; //this should only happen if the actual adding of items to the skill queue fails.
                    return true;
                }

                Logging.Log("SkillPlan.CheckTrainingQueue:", "Training Queue is full. [" + Math.Round(Cache.Instance.DirectEve.Skills.SkillQueueLength.TotalHours, 2) + " is more than 24 hours]", Logging.White);  
                return true;
            }

            if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan.CheckTrainingQueue:", " false: if (Cache.Instance.DirectEve.Skills.AreMySkillsReady)", Logging.White);  
            return false;
        }

        public static bool AddPlannedSkillToQueue(string module)
        {
            //if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillTrainer", "before: if (!Cache.Instance.ReadyItemsHangar(module)) return false;", Logging.White);
            //if (!Cache.Instance.ReadyItemsHangar(module)) return false;
            //if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillTrainer", "after: if (!Cache.Instance.ReadyItemsHangar(module)) return false;", Logging.White);
            //int i = 1;
            foreach (KeyValuePair<string, int> skill in mySkillPlan)
            {
                //bool PlannedSkillInjected = false;
                if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "SkillPlan: skill [" + skill.Key + "] level [" + skill.Value + "]", Logging.White);

                //MyCharacterSheetSkills.Where(v => v.TypeName == skill.Key && v.Level < skill.Value && !v.InTraining);

                bool SkillAlreadyQueued = false;
                foreach (DirectSkill knownskill in MyCharacterSheetSkills)
                {
                    if (knownskill.TypeName == skill.Key)
                    {
                        //PlannedSkillInjected = true;
                        if (knownskill.Level < skill.Value)
                        {
                            if (!knownskill.InTraining)
                            {
                                foreach (DirectSkill queuedskill in MySkillQueue)
                                {
                                    if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "Skill in the queue [" + queuedskill.TypeName + "] InTraining [" + queuedskill.InTraining + "] Level [" + queuedskill.Level + "] SkillPoints[" + queuedskill.SkillPoints + "] SkillTimeConstant[" + queuedskill.SkillTimeConstant + "] Planned Skill[" + skill.Key + "] Planned Level[" + skill.Value + "]", Logging.Teal);
                                        
                                    if (queuedskill.TypeName == skill.Key && queuedskill.Level >= skill.Value)
                                    {
                                        if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "Planned skill [" + skill.Key + "] to level [" + skill.Value + "] matches skill in the queue [" + queuedskill.TypeName + "][" + queuedskill.Level + "]", Logging.Teal);
                                        SkillAlreadyQueued = true;
                                        break;
                                    }
                                }

                                if (SkillAlreadyQueued)
                                {
                                    //if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillPlan", "Planned skill [" + skill.Key + "] to level [" + skill.Value + "] matches skill in the queue [" + queuedskill.TypeName + "][" + queuedskill.Level + "]", Logging.Teal);
                                    SkillAlreadyQueued = false;
                                    continue;
                                }

                                Logging.Log("AddPlannedSkillToQueue", "CharacterSheet: [" + knownskill.TypeName + "] needs to be training now.", Logging.White);
                                knownskill.AddToEndOfQueue();
                                _nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(2, 5));
                                return true;
                            }

                            Logging.Log("AddPlannedSkillToQueue", "CharacterSheet: [" + knownskill.TypeName + "] is already being trained.", Logging.White);
                            continue;
                        }
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "CharacterSheet: [" + knownskill.TypeName + "] is already at the planned level", Logging.White);
                        continue;
                    }
                    //if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillTrainer", "CharacterSheet: [" + skill.Key + "] has not been learned. we need the skillbook injected.", Logging.White);
                    continue;
                }

                continue;
            }
            //
            // no skill in skill plan could be trained!!!
            //
            if (Settings.Instance.DebugSkillTraining) Logging.Log("SkillTrainer", "CharacterSheet: No skill in plan could be trained. Either we are done or we need to inject some skills", Logging.White);
            return true;
        }

        private static string ParseRomanNumeral(string importedSkill)
        {
            string subString = importedSkill.Substring(importedSkill.Length - 3);
                
            try
            {
                var startsWithWhiteSpace = char.IsWhiteSpace(subString, 0); // 0 = first character
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
                            //if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.ParseRomanNumeral", "[" + SubString + "]" + imported_skill, Logging.Teal);
                            return subString;
                        }
                        //if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.ParseRomanNumeral", "[" + SubString + "]" + imported_skill, Logging.Teal);
                        return subString;
                    }
                    //if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.ParseRomanNumeral", "[" + SubString + "]" + imported_skill, Logging.Teal);
                    return subString;
                }
                //if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.ParseRomanNumeral", "[" + SubString + "]" + imported_skill, Logging.Teal);
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
            /* Make the input string upper-case,
             * because the dictionary does not support lower-case characters. */
            roman = roman.ToUpper();

            /* total = the current total value that will be returned.
             * minus = value to subtract from next numeral. */
            int total = 0, minus = 0;

            for (int icount2 = 0; icount2 < roman.Length; icount2++) // Iterate through characters.
            {
                // Get the value for the current numeral. Takes subtraction into account.
                int thisNumeral = RomanDictionary[roman[icount2]] - minus;

                /* Checks if this is the last character in the string, or if the current numeral
                 * is greater than or equal to the next numeral. If so, we will reset our minus
                 * variable and add the current numeral to the total value. Otherwise, we will
                 * subtract the current numeral from the next numeral, and continue. */
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

            return total; // Return the total.
        }

        public static bool InjectSkillsThatAreReadyToInject()
        {
            //
            // what skills do we have planned that are missing in our head?
            //
            foreach (KeyValuePair<string, int> skill in mySkillPlan)
            {
                if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "SkillPlan: skill [" + skill.Key + "] level [" + skill.Value + "]", Logging.White);

                //MyCharacterSheetSkills.Where(v => v.TypeName == skill.Key && v.Level < skill.Value && !v.InTraining);

                if (MyCharacterSheetSkills.All(i => i.TypeName != skill.Key))
                {
                    //
                    // we do not yet check to make sure that it is a valid thing to do to try to inject this skill
                    // make sure your skill plan is correct ffs!
                    //
                    if (injectSkillBookAttempts >= 2)
                    {
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skill.Key + "] if (injectSkillBookAttempts >= 2)", Logging.Debug);
                        continue;
                    }

                    if (Settings.Instance.DebugSkillTraining) Logging.Log("AddPlannedSkillToQueue", "SkillPlan: skill [" + skill.Key + "] Is not yet injected", Logging.White);

                    IEnumerable<DirectItem> items = Cache.Instance.ItemHangar.Items.Where(k => k.CategoryId == (int)CategoryID.Skill).ToList();
                    if (items.Any())
                    {
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skill.Key + "] found in ItemHangar", Logging.Debug);
                        if (!SkillPlan.InjectSkillBook(skill.Key))
                        {
                            injectSkillBookAttempts++;
                            continue;
                        }

                        continue;
                    }
                    
                    if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skill.Key + "] not found in ItemHangar, trying AmmoHangar", Logging.Debug);
                    items = Cache.Instance.AmmoHangar.Items.Where(k => k.CategoryId == (int) CategoryID.Skill).ToList();

                    if (items.Any())
                    {
                        if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skill.Key + "] found in AmmoHangar", Logging.Debug);
                        if (!SkillPlan.InjectSkillBook(skill.Key))
                        {
                            injectSkillBookAttempts++;
                            continue;
                        }

                        continue;
                    }

                    if (Settings.Instance.DebugSkillTraining) Logging.Log("InjectSkillBook", "SkillBook [" + skill.Key + "] not found in AmmoHangar, not found in any hangar", Logging.Debug);
                    _nextSkillTrainingAction = DateTime.UtcNow.AddSeconds(Cache.Instance.RandomNumber(2, 5));
                    continue;
                }

                continue;
            }

            return false;
        }

        public static bool ReadMyCharacterSheetSkills()
        {
            //Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCharactersheet);
            if (MyCharacterSheetSkills == null || !MyCharacterSheetSkills.Any())
            {
                if (Settings.Instance.DebugSkillTraining) Logging.Log("readMyCharacterSheetSkills", "if (!MyCharacterSheetSkills.Any())", Logging.Teal);
                
                MyCharacterSheetSkills = Cache.Instance.DirectEve.Skills.MySkills;
                return false;
            }

            int icount = 1;

            //Cache.Instance.DirectEve.Skills.RefreshMySkills();
            if (!Cache.Instance.DirectEve.Skills.AreMySkillsReady)
            {
                if(Settings.Instance.DebugSkillTraining) Logging.Log("readMyCharacterSheetSkills", "if (!Cache.Instance.DirectEve.Skills.AreMySkillsReady)", Logging.Teal);
                return false;
            }

            //Logging.Log("readMyCharacterSheetSkills", "MySkillQueue has [" + Cache.Instance.DirectEve.Skills.MySkillQueue.Count() "] items in it", Logging.Teal); 

            foreach (DirectSkill trainedskill in MyCharacterSheetSkills)
            {
                icount++;
                if (Settings.Instance.DebugSkillTraining) Logging.Log("Skills.MyCharacterSheetSkills", "[" + icount + "] SkillName [" + trainedskill.TypeName + "] lvl [" + trainedskill.Level + "] SkillPoints [" + trainedskill.SkillPoints + "] inTraining [" + trainedskill.InTraining + "]", Logging.Teal);
            }
            return true;
        }
    }
}