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
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Character_ID
   public sealed class CharacterID : APIObject
   {
      private string[] characterNames_ = null;

      public CharacterID() { }

      public CharacterID(params string[] characterNames)
      {
         characterNames_ = characterNames;
      }

      #region Overrides

      public override bool Query()
      {
         if (characterNames_ == null || characterNames_.Length == 0)
         {
             if (Settings.FailGracefully)
             {
                 this.error = new APIError(0, "Invalid argument. One or more character IDs are required.");
                 return false;
             }
             else
             {
                 this.error = new APIError(0, "Invalid argument. One or more character IDs are required.");
                 throw new ArgumentException("Invalid argument. One or more character IDs are required.");
             }
         }

         APIReader reader = new APIReader();

         return reader.Query(Uri, this, "&names={0}", String.Join(",", characterNames_));
      }

      public override string Uri { get { return BaseUrls.eveCharacterIDUri; } }

      #endregion

      #region Nested Classes

      public sealed class Character
      {
         public Character() { }

         public string name { get; set; }
         public Int64 characterID { get; set; }
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
