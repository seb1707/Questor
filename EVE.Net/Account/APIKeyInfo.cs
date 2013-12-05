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
   public sealed class APIKeyInfo : APIObject
   {
      public APIKeyInfo() { }

      #region Overrides

      public APIKeyInfo(string keyid, string vcode)
         : this(keyid, vcode, "") { }

      public APIKeyInfo(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid)
      {
         keyID = keyid;
         vCode = vcode;
      }

      public override bool Query()
      {
         APIReader reader = new APIReader();

         if (String.IsNullOrEmpty(keyID))
             return reader.Query(Uri, this, "");
         else
             return reader.Query(Uri, this, "keyID={0}&vCode={1}", keyID, vCode);
      }

      public override string Uri { get { return BaseUrls.accountApiKeyInfoUri; } }

      #endregion

      #region Nested Classes

      public sealed class Character
      {
         public Character() { }

         public Int64 characterID { get; set; }
         public string characterName { get; set; }
         public string corporationName { get; set; }
         public Int64 corporationID { get; set; }
      }

      #endregion

      public int accessMask { get; set; }
      public string type { get; set; }
      public DateTime expires { get; set; }

      private List<Character> characters_ = new List<Character>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Character> characters
      {
         get { return characters_; }
         set { characters_ = value; }
      }
   }
}
