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
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Conquerable_Station_List
   public sealed class ConquerableStationList : APIObject
   {
      public ConquerableStationList() { }

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "");
      }

      public override string Uri { get { return BaseUrls.eveConquerableStationListUri; } }


      #region Nested Classes

      public sealed class Outpost
      {
         public Outpost() { }

         public Int64 stationID { get; set; }
         public string stationName { get; set; }
         public Int64 stationTypeID { get; set; }
         public Int64 solarSystemID { get; set; }
         public Int64 corporationID { get; set; }
         public string corporationName { get; set; }
      }

      #endregion

      private List<Outpost> outposts_ = new List<Outpost>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Outpost> outposts
      {
         get { return outposts_; }
         set { outposts_ = value; }
      }
   }
}
