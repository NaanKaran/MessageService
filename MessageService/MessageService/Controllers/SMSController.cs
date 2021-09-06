using System;
using System.Net;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.ExportModels;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageService.Controllers
{
    [ApiController]
    public class SMSController : ControllerBase
    {
        private readonly ILogger<SMSController> _log;
        private readonly ISMSService _smsService;

        public SMSController(ILogger<SMSController> log, ISMSService smsService)
        {
            _log = log;
            _smsService = smsService;

        }

        [HttpPost]
        [Route("api/[controller]/GetJournies")]
        public async Task<IActionResult> GetJournies(JourneyFilterModel filterModel)
        {
            try
            {
                var result = await _smsService.GetJourneysAsync(filterModel);
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

        [HttpPost]
        [Route("api/[controller]/GetIncomingMessages")]
        public async Task<IActionResult> GetIncomingMessages(GetIncomingMessagesModel model)
        {
            try
            {
                var result = await _smsService.GetIncomingMessagesAsync(model);
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
        [Route("api/[controller]/GetVerificationSMS")]
        public async Task<IActionResult> GetVerificationSMS(GetVerificationSMSModel model)
        {
            try
            {
                var result = await _smsService.GetVerificationSMSAsync(model);
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
        [Route("api/[controller]/SendVerificationSMS")]
        public async Task<IActionResult> SendVerificationSMS(VerificationSMSModel model)
        {
            try
            {
                var result = await _smsService.SendVerificationSMSAsync(model);
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
                await _smsService.AddQueueForIncomingMessagesExportAsync(model);
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
        [Route("api/[controller]/ExportVerificationSMS")]
        public async Task<IActionResult> ExportVerificationSMS(VerificationSMSExportModel model)
        {
            try
            {
                await _smsService.AddQueueForVerificationSMSExportAsync(model);
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
                await _smsService.AddQueueForOptOutMessagesExportAsync(model);
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
        [Route("api/[controller]/ExportJournies")]
        public async Task<IActionResult> ExportJournies(JourneyExportModel model)
        {
            try
            {
                await _smsService.AddQueueForJourneysExportAsync(model);
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
        [Route("api/[controller]/ExportSMSLog")]
        public async Task<IActionResult> ExportSMSLog(LogFilterModel logFilter)
        {
            try
            {
                await _smsService.AddQueueForSMSLogExportAsync(logFilter);
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
        [Route("api/[controller]/SalesForceJourneyActivatePush")]
        public async Task<IActionResult> SalesForceJourneyActivatePush(JourneyActivateModel data)
        {
            try
            {
                _log.LogInformation(ParamLogHelper.GetLogString(() => data), data.ToJsonString());
                await _smsService.AddJourneyEntryAsync(data).ConfigureAwait(false);
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
        [Route("api/[controller]/GetSMSBalance")]
        public async Task<IActionResult> GetSMSBalance(long accountid)
        {
            try
            {
                var result = await _smsService.GetSMSBalanceAsync(accountid);
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
        [Route("api/[controller]/GetAllJourney")]
        public async Task<IActionResult> GetAllJourney(long accountid)
        {
            try
            {
                var result = await _smsService.GetAllJourneyAsync(accountid);
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
        [Route("api/[controller]/GetJourneyDetails")]
        public async Task<IActionResult> GetJourneyDetails(long accountId, string journeyId)
        {
            try
            {
                var result = await _smsService.GetJourneyDetailsAsync(accountId, journeyId);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => journeyId, () => accountId), journeyId, accountId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => journeyId, () => accountId), journeyId, accountId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetSMSLogDetail")]
        public async Task<IActionResult> GetSMSLogDetail(LogFilterModel logFilter)
        {
            try
            {
                var result = await _smsService.GetSMSLogDetailsAsync(logFilter);
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
        [Route("api/[controller]/ReprocessFailedQueue")]
        public async Task<IActionResult> ReprocessFailedQueue(string queueName)
        {
            try
            {
                await _smsService.ReprocessQueueAsync(queueName);
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
        [Route("api/[controller]/GetJourneySMSCount")]
        public async Task<IActionResult> GetJourneySMSCount(string journeyId)
        {
            try
            {
                var result = await _smsService.GetSMSLogCountInJourneyAsync(journeyId);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => journeyId), journeyId);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => journeyId), journeyId);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetSMSUsageReport")]
        public async Task<IActionResult> GetSMSUsageReport(long accountId, int? year)
        {
            try
            {
                var result = await _smsService.GetSMSMonthlyUsageAsync(accountId, year);
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
        [Route("api/[controller]/GetSMSRechargeHistory")]
        public async Task<IActionResult> GetSMSRechargeHistory(long accountId, int year, int month)
        {
            try
            {
                var result = await _smsService.GetRechargeHistoryAsync(accountId, year, month);
                return Ok(APIResponse.Success(result));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => year, () => month), accountId, year, month);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => year, () => month), accountId, year, month);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/UpdateSMSMonthlyUsage")]
        public async Task<IActionResult> UpdateSMSMonthlyUsage(long accountId, DateTime startDate, DateTime endDate)
        {
            try
            {
                await _smsService.UpdateSMSMonthlyUsageAsync(accountId, startDate, endDate);
                return Ok(APIResponse.Success(true));

            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => startDate, () => endDate), accountId, startDate, endDate);
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => accountId, () => startDate, () => endDate), accountId, startDate, endDate);
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        //[HttpGet]
        //[Route("api/[controller]/DeleteAllRecords")]
        //public async Task<IActionResult> DeleteAllRecords(long accountId)
        //{
        //    try
        //    {
        //        await _smsService.CustomQueryExecutorAsync(accountId);
        //        return Ok(APIResponse.Success(true));

        //    }
        //    catch (AggregateException agg)
        //    {
        //        _log.LogDebug(ParamLogHelper.GetLogString(() => accountId), accountId);
        //        _log.LogError(agg.Flatten(), agg.Flatten().Message);
        //        return Ok(APIResponse.Error(agg.Flatten().Message));
        //    }
        //    catch (Exception e)
        //    {
        //        _log.LogDebug(ParamLogHelper.GetLogString(() => accountId), accountId);
        //        _log.LogError(e, e.Message);
        //        return Ok(APIResponse.Error(e.Message));
        //    }
        //}
    }
}