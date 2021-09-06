using Autofac;
using MessageService.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestService
{
    [TestClass]
    public class TestDataExtensionService
    {
        private readonly IContainer _container;

        public TestDataExtensionService()
        {
            _container = new ContainerResolver().Container;
        }

       


        [TestMethod]
        public void UpdateMMSLogToDE_UpdateLog_IsTure()
        {
            var con = _container.Resolve<IMMSDataExtensionService>();

            var accountId = 88;
            con.UpdateMmsLogToDataExtension(accountId).Wait();

            Assert.IsTrue(true);
        }


        [TestMethod]
        public void CreateDE_createLog_IsTure()
        {
            var con = _container.Resolve<ISMSDataExtensionService>();

           // con.CreateDataExtension(typeof(WeChatifyMMSLog), 2700, "WeChatifyStageFolder3").Wait();

           var orgId = "7269842";
           var oauthToken = "0XKmOEZUxG1_QZb6XpmgRV2l3ETyhmu2HvwHxcBTX2myDeQYCqlqqHJFDww16somVPhG8GpjinDacDHuxxCxsfF9aSwmZCR0SzQVC214q9DvKqSu3fKSyMjnOF9AqR6Hdz0WJs5LQX7fGg2eE4qu59MCmuMeOOybDLMkF8wuTyGSR-j0npf40g9UcpchAptvWH2KjkUheVVjWYcfJ1U5uxeu3wWexYqgSALViXZzEjA7aFsUcKoEIMWl5l4uXwr1z";
           var deName = "testDE";
           var soapEndpointUrl = "https://webservice.s7.exacttarget.com/Service.asmx";
           var isDe = false;
           var deKey = con.GetDataExtensionKey(orgId, oauthToken, deName, soapEndpointUrl, isDe).Result;

            Assert.IsNotNull(deKey);
        }
        [TestMethod]
        public void UpdateMMSLog_DELog_IsTure()
        {
            var con = _container.Resolve<IMMSDataExtensionService>();

            var accountId = 2700;
             con.UpdateMmsLogToDataExtension(accountId).Wait();

            Assert.IsNotNull(true);
        }

        [TestMethod]
        public void UpdateIncomingMMSLog_DELog_IsTure()
        {
            var con = _container.Resolve<IMMSDataExtensionService>();

            var accountId = 2700;
            con.UpdateIncomingMmsLogToDataExtension(accountId).Wait();

            Assert.IsNotNull(true);
        }


        [TestMethod]
        public void UpdateSMSLog_DELog_IsTure()
        {
            var con = _container.Resolve<ISMSDataExtensionService>();

            var accountId = 638;
            con.UpdateSMSLogToDataExtensionAsync(accountId).Wait();
            //con.UpdateIncomingSMSLogToDataExtensionAsync(accountId).Wait();
            Assert.IsNotNull(true);
        }

    }
}
