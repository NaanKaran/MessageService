using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class LogGridModel
    {
       [JsonProperty("versionddl")] public IEnumerable<VersionDropDownModel> VersionDdl { get; set; } = new List<VersionDropDownModel>();
       [JsonProperty("activityddl")] public IEnumerable<ActivityDropDownModel> ActivityDdl { get; set; } = new List<ActivityDropDownModel>();
       [JsonProperty("quadrantddl")] public IEnumerable<QuadrantDropDownModel> QuadrantDdl { get; set; } = new List<QuadrantDropDownModel>();
       [JsonProperty("logviewmodels")] public PagingModel<LogViewModel> LogViewModels { get; set; } = new PagingModel<LogViewModel>();

    }

    public class VersionDropDownModel
    {
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [IgnoreDataMember]
        public string Version { get; set; }
        [JsonProperty("version")] public string VersionName => $"Version {Version}";
    }

    public class ActivityDropDownModel
    {
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [JsonProperty("activityid")] public string ActivityId { get; set; }
        [JsonProperty("activityname")] public string ActivityName { get; set; }
    }

    public class QuadrantDropDownModel
    {
        [JsonProperty("tablename")] public string TableName { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }
}
