using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Models.SubmailModel;
using MessageService.Models.WeChatifyModels;

namespace MessageService.Service.Interface
{
    public interface ISettingsService
    {
        Task<int> AddOrUpdateMMSVendorSettingsAsync(VendorSettingsModel model);
        Task<VendorSettingsModel> GetMMSVendorSettingsAsync(long accountid);
        Task<bool> CheckMMSVendorSettingsExistsAsync(long accountid);
        Task<bool> IsMMSVendorSettingsValidAsync(VendorSettingsModel model);
        Task<bool> AddOrUpdateSMSVendorSettingsAsync(SMSVendorSettingsDocumentModel model);
        Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(long accountid);
        Task<bool> CheckSMSVendorSettingsExistsAsync(long accountid);
        Task<bool> IsSMSVendorSettingsValidAsync(SMSVendorSettingsDocumentModel model);
        Task<string> GetErrorDescriptionAsync(string errorCode, string language=LanguageType.English);
        Task<string> GetSMSErrorDescriptionAsync(string errorCode, string language = LanguageType.English);
        Task<IEnumerable<CategoryDocumentModel>> GetCategoriesAsync();
        Task<bool> AddCategoryAsync(CategoryDocumentModel model);
        Task<bool> IsCategoryValidAsync(CategoryDocumentModel model);
        Task<bool> AddOrUpdateInventoryAndAlertSettingAsync(InventoryAndAlertSettingDocumentModel model);
        Task<InventoryAndAlertSettingDocumentModel> GetInventoryAndAlertSettingAsync(long accountId);
        Task<InventoryAndAlertSettingDocumentModel> AddUserToInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails);
        Task<InventoryAndAlertSettingDocumentModel> RemoveUserFromInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails);
        Task<List<WeChatifyUserModel>> GetUserToAddInventoryAndAlertSettingAsync(long accountId);
        Task<List<RechargeHistoryModel>> GetSMSRechargeHistoryAsync(long accountId);

        Task<bool> AddTopUpRequestAsync(TopUpRequestDocumentModel model);
        Task<bool> UpdateTopUpRequestAsync(TopUpRequestDocumentModel model);
        Task<PagingModel<TopUpRequestDocumentModel>> GetAllTopUpRequestAsync(TopUpHistoryFilterModel model);
        Task<TopUpRequestDocumentModel> GetPendingTopUpRequestAsync(long accountId);
        Task<Dictionary<string, List<string>>> GetCategoryMappingAsync();

        Task<string> AddOrUpdateSMSDeliveryReportNotificationAsync(DeliveryReportNotificationDocumentModel model);
        Task<DeliveryReportNotificationDocumentModel> GetSMSDeliveryReportNotificationAsync(long accountId);

        Task<DeliveryReportNotificationDocumentModel> AddUserToSMSDeliveryReportNotificationAsync(long accountId,
            List<EmailNotificationUserDocumentModel> userDetails);

        Task<DeliveryReportNotificationDocumentModel> RemoveUserFromSMSDeliveryReportNotificationAsync(long accountId,
            List<EmailNotificationUserDocumentModel> userDetails);

        Task<List<WeChatifyUserModel>> GetUserToAddDeliveryReportSettingAsync(long accountId);
        #region MMS Setting
        Task<int> InsertMMSNotificationUsers(NotifyUser param);
        Task<int> DeleteMMSNotificationUsers(NotifyUser param);
        Task<int> InsertOrUpdateMMSDeliveryReportNotification(MMSDeliveryReportNotification model);
        Task<int> InsertOrUpdateMMSBalanceThreshold(MMSBalanceThreshold model);
        Task<List<NotifyUserList>> GetDeliveryReportNotificationUsers(FilterParam param);
        Task<List<NotifyUserList>> GetMMSBalanceThreshold(FilterParam param);
        Task<MMSBalanceThreshold> GetMMSBalanceThresholdByAccountId(long accountId);
        Task<MMSDeliveryReportNotification> GetMMSDeliveryReportNotificationSettingByAccountId(long accountId);
        Task<List<string>> GetMMSNotificationUsers(long accountId, NotificationType Type);
        #endregion
    }
}
