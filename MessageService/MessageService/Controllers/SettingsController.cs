using System;
using System.Threading.Tasks;
using MessageService.ActionFilters;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageService.Controllers
{
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsController> _log;
        public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _log = logger;
        }

        [HttpPost]
        [Route("api/[controller]/SaveVendorSettings")]
        public async Task<IActionResult> SaveVendorSettings(VendorSettingsModel model)
        {
            try
            {
                //Server Side Validation
                var isValid = await _settingsService.IsMMSVendorSettingsValidAsync(model);
                if (!isValid)
                {
                    return Ok(APIResponse.ExpectationFailed("AppId or AppKey already exists"));
                }
                //End
                var result = await _settingsService.AddOrUpdateMMSVendorSettingsAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetVendorSettings")]
        public async Task<IActionResult> GetVendorSettings(long accountid)
        {
            try
            {
                var result = await _settingsService.GetMMSVendorSettingsAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/IsVendorSettingsConfigured")]
        public async Task<IActionResult> IsVendorSettingsConfigured(long accountid)
        {
            try
            {
                var result = await _settingsService.CheckMMSVendorSettingsExistsAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetErrorDescription")]
        public async Task<IActionResult> GetErrorDescription(string errorCode, string language)
        {
            try
            {
                var result = await _settingsService.GetErrorDescriptionAsync(errorCode, language);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => errorCode, () => language), errorCode, language);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => errorCode, () => language), errorCode, language);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetSMSErrorDescription")]
        public async Task<IActionResult> GetSMSErrorDescription(string errorCode, string language)
        {
            try
            {
                var result = await _settingsService.GetSMSErrorDescriptionAsync(errorCode, language);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => errorCode, () => language), errorCode, language);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => errorCode, () => language), errorCode, language);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/SaveSMSVendorSettings")]
        public async Task<IActionResult> SaveSMSVendorSettings(SMSVendorSettingsDocumentModel model)
        {
            try
            {
                //Server Side Validation
                var isValid = await _settingsService.IsSMSVendorSettingsValidAsync(model);
                if (!isValid)
                {
                    return Ok(APIResponse.ExpectationFailed("AppId or AppKey already exists"));
                }
                //End
                var result = await _settingsService.AddOrUpdateSMSVendorSettingsAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetSMSVendorSettings")]
        public async Task<IActionResult> GetSMSVendorSettings(long accountid)
        {
            try
            {
                var result = await _settingsService.GetSMSVendorSettingsAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/IsSMSVendorSettingsConfigured")]
        public async Task<IActionResult> IsSMSVendorSettingsConfigured(long accountid)
        {
            try
            {
                var result = await _settingsService.CheckSMSVendorSettingsExistsAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var result = await _settingsService.GetCategoriesAsync();
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetCategoryMapping")]
        public async Task<IActionResult> GetCategoryMapping()
        {
            try
            {
                var result = await _settingsService.GetCategoryMappingAsync();
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/AddCategory")]
        public async Task<IActionResult> AddCategory(CategoryDocumentModel model)
        {
            try
            {
                //Server Side Validation
                var isValid = await _settingsService.IsCategoryValidAsync(model);
                if (!isValid)
                {
                    return Ok(APIResponse.ExpectationFailed("CategoryAlready already exists"));
                }
                //End
                var result = await _settingsService.AddCategoryAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }


        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/AddOrUpdateInventoryAndAlert")]
        public async Task<IActionResult> AddOrUpdateInventoryAndAlert(InventoryAndAlertSettingDocumentModel model)
        {
            try
            {
                var result = await _settingsService.AddOrUpdateInventoryAndAlertSettingAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetInventoryAndAlertSetting")]
        public async Task<IActionResult> GetInventoryAndAlertSetting(long accountid)
        {
            try
            {
                var result = await _settingsService.GetInventoryAndAlertSettingAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/AddUserToInventoryAndAlert")]
        public async Task<IActionResult> AddUserToInventoryAndAlert(InventoryAndAlertApiModel model)
        {
            try
            {
                var result = await _settingsService.AddUserToInventoryAndAlertSettingAsync(model.AccountId, model.UserDetails);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/RemoveUserFromInventoryAndAlertSetting")]
        public async Task<IActionResult> RemoveUserFromInventoryAndAlertSetting(InventoryAndAlertApiModel model)
        {
            try
            {
                var result = await _settingsService.RemoveUserFromInventoryAndAlertSettingAsync(model.AccountId, model.UserDetails);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetUserToAddInventoryAndAlertSetting")]
        public async Task<IActionResult> GetUserToAddInventoryAndAlertSetting(long accountid)
        {
            try
            {
                var result = await _settingsService.GetUserToAddInventoryAndAlertSettingAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }


        [HttpGet]
        [Route("api/[controller]/GetSMSRechargeHistory")]
        public async Task<IActionResult> GetSMSRechargeHistory(long accountid)
        {
            try
            {
                var result = await _settingsService.GetSMSRechargeHistoryAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/AddTopUpRequest")]
        public async Task<IActionResult> AddTopUpRequest(TopUpRequestDocumentModel model)
        {
            try
            {
                var result = await _settingsService.AddTopUpRequestAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/UpdateTopUpRequest")]
        public async Task<IActionResult> UpdateTopUpRequest(TopUpRequestDocumentModel model)
        {
            try
            {
                var result = await _settingsService.UpdateTopUpRequestAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
        [HttpPost]
        [Route("api/[controller]/GetAllTopUpRequest")]
        public async Task<IActionResult> GetAllTopUpRequest(TopUpHistoryFilterModel model)
        {
            try
            {
                var result = await _settingsService.GetAllTopUpRequestAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetPendingTopUpRequest")]
        public async Task<IActionResult> GetPendingTopUpRequest(long accountid)
        {
            try
            {
                var result = await _settingsService.GetPendingTopUpRequestAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }




        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/AddOrUpdateSMSDeliveryReportNotification")]
        public async Task<IActionResult> AddOrUpdateSMSDeliveryReportNotification(DeliveryReportNotificationDocumentModel model)
        {
            try
            {
                var result = await _settingsService.AddOrUpdateSMSDeliveryReportNotificationAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetSMSDeliveryReportNotification")]
        public async Task<IActionResult> GetSMSDeliveryReportNotification(long accountid)
        {
            try
            {
                var result = await _settingsService.GetSMSDeliveryReportNotificationAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/AddUserToSMSDeliveryReportNotification")]
        public async Task<IActionResult> AddUserToSMSDeliveryReportNotification(InventoryAndAlertApiModel model)
        {
            try
            {
                var result = await _settingsService.AddUserToSMSDeliveryReportNotificationAsync(model.AccountId, model.UserDetails);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("api/[controller]/RemoveUserFromSMSDeliveryReportNotification")]
        public async Task<IActionResult> RemoveUserFromSMSDeliveryReportNotification(InventoryAndAlertApiModel model)
        {
            try
            {
                var result = await _settingsService.RemoveUserFromSMSDeliveryReportNotificationAsync(model.AccountId, model.UserDetails);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model), model.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetUserToAddDeliveryReportSetting")]
        public async Task<IActionResult> GetUserToAddDeliveryReportSetting(long accountid)
        {
            try
            {
                var result = await _settingsService.GetUserToAddDeliveryReportSettingAsync(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }


        #region  MMS Setting
        [HttpPost]
        [Route("api/[controller]/MMSSaveNotifyUser")]
        public async Task<IActionResult> MMSSaveNotifyUser(NotifyUser model)
        {
            try
            {
                var result = await _settingsService.InsertMMSNotificationUsers(model);
                switch (result)
                {
                    case 99: return Ok(APIResponse.Success("Balance Threshold setting is not created"));
                    case 98: return Ok(APIResponse.Success("Delivery report setting is not created"));
                    default:
                        return Ok(APIResponse.Success(result));
                }

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
        [HttpPost]
        [HttpPut]
        [Route("api/[controller]/MMSDeleiveryNotificationSetting")]
        public async Task<IActionResult> MMSDeleiveryNotificationSetting(MMSDeliveryReportNotification model)
        {
            try
            {
                var result = await _settingsService.InsertOrUpdateMMSDeliveryReportNotification(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
        [HttpPost]
        [HttpPut]
        [Route("api/[controller]/MMSBalanceThresholdSetting")]
        public async Task<IActionResult> MMSBalanceThresholdSetting(MMSBalanceThreshold model)
        {
            try
            {
                var result = await _settingsService.InsertOrUpdateMMSBalanceThreshold(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
        [HttpDelete]
        [Route("api/[controller]/RemoveNotificationUser")]
        public async Task<IActionResult> RemoveNotificationUser(NotifyUser model)
        {
            try
            {
                var result = await _settingsService.DeleteMMSNotificationUsers(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => model.AccountId), model.AccountId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetMMSUsersGridData")]
        public async Task<IActionResult> GetMMSUsersGridData(long accountId, int? pageNumber, int? recordPerPage, string sortBy, string sortType, int type)
        {
            FilterParam param = new FilterParam
            {
                accountId = accountId,
                pageNumber = pageNumber,
                recordPerPage = recordPerPage,
                sortBy = sortBy,
                sortType = sortType,
                Type = ((NotificationType)type)
            };
            try
            {
                switch (param.Type)
                {
                    case MessageService.Models.Enum.NotificationType.MMSBalanceThreshold:
                        {
                            var result = await _settingsService.GetMMSBalanceThreshold(param);
                            return Ok(APIResponse.Success(result));
                        }
                    case MessageService.Models.Enum.NotificationType.MMSDeliveryReportSetting:
                        {
                            var result = await _settingsService.GetDeliveryReportNotificationUsers(param);
                            return Ok(APIResponse.Success(result));
                        }
                    default:
                        return NotFound();
                }
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => param.accountId), param.accountId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => param.accountId), param.accountId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
        [HttpGet]
        [Route("api/[controller]/GetMMSBalanceThresholdByAccountId")]
        public async Task<IActionResult> GetMMSBalanceThresholdByAccountId(long accountid)
        {
            try
            {
                var result = await _settingsService.GetMMSBalanceThresholdByAccountId(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
        [HttpGet]
        [Route("api/[controller]/GetMMSDeliveryReportNotificationSettingByAccountId")]
        public async Task<IActionResult> GetMMSDeliveryReportNotificationSettingByAccountId(long accountid)
        {
            try
            {
                var result = await _settingsService.GetMMSDeliveryReportNotificationSettingByAccountId(accountid);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountid), accountid);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }

        }
        #endregion


    }
}