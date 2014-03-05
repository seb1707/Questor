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
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Character_Info
   public sealed class CharacterInfo : APIObject
   {
      public CharacterInfo() { }

      public CharacterInfo(string actorid)
          : base("", "", actorid) { }

      public CharacterInfo(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();

         if (String.IsNullOrEmpty(keyID))
             return reader.Query(Uri, this, "characterID={0}", actorID);
         else
             return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri { get { return BaseUrls.eveCharInfoUri; } }

      #endregion

      #region Nested Classes

      public sealed class EmploymentHistory
      {
         public EmploymentHistory() { }

         public UInt64 recordID { get; set; }
         public UInt64 corporationID { get; set; }
         public DateTime startDate { get; set; }
      }

      #endregion

      public DateTime lastUpdate { get; set; }
      public UInt64 characterID { get; set; }
      public string characterName { get; set; }
      public string race { get; set; }
      public string bloodLine { get; set; }
      public decimal accountBalance { get; set; }
      public int skillPoints { get; set; }
      public string shipName { get; set; }
      public UInt64 shipTypeID { get; set; }
      public string shipTypeName { get; set; }
      public UInt64 corporationID { get; set; }
      public string corporation { get; set; }
      public DateTime corporationDate { get; set; }
      public UInt64 allianceID { get; set; }
      public string alliance { get; set; }
      public DateTime allianceDate { get; set; }
      public string lastKnownLocation { get; set; }
      public double securityStatus { get; set; }

      private List<EmploymentHistory> employmentHistory_ = new List<EmploymentHistory>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<EmploymentHistory> employmentHistory
      {
         get { return employmentHistory_; }
         set { employmentHistory_ = value;  }
      }
   }
}
