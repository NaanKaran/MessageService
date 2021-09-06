using System;
using System.IO;
using Autofac;
using MessageService.InfraStructure.Helpers;
using MessageService.Service.Interface;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MMS.QueueAndTimeTriggerFunction
{
    public static class MMSFunctions
    {
        [FunctionName("MMSLogAddFunction")]
        public static void MMSLogAddFunction([QueueTrigger("mmslogaddqueue", Connection = "AzureWebJobsStorage")]string queueforLog, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {

                    var mmsService = scope.Resolve<IMMSService>();
                    var result = mmsService.AddMmsLogAsync(queueforLog).Result;
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString(), e, $" MMSLogAddFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("MMSLogUpdateFunction")]
        public static void MMSLogUpdateFunction([QueueTrigger("mmslogupdatequeue", Connection = "AzureWebJobsStorage")]string queueforLog, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var mmsService = scope.Resolve<IMMSService>();
                    var result = mmsService.UpdateMmsLogAsync(queueforLog).Result;
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString(), e, $" MMSLogUpdateFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("MMSExportIncomingMessagesFunction")]
        public static void MMSExportIncomingMessagesFunction([QueueTrigger("mmsincomingmessagesexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "EmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var mmsService = scope.Resolve<IMMSService>();
                    var result = mmsService.ExportIncomingMessagesAsync(false, queueData, directoryPath, templatePath).Result;
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSExportIncomingMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" MMSExportIncomingMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("MMSExportOptOutMessagesFunction")]
        public static void MMSExportOptOutMessagesFunction([QueueTrigger("mmsoptoutmessagesexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "EmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var mmsService = scope.Resolve<IMMSService>();
                    var result = mmsService.ExportIncomingMessagesAsync(true, queueData, directoryPath, templatePath).Result;
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSExportOptOutMessagesFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" MMSExportOptOutMessagesFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("MMSLogExportFunction")]
        public static void MMSLogExportFunction([QueueTrigger("mmslogexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "JouneyEmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var mmsService = scope.Resolve<IMMSService>();
                    var result = mmsService.ExportMMSLogAsync(queueData, directoryPath, templatePath).Result;
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSExportLogFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" MMSExportLogFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("MMSExportJourneyFunction")]
        public static void MMSExportJourneyFunction([QueueTrigger("mmsjouneyexportqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates",
                        "JouneyEmailExportTemplate.htm");
                    var directoryPath = context.FunctionAppDirectory;
                    var mmsService = scope.Resolve<IMMSService>();
                    var result = mmsService.ExportJourneysAsync(queueData, directoryPath, templatePath).Result;
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSExportJourneyFunction", queueData, context, e);
                log.LogError(e.ToString(), e, $" MMSExportJourneyFunction Error occured at: {DateTime.UtcNow}");
            }
        }

        [FunctionName("MMSUnConfirmedStatusUpdateFunction")]
        public static void MMSUnConfirmedStatusUpdateFunction([TimerTrigger("%MMSUnConfirmedStatusUpdateFunction%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            log.LogInformation($" MMSUnConfirmedUpdateFunction Started at: {DateTime.UtcNow}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {                   
                    var mmsService = scope.Resolve<IMMSService>();
                    var wechatAccounts = mmsService.GetAllMmsMappedSFAccountsAsync().Result;
                    var dateFrom = DateTime.UtcNow.ToChinaTime().AddDays(-2);

                    foreach (var accountModel in wechatAccounts)
                    {
                        mmsService.UpdateUnconfirmedStatusAsync(accountModel.AccountId, dateFrom).Wait();
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSExportIncomingMessagesFunction", "TimeTriggerFunction", context, e);
                log.LogError(e.ToString(), e, $" MMSUnConfirmedStatusUpdateFunction Error occured at: {DateTime.UtcNow}");
            }

            log.LogInformation($" MMSUnConfirmedStatusUpdateFunction Completed at: {DateTime.UtcNow}");


        }


        [FunctionName("MMSLogUpdateToDEFunction")]
        public static void MMSLogUpdateToDEFunction([TimerTrigger("%MMSLogUpdateToDEFunction%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            log.LogInformation($" MMSLogUpdateToDEFunction Started at: {DateTime.UtcNow}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {                  
                    var dataExtensionService = scope.Resolve<IMMSDataExtensionService>();
                    var mmsService = scope.Resolve<IMMSService>();
                    var wechatAccounts = mmsService.GetAllMmsMappedSFAccountsAsync().Result;
                    foreach (var accountModel in wechatAccounts)
                    {
                        try
                        {
                            log.LogInformation($"{accountModel.AccountId} - {accountModel.AccountName} UpdateMmsLogToDataExtension Started");
                            dataExtensionService.UpdateMmsLogToDataExtension(accountModel.AccountId).Wait();
                            log.LogInformation($"{accountModel.AccountId} - {accountModel.AccountName} UpdateMmsLogToDataExtension Completed");
                        }
                        catch (Exception e)
                        {
                            log.LogError($"MMSLogUpdateToDEFunction - MMSLog Error occured at: {DateTime.UtcNow} Exception : {e}");
                        }
                        try
                        {
                            log.LogInformation($"{accountModel.AccountId} - {accountModel.AccountName} UpdateIncomingMmsLogToDataExtension Started");
                            dataExtensionService.UpdateIncomingMmsLogToDataExtension(accountModel.AccountId).Wait();
                            log.LogInformation($"{accountModel.AccountId} - {accountModel.AccountName} UpdateIncomingMmsLogToDataExtension Completed");
                        }
                        catch (Exception e)
                        {
                            log.LogError($"MMSLogUpdateToDEFunction - IncomingMessage Error occured at: {DateTime.UtcNow} Exception : {e}");
                        }
                        try
                        {
                            mmsService.ReprocessQueueAsync("mmslogupdatefailedqueue");
                        }
                        catch (Exception e)
                        {
                            log.LogError($"MMSLogUpdateToDEFunction - mmslogupdatefailedqueue Error occured at: {DateTime.UtcNow} Exception : {e}");
                        }
                    }
                }
            }
            catch (AggregateException agg)
            {
                ExceptionEmailAlert("MMSLogUpdateToDEFunction", "TimeTriggerFunction", context, agg);
                log.LogError(agg.ToString(), agg, $" MMSLogUpdateToDEFunction Error occured at: {DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSLogUpdateToDEFunction", "TimeTriggerFunction", context, e);
                log.LogError(e.ToString(), e, $" MMSLogUpdateToDEFunction Error occured at: {DateTime.UtcNow}");
            }

            log.LogInformation($" MMSLogUpdateToDEFunction Completed at: {DateTime.UtcNow}");


        }


        #region MMS Setting
        [FunctionName("MMSDeliveryStatAndThreshold")]
        public static void MMSEmailNotificationForDeliveryStatusAndThreshold([TimerTrigger("%MMSDeliveryStatAndThreshold%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
            {
                var mmsService = scope.Resolve<IMMSService>();
                string templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates", "MMSDeliveryReport.htm");
                string xlsPath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates", "MMS_DeliveryReport.xlsx");
                mmsService.SendMailDeliveryRateBelowPercent(templatePath, xlsPath).ConfigureAwait(true);
            }
        }
        [FunctionName("MMSNotificationForBalanceThreshold")]
        public static void MMSEmailNotificationForBalanceThreshold([TimerTrigger("%MMSNotificationForBalanceThreshold%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
            {
                var mmsService = scope.Resolve<IMMSService>();
                string templatePath = Path.Combine(context.FunctionAppDirectory, "EmailTemplates", "MMSBalanceThresholdTemplate.htm");
                mmsService.SendEmailForMMSBalanceThreshold(templatePath).ConfigureAwait(true);
            }
        }
        #endregion

        [FunctionName("MMSTableAndProcCreateFunction")]
        public static void MMSTableAndProcCreateFunction([TimerTrigger("%MMSTableAndProcCreateFunction%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {

            log.LogInformation($" MMSTableAndProcCreateFunction Started at: {DateTime.UtcNow}");
            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var mmsService = scope.Resolve<IMMSService>();
                    var nextMonth = DateTime.UtcNow.ToChinaTime().AddDays(30);
                    mmsService.CreateMmsTableAsync(nextMonth).Wait();
                    mmsService.CreateMMSStoredProcedureAsync(nextMonth).Wait();
                    mmsService.AddEntryInMMSLogTableInfoAsync().Wait();
                }
            }
            catch (Exception e)
            {
                ExceptionEmailAlert("MMSTableAndProcCreateFunction", "TimeTriggerFunction", context, e);
                log.LogError(e.ToString(), e, $" MMSTableAndProcCreateFunction Error occured at: {DateTime.UtcNow}");
            }

            log.LogInformation($" MMSTableAndProcCreateFunction Completed at: {DateTime.UtcNow}");
        }



        [FunctionName("MMSUsageDetailUpdate")]
        public static void MMSUsageDetailUpdate([TimerTrigger("%MMSUsageDetailUpdate%")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
            {
                var mmsService = scope.Resolve<IMMSService>();

                var wechatAccounts = mmsService.GetAllMmsMappedSFAccountsAsync().Result;
                foreach (var accountModel in wechatAccounts)
                {
                    mmsService.MMSUsageCountUpdateAsync(accountModel.AccountId).Wait();
                }
            }
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
