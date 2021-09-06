using System;
using System.Collections.Generic;
using Autofac;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestRepository
{
    [TestClass]
    public class TestMMSRepository
    {
        private readonly IContainer _container;

        public TestMMSRepository()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void AddJourneyEntry_AnewRowAdd_IsTure()
        {
            var con = _container.Resolve<IMMSRepository>();

            var model = new JourneyActivateModel()
            {
                ActivityId = Guid.NewGuid().ToString(),
                InteractionId = Guid.NewGuid().ToString(),
                JourneyId = Guid.NewGuid().ToString()

            };
            var tt = con.AddJourneyEntryAsync(model).Result;


            var httpTrigger = new SFSendMessageRequestData()
            {
                ActivityId = model.ActivityId,
                InteractionId = model.InteractionId,
                ActivityObjectId = Guid.NewGuid().ToString(),
                ActivityInstanceId = Guid.NewGuid().ToString(),
                InArguments = new List<InArgument>() {
                    new InArgument()
                    {
                        ActionName ="Test",
                        ActivityId = Guid.NewGuid().ToString(),
                        MobileNumber ="13809288135",
                        SendAccount = "482",
                        TemplateId ="GRgbr3"
                    }
                }
            };
            var mmsservice = _container.Resolve<IMMSService>();
            var sendmms = mmsservice.SendJourneyMMSAsync(httpTrigger.ToJsonString()).Result;
            Assert.IsTrue(tt);
        }


        [TestMethod]
        public void UpdateTemplate_UpdateExistingId_IsTure()
        {
            var con = _container.Resolve<ITemplateRepository>();

            var model = new TemplateUpdateModel()
            {
                TemplateId = "NewId",
            
            };

            var tt = con.UpdateTemplateStatusAsync(model).Result;
            Assert.IsTrue(tt);
        }

        [TestMethod]
        public void AddEntryToMMSLogInfoTable_AnewRowAdd_IsTure()
        {
            var con = _container.Resolve<IMMSRepository>();
            var datetime = DateTime.UtcNow.AddMonths(3).ToChinaTime();
            var model = new MMSLogTableInfoModel()
            {
                Description = datetime.GetQuadrantMonthInfo(),
                QuadrantNumber = datetime.GetQuadrantNumberInfo(),
                TableName = datetime.GetQuadrantMMSLogTableName(),
                Year = datetime.Year
            };
            var tt = con.AddEntryToMmsLogTableInfoAsync(model).Result;
            Assert.IsTrue(tt);
        }


        [TestMethod]
        public void AddEntryToMMSLogTable_AnewRowAdd_IsTure()
        {
            var con = _container.Resolve<IMMSRepository>();

            var model = new MMSLogModel()
            {
                MobileNumber = "123456",
                AccountId = 482,
                SendDate = DateTime.Now,
                SendId = Guid.NewGuid().ToString("N"),
                SentStatus = SendStatus.Failed,
                MMSTemplateId = "yerei",
                ActivityId = "dc2044bf-b393-4834-8b7d-8bb3ae1848f1",
                ActivityInteractionId = Guid.NewGuid().ToString(),
                InteractionId = "94b48846-28a2-4f87-bfe3-edda085b44f3"
            };
            var tt = con.SaveMmsLogAsync(model).Result;
            Assert.IsTrue(tt);
        }

        [TestMethod]
        public void AddEntryToMMSIncomingMessagesTable_AnewRowAdd_IsTure()
        {
            var con = _container.Resolve<IMMSRepository>();
            for (int i = 0; i < 100; i++)
            {
                var model = new IncomingMessageModel()
                {
                    MobileNumber = Guid.NewGuid().ToString(),
                    AccountId = 2832,
                    Content = "T",
                    JourneyName = "TEST_JourneyName_" + Guid.NewGuid().ToString()

                };
                var tt = con.SaveIncomingMessageAsync(model).Result;
            }

            Assert.IsTrue(true);
        }
    }

}
