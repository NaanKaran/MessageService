using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;
using System;

namespace MessageService.Models.APIModels
{
    public class SMSBalanceModel
    {
        [JsonProperty("balance")]
        public string Balance { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("code")]
        public string ErrorCode { get; set; }
        [JsonProperty("msg")]
        public string ErrorMessage { get; set; }
    }
}
