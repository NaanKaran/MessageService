using System;
using System.Collections.Generic;
using Autofac;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Repository.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestRepository
{
    [TestClass]
    public class TestTemplateRepository
    {
        private readonly IContainer _container;

        public TestTemplateRepository()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void SaveTemplate_AnewRowAdd_IsTure()
        {
            var con = _container.Resolve<ITemplateRepository>();

            var model = new MMSTemplateModel()
            {
                Id = "1Xin3",
                AccountId = 481,
                Content = "",
                CreatedBy = Guid.NewGuid().ToString(),               
                Signature = "TMG",
                Status = TemplateStatus.InProcess,
                TemplateName = "Test",
                Title = "Test"
            };

            var tt = con.SaveAsync(model).Result;
            Assert.IsTrue(tt);
        }



        [TestMethod]
        public void GetTemplate_templatesList_IsTure()
        {
            var con = _container.Resolve<ITemplateRepository>();

            var model = new GetTemplateModel()
            {

                AccountId = 482,
                ItemsPerPage = 10,
                PageNo = 1,
               // TemplateName = "asdfa",
                Status = new List<TemplateStatus>() { TemplateStatus.InProcess}
            };

            var tt = con.GetAsync(model).Result;
            Assert.IsNotNull(tt);
        }

        [TestMethod]
        public void UpdateTemplate_UpdateExistingId_IsTure()
        {
            var con = _container.Resolve<ITemplateRepository>();

            var model = new TemplateUpdateModel()
            {
                TemplateId = "1Xin3",
                Comments = "Rejected",
                UpdatedBy = "TestUser"
            };

            var tt = con.UpdateTemplateStatusAsync(model).Result;
            Assert.IsTrue(tt);
        }

        [TestMethod]
        public void AzureLogTableAddEntry_MakeAEntryintoTable()
        {
            var con = _container.Resolve<IAzureStorageRepository>();

            var model = new MMSFailedQueueLog("100")
            {
            AccountId = "100",
            Data = "Testing",
            ErrorMessage = "testing",
            StackTrace = "nn",
            Type = MMSFailedQueueType.SalesForceHttpTrigger,
            RowKey = Guid.NewGuid().ToString(),
            PartitionKey = "100"
            };
            //con.AzureMessagingConnectionString = AppSettings.GetValue("ConnectionStrings:AzureConnection");
            con.InsertIntoTableAsync(model, nameof(MMSFailedQueueLog)).Wait();
            Assert.IsTrue(true);
        }
    }
}
