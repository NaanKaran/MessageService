using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;

namespace MessageService.Models.MMSModels
{
    public class MMSLogModel
    {
        public string SendId { get; set; } = Guid.NewGuid().ToString("N");
        public string MobileNumber { get; set; }  
        public string MMSTemplateId { get; set; }
        public string DynamicParamsValue { get; set; }
        public long AccountId { get; set; }
        public SendStatus SentStatus { get; set; }
        public DateTime SendDate { get; set; } = DateTime.UtcNow.ToChinaTime();
        public DeliveryStatus? DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DropErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string InteractionId { get; set; }
        public string JourneyId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityInteractionId { get; set; }
        public string OrgId { get; set; }
    }
}
