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

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Notification_Texts
   public sealed class NotificationTexts : APIObject
   {
      public string[] ids_ = null;

      public NotificationTexts() { }

      public NotificationTexts(string keyid, string vcode, string actorid, params string[] ids)
         : base(keyid, vcode, actorid)
      {
         ids_ = ids;
      }

      #region Overrides

      public override bool Query()
      {
         if (ids_ == null || ids_.Length == 0)
         {
             if (Settings.FailGracefully)
             {
                 this.error = new APIError(0, "Invalid argument. One or more ids are required.");
                 return false;
             }
             else
             {
                 this.error = new APIError(0, "Invalid argument. One or more ids are required.");
                 throw new ArgumentException("Invalid argument. One or more ids are required.");
             }
         }

         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&ids={3}", keyID, vCode, actorID, String.Join(",", ids_));
      }

      public override string Uri
      {
         get
         {
            return BaseUrls.charNotificationTextsUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Notice
      {
         public Notice() { }

         public Int64 notificationID { get; set; }

         [XmlIdentifier("CDATA")]
         public string content { get; set; }
      }

      #endregion

      private List<Notice> notices_ = new List<Notice>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Notice> notifications
      {
         get { return notices_; }
         set { notices_ = value; }
      }
   }
}
