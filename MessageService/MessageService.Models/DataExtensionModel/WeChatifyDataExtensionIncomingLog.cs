namespace MessageService.Models.DataExtensionModel
{
    public class WeChatifyDataExtensionIncomingLog
    {
        public string Id { get; set; } 
        public string AccountId { get; set; }
        public string Content { get; set; }
        public string MobileNumber { get; set; }
        public string IsOptOut { get; set; }
        public string JourneyName { get; set; }
        public string CreatedOn { get; set; }
    }
}
