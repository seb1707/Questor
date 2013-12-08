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
using System.IO;
using System.Net;
using System.Globalization;

namespace EVE.Net
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Misc_ID_to_Graphic
   public class ImageServer : APIObject
   {
      public enum ImageType { Character, Corporation, Alliance, Inventory, Render };
      private ImageType imageType_ = ImageType.Character;
      private int imageSize_ = 64;

      public ImageServer() { }

      public ImageServer(ImageType imageType, int imageSize, string actorid)
      {
         imageType_ = imageType;
         imageSize_ = imageSize;

         actorID = actorid;
      }

      #region Overrides

      public override bool Query()
      {
         ValidateRequestedSize();

         string fileName = FileName;

         if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         if (File.Exists(fileName) && (DateTime.Now - File.GetCreationTime(fileName)).TotalHours < 1)
            return true; // already done

         try
         {
             WebRequest req = WebRequest.Create(Uri);

             using (WebResponse response = req.GetResponse())
             {
                 using (Stream imageStream = response.GetResponseStream())
                 {
                     using (Stream outputStream = File.OpenWrite(fileName))
                     {
                         byte[] buffer = new byte[8 * 1024];
                         int len;

                         while ((len = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                             outputStream.Write(buffer, 0, len);
                     }
                 }
             }
         }
         catch (Exception e)
         {
             if (Settings.FailGracefully)
             {
                 this.error = new APIError(0, e.Message);
                 return false;
             }
             else
             {
                 this.error = new APIError(0, e.Message);
                 throw e;
             }
         }
         return true;
      }

      public override string Uri 
      { 
         get 
         {
            return string.Format(CultureInfo.InvariantCulture, @"http://image.eveonline.com/{0}/{1}_{2}.{3}", Enum.GetName(typeof(ImageType), imageType_), actorID, imageSize_, FileExtension);
         } 
      }

      #endregion

      private void ValidateRequestedSize()
      {
         int[] validSizes = null;

         switch (imageType_)
         {
            case ImageType.Character:
               validSizes = new int[] { 30, 32, 64, 128, 200, 256, 512, 1024 };
               break;
            case ImageType.Corporation:
               validSizes = new int[] { 30, 32, 64, 128, 256 };
               break;
            case ImageType.Alliance:
               validSizes = new int[] { 30, 32, 64, 128 };
               break;
            case ImageType.Inventory:
               validSizes = new int[] { 32, 64 };
               break;
            case ImageType.Render:
               validSizes = new int[] { 32, 64, 128, 256, 512 };
               break;
         }

         if (Array.IndexOf(validSizes, imageSize_) == -1)
         {
            throw new ArgumentException("An invalid image size was requested for this image type.  Valid sizes for this image type are: "
               + string.Join(",", Array.ConvertAll(validSizes, x => x.ToString(CultureInfo.CurrentCulture))));
         }
      }

      private string FileExtension
      {
         get
         {
            return (imageType_ == ImageType.Character) ? "jpg" : "png";
         }
      }

      public string FileName
      {
         get
         {
            return Path.Combine(Settings.ImagesFolder, string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}.{3}", Enum.GetName(typeof(ImageType), imageType_), actorID, imageSize_, FileExtension));
         }
      }
   }
}
