using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class SubmailResponseModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("send_id")]
        public string SendId { get; set; }
        [JsonProperty("fee")]
        public string Fee { get; set; }
        [JsonProperty("money_account")]
        public string AccountBalance { get; set; }
        [JsonProperty("msg")]
        public string ErrorMessage { get; set; }
        [JsonProperty("code")]
        public string ErrorCode { get; set; }

        public SendStatus SentStatus => (Status == "success") ? SendStatus.Success : SendStatus.Failed;
    }
}
