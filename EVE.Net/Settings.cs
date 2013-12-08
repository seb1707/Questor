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

namespace EVE.Net
{
   public static class Settings
   {
      private static string cacheFolder_ = "Cache";
      private static string imagesFolder_ = "Images";
      private static string apiUri_ = EVEConstants.tranquilityAPIUri;
      private static bool failGracefully_ = true;


      /// <summary>
      /// The BASE web address of the API. Both the tranquility (live) and
      /// singularity (test) server base addresses are listed in EVEConstants
      /// </summary>
      public static string APIUri
      {
         get { return Settings.apiUri_; }
         set { Settings.apiUri_ = value; }
      }

      /// <summary>
      /// The folder to store cache files in.
      /// </summary>
      public static string CacheFolder
      {
         get { return Settings.cacheFolder_; }
         set { Settings.cacheFolder_ = value; }
      }

      /// <summary>
      /// The folder to store image files in.
      /// </summary>
      public static string ImagesFolder
      {
         get { return Settings.imagesFolder_; }
         set { Settings.imagesFolder_ = value; }
      }

       /// <summary>
       /// Determines whether the library should fail gracefully (return false on Query()) or hard. (throw exception)
       /// The end result for the object is the same.
       /// </summary>
      public static bool FailGracefully
      {
          get
          {
              return Settings.failGracefully_;
          }
          set
          {
              Settings.failGracefully_ = value;
          }
      }
   }
}
