using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class FileUploadModel
    {
        [Required]
        [JsonProperty("file")]
        public IFormFile File { get; set; }
        [Required]
        [JsonProperty("accountid")]
        public long AccountId { get; set; }        
    }
}
