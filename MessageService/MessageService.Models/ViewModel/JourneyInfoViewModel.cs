using System.Collections.Generic;
using System.Linq;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using Newtonsoft.Json;

namespace MessageService.Models.ViewModel
{    
    public class JourneyInfoViewModel
    {
        [JsonProperty("piechartinfo")] public JourneyPieChart PieChartInfo { get; set; }
        [JsonProperty("journieslist")] public PagingModel<JourneyInfoDocumentModel> JourneysInfo { get; set; }
    }

    public class JourneyPieChart
    {
        [JsonProperty("totalcount")] public int TotalCount => JourneyInfoModels.Sum(x=>x.TotalCount);
        [JsonProperty("deliveredcount")] public int DeliveredCount => JourneyInfoModels.Sum(x => x.DeliveredCount);
        [JsonProperty("droppedcount")] public int DroppedCount => JourneyInfoModels.Sum(x => x.DroppedCount);
        [JsonProperty("sendfailedcount")] public int SendFailedCount => JourneyInfoModels.Sum(x => x.SendFailedCount);
        [JsonIgnore] public List<JourneyInfoDocumentModel> JourneyInfoModels { get; set; } = new List<JourneyInfoDocumentModel>();
    }
}
