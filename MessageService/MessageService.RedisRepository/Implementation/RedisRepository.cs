using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.CosmosModel;
using MessageService.Models.SMSModels;
using MessageService.RedisRepository.Interface;
using MessageService.RedisRepository.Utility;

namespace MessageService.RedisRepository.Implementation
{
    public class RedisRepository : IRedisRespository
    {
        private readonly IRedisCache _cache;

        public RedisRepository(IRedisCache cache)
        {
            _cache = cache;
        }
        public async Task<VendorSettingsModel> GetMMSVendorSettingsAsync(long accountId)
        {
            return await _cache.GetAsync<VendorSettingsModel>(RedisKeys.MMSVendorSettings + accountId).ConfigureAwait(false);
        }

        public async Task<VendorSettingsModel> GetMMSVendorSettingsAsync(string appId)
        {
            return await _cache.GetAsync<VendorSettingsModel>(RedisKeys.MMSVendorSettings + appId).ConfigureAwait(false);
        }

        public async Task<bool> UpdateMMSVendorSettingsAsync(VendorSettingsModel model)
        {
            if (model.IsNull())
            {
                return false;
            }
            await _cache.SetAsync(RedisKeys.MMSVendorSettings + model.AccountId, model).ConfigureAwait(false);
            return await _cache.SetAsync(RedisKeys.MMSVendorSettings + model.AppId, model).ConfigureAwait(false);
        }

        public async Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(long accountId)
        {
            return await _cache.GetAsync<SMSVendorSettingsDocumentModel>(RedisKeys.SMSVendorSettings + accountId).ConfigureAwait(false);
        }    

        public async Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(string appId)
        {
            return await _cache.GetAsync<SMSVendorSettingsDocumentModel>(RedisKeys.SMSVendorSettings + appId).ConfigureAwait(false);
        }

        public async Task<bool> UpdateSMSVendorSettingsAsync(SMSVendorSettingsDocumentModel model)
        {
            if (model.IsNull())
            {
                return false;
            }
            await _cache.SetAsync(RedisKeys.SMSVendorSettings + model.AccountId, model).ConfigureAwait(false);
            return await _cache.SetAsync(RedisKeys.SMSVendorSettings + model.AppId, model).ConfigureAwait(false);
        }

        public async Task<bool> AddToRedisAsync<T>(string key, T data)
        {
            return await _cache.SetAsync(key, data).ConfigureAwait(false);
        }

        public async Task<T> GetDataFromRedisAsync<T>(string key)
        {
            return await _cache.GetAsync<T>(key).ConfigureAwait(false);
        }

        public async Task<List<SMSSFInteractionModel>> GetSMSSFInteractionsAsync(long accountId)
        {
            return await _cache.GetAsync<List<SMSSFInteractionModel>>(RedisKeys.SMSSFInteractions + accountId).ConfigureAwait(false);
        }
        public async Task<bool> AddOrUpdateSMSSFInteractionsAsync(List<SMSSFInteractionModel> model, long accountId)
        {
            return await _cache.SetAsync(RedisKeys.SMSSFInteractions + accountId, model).ConfigureAwait(false);
        }
    }
}
