using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class MMSJourneyInfoViewModel
    {
        [JsonProperty("piechartinfo")] public JourneysPieChart PieChartInfo { get; set; }
        [JsonProperty("journieslist")] public PagingModel<JourneyInfoModel> JourneysInfo { get; set; }
    }

    public class JourneysPieChart
    {
        [JsonProperty("totalcount")] public int TotalCount { get; set; }
        [JsonProperty("deliveredcount")] public int DeliveredCount { get; set; }
        [JsonProperty("droppedcount")] public int DroppedCount { get; set; }
        [JsonProperty("sendfailedcount")] public int SendFailedCount { get; set; }
        [JsonIgnore] public List<JourneyInfoModel> JourneyInfoModels { get; set; } = new List<JourneyInfoModel>();
    }
}
