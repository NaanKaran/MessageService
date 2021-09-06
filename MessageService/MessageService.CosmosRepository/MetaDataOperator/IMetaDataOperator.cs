using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models.CosmosModel.ScaleModels;

namespace MessageService.CosmosRepository.MetaDataOperator
{
    internal interface IMetaDataOperator
    {
        Task AddActivityAsync(string databaseName, string collectionName, DateTimeOffset date, ActivityStrength activityStrength);
        Task<OperationActivity> GetLatestActivityAsync(string databaseName, string collectionName);
        Task AddActiveCollectionAsync(string databaseName, string collectionName, int minimumRu);
        Task<IEnumerable<ActiveCollection>> GetAllActiveCollectionsAsync();
        Task<DateTimeOffset> GetLatestScaleUpAsync(string databaseName, string collectionName);
        Task AddScaleActivityAsync(string databaseName, string collectionName, int ru, DateTimeOffset datetime);
    }
}
