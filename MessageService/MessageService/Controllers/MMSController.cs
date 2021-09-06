using System;
using System.Net;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.ExportModels;
using MessageService.Models.SubmailModel;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageService.Controllers
{
    [ApiController]
    public class MMSController : ControllerBase
    {
        private readonly ILogger<MMSController> _log;
        private readonly IMMSService _mmsService;

        public MMSController(ILogger<MMSController> log, IMMSService mmsService)
        {
            _log = log;
            _mmsService = mmsService;

        }

        [HttpPost]
        [Route("api/[controller]/SubmailCallBack")]
        public async Task<IActionResult> SubmailCallBack(SubmailStatusPushModel data)
        {
            try
            {
                await _mmsService.SubmailStatusUpdateAddToQueueAsync(data).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.OK);
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                 return StatusCode((int)HttpStatusCode.InternalServerError, agg);
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                _log.LogError(e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost]
        [Route("api/[controller]/SubmailTemplateCallBack")]
        public async Task<IActionResult> SubmailTemplateCallBack(SubmailStatusPushModel data)
        {
            _log.LogInformation(data.ToJsonString());
            try
            {
                await _mmsService.UpdateTemplateStatusFromSubmailAsync(data).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.OK);
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, agg);
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                _log.LogError(e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, e);
            }
        }


        [HttpPost]
        [Route("api/[controller]/GetIncomingMessages")]
        public async Task<IActionResult> GetIncomingMessages(GetIncomingMessagesModel model)
        {  
            try
            {
                var result = await _mmsService.GetIncomingMessagesAsync(model);
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
        [Route("api/[controller]/ExportIncomingMessages")]
        public async Task<IActionResult> ExportIncomingMessages(IncomingMessagesExportModel model)
        {
            try
            {
                await _mmsService.AddQueueForIncomingMessagesExportAsync(model);
                return Ok(APIResponse.Success(true));
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
        [Route("api/[controller]/ExportOptOutMessages")]
        public async Task<IActionResult> ExportOptOutMessages(IncomingMessagesExportModel model)
        {
            try
            {
                await _mmsService.AddQueueForOptOutMessagesExportAsync(model);
                return Ok(APIResponse.Success(true));
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
        [Route("api/[controller]/GetMMSBalance")]
        public async Task<IActionResult> GetMMSBalance(long accountid)
        {
            try
            {
                var result = await _mmsService.GetMMSBalanceAsync(accountid);
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
        [Route("api/[controller]/GetMMSTopupHistory")]
        public async Task<IActionResult> GetMMSTopupHistory(long accountid)
        {
            try
            {
                var result = await _mmsService.GetMMSTopupHistoryAsync(accountid);
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
        [Route("api/[controller]/SalesForceJourneyActivatePush")]
        public async Task<IActionResult> SalesForceJourneyActivatePush(JourneyActivateModel data)
        {
            try
            {
                
                await _mmsService.AddJourneyEntryAsync(data).ConfigureAwait(false);
                return StatusCode((int)HttpStatusCode.OK);
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, agg);
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                _log.LogError(e, e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetAllJourney")]
        public async Task<IActionResult> GetAllJourney(long accountid)
        {
            try
            {
                var result = await _mmsService.GetAllJourneyAsync(accountid);
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
        [Route("api/[controller]/ExportJournies")]
        public async Task<IActionResult> ExportJournies(JourneyExportModel model)
        {
            try
            {
                await _mmsService.AddQueueForJourneysExportAsync(model);
                return Ok(APIResponse.Success(true));
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
        [Route("api/[controller]/ExportMMSLog")]
        public async Task<IActionResult> ExportMMSLog(LogFilterModel logFilter)
        {
            try
            {
                await _mmsService.AddQueueForMMSLogExportAsync(logFilter);
                return Ok(APIResponse.Success(true));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => logFilter), logFilter.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => logFilter), logFilter.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetJournies")]
        public async Task<IActionResult> GetJournies(JourneyFilterModel filterModel)
        {
            try
            {
                var result = await _mmsService.GetJourneysAsync(filterModel);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => filterModel), filterModel.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => filterModel), filterModel.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetJourneyDetails")]
        public async Task<IActionResult> GetJourneyDetails(long accountId, string journeyId, string quadrantTableName = null)
        {
            try
            {
                var result = await _mmsService.GetJourneyDetailsAsync(accountId,journeyId, quadrantTableName);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => journeyId,()=>accountId,()=>quadrantTableName), journeyId,accountId,quadrantTableName);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => journeyId, () => accountId, () => quadrantTableName), journeyId, accountId, quadrantTableName);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetMmsLogDetail")]
        public async Task<IActionResult> GetMmsLogDetail(LogFilterModel logFilter)
        {
            try
            {
                var result = await _mmsService.GetMmsLogDetailsAsync(logFilter);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => logFilter), logFilter.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => logFilter), logFilter.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetMmsUsageDetail")]
        public async Task<IActionResult> GetMmsUsageDetail(long accountId, int? year)
        {
            try
            {
                var result = await _mmsService.GetMmsUsageDetailAsync(accountId, year);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, ()=> year), accountId, year);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => year), accountId, year);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetTopUpHistoryDetails")]
        public async Task<IActionResult> GetTopUpHistoryDetails(long accountId, int year, int month)
        {
            try
            {
                var result = await _mmsService.GetMMSTopUpHistoryAsync(accountId, year, month);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => year), accountId, year);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => year), accountId, year);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }


        [HttpGet]
        [Route("api/[controller]/ReprocessFailedQueue")]
        public async Task<IActionResult> ReprocessFailedQueue(string queueName)
        {
            try
            {
                 await _mmsService.ReprocessQueueAsync(queueName);
                return Ok(APIResponse.Success(true));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => queueName), queueName);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => queueName), queueName);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }


        [HttpGet]
        [Route("api/[controller]/MMSUsageCountUpdateAsync")]
        public async Task<IActionResult> MMSUsageCountUpdateAsync(long accountId, DateTime dateToCalculate)
        {
            try
            {
                await _mmsService.MMSUsageCountUpdateAsync(accountId, dateToCalculate);
                return Ok(APIResponse.Success(true));

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