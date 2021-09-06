using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService.CosmosRepository.Interface;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.Enum;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;
using MessageService.Service.Interface;
using Microsoft.Extensions.Logging;

namespace MessageService.Service.Implementation
{
    public class SMSDataExtensionService : ISMSDataExtensionService
    {
        private readonly IWeChatifyRepository _weChatifyRepository;
        private readonly ISMSCosmosRepository _smsRepository;
        private readonly IAzureStorageRepository _azureStorageRepository;
        private static readonly string SMSOptOutDEName = AppSettings.GetValue("SalesForce:WeChatifySmsOptOuts");
        private static readonly string SMSIncomingDEName = AppSettings.GetValue("SalesForce:WeChatifyIncomingSmsText");
        private static readonly string SMSLogDEName = AppSettings.GetValue("SalesForce:WeChatifySmsLogs");
        private static readonly string WechatifyCallsBaseUrl = AppSettings.GetValue("WeChatifyCallsBaseUrl");
        public SMSDataExtensionService(IWeChatifyRepository weChatifyRepository, IAzureStorageRepository azureStorageRepository, ISMSCosmosRepository smsRepository)
        {
            _weChatifyRepository = weChatifyRepository;
            _azureStorageRepository = azureStorageRepository;
            _smsRepository = smsRepository;
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


        public async Task UpdateSMSLogToDataExtensionAsync(ILogger logger)
        {
            var smslogs = await GetSMSLogQueuesAsync().ConfigureAwait(false);

            var dicOfSmsLogs =
                smslogs.GroupBy(m => m.AccountId).ToDictionary(y => y.Key, c => c.ToList());

            foreach (var data in dicOfSmsLogs)
            {
                logger.LogInformation(
                    $"SMS Log Fetching information For the Account :  {data.Key}");

                var (accountModel, authenticationTable, dataExtensionKey) = await GetDataExtensionKeyAsync(data.Key, SMSLogDEName);

                logger.LogInformation(
                    $"Started SMSLog DE Update  {accountModel.AccountName}: {accountModel.AccountId}, Records Count:{data.Value.Count} ");

                    authenticationTable = await GetAuthenticationDetailsAsync(accountModel);
                    if (dataExtensionKey.IsNull())
                    {
                        continue;
                    }

                    var logData = data.Value;

                    if (!logData.Any())
                    {
                        continue;
                    }

                    var accountDetails = await _weChatifyRepository.GetAccountDetailsAsync(data.Key);

                    var deData = logData.Where(x => x.JourneyId.IsNotNull()).Select(k => new DataExtensionSMSLogModel()
                    {
                        WeChatAccountId = accountModel.WeChatId,
                        WeChatifyAccountId = k.AccountId.ToString(),
                        ActionName = k.JourneyId,
                        Id = k.Id,
                        DeliveryDateTime = k.DeliveryDate.ToDateTimeString(),
                        DeliveryStatus = k.DeliveryStatus?.ToString(),
                        Entity = k.MobileNumber,
                        SendStatus = k.SentStatus.ToString(),
                        SendDateTime = k.SendDate.ToDateTimeString(),
                        Type = "Non-PII",
                        WeChatifyAccountName = accountDetails.AccountName,
                        CreatedOn = DateTime.UtcNow.ToChinaTime().ToDateTimeString(),
                        ContactKey = k.ContactKey ?? k.Id,
                        ActivityName = k.ActivityId
                    }).ToList();

                    var journeyDic = deData.GroupBy(k => k.ActionName).ToDictionary(y => y.Key, c => c.ToList());
                    foreach (var keyValuePair in journeyDic)
                    {
                        var journeyDetails = await _smsRepository.GetJourneyAsync(keyValuePair.Key);
                        deData.Where(y => y.ActionName == keyValuePair.Key).ToList().ForEach(k =>
                        {
                            k.ActionName = journeyDetails.JourneyName;
                        });
                    }

                    var activityDic = deData.GroupBy(k => k.ActivityName).ToDictionary(y => y.Key, c => c.ToList());
                    foreach (var keyValuePair in activityDic)
                    {
                        var activityInfo = await _smsRepository.GetActivityAsync(keyValuePair.Key);
                        deData.Where(y => y.ActivityName == keyValuePair.Key).ToList().ForEach(k =>
                        {
                            k.ActivityName = activityInfo.ActivityName;
                        });
                    }

                    var count = deData.Count();
                    for (var i = 0; i < count; i += 500)
                    {
                        var log = deData.Skip(i).Take(500).ToList();
                        var result = await DataExtensionRepository.AddOrUpdateDataExtenstionHasTwoKeysAsync(log,
                            authenticationTable, dataExtensionKey, accountModel.IsSharedDEConfigured);
                    }


                    logger.LogInformation(
                        $"Completed SMSLog DE Update  {accountModel.AccountName} : {accountModel.AccountId}, Records Count:{data.Value.Count} ");
            }
           

        }

        public async Task UpdateIncomingSMSLogToDataExtensionAsync(ILogger logger)
        {
            var listOfIncomingSms = await GetIncomingSMSQueuesAsync();

            var dicOfIncomingSms =
                listOfIncomingSms.GroupBy(m => m.AccountId).ToDictionary(y => y.Key, c => c.ToList());

            foreach (var data in dicOfIncomingSms)
            {
                logger.LogInformation(
                    $"Incoming Log Fetching information For the Account :  {data.Key}");
                var (accountModel, authenticationTable, dataExtensionKey) = await GetDataExtensionKeyAsync(data.Key, SMSIncomingDEName);


                logger.LogInformation(
                    $"Started Incoming Log DE Update  {accountModel.AccountName}: {accountModel.AccountId}, Records Count:{data.Value.Count} ");
                if (dataExtensionKey.IsNull())
                {
                    continue;
                }
                var logData = data.Value;
                var deData = logData.Select(k => new DataExtensionIncomingSMSLog()
                {
                    WeChatifyAccountId = k.AccountId,
                    Text = k.Content ?? "",
                    DateTime = k.CreatedOnString,
                    Entity = k.MobileNumber ?? "",
                    Id = k.Id.ToString(),
                    WeChatifyAccountName = accountModel.AccountName,
                    WeChatAccountId = accountModel.OrgId
                }).OrderBy(y => y.Id).ToList();


                var optOutSms = logData.Where(y => y.IsOptOut).ToList();
                await UpdateOptOutSMSLogToDataExtensionAsync(data.Key, optOutSms);

                var count = deData.Count();
                for (var i = 0; i < count; i += 1000)
                {
                    var log = deData.Skip(i).Take(1000).ToList();
                    var result = await DataExtensionRepository.AddOrUpdateDataExtenstionByKeyAsync(log, authenticationTable, dataExtensionKey, accountModel.IsSharedDEConfigured);
                }
                logger.LogInformation(
                    $"Completed Incoming Log DE Update  {accountModel.AccountName}: {accountModel.AccountId}, Records Count:{data.Value.Count} ");
            }

        }

        private async Task UpdateOptOutSMSLogToDataExtensionAsync(long accountId, List<IncomingMessageDocumentModel> logData)
        {

            var (accountModel, authenticationTable, dataExtensionKey) = await GetDataExtensionKeyAsync(accountId, SMSOptOutDEName);

            var deData = logData.Select(k => new DataExtensionOptOutSMSLog()
            {
                WeChatifyAccountId = k.AccountId,
                DateTime = k.CreatedOnString,
                Entity = k.MobileNumber ?? "",
                WeChatifyAccountName = accountModel.AccountName,
                WeChatAccountId = accountModel.WeChatId,
                ActionName = k.JourneyName,
                Type = "2"
            }).ToList();

            var count = deData.Count();
            for (var i = 0; i < count; i += 1000)
            {
                var log = deData.Skip(i).Take(1000).ToList();
                var result = await DataExtensionRepository.AddOrUpdateDataExtenstionByKeyAsync(log, authenticationTable, dataExtensionKey, accountModel.IsSharedDEConfigured);
            }


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

        private async Task<List<IncomingMessageDocumentModel>> GetIncomingSMSQueuesAsync()
        {
            var incomingSms = await _azureStorageRepository.GetQueuesAsync(QueueName.smsincomingdeupatequeue.ToString())
                .ConfigureAwait(false);

            var listOfIncomingSms = incomingSms.Select(y => y.ConvertToModel<IncomingMessageDocumentModel>()).ToList();
            return listOfIncomingSms;
        }

        private async Task<List<SMSLogDocumentModel>> GetSMSLogQueuesAsync()
        {
            var smslogList = await _azureStorageRepository.GetQueuesAsync(QueueName.smslogdeupdatequeue.ToString())
                .ConfigureAwait(false);

            var listOfSmsLog = smslogList.Select(y => y.ConvertToModel<SMSLogDocumentModel>()).ToList();
            return listOfSmsLog;
        }

    }
}
