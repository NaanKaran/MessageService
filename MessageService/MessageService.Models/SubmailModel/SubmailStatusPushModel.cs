using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class SubmailStatusPushModel
    {
        [JsonProperty("events")]
        public string Event { get; set; }

        [JsonProperty("address")]
        public string MobileNumber { get; set; }

        [JsonProperty("app")]
        public string AppId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("signature")]
        public string AppKey { get; set; }

        [JsonProperty("send_id")]
        public string SendId { get; set; }

        [JsonProperty("report")]
        public string DropCode { get; set; }

        [JsonProperty("template_id")]
        public string TemplateId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reason")]
        public string TemplateRejectReason { get; set; }

        public SubmailEventType EventType => Event.ToEnum<SubmailEventType>();

        public DateTime EventDateTime => Timestamp.ToDateTime();

        public DeliveryStatus DeliveryStatus {
            get
            {
                switch (EventType)
                {
                    case SubmailEventType.delivered:
                        return DeliveryStatus.Delivered;
                    case SubmailEventType.dropped:
                        return DeliveryStatus.Dropped;
                    case SubmailEventType.unknown:
                        return DeliveryStatus.UnConfirmed;
                    default:
                        return DeliveryStatus.UnConfirmed;
                }
            }
        }
    }
}
