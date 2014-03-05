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
using System.Globalization;

namespace EVE.Net.Character
{
   // http://wiki.eve-id.net/APIv2_Char_Locations_XML
   public sealed class Locations : APIObject
   {
      private string itemIDs_;

      public Locations() { }

      public Locations(string keyid, string vcode, string actorid, params Int64[] itemIDs)
         : base(keyid, vcode, actorid)
      {
         if (itemIDs == null || itemIDs.Length == 0)
            throw new ArgumentException("You must supply at least one itemID.");

         itemIDs_ = String.Join(",", Array.ConvertAll(itemIDs, x => x.ToString(CultureInfo.InvariantCulture)));
      }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&IDs={3}", keyID, vCode, actorID, itemIDs_);
      }

      public override string Uri
      {
         get
         {
            return IsCorpKey ? BaseUrls.corpLocationsUri : BaseUrls.charLocationsUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Location
      {
         public Location() { }

         public Int64 itemID { get; set; }
         public string itemName { get; set; }
         public Int64 x { get; set; }
         public Int64 y { get; set; }
         public Int64 z { get; set; }
      }

      #endregion

      private List<Location> locations_ = new List<Location>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Location> locations
      {
         get { return locations_; }
         set { locations_ = value; }
      }
   }
}
