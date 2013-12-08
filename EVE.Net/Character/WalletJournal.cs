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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Wallet_Journal
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Wallet_Journal
   public sealed class WalletJournal : APIObject
   {
      public WalletJournal() { }

      public WalletJournal(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public bool Query(int rowCount)
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&rowCount={3}", keyID, vCode, actorID, rowCount);
      }

      public bool Query(int rowCount, String startID)
      {
          APIReader reader = new APIReader();
          return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&rowCount={3}&fromID={4}", keyID, vCode, actorID, rowCount, startID);
      }

      public override string Uri
      {
         get
         {
            return IsCorpKey ? BaseUrls.corpWalletJournalUri : BaseUrls.charWalletJournalUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Entry
      {
         public Entry() { }

         public DateTime date { get; set; }
         public Int64 refID { get; set; }

         // refTypeID can be looked up using the EVE/RefTypes api call.
         public int refTypeID { get; set; }
         public string ownerName1 { get; set; }
         public int ownerID1 { get; set; }
         public int owner1TypeID { get; set; }
         public string ownerName2 { get; set; }
         public int ownerID2 { get; set; }
         public int owner2TypeID { get; set; }
         public string argName1 { get; set; }
         public int argID1 { get; set; }
         public decimal amount { get; set; }
         public decimal balance { get; set; }
         public string reason { get; set; }
         public Int64 taxReceiverID { get; set; }
         public decimal taxAmount { get; set; }
      }

      #endregion

      private List<Entry> transactions_ = new List<Entry>();

      [TypeConverter(typeof(ExpandableObjectConverter)),
      XmlIdentifier("transactions", "entries")]
      public List<Entry> transactions
      {
         get { return transactions_; }
         set { transactions_ = value; }
      }
   }
}
