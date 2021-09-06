using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.SubmailModel;
using MessageService.Models.ExportModels;
using MessageService.Models.ViewModel;
using Microsoft.Extensions.Logging;

namespace MessageService.Service.Interface
{
    public interface ISMSService
    {
        Task UpdateSMSMonthlyUsageAsync(long accountId, DateTime startDate, DateTime endDate);
        Task<SMSUsageModel> GetSMSMonthlyUsageAsync(long accountId, int? year);
        Task<List<RechargeHistoryModel>> GetRechargeHistoryAsync(long accountId, int year, int month);
        Task<PagingModel<IncomingMessageDocumentModel>> GetIncomingMessagesAsync(GetIncomingMessagesModel model);
        Task<PagingModel<VerificationSMSDocumentModel>> GetVerificationSMSAsync(GetVerificationSMSModel model);
        Task<bool> ExportIncomingMessagesAsync(bool isOptout, string queueData, string templatePath, ILogger log);
        Task<bool> ExportVerificationSMSAsync(string queueData, string directoryPath, string templatePath);
        Task<SMSBalanceModel> GetSMSBalanceAsync(long accountid);        
        Task<bool> ExportJourneysAsync(string queueData, string directoryPath, string templatePath, ILogger log);       
        Task<JourneyInfoViewModel> GetJourneysAsync(JourneyFilterModel filterModel);
        Task<bool> AddJourneyEntryAsync(JourneyActivateModel journeyModel);
        Task<bool> SendJourneySMSAsync(string smsContent);
        Task<bool> UpdateSMSLogAsync(string queueDataString);
        Task<LogGridModel> GetJourneyDetailsAsync(long accountId, string journeyId);
        Task<IEnumerable<JourneyInfoDocumentModel>> GetAllJourneyAsync(long accountId);
        Task AddQueueForDeadLetterProcessingAsync(string json);
        Task AddQueueForIncomingMessagesExportAsync(EmailExportModel model);
        Task AddQueueForOptOutMessagesExportAsync(EmailExportModel model);
        Task AddQueueForSMSLogExportAsync(LogFilterModel logFilter);
        Task AddQueueForJourneysExportAsync(EmailExportModel model);
        Task<bool> ExportSMSLogAsync(string queueData, string directoryPath, string templatePath);
        Task<bool> PublishSMSEventsAsync(string journeyData);
        Task<LogGridModel> GetSMSLogDetailsAsync(LogFilterModel logFilter);
        Task ReprocessQueueAsync(string queueName);

        Task<bool> SendDeliveryReportAsync(string baseDirectoryPath, string templatePath);

        Task<bool> SendDeliveryReportToShiseidoHKAsync(string[] accountIds, string[] toEmailIds, string baseDirectoryPath,
            string templatePath);

        Task<bool> SendDeliveryReportToShiseidoHKInternalAsync(string[] accountIds, string[] toEmailIds, string baseDirectoryPath,
            string templatePath);
        SubmailResponseModel EmulateSubmailSMSXSend(string model);

        Task<bool> SendSMSInventoryAndThresholdNotificationAsync(string templatePath);
        Task<JourneyInfoDocumentModel> GetSMSLogCountInJourneyAsync(string journeyId);

        Task<List<JourneyInfoDocumentModel>> GetLastDayRanJourneysAsync();

        Task<JourneyInfoDocumentModel> UpdateSMSLogCountInJourneyAsync(JourneyInfoDocumentModel journeyInfo);
        Task<bool> ProcessSMSDeadLetterAsync(string queueData);
        Task<bool> ProcessSMSFailedQueueAsync(string queueData, ILogger log);
        Task CustomQueryExecutorAsync(long accountId);
        Task AddQueueForVerificationSMSExportAsync(EmailExportModel model);
        Task<bool> SendVerificationSMSAsync(VerificationSMSModel model);
        Task UpdateUnconfirmedStatusAsync(long accountId, DateTime fromDate);
        Task<bool> AddOrUpdateJourneyFromSFAsync(string queueData);
        Task ProcessNullLogsAsync(string queueData);
        Task<bool> ProcessSMSJourneyWithoutPersonalisation(string journeyData, ILogger log);
        Task UpdateSMSMonthlyUsageAsync(DateTime startDate, DateTime endDate);
    }
}
