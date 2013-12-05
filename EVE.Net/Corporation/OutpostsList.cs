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

namespace EVE.Net.Corporation
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Outpost_List
   public sealed class OutpostsList : APIObject
   {
      public OutpostsList() { }

      public OutpostsList(string keyid, string vcode, string actorid)
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
            return BaseUrls.corpOutpostsListUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Outpost
      {
         public Outpost() { }

         public Int64 stationID { get; set; }
         public Int64 ownerID { get; set; }
         public string stationName { get; set; }
         public Int64 solarSystemID { get; set; }
         public decimal dockingCostPerShipVolume { get; set; }
         public decimal officeRentalCost { get; set; }
         public Int64 stationTypeID { get; set; }
         public float reprocessingEfficiency { get; set; }
         public float reprocessingStationTake { get; set; }
         public Int64 standingOwnerID { get; set; }
      }

      #endregion

      private List<Outpost> corporationStarbases_ = new List<Outpost>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Outpost> corporationStarbases
      {
         get { return corporationStarbases_; }
         set { corporationStarbases_ = value; }
      }
   }
}
