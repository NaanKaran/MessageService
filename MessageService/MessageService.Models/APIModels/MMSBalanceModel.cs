using System;
using System.Collections.Generic;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class MMSBalanceModel
    {
        [JsonProperty("balance")]
        public string Balance { get; set; }
        [JsonProperty("mmsbalance")]
        public int MMSBalance => Balance.IsNotNull() ? Convert.ToInt32((Convert.ToDouble(Balance) / 0.18)) : 0;
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("code")]
        public string ErrorCode { get; set; }
        [JsonProperty("msg")]
        public string ErrorMessage { get; set; }

        [JsonProperty("recharge_history")]
        public List<MMSTopupHistoryModel> RechargeHistory { get; set; } = new List<MMSTopupHistoryModel>();
    }

    public class MMSTopupHistoryModel
    {
        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("money_befor_recharge")]
        public decimal MoneyBeforeRecharge { get; set; }
        [JsonProperty("money_add")]
        public decimal MoneyAdd { get; set; }
        [JsonProperty("money_after_recharge")]
        public decimal MoneyAfterRecharge { get; set; }
        [JsonProperty("recharge_date")]
        public string RechargeDate { get; set; }

        public DateTime RechargeDateTime => Convert.ToDateTime(RechargeDate);
    }
}
