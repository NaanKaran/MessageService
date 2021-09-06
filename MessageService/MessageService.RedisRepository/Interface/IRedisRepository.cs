using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.CosmosModel;
using MessageService.Models.SMSModels;

namespace MessageService.RedisRepository.Interface
{
    public interface IRedisRespository
    {
        Task<bool> UpdateMMSVendorSettingsAsync(VendorSettingsModel model);
        Task<VendorSettingsModel> GetMMSVendorSettingsAsync(long accountId);
        Task<VendorSettingsModel> GetMMSVendorSettingsAsync(string appId);
        Task<bool> UpdateSMSVendorSettingsAsync(SMSVendorSettingsDocumentModel model);
        Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(long accountId);
        Task<SMSVendorSettingsDocumentModel> GetSMSVendorSettingsAsync(string appId);
        Task<bool> AddToRedisAsync<T>(string key, T data);
        Task<T> GetDataFromRedisAsync<T>(string key);
        Task<List<SMSSFInteractionModel>> GetSMSSFInteractionsAsync(long accountId);
        Task<bool> AddOrUpdateSMSSFInteractionsAsync(List<SMSSFInteractionModel> model, long accountId);
    }
}
