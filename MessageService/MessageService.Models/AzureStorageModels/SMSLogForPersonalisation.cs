using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.AzureStorageModels
{
    public class SMSLogForPersonalisation : TableEntity
    {
        public SMSLogForPersonalisation(string interactionId, string accountId)
        {
            JourneyId = interactionId;
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = accountId;
        }
        public string Payload { get; set; }
        public string JourneyId { get; set; }
        public string APIResponse { get; set; }
        public bool Status { get; set; }
    }
}
