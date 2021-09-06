using Newtonsoft.Json;

namespace MessageService.Models
{
    public class JourneyActivateModel
    {
        [JsonProperty("originalDefinitionId")] public string JourneyId { get; set; }
        [JsonProperty("interactionId")] public string InteractionId { get; set; }
        [JsonProperty("activityObjectID")] public string ActivityId { get; set; }
        [JsonProperty("interactionKey")] public string InteractionKey { get; set; }
        [JsonProperty("interactionVersion")] public string InteractionVersion { get; set; }
        [JsonProperty("organizationId")] public string OrganizationId { get; set; }
        [JsonProperty("enterpriseId")] public string EnterPriseId { get; set; }
        [JsonProperty("userId")] public string UserId { get; set; }
        [JsonProperty("restEndPoint")] public string RestEndPoint { get; set; }
        [JsonProperty("accountId")] public string AccountId { get; set; }
    }
}
