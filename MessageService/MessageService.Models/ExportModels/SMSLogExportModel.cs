using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MessageService.Models.ExportModels
{
    public class SMSLogExportModel
    {
        [DisplayName("Journey Name")] public string JourneyName { get; set; }
        [DisplayName("Versions")] public string Version { get; set; }
        [DisplayName("Activity Name")] public string ActivityName { get; set; }
        [DisplayName("Mobile Number")] public string MobileNumber { get; set; }
        [DisplayName("SMS Content")] public string SMSContent { get; set; }
        [DisplayName("Sent Status")] public string SentStatus { get; set; }
        [DisplayName("Send Date")] public string SendDate { get; set; }
        [DisplayName("Delivery Status")] public string DeliveryStatus { get; set; }
        [DisplayName("Dropped/Send Failed Reason")] public string DroppedReason { get; set; }
        [DisplayName("Delivery Date")] public string DeliveryDate { get; set; }
        
    }
}
