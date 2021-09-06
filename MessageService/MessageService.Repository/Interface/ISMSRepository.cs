using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.ExportModels;
using MessageService.Models.StoredProcedureModels;
using MessageService.Models.SubmailModel;

namespace MessageService.Repository.Interface
{
    public interface ISMSRepository
    {     
        Task<(IEnumerable<IncomingMessageModel>, int)> GetIncomingMessagesAsync(GetIncomingMessagesModel model);
        Task<bool> SaveIncomingMessageAsync(IncomingMessageProcModel incomingMessage);     
        Task<IEnumerable<IncomingMessageModel>> GetIncomingMessagesAsync(IncomingMessagesExportModel model, bool isOptOut);      
        Task<List<IncomingMessageModel>> GetIncomingMessagesToUpdateDEAsync(long accountId);
        Task<bool> UpdateDataExtensionPushInIncomingMessageAsync(IList<WeChatifyDataExtensionIncomingLog> logModels);
        Task<bool> SaveJourneyAsync(JourneyProcModel journeyProcModel);
        Task<IEnumerable<JourneyInfoModel>> GetAllJourneysAsync(JourneyExportModel model);
        Task<string> GetJourneyIdAsync(string interactionId);
        Task<bool> SaveSMSLogAsync(SMSLogInsertProcModel logModel, string storedProcedure);
        Task<bool> UpdateCountInJourneyAsync(SMSLogModel logModel);
        Task CreateQuadrantObjectsAsync(string sql);
        Task<string> GetJourneyNameByMobileNumberAsync(string mobileNumber);
        Task<bool> UpdateDeliveryReportInSMSLogAsync(SMSLogUpdateProcModel procModel);
        Task<bool> UpdateDeliveryCountInJourneyAsync(SubmailStatusPushModel statusPushModel);
        Task<(IEnumerable<JourneyInfoModel>, int)> GetJourneysAsync(JourneyFilterModel logFilterModel);
        Task<IEnumerable<VersionDropDownModel>> GetVersionsAsync(string journeyId, string quadrant);
        Task<IEnumerable<ActivityDropDownModel>> GetActivitiesAsync(string journeyId, string quadrant);
        Task<IEnumerable<QuadrantDropDownModel>> GetQuadrantsAsync();
        Task<(IEnumerable<LogViewModel>, int)> GetSMSLogForCurrentQuadrantAsync(long accountId, string journeyId);
        Task<bool> AddSMSLogTableInfo(SMSLogTableInfoModel logTableInfoModel);
        Task<IEnumerable<JourneyInfoModel>> GetAllJourneyAsync(long accountId);
        Task<IEnumerable<LogViewModel>> GetSMSLogAsync(LogFilterModel logFilter);
        Task<SMSContentModel> GetSMSContentAsync(string sendId);
        Task<(IEnumerable<LogViewModel>, int)> GetSMSLogByQuadrantAsync(LogFilterModel logFilter);        
    }
}
