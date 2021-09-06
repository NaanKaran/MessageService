using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.MigrationModel;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Repository.Interface
{
    public interface IAzureStorageRepository
    {
        Task<string> UploadStreamToBlobAsync(Stream stream, long accountId, string fileName,
            string folderName = "MMS");
        Task AddQueueAsync(string queueData, string queueName);
        Task AddDeferredQueueAsync(string queueData, string queueName, TimeSpan initialVisibilityDelay);
        Task<bool> InsertIntoTableAsync<T>(T insertObj, string tableName) where T : ITableEntity;
        Task<bool> UpsertIntoTableAsync<T>(T insertObj, string tableName) where T : ITableEntity;

        Task<SalesForceAuthenticationModel> GetSalesForceAuthenticationAsync(string tableName, string filterColumnName,
            string filterConditionValue,
            string queryCompare = QueryComparisons.Equal);

        Task<SMSSendStatusTrack> GetSMSSendStatusAsync(string tableName, string filterColumnName,
            string filterConditionValue,
            string queryCompare = QueryComparisons.Equal);
        Task<PIIDetails> GetPIIDetailsAsync(string partition, string tableName, string filterColumnName, string openId);

        Task<IList<T>> GetQueuesAsync<T>(string queueName);
        Task<IList<string>> GetQueuesAsync(string queueName);

        Task<string> GetBlobFileAsBase64Async(long accountId, string fileName,
            string folderName = "MMS");

        Task<IList<JourneyInfoAzureModel>> GetJourneyTableEntitysAsync(string tableName, string filters);
        Task<IList<InteractionInfoAzureModel>> GetInteractionTableEntitysAsync(string tableName, string filters);
        Task<IList<ActivityInfoAzureModel>> GetActivityTableEntitysAsync(string tableName, string filters);
        Task<IList<SMSLogAzureModel>> GetSmsLogTableEntitysAsync(string tableName, string filters);

    }
}
