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
    // http://wiki.eve-id.net/APIv2_Corp_StarbaseList_XML
    public sealed class StarbaseList : APIObject
    {
        public StarbaseList() { }

        public StarbaseList(string keyid, string vcode)
            : base(keyid, vcode, "") { }

        #region Overrides

        public override bool Query()
        {
            APIReader reader = new APIReader();
            return reader.Query(Uri, this, "keyID={0}&vCode={1}", keyID, vCode);
        }

        public override string Uri
        {
            get
            {
                return BaseUrls.corpStarbaseListUri;
            }
        }

        #endregion

        #region Nested Classes

        public sealed class Starbase
        {
            public Starbase() { }

            public UInt64 itemID { get; set; }
            public int typeID { get; set; }
            public UInt64 locationID { get; set; }
            public UInt64 moonID { get; set; }
            public int state { get; set; }
            public DateTime stateTimestamp { get; set; }
            public DateTime onlineTimestamp { get; set; }
            public int standingOwnerID { get; set; }

            public static string[] States = { "Unanchored", "Anchored / Offline", "Onlining", "Reinforced", "Online" };
        }

        #endregion

        private List<Starbase> starbases_ = new List<Starbase>();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public List<Starbase> starbases
        {
            get { return starbases_; }
            set { starbases_ = value; }
        }
    }
}
