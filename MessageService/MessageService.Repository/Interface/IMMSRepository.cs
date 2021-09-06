using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.ExportModels;
using MessageService.Models.MMSModels;
using MessageService.Models.StoredProcedureModels;
using MessageService.Models.SubmailModel;

namespace MessageService.Repository.Interface
{
    public interface IMMSRepository
    {
        Task<bool> SaveMmsLogAsync(MMSLogModel mmsLog);
        Task<(IEnumerable<IncomingMessageModel>, int)> GetIncomingMessagesAsync(GetIncomingMessagesModel model);
        Task<bool> UpdateDeliveryReportInMMSLogAsync(MMSLogModel mmsLog);
        Task<bool> UpdateDeliveryReportInMMSLogAsync(List<MMSLogModel> mmsLogs);
        Task<bool> SaveIncomingMessageAsync(IncomingMessageModel incomingMessage);
        Task<string> GetJourneyNameByMobileNumberAsync(string mobileNumber);
        Task<bool> UpdateCountInJourneyAsync(MMSLogModel logModel);
        Task<bool> UpdateDeliveryCountInJourneyAsync(SubmailStatusPushModel report);
        Task<bool> AddJourneyEntryAsync(JourneyActivateModel journey);
        Task<IEnumerable<JourneyInfoModel>> GetAllJourneyAsync(long accountId);
        Task<IEnumerable<VersionDropDownModel>> GetVersionsAsync(string journeyId, string quadrant);
        Task<IEnumerable<ActivityDropDownModel>> GetActivitiesAsync(string journeyId, string quadrant);
        Task<IEnumerable<QuadrantDropDownModel>> GetQuadrantsAsync();
        Task<(IEnumerable<LogViewModel>, int)> GetMmsLogForCurrentQuadrantAsync(long accountId, string journeyId, string quadrantTableName);
        Task<(IEnumerable<LogViewModel>, int)> GetMmsLogByQuadrantAsync(LogFilterModel logFilter);
        Task<IEnumerable<IncomingMessageModel>> GetIncomingMessagesAsync(IncomingMessagesExportModel model, bool isOptOut);
        Task<(IEnumerable<JourneyInfoModel>, int)> GetJourneysAsync(JourneyFilterModel logFilterModel);
        Task<bool> AddEntryToMmsLogTableInfoAsync(MMSLogTableInfoModel logTableInfoModel);
        Task<string> GetJourneyIdAsync(string interactionId);
        Task<IEnumerable<JourneyInfoModel>> GetAllJourneysAsync(JourneyExportModel model);
        Task<IEnumerable<LogViewModel>> GetMmsLogAsync(LogFilterModel logFilter);
        Task<IList<MMSLogModel>> GetPendingStatusMMSAsync(long accountId, DateTime fromDate);
        Task<IList<LogViewModel>> GetMmsLogForUpdateToDEAsync(long accountId);
        Task<bool> UpdateDataExtensionPushInMMSLogAsync(IList<WeChatifyDataExtensionMMSLog> logModels);
        Task<List<IncomingMessageModel>> GetIncomingMessagesToUpdateDEAsync(long accountId);
        Task<bool> UpdateDataExtensionPushInIncomingMessageAsync(IList<WeChatifyDataExtensionIncomingLog> logModels);
        Task<bool> SaveJourneyAsync(JourneyProcModel journeyProcModel);
        Task CreateQuadrantObjectsAsync(string sql);
        Task<JourneysPieChart> GetJourneysMMSCountAsync(JourneyFilterModel logFilterModel);
        Task<List<MMSActivityInfoModel>> GetReports(long accountId, string filterDate);
        Task<List<DropReasonModel>> GetMMSLogForDroppedReason(long accountId, string filterDate, string neededQuadTable);
        Task<List<MMSLogModel>> GetMMSLogByDate(long accountId, string filterDate, string neededQuadTable);
        Task<int> GetUsedMMSCountAsync(long accountId, DateTime fromDate, DateTime toDate);
        Task<List<MMSUsageModel>> GetMMSUsageDetailAsync(long accountId, int year);
        Task<List<int>> GetMMSUsageYearsAsync(long accountId);
        Task<bool> AddOrUpdateMMSUsageDetailAsync(MMSUsageModel model);
    }
}
