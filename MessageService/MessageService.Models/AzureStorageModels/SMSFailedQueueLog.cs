using System;
using MessageService.Models.Enum;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Models.AzureStorageModels
{
    public class SMSFailedQueueLog : TableEntity
    {
        public SMSFailedQueueLog()
        {
            
        }
        public SMSFailedQueueLog(string failedQueueType) //SMSFailedQueueType
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = failedQueueType;
        }
        public string AccountId { get; set; }
        public SMSFailedQueueType Type { get; set; }
        public string Data { get; set; }
        public string StackTrace { get; set; }
        public string ErrorMessage { get; set; }
    }
}
