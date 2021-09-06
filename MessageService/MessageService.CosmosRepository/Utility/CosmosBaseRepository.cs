using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MessageService.CosmosRepository.MetaDataOperator;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.CosmosModel.ScaleModels;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Timer = System.Timers.Timer;

namespace MessageService.CosmosRepository.Utility
{
    public class CosmosBaseRepository
    {
        private static DocumentClient _client;
        private static readonly string Endpoint = AppSettings.GetValue("CosmosEndpoint");
        private static readonly string Key = AppSettings.GetValue("CosmosKey");
        private static readonly string DatabaseName = AppSettings.GetValue("CosmosDatabaseId");
        private static readonly string SMSCollectionName = AppSettings.GetValue("CosmosSMSCollectionId");
        private static readonly int DefaultOfferThroughput = Convert.ToInt32(AppSettings.GetValue("OfferThroughput"));
        private static readonly int MinRu = Convert.ToInt32(AppSettings.GetValue("MinimumRU"));
        private static readonly int MaxRu = Convert.ToInt32(AppSettings.GetValue("MaximumRU"));
        private static readonly int MaximumRetryCount = Convert.ToInt32(AppSettings.GetValue("MaximumRetryCount"));
        private static readonly object BulkInsertLock = new object();
        private static readonly object BulkDeleteLock = new object();
        private static  BulkExecutor _bulkExecutor;

