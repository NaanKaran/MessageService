using Newtonsoft.Json;
using System.Collections.Generic;

namespace MessageService.Models.APIModels
{
    public class JourneyFilterModel
    {
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("journeyid")]
        public string JourneyId { get; set; }
        [JsonProperty("journeyidlist")]
        public List<string> JourneyIdList { get; set; }
        [JsonProperty("pageno")]
        public int PageNo { get; set; }
        [JsonProperty("itemsperpage")]
        public int ItemsPerPage { get; set; }
    }
}
