using System;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageService.Controllers
{
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplateController> _log;
        public TemplateController(ITemplateService templateService, ILogger<TemplateController> logger)
        {
            _templateService = templateService;
            _log = logger;
        }
        [HttpPost]
        [Route("api/[controller]/SaveTemplate")]
        public async Task<IActionResult> SaveTemplate(MMSTemplateModel mmsTemplateModel)
        {
            try
            {
                var result = await _templateService.SaveAsync(mmsTemplateModel);

                return Ok(result.Status == "success" ? APIResponse.Success(result) : APIResponse.Error(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => mmsTemplateModel), mmsTemplateModel.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => mmsTemplateModel), mmsTemplateModel.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/UpdateTemplate")]
        public async Task<IActionResult> UpdateTemplate(MMSTemplateModel mmsTemplateModel)
        {
            try
            {
                var result = await _templateService.UpdateAsync(mmsTemplateModel);
                return Ok(result.Status == "success" ? APIResponse.Success(result) : APIResponse.Error(result));
            }
            catch (AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => mmsTemplateModel), mmsTemplateModel.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => mmsTemplateModel), mmsTemplateModel.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpGet]
        [Route("api/[controller]/GetTemplate")]
        public async Task<IActionResult> GetTemplate(string id, long accountid)
        {
            try
            {
                var result = await _templateService.GetAsync(id, accountid);
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
        [Route("api/[controller]/DeleteTemplate")]
        public async Task<IActionResult> DeleteTemplate([FromForm]string id, [FromForm]long accountid)
        {
            try
            {
                var result = await _templateService.DeleteTemplateAsync(id, accountid);
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
        [Route("api/[controller]/GetTemplates")]
        public async Task<IActionResult> GetTemplates(GetTemplateModel model)
        {
            try
            {
                var result = await _templateService.GetAsync(model);
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
        [Route("api/[controller]/GetActiveTemplates")]
        public async Task<IActionResult> GetActiveTemplates(long accountid)
        {
            try
            {
                var result = await _templateService.GetActiveTemplatesAsync(accountid);
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
        [Route("api/[controller]/UpdateTemplateStatusAsync")]
        public async Task<IActionResult> UpdateTemplateId(TemplateUpdateModel templateUpdate)
        {
            try
            {
                var result = await _templateService.UpdateTemplateId(templateUpdate);
                return Ok(APIResponse.Success(result));
            }
            catch(AggregateException agg)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => templateUpdate), templateUpdate.ToJsonString());
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogDebug(ParamLogHelper.GetLogString(() => templateUpdate), templateUpdate.ToJsonString());
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }

        }
    }
}