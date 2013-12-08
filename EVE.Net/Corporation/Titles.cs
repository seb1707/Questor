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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Titles
   public sealed class Titles : APIObject
   {
      public Titles() { }

      public Titles(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri { get { return BaseUrls.corpTitlesUri; } }

      #endregion

      #region Nested Classes

      public sealed class Role
      {
         public Role() { }

         public Int64 roleID { get; set; }
         public string roleName { get; set; }
         public string roleDescription { get; set; }
      }

      public sealed class Title
      {
         public Title() { }

         public int titleID { get; set; }
         public string titleName { get; set; }

         private List<Role> roles_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> roles
         {
            get { return roles_; }
            set { roles_ = value; }
         }

         private List<Role> grantableRoles_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> grantableRoles
         {
            get { return grantableRoles_; }
            set { grantableRoles_ = value; }
         }

         private List<Role> rolesAtHQ_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> rolesAtHQ
         {
            get { return rolesAtHQ_; }
            set { rolesAtHQ_ = value; }
         }

         private List<Role> grantableRolesAtHQ_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> grantableRolesAtHQ
         {
            get { return grantableRolesAtHQ_; }
            set { grantableRolesAtHQ_ = value; }
         }

         private List<Role> rolesAtBase_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> rolesAtBase
         {
            get { return rolesAtBase_; }
            set { rolesAtBase_ = value; }
         }

         private List<Role> grantableRolesAtBase_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> grantableRolesAtBase
         {
            get { return grantableRolesAtBase_; }
            set { grantableRolesAtBase_ = value; }
         }

         private List<Role> rolesAtOther_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> rolesAtOther
         {
            get { return rolesAtOther_; }
            set { rolesAtOther_ = value; }
         }

         private List<Role> grantableRolesAtOther_ = new List<Role>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Role> grantableRolesAtOther
         {
            get { return grantableRolesAtOther_; }
            set { grantableRolesAtOther_ = value; }
         }
      }

      #endregion

      private List<Title> titles_ = new List<Title>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Title> titles
      {
         get { return titles_; }
         set { titles_ = value; }
      }
   }
}
