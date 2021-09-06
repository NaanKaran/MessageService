using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Autofac;
using MessageService.Service.Interface;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using Newtonsoft.Json;

namespace SMS.HttpTriggerFunction
{
    public static class SMSHttpTriggerFunction
    {
        private static NLog.ILogger _logger = null;
        private static readonly IContainer Container;

        static SMSHttpTriggerFunction()
        {
            Container = new ContainerResolver().Container;
        }

        private static void GetLogger(string functionAppDirectory)

        {
            FunctionAppLogHelper.GetLogConfiguration(_logger, functionAppDirectory);
            _logger = LogManager.GetCurrentClassLogger();

        }

        [FunctionName("SendSMS")]
        public static async Task SendSMSFunction([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            string smsContent = eventGridEvent.Data.ToJsonString();

            try
            {
                var smsService = Container.Resolve<ISMSService>();
                var result = await smsService.SendJourneySMSAsync(smsContent).ConfigureAwait(false);
            }
            catch (AggregateException agg)
            {
                _logger.Debug("Data:{smsContent}", smsContent);
                _logger.Error(agg.Flatten(), agg.Flatten().Message);
                log.LogCritical("SMS  HTTP trigger function Throw exception :" + agg.Flatten().Message);
            }
            catch (Exception ex)
            {
                _logger.Debug("Data:{smsContent}", smsContent);
                _logger.Error(ex, ex.Message);
                log.LogCritical("SMS  HTTP trigger function Throw exception :" + ex.ToString());
            }

        }

        //[FunctionName("PublishSMSEvent")]
        [FunctionName("SfNonPIIWebHook")]        
        public static async Task<IActionResult> PublishSMSEventFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            GetLogger(context.FunctionAppDirectory);

            string smsContent = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            
            if (smsContent.IsNullOrWhiteSpace())
            {
                return new OkObjectResult("");
            }

            try
            {
                var smsService = Container.Resolve<ISMSService>();
                var result = await smsService.PublishSMSEventsAsync(smsContent).ConfigureAwait(false);
                return new OkObjectResult(result);
            }
            catch (AggregateException agg)
            {
                _logger.Debug("Data:{smsContent}",smsContent);
                _logger.Error(agg.Flatten(), agg.Flatten().Message);
                log.LogCritical("SMS  HTTP trigger function Throw exception :" + agg.Flatten().Message);
                return new OkObjectResult(false);
            }
            catch (Exception ex)
            {
                _logger.Debug("Data:{smsContent}", smsContent);
                _logger.Error(ex, ex.Message);
                log.LogCritical("SMS  HTTP trigger function Throw exception :" + ex.ToString());
                return new OkObjectResult(false);
            }
            
        }

        [FunctionName("SubmailCallback")]
        public static async Task<IActionResult> SubmailCallbackFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            string data = string.Empty;
            log.LogInformation("SMS SubmailCallback HTTP trigger function processed a request.");
            GetLogger(context.FunctionAppDirectory);
            
            try
            {
                var obj = new ExpandoObject();
                foreach (var key in req.Form.Keys)
                {
                    ((IDictionary<string, object>)obj)[key] = Convert.ToString(req.Form[key]);
                }

                var smsService = Container.Resolve<ISMSService>();             
                data = JsonConvert.SerializeObject(obj);

                _logger.Info("SubmailCallbackFunction - Data:{data}", data); 

                await smsService.UpdateSMSLogAsync(data).ConfigureAwait(false);
                return new OkObjectResult(true);
            }
            catch (AggregateException agg)
            {
                _logger.Debug("Data:{data}", data);
                _logger.Error(agg.Flatten(), agg.Flatten().Message);
                log.LogCritical("SMS SubmailCallback HTTP trigger function Throw exception :" + agg.Flatten().Message);
                return new OkObjectResult(false);
            }
            catch (Exception ex)
            {
                _logger.Debug("Data:{data}", data);
                _logger.Error(ex, ex.Message);
                log.LogCritical("SMS SubmailCallback HTTP trigger function Throw exception :" + ex.ToString());
                return new OkObjectResult(false);
            }

        }

        [FunctionName("SalesForceJourneyActivatePush")]
        public static async Task<IActionResult> SalesForceJourneyActivatePushFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# SalesForceJourneyActivatePush HTTP trigger function processed a request.");

            GetLogger(context.FunctionAppDirectory);            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            
            try
            {
                var smsService = Container.Resolve<ISMSService>();
                JourneyActivateModel data = JsonConvert.DeserializeObject<JourneyActivateModel>(requestBody);
                var result = await smsService.AddJourneyEntryAsync(data).ConfigureAwait(false);
                return new OkObjectResult(result);
            }
            catch (AggregateException agg)
            {
                _logger.Debug("Data:{requestBody}", requestBody);
                _logger.Error(agg.Flatten(), agg.Flatten().Message);
                log.LogCritical("SMS HTTP trigger function Throw exception :" + agg.Flatten().Message);
                return new OkObjectResult(false);
            }
            catch (Exception ex)
            {
                _logger.Debug("Data:{requestBody}", requestBody);
                _logger.Error(ex, ex.Message);
                log.LogCritical("SMS HTTP trigger function Throw exception :" + ex.ToString());
                return new OkObjectResult(false);
            }

        }

        [FunctionName("EmulateSend")]
        public static async Task<IActionResult> EmulateSendFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            GetLogger(context.FunctionAppDirectory);

            string submailPostModel = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

            try
            {
                var smsService = Container.Resolve<ISMSService>();
                var result = smsService.EmulateSubmailSMSXSend(submailPostModel);
                return new OkObjectResult(result);
            }
            catch (AggregateException agg)
            {
                _logger.Debug("Data:{submailPostModel}", submailPostModel);
                _logger.Error(agg.Flatten(), agg.Flatten().Message);
                log.LogCritical("SMS  HTTP trigger function Throw exception :" + agg.Flatten().Message);
                return new OkObjectResult(false);
            }
            catch (Exception ex)
            {
                _logger.Debug("Data:{submailPostModel}", submailPostModel);
                _logger.Error(ex, ex.Message);
                log.LogCritical("SMS  HTTP trigger function Throw exception :" + ex.ToString());
                return new OkObjectResult(false);
            }

        }

        [FunctionName("BlobTrigger")]
        public static void BlobTriggerFunction([BlobTrigger("smsdeadletter", Connection = "AzureWebJobsStorage")]string json, ILogger log)
        {
            log.LogInformation($"BlobTriggerFunction Started");
            try
            {
                var smsService = Container.Resolve<ISMSService>();
                var result = smsService.AddQueueForDeadLetterProcessingAsync(json);
            }
            catch (AggregateException agg)
            {
                log.LogCritical("SMS Blob trigger function Throw exception :" + agg.Flatten().Message);
            }
            catch (Exception ex)
            {
                log.LogCritical("SMS Blob trigger function Throw exception :" + ex.ToString());
            }
        }
    }
}
