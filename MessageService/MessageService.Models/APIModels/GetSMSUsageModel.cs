using MessageService.Models.CosmosModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.APIModels
{
    public class SMSUsageModel
    {
        [JsonProperty("smsusage")]
        public List<SMSUsageDocumentModel> SMSUsage { get; set; }
        [JsonProperty("years")]
        public List<int> Years { get; set; }
    }
}
