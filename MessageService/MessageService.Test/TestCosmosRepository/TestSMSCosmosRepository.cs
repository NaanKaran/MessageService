using System;
using System.Linq;
using Autofac;
using MessageService.CosmosRepository.Interface;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.StoredProcedureModels;
using MessageService.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestCosmosRepository
{
    [TestClass]
    public class TestSMSCosmosRepository
    {
        private readonly IContainer _container;

        public TestSMSCosmosRepository()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void AddJourneyEntry_InsertJourney()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();
            var journeyProcModel = new JourneyProcModel()
            {
                AccountId = 2700,
                ActivityId = "72f4db32-b1e2-413e-ba10-2fa3527c5959",
                ActivityName = "Test Action",
                CreatedOn = DateTime.Now,
                InteractionId = "4ffce8cf-e288-4f1d-8411-57d55b452cdb",
                JourneyId = "358bbb78-e35c-43d3-a88c-46aa5ff2bc89",
                JourneyName = "Test journey",
                JourneyKey = "key",
                Version = "1.0"
            };

            var data = con.AddOrUpdateJourneyDetailsAsync(journeyProcModel).Result;


        }

        [TestMethod]
        public void AddIncomingSMS()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();
            for (int i = 0; i < 25; i++)
            {
                var incomingMessageDocumentModel = new IncomingMessageDocumentModel()
                {
                    AccountId = 644,
                    Content = "T",
                    IsUpdatedIntoDE = false,
                    MobileNumber = "1234567809" + i,
                    CreatedOn = DateTime.UtcNow.ToChinaTime().AddSeconds(i),
                    JourneyName = "Testing"
                };

                var data = con.SaveIncomingMessageAsync(incomingMessageDocumentModel).Result;
            }  

        }

        [TestMethod]
        public void AddSMSLog_AnewDocumentAdd_IsTrue()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();
            for (int i = 0; i < 1000; i++)
            {
                var model = new SMSLogDocumentModel()
                {
                    ActivityId = "72f4db32-b1e2-413e-ba10-2fa3527c5959",
                    InteractionId = "4ffce8cf-e288-4f1d-8411-57d55b452cdb",
                    JourneyId = "358bbb78-e35c-43d3-a88c-46aa5ff2bc89",//Guid.NewGuid().ToString(),
                    PartitionKey = "358bbb78-e35c-43d3-a88c-46aa5ff2bc89",//Guid.NewGuid().ToString(),
                    AccountId = 2700,
                    SentStatus = SendStatus.Success,
                    Id = Guid.NewGuid().ToString(),
                    DeliveryStatus = DeliveryStatus.Pending,
                    DropErrorCode = "200",
                    ErrorMessage = "Success",
                    MobileNumber = "54s64",
                    SendDate = DateTime.UtcNow,
                    SMSContent = "TEST",
                    PersonalizationData = "HA",
                    DeliveryDate = DateTime.UtcNow.ToChinaTime(),
                    Type = CosmosDocumentType.Log
                };
                con.InsertSMSLogAsync(model).Wait();
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void UpdateJourneyCount()
        {
            var con = _container.Resolve<ISMSService>();
            var journeyData = new JourneyInfoDocumentModel()
            {
                Id = "358bbb78-e35c-43d3-a88c-46aa5ff2bc89",
                AccountId = 2700,
                JourneyName = "New Test",
                InitiatedDate = DateTime.Now.ToChinaTime()
            };
            con.UpdateSMSLogCountInJourneyAsync(journeyData).Wait();
        }


        [TestMethod]
        public void UpdateSMSLogCountInJourney()
        {
            var smsService = _container.Resolve<ISMSService>();
            var journeys = smsService.GetLastDayRanJourneysAsync().Result;
            foreach (var journey in journeys)
            {
                smsService.UpdateSMSLogCountInJourneyAsync(journey).Wait();
            }
        }

        [TestMethod]
        public void AddJourneyInfo_AnewDocumentAdd_IsTrue()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();
            for (int i = 0; i < 100; i++)
            {
                var model = new JourneyInfoDocumentModel()
                {

                    AccountId = 481,
                    Id = Guid.NewGuid().ToString(),
                    Type = CosmosDocumentType.Journey,
                    JourneyName = "Test " + i,

                };
                con.AddOrUpdateJourneyInfoAsync(model).Wait();
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetSMSLogs()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();

            var data = con.GetSMSLogAsync(481, "fdcc1c75-ee51-4037-95ff-fc0b3541c937").Result;

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetJourneyInfo_GetAllData_ListOfdata()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();

            var data = con.GetJourneysAsync(481).Result;

            Assert.IsTrue(data.Any());
        }

        [TestMethod]
        public void UpdateAllJourneyCount()
        {
            var smsService = _container.Resolve<ISMSService>();
            var journeys = smsService.GetLastDayRanJourneysAsync().Result;

            foreach (var journey in journeys)
            {
                smsService.UpdateSMSLogCountInJourneyAsync(journey).Wait();
            }
        }

        [TestMethod]
        public void UpadateJourney_ShouldChangeTheValueAlone_True()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();
            var model = new JourneyInfoDocumentModel()
            {
                Id = "59e35a2d-41c4-466e-8bef-b0c3b13be6c6",
                AccountId = 481,
                JourneyName = "Test UpdateJourney",
                TotalCount = 1
            };
            var data = con.AddOrUpdateJourneyInfoAsync(model).Result;

            Assert.IsTrue(true);
        }


        [TestMethod]
        public void GetSMSLogCount_True()
        {
            var con = _container.Resolve<ISMSCosmosRepository>();

            var data = con.GetSMSLogCountAsync(2700, "0e301a4f-b83b-4ebc-ab1d-79b7083c926d", DateTime.Now.AddDays(-10)).Result;

            Assert.IsTrue(true);
        }



    }

}
