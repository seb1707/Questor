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
using System.Globalization;

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Character_ContractItems
   public sealed class ContractItems : APIObject
   {
      private Int64 contractID_;

      public ContractItems() { }

      public ContractItems(string keyid, string vcode, string actorid, string contractID)
         : base(keyid, vcode, actorid)
      {
         contractID_ = Int64.Parse(contractID, CultureInfo.InvariantCulture);
      }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&contractID={3}", keyID, vCode, actorID, contractID_);
      }

      public override string Uri
      {
         get
         {
            return IsCorpKey ? BaseUrls.corpContractItemsUri : BaseUrls.charContractItemsUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Item
      {
         public Item() { }

         public Int64 recordID { get; set; }
         public Int64 typeID { get; set; }
         public Int64 quantity { get; set; }
         public int rawQuantity { get; set; }
         public int singleton { get; set; }
         public int included { get; set; }
      }

      #endregion

      private List<Item> itemList_ = new List<Item>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Item> itemList
      {
         get { return itemList_; }
         set { itemList_ = value; }
      }
   }
}
