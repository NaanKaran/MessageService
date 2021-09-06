using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.MMSModels;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private static readonly string MandrilEmailKey =  AppSettings.GetValue("MandrillEmailKey");
        private static readonly string WeChatifyFromEmail = AppSettings.GetValue("WeChatifyFromEmail");
        private static string AlertEmailIds = AppSettings.GetValue("AlertEmailId");
        private readonly IMandrillService _mandrillService;

        public EmailService(IMandrillService mandrillService)
        {
            _mandrillService = mandrillService;
        }

        public async Task SendExceptionAlertEmail(string functionName, string parameters, Exception exception, string path)
        {

            const string subject = "MessageService Function - Exception Alert";

            using (StreamReader reader = new StreamReader(path))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{FUNCTIONNAME}", functionName);
                body = body.Replace("{PARAMETERS}", parameters);
                body = body.Replace("{EXCEPTION}", exception.ToString());
                body = body.Replace("{STACKTRACE}", exception.StackTrace);

                string[] toEmailIds = AlertEmailIds.Split(',');

                List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailIds, subject);
            }
        }

        public async Task SendExceptionAlertEmail(string functionName, string parameters, AggregateException exception, string path)
        {

            const string subject = "MessageService Function - Exception Alert";

            using (StreamReader reader = new StreamReader(path))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{FUNCTIONNAME}", functionName);
                body = body.Replace("{PARAMETERS}", parameters);
                body = body.Replace("{EXCEPTION}", exception.Flatten().ToString());
                body = body.Replace("{STACKTRACE}", exception.Flatten().StackTrace);

                string[] toEmailIds = AlertEmailIds.Split(',');

                List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailIds, subject);
            }
        }

        public async Task<bool> SendExportEmailAsync(string[] toEmailIds, string link, string accountName, string templatePath, string dateInterval, string subject, string heading)
        {
            using (StreamReader reader = new StreamReader(templatePath))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{AccountName}", accountName);
                body = body.Replace("{DownloadLink}", link);
                body = body.Replace("{DateInterval}", dateInterval);
                body = body.Replace("{ReportHeading}", heading);

                List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailIds, subject);
            }
            return true;
        }

        public async Task<bool> SendMMSBalanceThresholdAlertMail(MMSThersholdNotifyUser emailData, string templatePath, string subject)
        {
            foreach (var item in emailData.NotifyUserDetails)
            {
                using (StreamReader reader = new StreamReader(templatePath))
                {
                    string body = reader.ReadToEnd();
                    body = body.Replace("{Name}", item.Name);
                    body = body.Replace("{AccountName}", emailData.AccountName);
                    body = body.Replace("{ThresholdCount}", emailData.ThresholdCount.ToString());
                    List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, new[] { item.Email }, subject);
                }
            }
            return true;
        }
        public async Task<bool> SendMMSDeliveryReportAlertMail(string templatePath, string[] toEmailId, StringBuilder reportdata, string filePath, DateTime fromDate, DateTime toDate)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(templatePath))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{AccountNameAndCount}", reportdata.ToString());
            body = body.Replace("{From}", fromDate.ToString());
            body = body.Replace("{To}", toDate.ToString());
            string subject = "MMS Delivery Report";
            var xlsBytes = File.ReadAllBytes(filePath);
            List<Mandrill.Models.EmailAttachment> attachments = new List<Mandrill.Models.EmailAttachment>()
            {
                new Mandrill.Models.EmailAttachment()
            {
                Content = Convert.ToBase64String(xlsBytes),
                Name = $@"{subject}_{DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.xlsx",
                Type = "application/xlsx",
                Base64 = true
            }
            };
            List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailId, subject,attachments);
            return true;
        }
        public async Task<bool> SendUpdateEmailForBalanceThreshold(string templatePath,string accountName,string userName,string requestedUserName,string CurrentEmail,string CancelledOnString,long RequestCount,long PreviousCount,string email,string subject)
        {
            using (StreamReader reader = new StreamReader(templatePath))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{UserName}", userName);
                body = body.Replace("{FirstName}", requestedUserName);
                body = body.Replace("{PreviousCount}", PreviousCount.ToString());
                body = body.Replace("{RequestCount}", RequestCount.ToString());
                body = body.Replace("{CancelledOnString}", CancelledOnString);
                body = body.Replace("{AccountName}", accountName);
                List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, new[] { email }, subject);
            }

            return true;
        }

        public async Task<bool> SendTopUpRequestEmailAsync(string[] toEmailIds, string accountName, string templatePath, string userName, string requestedBy, string requestEmailId, int topUpCount)
        {
            var subject = "SMS Top-up Request";
            using (StreamReader reader = new StreamReader(templatePath))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{UserName}", userName);
                body = body.Replace("{AccountName }", accountName);
                body = body.Replace("{RequestedBy}", requestedBy);
                body = body.Replace("{TopUpCount}", topUpCount.ToString());
                body = body.Replace("{RequestedByEmailId}", requestEmailId);

                List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailIds, subject);
            }
            return true;
        }

        public async Task<bool> SendDeliveryReportEmailAsync(string[] toEmailIds, string templatePath, string pathDirectory, string excelFileName, StringBuilder reports, DateTime fromDate, DateTime toDate, string subject)
        {  
            var xlsBytes = File.ReadAllBytes(Path.Combine(pathDirectory, excelFileName));

            var attachments = new List<Mandrill.Models.EmailAttachment>()
            {
                new Mandrill.Models.EmailAttachment()
                {
                    Content = Convert.ToBase64String(xlsBytes),
                    Name = $@"{subject}_{DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.xlsx",
                    Type = "application/xlsx",
                    Base64 = true
                }

            };


            using (StreamReader reader = new StreamReader(templatePath))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{AccountNameAndCount}", reports.ToString());
                body = body.Replace("{From}", fromDate.ToDateTimeString());
                body = body.Replace("{To}", toDate.ToDateTimeString());

                var emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailIds, subject, attachments);
            }
            return true;
        }

        public async Task<bool> SMSThresholdNotificationEmailAsync(string toEmailId, string templatePath,
            string accountName, string userName, int thresholdCount)
        {

            var toEmailIds = new[] {toEmailId};
            var subject = "SMS Threshold Notification";
            using (StreamReader reader = new StreamReader(templatePath))
            {
                string body = reader.ReadToEnd();
                body = body.Replace("{UserName}", userName);
                body = body.Replace("{AccountName}", accountName);
                body = body.Replace("{ThresholdCount}", thresholdCount.ToString());

                List<Mandrill.Models.EmailResult> emailResult = await _mandrillService.SendEmailsAsync(MandrilEmailKey, body, WeChatifyFromEmail, toEmailIds, subject);
            }
            return true;
        }
    }
}
