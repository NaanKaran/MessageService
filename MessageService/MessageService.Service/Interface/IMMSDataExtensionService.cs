using System.Threading.Tasks;

namespace MessageService.Service.Interface
{
    public interface IMMSDataExtensionService
    {
        Task UpdateMmsLogToDataExtension(long accountId);
        Task<string> GetDataExtensionKey(string orgId, string oauthToken, string deName, string soapEndPointUrl,
             bool isSharedDE);
        Task UpdateIncomingMmsLogToDataExtension(long accountId);
    }
}
