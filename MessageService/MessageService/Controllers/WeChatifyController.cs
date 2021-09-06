using System;
using System.Linq;
using System.Threading.Tasks;
using MessageService.ActionFilters;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageService.Controllers
{
    [ApiController]
    public class WeChatifyController : ControllerBase
    {
        private readonly IWeChatifyService _wechatifyService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<WeChatifyController> _log;
        public WeChatifyController(IWeChatifyService wechatifyService,ISettingsService settings ,ILogger<WeChatifyController> logger)
        {
            _wechatifyService = wechatifyService;
            _settingsService = settings;
            _log = logger;
        }

        [HttpGet]
        [Route("api/[controller]/GetAllWeChatifyUsers")]
        public async Task<IActionResult> GetAllWeChatifyUsers(long accountid)
        {
            try
            {
                var result = await _wechatifyService.GetAllWeChatifyUsersAsync(accountid);
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
        [Route("api/[controller]/GetWeChatifyUsersForMMSSetting")]
        public async Task<IActionResult> GetWeChatifyUsersForMMSSetting(long accountid,int pg,int numberOfRecords,int type=0)
        {
            try
            {
                var result = await _wechatifyService.GetAllWeChatifyUsersAsync(accountid);
                switch (type)
                {
                    case 1: {
                       var notifyUsers= await  _settingsService.GetMMSNotificationUsers(accountid, NotificationType.MMSBalanceThreshold);
                            if (notifyUsers != null && notifyUsers.Count > 0)
                            {
                                var exceptUserList = (from a in result
                                                      join b in notifyUsers on a.UserId equals b into lej
                                                      from d in lej.DefaultIfEmpty()
                                                      where string.IsNullOrEmpty(d)
                                                      select a
                                               );
                                var totalCount = exceptUserList.Count();
                                 var returnData= exceptUserList.Skip(((pg - 1) * numberOfRecords)).Take(numberOfRecords);
                                return Ok(APIResponse.Success(new {TotalCount= totalCount, Data=returnData }));
                            }

                        } break;
                    case 3: {
                            var notifyUsers = await _settingsService.GetMMSNotificationUsers(accountid, NotificationType.MMSDeliveryReportSetting);
                            if (notifyUsers!=null && notifyUsers.Count>0)
                            {
                                var exceptUserList = (from a in result
                                                      join b in notifyUsers on a.UserId equals b into lej
                                                      from d in lej.DefaultIfEmpty()
                                                      where string.IsNullOrEmpty(d)
                                                      select a
                                               ).ToList();
                                var totalCount = exceptUserList.Count();
                                var returnData = exceptUserList.Skip(((pg - 1) * numberOfRecords)).Take(numberOfRecords);
                                return Ok(APIResponse.Success(new { TotalCount = totalCount, Data = returnData }));
                            }
                        }break; 
                }
                return Ok(APIResponse.Success(new { TotalCount = result.Count(), Data = result.Skip(((pg - 1) * numberOfRecords)).Take(numberOfRecords) }));
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
        [Route("api/[controller]/GetAccountDetails")]
        public async Task<IActionResult> GetAccountDetails(long accountid)
        {
            try
            {
                var result = await _wechatifyService.GetAccountDetailsAsync(accountid);
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
        [Route("api/[controller]/GetMMSMappedAccounts")]
        [JsonWebTokenFilter]
        public async Task<IActionResult> GetMMSMappedAccounts(string id)
        {
            try
            {
                if (id.IsNull())
                {
                    Ok(APIResponse.Error("Access Denied"));
                }
                var result = await _wechatifyService.GetAllMmsMappedSFAccountsAsync(id);
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
        [Route("api/[controller]/GetSMSMappedAccounts")]
        [JsonWebTokenFilter]
        public async Task<IActionResult> GetSMSMappedAccounts(string id)
        {
            try
            {
                if (id.IsNull())
                {
                    Ok(APIResponse.Error("Access Denied"));
                }
                var result = await _wechatifyService.GetAllSMSMappedSFAccountsAsync(id);
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

    }
}