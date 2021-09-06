using System;
using MessageService.Models.Enum;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Models.AzureStorageModels
{
    public class MMSFailedQueueLog : TableEntity
    {
        public MMSFailedQueueLog()
        {
            
        }
        public MMSFailedQueueLog(string failedQueueType) //MMSFailedQueueType
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = failedQueueType;
        }
        public string AccountId { get; set; }
        public MMSFailedQueueType Type { get; set; }
        public string Data { get; set; }
        public string StackTrace { get; set; }
        public string ErrorMessage { get; set; }
    }
}
