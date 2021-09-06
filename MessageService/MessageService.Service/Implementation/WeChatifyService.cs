using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService.CosmosRepository.Interface;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class WeChatifyService : IWeChatifyService
    {
        private readonly IWeChatifyRepository _wechatifyRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ISettingsCosmosRepository _settingsCosmosRepository;
        public WeChatifyService(IWeChatifyRepository wechatifyRepository, ISettingsRepository settingsRepository, ISettingsCosmosRepository settingsCosmosRepository)
        {
            _wechatifyRepository = wechatifyRepository;
            _settingsRepository = settingsRepository;
            _settingsCosmosRepository = settingsCosmosRepository;
        }

        public async Task<WeChatifyAccountModel> GetAccountDetailsAsync(long accountId)
        {
            return await _wechatifyRepository.GetAccountDetailsAsync(accountId);
        }

        public Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId)
        {
            return _wechatifyRepository.GetAllWeChatifyUsersAsync(accountId);
        }
        public Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId,int pg,int numberOfRecords)
        {
            return _wechatifyRepository.GetAllWeChatifyUsersAsync(accountId, pg,numberOfRecords);
        }

        public async Task<IEnumerable<WeChatifySFAccountModel>> GetAllMmsMappedSFAccountsAsync()
        {
            var accountIds = await _settingsRepository.GetMMSMappedAccountIdsAsync();
            return await _wechatifyRepository.GetSFAccountDetailsAsync(accountIds.ToArray());
        }

        public async Task<IEnumerable<WeChatifySFAccountModel>> GetAllMmsMappedSFAccountsAsync(string id)
        {
            var accountIds = await _settingsRepository.GetMMSMappedAccountIdsAsync();
            var userId = await _wechatifyRepository.GetUserIdAsync(id);
            return await _wechatifyRepository.GetSFMappedAccountsAsync(userId,accountIds.ToArray());
        }

        public async Task<IEnumerable<WeChatifySFAccountModel>> GetAllSMSMappedSFAccountsAsync(string id)
        {
            var vendorSettings = await _settingsCosmosRepository.GetAllSMSVendorSettingsAsync();
            var userId = await _wechatifyRepository.GetUserIdAsync(id);
            return await _wechatifyRepository.GetSFMappedAccountsAsync(userId, vendorSettings.Select(k=>k.AccountId).ToArray());
        }

        public async Task<IEnumerable<WeChatifySFAccountModel>> GetAllSMSMappedSFAccountsAsync()
        {
            var vendorSettings = await _settingsCosmosRepository.GetAllSMSVendorSettingsAsync();
            return await _wechatifyRepository.GetSFAccountDetailsAsync(vendorSettings.Select(k=>k.AccountId).ToArray());
        }

        public async Task<IEnumerable<string>> GetWeChatifyAccountNamesAsync(List<long> accountIds)
        {
            return await _wechatifyRepository.GetWeChatifyAccountNamesAsync(accountIds);
        }
    }
}
