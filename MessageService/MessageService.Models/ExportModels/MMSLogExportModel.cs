using System.ComponentModel;

namespace MessageService.Models.ExportModels
{
    public class MMSLogExportModel
    {
        [DisplayName("Journey Name")] public string JourneyName { get; set; }
        [DisplayName("Versions")] public string Version  { get; set; }
        [DisplayName("Activity Name")] public string ActivityName { get; set; } 
        [DisplayName("Mobile Number")] public string MobileNumber { get; set; } 
        [DisplayName("MMSTemplate Name")] public string MMSTemplateName { get; set; }
        [DisplayName("Sent Status")] public string SentStatus { get; set; }
        [DisplayName("Send Date")] public string SendDate { get; set; }
        [DisplayName("Delivery Status")] public string DeliveryStatus { get; set; }
        [DisplayName("Dropped/SendFaild Reason")] public string DroppedReason { get; set; }
        [DisplayName("Delivery Date")] public string DeliveryDate { get; set; }
        [DisplayName("Quadrants")] public string Quadrants { get; set; }
    }
}
