using System;
using MessageService.Models.Enum;

namespace MessageService.Models.MMSModels
{
    public class MMSDeliveryReportNotification
    {
        public string Id { get; set; }
        public long AccountId { get; set; }
        public Int16 RunBy { get; set; }
        public Int16? RunDay { get; set; }
        public Int16 RunOnTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Percentage { get; set; }
    }

    public class FilterParam
    {
        public long accountId { get; set; }
        public int? pageNumber { get; set; }
        public int? recordPerPage { get; set; }
        public string sortBy { get; set; }
        public string sortType { get; set; }
        public NotificationType Type { get; set; }
    }

}
