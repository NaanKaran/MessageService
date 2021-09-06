using System;
using MessageService.InfraStructure.Helpers;

namespace MessageService.Models.MMSModels
{
    public class MMSActivityInfoModel
    {
        public string ActivityId { get; set; }
        public string InteractionId { get; set; }
        public string JourneyId { get; set; }
        public string QuadrantInfo { get; set; }
        public string ActivityName { get; set; }
        public string JourneyName { get; set; }
        public DateTime InitiatedDate { get; set; }
        public long AccountId { get; set; }
        public int TotalCount { get; set; }
        public int DeliveredCount { get; set; }
        public int DroppedCount { get; set; }
        public int SendFailedCount { get; set; }        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        public DateTime LastTriggeredOn { get; set; }

    }
}
