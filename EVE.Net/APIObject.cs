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
using System.Globalization;

namespace EVE.Net
{
   public abstract class APIObject : IAPIReader
   {
      protected APIReader reader = new APIReader();
      public string keyID;
      public string vCode;
      public string actorID; // character, corporation or alliance id

      private string cacheFile;
      private DateTime cachedTime = DateTime.MinValue;
      private bool? isCorpKey = null;

      public APIError error = null;

      public abstract string Uri { get; }

      public abstract bool Query();

      public string CacheFile { get { return cacheFile; }  set { cacheFile = value; } }
      public DateTime CachedTime { get { return cachedTime; }  set { cachedTime = value; } }

      public bool IsCorpKey
      {
         get
         {
            if (isCorpKey != null)
               return isCorpKey.Value;

            APIKeyInfo apikey = new APIKeyInfo(keyID, vCode);
            apikey.Query();

            isCorpKey = (!String.IsNullOrEmpty(apikey.type) && String.Compare(apikey.type, "Corporation", StringComparison.OrdinalIgnoreCase) == 0);

            return isCorpKey.Value;
         }
      }

      public APIObject() { }

      public APIObject(string keyid, string vcode, string actorid)
      {
         keyID = keyid;
         vCode = vcode;
         actorID = actorid;
      }
   }
}
