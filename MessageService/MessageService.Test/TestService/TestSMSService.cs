using System;
using System.Collections.Generic;
using Autofac;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Service.Interface;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestService
{
    [TestClass]
    public class TestSMSService
    {
        private readonly IContainer _container;

        public TestSMSService()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void TestSMSEventGridPublish()
        {
            var payload = @"{
  'inArguments': [
    {
      'email': 'dummy@wechatify.com',
      'contactKey': 'Bhavans 60057',
      'IsBuLoadRequired': true,
      'webhookUrl': 'https://wechatifysalesforcefunctiondev.azurewebsites.net/api/NonPIISMS?code=ICPmILfolV2viYNVxwPFTE0ZFduClUMvrsTUcZd7y442REoxa4LxmA==&clientId=default',
      'sendAccount': '2700',
      'sendBot': 'Hi %%1lakhrecord.CustomerName%%,\n%%1lakhrecord.PhoneNumber%%\ntesting.\nsms',
      'sendKeyword': 'Hi %%1lakhrecord.CustomerName%%,\n%%1lakhrecord.PhoneNumber%%\ntesting.\nsms',
      'enrtySource': '1lakhrecord',
      'enrtySourceContactKey': 'CustomerName',
      'enrtySourcePhoneNumber': 'PhoneNumber',
      'enrtySourceId': 'CA9DD34D-1573-46A4-A933-51DBB1AE678E',
      'enrtySourceContactKeyId': '[CA9DD34D-1573-46A4-A933-51DBB1AE678E].[CustomerName]',
      'enrtySourcePhoneNumberId': '[CA9DD34D-1573-46A4-A933-51DBB1AE678E].[PhoneNumber]',
      'orgId': '100009314',
      'phoneNumber': '15217326571',
      'journeyVersion': '1',
      'customData': {
        '1lakhrecord.CustomerName': '1Kay6000',
        '1lakhrecord.PhoneNumber': '15217326571'
      },
      'variables': [
        {}
      ],
      'dataextensionvariables': [
        {}
      ],
      'activityId': null,
      'messageType': 'sms',
      'journeyKey': '9613de98-647e-3dc6-9451-3e286ecded55',
      'actionName': 'test_sms_2'
    }
  ],
  'outArguments': [],
  'activityObjectID': '8d59bcc5-44cd-406a-9afd-21f7800802a0',
  'journeyId': 'fef9ca28-37e6-49de-af42-894c4d4f97e4',
  'activityId': '8d59bcc5-44cd-406a-9afd-21f7800802a0',
  'definitionInstanceId': '14c63f26-3af1-4f1a-b9a2-b99ebc23fe08',
  'activityInstanceId': '5660f389-d085-473e-bdcd-afd85b8a5fd0',
  'keyValue': 'Bhavans 60057',
  'mode': 0
}";
            var smsRequestData = payload.ConvertToModel<SFSendMessageRequestData>();
            List<EventGridEvent> gridEvent = new List<EventGridEvent>();
            gridEvent.Add(new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Data = smsRequestData,
                EventTime = DateTime.UtcNow,
                EventType = "Category 1",
                Subject = "SMS Send Event",
                DataVersion = "1.0"
            });

            EventGridHelper.PublishSMSEventsAsync(gridEvent).Wait();
        }

        [TestMethod]
        public void TestSMSSendUsingPayload_ResponseSuccessFormSubmail_IsTrue()
        {
            var con = _container.Resolve<ISMSService>();

            var payload = @"{
  'inArguments': [
    {
      'email': '',
      'contactKey': '01',
      'IsBuLoadRequired': true,
      'webhookUrl': 'https://wechatifysalesforcefunctiondev.azurewebsites.net/api/NonPIISMS?code=ICPmILfolV2viYNVxwPFTE0ZFduClUMvrsTUcZd7y442REoxa4LxmA==&clientId=default',
      'sendAccount': '2700',
      'sendBot': 'Hi %%MMS Personalization.Name%%,\nThis is a Test message from TMG.\nThis is your email in Salesforce : %%MMS Personalization.EmailId%%\nLocation : %%MMS Personalization.Location%%\n\nPlease Ignore this SMS.\nThanks',
      'sendKeyword': 'Hi %%MMS Personalization.Name%%,\nThis is a Test message from TMG.\nThis is your email in Salesforce : %%MMS Personalization.EmailId%%\nLocation : %%MMS Personalization.Location%%\n\nPlease Ignore this SMS.\nThanks',
      'enrtySource': 'MMS Personalization',
      'enrtySourceContactKey': 'Name',
      'enrtySourcePhoneNumber': 'Number',
      'enrtySourceId': 'MMS Personalization',
      'enrtySourceContactKeyId': '[MMS',
      'enrtySourcePhoneNumberId': '[MMS',
      'orgId': '100009314',
      'phoneNumber': '13763330013',
      'journeyVersion': '1',
      'customData': {
        'MMS Personalization.Name': 'Gwen',
        'MMS Personalization.EmailId': 'gwen@google.com',
        'MMS Personalization.Location': ''
      },
      'variables': [
        {}
      ],
      'dataextensionvariables': [
        {}
      ],
      'activityId': null,
      'messageType': 'sms',
      'journeyKey': '01e858ca-5d15-d305-84f8-f1d85002b1c2',
      'actionName': 'test_smsActivity_personalization_ASWIN'
    }
  ],
  'outArguments': [],
  'activityObjectID': '023ddf6e-dbb3-4a0d-8daa-740f1485baf5',
  'journeyId': 'c954c9e3-facd-4dba-8fb2-b83c57008d3b',
  'activityId': '023ddf6e-dbb3-4a0d-8daa-740f1485baf5',
  'definitionInstanceId': '612a50d9-77b2-41fd-8fb9-5c040a5845f0',
  'activityInstanceId': '6b580c66-e5d5-4cb5-8013-ee9493722fe2',
  'keyValue': '01',
  'mode': 0
}";

            var yy = con.SendJourneySMSAsync(payload).Result;

            Assert.IsTrue(yy);
        }


        [TestMethod]
        public void SendDeliveryReport_SMS()
        {
            var con = _container.Resolve<ISMSService>();

            var templatePath =
                @"E:\Code\Git\MessageService\SMS.QueueAndTimeTriggerFunction\EmailTemplates\SMSDeliveryReportTemplate.htm";
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            con.SendDeliveryReportAsync(baseDirectory, templatePath).Wait();
        }


        [TestMethod]
        public void SubmailCallback()
        {
            var con = _container.Resolve<ISMSService>();

            

            con.UpdateSMSLogAsync("{'events':'delivered','address':'18826487813','app':'18943','content':null,'timestamp':'1568220280','token':'78e0f56731fe69a5f7dd76d8453c3c86','signature':'1dd6a4bf275693b7ef4c42fef9f01cad','send_id':'cbf42925920cd22e9f8cd65d8c177c05','report':null,'template_id':null,'status':null,'reason':null,'EventType':1,'EventDateTime':'2019-09-11T16:44:40Z','DeliveryStatus':1}").Wait();
        }




        [TestMethod]
        public void Threshold_SMS()
        {
            var con = _container.Resolve<ISMSService>();

            var templatePath =
                @"E:\Code\Git\MessageService\SMS.QueueAndTimeTriggerFunction\EmailTemplates\SMSThresholdNotificationTemplate.htm";
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            con.SendSMSInventoryAndThresholdNotificationAsync(templatePath).Wait();
        }

    }
}
