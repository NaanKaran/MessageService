using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MessageService.CosmosRepository.Interface;
using MessageService.InfraStructure.APIUrls;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Models.SubmailModel;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ISettingsCosmosRepository _settingsCosmosRepository;
        private readonly IWeChatifyRepository _weChatifyRepository;
        private readonly IEmailService _emailService;
        public SettingsService(ISettingsRepository settingsRepository, ISettingsCosmosRepository settingsCosmosRepository, IWeChatifyRepository weChatifyRepository, IEmailService emailService)
        {
            _settingsRepository = settingsRepository;
            _settingsCosmosRepository = settingsCosmosRepository;
            _weChatifyRepository = weChatifyRepository;
            _emailService = emailService;
        }
        public async Task<int> AddOrUpdateMMSVendorSettingsAsync(VendorSettingsModel model)
        {
            return await _settingsRepository.AddOrUpdateMMSVendorSettingsAsync(model).ConfigureAwait(false);
        }

        public async Task<VendorSettingsModel> GetMMSVendorSettingsAsync(long accountid)
        {
            return await _settingsRepository.GetMMSVendorSettingsAsync(accountid).ConfigureAwait(false);
        }

        public async Task<string> GetErrorDescriptionAsync(string errorCode, string language)
        {
            return await _settingsRepository.GetErrorDescriptionAsync(errorCode, language);
        }

        public async Task<string> GetSMSErrorDescriptionAsync(string errorCode, string language)
        {
            string defaultDescription = "Description Not Available";
            var result = await _settingsCosmosRepository.GetErrorDescriptionAsync(errorCode, language);
            if(result.IsNullOrEmpty())
            {                
                SMSErrorCodeDetailsDocumentModel model = new SMSErrorCodeDetailsDocumentModel() {
                    VendorId = VendorType.Submail,
                    ChineseDescription = defaultDescription,
                    EnglishDescription = defaultDescription,
                    ErrorCode = errorCode
                };
                await _settingsCosmosRepository.InsertErrorDescriptionAsync(model);
            }
            return result ?? defaultDescription;
        }

        public async Task<bool> CheckMMSVendorSettingsExistsAsync(long accountid)
        {
            return await _settingsRepository.CheckMMSVendorSettingsExistsAsync(accountid).ConfigureAwait(false);
        }

        public async Task<bool> IsMMSVendorSettingsValidAsync(VendorSettingsModel model)
        {
            return await _settingsRepository.IsMMSVendorSettingsValidAsync(model).ConfigureAwait(false);
        }

        public async Task<bool> AddOrUpdateSMSVendorSettingsAsync(SMSVendorSettingsDocumentModel model)
        {            
            await _weChatifyRepository.AddOrUpdateSMSVendorSettingsAsync(model).ConfigureAwait(false);
            return await _settingsCosmosRepository.AddOrUpdateSMSVendorSettingsAsync(model).ConfigureAwait(false);
        }

        public async Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(long accountid)
        {
            return await _settingsCosmosRepository.GetSMSVendorSettingsAsync(accountid).ConfigureAwait(false);
        }

        public async Task<bool> CheckSMSVendorSettingsExistsAsync(long accountid)
        {
            return await _settingsCosmosRepository.CheckSMSVendorSettingsExistsAsync(accountid).ConfigureAwait(false);
        }

        public async Task<bool> IsSMSVendorSettingsValidAsync(SMSVendorSettingsDocumentModel model)
        {
            return await _settingsCosmosRepository.IsSMSVendorSettingsValidAsync(model).ConfigureAwait(false);
        }

        public async Task<bool> IsCategoryValidAsync(CategoryDocumentModel model)
        {
            return await _settingsCosmosRepository.IsCategoryValidAsync(model).ConfigureAwait(false);
        }

        public async Task<IEnumerable<CategoryDocumentModel>> GetCategoriesAsync()
        {
            return await _settingsCosmosRepository.GetCategoriesAsync().ConfigureAwait(false);
        }

        public async Task<bool> AddCategoryAsync(CategoryDocumentModel model)
        {
            return await _settingsCosmosRepository.AddCategoryAsync(model).ConfigureAwait(false);
        }

        public async Task<bool> AddOrUpdateInventoryAndAlertSettingAsync(InventoryAndAlertSettingDocumentModel model)
        {
            await SendSMSThresholdUpdateEmailAsync(model);
            return await _settingsCosmosRepository.AddOrUpdateInventoryAndAlertSettingAsync(model).ConfigureAwait(false);
        }

        private async Task SendSMSThresholdUpdateEmailAsync(InventoryAndAlertSettingDocumentModel model)
        {
            var currentSettings = await GetInventoryAndAlertSettingAsync(model.AccountId);
            if (currentSettings != null && currentSettings.AlertThreshold != model.AlertThreshold)
            {
                var accountDetails = await _weChatifyRepository.GetAccountDetailsAsync(model.AccountId);
                DateTime updatedOn = DateTime.UtcNow.ToChinaTime();
                string accountName = accountDetails.AccountName;
                var userDetail = await _weChatifyRepository.GetUserByIdAsync(model.UpdatedBy);
                string updatedBy = userDetail?.FirstName + " " + userDetail?.LastName;
                string updatedByEmail = userDetail?.Email;
                string templateFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EmailTemplates", "SMSThresholdUpdate.htm");
                foreach (var detail in currentSettings.NotificationUsers)
                {
                    string userName = detail.UserName;
                    await _emailService.SendUpdateEmailForBalanceThreshold(templateFile, accountName, detail.UserName,
                        updatedBy, updatedByEmail, updatedOn.ToString("ddd, dd MMM yyy HH:mm tt"),
                        model.AlertThreshold, currentSettings.AlertThreshold,
                        detail.EmailId, "Alert Threshold Change Notification");
                }
            }           
        }

        public async Task<InventoryAndAlertSettingDocumentModel> GetInventoryAndAlertSettingAsync(long accountId)
        {
            var result = await _settingsCosmosRepository.GetInventoryAndAlertSettingAsync(accountId).ConfigureAwait(false);
            if(result.IsNull())
            {
                return new InventoryAndAlertSettingDocumentModel();
            }
            else
            {
                result.IsConfigured = true;
                return result;
            }
        }

        public async Task<InventoryAndAlertSettingDocumentModel> AddUserToInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails)
        {
            return await _settingsCosmosRepository.AddUserToInventoryAndAlertSettingAsync(accountId, userDetails).ConfigureAwait(false);
        }

        public async Task<InventoryAndAlertSettingDocumentModel> RemoveUserFromInventoryAndAlertSettingAsync(long accountId, List<EmailNotificationUserDocumentModel> userDetails)
        {
            return await _settingsCosmosRepository.RemoveUserFromInventoryAndAlertSettingAsync(accountId, userDetails).ConfigureAwait(false);
        }

        public async Task<List<WeChatifyUserModel>> GetUserToAddInventoryAndAlertSettingAsync(long accountId)
        {
            var inventorySetting = await _settingsCosmosRepository.GetInventoryAndAlertSettingAsync(accountId).ConfigureAwait(false);

            var wechatUsers = await _weChatifyRepository.GetAllSMSWeChatifyUsersAsync(accountId).ConfigureAwait(false);

            var userIds = inventorySetting.NotificationUsers.Select(k => k.UserId);

            wechatUsers = wechatUsers.Where(k => !userIds.Contains(k.UserId));

            return wechatUsers.ToList();
        }

        public async Task<List<RechargeHistoryModel>> GetSMSRechargeHistoryAsync(long accountId)
        {
            var vendorSetting = await _settingsCosmosRepository.GetSMSVendorSettingsAsync(accountId).ConfigureAwait(false);

            if (vendorSetting.IsNull())
            {
                return new List<RechargeHistoryModel>();
            }

            var data = new BaseSubmailModel()
            {
                AppId = vendorSetting.AppId,
                AppKey = vendorSetting.AppKey
            };

            var rechargeHistory = await HttpHelper<RechargeHistoryResponseModel>
                .HttpPostAsync(SubmailAPIUrls.SMSRechargeHistoryUrl, data.ToJsonString()).ConfigureAwait(false);

            if (rechargeHistory.Status != "success")
            {
                return new List<RechargeHistoryModel>();
            }

            return rechargeHistory.RechargeHistories.Where(k=>Convert.ToInt32(k.SmsAddCredits) != 0).ToList();
        }

        public async Task<bool> AddTopUpRequestAsync(TopUpRequestDocumentModel model)
        {
            var accountDetails = await _weChatifyRepository.GetAccountDetailsAsync(model.AccountId);

            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates",
                "TopUpRequestTemplate.htm");

            foreach (var request in model.RequestedToUsers)
            {
                await _emailService.SendTopUpRequestEmailAsync(new[] { request.EmailId }, accountDetails.AccountName, templatePath, request.UserName,
                    model.RaisedByUserName, model.RaisedByUserEmailId, model.TopUpValue);
            }

            var userDetails = await _weChatifyRepository.GetUsersAsync(model.RaisedByUserId);

            model.RaisedByUserRole = userDetails.RoleName;

            return await _settingsCosmosRepository.AddTopUpRequestAsync(model);
        }

        public async Task<bool> UpdateTopUpRequestAsync(TopUpRequestDocumentModel model)
        {
            model.HandledOn = DateTime.UtcNow.ToChinaTime();
            return await _settingsCosmosRepository.UpdateTopUpRequestAsync(model);
        }

        public async Task<PagingModel<TopUpRequestDocumentModel>> GetAllTopUpRequestAsync(TopUpHistoryFilterModel model)
        {
            var (listOfdata, count) = await _settingsCosmosRepository.GetAllTopUpRequestAsync(model);

            var paging = new PagingModel<TopUpRequestDocumentModel>()
            {
                Items = listOfdata,
                TotalCount = count,
                ItemsPerPage = model.ItemsPerPage,
                PageNumber = model.PageNo
            };

            return paging;
        }

        public async Task<TopUpRequestDocumentModel> GetPendingTopUpRequestAsync(long accountId)
        {
            return await _settingsCosmosRepository.GetPendingTopUpRequestAsync(accountId);
        }

        public async Task<List<WeChatifyUserModel>> GetUserToAddDeliveryReportSettingAsync(long accountId)
        {
            var inventorySetting = await _settingsCosmosRepository.GetSMSDeliveryReportNotificationAsync(accountId).ConfigureAwait(false);

            var wechatUsers = await _weChatifyRepository.GetAllSMSWeChatifyUsersAsync(accountId).ConfigureAwait(false);

            var userIds = inventorySetting.NotificationUsers.Select(k => k.UserId);

            wechatUsers = wechatUsers.Where(k => !userIds.Contains(k.UserId));

            return wechatUsers.ToList();
        }

        public async Task<string> AddOrUpdateSMSDeliveryReportNotificationAsync(DeliveryReportNotificationDocumentModel model)
        {
            var notify = NotifyString(model);
            model.Notify = notify;
            await _settingsCosmosRepository.AddOrUpdateSMSDeliveryReportNotificationAsync(model).ConfigureAwait(false);
          
            return notify;
        }

        private string NotifyString(DeliveryReportNotificationDocumentModel setting)
        {

            RunBy runBy = (RunBy)setting.RunBy;
            switch (runBy)
            {
                case RunBy.Daily:
                    {
                        return $@"Daily at {setting.RunOn} o'clock in china time";

                    }

                case RunBy.Weekly:
                    {
                        DayOfWeek weekDay = (DayOfWeek)setting.DayOn;
                        return $@"Weekly on {weekDay.ToString()} at {setting.RunOn} o'clock in china time.";
                    }

                case RunBy.Monthly:
                    {
                        return setting.DayOn < 29
                            ? $@"Monthly on {setting.DayOn} at {setting.RunOn} o'clock in china time."
                            : $@"Every Month End at {setting.RunOn} o'clock in china time.";
                    }

                default:
                    return "Not Yet Configured.";

            }

        }

        public async Task<DeliveryReportNotificationDocumentModel> GetSMSDeliveryReportNotificationAsync(long accountId)
        {
           var result = await _settingsCosmosRepository.GetSMSDeliveryReportNotificationAsync(accountId).ConfigureAwait(false);
            if(result.IsNull())
            {
                return new DeliveryReportNotificationDocumentModel();
            }
            else
            {
                result.IsConfigured = true;
                return result;
            }
        }
        public async Task<DeliveryReportNotificationDocumentModel> AddUserToSMSDeliveryReportNotificationAsync(long accountId,
            List<EmailNotificationUserDocumentModel> userDetails)
        {
            return await _settingsCosmosRepository.AddUserToSMSDeliveryReportNotificationAsync(accountId, userDetails).ConfigureAwait(false);
        }

        public async Task<DeliveryReportNotificationDocumentModel> RemoveUserFromSMSDeliveryReportNotificationAsync(long accountId,
            List<EmailNotificationUserDocumentModel> userDetails)
        {
            return await _settingsCosmosRepository.RemoveUserFromSMSDeliveryReportNotificationAsync(accountId, userDetails).ConfigureAwait(false);
        }


        public async Task<Dictionary<string, List<string>>> GetCategoryMappingAsync()
        {
            var categories = await _settingsCosmosRepository.GetCategoriesAsync().ConfigureAwait(false);
            var settings = await _settingsCosmosRepository.GetAllSMSVendorSettingsAsync().ConfigureAwait(false);

            var categoryDictionary = settings.GroupBy(k => k.Category).ToDictionary(y => y.Key, m => m.ToList());
            var result = new Dictionary<string, List<string>>();
            foreach (var category in categories)
            {
                if (!categoryDictionary.ContainsKey(category.CategoryName))
                {
                    result.Add(category.CategoryName, new List<string>());
                    continue;
                }
                var accountNames = await _weChatifyRepository.GetWeChatifyAccountNamesAsync(categoryDictionary[category.CategoryName].Select(x => x.AccountId).ToList());
                result.Add(category.CategoryName, accountNames.ToList());
            }
            return result;
        }
        #region MMS Setting
        public async Task<int> InsertMMSNotificationUsers(NotifyUser param)
        {
            return await _settingsRepository.InsertMMSNotificationUsers(param);
        }
        public async Task<int> DeleteMMSNotificationUsers(NotifyUser param)
        {
            return await _settingsRepository.DeleteMMSNotificationUsers(param);
        }
        public async Task<int> InsertOrUpdateMMSDeliveryReportNotification(MMSDeliveryReportNotification model)
        {
            return await _settingsRepository.InsertOrUpdateMMSDeliveryReportNotification(model);
        }
        public async Task<int> InsertOrUpdateMMSBalanceThreshold(MMSBalanceThreshold model)
        {
            long prevCount = 0;
            var mmsBalanceThreshold = await _settingsRepository.GetMMSBalanceThresholdByAccountId(model.AccountId);
            if (mmsBalanceThreshold!=null)
            {
                prevCount = mmsBalanceThreshold.ThresholdCount;
            }
            var notifyUserList= await _settingsRepository.InsertOrUpdateMMSBalanceThreshold(model);
            if (notifyUserList!=null &&  notifyUserList.Count>0)
            {
                await SendThresholdChangeEmail(notifyUserList, model.AccountId, model.UserId, model.ModifiedOn, prevCount, model.ThresholdCount);
            }
            return 1;
        }
        public async Task<List<NotifyUserList>> GetDeliveryReportNotificationUsers(FilterParam param)
        {
            return await _settingsRepository.GetDeliveryReportNotificationUsers(param);
        }
        public async Task<List<NotifyUserList>> GetMMSBalanceThreshold(FilterParam param)
        {
            return await _settingsRepository.GetMMSBalanceThreshold(param);
        }
        public async Task<MMSBalanceThreshold> GetMMSBalanceThresholdByAccountId(long accountId)
        {
            return await _settingsRepository.GetMMSBalanceThresholdByAccountId(accountId);
        }
        public async Task<MMSDeliveryReportNotification> GetMMSDeliveryReportNotificationSettingByAccountId(long accountId)
        {
            return await _settingsRepository.GetMMSDeliveryReportNotificationSettingByAccountId(accountId);
        }

        private async Task SendThresholdChangeEmail(List<NotifyUserList> users, long accountId, string changedUserId, DateTime? changedOn,long fromcount,long toCount)
        {
            var accountDetails = await _weChatifyRepository.GetAccountDetailsAsync(accountId);
            changedOn = changedOn == null ? DateTime.UtcNow.ToChinaTime() : changedOn;
            string accountName = accountDetails.AccountName;
            var changedUserDetail = await _weChatifyRepository.GetUserByIdAsync(changedUserId);
            string requestedUserName = changedUserDetail.FirstName + " " + changedUserDetail.LastName;
            string requestedEmail = changedUserDetail.Email;
            string templateFile= Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EmailTemplates", "MMSThresholdUpdate.htm");
            foreach (var detail in users)
            {
                string userName = detail.Name;
                await _emailService.SendUpdateEmailForBalanceThreshold(templateFile, accountName, detail.Name, requestedUserName, requestedEmail, changedOn.Value.ToString("ddd, dd MMM yyy HH:mm tt"), toCount, fromcount, detail.Email, "Alert Threshold Change Notification");
            }
        }
        public async Task<List<string>> GetMMSNotificationUsers(long accountId, NotificationType Type)
        {
            return await _settingsRepository.GetMMSNotificationUsers(accountId, Type);
        }
        #endregion
    }
}
