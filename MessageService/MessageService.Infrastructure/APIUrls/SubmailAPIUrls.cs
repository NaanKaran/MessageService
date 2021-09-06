using MessageService.InfraStructure.Helpers;

namespace MessageService.InfraStructure.APIUrls
{
    public static class SubmailAPIUrls
    {
        public static string SubMailBaseDomainUrl = AppSettings.GetValue("SubmailAPIBaseUrl");
        public static string MMSXSendUrl => SubMailBaseDomainUrl + "mms/xsend";
        public static string MMSMultiXSend => SubMailBaseDomainUrl + "mms/multixsend";
        public static string MMSBalanceUrl => SubMailBaseDomainUrl + "balance/mms";
        public static string MMSTemplateUrl => SubMailBaseDomainUrl + "mms/template";
        public static string MMSTopupHistoryUrl => SubMailBaseDomainUrl + "balance/rechargehistory_for_mms";
        public static string SMSBalanceUrl => SubMailBaseDomainUrl + "balance/sms";
        public static string SMSSendUrl => SubMailBaseDomainUrl + "message/send";
        public static string SMSXSendUrl => SubMailBaseDomainUrl + "message/xsend";
        public static string SMSRechargeHistoryUrl => SubMailBaseDomainUrl + "balance/rechargehistory";

        //This endpoint is just for testing without hitting Submail's SMS send URL
        public static string EmulateSMSXSendUrl = "https://stage-wechatify-sms-httptrigger-funcapp.azurewebsites.net/api/EmulateSend";
    }
}
