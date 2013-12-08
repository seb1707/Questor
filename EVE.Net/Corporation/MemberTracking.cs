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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Member_Tracking
   public sealed class MemberTracking : APIObject
   {
      private bool? extended_ = null;

      public MemberTracking() { }

      public MemberTracking(string keyid, string vcode, string actorid)
         : this(keyid, vcode, actorid, true) { }

      public MemberTracking(string keyid, string vcode, string actorid, bool? extended)
         : base(keyid, vcode, actorid)
      {
         extended_ = extended;
      }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();

         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("keyID={0}&vCode={1}", keyID, vCode);

         if ( extended_ != null && extended_.Value == true )
            sb.Append("&extended=1");

         return reader.Query(Uri, this, sb.ToString());
      }

      public override string Uri { get { return BaseUrls.corpMemberTrackingUri; } }

      #endregion

      #region Nested Classes

      public sealed class Member
      {
         public Member() {}

         public Int64 characterID { get; set; }
         public string name { get; set; }
         public DateTime startDateTime  { get; set; }
         public Int64 baseID { get; set; }

         [XmlIdentifier("base")]
         public string baseLocation { get; set; }
         public string title { get; set; }
         public DateTime logonDateTime { get; set; }
         public DateTime logoffDateTime { get; set; }
         public Int64 locationID  { get; set; }
         public string location { get; set; }
         public Int64 shipTypeID { get; set; }
         public string shipType { get; set; }
         public Int64 roles { get; set; }
         public Int64 grantableRoles { get; set; }
      }

      #endregion

      private List<Member> members_ = new List<Member>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Member> members
      {
         get { return members_; }
         set { members_ = value; }
      }
   }
}
