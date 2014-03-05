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

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EVE.Net
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Skill_Tree
   public sealed class SkillTree : APIObject
   {
      public SkillTree() { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "");
      }

      public override string Uri { get { return BaseUrls.eveSkillTreeUri; } }

      #endregion

      #region Nested Classes

      public sealed class Skill
      {
         public Skill() { }

         #region Nested Classes

         public sealed class RequiredSkill
         {
            public RequiredSkill() { }

            public Int64 typeID { get; set; }
            public int skillLevel { get; set; }
         }

         public sealed class ReqAttributes
         {
            public ReqAttributes() { }

            public string primaryAttribute { get; set; }
            public string secondaryAttribute { get; set; }
         }

         public sealed class Bonus
         {
            public Bonus() { }

            public string bonusType { get; set; }
            public float bonusValue { get; set; }
         }

         #endregion

         public string typeName { get; set; }
         public Int64 groupID { get; set; }
         public Int64 typeID { get; set; }
         public bool published { get; set; }
         public string description { get; set; }
         public int rank { get; set; }

         private List<RequiredSkill> requiredSkills_ = new List<RequiredSkill>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<RequiredSkill> requiredSkills
         {
            get { return requiredSkills_; }
            set { requiredSkills_ = value; }
         }

         private ReqAttributes requiredAttributes_ = new ReqAttributes();

         public Skill.ReqAttributes requiredAttributes
         {
            get { return requiredAttributes_; }
            set { requiredAttributes_ = value; }
         }

         private List<Bonus> skillBonusCollection_ = new List<Bonus>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Bonus> skillBonusCollection
         {
            get { return skillBonusCollection_; }
            set { skillBonusCollection_ = value; }
         }
      }

      public sealed class Group
      {
         public Group() { }

         public int groupID { get; set; }
         public string groupName { get; set; }

         private List<Skill> skills_ = new List<Skill>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Skill> skills
         {
            get { return skills_; }
            set { skills_ = value; }
         }
      }

      #endregion

      private List<Group> skillGroups_ = new List<Group>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Group> skillGroups
      {
         get { return skillGroups_; }
         set { skillGroups_ = value; }
      }
   }
}
