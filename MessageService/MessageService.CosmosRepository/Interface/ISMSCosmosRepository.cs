using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.Enum;
using MessageService.Models.ExportModels;
using MessageService.Models.SMSModels;
using MessageService.Models.StoredProcedureModels;

namespace MessageService.CosmosRepository.Interface
{
    public interface ISMSCosmosRepository
    {
        Task<bool> UpsertSMSUsageAsync(SMSUsageDocumentModel model);
        Task<bool> AddOrUpdateActivityInfoAsync(List<ActivityInfoDocumentModel> models);
        Task<bool> UpsertDocument(object document);
        Task<bool> UpdateVerificationSMSAsync(SMSLogDocumentModel model);
        Task<bool> UpdateUnConfirmedStatusAsync(List<SMSLogDocumentModel> model);
        Task<List<VerificationSMSDocumentModel>> GetVerificationSMSAsync(VerificationSMSExportModel model);
        Task<(List<IncomingMessageDocumentModel>, int)> GetIncomingMessagesAsync(
            GetIncomingMessagesModel model);
        Task<(List<VerificationSMSDocumentModel>, int)> GetVerificationSMSAsync(GetVerificationSMSModel model);
        Task<bool> SaveIncomingMessageAsync(IncomingMessageDocumentModel incomingMessage);
        Task<bool> SaveVerificationSMSAsync(VerificationSMSDocumentModel verificationSMS);
        Task<List<IncomingMessageDocumentModel>> GetIncomingMessagesAsync(IncomingMessagesExportModel model,
            bool isOptOut);
        Task<List<IncomingMessageDocumentModel>> GetIncomingMessagesToUpdateDEAsync(long accountId);
        Task<bool> UpdateDataExtensionPushInIncomingMessageAsync(List<IncomingMessageDocumentModel> logModels);
        Task<bool> AddOrUpdateJourneyDetailsAsync(JourneyProcModel journeyProcModel);
        Task<bool> AddOrUpdateJourneyInfoAsync(JourneyInfoDocumentModel journeyProcModel);
        Task<List<JourneyInfoDocumentModel>> GetJourneysAsync(JourneyExportModel model);
        Task<JourneyInfoDocumentModel> GetJourneyAsync(string id);
        Task<string> GetJourneyIdAsync(string interactionId);
        Task<bool> InsertSMSLogAsync(SMSLogDocumentModel logModel);
        Task<string> GetJourneyNameByMobileNumberAsync(string mobileNumber, long accountId);
        Task<SMSLogDocumentModel> UpdateDeliveryReportInSMSLogAsync(SMSLogDocumentModel procModel);
        Task<(List<JourneyInfoDocumentModel>, List<JourneyInfoDocumentModel>, int)> GetJourneysAsync(JourneyFilterModel logFilterModel);
        Task<List<VersionDropDownModel>> GetVersionsAsync(long accountId, string journeyId);
        Task<List<ActivityInfoDocumentModel>> GetActivitiesAsync(long accountId, string journeyId);
        Task<(List<LogViewModel>, int)> GetSMSLogAsync(long accountId, string journeyId);
        Task<List<JourneyInfoDocumentModel>> GetJourneysAsync(long accountId);
        Task<List<LogViewModel>> GetSMSLogAsync(LogFilterModel logFilter);
        Task<(List<LogViewModel>, int)> GetSMSLogWithPagingAsync(LogFilterModel logFilter);
        //Task<List<SMSLogDocumentModel>> GetSMSLogAsync(long accountId, DateTime dateFrom);
        Task<List<ActivityInfoDocumentModel>> GetActivitiesAsync(long accountId, DateTime dateFrom);
        Task<int> GetSMSLogCountBySendStatusAsync(long accountId, string journeyId, DateTime dateFrom, SendStatus sendStatus);
        Task<int> GetSMSLogCountBySendStatusAsync(string journeyId, SendStatus sendStatus, long? accountId = null);
        Task<int> GetSMSLogCountByDeliveryStatusAsync(long accountId, string journeyId, DateTime dateFrom, DeliveryStatus deliveryStatus);
        Task<int> GetSMSLogCountByDeliveryStatusAsync(string journeyId, DeliveryStatus deliveryStatus, long? accountId = null);
        Task<List<SMSLogDocumentModel>> GetSMSLogByDeliveryStatusAsync(long accountId, DateTime dateFrom,
            DeliveryStatus status);
        Task<List<SMSLogDocumentModel>> GetSMSLogBySendStatusAsync(long accountId, DateTime dateFrom,
            SendStatus status);
        Task<int> GetSMSLogCountAsync(long accountId, string journeyId, DateTime dateFrom);        
        Task<int> GetSMSLogCountAsync(string journeyId, long? accountId = null);
        Task<List<SMSLogDocumentModel>> GetSmsLogForUpdateToDEAsync(long accountId);
        Task<List<JourneyInfoDocumentModel>> GetJourneysAsync(List<string> journeyIds);
        Task<bool> UpdateDataExtensionPushInSMSLogAsync(List<SMSLogDocumentModel> logModels);
        Task CustomQueryExecutorAsync(long accountId);
        Task<List<SMSLogDocumentModel>> GetPendingStatusSMSAsync(long accountId, DateTime fromDate);
        
        Task<List<ActivityInfoDocumentModel>> GetActivitiesAsync(long accountId, List<string> activityIds);
        Task<List<string>> GetDistinctActivitiesAsync(long accountId, DateTime dateFrom);
        Task<List<string>> GetDistinctJourneyIdsAsync(DateTime dateFrom);
        Task<SMSLogDocumentModel> GetFirstSMSLogAsync(long accountId, string journeyId, DateTime dateFrom);

        Task<List<SMSLogDocumentModel>> GetSMSLogForExportAsync(JourneyExportModel model, string journeyId);
        Task<List<JourneyInfoDocumentModel>> GetJourneysListAsync(long accountId, string[] journeyId);
        Task<List<ActivityInfoDocumentModel>> GetActivityListAsync(long AccountId, string[] ids);
        Task<List<InteractionInfoDocumentModel>> GetVersionsListByIdsAsync(long accountId, string[] ids);
        Task ProcessNullLogsAsync(SMSSFInteractionModel smsSFInteractionModel);
       
        Task<ActivityInfoDocumentModel> GetActivityAsync(string id);
        Task<int> GetSMSUsedCountAsync(long accountId, DateTime startDate, DateTime dateTime);
        Task<List<SMSUsageDocumentModel>> GetSMSMonthlyUsageAsync(long accountId, int year);
        Task<List<int>> GetSMSMonthlyUsageYearsAsync(long accountId);

        Task<List<string>> GetDistinctJourneyForExportAsync(JourneyExportModel model);
    }
}
