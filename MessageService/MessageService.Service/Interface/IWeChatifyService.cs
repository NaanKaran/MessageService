using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models.WeChatifyModels;

namespace MessageService.Service.Interface
{
    public interface IWeChatifyService
    {
        Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId);
        Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId, int pg, int numberOfRecords);
        Task<WeChatifyAccountModel> GetAccountDetailsAsync(long accountId);
        Task<IEnumerable<WeChatifySFAccountModel>> GetAllMmsMappedSFAccountsAsync();
        Task<IEnumerable<WeChatifySFAccountModel>> GetAllMmsMappedSFAccountsAsync(string id);
        Task<IEnumerable<WeChatifySFAccountModel>> GetAllSMSMappedSFAccountsAsync(string id);
        Task<IEnumerable<WeChatifySFAccountModel>> GetAllSMSMappedSFAccountsAsync();
        Task<IEnumerable<string>> GetWeChatifyAccountNamesAsync(List<long> accountIds);
    }
}
