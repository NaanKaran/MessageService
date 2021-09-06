using System;
using MessageService.InfraStructure.Helpers;

namespace MessageService.Models.MMSModels
{
    public class MMSInteractionInfoModel
    {
        public string InteractionId { get; set; }
        public string JourneyId  { get; set; }
        public string QuadrantInfo { get; set; }
        public string Version { get; set; }
        //public string VersionName { get; set; }
        public DateTime? PublishedDate { get; set; }
        public long AccountId { get; set; }
        public int TotalCount  { get; set; }
        public int DeliveredCount { get; set; }
        public int DroppedCount  { get; set; }
        public int SendFailedCount { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
    }
}
