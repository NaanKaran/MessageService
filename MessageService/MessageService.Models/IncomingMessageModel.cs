using System;
using System.Collections.Generic;
using System.Linq;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models
{
    public class IncomingMessageModel
    {
        [JsonProperty("id")] public Guid Id { get; set; } = Guid.NewGuid();
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("mobilenumber")]  public string MobileNumber { get; set; }
        [JsonProperty("isoptout")] public bool IsOptOut => OptOutCharacters.Contains(Content?.Trim().ToUpper() ?? "");
        [JsonProperty("journeyname")] public string JourneyName { get; set; }
        [JsonProperty("createdon")]  public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("createdonstring")] public string CreatedOnString => CreatedOn.ToDateTimeString();

        private static IEnumerable<string> OptOutCharacters => new [] { "T", "TD","N" };
    }
}
