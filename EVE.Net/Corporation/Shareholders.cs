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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Corporation_Shareholder
   public sealed class Shareholders : APIObject
   {
      public Shareholders() { }

      public Shareholders(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}", keyID, vCode, actorID);
      }

      public override string Uri
      {
         get
         {
            return BaseUrls.corpShareholdersUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class CharacterShareholder
      {
         public CharacterShareholder() { }

         public Int64 shareholderID { get; set; }
         public string shareholderName { get; set; }
         public Int64 shareholderCorporationID { get; set; }
         public string shareholderCorporationName { get; set; }
         public int shares { get; set; }
      }

      public sealed class CorporationShareholder
      {
         public CorporationShareholder() { }

         public Int64 shareholderID { get; set; }
         public string shareholderName { get; set; }
         public int shares { get; set; }
      }

      #endregion

      private List<CharacterShareholder> characters_ = new List<CharacterShareholder>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<CharacterShareholder> characters
      {
         get { return characters_; }
         set { characters_ = value; }
      }

      private List<CorporationShareholder> corporations_ = new List<CorporationShareholder>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<CorporationShareholder> corporations
      {
         get { return corporations_; }
         set { corporations_ = value; }
      }
   }
}
