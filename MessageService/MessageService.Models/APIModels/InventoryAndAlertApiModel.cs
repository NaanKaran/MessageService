using System;
using System.Collections.Generic;
using System.Text;
using MessageService.Models.CosmosModel;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class InventoryAndAlertApiModel
    {
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("userdetails")]public List<EmailNotificationUserDocumentModel> UserDetails  { get; set; }
    }
}
