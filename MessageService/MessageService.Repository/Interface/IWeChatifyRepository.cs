using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models.CosmosModel;
using MessageService.Models.WeChatifyModels;

namespace MessageService.Repository.Interface
{
    public interface IWeChatifyRepository
    {
        Task<WeChatUserModel> GetFollowerDetailsAsync(string openId);
        Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId);
        Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId, int pg, int numberOfRecords);
        Task<WeChatifyAccountModel> GetAccountDetailsAsync(long accountId);
        Task<IEnumerable<WeChatifySFAccountModel>> GetSFAccountDetailsAsync(long[] accountIds);
        Task<string> GetUserIdAsync(string id);
        Task<WeChatifyUserModel> GetUserByIdAsync(string userId);
        Task<IEnumerable<WeChatifySFAccountModel>> GetSFMappedAccountsAsync(string userId, long[] accountIds);
        Task<WeChatifySFAccountModel> GetSFMappedAccountAsync(long accountId);
        Task<IEnumerable<long>> GetSMSMappedAccountIdsAsync();
        Task<IEnumerable<string>> GetWeChatifyAccountNamesAsync(List<long> accountIds);
        Task<WeChatifyUserModel> GetUsersAsync(string userId);
        Task<int> AddOrUpdateSMSVendorSettingsAsync(Models.CosmosModel.SMSVendorSettingsDocumentModel model);
        Task<IEnumerable<WeChatifyUserModel>> GetAllSMSWeChatifyUsersAsync(long accountId);
        Task<bool> UpdateSMSCampaignNumbersAsync(SMSLogDocumentModel logModel);
    }
}
