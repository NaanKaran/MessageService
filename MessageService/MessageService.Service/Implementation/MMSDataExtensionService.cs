using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class MMSDataExtensionService : IMMSDataExtensionService
    {
        private readonly IWeChatifyRepository _weChatifyRepository;
        private readonly IMMSRepository _mmsRepository;        
        private readonly IAzureStorageRepository _azureStorageRepository;
        private static readonly string MMSLogDETableName = AppSettings.GetValue("SalesForce:WeChatifyMMSLog");
        private static readonly string MMSIncomingLogDETableName = AppSettings.GetValue("SalesForce:WeChatifyIncomingMMSLog");
        private static readonly string WechatifyCallsBaseUrl = AppSettings.GetValue("WeChatifyCallsBaseUrl");
        public MMSDataExtensionService(IWeChatifyRepository weChatifyRepository, IMMSRepository mmsRepository, IAzureStorageRepository azureStorageRepository)
        {
            _weChatifyRepository = weChatifyRepository;
            _mmsRepository = mmsRepository;
            _azureStorageRepository = azureStorageRepository;           
        }
        public async Task UpdateMmsLogToDataExtension(long accountId)
        {

            var (accountModel, authenticationTable, dataExtensionKey) = await GetDataExtensionKeyAsync(accountId, MMSLogDETableName);

            if (dataExtensionKey.IsNull())
            {
                return;
            }
            var logData = await _mmsRepository.GetMmsLogForUpdateToDEAsync(accountId);
            var deData = logData.Select(k => new WeChatifyDataExtensionMMSLog()
            {
                AccountId = k.AccountId.ToString(),
                ActivityId = k.ActivityId,
                ActivityName = k.ActivityName ?? "",
                DeliveryDate = k.DeliveryDateString ?? "",
                DeliveryStatus = k.DeliveryStatusString ?? "",
                DropErrorCode = k.DropErrorCode ?? "",
                DynamicParamsValue = k.DynamicParamsValue ?? "",
                ErrorMessage = k.ErrorMessage ?? "",
                InteractionId = k.InteractionId,
                JourneyId = k.JourneyId,
                JourneyName = k.JourneyName ?? "",
                MMSTemplateId = k.MMSTemplateId ?? "",
                MMSTemplateName = k.MMSTemplateName ?? "",
                MobileNumber = k.MobileNumber ?? "",
                SendDate = k.SendDateString,
                SendId = k.SendId,
                SentStatus = k.SentStatusString,
                VersionName = k.VersionName
            }).OrderBy(y => y.SendId).ToList();

            var count = deData.Count();
            for (var i = 0; i < count; i += 1000)
            {
                var log = deData.Skip(i).Take(1000).ToList();
                var result = await DataExtensionRepository.AddOrUpdateDataExtenstionByKeyAsync(log,
                    authenticationTable, dataExtensionKey, accountModel.IsSharedDEConfigured);
                var re = await _mmsRepository.UpdateDataExtensionPushInMMSLogAsync(log);


            }

        }

        public async Task UpdateIncomingMmsLogToDataExtension(long accountId)
        {

            var (accountModel, authenticationTable, dataExtensionKey) = await GetDataExtensionKeyAsync(accountId, MMSIncomingLogDETableName);

            if (dataExtensionKey.IsNull())
            {
                return;
            }

            var logData = await _mmsRepository.GetIncomingMessagesToUpdateDEAsync(accountId);
            var deData = logData.Select(k => new WeChatifyDataExtensionIncomingLog()
            {
                AccountId = k.AccountId.ToString(),
                Content = k.Content ?? "",
                CreatedOn = k.CreatedOnString,
                JourneyName = k.JourneyName ?? "",
                MobileNumber = k.MobileNumber ?? "",
                Id = k.Id.ToString(),
                IsOptOut = k.IsOptOut ? "Yes" : "No"
            }).OrderBy(y => y.Id).ToList();

            var count = deData.Count();
            for (var i = 0; i < count; i += 1000)
            {
                var log = deData.Skip(i).Take(1000).ToList();
                var result = await DataExtensionRepository.AddOrUpdateDataExtenstionByKeyAsync(log, authenticationTable, dataExtensionKey, accountModel.IsSharedDEConfigured);
                var re = await _mmsRepository.UpdateDataExtensionPushInIncomingMessageAsync(log);

            }



        }

        public async Task<string> GetDataExtensionKey(string orgId, string oauthToken, string deName, string soapEndPointUrl,
            bool isSharedDE)
        {
            var url = WechatifyCallsBaseUrl + "api/SfApi/GetDataExtensionKey";
            var data = new Dictionary<string, string>()
            {
                {"orgId" , orgId },{"oauthToken",oauthToken},{"deName",deName},{"soapEndPointUrl",soapEndPointUrl},{"isSharedDE", isSharedDE.ToString()}

            };
            var customerKey = await HttpHelper<string>.HttpGetAsync(url, data);
            return customerKey;
        }

        private async Task<(WeChatifySFAccountModel, SalesForceAuthenticationModel, string)> GetDataExtensionKeyAsync(long accountId, string dataExtensionName)
        {
            var accountModel = await _weChatifyRepository.GetSFMappedAccountAsync(accountId);
            var authenticationtable = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication",
                "RowKey", accountModel.OrganizationId, "eq");
            var dataExtensionKey = await GetDataExtensionKey(authenticationtable.OrganizationId, authenticationtable.LegacyToken,
                dataExtensionName, authenticationtable.SoapEndPointUrl, accountModel.IsSharedDEConfigured);
            return (accountModel, authenticationtable, dataExtensionKey);
        }

        private async Task<SalesForceAuthenticationModel> GetAuthenticationDetailsAsync(WeChatifySFAccountModel accountModel)
        {
           return await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication",
                "RowKey", accountModel.OrganizationId, "eq");
        }

        public async Task CreateDataExtensionAsync(Type obj, long accountId, string folderName, string deName)
        {

            var accountModel = await _weChatifyRepository.GetSFMappedAccountAsync(accountId);
            var authenticationtable = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", accountModel.OrganizationId, "eq");
            var folderId = DataExtensionRepository.GetDataExtenstionRootFolderId(accountModel.OrganizationId.ToInt(),
                authenticationtable.LegacyToken, authenticationtable.SoapEndPointUrl, accountModel.IsSharedDEConfigured, folderName);
            DataExtensionRepository.CreateDataExtension(obj, folderId, authenticationtable.SoapEndPointUrl, authenticationtable.LegacyToken);
        }

    }
}
