using System.Collections.Generic;
using System.Threading.Tasks;
using Mandrill.Models;

namespace MessageService.Service.Interface
{
    public interface IMandrillService
    {
        Task<EmailResult> SendEmailAsync(string key, string body, string from, string to, string subject, IList<EmailAttachment> attachments = null);
        Task<List<EmailResult>> SendEmailsAsync(string key, string body, string from, string[] to, string subject, IList<EmailAttachment> attachments = null);
    }
}
