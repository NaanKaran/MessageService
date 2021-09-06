using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class APIResponseModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("statuscode")]
        public int StatusCode { get; set; }
        [JsonProperty("result")]
        public object Result { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("errmsg")]
        public object ErrorMessage { get; set; }
    }
}