        private static readonly Uri SmsCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, SMSCollectionName);
        private static ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp
        };

        private static Timer _aTimer;
        private static IMetaDataOperator _metaDataOperator = null;


        static CosmosBaseRepository()
        {
            _client = new DocumentClient(new Uri(Endpoint), Key, ConnectionPolicy);
            SetTimer();
        }

        private static async Task CreateCollectionIfNotExistsAsync(string collectionName)
        {
            // Create a document collection
            var collection = new DocumentCollection { Id = collectionName, PartitionKey = new PartitionKeyDefinition() { Paths = new Collection<string> { "/partitionkey" } } };

            var indexingPolicy = new IndexingPolicy();
            var indexRules = new Collection<Index>()
            {
                new RangeIndex(DataType.String, -1),
                new RangeIndex(DataType.Number, -1),
                new SpatialIndex(DataType.Point),

            };
            indexingPolicy.IncludedPaths.Add(new IncludedPath
            {
                Path = "/*",
                Indexes = indexRules
            });


            // Now assign the policy to the document collection
            collection.IndexingPolicy = indexingPolicy;

            await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DatabaseName),
                collection,
                new RequestOptions { OfferThroughput = DefaultOfferThroughput }
            );
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName });
        }


        public static async Task InitializeAsync()
        {
            
            await InitializeMetaDataOperator(StateMetaDataStorage.PermamentCosmosCollection);
            await InitializeBulkExecutor();
        }

        public static async Task InitializeCosmosCollectionAsync()
        {
            await CreateDatabaseIfNotExistsAsync();
            await CreateCollectionIfNotExistsAsync(SMSCollectionName);
        }

        private static async Task InitializeBulkExecutor()
        {
            _bulkExecutor = new BulkExecutor(_client, _client.ReadDocumentCollectionAsync(SmsCollectionUri).GetAwaiter().GetResult());
            await _bulkExecutor.InitializeAsync();
        }

        private static async Task InitializeMetaDataOperator(StateMetaDataStorage metaDataStorage)
        {

            if (_metaDataOperator == null)
            {
                switch (metaDataStorage)
                {
                    case StateMetaDataStorage.PermamentCosmosCollection:
                        _metaDataOperator = new PermanentCollectionMetaDataOperator();
                        break;
                        //case StateMetaDataStorage.TemporaryCosmosCollection:
                        //    _metaDataOperator = new TemporaryCollectionMetaDataOperator();
                        //    break;
                        //case StateMetaDataStorage.InMemoryCollection:
                        //    _metaDataOperator = new InMemoryMetaDataOperator();
                        //    break;
                }
            }

            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Cold);
            await _metaDataOperator.AddActiveCollectionAsync(DatabaseName, SMSCollectionName, MinRu);
        }

        public async Task<List<T>> ReadDocumentsAsync<T>(string cosmosSqlQuery, Dictionary<string, object> parameters = null, int maxItemCount = -1, bool QueryCrossPartition = false)
        {
            //await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Cold);
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = QueryCrossPartition };

            return await _client.CreateDocumentQuery<T>(
                SmsCollectionUri,
                    new SqlQuerySpec()
                    {
                        QueryText = cosmosSqlQuery,
                        Parameters = parameters.ToSQLParameterCollection()
                    },
                    queryOptions).ToListAsync();
        }

        public async Task<T> ReadDocumentAsync<T>(string cosmosSqlQuery, Dictionary<string, object> parameters = null, int maxItemCount = -1, bool QueryCrossPartition = false)
        {
           // await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Cold);
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = QueryCrossPartition };

            return await _client.CreateDocumentQuery<T>(
                SmsCollectionUri,
                    new SqlQuerySpec()
                    {
                        QueryText = cosmosSqlQuery,
                        Parameters = parameters.ToSQLParameterCollection()
                    },
                    queryOptions).FirstOrDefaultAsync();
        }

        public async Task<List<T>> ReadDocumentsWithScaleUpAsync<T>(string cosmosSqlQuery, Dictionary<string, object> parameters = null, int maxItemCount = -1, bool QueryCrossPartition = false)
        {
            var scaleOperation = await ScaleLogic.ScaleUpMaxCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu);
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = QueryCrossPartition };

            return await _client.CreateDocumentQuery<T>(
                SmsCollectionUri,
                new SqlQuerySpec()
                {
                    QueryText = cosmosSqlQuery,
                    Parameters = parameters.ToSQLParameterCollection()
                },
                queryOptions).ToListAsync();
        }

        #region INSERT
        public async Task<CosmosOperationResponse> InsertDocumentAsync(object document)
        {
            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Hot);
            CosmosOperationResponse result = new CosmosOperationResponse();
            return await InsertDocumentWithScaleAsync(document, result);
        }

        public static async Task<BulkInsertOpeartionResult> BulkInsertDocumentsAsync(IEnumerable<object> documents, bool enableUpsert = false, bool disableAutomaticIdGeneration = true, int? maxConcurrencyPerPartitionKeyRange = null,
            int? maxInMemorySortingBatchSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Hot);
            lock (BulkInsertLock)
            {
                var scaleOperation = ScaleLogic.ScaleUpMaxCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu).GetAwaiter().GetResult();
                _bulkExecutor.BulkImportAsync(documents, enableUpsert, disableAutomaticIdGeneration, maxConcurrencyPerPartitionKeyRange, maxInMemorySortingBatchSize, cancellationToken).Wait(cancellationToken);

                return new BulkInsertOpeartionResult
                {
                    ScaleOperations = new List<ScaleOperation>() { scaleOperation },
                    OperationSuccess = true
                };
            }
        }

        public static async Task<BulkInsertOpeartionResult> BulkDeleteDocumentsAsync(List<Tuple<string, string>> pkIdTuplesToDelete, int? deleteBatchSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Hot);
            lock (BulkDeleteLock)
            {
                var scaleOperation = ScaleLogic.ScaleUpMaxCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu).GetAwaiter().GetResult();
                var response = _bulkExecutor.BulkDeleteAsync(pkIdTuplesToDelete, deleteBatchSize, cancellationToken).GetAwaiter().GetResult();

                return new BulkInsertOpeartionResult
                {
                    ScaleOperations = new List<ScaleOperation>() { scaleOperation },
                    OperationSuccess = true
                };
            }
        }

        private async Task<CosmosOperationResponse> InsertDocumentWithScaleAsync(object document, CosmosOperationResponse result, int retryCount = 0)
        {
            try
            {
                await _client.CreateDocumentAsync(SmsCollectionUri, document);
                result.Success = true;
                result.TotalRetries = retryCount;
                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Request rate is large"))
                {
                    if (retryCount > MaximumRetryCount)
                    {
                        result.Success = false;
                        result.TotalRetries = retryCount;
                        return result;
                    }
                    else
                    {
                        var op = ScaleLogic.ScaleUpCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu);
                        result.ScaleOperations.Add(op);
                        return await InsertDocumentWithScaleAsync(document, result, retryCount++);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        #region DELETE
        public async Task<CosmosOperationResponse> DeleteDocumentAsync(string id, object partitionKey)
        {
            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Cold);
            CosmosOperationResponse result = new CosmosOperationResponse();
            return await DeleteDocumentAsync(id, result, partitionKey);
        }

        private async Task<CosmosOperationResponse> DeleteDocumentAsync(string id, CosmosOperationResponse result, object partitionKey, int retryCount = 0)
        {
            try
            {
                var deleteSmsCollectionUri = UriFactory.CreateDocumentUri(DatabaseName, SMSCollectionName, id);
                await _client.DeleteDocumentAsync(deleteSmsCollectionUri, new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) });
                result.Success = true;
                result.TotalRetries = retryCount;
                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Request rate is large"))
                {
                    if (retryCount > MaximumRetryCount)
                    {
                        result.Success = false;
                        result.TotalRetries = retryCount;
                        return result;
                    }
                    else
                    {
                        var op = ScaleLogic.ScaleUpCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu);
                        result.ScaleOperations.Add(op);
                        return await DeleteDocumentAsync(id, result, partitionKey, retryCount++);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        #region REPLACE
        public async Task<CosmosOperationResponse> ReplaceDocumentAsync(string oldDocumentId, object newDocument)
        {
            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Cold);
            CosmosOperationResponse result = new CosmosOperationResponse();
            return await ReplaceDocumentWithScaleAsync(oldDocumentId, newDocument, result);
        }
        private async Task<CosmosOperationResponse> ReplaceDocumentWithScaleAsync(string oldDocumentId, object newDocument, CosmosOperationResponse result, int retryCount = 0)
        {
            try
            {
                var replaceSmsCollectionUri = UriFactory.CreateDocumentUri(DatabaseName, SMSCollectionName, oldDocumentId);
                await _client.ReplaceDocumentAsync(replaceSmsCollectionUri, newDocument);
                result.Success = true;
                result.TotalRetries = retryCount;
                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Request rate is large"))
                {
                    if (retryCount > MaximumRetryCount)
                    {
                        result.Success = false;
                        result.TotalRetries = retryCount;
                        return result;
                    }
                    else
                    {
                        var op = ScaleLogic.ScaleUpCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu);
                        result.ScaleOperations.Add(op);
                        return await ReplaceDocumentWithScaleAsync(oldDocumentId, newDocument, result, retryCount++);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<CosmosOperationResponse> UpsertDocumentAsync(object document)
        {
            await _metaDataOperator.AddActivityAsync(DatabaseName, SMSCollectionName, DateTimeOffset.Now, ActivityStrength.Hot);
            CosmosOperationResponse result = new CosmosOperationResponse();
            return await UpsertDocumentWithScaleAsync(document, result);
        }
        private async Task<CosmosOperationResponse> UpsertDocumentWithScaleAsync(object document, CosmosOperationResponse result, int retryCount = 0)
        {
            try
            {
                await _client.UpsertDocumentAsync(SmsCollectionUri, document);
                result.Success = true;
                result.TotalRetries = retryCount;
                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Request rate is large"))
                {
                    if (retryCount > MaximumRetryCount)
                    {
                        result.Success = false;
                        result.TotalRetries = retryCount;
                        return result;
                    }
                    else
                    {
                        var op = ScaleLogic.ScaleUpCollectionAsync(_client, _metaDataOperator, DatabaseName, SMSCollectionName, MinRu, MaxRu);
                        result.ScaleOperations.Add(op);
                        return await UpsertDocumentWithScaleAsync(document, result, retryCount++);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion

        #region VOLLEYBALL SCALEDOWN
        private static void SetTimer()
        {
            // Create a timer with a 5 minute interval.
            _aTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            foreach (var collection in _metaDataOperator.GetAllActiveCollectionsAsync().Result)
            {
                var latestActivityForCollection = _metaDataOperator.GetLatestActivityAsync(collection.DatabaseName, collection.CollectionName).Result;

                if (latestActivityForCollection != null)
                {
                    var databaseName = collection.DatabaseName;
                    var collectioName = collection.CollectionName;
                    var minRu = collection.MinimumRU;

                    var latestActivityDateForCollection = latestActivityForCollection.ActivityTime;
                    var latestActivityStrengthForCollection = latestActivityForCollection.ActivityStrength;

                    DateTime dateToCompare = DateTime.MinValue;
                    switch (latestActivityStrengthForCollection)
                    {
                        case ActivityStrength.Hot:
                            dateToCompare = DateTime.Now.AddMinutes(-3); //3min inactivity
                            break;
                        case ActivityStrength.Medium:
                            dateToCompare = DateTime.Now.AddMinutes(-1); //1min inactivity
                            break;
                        case ActivityStrength.Cold:
                            dateToCompare = DateTime.Now.AddSeconds(-10); //10sec inactivity
                            break;
                    }

                    if (dateToCompare > latestActivityDateForCollection)
                    {
                        //no activity for 5 minutes.. scale back down to minRu
                        ScaleLogic.ScaleDownCollectionAsync(_client, _metaDataOperator, databaseName, collectioName, minRu).Wait();
                    }
                }
            }
        }

        #endregion

    }
}
