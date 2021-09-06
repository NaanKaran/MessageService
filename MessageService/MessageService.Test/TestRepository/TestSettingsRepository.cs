using Autofac;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Repository.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestRepository
{
    [TestClass]
    public class TestSettingsRepository
    {
        private readonly IContainer _container;

        public TestSettingsRepository()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void SaveSettings_AnewRowAdd_IsTure()
        {
            var con = _container.Resolve<ISettingsRepository>();

            var model = new VendorSettingsModel()
            {
                AppId = "10027",
                AppKey = "403aab5166e71e8d942acf8f2918a457",
                VendorId = 1,
                AccountId = 482
              
            };
            var tt = con.AddOrUpdateMMSVendorSettingsAsync(model).Result;
            Assert.IsTrue(tt!=0);
        }


        [TestMethod]
        public void UpdateTemplate_UpdateExistingId_IsTure()
        {
            var con = _container.Resolve<ITemplateRepository>();

            var model = new TemplateUpdateModel()
            {
                TemplateId = "NewId"
            };

            var tt = con.UpdateTemplateStatusAsync(model).Result;
            Assert.IsTrue(tt);
        }
    }
}
