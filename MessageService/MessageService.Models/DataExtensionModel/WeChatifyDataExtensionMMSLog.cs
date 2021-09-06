namespace MessageService.Models.DataExtensionModel
{
    public class WeChatifyDataExtensionMMSLog
    {
        public string SendId { get; set; }
        public string MobileNumber { get; set; }
        public string MMSTemplateId { get; set; }
        public string MMSTemplateName { get; set; }
        public string DynamicParamsValue { get; set; }
        public string AccountId { get; set; }
        public string SentStatus { get; set; }
        public string SendDate { get; set; } 
        public string DeliveryStatus { get; set; }
        public string DeliveryDate { get; set; }
        public string DropErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string InteractionId { get; set; }
        public string JourneyId { get; set; }
        public string JourneyName { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string VersionName { get; set; }
    }
}
