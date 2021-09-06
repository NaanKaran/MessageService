using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.SMSModels
{
    public class SMSSFInteractionModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("accountid")]
        public long AccountId { get; set; }     
    }
}
