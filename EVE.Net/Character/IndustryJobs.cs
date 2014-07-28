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

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Industry_Jobs
   public sealed class IndustryJobs : APIObject
   {
      public IndustryJobs() { }

      public IndustryJobs(string keyid, string vcode, string actorid)
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
            return IsCorpKey ? BaseUrls.corpIndustryJobsUri : BaseUrls.charIndustryJobsUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Job
      {
         public Job() { }

         public int jobID { get; set; }
         public int assemblyLineID  { get; set; }
         public Int64 containerID  { get; set; }
         public Int64 installedItemID { get; set; }
         public Int64 installedItemLocationID { get; set; }
         public int installedItemQuantity { get; set; }
         public int installedItemProductivityLevel { get; set; }
         public int installedItemMaterialLevel { get; set; }
         public int installedItemLicensedProductionRunsRemaining { get; set; }
         public int outputLocationID { get; set; }
         public Int64 installerID { get; set; }
         public int runs { get; set; }
         public int licensedProductionRuns { get; set; }
         public Int64 installedInSolarSystemID { get; set; }
         public Int64 containerLocationID { get; set; }
         public float materialMultiplier { get; set; }
         public float charMaterialMultiplier { get; set; }
         public float timeMultiplier { get; set; }
         public float charTimeMultiplier { get; set; }
         public Int64 installedItemTypeID { get; set; }
         public Int64 outputTypeID { get; set; }
         public Int64 containerTypeID { get; set; }
         public int installedItemCopy { get; set; }
         public int completed { get; set; }
         public int completedSuccessfully { get; set; }
         public int installedItemFlag { get; set; }
         public int outputFlag { get; set; }
         public int activityID { get; set; }
         public int completedStatus { get; set; }
         public DateTime installTime { get; set; }
         public DateTime beginProductionTime { get; set; }
         public DateTime endProductionTime { get; set; }
         public DateTime pauseProductionTime { get; set; }
      }

      #endregion

      private List<Job> jobs_ = new List<Job>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Job> jobs
      {
         get { return jobs_; }
         set { jobs_ = value; }
      }
   }
}
