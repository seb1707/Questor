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

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_NPC_Standings
   public sealed class NPCStandings : APIObject
   {
      public NPCStandings() { }

      public NPCStandings(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri
      {
         get
         {
            return IsCorpKey ? BaseUrls.corpNPCStandingsUri : BaseUrls.charNPCStandingsUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Standing
      {
         public Standing() { }

         public Int64 fromID { get; set; }
         public string fromName { get; set; }
         public float standing { get; set; }
      }

      public sealed class Standings
      {
         public Standings() { }

         private List<Standing> agents_ = new List<Standing>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Standing> agents
         {
            get { return agents_; }
            set { agents_ = value; }
         }

         private List<Standing> npcCorporations_ = new List<Standing>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Standing> NPCCorporations
         {
            get { return npcCorporations_; }
            set { npcCorporations_ = value; }
         }

         private List<Standing> factions_ = new List<Standing>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Standing> factions
         {
            get { return factions_; }
            set { factions_ = value; }
         }
      }

      #endregion

      private Standings standings_ = new Standings();

      [XmlIdentifier("corporationNPCStandings", "characterNPCStandings"),
       TypeConverter(typeof(ExpandableObjectConverter))]
      public Standings standings
      {
         get { return standings_; }
         set { standings_ = value; }
      }
   }
}
