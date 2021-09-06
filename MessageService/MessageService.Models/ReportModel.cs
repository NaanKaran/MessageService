using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models
{
    public class ReportModel
    {
        public string AccountName { get; set; }
        public string ActionName { get; set; }
        public string CampaignsRunDay { get; set; }
        public int? TotalSentMMS { get; set; }
        public int? Delivered { get; set; }
        public int? Dropped { get; set; }
        public int? Pending { get; set; }
       // public int? Invalid { get; set; }
      //  public int? UnSubscribed { get; set; }
        public int? Unconfirmed { get; set; }
        public int? SendFailed { get; set; }
        public string DeliveredPercentage { get; set; }
        public string DroppedPercentage { get; set; }
        public string OthersPercentage { get; set; }
        public long AccountId { get; set; }

    }

    public class DropReasonModel
    {
        public string ActionName { get; set; }
        public string MobileNumber { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public DateTime SendDate { get; set; }
    }

}
