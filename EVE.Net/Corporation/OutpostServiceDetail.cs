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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Outpost_Service_Detail
   public sealed class OutpostServiceDetail : APIObject
   {
      private string itemID_;

      public OutpostServiceDetail() { }

      public OutpostServiceDetail(string keyid, string vcode, string actorid)
         : this(keyid, vcode, actorid, "") { }

      public OutpostServiceDetail(string keyid, string vcode, string actorid, string itemID)
         : base(keyid, vcode, actorid)
      {
         itemID_ = itemID;
      }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();

         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);

         if (!String.IsNullOrEmpty(itemID_))
            sb.AppendFormat("&itemID={0}", itemID_);

         return reader.Query(Uri, this, sb.ToString());
      }

      public override string Uri
      {
         get
         {
            return BaseUrls.corpOutpostServiceDetailUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Detail
      {
         public Detail() { }

         public Int64 stationID { get; set; }
         public Int64 ownerID { get; set; }
         public string serviceName { get; set; }
         public float minStanding { get; set; }
         public int surchargePerBadStanding { get; set; }
         public int discountPerGoodStanding { get; set; }
      }

      #endregion

      private List<Detail> outpostServiceDetails_ = new List<Detail>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Detail> outpostServiceDetails
      {
         get { return outpostServiceDetails_; }
         set { outpostServiceDetails_ = value; }
      }
   }

}
