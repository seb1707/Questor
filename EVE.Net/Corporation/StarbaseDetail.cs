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
    // http://wiki.eve-id.net/APIv2_Corp_StarbaseDetail_XML
    public sealed class StarbaseDetail : APIObject
    {
        public StarbaseDetail() { }

        public StarbaseDetail(string keyid, string vcode)
            : base(keyid, vcode, "") { }

        #region Overrides

        public override bool Query()
        {
            throw new NotImplementedException();
        }

        public bool Query(UInt64 itemID)
        {
            APIReader reader = new APIReader();
            return reader.Query(Uri, this, "keyID={0}&vCode={1}&itemID={2}", keyID, vCode, itemID);
        }

        public override string Uri
        {
            get
            {
                return BaseUrls.corpStarbaseDetailUri;
            }
        }

        #endregion

        #region Nested Classes
        public class GeneralSettings
        {
            public GeneralSettings()
            { }

            public int usageFlags { get; set; }
            public int deployFlags { get; set; }
            public bool allowCorporationMembers { get; set; }
            public bool allowAllianceMembers { get; set; }
        }

        public class CombatSettings
        {
            public CombatSettings()
            { }

            public StandingsFrom useStandingsFrom { get; set; }
            public StandingDrop onStandingDrop { get; set; }

            #region Subnested classes
            public class StandingsFrom
            {
                public StandingsFrom()
                { }

                public UInt64 ownerID { get; set; }
            }

            public class StandingDrop
            {
                public StandingDrop()
                { }

                public int standing { get; set; }
            }

            public class StatusDrop
            {
                public StatusDrop()
                { }

                public bool enabled { get; set; }
                public int standing { get; set; }
            }

            public class Aggression
            {
                public Aggression()
                { }

                public bool enabled { get; set; }
            }

            public class CorporationWar
            {
                public CorporationWar()
                { }

                public bool enabled { get; set; }
            }
            #endregion Subnested classes
        }

        public class Fuel
        {
            public Fuel()
            { }

            public int typeID { get; set; }
            public int quantity { get; set; }
        }
        #endregion

        #region Properties
        public int state { get; set; }
        public DateTime stateTimestamp { get; set; }
        public DateTime onlineTimestamp { get; set; }
        public GeneralSettings generalSettings { get; set; }
        public CombatSettings combatSettings { get; set; }

        private List<Fuel> fuel_ = new List<Fuel>();
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public List<Fuel> fuel
        {
            get { return fuel_; }
            set { fuel_ = value; }
        }
        #endregion
    }
}
