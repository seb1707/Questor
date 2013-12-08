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
   // http://wiki.eveonline.com/en/wiki/EVE_API_EVE_Certificate_Tree
   public sealed class CertificateTree : APIObject
   {
      public CertificateTree() { }

      #region Overrides

      public override bool Query()
      {
         APIReader reader = new APIReader();
         return reader.Query(Uri, this, "");
      }

      public override string Uri { get { return BaseUrls.eveCertificateTreeUri; } }

      #endregion

      #region Nested Classes

      public sealed class RequiredSkill
      {
         public RequiredSkill() { }

         public int typeID { get; set; }
         public int level { get; set; }
      }

      public sealed class Certificate
      {
         public Certificate() { }

         public int certificateID { get; set; }
         public int grade { get; set; }
         public Int64 corporationID { get; set; }
         public string description { get; set; }

         private List<RequiredSkill> requiredSkills_ = new List<RequiredSkill>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<RequiredSkill> requiredSkills
         {
            get { return requiredSkills_; }
            set { requiredSkills_ = value; }
         }

         private List<Certificate> requiredCertificates_ = new List<Certificate>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Certificate> requiredCertificates
         {
            get { return requiredCertificates_; }
            set { requiredCertificates_ = value; }
         }
      }

      public sealed class CertificateClass
      {
         public CertificateClass() { }

         public int classID { get; set; }
         public string className { get; set; }

         private List<Certificate> certificates_ = new List<Certificate>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<Certificate> certificates
         {
            get { return certificates_; }
            set { certificates_ = value; }
         }
      }

      public sealed class Category
      {
         public Category() { }

         public int categoryID { get; set; }
         public string categoryName { get; set; }

         private List<CertificateClass> classes_ = new List<CertificateClass>();

         [TypeConverter(typeof(ExpandableObjectConverter))]
         public List<CertificateClass> classes
         {
            get { return classes_; }
            set { classes_ = value; }
         }
      }

      #endregion

      private List<Category> categories_ = new List<Category>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Category> categories
      {
         get { return categories_; }
         set { categories_ = value; }
      }
   }
}
