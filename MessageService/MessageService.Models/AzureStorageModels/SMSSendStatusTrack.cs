using MessageService.Models.Enum;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.AzureStorageModels
{
    public class SMSSendStatusTrack : TableEntity
    {
        public SMSSendStatusTrack()
        {          
        }
        public SMSSendStatusTrack(string correlationId, string accountId)
        {
            RowKey = correlationId;
            PartitionKey = accountId;
        }
        public int Step { get; set; }
        public string Content { get; set; }
        public int RetryCount { get; set; } = 0;
        public string Payload { get; set; }
        public string SubmailPayload { get; set; }
        public string SubmailResponse { get; set; }
        public string CosmosLogPayload { get; set; }
        public string StackTrace { get; set; }
        public string ErrorMessage { get; set; }
    }
}
