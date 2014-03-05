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
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Factional_Warfare_Top_Stats
   public sealed class FacWarTopStats : APIObject
   {
      public FacWarTopStats() { }

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "");
      }

      public override string Uri { get { return BaseUrls.eveFactionalWarfareTopStatsUri; } }

      #region Nested Classes

      public sealed class Point
      {
         public Point() { }

         [XmlIdentifier("characterID", "corporationID", "factionID")]
         public Int64 actorID { get; set; }

         [XmlIdentifier("characterName", "corporationName", "factionName")]
         public string actorName { get; set; }

         [XmlIdentifier("kills", "victoryPoints")]
         public int points { get; set; }
      }

      public sealed class Faction
      {
         public Faction() { }

         private List<Point> killsYesterday_ = new List<Point>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Point> KillsYesterday
         {
            get { return killsYesterday_; }
            set { killsYesterday_ = value; }
         }

         private List<Point> killsLastWeek_ = new List<Point>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Point> KillsLastWeek
         {
            get { return killsLastWeek_; }
            set { killsLastWeek_ = value; }
         }

         private List<Point> killsTotal_ = new List<Point>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Point> KillsTotal
         {
            get { return killsTotal_; }
            set { killsTotal_ = value; }
         }

         private List<Point> victoryPointsYesterday_ = new List<Point>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Point> VictoryPointsYesterday
         {
            get { return victoryPointsYesterday_; }
            set { victoryPointsYesterday_ = value; }
         }

         private List<Point> victoryPointsLastWeek_ = new List<Point>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Point> VictoryPointsLastWeek
         {
            get { return victoryPointsLastWeek_; }
            set { victoryPointsLastWeek_ = value; }
         }

         private List<Point> victoryPointsTotal_ = new List<Point>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Point> VictoryPointsTotal
         {
            get { return victoryPointsTotal_; }
            set { victoryPointsTotal_ = value; }
         }
      }

      #endregion

      private Faction characters_ = new Faction();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public Faction characters
      {
         get { return characters_; }
         set { characters_ = value; }
      }

      private Faction corporations_ = new Faction();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public Faction corporations
      {
         get { return corporations_; }
         set { corporations_ = value; }
      }

      private Faction factions_ = new Faction();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public Faction factions
      {
         get { return factions_; }
         set { factions_ = value; }
      }
   }
}
