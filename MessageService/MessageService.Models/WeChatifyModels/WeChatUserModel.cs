using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.WeChatifyModels
{
    public class WeChatUserModel
    {
        [JsonProperty("openid")]
        public string OpenId { get; set; }
        [JsonProperty("nickname")]
        public string NickName { get; set; }
        [JsonProperty("headimgurl")]
        public string HeadImgUrl { get; set; }
        //[JsonProperty("sex")]
        //public GenderType Sex { get; set; }
        //[JsonProperty("gender")]
        //public string Gender => Sex.ToString();
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("unionid")]
        public string UnionId { get; set; }
        [JsonProperty("province")]
        public string Province { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
    }
}
