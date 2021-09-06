using System;
using System.IO;
using Autofac;
using Microsoft.Azure.WebJobs;
using MessageService.Service.Interface;
using Microsoft.Extensions.Logging;
using MessageService.InfraStructure.Helpers;

namespace SMS.QueueAndTimeTriggerFunction
{
    public static class SMSFunctions
    {
        [FunctionName("SMSExportIncomingMessagesFunction")]
        public static void SMSExportIncomingMessagesFunction([QueueTrigger("smsincomingmessagesexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "EmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.ExportIncomingMessagesAsync(false, queueData, templatePath, log).Result;
                }
            }
            catch (AggregateException e)
            {
                ExceptionEmailAlert("SMSExportIncomingMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportIncomingMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSExportIncomingMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportIncomingMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("SMSExportOptOutMessagesFunction")]
        public static void SMSExportOptOutMessagesFunction([QueueTrigger("smsoptoutmessagesexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "EmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.ExportIncomingMessagesAsync(true, queueData, templatePath, log).Result;
                }
            }
            catch (AggregateException e)
            {
                ExceptionEmailAlert("SMSExportOptOutMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportOptOutMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSExportOptOutMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportOptOutMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("SMSExportVerificationMessagesFunction")]
        public static void SMSExportVerificationMessagesFunction([QueueTrigger("smsverificationexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "EmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.ExportVerificationSMSAsync(queueData, directoryPath, templatePath).Result;
                }
            }
            catch (AggregateException e)
            {
                ExceptionEmailAlert("SMSExportVerificationMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportVerificationMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSExportVerificationMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportVerificationMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("SMSLogExportFunction")]
        public static void SMSLogExportFunction([QueueTrigger("smslogexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "JouneyEmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.ExportSMSLogAsync(queueData, directoryPath, templatePath).Result;
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSLogExportFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSLogExportFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("SMSFailedQueueProcessingFunction")]
        public static void SMSFailedQueueProcessingFunction([QueueTrigger("smsfailedqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# SMSFailedQueueProcessingFunction Queue trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var smsService = scope.Resolve<ISMSService>();
                    smsService.ProcessSMSFailedQueueAsync(queueData, log).Wait();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString(), e, $" SMSFailedQueueProcessingFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("SMSUnConfirmedStatusUpdateFunction")]
        public static void SMSUnConfirmedStatusUpdateFunction([TimerTrigger("%SMSUnConfirmedStatusUpdateFunction%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            log.LogInformation($" SMSUnConfirmedStatusUpdateFunction Started at: {DateTime.UtcNow}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {

                    var weChatifyService = scope.Resolve<IWeChatifyService>();
                    var wechatAccounts = weChatifyService.GetAllSMSMappedSFAccountsAsync().Result;

                    var smsService = scope.Resolve<ISMSService>();

                    var dateFrom = DateTime.UtcNow.ToChinaTime().AddDays(-2);

                    foreach (var accountModel in wechatAccounts)
                    {
                        smsService.UpdateUnconfirmedStatusAsync(accountModel.AccountId, dateFrom).Wait();
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSUnConfirmedStatusUpdateFunction", "TimeTriggerFunction", context, e);
                log.LogError(e.ToString(), e, $" SMSUnConfirmedStatusUpdateFunction Error occured at: {DateTime.UtcNow}");
            }

            log.LogInformation($" SMSUnConfirmedStatusUpdateFunction Completed at: {DateTime.UtcNow}");


        }

        [FunctionName("SMSExportJourneyFunction")]
        public static void SMSExportJourneyFunction([QueueTrigger("smsjourneyexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "JouneyEmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.ExportJourneysAsync(queueData, directoryPath, templatePath, log).Result;
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSExportJourneyFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" SMSExportJourneyFunction Error occured at: {DateTime.UtcNow}");
            }
        }


        [FunctionName("SMSDeliveryReportFunction")]
        public static void SMSDeliveryReportFunction([TimerTrigger("%SMSDeliveryReportFunction%")]TimerInfo info, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string[] hkAccounts = AppSettings.GetValue("HKAccounts").Split(',');
            string toDeliveryReport = AppSettings.GetValue("ToDeliveryReport");
            string toShiseidoReport = AppSettings.GetValue("ToShiseidoReport") ?? "";
            string toShiseidoReportInternal = AppSettings.GetValue("ToShiseidoReportInternal") ?? "";
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "SMSDeliveryReportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var smsService = scope.Resolve<ISMSService>();

                    var result = smsService.SendDeliveryReportAsync(directoryPath, templatePath).Result;

                    smsService.SendDeliveryReportToShiseidoHKAsync(hkAccounts, toShiseidoReport.Split(','), directoryPath, templatePath).Wait();
                    smsService.SendDeliveryReportToShiseidoHKInternalAsync(hkAccounts, toShiseidoReportInternal.Split(','), directoryPath, templatePath).Wait();

                    smsService.ReprocessQueueAsync("smslogupdatefailedqueue");
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSDeliveryReportFunction", "SMS Delivery Report Timer trigger function ", context, e);
                log.LogError(e.ToString(), e, $" SMSDeliveryReportFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("SMSThresholdNotificationFunction")]
        public static void SMSThresholdNotificationFunction([TimerTrigger("%SMSThresholdNotificationFunction%")]TimerInfo info, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "SMSThresholdNotificationTemplate.htm");
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.SendSMSInventoryAndThresholdNotificationAsync(templatePath).Result;

                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSThresholdNotificationFunction", "SMSThreshold Notification Timer trigger function ", context, e);
                log.LogError(e.ToString(), e, $" SMSThresholdNotificationFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("JourneyCountUpdateFunction")]
        public static void JourneyCountUpdateFunction([TimerTrigger("%JourneyCountUpdateFunction%")]TimerInfo info, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var smsService = scope.Resolve<ISMSService>();
                    var journeys = smsService.GetLastDayRanJourneysAsync().Result;

                    foreach (var journey in journeys)
                    {
                        try
                        {
                            smsService.UpdateSMSLogCountInJourneyAsync(journey).Wait();
                        }
                        catch (Exception e)
                        {
                            //log.LogError(e.ToString(), e, $" JourneyCountUpdateFunction Error occured at: {DateTime.UtcNow}");
                        }

                    }

                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("JourneyCountUpdateFunction", "JourneyCountUpdateFunction Timer trigger function ", context, e);
                log.LogError(e.ToString(), e, $" JourneyCountUpdateFunction Error occured at: {DateTime.UtcNow}");
            }
        }


        [FunctionName("SMSLogUpdateToDEFunction")]
        public static void SMSLogUpdateToDEFunction([TimerTrigger("%SMSLogUpdateToDEFunction%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            log.LogInformation($" SMSLogUpdateToDEFunction Started at: {DateTime.UtcNow}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {

                    var dataExtensionService = scope.Resolve<ISMSDataExtensionService>();

                        try
                        {
                            dataExtensionService.UpdateSMSLogToDataExtensionAsync(log).Wait();
                        }
                        catch (Exception e)
                        {
                            log.LogError($" SMSLogUpdateToDEFunction - SMSLog Error occured at: " + DateTime.UtcNow + "Exception : " + e.Message);
                        }
                        try
                        {
                            dataExtensionService.UpdateIncomingSMSLogToDataExtensionAsync(log).Wait();
                        }
                        catch (Exception e)
                        {
                            log.LogError($" SMSLogUpdateToDEFunction - IncomingSMS Error occured at: " + DateTime.UtcNow + "Exception : " + e.Message);
                        }
                    
                }
            }
            catch (AggregateException agg)
            {
                ExceptionEmailAlert("SMSLogUpdateToDEFunction", "TimeTriggerFunction", context, agg);
                log.LogError(agg.ToString(), agg, $" SMSLogUpdateToDEFunction Error occured at: {DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSLogUpdateToDEFunction", "TimeTriggerFunction", context, e);
                log.LogError(e.ToString(), e, $" SMSLogUpdateToDEFunction Error occured at: {DateTime.UtcNow}");
            }

            log.LogInformation($" SMSLogUpdateToDEFunction Completed at: {DateTime.UtcNow}");


        }

        [FunctionName("SMSUpdateJourneyFromSFFunction")]
        public static void SMSUpdateJourneyFromSFFunction([QueueTrigger("smsupdatejourneyfromsfqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"SMSUpdateJourneyFromSF Queue trigger function Started at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.AddOrUpdateJourneyFromSFAsync(queueData).Result;
                    if (result)
                    {
                        smsService.ProcessNullLogsAsync(queueData).Wait();
                    }
                }
            }
            catch (AggregateException e)
            {
                ExceptionEmailAlert("SMSUpdateJourneyFromSFFunction", queueData, context, e);
                log.LogError($"SMSUpdateJourneyFromSFFunction Error occured at: " + DateTime.UtcNow + "Exception: " + e.ToString());
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSUpdateJourneyFromSFFunction", queueData, context, e);
                log.LogError($"SMSUpdateJourneyFromSFFunction Error occured at: " + DateTime.UtcNow + "Exception: " + e.ToString());
            }
        }

        [FunctionName("SMSMonthlyUsageUpdateFunction")]
        public static void SMSMonthlyUsageUpdateFunction([TimerTrigger("%SMSMonthlyUsageUpdateFunction%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            log.LogInformation($"SMSMonthlyUsageUpdateFunction Started at: {DateTime.UtcNow}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    DateTime currentChinaTime = DateTime.UtcNow.ToChinaTime();
                    DateTime startDate = currentChinaTime.Date.AddMonths(-1);
                    DateTime endDate = startDate.Date.AddMonths(1);

                    var smsService = scope.Resolve<ISMSService>();
                    smsService.UpdateSMSMonthlyUsageAsync(startDate, endDate).Wait();
                }
            }
            catch (AggregateException agg)
            {
                ExceptionEmailAlert("SMSMonthlyUsageUpdateFunction", "TimeTriggerFunction", context, agg);
                log.LogError($"SMSMonthlyUsageUpdateFunction Error occured at: " + DateTime.UtcNow + "Exception: " + agg.ToString());
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("SMSMonthlyUsageUpdateFunction", "TimeTriggerFunction", context, e);
                log.LogError($"SMSMonthlyUsageUpdateFunction Error occured at: " + DateTime.UtcNow + "Exception: " + e.ToString());
            }

            log.LogInformation($"SMSMonthlyUsageUpdateFunction Completed at: {DateTime.UtcNow}");

        }

        private static void ExceptionEmailAlert(string functionName, string queuedExport, ExecutionContext context, Exception e)
        {
            using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
            {
                string templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates", "ExceptionAlertTemplate.htm");
                var emailService = scope.Resolve<IEmailService>();
                emailService.SendExceptionAlertEmail(functionName, queuedExport, e, templatePath).Wait();
            }
        }
        private static void ExceptionEmailAlert(string functionName, string queuedExport, ExecutionContext context, AggregateException e)
        {
            using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
            {
                string templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates", "ExceptionAlertTemplate.htm");
                var emailService = scope.Resolve<IEmailService>();
                emailService.SendExceptionAlertEmail(functionName, queuedExport, e, templatePath).Wait();
            }
        }

    }
}
