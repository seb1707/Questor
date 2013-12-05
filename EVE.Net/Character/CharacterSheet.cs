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

using EVE.Net.Corporation;
using System.ComponentModel;

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Character_Sheet
   public sealed class CharacterSheet : APIObject
   {
      public CharacterSheet() { }

      public CharacterSheet(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri { get { return BaseUrls.characterSheetUri; } }

      #endregion

      #region Nested Classes

      public sealed class Attributes
      {
         public Attributes() { }

         public int intelligence { get; set; }
         public int memory { get; set; }
         public int charisma { get; set; }
         public int perception { get; set; }
         public int willpower { get; set; }
      }

      public sealed class AttributeEnhancers
      {
         public AttributeEnhancers() { }

         #region Nested Classes

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public class Augmentator
         {
            public Augmentator() { }

            public string augmentatorName { get; set; }
            public int augmentatorValue { get; set; }
         }

         #endregion

         #region Properties

         private Augmentator memoryBonus_ = new Augmentator();

         public Augmentator memoryBonus
         {
            get { return memoryBonus_; }
            set { memoryBonus_ = value; }
         }

         private Augmentator perceptionBonus_ = new Augmentator();

         public Augmentator perceptionBonus
         {
            get { return perceptionBonus_; }
            set { perceptionBonus_ = value; }
         }

         private Augmentator willpowerBonus_ = new Augmentator();

         public Augmentator willpowerBonus
         {
            get { return willpowerBonus_; }
            set { willpowerBonus_ = value; }
         }

         private Augmentator intelligenceBonus_ = new Augmentator();

         public Augmentator intelligenceBonus
         {
            get { return intelligenceBonus_; }
            set { intelligenceBonus_ = value; }
         }

         private Augmentator charismaBonus_ = new Augmentator();

         public Augmentator charismaBonus
         {
            get { return charismaBonus_; }
            set { charismaBonus_ = value; }
         }

         #endregion
      }

      public sealed class Skill
      {
         public Skill() { }

         public Int64 typeID { get; set; }
         public int skillpoints { get; set; }
         public int level { get; set; }
         public bool published { get; set; }
      }

      public sealed class Certificate
      {
         public Certificate() { }

         public Int64 certificateID { get; set; }
      }

      public sealed class Role
      {
         public Role() { }

         public Int64 roleID { get; set; }
         public string roleName { get; set; }
      }

      public sealed class Title
      {
         public Title() { }

         public int titleID { get; set; }
         public string titleName { get; set; }
      }

      #endregion

      #region Properties

      public UInt64 characterID { get; set; }
      public string name { get; set; }
      public string DoB { get; set; }
      public string race { get; set; }
      public string bloodLine { get; set; }
      public string ancestry { get; set; }
      public string gender { get; set; }
      public string corporationName { get; set; }
      public UInt64 corporationID { get; set; }
      public string allianceName { get; set; }
      public UInt64 allianceID { get; set; }
      public string cloneName { get; set; }
      public int cloneSkillPoints { get; set; }
      public decimal balance { get; set; }

      private AttributeEnhancers attributeEnhancers_ = new AttributeEnhancers();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public AttributeEnhancers attributeEnhancers
      {
         get { return attributeEnhancers_; }
         set { attributeEnhancers_ = value; }
      }

      private Attributes attributes_ = new Attributes();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public Attributes attributes
      {
         get { return attributes_; }
         set { attributes_ = value; }
      }

      private List<Skill> skills_ = new List<Skill>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Skill> skills
      {
         get { return skills_; }
         set { skills_ = value; }
      }

      private List<Certificate> certificates_ = new List<Certificate>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Certificate> certificates
      {
         get { return certificates_; }
         set { certificates_ = value; }
      }

      private List<Role> corporationRoles_ = new List<Role>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Role> corporationRoles
      {
         get { return corporationRoles_; }
         set { corporationRoles_ = value; }
      }

      private List<Role> corporationRolesAtHQ_ = new List<Role>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Role> corporationRolesAtHQ
      {
         get { return corporationRolesAtHQ_; }
         set { corporationRolesAtHQ_ = value; }
      }

      private List<Role> corporationRolesAtBase_ = new List<Role>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Role> corporationRolesAtBase
      {
         get { return corporationRolesAtBase_; }
         set { corporationRolesAtBase_ = value; }
      }

      private List<Title> corporationTitles_ = new List<Title>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Title> corporationTitles
      {
         get { return corporationTitles_; }
         set { corporationTitles_ = value; }
      }

      #endregion
   }
}
