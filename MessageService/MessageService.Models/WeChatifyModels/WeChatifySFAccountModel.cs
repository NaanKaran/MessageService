using Newtonsoft.Json;

namespace MessageService.Models.WeChatifyModels
{
    public class WeChatifySFAccountModel
    {
        [JsonProperty("Id")] public long AccountId { get; set; }
        [JsonProperty("Name")] public string AccountName { get; set; }
         public string WeChatId { get; set; }
        [JsonIgnore] public bool LoadWithSpecificBU { get; set; }
        [JsonProperty("IncludeBUSelection")] public bool IncludeBUSelection => !LoadWithSpecificBU;
        [JsonIgnore] public string UserId { get; set; }
        [JsonIgnore] public string OrgId { get; set; }
        [JsonIgnore] public string OrganizationId => OrgId?.Split('@')[0];
        [JsonIgnore] public bool IsSharedDEConfigured { get; set; }
        [JsonIgnore] public bool IsEnterpriseAccount { get; set; }
        [JsonIgnore] public string ParentOrgId { get; set; }

    }
}
