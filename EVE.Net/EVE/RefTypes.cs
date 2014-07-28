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

namespace EVE.Net
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Reference_Types
   public sealed class RefTypes : APIObject
   {
      public RefTypes() { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "");
      }

      public override string Uri { get { return BaseUrls.eveRefTypesUri; } }

      #endregion

      #region Nested Classes

      public sealed class ReferenceType
      {
         public ReferenceType() { }

         public Int64 refTypeID { get; set; }
         public string refTypeName { get; set; }
      }

      #endregion

      private List<ReferenceType> refTypes_ = new List<ReferenceType>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<ReferenceType> refTypes
      {
         get { return refTypes_; }
         set { refTypes_ = value; }
      }
   }
}
