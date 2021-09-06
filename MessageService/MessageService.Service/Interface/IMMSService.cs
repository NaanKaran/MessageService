using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.ExportModels;
using MessageService.Models.MMSModels;
using MessageService.Models.SubmailModel;
using MessageService.Models.WeChatifyModels;

namespace MessageService.Service.Interface
{
    public interface IMMSService
    {
        Task<bool> SendJourneyMMSAsync(string journeyData);
        Task SubmailStatusUpdateAddToQueueAsync(SubmailStatusPushModel statusPushModel);
        Task<PagingModel<IncomingMessageModel>> GetIncomingMessagesAsync(GetIncomingMessagesModel model);
        Task<bool> UpdateMmsLogAsync(string queueDataString);
        Task<MMSBalanceModel> GetMMSBalanceAsync(long accountid);
        Task<MMSBalanceModel> GetMMSTopupHistoryAsync(long accountId);
        Task<bool> AddJourneyEntryAsync(JourneyActivateModel journeyModel);
        Task AddQueueForIncomingMessagesExportAsync(EmailExportModel model);
        Task AddQueueForOptOutMessagesExportAsync(EmailExportModel model);
        Task<bool> ExportIncomingMessagesAsync(bool isOptout, string queueData, string directoryPath, string templatePath);
        Task<IEnumerable<JourneyInfoModel>> GetAllJourneyAsync(long accountId);
        Task<LogGridModel> GetJourneyDetailsAsync(long accountId, string journeyId, string quadrantTableName = null);
        Task<PagingModel<LogViewModel>> GetMmsLogDetailsAsync(LogFilterModel logFilter);
        Task<bool> AddMmsLogAsync(string queueDataString);
        Task<MMSJourneyInfoViewModel> GetJourneysAsync(JourneyFilterModel journeyFilter);
        Task AddQueueForJourneysExportAsync(EmailExportModel model);
        Task<bool> ExportJourneysAsync(string queueData, string directoryPath, string templatePath);
        Task AddQueueForMMSLogExportAsync(LogFilterModel logFilter);
        Task<bool> ExportMMSLogAsync(string queueData, string directoryPath, string templatePath);
        Task UpdateUnconfirmedStatusAsync(long accountId, DateTime fromDate);
        Task ReprocessQueueAsync(string queueName);
        Task<bool> UpdateTemplateStatusFromSubmailAsync(SubmailStatusPushModel statusPushModel);
        Task<IEnumerable<WeChatifySFAccountModel>> GetAllMmsMappedSFAccountsAsync();
        Task<IEnumerable<WeChatifySFAccountModel>> GetAllMmsMappedSFAccountsAsync(string id);
        Task CreateMmsTableAsync(DateTime onDateTime);
        Task CreateMMSStoredProcedureAsync(DateTime onDateTime);
        Task<bool> AddEntryInMMSLogTableInfoAsync();
        Task SendMailDeliveryRateBelowPercent(string templatePath, string xlsPath);
        Task SendEmailForMMSBalanceThreshold(string TemplatePath);

        Task MMSUsageCountUpdateAsync(long accountId);

        Task<MMSUsageViewModel> GetMmsUsageDetailAsync(long accountId, int? year);

        Task<List<MMSTopupHistoryModel>> GetMMSTopUpHistoryAsync(long accountId, int year, int month);

        Task MMSUsageCountUpdateAsync(long accountId, DateTime dateToCalculate);
    }
}
 