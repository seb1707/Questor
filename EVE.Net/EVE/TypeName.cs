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
   // http://wiki.eve-id.net/APIv2_Eve_TypeName_XML
   public sealed class TypeName : APIObject
   {
      public string[] typeIDs_ = null;

      public TypeName() { }

      public TypeName(params string[] typeIDs)
      {
         typeIDs_ = typeIDs;
      }

      #region Overrides

      public override bool Query()
      {
         if (typeIDs_ == null || typeIDs_.Length == 0)
         {
             if (Settings.FailGracefully)
             {
                 this.error = new APIError(0, "Invalid argument. One or more typeIDs are required.");
                 return false;
             }
             else
             {
                 this.error = new APIError(0, "Invalid argument. One or more character IDs are required.");
                 throw new ArgumentException("Invalid argument. One or more typeIDs are required.");
             }
         }

         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "&ids={0}", String.Join(",", typeIDs_));
      }

      public override string Uri { get { return BaseUrls.eveTypeNameUri; } }

      #endregion

      #region Nested Classes

      public sealed class GameType
      {
         public GameType() { }

         public Int64 typeID { get; set; }
         public string typeName { get; set; }
      }

      #endregion

      private List<GameType> gameTypes_ = new List<GameType>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<GameType> types
      {
         get { return gameTypes_; }
         set { gameTypes_ = value; }
      }
   }
}
