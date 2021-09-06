using MessageService.CosmosRepository.Interface;
using MessageService.InfraStructure.Helpers;
using System.Threading.Tasks;
using MessageService.CosmosRepository.Utility;
using MessageService.Models.CosmosModel;
using MessageService.RedisRepository.Interface;
using System.Collections.Generic;
using System.Linq;
using MessageService.Models.APIModels;
using MessageService.Models.Enum;
using System.Text;
using MessageService.Models.SMSModels;

namespace MessageService.CosmosRepository.Implementation
{
    public class SettingsCosmosRepository : CosmosBaseRepository, ISettingsCosmosRepository
    {
        private readonly IRedisRespository _redisRepository;

        public SettingsCosmosRepository(IRedisRespository redisRepository)
        {
            _redisRepository = redisRepository;
        }

        public async Task<bool> AddOrUpdateSMSVendorSettingsAsync(SMSVendorSettingsDocumentModel model)
        {
            var result = await UpsertDocumentAsync(model);
            await _redisRepository.UpdateSMSVendorSettingsAsync(model);
            return result.IsNotNull();
        }

        public async Task<bool> CheckSMSVendorSettingsExistsAsync(long accountid)
        {
            var settingsFromCache = await _redisRepository.GetSMSVendorSettingsAsync(accountid);
            if (settingsFromCache.IsNotNull())
            {
                return settingsFromCache.IsNotNull();
            }
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VendorSetting },
                { "@partitionkey", CosmosDocumentType.VendorSetting.ToString() },
                { "@accountid", accountid },
            };

