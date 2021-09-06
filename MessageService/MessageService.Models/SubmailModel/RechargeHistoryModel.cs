using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class RechargeHistoryModel
    {

        [JsonProperty("orderNumber")] public string OrderNumber { get; set; }
        [JsonProperty("email_befor_recharge")] public string EmailBeforeRecharge { get; set; }
        [JsonProperty("email_add_credits")] public string EmailAddCredits { get; set; }
        [JsonProperty("email_after_recharge")] public string EmailAfterRecharge { get; set; }
        [JsonProperty("sms_befor_recharge")] public string SmsBeforeRecharge { get; set; }
        [JsonProperty("sms_add_credits")] public string SmsAddCredits { get; set; }
        [JsonProperty("sms_after_recharge")] public string SmsAfterRecharge { get; set; }
        [JsonProperty("transactional_sms_befor_recharge")] public string TransactionalSmsBeforeRecharge { get; set; }
        [JsonProperty("transactional_sms_add_credits")] public string TransactionalSmsAddCredits { get; set; }
        [JsonProperty("transactional_sms_after_recharge")] public string TransactionalSmsAfterRecharge { get; set; }
        [JsonProperty("money_befor_recharge")] public string MoneyBeforeRecharge { get; set; }
        [JsonProperty("money_add")] public string MoneyAdd { get; set; }
        [JsonProperty("money_after_recharge")] public string MoneyAfterRecharge { get; set; }
        [JsonProperty("recharge_date")] public string RechargeDate { get; set; }
        [JsonProperty("recharge_date_time")] public DateTime RechargeDateTime => Convert.ToDateTime(RechargeDate);

    }
}
