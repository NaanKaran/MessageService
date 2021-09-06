using Newtonsoft.Json;

namespace MessageService.Models.WeChatifyModels
{
    public class WeChatifyUserModel
    {      
        [JsonProperty("userid")]
        public string UserId { get; set; }
        [JsonProperty("accountid")]
        public long AccountId { get; set; }      
        [JsonProperty("name")]
        public string Name => string.IsNullOrEmpty(LastName) ? FirstName : FirstName + " " + LastName;
        [JsonProperty("firstname")]
        public string FirstName { get; set; }
        [JsonProperty("lastname")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("rolename")]
        public string RoleName { get; set; }

    }
}
