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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Market_Orders
   public sealed class MarketOrders : APIObject
   {
      public MarketOrders() { }

      public MarketOrders(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      public static readonly Dictionary<int, string> OrderStateMap = new Dictionary<int,string>
      {
         {0, "open/active"},
         {1, "closed"},
         {2, "expired/fulfilled"},
         {3, "cancelled"},
         {4, "pending"},
         {5, "character deleted"}
      };

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
            return IsCorpKey ? BaseUrls.corpMarketOrdersUri : BaseUrls.charMarketOrdersUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Order
      {
         public Order() { }

         public Int64 orderID { get; set; }
         public Int64 charID { get; set; }
         public Int64 stationID { get; set; }
         public int volEntered { get; set; }
         public int volRemaining { get; set; }
         public int minVolume { get; set; }
         public int orderState { get; set; }
         public int typeID { get; set; }
         public int range { get; set; }
         public int accountKey { get; set; }
         public int duration { get; set; }
         public decimal escrow { get; set; }
         public decimal price { get; set; }
         public bool bid { get; set; }
         public string issued { get; set; }
      }

      #endregion

      private List<Order> orders_ = new List<Order>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Order> orders
      {
         get { return orders_; }
         set { orders_ = value; }
      }
   }
}
