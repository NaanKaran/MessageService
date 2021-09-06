using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.SMSModels;

namespace MessageService.CosmosRepository.Interface
{
    public interface ISettingsCosmosRepository
    {
        Task<bool> AddOrUpdateSMSVendorSettingsAsync(SMSVendorSettingsDocumentModel model);
        Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(long accountid);
        Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(string appId);
        Task<bool> CheckSMSVendorSettingsExistsAsync(long accountid);
        Task<bool> IsSMSVendorSettingsValidAsync(SMSVendorSettingsDocumentModel model);
        Task<List<CategoryDocumentModel>> GetCategoriesAsync();
        Task<bool> IsCategoryValidAsync(CategoryDocumentModel model);
        Task<bool> AddCategoryAsync(CategoryDocumentModel model);
        Task<bool> AddOrUpdateInventoryAndAlertSettingAsync(InventoryAndAlertSettingDocumentModel model);
        Task<InventoryAndAlertSettingDocumentModel> GetInventoryAndAlertSettingAsync(long accountId);
        Task<InventoryAndAlertSettingDocumentModel> AddUserToInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails);
        Task<InventoryAndAlertSettingDocumentModel> RemoveUserFromInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails);
        Task<bool> AddTopUpRequestAsync(TopUpRequestDocumentModel model);
        Task<bool> UpdateTopUpRequestAsync(TopUpRequestDocumentModel model);
        Task<(List<TopUpRequestDocumentModel>, int)> GetAllTopUpRequestAsync(TopUpHistoryFilterModel model);
        Task<TopUpRequestDocumentModel> GetPendingTopUpRequestAsync(long accountId);
        Task<List<SMSVendorSettingsDocumentModel>> GetAllSMSVendorSettingsAsync();
        Task<bool> AddOrUpdateSMSDeliveryReportNotificationAsync(DeliveryReportNotificationDocumentModel model);
        Task<DeliveryReportNotificationDocumentModel> GetSMSDeliveryReportNotificationAsync(long accountId);
        Task<DeliveryReportNotificationDocumentModel> AddUserToSMSDeliveryReportNotificationAsync(long accountId,
            List<EmailNotificationUserDocumentModel> userDetails);
        Task<string> GetErrorDescriptionAsync(string errorCode, string language);
        Task<DeliveryReportNotificationDocumentModel> RemoveUserFromSMSDeliveryReportNotificationAsync(long accountId,
            List<EmailNotificationUserDocumentModel> userDetails);
        Task<List<DeliveryReportNotificationDocumentModel>> GetAllSMSDeliveryReportNotificationAsync();
        Task<List<SMSErrorCodeDetailsDocumentModel>> GetErrorCodeDetailsAsync();
        Task<List<InventoryAndAlertSettingDocumentModel>> GetAllInventoryAndAlertSettingAsync();
        Task InsertErrorDescriptionAsync(SMSErrorCodeDetailsDocumentModel document);
        Task<List<SMSSFInteractionModel>> GetSMSSFInteractionsAsync(long accountId);
        Task<bool> AddOrUpdateSMSSFInteractionsAsync(List<SMSSFInteractionModel> model, long accountId);
    }
}
