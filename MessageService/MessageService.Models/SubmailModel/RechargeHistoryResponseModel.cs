using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class RechargeHistoryResponseModel
    {
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("recharge_history")] public List<RechargeHistoryModel> RechargeHistories { get; set; } = new List<RechargeHistoryModel>();
        [JsonProperty("code")] public string Code { get; set; }
        [JsonProperty("msg")] public string ErrorMessage { get; set; }
    }
}
