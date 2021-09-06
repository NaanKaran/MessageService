using System;
using System.Collections.Generic;
using Autofac;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.SubmailModel;
using MessageService.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestService
{
    [TestClass]
    public class TestMmsService
    {
        private readonly IContainer _container;

        public TestMmsService()
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

            var payload = @"{
  'inArguments': [
    {
      'email': 'dummy@wechatify.com',
      'contactKey': 'CPS 9',
      'IsBuLoadRequired': true,
      'webhookUrl': 'https://wechatifysalesforcefunctiondev.azurewebsites.net/api/NonPIISMS?code=ICPmILfolV2viYNVxwPFTE0ZFduClUMvrsTUcZd7y442REoxa4LxmA==&clientId=default',
      'sendAccount': '2700',
      'sendBot': '',
      'sendKeyword': '',
      'enrtySource': 'DE 2 - Teja',
      'enrtySourceContactKey': 'Customer ID',
      'enrtySourcePhoneNumber': 'Mobile Number',
      'enrtySourceId': 'B552DB0A-3E65-4E82-A8E6-BAC9F395C1CF',
      'enrtySourceContactKeyId': '[B552DB0A-3E65-4E82-A8E6-BAC9F395C1CF].[Customer',
      'enrtySourcePhoneNumberId': '[B552DB0A-3E65-4E82-A8E6-BAC9F395C1CF].[Mobile',
      'orgId': '7269842',
      'phoneNumber': '',
      'journeyVersion': '1',
      'customData': {},
      'variables': [
        {
          'hello': 'Association croquet',
          'name': ''
        }
      ],
      'dataextensionvariables': [
        {
          'hello': 'DE 2 - Teja.Interest',
          'name': 'DE 2 - Teja.Mobile Number'
        }
      ],
      'activityId': null,
      'messageType': 'mms',
      'mmsTemplateId': 'EqCWi2',
      'journeyKey': '04d304fb-86ad-b1ae-fc25-4011d5d3488c',
      'vendorSettings': {
        'accountid': 2700,
        'vendorid': 1,
        'appid': '10047',
        'appkey': 'cdad680d900654b8cdc335eaf91887bb',
        'settingsID': null,
        'signatureText': null,
        'unSubscribeText': null,
        'vendorUserName': null,
        'vendorPassword': null
      },
      'actionName': 'Testing kk'
    }
  ],
  'outArguments': [],
  'activityObjectID': '729e1f3b-90bd-4452-aee6-d26b9c3bca8d',
  'journeyId': 'cefd8132-7be6-4959-b50d-310f449385ad',
  'activityId': '729e1f3b-90bd-4452-aee6-d26b9c3bca8d',
  'definitionInstanceId': '380a9db7-5411-45cf-90f0-bd9a50b52ae0',
  'activityInstanceId': 'edafed16-fe24-4dfa-94c1-2640b0d1d902',
  'keyValue': 'CPS 9',
  'mode': 0
}";

            var yy = con.SendJourneyMMSAsync(payload).Result;

            Assert.IsTrue(yy);
        }

        [TestMethod]
        public void AddMMSLog_ResponseSuccessFormSubmail_IsTure()
        {
            var con = _container.Resolve<IMMSService>();
            //var payload =
            //    "{\"SendId\":\"46fbf7b3abb848059e597eb725485d9b\",\"MobileNumber\":\"Test\",\"MMSTemplateId\":\"GRgbr3\",\"DynamicParamsValue\":null,\"AccountId\":482,\"SentStatus\":0,\"SendDate\":\"2019-05-29T14:31:50.0628274Z\",\"DeliveryStatus\":null,\"DeliveryDate\":null,\"DropErrorCode\":\"252\",\"ErrorMessage\":\"Incorrect recipient message address, if you are using the addressbook model, this addressbook does not have any contacts\",\"InteractionId\":\"fbf68fee-49ba-4c73-b21d-9892bb90717c\",\"JourneyId\":null,\"ActivityId\":\"8955c022-dad9-4912-bec8-70638931cb8f\",\"ActivityInteractionId\":\"8955c022-dad9-4912-bec8-70638931cb8f\",\"OrgId\":null}";

            var payload = "{'RetryCount':0,'Data':{'events':'dropped','address':'18122311333','app':'10047','content':null,'timestamp':'1562635469','token':'74b0fa39364a4a7640f0b367f5b58239','signature':'3bc28494982dfc4118ae1e7f2e770ecc','send_id':'967f5391da5396bb01a2fa41d5ecd12d','report':'ERROR:203:[不具有发送电信手机彩信功能]','template_id':null,'status':null,'reason':null,'EventType':2,'EventDateTime':'2019-07-09T01:24:29Z','DeliveryStatus':2}}";

            var yy = con.UpdateMmsLogAsync(payload).Result;

            Assert.IsTrue(yy);
        }

        [TestMethod]
        public void MMSLogExport_UpdateExistingId_IsTure()
        {
            var con = _container.Resolve<IMMSService>();

            var data = new LogFilterModel()
            {
                AccountId = 482,
                JourneyId = "8bdf6d1b-a305-4a95-a9fe-cab30dd1c400",
                QuadrantTableName = DateTime.Now.GetQuadrantMMSLogTableName()
            };

            var ttt = data.ToJsonString();

            var tt = con.ExportMMSLogAsync(ttt, "", "").Result;
            Assert.IsTrue(tt);
        }


        [TestMethod]
        public void MMSSend_UpdateExistingId_IsTure()
        {
            var con = _container.Resolve<ISubMailApiClientService>();

            var data = new LogFilterModel()
            {
                AccountId = 482,
                JourneyId = "8bdf6d1b-a305-4a95-a9fe-cab30dd1c400",
                QuadrantTableName = DateTime.Now.GetQuadrantMMSLogTableName()
            };

            var ttt = data.ToJsonString();

            var tt = con.SendMMS<SubmailResponseModel>(new SubmailMMSPostModel()).Result;
            Assert.IsTrue(true);
        }


        [TestMethod]
        public void Journey_Tesst_IsTure()
        {
            var con = _container.Resolve<IMMSService>();

            var yy = con.ExportJourneysAsync(
                "{'journeyid':'06546536-8c83-4a4d-bb3a-6e4b34bda2a3','emailids':['karunakaran@tmgworldwide.com'],'accountid':2807,'fromdate':'2019-07-24T00:00:00','todate':'2019-07-24T00:00:00'}",
                "", "");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void MMSUsageCount_AddOrUpdate()
        {
            var con = _container.Resolve<IMMSService>();

            con.MMSUsageCountUpdateAsync(638).Wait();
        }


        [TestMethod]
        public void GetMMSUsageCount()
        {
            var con = _container.Resolve<IMMSService>();

           var data = con.GetMmsUsageDetailAsync(638,2019).Result;

           Assert.IsNotNull(data);
        }

    }
}
