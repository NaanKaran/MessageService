using System;
using Newtonsoft.Json;

namespace MessageService.Models.MMSModels
{
    public class MMSBalanceThreshold
    {
        [JsonProperty("settingId")]
        public string SettingId { get; set; }
        [JsonProperty("accountId")]
        public long AccountId { get; set; }
        [JsonProperty("thresholdCount")]
        public long ThresholdCount { get; set; }
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
        [JsonProperty("createdIP")]
        public string CreatedIP { get; set; }
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }
        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }
        [JsonProperty("modifiedIP")]
        public string ModifiedIP { get; set; }
        [JsonProperty("modifiedOn")]
        public DateTime? ModifiedOn { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
