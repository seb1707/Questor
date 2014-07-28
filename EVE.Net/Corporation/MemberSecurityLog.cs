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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Member_Security_Log
   public sealed class MemberSecurityLog : APIObject
   {
      public MemberSecurityLog() { }

      public MemberSecurityLog(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri { get { return BaseUrls.corpMemberSecurityLogUri; } }

      #endregion

      #region Nested Classes

      public sealed class Role
      {
         public Role() { }

         public Int64 roleID { get; set; }
         public string roleName { get; set; }
      }

      public sealed class LogEntry
      {
         public LogEntry() { }

         public DateTime changeTime { get; set; }
         public Int64 characterID { get; set; }
         public string characterName { get; set; }
         public Int64 issuerID { get; set; }
         public string issuerName { get; set; }
         public string roleLocationType { get; set; }

         private List<Role> oldRoles_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> oldRoles
         {
            get { return oldRoles_; }
            set { oldRoles_ = value; }
         }

         private List<Role> newRoles_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> newRoles
         {
            get { return newRoles_; }
            set { newRoles_ = value; }
         }
      }

      #endregion

      private List<LogEntry> roleHistory_ = new List<LogEntry>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<LogEntry> roleHistory
      {
         get { return roleHistory_; }
         set { roleHistory_ = value; }
      }
   }
}
