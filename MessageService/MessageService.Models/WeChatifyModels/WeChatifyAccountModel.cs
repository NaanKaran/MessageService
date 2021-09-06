using Newtonsoft.Json;

namespace MessageService.Models.WeChatifyModels
{
    public class WeChatifyAccountModel
    {
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("accountname")]
        public string AccountName { get; set; }
    }
}
