using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;

namespace MessageService.Repository.Interface
{
    public interface ISettingsRepository
    {
        Task<int> AddOrUpdateMMSVendorSettingsAsync(VendorSettingsModel model);
        Task<VendorSettingsModel> GetMMSVendorSettingsAsync(long accountid);
        Task<VendorSettingsModel> GetMMSVendorSettingsAsync(string appId);
        Task<List<VendorSettingsModel>> GetAllMMSVendorSettingsAsync();
        Task<bool> CheckMMSVendorSettingsExistsAsync(long accountid);
        Task<bool> IsMMSVendorSettingsValidAsync(VendorSettingsModel model);
        Task<IEnumerable<long>> GetMMSMappedAccountIdsAsync();
        Task<string> GetErrorDescriptionAsync(string errorCode, string language = LanguageType.English);
        Task<List<MMSErrorCodeModel>> GetErrorDescriptionAsync();

        #region MMS Setting
        Task<int> InsertMMSNotificationUsers(NotifyUser param);
        Task<int> DeleteMMSNotificationUsers(NotifyUser param);
        Task<int> InsertOrUpdateMMSDeliveryReportNotification(MMSDeliveryReportNotification model);
        Task<List<NotifyUserList>> InsertOrUpdateMMSBalanceThreshold(MMSBalanceThreshold model);
        Task<MMSBalanceThreshold> GetMMSBalanceThresholdByAccountId(long accountId);
        Task<MMSDeliveryReportNotification> GetMMSDeliveryReportNotificationSettingByAccountId(long accountId);
        Task<List<NotifyUserList>> GetDeliveryReportNotificationUsers(FilterParam param);
        Task<List<NotifyUserList>> GetMMSBalanceThreshold(FilterParam param);
        Task<List<NotifyUserList>> GetNotificationToUsers(long accountId, NotificationType Type);
        Task<List<string>> GetMMSNotificationUsers(long accountId, NotificationType Type);        
        #endregion
    }
}
