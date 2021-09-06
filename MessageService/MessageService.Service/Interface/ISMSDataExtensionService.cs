using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MessageService.Service.Interface
{
    public interface ISMSDataExtensionService
    {
        Task<string> GetDataExtensionKey(string orgId, string oauthToken, string deName, string soapEndPointUrl,
             bool isSharedDE);
        Task UpdateSMSLogToDataExtensionAsync(ILogger log);
        Task UpdateIncomingSMSLogToDataExtensionAsync(ILogger log);
    }
}
