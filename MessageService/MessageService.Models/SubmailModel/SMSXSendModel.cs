using System.Collections.Generic;
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class SMSXSendModel : BaseSubmailModel
    {
        [JsonProperty("to")] public string To { get; set; }
        [JsonProperty("project")] public string Project { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("vars")] public Dictionary<string, string> Variables { get; set; }

    }
}
