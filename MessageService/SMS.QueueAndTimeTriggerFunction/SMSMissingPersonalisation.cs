using Autofac;
using MessageService.Service.Interface;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMS.QueueAndTimeTriggerFunction
{
    public class SMSMissingPersonalisation
    {
        [FunctionName("SMSProcessMissedPersonalisation")]
        public static void SMSProcessMissedPersonalisation([QueueTrigger("smspersonalisationmissingqueue", Connection = "AzureWebJobsStorage")]string queueData, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"SMSProcessMissedPersonalisation Queue trigger function Started at: {DateTime.Now}");

            try
            {
                using (var scope = new ContainerResolver().Container.BeginLifetimeScope())
                {
                    var smsService = scope.Resolve<ISMSService>();
                    var result = smsService.ProcessSMSJourneyWithoutPersonalisation(queueData,log).Result;
                }
            }
            catch (AggregateException e)
            {
                log.LogError($"SMSUpdateJourneyFromSFFunction Error occured at: " + DateTime.UtcNow + "Exception: " + e.ToString());
            }
            catch (Exception e)
            {
                log.LogError($"SMSUpdateJourneyFromSFFunction Error occured at: " + DateTime.UtcNow + "Exception: " + e.ToString());
            }
        }
    }
}
