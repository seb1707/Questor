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

namespace EVE.Net.Account
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Account_Characters
   public sealed class Characters : APIObject
   {
      public Characters() { }

      #region Overrides

      public Characters(string keyid, string vcode)
         : base(keyid, vcode, "") { }

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}", keyID, vCode);
      }

      public override string Uri { get { return BaseUrls.accountCharacterListUri; } }

      #endregion

      #region Nested Classes

      public sealed class Character
      {
         public Character() { }

         public Int64 characterID { get; set; }
         public string name { get; set; }
         public string corporationName { get; set; }
         public Int64 corporationID { get; set; }
      }

      #endregion

      private List<Character> characters_ = new List<Character>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Character> characters
      {
         get { return characters_; }
         set { characters_ = value; }
      }
   }
}
