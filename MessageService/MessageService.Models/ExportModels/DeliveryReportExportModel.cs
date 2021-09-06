using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MessageService.Models.ExportModels
{
    public class DeliveryReportExportModel
    {

       // [DisplayName("Journey Name")] public string JourneyName { get; set; }
        [DisplayName("Account Name")] public string AccountName { get; set; }
        [DisplayName("Action Name")] public string ActionName { get; set; }
        [DisplayName("Campaigns Run Day")] public string CampaignsRunDay { get; set; }
        [DisplayName("Total SMS Sent")] public int? TotalSMSSent { get; set; }
        [DisplayName("Delivered")] public int? Delivered { get; set; }
        [DisplayName("Dropped")] public int? Dropped { get; set; }
        [DisplayName("Pending")] public int? Pending { get; set; }
        [DisplayName("SendFailed")] public int? SendFailed { get; set; }
        [DisplayName("UnConfirmed")] public int? UnConfirmed { get; set; }
        [DisplayName("Delivered Percentage")] public string DeliveredPercentage { get; set; }
        [DisplayName("Dropped Percentage")] public string DroppedPercentage { get; set; }
        [DisplayName("Others Percentage")] public string OthersPercentage { get; set; }
    }
}
