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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Corporation_Sheet
   public sealed class CorporationSheet : APIObject
   {
      public CorporationSheet() { }

      public CorporationSheet(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public bool Query(string corpid)
      {
          APIReader reader = new APIReader();
          return reader.Query(Uri, this, "corporationID={0}", corpid);
      }

      public override string Uri { get { return BaseUrls.corpSheetUri; } }

      #endregion

      #region Nested Classes

      public sealed class Role
      {
         public Role() { }

         public int roleID { get; set; }
         public string roleName { get; set; }
      }

      public sealed class Title
      {
         public Title() { }

         public int titleID { get; set; }
         public string titleName { get; set; }
      }

      public sealed class Division
      {
         public Division() { }

         public int accountKey { get; set; }
         public string description { get; set; }
      }

      public sealed class CorporateLogo
      {
         public CorporateLogo() { }

         public UInt64 graphicID { get; set; }
         public int shape1 { get; set; }
         public int shape2 { get; set; }
         public int shape3 { get; set; }
         public int color1 { get; set; }
         public int color2 { get; set; }
         public int color3 { get; set; }
      }

      #endregion

      #region Properties

      public UInt64 corporationID { get; set; }
      public string corporationName { get; set; }
      public string ticker { get; set; }
      public UInt64 ceoID { get; set; }
      public string ceoName { get; set; }
      public UInt64 stationID { get; set; }
      public string stationName { get; set; }
      public string description { get; set; }
      public string url { get; set; }
      public UInt64 allianceID { get; set; }
      public string allianceName { get; set; }
      public int factionID { get; set; }
      public float taxRate { get; set; }
      public int memberCount { get; set; }
      public int memberLimit { get; set; }
      public UInt64 shares { get; set; }

      private CorporateLogo logo_ = new CorporateLogo();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public CorporateLogo logo
      {
         get { return logo_; }
         set { logo_ = value; }
      }

      List<Division> divisions_;

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Division> divisions
      {
         get { return divisions_; }
         set { divisions_ = value; }
      }

      List<Division> walletDivisions_;

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Division> walletDivisions
      {
         get { return walletDivisions_; }
         set { walletDivisions_ = value; }
      }

      #endregion
   }
}
