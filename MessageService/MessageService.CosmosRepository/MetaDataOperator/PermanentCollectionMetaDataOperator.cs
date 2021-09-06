using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService.Models.CosmosModel.ScaleModels;
using MessageService.RedisRepository.Implementation;
using MessageService.RedisRepository.Interface;

namespace MessageService.CosmosRepository.MetaDataOperator
{
    internal class PermanentCollectionMetaDataOperator : IMetaDataOperator
    {
        private readonly IRedisCache _redisRepository;

        public PermanentCollectionMetaDataOperator()
        {
            _redisRepository = new RedisCache();
        }

        public async Task AddActiveCollectionAsync(string databaseName, string collectionName, int minimumRu)
        {

            var activeCollection = new ActiveCollection(databaseName, collectionName, minimumRu);
            var activeCollections = (await _redisRepository.GetAsync<List<ActiveCollection>>(activeCollection.MetaDataType)) ?? new List<ActiveCollection>();
            if (activeCollections.Any(k =>
                k.CollectionName == collectionName && k.MetaDataType == activeCollection.MetaDataType))
            {
                return;
            }
            activeCollections.Add(activeCollection);
            await _redisRepository.SetAsync(activeCollection.MetaDataType, activeCollections);

        }


        public async Task<IEnumerable<ActiveCollection>> GetAllActiveCollectionsAsync()
        {
            return await _redisRepository.GetAsync<List<ActiveCollection>>(nameof(ActiveCollection));
        }


        public async Task AddActivityAsync(string databaseName, string collectionName, DateTimeOffset date, ActivityStrength activityStrength)
        {
            var operationActivity = new OperationActivity(databaseName, collectionName, date, activityStrength);
            var redisKey = collectionName + "_" + operationActivity.MetaDataType;

            var operationActivities = (await _redisRepository.GetAsync<List<OperationActivity>>(redisKey)) ?? new List<OperationActivity>();

            if (!operationActivities.Any())
            {
                operationActivities.Add(operationActivity);
                await _redisRepository.SetAsync(redisKey, operationActivities);
                return;
            }

            var operation = operationActivities.FirstOrDefault(k => k.ActivityStrength == activityStrength) ?? operationActivity;

            operation.ActivityTime = date;

            operationActivities.Add(operation);

            await _redisRepository.SetAsync(redisKey, operationActivities.Distinct());

        }



        public async Task<OperationActivity> GetLatestActivityAsync(string databaseName, string collectionName)
        {
            var redisKey = collectionName + "_" + nameof(OperationActivity);
            var operationActivities = (await _redisRepository.GetAsync<List<OperationActivity>>(redisKey)) ?? new List<OperationActivity>();
            return operationActivities.OrderByDescending(k => k.ActivityTime).FirstOrDefault();
        }


        public async Task<DateTimeOffset> GetLatestScaleUpAsync(string databaseName, string collectionName)
        {
            var redisKey = collectionName + "_" + nameof(OperationActivity);

            var scaleActivities = (await _redisRepository.GetAsync<List<ScaleActivity>>(redisKey)) ?? new List<ScaleActivity>();
            return scaleActivities.OrderByDescending(k => k.ScaleTime).FirstOrDefault()?.ScaleTime ?? DateTimeOffset.MinValue;
        }

        public async Task AddScaleActivityAsync(string databaseName, string collectionName, int ru, DateTimeOffset datetime)
        {
            var scaleActivity = new ScaleActivity(databaseName, collectionName, ru, datetime);
            var redisKey = collectionName + "_" + scaleActivity.MetaDataType;

            var scaleActivities = (await _redisRepository.GetAsync<List<ScaleActivity>>(redisKey)) ?? new List<ScaleActivity>();
            scaleActivities.Add(scaleActivity);
            var expiredDateTime = DateTimeOffset.Now.AddHours(-1);
            //it will remove old data from list
            scaleActivities = scaleActivities.Where(k => k.ScaleTime > expiredDateTime).ToList();
            await _redisRepository.SetAsync(redisKey, scaleActivities);
        }
    }
}
