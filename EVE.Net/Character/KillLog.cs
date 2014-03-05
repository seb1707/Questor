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
    // This API is deprecated as of 2013/06/28.
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Kill_Log
   public sealed class KillLog : APIObject
   {
      public KillLog() { }

      public KillLog(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public bool Query(int beforeKillID)
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&beforeKillID={3}", keyID, vCode, actorID, beforeKillID);
      }

      public override string Uri
      {
         get
         {
            return IsCorpKey ? BaseUrls.corpKillLogUri : BaseUrls.charKillLogUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Combatant
      {
         public Combatant() { }

         public UInt64 characterID { get; set; }
         public string characterName { get; set; }
         public UInt64 corporationID { get; set; }
         public string corporationName { get; set; }
         public UInt64 allianceID { get; set; }
         public string allianceName { get; set; }
         public UInt64 factionID { get; set; }
         public string factionName { get; set; }
         public int damageTaken { get; set; }
         public int damageDone { get; set; }
         public Int64 shipTypeID { get; set; }
         public bool finalBlow { get; set; }
         public Int64 weaponTypeID { get; set; }
         public float securityStatus { get; set; }
      }

      public sealed class Item
      {
         public Item() { }

         public int flag { get; set; }
         public int qtyDropped { get; set; }
         public int qtyDestroyed { get; set; }
         public int typeID { get; set; }
         public int singleton { get; set; }
      }

      public sealed class Kill
      {
         public Kill() { }

         public Int64 killID { get; set; }
         public Int64 solarSystemID { get; set; }
         public DateTime killTime { get; set; }
         public Int64 moonID { get; set; }
         public Combatant victim { get; set; }

         private List<Combatant> attackers_ = new List<Combatant>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Combatant> attackers
         {
            get { return attackers_; }
            set { attackers_ = value; }
         }

         private List<Item> items_ = new List<Item>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Item> items
         {
            get { return items_; }
            set { items_ = value; }
         }
      }

      #endregion

      private List<Kill> kills_ = new List<Kill>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Kill> kills
      {
         get { return kills_; }
         set { kills_ = value; }
      }
   }
}
