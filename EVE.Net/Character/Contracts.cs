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

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Character_Contracts
   public sealed class Contracts : APIObject
   {
      public Contracts() { }

      public Contracts(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri { get { return BaseUrls.charContractsUri; } }

      #endregion

      #region Nested Classes

      public sealed class Contract
      {
         public Contract() { }

         public Int64 contractID { get; set; }
         public Int64 issuerID { get; set; }
         public Int64 issuerCorpID { get; set; }
         public Int64 assigneeID { get; set; }
         public Int64 acceptorID { get; set; }
         public Int64 startStationID { get; set; }
         public Int64 endStationID { get; set; }
         public string type { get; set; }
         public string status { get; set; }
         public string title { get; set; }
         public bool forCorp { get; set; }
         public string availability { get; set; }
         public DateTime dateIssued { get; set; }
         public DateTime dateExpired { get; set; }
         public DateTime dateAccepted { get; set; }
         public DateTime dateCompleted { get; set; }
         public int numDays { get; set; }
         public decimal price { get; set; }
         public decimal reward { get; set; }
         public decimal collateral { get; set; }
         public decimal buyout { get; set; }
         public float volume { get; set; }
      }

      #endregion

      private List<Contract> contractList_ = new List<Contract>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Contract> contractList
      {
         get { return contractList_; }
         set { contractList_ = value; }
      }
   }
}
