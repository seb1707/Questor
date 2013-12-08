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
using System.Text;
using System.ComponentModel;

namespace EVE.Net
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Factional_Warfare_Statistics
   public sealed class FacWarStats : APIObject
   {
      public FacWarStats() { }

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "");
      }

      public override string Uri { get { return BaseUrls.eveFactionalWarfareStatsUri; } }

      #region Nested Classes

      public sealed class Totals
      {
         public Totals() { }

         public int killsYesterday { get; set; }
         public int killsLastWeek { get; set; }
         public int killsTotal { get; set; }

         public int victoryPointsYesterday { get; set; }
         public int victoryPointsLastWeek { get; set; }
         public int victoryPointsTotal { get; set; }
      }

      public sealed class Faction
      {
         public Faction() { }

         public Int64 factionID { get; set; }
         public string factionName { get; set; }
         public int pilots { get; set; }
         public int systemsControlled { get; set; }

         public int killsYesterday { get; set; }
         public int killsLastWeek { get; set; }
         public int killsTotal { get; set; }

         public int victoryPointsYesterday { get; set; }
         public int victoryPointsLastWeek { get; set; }
         public int victoryPointsTotal { get; set; }
      }

      public sealed class FactionWar
      {
         public FactionWar() { }

         public Int64 factionID { get; set; }
         public string factionName { get; set; }
         public Int64 againstID { get; set; }
         public string againstName { get; set; }
      }

      #endregion

      private Totals totals_ = new Totals();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public Totals totals
      {
         get { return totals_; }
         set { totals_ = value; }
      }

      private List<Faction> factions_ = new List<Faction>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Faction> factions
      {
         get { return factions_; }
         set { factions_ = value; }
      }

      private List<FactionWar> factionWars_ = new List<FactionWar>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<FactionWar> factionWars
      {
         get { return factionWars_; }
         set { factionWars_ = value; }
      }
   }
}
