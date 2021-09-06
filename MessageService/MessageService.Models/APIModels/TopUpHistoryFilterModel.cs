using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class TopUpHistoryFilterModel
    {
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("pageno")] public int PageNo { get; set; }
        [JsonProperty("itemsperpage")] public int ItemsPerPage { get; set; }
    }
}
