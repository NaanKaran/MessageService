using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Repository.Utility
{
    public abstract class AzureTableStorageRepository
    {
        public virtual string AzureTableStorageConnectionString { get; protected set; } = AppSettings.GetValue("ConnectionStrings:WechatifyAzureConnection");
        protected async Task<bool> CreateTableAsync(string tableName)
        {
            try
            {
                var table = GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
                return true;
            }
            catch
            {
                return false;
            }

        }

        protected async Task<bool> InsertIntoTableAsync<T>(T insertObj, string tableName) where T : ITableEntity
        {

            await CreateTableAsync(tableName);
            var table = GetTableReference(tableName);
            var insertOperation = TableOperation.Insert(insertObj);
            var tableResult = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            return true;
        }
        protected async Task<bool> UpsertIntoTableAsync<T>(T upsertObj, string tableName) where T : ITableEntity
        {

            await CreateTableAsync(tableName);
            var table = GetTableReference(tableName);
            var insertOperation = TableOperation.InsertOrMerge(upsertObj);
            var tableResult = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            return true;
        }

        protected async Task<string> UpdateIntoTableAsync<T>(T insertObj, string tableName) where T : ITableEntity
        {
            try
            {
                var table = GetTableReference(tableName);
                var updateOperation = TableOperation.Replace(insertObj);
                 await table.ExecuteAsync(updateOperation);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        protected async Task<T> GetFirstRowAsync<T>(string tableName, string partitionKey, string rowKey = "") where T : ITableEntity
        {
            try
            {
                var table = GetTableReference(tableName);
                var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                var retrievedResult = await table.ExecuteAsync(retrieveOperation);

                return (T)retrievedResult.Result;
            }
            catch
            {
                return default(T);
            }
        }

        protected async Task<T> GetRecordAsync<T>(string tableName, string filterColumnName, string filterConditionValue, string queryCompare = QueryComparisons.Equal) where T : TableEntity, new()
        {
            try
            {
                var table = GetTableReference(tableName);

                var rangeQuery = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition(filterColumnName, queryCompare, filterConditionValue));
                List<T> allEntities = new List<T>();
                TableContinuationToken tableContinuationToken = null;
                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(rangeQuery, tableContinuationToken, null, null);
                    tableContinuationToken = queryResponse.ContinuationToken;
                    allEntities.AddRange(queryResponse.Results);
                }
                while (tableContinuationToken != null);

                return allEntities.FirstOrDefault();
            }
            catch
            {
                return default(T);
            }
        }

        protected async Task<T> GetRecordAsync<T>(string tableName, string filter) where T : TableEntity, new()
        {
            try
            {
                var table = GetTableReference(tableName);

                var rangeQuery = new TableQuery<T>().Where(filter);
                List<T> allEntities = new List<T>();
                TableContinuationToken tableContinuationToken = null;
                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(rangeQuery, tableContinuationToken, null, null);
                    tableContinuationToken = queryResponse.ContinuationToken;
                    allEntities.AddRange(queryResponse.Results);
                }
                while (tableContinuationToken != null);

                return allEntities.FirstOrDefault();
            }
            catch
            {
                return default(T);
            }
        }

        protected async Task<IEnumerable<T>> GetRecordsAsync<T>(string tableName, string filter) where T : TableEntity, new()
        {
            try
            {
                List<T> allEntities = new List<T>();
                var table = GetTableReference(tableName);
                var query = new TableQuery<T>().Where(filter);
                TableContinuationToken tableContinuationToken = null;
                do
                {
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(query, tableContinuationToken, null, null);
                    tableContinuationToken = queryResponse.ContinuationToken;
                    allEntities.AddRange(queryResponse.Results);
                }
                while (tableContinuationToken != null);
                return allEntities;
            }
            catch(Exception ex)
            {
                return default(IEnumerable<T>);
            }
        }

        protected async Task DeleteTableAsync(string tableName)
        {
            try
            {
                var table = GetTableReference(tableName);
                if (await table.ExistsAsync())
                {
                    await table.DeleteAsync();
                }
            }
            catch
            {
                return;
            }
        }

        protected async Task<T> GetRecordByRowKeyAsync<T>(string tableName, string rowKey, CancellationToken ct = default(CancellationToken), Action<IList<T>> onProgress = null) where T : TableEntity, new()
        {
            try
            {
                var table = GetTableReference(tableName);
                var rangeQuery = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
                TableContinuationToken token = null;
                var items = new List<T>();
                var retrievedResult = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);
                token = retrievedResult.ContinuationToken;
                items.AddRange(retrievedResult);
                onProgress?.Invoke(items);
                while (token != null && !ct.IsCancellationRequested) ;

                return items.FirstOrDefault();
            }
            catch
            {
                return default(T);
            }
        }

        private CloudTable GetTableReference(string tableName)
        {
         var tableClient = CloudStorageAccount.Parse(AzureTableStorageConnectionString).CreateCloudTableClient();
         return tableClient.GetTableReference(tableName);
        }
    

    }
}
