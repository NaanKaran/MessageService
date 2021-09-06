using System;
using System.Collections.Generic;
using System.Text;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.MMSModels
{
    public class MMSUsageModel
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("month")] public Months Month { get; set; }
        [JsonProperty("monthstring")] public string MonthString => Month.ToString();
        [JsonProperty("year")] public int Year { get; set; }
        [JsonProperty("rechargecount")] public int RechargeCount { get; set; }
        [JsonProperty("usedcount")] public int UsedCount { get; set; }
        [JsonProperty("balance")] public int Balance { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
    }


    public class MMSUsageViewModel
    {
        [JsonProperty("years")] public List<int> Years { get; set; }
        [JsonProperty("mmsusage")] public List<MMSUsageModel> MmsUsage { get; set; }
    }
}
