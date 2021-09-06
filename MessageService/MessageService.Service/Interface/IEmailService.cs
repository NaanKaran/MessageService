using MessageService.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using MessageService.Models.MMSModels;

namespace MessageService.Service.Interface
{
    public interface IEmailService
    {
        Task<bool> SendExportEmailAsync(string[] toEmailIds, string link, string accountName, string templatePath,
            string dateInterval, string subject, string heading);
        Task SendExceptionAlertEmail(string functionName, string parameters, Exception exception, string path);
        Task SendExceptionAlertEmail(string functionName, string parameters, AggregateException exception, string path);

        Task<bool> SendTopUpRequestEmailAsync(string[] toEmailIds, string accountName, string templatePath,
            string userName, string requestedBy, string requestEmailId, int topUpCount);

        Task<bool> SendDeliveryReportEmailAsync(string[] toEmailIds, string templatePath, string pathDirectory,
            string excelFileName, StringBuilder reports, DateTime fromDate, DateTime toDate, string subject);

        Task<bool> SMSThresholdNotificationEmailAsync(string toEmailId, string templatePath, string accountName,
            string userName, int thresholdCount);
        Task<bool> SendMMSBalanceThresholdAlertMail(MMSThersholdNotifyUser emailData, string templatePath, string subject);
        Task<bool> SendMMSDeliveryReportAlertMail(string templatePath, string[] toEmailId, StringBuilder reportdata, string filePath, DateTime fromDate, DateTime toDate);
        Task<bool> SendUpdateEmailForBalanceThreshold(string templatePath, string accountName, string userName, string requestedUserName, string CurrentEmail, string CancelledOnString, long RequestCount, long PreviousCount, string email, string subject);
    }
}
