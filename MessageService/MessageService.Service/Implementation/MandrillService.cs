using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class MandrillService : IMandrillService
    {
        public async Task<EmailResult> SendEmailAsync(string key, string body, string from, string to, string subject, IList<EmailAttachment> attachments = null)
        {
            MandrillApi mandrillApi = new MandrillApi(key);
            List<EmailResult> results = await mandrillApi.SendMessage(CreateEmailMessage(body, key, from, to, subject, attachments));

            return results.FirstOrDefault();

        }

        public async Task<List<EmailResult>> SendEmailsAsync(string key, string body, string from, string[] to, string subject, IList<EmailAttachment> attachments = null)
        {
            MandrillApi mandrillApi = new MandrillApi(key);
            return await mandrillApi.SendMessage(CreateEmailMessage(body, key, from, to, subject, attachments));

        }

        private SendMessageRequest CreateEmailMessage(string body, string key, string from, string[] to, string subject, IList<EmailAttachment> attachments)
        {

            List<EmailAddress> sendList = to.Select(k => new EmailAddress { Email = k, Name = "Information", Type = "to" }).ToList();

            EmailMessage emailMsg = new EmailMessage
            {
                FromEmail = from,
                Subject = subject,
                Html = body,
                To = sendList,
                Attachments = attachments

            };

            SendMessageRequest sndReq = new SendMessageRequest(emailMsg)
            {
                Async = false,
                IpPool = "",
                Key = key,
                Message = emailMsg
            };
            return sndReq;
        }

        private SendMessageRequest CreateEmailMessage(string body, string key, string from, string to, string subject, IList<EmailAttachment> attachments)
        {

            to = to ?? "";
            string[] tolist = to.Split(',');

            List<EmailAddress> sendList = tolist.Select(k => new EmailAddress { Email = k, Name = "Information", Type = "to" }).ToList();

            EmailMessage emailMsg = new EmailMessage
            {
                FromEmail = from,
                Subject = subject,
                Html = body,
                To = sendList,
                Attachments = attachments

            };

            SendMessageRequest sndReq = new SendMessageRequest(emailMsg)
            {
                Async = false,
                IpPool = "",
                Key = key,
                Message = emailMsg
            };
            return sndReq;
        }
    }
}