            var result = await ReadDocumentAsync<SMSVendorSettingsDocumentModel>(query, parameters);
            await _redisRepository.UpdateSMSVendorSettingsAsync(result);
            return result.IsNotNull();
        }

        public async Task<List<CategoryDocumentModel>> GetCategoriesAsync()
        {
            string query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.TopicSubscriptionCategory },
                { "@partitionkey", CosmosDocumentType.TopicSubscriptionCategory.ToString() }
            };
            var result = await ReadDocumentsAsync<CategoryDocumentModel>(query, parameters);
            return result;
        }

        public async Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(long accountid)
        {
            var settingsFromCache = await _redisRepository.GetSMSVendorSettingsAsync(accountid);
            if (settingsFromCache.IsNotNull())
            {
                return settingsFromCache;
            }

            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VendorSetting },
                { "@partitionkey", CosmosDocumentType.VendorSetting.ToString() },
                { "@accountid", accountid },
            };

            var result = await ReadDocumentAsync<SMSVendorSettingsDocumentModel>(query, parameters);
            if (result.IsNotNull())
            {
                await _redisRepository.UpdateSMSVendorSettingsAsync(result);
            }
            return result;
        }

        public async Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(string appId)
        {
            var settingsFromCache = await _redisRepository.GetSMSVendorSettingsAsync(appId);
            if (settingsFromCache.IsNotNull())
            {
                return settingsFromCache;
            }
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.appid = @appid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VendorSetting },
                { "@partitionkey", CosmosDocumentType.VendorSetting.ToString() },
                { "@appid", appId },
            };

            var result = await ReadDocumentAsync<SMSVendorSettingsDocumentModel>(query, parameters);
            await _redisRepository.UpdateSMSVendorSettingsAsync(result);
            return result;
        }

        public async Task<bool> IsSMSVendorSettingsValidAsync(SMSVendorSettingsDocumentModel model)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid != @accountid AND (c.appid = @appid OR c.appkey = @appkey)";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VendorSetting },
                { "@partitionkey", CosmosDocumentType.VendorSetting.ToString() },
                { "@accountid", model.AccountId },
                { "@appid", model.AppId },
                { "@appkey", model.AppKey }
            };

            var result = await ReadDocumentAsync<SMSVendorSettingsDocumentModel>(query, parameters);
            return result == null;
        }

        public async Task<bool> IsCategoryValidAsync(CategoryDocumentModel model)
        {
            string query = @"SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.categoryname = @categoryname";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.TopicSubscriptionCategory },
                { "@partitionkey", CosmosDocumentType.TopicSubscriptionCategory.ToString() },
                { "@categoryname", model.CategoryName },
            };

            var result = await ReadDocumentAsync<CategoryDocumentModel>(query, parameters);
            return result == null;
        }

        public async Task<bool> AddCategoryAsync(CategoryDocumentModel model)
        {
            var result = await InsertDocumentAsync(model);
            return result.IsNotNull();
        }

        public async Task<List<SMSSFInteractionModel>> GetSMSSFInteractionsAsync(long accountId)
        {
            var result = await _redisRepository.GetSMSSFInteractionsAsync(accountId);
            return result ?? new List<SMSSFInteractionModel>();
        }
        public async Task<bool> AddOrUpdateSMSSFInteractionsAsync(List<SMSSFInteractionModel> model, long accountId)
        {
            var result = await _redisRepository.AddOrUpdateSMSSFInteractionsAsync(model, accountId);
            return result;
        }

        public async Task<bool> AddOrUpdateInventoryAndAlertSettingAsync(InventoryAndAlertSettingDocumentModel model)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.InventoryAndAlert },
                { "@partitionkey", CosmosDocumentType.InventoryAndAlert.ToString() },
                { "@accountid", model.AccountId },
            };
            var result = await ReadDocumentAsync<InventoryAndAlertSettingDocumentModel>(query, parameters);
            if (result.IsNull())
            {
                model.IsConfigured = true;
                await InsertDocumentAsync(model);
            }
            else
            {
                result.IsConfigured = true;
                result.AlertThreshold = model.AlertThreshold;
                await UpsertDocumentAsync(result);
            }
            return true;
        }

        public async Task<InventoryAndAlertSettingDocumentModel> GetInventoryAndAlertSettingAsync(long accountId)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.InventoryAndAlert },
                { "@partitionkey", CosmosDocumentType.InventoryAndAlert.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentAsync<InventoryAndAlertSettingDocumentModel>(query, parameters);
            return result;
        }

        public async Task<InventoryAndAlertSettingDocumentModel> AddUserToInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.InventoryAndAlert },
                { "@partitionkey", CosmosDocumentType.InventoryAndAlert.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentAsync<InventoryAndAlertSettingDocumentModel>(query, parameters);
            result.NotificationUsers = result.NotificationUsers.Union(userDetails).ToList();
            await UpsertDocumentAsync(result);
            return result;
        }

        public async Task<InventoryAndAlertSettingDocumentModel> RemoveUserFromInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.InventoryAndAlert },
                { "@partitionkey", CosmosDocumentType.InventoryAndAlert.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentAsync<InventoryAndAlertSettingDocumentModel>(query, parameters);
            var userIds = userDetails.Select(y => y.UserId);

            result.NotificationUsers = result.NotificationUsers.Where(k => !userIds.Contains(k.UserId)).ToList();
            await UpsertDocumentAsync(result);
            return result;
        }

        public async Task<bool> AddTopUpRequestAsync(TopUpRequestDocumentModel model)
        {
            await InsertDocumentAsync(model).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> UpdateTopUpRequestAsync(TopUpRequestDocumentModel model)
        {
            await UpsertDocumentAsync(model).ConfigureAwait(false);
            return true;
        }

        // TO be tested
        public async Task<(List<TopUpRequestDocumentModel>, int)> GetAllTopUpRequestAsync(TopUpHistoryFilterModel model)
        {
            StringBuilder query = new StringBuilder($@"SELECT [PROJECTION] FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");


            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.TopUpRequest },
                { "@partitionkey", CosmosDocumentType.TopUpRequest.ToString() },
                { "@accountid", model.AccountId },
                { "@pageno", (model.PageNo - 1) * model.ItemsPerPage },
                { "@itemsperpage", model.ItemsPerPage },

            };       

            var count = (await ReadDocumentsAsync<int>(query.ToString().Replace("[PROJECTION]", " VALUE COUNT(1) "), parameters)).Sum();

            query.Append("ORDER BY c.raisedon OFFSET @pageno LIMIT @itemsperpage ");

            var result = await ReadDocumentsAsync<TopUpRequestDocumentModel>(query.ToString().Replace("[PROJECTION]", " * "), parameters);
            return (result, count);
        }

        public async Task<TopUpRequestDocumentModel> GetPendingTopUpRequestAsync(long accountId)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid AND c.status = @status";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.TopUpRequest },
                { "@partitionkey", CosmosDocumentType.TopUpRequest.ToString() },
                { "@accountid", accountId },
                { "@status", TopUpRequestStatus.Pending },
            };
            var result = await ReadDocumentAsync<TopUpRequestDocumentModel>(query, parameters);
            return result;
        }

        public async Task<bool> AddOrUpdateSMSDeliveryReportNotificationAsync(DeliveryReportNotificationDocumentModel model)
        {

            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.DeliveryReportNotification },
                { "@partitionkey", CosmosDocumentType.DeliveryReportNotification.ToString() },
                { "@accountid", model.AccountId },
            };
            var result = await ReadDocumentAsync<DeliveryReportNotificationDocumentModel>(query, parameters);
            if (result.IsNull())
            {
                model.IsConfigured = true;
                await InsertDocumentAsync(model);
            }
            else
            {
                result.Notify = model.Notify;
                result.DayOn = model.DayOn;
                result.RunOn = model.RunOn;
                result.DeliveryPercentage = model.DeliveryPercentage;
                result.RunBy = model.RunBy;
                result.CreatedOn = model.CreatedOn;
                result.IsConfigured = true;
                await UpsertDocumentAsync(result);
            }
            return true;
        }

        public async Task<DeliveryReportNotificationDocumentModel> GetSMSDeliveryReportNotificationAsync(long accountId)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.DeliveryReportNotification },
                { "@partitionkey", CosmosDocumentType.DeliveryReportNotification.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentAsync<DeliveryReportNotificationDocumentModel>(query, parameters);
            return result;
        }

        public async Task<DeliveryReportNotificationDocumentModel> AddUserToSMSDeliveryReportNotificationAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.DeliveryReportNotification },
                { "@partitionkey", CosmosDocumentType.DeliveryReportNotification.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentAsync<DeliveryReportNotificationDocumentModel>(query, parameters);
            result.NotificationUsers = result.NotificationUsers.Union(userDetails).ToList();
            await UpsertDocumentAsync(result);
            return result;
        }

        public async Task<List<SMSVendorSettingsDocumentModel>> GetAllSMSVendorSettingsAsync()
        {
            string query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VendorSetting },
                { "@partitionkey", CosmosDocumentType.VendorSetting.ToString() }
            };
            var result = await ReadDocumentsAsync<SMSVendorSettingsDocumentModel>(query, parameters);
            return result;
        }
        public async Task<DeliveryReportNotificationDocumentModel> RemoveUserFromSMSDeliveryReportNotificationAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails)
        {
            string query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.DeliveryReportNotification },
                { "@partitionkey", CosmosDocumentType.DeliveryReportNotification.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentAsync<DeliveryReportNotificationDocumentModel>(query, parameters);
            var userIds = userDetails.Select(y => y.UserId);

            result.NotificationUsers = result.NotificationUsers.Where(k => !userIds.Contains(k.UserId)).ToList();
            await UpsertDocumentAsync(result);
            return result;
        }

        public async Task<List<DeliveryReportNotificationDocumentModel>> GetAllSMSDeliveryReportNotificationAsync()
        {
            string query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.DeliveryReportNotification },
                { "@partitionkey", CosmosDocumentType.DeliveryReportNotification.ToString() }
            };
            var result = await ReadDocumentsAsync<DeliveryReportNotificationDocumentModel>(query, parameters);
            return result;
        }

        public async Task<List<InventoryAndAlertSettingDocumentModel>> GetAllInventoryAndAlertSettingAsync()
        {
            string query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.InventoryAndAlert },
                { "@partitionkey", CosmosDocumentType.InventoryAndAlert.ToString() }
            };
            var result = await ReadDocumentsAsync<InventoryAndAlertSettingDocumentModel>(query, parameters);
            return result;
        }

        public async Task<List<SMSErrorCodeDetailsDocumentModel>> GetErrorCodeDetailsAsync()
        {
            string query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.ErrorCodeDetails },
                { "@partitionkey", CosmosDocumentType.ErrorCodeDetails.ToString() }
            };
            var result = await ReadDocumentsAsync<SMSErrorCodeDetailsDocumentModel>(query, parameters);
            return result;
        }

        public async Task<string> GetErrorDescriptionAsync(string errorCode, string language)
        {
            string projection = (language == LanguageType.Chinese) ? "c.chinesedescription" : "c.englishdescription";
            string query = $@"SELECT VALUE {projection} FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.errorcode = @errorcode ";
            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.ErrorCodeDetails },
                { "@partitionkey", CosmosDocumentType.ErrorCodeDetails.ToString() },
                { "@errorcode", errorCode }
            };
            var result = await ReadDocumentAsync<string>(query, parameters);

            if (result.IsNullOrEmpty())
            {
                parameters["@errorcode"] = errorCode.Substring(0, 3);
                query = $@"SELECT VALUE {projection} FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND STARTSWITH(c.errorcode, @errorcode) ";
                result = await ReadDocumentAsync<string>(query, parameters);
            }

            return result;
        }

        public async Task InsertErrorDescriptionAsync(SMSErrorCodeDetailsDocumentModel document)
        {
            await InsertDocumentAsync(document);
        }

    }
}
