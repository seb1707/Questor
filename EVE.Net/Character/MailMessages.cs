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
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Mail_Messages
   public sealed class MailMessages : APIObject
   {
      public MailMessages() { }

      public MailMessages(string keyid, string vcode, string actorid)
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
            return BaseUrls.charMailMessagesUri; ;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Message
      {
         public Message() { }

         public Int64 messageID { get; set; }
         public Int64 senderID { get; set; }
         public int senderTypeID { get; set; }
         public DateTime sentDate { get; set; }
         public string title { get; set; }
         public Int64 toCorpOrAllianceID { get; set; }
         public string toCharacterIDs { get; set; }
         public string toListIDs { get; set; }

      }

      #endregion

      private List<Message> messages_ = new List<Message>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Message> messages
      {
         get { return messages_; }
         set { messages_ = value; }
      }
   }
}
