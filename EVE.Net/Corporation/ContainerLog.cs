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

namespace EVE.Net.Corporation
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Container_Log
   public sealed class ContainerLog : APIObject
   {
      public ContainerLog() { }

      public ContainerLog(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri { get { return BaseUrls.containerLogUri; } }

      #endregion

      #region Nested Classes

      public sealed class Entry
      {
         public Entry() { }

         public DateTime logTime { get; set; }
         public Int64 itemID { get; set; }
         public int itemTypeID { get; set; }
         public Int64 actorID { get; set; }
         public string actorName { get; set; }
         public int flag { get; set; }
         public Int64 locationID { get; set; }
         public string action { get; set; }
         public string passwordType { get; set; }
         public Int64 typeID { get; set; }
         public int quantity { get; set; }
      }

      #endregion

      private List<Entry> containerLog_ = new List<Entry>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Entry> containerLog
      {
         get { return containerLog_; }
         set { containerLog_ = value; }
      }
   }
}
