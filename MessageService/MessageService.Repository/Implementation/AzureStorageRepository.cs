using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.MigrationModel;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Repository.Implementation
{
    public class AzureStorageRepository : AzureTableStorageRepository, IAzureStorageRepository
    {
        public string AzureMessagingConnectionString => AppSettings.GetValue("ConnectionStrings:AzureConnection");
        public string AzurePIIConnectionString => AppSettings.GetValue("ConnectionStrings:AzurePIIConnection");
        private readonly CloudBlobClient _blobClient;
        private readonly CloudQueueClient _queueClient;
        public AzureStorageRepository()
        {
            // Create the blob & queue client.
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(AzureMessagingConnectionString);
            _blobClient = cloudStorageAccount.CreateCloudBlobClient();
            _queueClient = cloudStorageAccount.CreateCloudQueueClient();

        }

        public async Task<string> UploadStreamToBlobAsync(Stream stream, long accountId, string fileName,
             string folderName = "MMS")
        {
            try
            {
                CloudBlobContainer blobContainer = await GetAccountBlobContainer(accountId);
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference((folderName.IsNotNullOrWhiteSpace() ? folderName + "/" : "") + fileName);
                await blob.DeleteIfExistsAsync();
                await blob.UploadFromStreamAsync(stream);
                return blob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetBlobFileAsBase64Async(long accountId, string fileName,
            string folderName = "MMS")
        {
            try
            {
                CloudBlobContainer blobContainer = await GetAccountBlobContainer(accountId);
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference((folderName.IsNotNullOrWhiteSpace() ? folderName + "/" : "") + fileName);
                MemoryStream memStream = new MemoryStream();
                await blob.DownloadToStreamAsync(memStream);
                string base64 = Convert.ToBase64String(memStream.ReadToEnd());
                return base64;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task AddQueueAsync(string queueData, string queueName)
        {
            CloudQueue queue = _queueClient.GetQueueReference(queueName);
            CloudQueueMessage message = new CloudQueueMessage(queueData);
            await queue.CreateIfNotExistsAsync();
            await queue.AddMessageAsync(message);
        }
        public async Task AddDeferredQueueAsync(string queueData, string queueName, TimeSpan initialVisibilityDelay)
        {
            CloudQueue queue = _queueClient.GetQueueReference(queueName);
            CloudQueueMessage message = new CloudQueueMessage(queueData);
            await queue.CreateIfNotExistsAsync();            
            await queue.AddMessageAsync(message, null, initialVisibilityDelay, null, null);
        }

        public async Task<IList<T>> GetQueuesAsync<T>(string queueName)
        {
            var queues = new List<T>();
            CloudQueue queue = _queueClient.GetQueueReference(queueName);
            var cloudQueueMessages = await queue.GetMessagesAsync(32);

            foreach (CloudQueueMessage message in cloudQueueMessages)
            {
                // Reading content from message
                queues.Add(message.AsString.ConvertToModel<T>());
                // Process all messages in less than 5 minutes, deleting each message after processing.
                await queue.DeleteMessageAsync(message);
            }

            return queues;

        }

        public async Task<IList<string>> GetQueuesAsync(string queueName)
        {
            var queues = new List<string>();
            CloudQueue queue = _queueClient.GetQueueReference(queueName);
            var cloudQueueMessages = await queue.GetMessagesAsync(32);

            foreach (CloudQueueMessage message in cloudQueueMessages)
            {
                // Reading content from message
                queues.Add(message.AsString);
                if (queues.Count() > 5000)
                {
                    return queues;
                }
                // Process all messages in less than 5 minutes, deleting each message after processing.
                await queue.DeleteMessageAsync(message);
            }

            return queues;

        }

        async Task<bool> IAzureStorageRepository.InsertIntoTableAsync<T>(T insertObj, string tableName)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            return await InsertIntoTableAsync(insertObj, tableName);
        }

        async Task<bool> IAzureStorageRepository.UpsertIntoTableAsync<T>(T insertObj, string tableName)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            return await UpsertIntoTableAsync(insertObj, tableName);
        }

        private async Task<CloudBlobContainer> GetAccountBlobContainer(long accountId)
        {
            CloudBlobContainer blobContainer = _blobClient.GetContainerReference("account" + accountId);
            await blobContainer.CreateIfNotExistsAsync();
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            return blobContainer;
        }

        public async Task<SalesForceAuthenticationModel> GetSalesForceAuthenticationAsync(string tableName, string filterColumnName, string filterConditionValue, string queryCompare)
        {            
            var data = await GetRecordAsync<SalesForceAuthenticationModel>(tableName, filterColumnName, filterConditionValue);
            return data;
        }

        public async Task<PIIDetails> GetPIIDetailsAsync(string partition, string tableName, string filterColumnName, string openId)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition);
            string columnFilter = TableQuery.GenerateFilterCondition(filterColumnName, QueryComparisons.Equal, openId);
            var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, columnFilter);
            var data = await GetRecordAsync<PIIDetails>(tableName, finalFilter);
            return data;
        }

        public async Task<SMSSendStatusTrack> GetSMSSendStatusAsync(string tableName, string filterColumnName, string filterConditionValue, string queryCompare)
        {
            //string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, accountId);
            //string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, correlationId);
            //var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter);            
            var data = await GetRecordAsync<SMSSendStatusTrack>(tableName, filterColumnName, filterConditionValue);
            return data;
        }

        #region Data migration methods

        public async Task<IList<JourneyInfoAzureModel>> GetJourneyTableEntitysAsync(string tableName, string filters)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            var data = await GetRecordsAsync<JourneyInfoAzureModel>(tableName, filters);
            return data.ToList();
        }

        public async Task<IList<InteractionInfoAzureModel>> GetInteractionTableEntitysAsync(string tableName, string filters)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            var data = await GetRecordsAsync<InteractionInfoAzureModel>(tableName, filters);
            return data.ToList();
        }

        public async Task<IList<ActivityInfoAzureModel>> GetActivityTableEntitysAsync(string tableName, string filters)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            var data = await GetRecordsAsync<ActivityInfoAzureModel>(tableName, filters);
            return data.ToList();
        }

        public async Task<IList<SMSLogAzureModel>> GetSmsLogTableEntitysAsync(string tableName, string filters)
        {
            AzureTableStorageConnectionString = AzureMessagingConnectionString;
            var data = await GetRecordsAsync<SMSLogAzureModel>(tableName, filters);
            return data.ToList();
        }

        #endregion
    }
}
