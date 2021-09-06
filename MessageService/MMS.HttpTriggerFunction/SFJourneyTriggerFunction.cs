using System;
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

namespace MMS.HttpTriggerFunction
{
    public static class SFJourneyTriggerFunction
    {
        private static NLog.ILogger _logger = null;
        private static readonly IContainer Container;

        static SFJourneyTriggerFunction()
        {
            Container = new ContainerResolver().Container;
        }

        private static void GetLogger(string functionAppDirectory)

        {
            FunctionAppLogHelper.GetLogConfiguration(_logger, functionAppDirectory);
            _logger = LogManager.GetCurrentClassLogger();

        }

        [FunctionName("MMS")]
        public static async Task<IActionResult> GetJourneyData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            GetLogger(context.FunctionAppDirectory);
           
            string smsContent = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            _logger.Info(smsContent);
            try
            {
                var mmsService = Container.Resolve<IMMSService>();
                var result = await mmsService.SendJourneyMMSAsync(smsContent).ConfigureAwait(false);
                return new OkObjectResult(result);

            }
            catch (AggregateException agg)
            {
                _logger.Debug("Data:{smsContent}",smsContent);
                _logger.Error(agg.Flatten(), agg.Flatten().Message);
                log.LogCritical("MMS  HTTP trigger function Throw exception :" + agg.Flatten().Message);
                return (ActionResult)new OkObjectResult(false);
            }
            catch (Exception ex)
            {
                _logger.Debug("Data:{smsContent}", smsContent);
                _logger.Error(ex, ex.Message);
                log.LogCritical("MMS  HTTP trigger function Throw exception :" + ex.ToString());
                return (ActionResult)new OkObjectResult(false);
            }
            
        }
    }
}
