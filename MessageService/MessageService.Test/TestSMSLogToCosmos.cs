using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using AutoMapper;
using MessageService.CosmosRepository.Utility;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.MigrationModel;
using MessageService.Repository.Interface;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Test
{
    [TestClass]
    public class SMSLogToCosmos
    {
        private readonly IContainer _container;
        public SMSLogToCosmos()
        {
            _container = new ContainerResolver().Container;
        }
        [TestMethod]
        public void AzureTableToCosmos()
        {

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<JourneyInfoAzureModel, JourneyInfoDocumentModel>().ReverseMap();
                cfg.CreateMap<ActivityInfoAzureModel, ActivityInfoDocumentModel>().ReverseMap();
                cfg.CreateMap<InteractionInfoAzureModel, InteractionInfoDocumentModel>().ReverseMap();
                cfg.CreateMap<SMSLogAzureModel, SMSLogDocumentModel>().ReverseMap();
            });

            var mapper = config.CreateMapper();

            var azureStorage = _container.Resolve<IAzureStorageRepository>();

            var accountId = 583;
            var replaceDocumentAccount = 644;


            //var ttt = GetSmsLogModels(accountId, azureStorage, "5d8bb8d4-e543-4e85-a344-a87cf3ee39ac");

            //var mm = mapper.Map<List<SMSLogDocumentModel>>(ttt);


            var journeyInfo = GetJourneyModels(accountId, azureStorage);
            var interactionInfo = GetInteractionInfoModels(accountId, azureStorage);
            var activityInfoModels = GetActivityInfoModels(accountId, azureStorage);

            var cosomosJourney = mapper.Map<List<JourneyInfoDocumentModel>>(journeyInfo);
            cosomosJourney.ForEach(u=>u.AccountId = replaceDocumentAccount);
            CosmosBaseRepository.BulkInsertDocumentsAsync(cosomosJourney, true).Wait();

            var cosomosInteraction = mapper.Map<List<InteractionInfoDocumentModel>>(interactionInfo);

            cosomosInteraction.ForEach(u => u.AccountId = replaceDocumentAccount);
            CosmosBaseRepository.BulkInsertDocumentsAsync(cosomosInteraction, true).Wait();

            var cosomosActivity = mapper.Map<List<ActivityInfoDocumentModel>>(activityInfoModels);
            cosomosActivity.ForEach(u => u.AccountId = replaceDocumentAccount);
            CosmosBaseRepository.BulkInsertDocumentsAsync(cosomosActivity, true).Wait();


            foreach (var journey in journeyInfo)
            {
                var logdata = GetSmsLogModels(accountId, azureStorage,journey.Id);

                var cosmosData = mapper.Map<List<SMSLogDocumentModel>>(logdata);

                cosmosData.ForEach(y=>
                {
                    y.AccountId = replaceDocumentAccount;
                    y.MobileNumber = "XXXXXXXXXXX";
                });
                CosmosBaseRepository.BulkInsertDocumentsAsync(cosmosData,true).Wait();
            }

            //string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, accountId);
            //string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, correlationId);
            //var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter);    
        }

        private List<JourneyInfoAzureModel> GetJourneyModels(long accountId, IAzureStorageRepository repository)
        {
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CosmosDocumentType.Journey.ToString());
            string accountFilter = TableQuery.GenerateFilterConditionForInt("AccountId", QueryComparisons.Equal, Convert.ToInt32(accountId));

            var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, accountFilter);

            var journeyData =
                repository.GetJourneyTableEntitysAsync(CosmosDocumentType.Journey.ToString(), finalFilter).Result;

            return journeyData.ToList();
        }

        private List<InteractionInfoAzureModel> GetInteractionInfoModels(long accountId, IAzureStorageRepository repository)
        {
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CosmosDocumentType.Interaction.ToString());
            string accountFilter = TableQuery.GenerateFilterConditionForInt("AccountId", QueryComparisons.Equal, Convert.ToInt32(accountId));

            var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, accountFilter);

            var journeyData =
                repository.GetInteractionTableEntitysAsync(CosmosDocumentType.Interaction.ToString(), finalFilter).Result;

            return journeyData.ToList();
        }

        private List<ActivityInfoAzureModel> GetActivityInfoModels(long accountId, IAzureStorageRepository repository)
        {
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CosmosDocumentType.Activity.ToString());
            string accountFilter = TableQuery.GenerateFilterConditionForInt("AccountId", QueryComparisons.Equal, Convert.ToInt32(accountId));

            var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, accountFilter);

            var journeyData =
                repository.GetActivityTableEntitysAsync(CosmosDocumentType.Activity.ToString(), finalFilter).Result;

            return journeyData.ToList();
        }

        private List<SMSLogAzureModel> GetSmsLogModels(long accountId, IAzureStorageRepository repository, string key)
        {
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key);
            string accountFilter = TableQuery.GenerateFilterConditionForInt("AccountId", QueryComparisons.Equal, Convert.ToInt32(accountId));

            var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, accountFilter);

            var journeyData =
                repository.GetSmsLogTableEntitysAsync(CosmosDocumentType.Log.ToString(), finalFilter).Result;

            return journeyData.ToList();
        }
    }
}
