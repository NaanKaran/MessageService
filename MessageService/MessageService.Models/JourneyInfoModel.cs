using System;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models
{
    public class JourneyInfoModel
    {
       [JsonProperty("journeyid")]  public string JourneyId  { get; set; }
       [JsonProperty("journeykey")] public string JourneyKey { get; set; }
       [JsonProperty("journeyname")] public string JourneyName  { get; set; }
       [JsonProperty("initiateddate")] public DateTime? InitiatedDate { get; set; }
       [JsonProperty("initiateddatestring")] public string InitiatedDateString => InitiatedDate.IsNull() ? string.Empty : InitiatedDate.Value.ToDateTimeString();
       [JsonProperty("accountid")] public long AccountId  { get; set; }
       [JsonProperty("totalcount")] public int TotalCount  { get; set; }
       [JsonProperty("deliveredcount")] public int DeliveredCount { get; set; }
       [JsonProperty("droppedcount")] public int DroppedCount  { get; set; }
       [JsonProperty("sendfailedcount")] public int SendFailedCount { get; set; }
       [JsonProperty("lasttriggeredon")] public DateTime LastTriggeredOn { get; set; } = DateTime.UtcNow.ToChinaTime();
    }
}
