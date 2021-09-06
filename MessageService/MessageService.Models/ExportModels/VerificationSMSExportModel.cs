using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.ExportModels
{  
    public class VerificationSMSExportModel : EmailExportModel
    {
        //[JsonProperty("mobilenumber")]
        //public string MobileNumber { get; set; }      
        //[JsonProperty("smscontent")]
        //public string SMSContent { get; set; }

        [JsonProperty("followername")]
        public string FollowerName { get; set; }
    }
}
