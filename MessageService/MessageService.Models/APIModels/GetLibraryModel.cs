using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class GetLibraryModel
    {
        [Required]
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("types")]
        public List<string> Types { get; set; }
        [JsonProperty("filename")]
        public string Filename { get; set; }
        [JsonProperty("pageno")]
        public int PageNo { get; set; }
        [JsonProperty("itemsperpage")]
        public int ItemsPerPage { get; set; }
    }
}
