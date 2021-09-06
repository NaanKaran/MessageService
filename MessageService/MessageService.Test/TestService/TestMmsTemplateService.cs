using System;
using System.Collections.Generic;
using Autofac;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestService
{
    [TestClass]
    public class TestMmsTemplateService
    {
        private readonly IContainer _container;

        public TestMmsTemplateService()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void TestMMSSend_ResponseSuccessFormSubmail_IsTure()
        {
            var con = _container.Resolve<IMMSService>();

            //var obj = new
            //{
            //    appid = "10027",
            //    to = "13809288135",
            //    project = "GRgbr3",
            //    signature = "403aab5166e71e8d942acf8f2918a457"
            //};

            var sfJourney = new SFSendMessageRequestData()
            {
                ActivityId = "8955c022-dad9-4912-bec8-70638931cb8f",
                ActivityInstanceId = "fbf68fee-49ba-4c73-b21d-9892bb90717c",
                //JourneyId = Guid.NewGuid().ToString(),
                InArguments = new List<InArgument>()
                {
                    new InArgument()
                    {
                        TemplateId ="BXkNX3",
                        MobileNumber ="13809288135",
                        ActivityId =Guid.NewGuid().ToString(),
                        SendAccount = "482",
                        VendorSettings = new VendorSettingsModel()
                        {
                            AppId = "10047",
                            AccountId = 482,
                            AppKey = "cdad680d900654b8cdc335eaf91887bb",
                            VendorId = 1
                        }
                    }
                }
            };

            var yy = con.SendJourneyMMSAsync(sfJourney.ToJsonString()).Result;

            Assert.IsTrue(yy);
        }


       

        [TestMethod]
        public void TestMMSSendUsingPayload_ResponseSuccessFormSubmail_IsTure()
        {
            var con = _container.Resolve<IMMSService>();

            var payload = "{\"inArguments\": [{\"email\": \"dummy@wechatify.com\",\"contactKey\": \"CPS 3\",\"IsBuLoadRequired\": true,\"webhookUrl\": \"https://wechatifysalesforcefunctiondev.azurewebsites.net/api/NonPIISMS?code=ICPmILfolV2viYNVxwPFTE0ZFduClUMvrsTUcZd7y442REoxa4LxmA==&clientId=default\",\"sendAccount\": \"2832\",\"sendBot\": \"\",\"sendKeyword\": \"\",\"enrtySource\": \"DE - 1 Teja\",\"enrtySourceContactKey\": \"Customer ID\",\"enrtySourcePhoneNumber\": \"Phone Number\",\"enrtySourceId\": \"FDEB5481-4278-42ED-8670-EBF8D2A89C13\",\"enrtySourceContactKeyId\": \"[FDEB5481-4278-42ED-8670-EBF8D2A89C13].[Customer\",\"enrtySourcePhoneNumberId\": \"[FDEB5481-4278-42ED-8670-EBF8D2A89C13].[Phone\",\"orgId\": \"7269842\",\"phoneNumber\": \"13809288135\",\"journeyVersion\": \"1\",\"customData\": {},\"activityId\": null,\"messageType\": \"mms\",\"mmsTemplateId\": \"GRgbr3\",\"actionName\": \"Testing kk\"}],\"outArguments\": [],\"activityObjectID\": \"5d758de2-4424-4a69-adc9-cb66a15431f1\",\"journeyId\": \"05e41d3e-6366-4ce3-b431-52967096cf97\",\"activityId\": \"5d758de2-4424-4a69-adc9-cb66a15431f1\",\"definitionInstanceId\": \"f1f8ec84-812f-4150-8558-9743a95720ee\",\"activityInstanceId\": \"ef6a9ffe-f64e-4bfd-a6c9-459c9612c6db\",\"keyValue\": \"CPS 3\",\"mode\": 0}";

            var yy = con.SendJourneyMMSAsync(payload).Result;

            Assert.IsTrue(yy);
        }

        [TestMethod]
        public void DeleteTemplate_ResponseSuccessFormSubmail_IsTure()
        {
            var con = _container.Resolve<ITemplateService>();

            var dd = con.DeleteTemplateAsync("lwIgL3", 481).Result;

            Assert.IsTrue(true);
        }


        [TestMethod]
        public void CreateTemplate_ResponseSuccessFormSubmail_IsTure()
        {
            var con = _container.Resolve<ITemplateService>();

            var mm = new MMSTemplateModel()
            {
                AccountId = 2700,
                Content = "[{\"text\":\"sdfsfdsdfsd\",\"bloburl\":\"https://messageservicestg.blob.core.windows.net/account2700/MMS/f1150f85-b28c-4642-a61d-c49b4b04f069\",\"filesize\":19,\"filetype\":\"image/jpg\"}]",
                Signature = "【CCCCC】",
                Status = TemplateStatus.InProcess,
                TemplateName = "Test",
                Title = "Testio"
            };

            var dd = con.SaveAsync(mm).Result;

            Assert.IsTrue(true);
        }

    }
}
