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
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Alliance_List
   public sealed class AllianceList : APIObject
   {
      private string version_;

      public AllianceList() { }

      public AllianceList(string version)
      {
         version_ = version;
      }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();

         StringBuilder sb = new StringBuilder();

         if (!string.IsNullOrEmpty(version_))
            sb.AppendFormat("&version={0}", version_);

         return reader.Query(Uri, this, sb.ToString());
      }

      public override string Uri { get { return BaseUrls.eveAllianceListUri; } }

      #endregion

      #region Nested Classes

      public sealed class Corporation
      {
         public Corporation() { }

         public Int64 corporationID { get; set; }
         public DateTime startDate { get; set; }
      }

      public sealed class Alliance
      {
         public Alliance() { }

         public string name { get; set; }
         public string shortName { get; set; }
         public Int64 allianceID { get; set; }
         public Int64 executorCorpID { get; set; }
         public int memberCount { get; set; }
         public DateTime startDate { get; set; }

         private List<Corporation> memberCorporations_ = new List<Corporation>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Corporation> memberCorporations
         {
            get { return memberCorporations_; }
            set { memberCorporations_ = value; }
         }
      }

      #endregion

      private List<Alliance> alliances_ = new List<Alliance>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Alliance> alliances
      {
         get { return alliances_; }
         set { alliances_ = value; }
      }
   }
}
