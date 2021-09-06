using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageService.CosmosRepository.Interface;
using MessageService.InfraStructure.APIUrls;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.ExportModels;
using MessageService.Models.SMSModels;
using MessageService.Models.StoredProcedureModels;
using MessageService.Models.SubmailModel;
using MessageService.Models.ViewModel;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;



namespace MessageService.Service.Implementation
{
    public class SMSService : ISMSService
    {
        private readonly ISMSCosmosRepository _smsRepository;
        private readonly IEmailService _emailService;
        private readonly IAzureStorageRepository _azureStorageRepository;
        private readonly IWeChatifyRepository _wechatifyRepository;
        private readonly ISettingsCosmosRepository _settingsCosmosRepository;
        private readonly int QueueMaxRetries = Convert.ToInt32(AppSettings.GetValue("QueueMaxRetries"));

        public SMSService(ISMSCosmosRepository smsRepository, IAzureStorageRepository azureStorageRepository, ISettingsCosmosRepository settingsCosmosRepository, IWeChatifyRepository wechatifyRepository, IEmailService emailService)
        {
            _smsRepository = smsRepository;
            _emailService = emailService;
            _azureStorageRepository = azureStorageRepository;
            _wechatifyRepository = wechatifyRepository;
            _settingsCosmosRepository = settingsCosmosRepository;
        }
        public async Task<bool> PublishSMSEventsAsync(string journeyData)
        {
            var smsRequestData = journeyData.ConvertToModel<SFSendMessageRequestData>();

            smsRequestData.CorrelationId = Guid.NewGuid(); // set correlationId on the payload

            var inArgumentsData = smsRequestData.InArguments.FirstOrDefault();

            var vendorSettings = inArgumentsData.SMSVendorSettings.IsNotNull() ? inArgumentsData.SMSVendorSettings : await _settingsCosmosRepository.GetSMSVendorSettingsAsync(inArgumentsData?.AccountId ?? 0).ConfigureAwait(false);

            List<EventGridEvent> gridEvent = new List<EventGridEvent>();
            gridEvent.Add(new EventGridEvent()
            {
                Id = smsRequestData.ActivityInstanceId,
                Data = smsRequestData,
                EventTime = DateTime.UtcNow,
                EventType = vendorSettings?.Category ?? "Default Category",
                Subject = "SMS Send Event",
                DataVersion = "1.0"
            });

            await EventGridHelper.PublishSMSEventsAsync(gridEvent);

            return true;
        }

        public SubmailResponseModel EmulateSubmailSMSXSend(string model)
        {
            SubmailResponseModel response = new SubmailResponseModel()
            {
                Status = "error",
                ErrorCode = "unsubscribed",
                ErrorMessage = "unsubscribed"

            };
            return response;
        }

        public async Task<bool> SendJourneySMSAsync(string journeyData)
        {
            SMSSendStatusTrack tracker = null;

            try
            {

                var smsRequestData = journeyData.ConvertToModel<SFSendMessageRequestData>();

                if (smsRequestData.IsNull()) { return false; }

                var inArgumentsData = smsRequestData.InArguments.FirstOrDefault();

                tracker = new SMSSendStatusTrack(smsRequestData.CorrelationId.ToString(), (inArgumentsData?.AccountId).ToString())
                {
                    Step = (int)SMSSendSteps.SMS_Sending,
                    Payload = journeyData
                };

                var vendorSettings = inArgumentsData.SMSVendorSettings.IsNotNull() ? inArgumentsData.SMSVendorSettings : await _settingsCosmosRepository.GetSMSVendorSettingsAsync(inArgumentsData?.AccountId ?? 0).ConfigureAwait(false);

                string content = ReplacePersonalizationContent(inArgumentsData);

                var obj = new SMSXSendModel
                {
                    Project = vendorSettings.TemplateId,
                    AppKey = vendorSettings.AppKey,
                    AppId = vendorSettings.AppId,
                    To = inArgumentsData?.MobileNumber,
                    Content = "【" + vendorSettings.SignatureText + "】" + content + " " + vendorSettings.UnSubscribeText
                };

                tracker.SubmailPayload = obj.ToJsonString();

                SubmailResponseModel submailResponse;
                bool res = false;
                if (HasPersonalizationValues(inArgumentsData))
                {
                    submailResponse = await HttpHelper<SubmailResponseModel>
                        .HttpPostAsync(SubmailAPIUrls.SMSSendUrl, obj.ToJsonString()).ConfigureAwait(false);
                    tracker.SubmailResponse = submailResponse.ToJsonString();
                    tracker.Step = (int)SMSSendSteps.SMS_Sent;
                    res = await LogSMSToCosmosAsync(tracker, smsRequestData, inArgumentsData, obj.Content, submailResponse);
                }
                else
                {
                    await _azureStorageRepository.AddQueueAsync(journeyData, QueueName.smspersonalisationmissingqueue.ToString());
                }
                return res;
            }
            catch (Exception ex)
            {

                if (tracker != null)
                {
                    tracker.ErrorMessage = ex.Message;
                    tracker.StackTrace = ex.StackTrace;
                    await _azureStorageRepository.InsertIntoTableAsync(tracker, nameof(SMSSendStatusTrack));
                }
                await _azureStorageRepository.AddDeferredQueueAsync(journeyData, QueueName.smsfailedqueue.ToString(), TimeSpan.FromMinutes(30)).ConfigureAwait(false);
                return false;
                /*throw; 
                 * Not throwing the exception here as the above queue will process the failed request. 
                 * Only request that are not reachable will be pushed to dead letter blob.                 
                 */
            }
        }

        public async Task<bool> ProcessSMSJourneyWithoutPersonalisation(string journeyData, ILogger log)
        {
            SMSSendStatusTrack tracker = null;
            SubmailResponseModel submailResponse = null;
            SMSLogForPersonalisation smsLog = null;
            try
            {
                log.LogInformation($"SMS Missing Personalisation data for " + journeyData);
                var smsRequestData = journeyData.ConvertToModel<SFSendMessageRequestData>();

                var inArgumentsData = smsRequestData.InArguments.FirstOrDefault();
                smsLog = new SMSLogForPersonalisation(smsRequestData.InteractionId, (inArgumentsData?.AccountId).ToString())
                {
                    Payload = journeyData,
                    Status = false
                };
                tracker = new SMSSendStatusTrack(smsRequestData.CorrelationId.ToString(), (inArgumentsData?.AccountId).ToString())
                {
                    Step = (int)SMSSendSteps.SMS_PersonalisationAPIRetry,
                    Payload = journeyData
                };

                var apiPersonalisedData = await GetPersonalisedContent(inArgumentsData);
                smsLog.APIResponse = apiPersonalisedData;
                var personalisedData = apiPersonalisedData.ConvertToModel<SFSearchResponse>();

                string content = ReplacePersonalizationContent(inArgumentsData?.SMSContent, personalisedData);

                var vendorSettings = inArgumentsData.SMSVendorSettings.IsNotNull() ? inArgumentsData.SMSVendorSettings : await _settingsCosmosRepository.GetSMSVendorSettingsAsync(inArgumentsData?.AccountId ?? 0).ConfigureAwait(false);
                var obj = new SMSXSendModel
                {
                    Project = vendorSettings.TemplateId,
                    AppKey = vendorSettings.AppKey,
                    AppId = vendorSettings.AppId,
                    To = inArgumentsData?.MobileNumber,
                    Content = string.Format("【{0}】{1} {2}", vendorSettings.SignatureText, content, vendorSettings.UnSubscribeText)
                };
                if (HasPersonalization(personalisedData))
                {
                    tracker.SubmailPayload = obj.ToJsonString();
                    submailResponse = await HttpHelper<SubmailResponseModel>
                            .HttpPostAsync(SubmailAPIUrls.SMSSendUrl, obj.ToJsonString()).ConfigureAwait(false);

                    smsLog.Status = true;
                }
                else
                {
                    submailResponse = new SubmailResponseModel()
                    {
                        Status = "error",
                        ErrorCode = "variablemissing",
                        ErrorMessage = " Personalization content is missing"
                    };
                    obj.Content = string.Format("【{0}】{1} {2}", vendorSettings.SignatureText, ReplaceMissingPersonalization(inArgumentsData?.SMSContent, personalisedData), vendorSettings.UnSubscribeText);
                }

                tracker.SubmailResponse = submailResponse.ToJsonString();
                tracker.Step = (int)SMSSendSteps.SMS_Sent;

                bool res = await LogSMSToCosmosAsync(tracker, smsRequestData, inArgumentsData, obj.Content, submailResponse);
                return true;
            }
            catch (Exception ex)
            {
                StringBuilder exception = new StringBuilder();
                exception.Append("SMS Missing Personalisation exception for input : ")
                    .Append(journeyData)
                    .Append(" Exception : ")
                    .Append(ex);
                log.LogError(exception.ToString());
                if (tracker != null)
                {
                    tracker.ErrorMessage = ex.Message;
                    tracker.StackTrace = ex.StackTrace;
                    await _azureStorageRepository.InsertIntoTableAsync(tracker, nameof(SMSSendStatusTrack));
                }
                await _azureStorageRepository.AddDeferredQueueAsync(journeyData, QueueName.smsfailedqueue.ToString(), TimeSpan.FromMinutes(30)).ConfigureAwait(false);
                return false;
            }
            finally
            {
                await _azureStorageRepository.InsertIntoTableAsync(smsLog, nameof(SMSLogForPersonalisation));
            }
        }


        private async Task<string> GetPersonalisedContent(InArgument inArguments)
        {
            SalesForceSearch sfSearch = new SalesForceSearch();
            foreach (var item in inArguments.SMSPersonalizationVariables)
            {
                sfSearch.Request.Attributes.Add(new SFAttribute { Key = item.Key });
            }
            sfSearch.ConditionSet.Operator = "And";
            sfSearch.ConditionSet.conditions.Add(new SFCondition { Attribute = new SFAttribute { Key = $"{inArguments.EntrySource}.{inArguments.EntrySourceContactKey}" }, Operator = "Equals", Value = new SFValue { Items = new List<string>() { inArguments.ContactKey } } });
            var sfMappedAccount = await _wechatifyRepository.GetSFMappedAccountAsync(inArguments.AccountId);
            if (sfMappedAccount != null)
            {
                var organizationId = sfMappedAccount.OrganizationId;
                var enterpriseId = sfMappedAccount.ParentOrgId;

                var authenticationInfo = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", organizationId, "eq");
                if (authenticationInfo.IsNull())
                {
                    authenticationInfo = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", enterpriseId, "eq");
                }
                var sfDictionary = new Dictionary<string, string>();
                sfDictionary["Authorization"] = $"Bearer {authenticationInfo.AccessToken}";
                var personalisationData = await HttpHelper<string>
                       .HttpPostStringAsync($"{authenticationInfo.RestDomainUrl}contacts/v1/attributes/search", sfSearch.ToJsonString(), headers: sfDictionary).ConfigureAwait(false);
                return personalisationData;
            }
            return null;
        }


        private async Task<bool> LogSMSToCosmosAsync(SMSSendStatusTrack tracker, SFSendMessageRequestData smsRequestData, InArgument inArgumentsData, string smsContent, SubmailResponseModel submailResponse)
        {
            string interactionId = smsRequestData.InteractionId;
            var journeyId = await _smsRepository.GetJourneyIdAsync(interactionId);

            long accountId = inArgumentsData?.AccountId ?? 0;

            if (journeyId.IsNullOrWhiteSpace())
            {
                SMSSFInteractionModel queueData = new SMSSFInteractionModel()
                {
                    AccountId = accountId,
                    Id = interactionId
                };
                var interactionsCache = await _settingsCosmosRepository.GetSMSSFInteractionsAsync(accountId);
                if (interactionsCache.All(x => x.Id != interactionId))
                {
                    await _azureStorageRepository.AddQueueAsync(queueData.ToJsonString(), QueueName.smsupdatejourneyfromsfqueue.ToString());
                    interactionsCache.Add(queueData);
                    await _settingsCosmosRepository.AddOrUpdateSMSSFInteractionsAsync(interactionsCache, accountId);
                }
            }

            var smsLog = new SMSLogDocumentModel()
            {
                AccountId = accountId,
                ActivityId = smsRequestData.ActivityId,
                InteractionId = smsRequestData.InteractionId,
                Id = submailResponse.SendId ?? Guid.NewGuid().ToString("N"),
                SentStatus = submailResponse.SentStatus,
                DeliveryStatus = (submailResponse.SentStatus == SendStatus.Success) ? DeliveryStatus.Pending : (DeliveryStatus?)null,
                DropErrorCode = submailResponse.ErrorCode,
                ErrorMessage = submailResponse.ErrorMessage,
                MobileNumber = inArgumentsData?.MobileNumber,
                SMSContent = smsContent,
                Credit = submailResponse.Fee.ToInt(),
                PersonalizationData = inArgumentsData?.SMSPersonalizationVariables.ToJsonString(),
                IsUpdatedToDE = false,
                JourneyId = journeyId,
                PartitionKey = journeyId,
                ContactKey = inArgumentsData.ContactKey
            };

            tracker.CosmosLogPayload = smsLog.ToJsonString();
            tracker.Step = (int)SMSSendSteps.SMS_Logging;

            var res = await _smsRepository.InsertSMSLogAsync(smsLog);

            await _azureStorageRepository.AddQueueAsync(smsLog.ToJsonString(), QueueName.smslogdeupdatequeue.ToString());

            tracker.Step = (int)SMSSendSteps.SMS_Logged;
            return res;
        }

        private static string ReplaceMissingPersonalization(InArgument inArgumentsData)
        {
            string content = inArgumentsData?.SMSContent;
            if (inArgumentsData?.SMSPersonalizationVariables == null) return content;

            foreach (var item in inArgumentsData?.SMSPersonalizationVariables)
            {
                content = content?.Replace("%%" + item.Key + "%%",
                    item.Value.IsNotNullOrWhiteSpace() ? item.Value : item.Key);
            }

            return content;
        }

        private static string ReplaceMissingPersonalization(string smsContent, SFSearchResponse personalisedData)
        {
            string content = smsContent;
            if (personalisedData != null && personalisedData.Items != null)
            {
                foreach (var item in personalisedData.Items.FirstOrDefault()?.Values)
                {
                    content = content.Replace("%%" + item.Key + "%%", item.Value.IsNotNullOrWhiteSpace() ? item.Value : item.Key);
                }
            }

            return content;
        }

        private string ReplacePersonalizationContent(string smsContent, SFSearchResponse personalisedData)
        {
            string content = smsContent;
            if (personalisedData != null && personalisedData.Items != null)
            {
                foreach (var item in personalisedData.Items.FirstOrDefault()?.Values)
                {
                    content = content.Replace("%%" + item.Key + "%%", item.Value);
                }
            }

            return content;
        }

        private string ReplacePersonalizationContent(InArgument inArgumentsData)
        {
            string content = inArgumentsData?.SMSContent;
            if (inArgumentsData?.SMSPersonalizationVariables == null) return content;
            foreach (var item in inArgumentsData?.SMSPersonalizationVariables)
            {
                content = content.Replace("%%" + item.Key + "%%", item.Value);
            }

            return content;
        }

        private static bool HasPersonalizationValues(InArgument inArgumentsData)
        {

            if (!inArgumentsData.SMSPersonalizationVariables.Any()) // if template not contains variable then no need to check 
            {
                return true;
            }
            return inArgumentsData.SMSPersonalizationVariables.Count == 1 || inArgumentsData.SMSPersonalizationVariables.Any(k => k.Value.IsNotNullOrWhiteSpace());
        }
        private static bool HasPersonalization(SFSearchResponse inArgumentsData)
        {
            if (inArgumentsData.ErrorCode.IsNull()
                     && inArgumentsData.Items.Count > 0)
                return inArgumentsData.Items.FirstOrDefault().Values.Count == 1 || inArgumentsData.Items.FirstOrDefault().Values.Any(k => k.Value.IsNotNullOrWhiteSpace());
            else
                return false;

        }

        public async Task<SMSBalanceModel> GetSMSBalanceAsync(long accountId)
        {
            var settings = await _settingsCosmosRepository.GetSMSVendorSettingsAsync(accountId);
            if (settings.IsNull())
            {
                return new SMSBalanceModel { ErrorCode = "-99", ErrorMessage = "invalid vendor settings", Status = "error" };
            }
            var postBody = new { appid = settings.AppId, signature = settings.AppKey };
            return await HttpHelper<SMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.SMSBalanceUrl, postBody.ToJsonString());
        }

        public async Task AddQueueForDeadLetterProcessingAsync(string json)
        {
            var model = JsonConvert.DeserializeObject<List<SMSDeadLetterModel>>(json);
            await _azureStorageRepository.AddDeferredQueueAsync(model.FirstOrDefault().Data.ToJsonString(), QueueName.smsfailedqueue.ToString(), TimeSpan.FromMinutes(30));
        }

        public async Task AddQueueForIncomingMessagesExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.smsincomingmessagesexportqueue.ToString());
        }
        public async Task AddQueueForVerificationSMSExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.smsverificationexportqueue.ToString());
        }

        public async Task AddQueueForOptOutMessagesExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.smsoptoutmessagesexportqueue.ToString());
        }
        public async Task AddQueueForJourneysExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.smsjourneyexportqueue.ToString());
        }
        public async Task AddQueueForSMSLogExportAsync(LogFilterModel logFilter)
        {
            await _azureStorageRepository.AddQueueAsync(logFilter.ToJsonString(), QueueName.smslogexportqueue.ToString());
        }
        public async Task<JourneyInfoViewModel> GetJourneysAsync(JourneyFilterModel journeyFilter)
        {

            var (log, pieChart, count) = await _smsRepository.GetJourneysAsync(journeyFilter);
            var smsJourneyInfoModels = log;

            var result = new JourneyInfoViewModel()
            {
                JourneysInfo = new PagingModel<JourneyInfoDocumentModel>()
                {
                    Items = smsJourneyInfoModels,
                    ItemsPerPage = journeyFilter.ItemsPerPage,
                    PageNumber = journeyFilter.PageNo,
                    TotalCount = count
                },
                PieChartInfo = new JourneyPieChart()
                {
                    JourneyInfoModels = pieChart
                }
            };


            return result;
        }
        public async Task<PagingModel<IncomingMessageDocumentModel>> GetIncomingMessagesAsync(GetIncomingMessagesModel model)
        {

            var (incomingMessageModel, totalCount) = await _smsRepository.GetIncomingMessagesAsync(model);

            return new PagingModel<IncomingMessageDocumentModel>()
            {
                ItemsPerPage = model.ItemsPerPage,
                PageNumber = model.PageNo,
                TotalCount = totalCount,
                Items = incomingMessageModel
            };
        }

        public async Task<PagingModel<VerificationSMSDocumentModel>> GetVerificationSMSAsync(GetVerificationSMSModel model)
        {

            var (verificationMessageModel, totalCount) = await _smsRepository.GetVerificationSMSAsync(model);

            return new PagingModel<VerificationSMSDocumentModel>()
            {
                ItemsPerPage = model.ItemsPerPage,
                PageNumber = model.PageNo,
                TotalCount = totalCount,
                Items = verificationMessageModel
            };
        }

        public async Task<bool> ExportVerificationSMSAsync(string queueData, string directoryPath, string templatePath)
        {
            var emailExportModel = queueData.ConvertToModel<VerificationSMSExportModel>();
            emailExportModel.ToDate = emailExportModel.ToDate.Date.AddDays(1);
            var data = await _smsRepository.GetVerificationSMSAsync(emailExportModel).ConfigureAwait(false);
            DataSet ds = GetVerificationSMSExcelDataSet(data.ToList());

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(emailExportModel.AccountId);

            string fileNamePrefix = "VerificationSMS";
            string subject = "Verification SMS Export";

            await SendEmail(templatePath, emailExportModel, accDetails, ds, fileNamePrefix, subject);

            return true;
        }

        private DataSet GetVerificationSMSExcelDataSet(List<VerificationSMSDocumentModel> data)
        {
            var ds = new DataSet();
            var sheet = data.Select(_ => new
            {
                _.FollowerName,
                _.MobileNumber,
                _.SMSContent,
                _.City,
                _.Province,
                _.Country,
                SendStatus = _.SendStatus?.ToString(),
                SendDate = _.SendDateString,
                DeliveryStatus = _.DeliveryStatus?.ToString(),
                DeliveryDate = _.DeliveryDateString,
            }).ToList();

            sheet = sheet.OrderBy(x => x.SendDate).ToList();
            var dataTable = sheet.ToDataTable();
            dataTable.TableName = "Verification SMS Report";

            ds.Tables.Add(dataTable);

            return ds;
        }

        public async Task<bool> ExportIncomingMessagesAsync(bool isOptOut, string queueData, string templatePath,
            ILogger log)
        {
            var emailExportModel = queueData.ConvertToModel<IncomingMessagesExportModel>();
            emailExportModel.ToDate = emailExportModel.ToDate.Date.AddDays(1);
            var data = await _smsRepository.GetIncomingMessagesAsync(emailExportModel, isOptOut).ConfigureAwait(false);
            var dt = GetIncomingMessagesExcelDataTable(data.ToList(), isOptOut);

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(emailExportModel.AccountId);

            string fileNamePrefix = isOptOut ? "OptOutMessages" : "IncomingMessages";
            string subject = isOptOut ? "SMS OptOut Messages Export" : "SMS Incoming Messages Export";

            var dateInterval = GetDateInterval(emailExportModel);

            var fileName = $"{accDetails?.AccountName }_{fileNamePrefix}_{dateInterval}.xlsx";

            var document = await GetExcelBlobLinkAsync(emailExportModel, dt, fileName, log);

            var link = $@"<a href='{document}'>Click here </a> <br>";

            await _emailService.SendExportEmailAsync(emailExportModel.EmailIds, link, accDetails?.AccountName, templatePath, dateInterval, subject, subject).ConfigureAwait(false);

            return true;
        }

        private async Task<string> SendEmail(string templatePath,
            EmailExportModel emailExportModel, WeChatifyAccountModel accDetails, DataSet ds, string fileNamePrefix,
            string subject)
        {
            string dateInterval = GetDateInterval(emailExportModel);

            var fileName = $"{accDetails?.AccountName }_{fileNamePrefix}_{dateInterval}.xlsx";

            string link;
            using (var stream = new MemoryStream())
            {
                ds.ConvertAsStream(stream);
                var uri = await _azureStorageRepository.UploadStreamToBlobAsync(stream, emailExportModel.AccountId,
                    fileName, "SMS/ExcelExport");
                link = $@"<a href='{uri}'>Click here</a>";
            }

            await _emailService.SendExportEmailAsync(emailExportModel.EmailIds, link, accDetails?.AccountName,
                templatePath, dateInterval, subject, subject).ConfigureAwait(false);
            return fileName;
        }


        private async Task<string> GetExcelBlobLinkAsync(EmailExportModel emailExportModel, DataTable dt, string fileName, ILogger log)
        {

            string link;
            using (var stream = new MemoryStream())
            {
                dt.ConvertAsStream(stream);
                log.LogInformation("blob upload started");
                link = await _azureStorageRepository.UploadStreamToBlobAsync(stream, emailExportModel.AccountId, fileName, "SMS/ExcelExport");
                log.LogInformation("blob upload Completed");
            }

            return link;
        }

        private void CreateExcelDocument(DataSet ds, string filePath)
        {

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            CreateExcel.CreateExcelDocument(ds, filePath);
        }

        private DataTable GetIncomingMessagesExcelDataTable(List<IncomingMessageDocumentModel> incomingMessages, bool isOptout)
        {
            DataTable dataTable;
            if (isOptout)
            {
                var sheet = incomingMessages.Select(_ => new
                {
                    MobileNumber = _.MobileNumber,
                    JourneyName = _.JourneyName,
                    ReceivedDate = _.CreatedOn.ToDateTimeString()
                }).ToList();

                sheet = sheet.OrderBy(x => x.ReceivedDate).ToList();
                dataTable = sheet.ToDataTable();
            }
            else
            {
                var sheet = incomingMessages.Select(_ => new
                {
                    MobileNumber = _.MobileNumber,
                    Content = _.Content,
                    ReceivedDate = _.CreatedOn.ToDateTimeString()
                }).ToList();

                sheet = sheet.OrderBy(x => x.ReceivedDate).ToList();
                dataTable = sheet.ToDataTable();
            }

            dataTable.TableName = isOptout ? "Opt Out Messages Report" : "Incoming Messages Report";

            return dataTable;
        }
        //Insert into SMSJourneyInfo, SMSInteractionInfo and SMSActivityInfo
        public async Task<bool> AddJourneyEntryAsync(JourneyActivateModel journeyModel)
        {
            await _azureStorageRepository.AddQueueAsync(journeyModel.ToJsonString(), "smsjourneyactivateinfo");

            var authenticationtable = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", journeyModel.OrganizationId, "eq");
            if (authenticationtable.IsNull())
            {
                authenticationtable = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", journeyModel.EnterPriseId, "eq");
            }

            var url = authenticationtable.RestDomainUrl + "interaction/v1/interactions/" + journeyModel.JourneyId;

            var headers = new Dictionary<string, string>() {{"Authorization","Bearer "+authenticationtable.AccessToken }
            };
            var result = await HttpHelper<SFJourneyDetailsModel>.HttpGetAsync(url, null, headers);

            var currentDate = DateTime.UtcNow.ToChinaTime();

            var activity = result.Activities.Where(_ => _.arguments.execute.IsNotNull()).FirstOrDefault(k => k.id == journeyModel.ActivityId);

            var journeyProcModel = new JourneyProcModel()
            {
                AccountId = Convert.ToInt64(journeyModel.AccountId),
                ActivityId = activity?.id,
                ActivityName = activity?.arguments?.execute?.inArguments?.FirstOrDefault()?.actionName,
                CreatedOn = currentDate,
                InteractionId = journeyModel.InteractionId,
                JourneyId = journeyModel.JourneyId,
                JourneyName = result.JourneyName,
                JourneyKey = result.Key,
                Version = journeyModel.InteractionVersion
            };

            var data = await _smsRepository.AddOrUpdateJourneyDetailsAsync(journeyProcModel);

            return data;
        }

        public async Task<bool> ExportJourneysAsync(string queueData, string directoryPath, string templatePath, ILogger log)
        {
            var subject = string.Empty;
            string fileName = string.Empty;
            var link = string.Empty;

            var emailExportModel = queueData.ConvertToModel<JourneyExportModel>();
            emailExportModel.FromDate = emailExportModel.FromDate.Date;
            emailExportModel.ToDate = emailExportModel.ToDate.AddDays(1).Date;

            var dateInterval = GetDateInterval(emailExportModel);

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(emailExportModel.AccountId);
            switch (emailExportModel.EmailExportType)
            {
                case SMSEmailType.JourneyBased:
                    {
                        string fileNamePrefix = "Journeys";
                        subject = "SMS Journey Export";

                        fileName = $"{accDetails?.AccountName }_{fileNamePrefix}_{dateInterval}.xlsx";
                        var data = await GetJourneysToExportAsync(emailExportModel);
                        var dt = GetJourneysExcelDataTable(data.ToList());
                        var document = await GetExcelBlobLinkAsync(emailExportModel, dt, fileName, log);
                        link += $@"<a href='{document}'>Click here </a> <br>";
                    }
                    break;
                case SMSEmailType.SMSBased:
                    {
                        var journeyIds = await _smsRepository.GetDistinctJourneyForExportAsync(emailExportModel);
                        var logs = new List<SMSLogExportModel>();
                        var page = 1;
                        subject = "SMS Log Export";
                        string fileNamePrefix = "Logs";
                        foreach (var journey in journeyIds.Where(k=>k.IsNotNullOrEmpty()))
                        {
                            var data = await GetSMSLogForExportAsync(emailExportModel, journey, log);
                            logs.AddRange(data);
                            if (logs.Count > 90000)
                            {
                                fileName = $"{accDetails?.AccountName }_{fileNamePrefix}_{dateInterval}_{page}.xlsx";
                                var dt = GetSmsLogExcelDataTable(logs.ToList());
                                var document = await GetExcelBlobLinkAsync(emailExportModel, dt, fileName, log);
                                link += $@"<a href='{document}'>Click here - Document_{page} </a> <br>";
                                page++;
                                logs = new List<SMSLogExportModel>();
                            }

                        }

                        if (logs.Any())
                        {
                            fileName = $"{accDetails?.AccountName }_{fileNamePrefix}_{dateInterval}_{page}.xlsx";
                            var finalData = GetSmsLogExcelDataTable(logs.ToList());
                            var totalDocument = await GetExcelBlobLinkAsync(emailExportModel, finalData, fileName, log);
                            link += $@"<a href='{totalDocument}'>Click here - Document_{page} </a> <br>";
                        }


                    }
                    break;
            }

            await _emailService.SendExportEmailAsync(emailExportModel.EmailIds, link, accDetails?.AccountName, templatePath, dateInterval, subject, subject).ConfigureAwait(false);
            log.LogInformation("Email Sent");
            return true;
        }

        private static string GetDateInterval(EmailExportModel emailExportModel)
        {
            var dateInterval = $"({emailExportModel.FromDate:yyyy-MM-dd} to {emailExportModel.ToDate.AddDays(-1):yyyy-MM-dd})";
            if (emailExportModel.FromDate.ToString("yyyy-MM-dd")
                .Equals(emailExportModel.ToDate.AddDays(-1).ToString("yyyy-MM-dd")))
            {
                dateInterval = $"({emailExportModel.FromDate:yyyy-MM-dd})";
            }

            return dateInterval;
        }

        private async Task<List<SMSLogExportModel>> GetSMSLogForExportAsync(JourneyExportModel emailExportModel, string journeyId, ILogger log)
        {
            log.LogInformation("Cosmos Data Fetch started");
            List<SMSLogDocumentModel> result = await _smsRepository.GetSMSLogForExportAsync(emailExportModel, journeyId).ConfigureAwait(false);
            log.LogInformation("Cosmos Data Fetch Completed");
            log.LogInformation($"Data Count :{result.Count}");
            var journeyIds = result.Select(x => x.JourneyId).Distinct().ToArray();
            var interactionIds = result.Select(x => x.InteractionId).Distinct().ToArray();
            var activityIds = result.Select(x => x.ActivityId).Distinct().ToArray();
            var activityList = await _smsRepository.GetActivityListAsync(emailExportModel.AccountId, activityIds);
            var journeyList = await _smsRepository.GetJourneysListAsync(emailExportModel.AccountId, journeyIds);
            var versionList = await _smsRepository.GetVersionsListByIdsAsync(emailExportModel.AccountId, interactionIds);

            var exportData = result.Select(k =>
                new SMSLogExportModel
                {
                    DeliveryDate = k.DeliveryDate?.ToDateTimeString() ?? "",
                    SentStatus = k.SentStatus.ToString(),
                    DeliveryStatus = k.DeliveryStatus.ToString(),
                    SendDate = k.SendDate.ToDateTimeString(),
                    SMSContent = k.SMSContent,
                    MobileNumber = k.MobileNumber,
                    JourneyName = journeyList.FirstOrDefault(y => y.Id == k.JourneyId)?.JourneyName,
                    ActivityName = activityList.FirstOrDefault(c => c.Id == k.ActivityId)?.ActivityName,
                    Version = versionList.FirstOrDefault(m => m.Id == k.InteractionId)?.Version
                }).ToList();

            return exportData;
        }
        private async Task<List<JourneyInfoDocumentModel>> GetJourneysToExportAsync(JourneyExportModel emailExportModel)
        {
            return await _smsRepository.GetJourneysAsync(emailExportModel).ConfigureAwait(false);
        }

        private DataTable GetJourneysExcelDataTable(List<JourneyInfoDocumentModel> data)
        {
            var sheet = data.Select(_ => new
            {
                JourneyName = _.JourneyName,
                InitiatedDate = _.InitiatedDate.IsNull() ? string.Empty : _.InitiatedDate.Value.ToDateTimeString(),
                TotalCount = _.TotalCount,
                SuccessCount = _.DeliveredCount,
                DroppedCount = _.DroppedCount,
                SendFailedCount = _.SendFailedCount

            }).ToList();

            var dataTable = sheet.ToDataTable();
            dataTable.TableName = "SMS Journey Report";


            return dataTable;
        }
        private DataTable GetSmsLogExcelDataTable(List<SMSLogExportModel> data)
        {

            var sheet = data.Select(a => new
            {
                JourneyName = a.JourneyName,
                Version = "Version " + a.Version,
                ActivityName = a.ActivityName,
                MobileNumber = a.MobileNumber,
                SMSContent = a.SMSContent,
                LoggedDate = a.SendDate,
                SentStatus = a.SentStatus,
                DeliveryDate = a.DeliveryDate,
                DeliveryStatus = a.DeliveryStatus
            }).ToList();

            var dataTable = sheet.ToDataTable();
            dataTable.TableName = "SMS Log Report";


            return dataTable;
        }

        private async Task AddAzureToTable(string accountId, SMSFailedQueueType type, string data, string errorMsg = "", string stackTrace = "")
        {
            var model = new SMSFailedQueueLog(type.ToString())
            {
                AccountId = accountId,
                Type = type,
                Data = data,
                ErrorMessage = errorMsg,
                StackTrace = stackTrace
            };
            await _azureStorageRepository.InsertIntoTableAsync(model, nameof(SMSFailedQueueLog));
        }

        public async Task<bool> UpdateSMSLogAsync(string queueDataString)
        {
            try
            {
                var queueData = queueDataString.ConvertToModel<SubmailStatusPushModel>();
                var isUpdated = await UpdateStatusFromSubmailAsync(queueData);
                if (!isUpdated)
                {
                    await _azureStorageRepository.AddQueueAsync(queueDataString, QueueName.smslogupdatefailedqueue.ToString()).ConfigureAwait(false);
                }

                return isUpdated;
            }
            catch (Exception ex)
            {
                await _azureStorageRepository.AddDeferredQueueAsync(queueDataString, QueueName.smslogupdatefailedqueue.ToString(), TimeSpan.FromMinutes(30)).ConfigureAwait(false);

                await AddAzureToTable("smsLogUpdate()", SMSFailedQueueType.UpdateSMSLog,
                    queueDataString, ex.Message, ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateStatusFromSubmailAsync(SubmailStatusPushModel statusPushModel)
        {

            switch (statusPushModel.EventType)
            {
                case SubmailEventType.delivered:
                case SubmailEventType.dropped:
                case SubmailEventType.unknown:
                    return await SMSDeliveryReportUpdateAsync(statusPushModel).ConfigureAwait(false);
                case SubmailEventType.mo:
                    var vendorSetting = await _settingsCosmosRepository.GetSMSVendorSettingsAsync(statusPushModel.AppId);
                    var journeyName = await _smsRepository.GetJourneyNameByMobileNumberAsync(statusPushModel.MobileNumber, vendorSetting.AccountId);
                    var incomingMsg = new IncomingMessageDocumentModel()
                    {
                        Id = statusPushModel.Token,
                        AccountId = vendorSetting.AccountId,
                        Content = statusPushModel.Content,
                        CreatedOn = statusPushModel.EventDateTime,
                        MobileNumber = statusPushModel.MobileNumber,
                        JourneyName = journeyName
                    };

                    await _azureStorageRepository
                        .AddQueueAsync(incomingMsg.ToJsonString(), QueueName.smsincomingdeupatequeue.ToString())
                        .ConfigureAwait(false);
                    return await _smsRepository.SaveIncomingMessageAsync(incomingMsg).ConfigureAwait(false);
                default:
                    break;
            }

            return false;
        }

        private async Task<bool> SMSDeliveryReportUpdateAsync(SubmailStatusPushModel statusPushModel)
        {
            var smsLog = new SMSLogDocumentModel()
            {
                Id = statusPushModel.SendId,
                DeliveryDate = statusPushModel.EventDateTime,
                DropErrorCode = statusPushModel.DropCode,
                ErrorMessage = statusPushModel.DropCode,
                DeliveryStatus = statusPushModel.DeliveryStatus
            };

            var updatedLog = await _smsRepository.UpdateDeliveryReportInSMSLogAsync(smsLog).ConfigureAwait(false);

            if (updatedLog.IsNotNull())
            {
               await _azureStorageRepository.AddQueueAsync(updatedLog.ToJsonString(),
                    QueueName.smslogdeupdatequeue.ToString()).ConfigureAwait(false);
                return true;
            }

            var isUpdated = await _wechatifyRepository.UpdateSMSCampaignNumbersAsync(smsLog).ConfigureAwait(false);
            if (!isUpdated)
            {
                isUpdated = await _smsRepository.UpdateVerificationSMSAsync(smsLog).ConfigureAwait(false);
            }

            return isUpdated;
        }

        public async Task<LogGridModel> GetJourneyDetailsAsync(long accountId, string journeyId)
        {
            var logModelsTask = _smsRepository.GetSMSLogAsync(accountId, journeyId);
            var versionDdlTask = _smsRepository.GetVersionsAsync(accountId, journeyId);
            var activityDdlTask = _smsRepository.GetActivitiesAsync(accountId, journeyId);


            await Task.WhenAll(versionDdlTask, activityDdlTask, logModelsTask);

            var activityDdl = activityDdlTask.Result.Select(y =>
                new ActivityDropDownModel()
                {
                    InteractionId = y.InteractionId,
                    ActivityName = y.ActivityName,
                    ActivityId = y.Id
                }).ToList();


            var gridModel = new LogGridModel()
            {
                ActivityDdl = activityDdl,
                VersionDdl = versionDdlTask.Result,
                LogViewModels = new PagingModel<LogViewModel>()
                {
                    Items = logModelsTask.Result.Item1,
                    TotalCount = logModelsTask.Result.Item2,
                    ItemsPerPage = 10,
                    PageNumber = 1
                }
            };

            return gridModel;
        }

        public async Task<JourneyInfoDocumentModel> GetSMSLogCountInJourneyAsync(string journeyId)
        {

            var logCountTask = _smsRepository.GetSMSLogCountAsync(journeyId);
            var deliveryCountTask =
                _smsRepository.GetSMSLogCountByDeliveryStatusAsync(journeyId, DeliveryStatus.Delivered);

            var droppedCountTask =
                _smsRepository.GetSMSLogCountByDeliveryStatusAsync(journeyId, DeliveryStatus.Dropped);

            var sendFailedCountTask = _smsRepository.GetSMSLogCountBySendStatusAsync(journeyId, SendStatus.Failed);

            await Task.WhenAll(logCountTask, deliveryCountTask, droppedCountTask, sendFailedCountTask);

            var data = new JourneyInfoDocumentModel()
            {
                Id = journeyId,
                TotalCount = logCountTask.Result,
                DeliveredCount = deliveryCountTask.Result,
                DroppedCount = droppedCountTask.Result,
                SendFailedCount = sendFailedCountTask.Result
            };

            return data;
        }

        public async Task<JourneyInfoDocumentModel> UpdateSMSLogCountInJourneyAsync(JourneyInfoDocumentModel journeyInfo)
        {

            var logCountTask = _smsRepository.GetSMSLogCountAsync(journeyInfo.Id, journeyInfo.AccountId);
            var deliveryCountTask =
                _smsRepository.GetSMSLogCountByDeliveryStatusAsync(journeyInfo.Id, DeliveryStatus.Delivered, journeyInfo.AccountId);

            var droppedCountTask =
                _smsRepository.GetSMSLogCountByDeliveryStatusAsync(journeyInfo.Id, DeliveryStatus.Dropped, journeyInfo.AccountId);

            var sendFailedCountTask = _smsRepository.GetSMSLogCountBySendStatusAsync(journeyInfo.Id, SendStatus.Failed, journeyInfo.AccountId);

            await Task.WhenAll(logCountTask, deliveryCountTask, droppedCountTask, sendFailedCountTask);


            journeyInfo.TotalCount = logCountTask.Result;
            journeyInfo.DeliveredCount = deliveryCountTask.Result;
            journeyInfo.DroppedCount = droppedCountTask.Result;
            journeyInfo.SendFailedCount = sendFailedCountTask.Result;

            await _smsRepository.AddOrUpdateJourneyInfoAsync(journeyInfo);

            return journeyInfo;
        }

        public async Task<List<JourneyInfoDocumentModel>> GetLastDayRanJourneysAsync()
        {
            var journeyIds = await _smsRepository.GetDistinctJourneyIdsAsync(DateTime.UtcNow.ToChinaTime().AddDays(-1));
            var lastDayJourneys = await _smsRepository.GetJourneysAsync(journeyIds);
            return lastDayJourneys;
        }

        public async Task<IEnumerable<JourneyInfoDocumentModel>> GetAllJourneyAsync(long accountId)
        {
            return await _smsRepository.GetJourneysAsync(accountId);
        }

        public async Task<bool> ExportSMSLogAsync(string queueData, string directoryPath, string templatePath)
        {
            var smsLogExportModel = queueData.ConvertToModel<LogFilterModel>();
            var data = await _smsRepository.GetSMSLogAsync(smsLogExportModel).ConfigureAwait(false);
            var errorCodes = await _settingsCosmosRepository.GetErrorCodeDetailsAsync();
            var journeys = await _smsRepository.GetJourneysAsync(smsLogExportModel.AccountId);
            data.ForEach(x =>
            {
                x.JourneyName = journeys.FirstOrDefault(j => j.Id == x.JourneyId)?.JourneyName;
                x.ErrorMessage = errorCodes.FirstOrDefault(e => e.ErrorCode == x.DropErrorCode)?.EnglishDescription ?? x.ErrorMessage;
            });
            DataSet ds = GetSMSLogExcelDataSet(data.ToList());

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(smsLogExportModel.AccountId);

            string fileNamePrefix = "SMSLog";
            string subject = "SMS Log Export";

            var emailExportModel = new EmailExportModel()
            {
                AccountId = smsLogExportModel.AccountId,
                EmailIds = smsLogExportModel.EmailIds,
                FromDate = smsLogExportModel.SendDateFrom ?? DateTime.UtcNow.ToChinaTime().Date,
                ToDate = smsLogExportModel.SendDateTo ?? DateTime.UtcNow.ToChinaTime().Date.AddDays(1)
            };

             await SendEmail(templatePath, emailExportModel, accDetails, ds, fileNamePrefix, subject);

            return true;
        }
        private DataSet GetSMSLogExcelDataSet(List<LogViewModel> logViewModel)
        {
            var ds = new DataSet();

            var sheet = logViewModel.Select(_ => new SMSLogExportModel
            {
                ActivityName = _.ActivityName,
                DroppedReason = _.ErrorMessage,
                JourneyName = _.JourneyName,
                MobileNumber = _.MobileNumber,
                Version = _.VersionName,
                DeliveryDate = _.DeliveryDateString,
                DeliveryStatus = _.DeliveryStatus?.ToString(),
                SMSContent = _.SMSContent,
                SendDate = _.SendDateString,
                SentStatus = _.SentStatus.ToString()
            }).ToList();

            sheet = sheet.OrderBy(x => x.MobileNumber).ToList();
            var dataTable = sheet.ToDataTable();

            dataTable.TableName = "SMS Log Report";

            ds.Tables.Add(dataTable);

            return ds;
        }

        public async Task<LogGridModel> GetSMSLogDetailsAsync(LogFilterModel logFilter)
        {


            var (smslog, totalCount) = await _smsRepository.GetSMSLogWithPagingAsync(logFilter);

            var interactionInfos = await _smsRepository.GetVersionsListByIdsAsync(logFilter.AccountId,
                smslog.Select(y => y.InteractionId).ToArray());

            var activityInfos =
                await _smsRepository.GetActivityListAsync(logFilter.AccountId,
                    smslog.Select(y => y.ActivityId).ToArray());

            var versionDdl = interactionInfos.Select(k => new VersionDropDownModel()
            {
                InteractionId = k.Id,
                Version = k.Version
            });

            var activityDdl = activityInfos.Select(y =>
                new ActivityDropDownModel()
                {
                    InteractionId = y.InteractionId,
                    ActivityName = y.ActivityName,
                    ActivityId = y.Id
                }).ToList();


            var gridModel = new LogGridModel()
            {
                ActivityDdl = activityDdl,
                VersionDdl = versionDdl,
                LogViewModels = new PagingModel<LogViewModel>()
                {
                    Items = smslog,
                    TotalCount = totalCount,
                    ItemsPerPage = logFilter.ItemsPerPage,
                    PageNumber = logFilter.PageNo
                }
            };


            return gridModel;
        }

        public async Task ReprocessQueueAsync(string queueName)
        {
            var queueNameEnum = queueName.ToLower().ToEnum<QueueName>();

            switch (queueNameEnum)
            {
                case QueueName.smslogupdatefailedqueue:
                    while (true)
                    {
                        var addFailedQueues = await _azureStorageRepository.GetQueuesAsync(queueName);
                        if (!addFailedQueues.Any())
                        {
                            return;
                        }
                        foreach (var failedQueue in addFailedQueues)
                        {
                            await UpdateSMSLogAsync(failedQueue).ConfigureAwait(false);
                        }
                    }
            }


        }

        public async Task<bool> SendDeliveryReportAsync(string baseDirectoryPath, string templatePath)
        {
            var notificationSettings = await _settingsCosmosRepository.GetAllSMSDeliveryReportNotificationAsync();
            var errorCodes = await _settingsCosmosRepository.GetErrorCodeDetailsAsync();
            foreach (var setting in notificationSettings)
            {
                switch (setting.RunBy)
                {
                    case RunBy.Daily:
                        {
                            var day = 1;
                            if (DateTime.UtcNow.ToChinaTime().Hour == setting.RunOn)
                            {
                                var filterDate = DateTime.UtcNow.ToChinaTime().AddDays(-day);
                                await SendNotificationMailAsync(setting, filterDate, baseDirectoryPath, errorCodes, templatePath).ConfigureAwait(false);
                            }
                        }
                        break;
                    case RunBy.Weekly:
                        {
                            var day = 7;
                            DayOfWeek weekDay = (DayOfWeek)setting.DayOn;
                            DayOfWeek today = DateTime.Today.DayOfWeek;
                            if (today == weekDay && DateTime.UtcNow.ToChinaTime().Hour == setting.RunOn)
                            {
                                var filterDate = DateTime.UtcNow.ToChinaTime().AddDays(-day);
                                await SendNotificationMailAsync(setting, filterDate, baseDirectoryPath, errorCodes, templatePath).ConfigureAwait(false);
                            }

                        }
                        break;
                    case RunBy.Monthly:
                        {

                            //month end configuration
                            if (setting.DayOn >= 29)
                            {
                                setting.DayOn = Convert.ToUInt32(DateTime.UtcNow.ToChinaTime().GetLastDayOfMonth().ToString("dd"));
                            }

                            var date = DateTime.UtcNow.ToChinaTime().ToString("dd");
                            if (setting.DayOn == Convert.ToUInt32(date) && DateTime.UtcNow.ToChinaTime().Hour == setting.RunOn)
                            {
                                var filterDate = DateTime.UtcNow.ToChinaTime().AddMonths(-1);
                                await SendNotificationMailAsync(setting, filterDate, baseDirectoryPath, errorCodes, templatePath).ConfigureAwait(false);
                            }
                        }
                        break;


                    default:
                        return false;
                }

            }

            return true;
        }

        public async Task<bool> SendSMSInventoryAndThresholdNotificationAsync(string templatePath)
        {
            var notificationSettings = await _settingsCosmosRepository.GetAllInventoryAndAlertSettingAsync();
            foreach (var setting in notificationSettings)
            {
                var currentSmsBalance = await GetSMSBalanceAsync(setting.AccountId);

                if (currentSmsBalance.Balance.ToInt() < setting.AlertThreshold)
                {
                    var accountDetails = await _wechatifyRepository.GetAccountDetailsAsync(setting.AccountId);

                    setting.NotificationUsers.ForEach(async k => await _emailService.SMSThresholdNotificationEmailAsync(k.EmailId, templatePath, accountDetails.AccountName, k.UserName, setting.AlertThreshold));
                }

            }

            return true;
        }

        private async Task SendNotificationMailAsync(DeliveryReportNotificationDocumentModel setting,
            DateTime filterDate, string baseDirectoryPath, List<SMSErrorCodeDetailsDocumentModel> errorCodes,
            string templatePath)
        {
            var activityIds = await _smsRepository.GetDistinctActivitiesAsync(setting.AccountId, filterDate);

            var reportData = await _smsRepository.GetActivitiesAsync(setting.AccountId, activityIds);

            var accountDetails = await _wechatifyRepository.GetAccountDetailsAsync(setting.AccountId);
            var reports = await GetReportModelsAsync(filterDate, reportData, accountDetails);

            reports = reports.Where(
                k => Convert.ToDecimal(k.DeliveredPercentage.Replace(" %", string.Empty)) <= Convert.ToDecimal(setting.DeliveryPercentage)).ToList();
            if (reports.Any())
            {
                reports.Add(new DeliveryReportExportModel()
                {
                    CampaignsRunDay = "Total  ",
                    TotalSMSSent = reports.Sum(k => k.TotalSMSSent),
                    Delivered = reports.Sum(k => k.Delivered),
                    Dropped = reports.Sum(k => k.Dropped),
                    SendFailed = reports.Sum(k => k.SendFailed),
                    Pending = reports.Sum(k => k.Pending),
                    UnConfirmed = reports.Sum(k => k.UnConfirmed)
                });
            }
            else
            {
                reports.Add(new DeliveryReportExportModel()
                {
                    ActionName = "No Records"
                });
            }


            var accountNameAndCount = GetReportsToAppendEmailTable(reports);

            reports.Add(new DeliveryReportExportModel());

            var droppedReason = await GetDroppedReasonAsync(errorCodes, accountDetails.AccountId, filterDate);
            reports.AddRange(droppedReason);
            var sendFailedReason = await GetSendFailedReasonAsync(errorCodes, accountDetails.AccountId, filterDate);
            reports.AddRange(sendFailedReason);

            var ds = new DataSet();
            var dataTable = reports.ToDataTable();
            dataTable.TableName = accountDetails.AccountName;
            ds.Tables.Add(dataTable);

            var fileName = $@"SMS_DeliveryReport.xlsx";

            var excelFileName = baseDirectoryPath + @"\" + fileName;

            CreateExcelDocument(ds, excelFileName);
            var toEmailIds = setting.NotificationUsers.Select(c => c.EmailId).ToArray();

            var toDate = DateTime.UtcNow.ToChinaTime();
            string subject = "SMS Delivery Alert Report";
            await _emailService.SendDeliveryReportEmailAsync(toEmailIds, templatePath, baseDirectoryPath, excelFileName,
                 accountNameAndCount, filterDate, toDate, subject);
        }

        public async Task<bool> SendDeliveryReportToShiseidoHKAsync(string[] accountIds, string[] toEmailIds, string baseDirectoryPath, string templatePath)
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            if (today == DayOfWeek.Friday && DateTime.UtcNow.ToChinaTime().Hour == 10)
            {
                var filterDate = DateTime.UtcNow.ToChinaTime().AddDays(-8);
                var errorCodes = await _settingsCosmosRepository.GetErrorCodeDetailsAsync();

                await SendNotificationMailAsync(accountIds, filterDate, baseDirectoryPath, errorCodes, templatePath,
                    toEmailIds).ConfigureAwait(false);
            }

            return true;
        }

        public async Task<bool> SendDeliveryReportToShiseidoHKInternalAsync(string[] accountIds, string[] toEmailIds, string baseDirectoryPath, string templatePath)
        {          
            if (DateTime.UtcNow.ToChinaTime().Hour == 23)
            {
                var filterDate = DateTime.UtcNow.ToChinaTime().AddDays(-5);
                var errorCodes = await _settingsCosmosRepository.GetErrorCodeDetailsAsync();
                await SendNotificationMailAsync(accountIds, filterDate, baseDirectoryPath, errorCodes, templatePath,
                    toEmailIds).ConfigureAwait(false);
            }

            return true;
        }
        private async Task SendNotificationMailAsync(string[] accountIds,
           DateTime filterDate, string baseDirectoryPath, List<SMSErrorCodeDetailsDocumentModel> errorCodes,
           string templatePath, string[] toEmailIds)
        {

            var ds = new DataSet();
            var allAccountsReports = new List<DeliveryReportExportModel>();

            foreach (var accountId in accountIds)
            {
                var accountDetails = await _wechatifyRepository.GetAccountDetailsAsync(accountId.ToLong());

                var reports = await GetDeliveryReportExportModelsAsync(filterDate, accountDetails);
                if (reports.IsNull())
                {
                    continue;
                }
                allAccountsReports.AddRange(reports);

                await AddDropAndSendFileReasonAsync(filterDate, errorCodes, accountDetails, reports, ds);
            }

            var accountNameAndCount = GetReportsToAppendEmailTable(allAccountsReports);

            var fileName = $@"SMS Summarised Delivery Alert Report_{DateTime.UtcNow.ToChinaTime().ToString("yyyy-MM-dd")}.xlsx";
            string subject = "SMS Summarised Delivery Alert Report";

            var excelFileName = baseDirectoryPath + @"\" + fileName;

            CreateExcelDocument(ds, excelFileName);

            var toDate = DateTime.UtcNow.ToChinaTime();

            await _emailService.SendDeliveryReportEmailAsync(toEmailIds, templatePath, baseDirectoryPath, excelFileName,
                 accountNameAndCount, filterDate, toDate, subject);
        }

        private async Task<List<DeliveryReportExportModel>> GetDeliveryReportExportModelsAsync(DateTime filterDate,
            WeChatifyAccountModel accountDetails)
        {
            var activityIds = await _smsRepository.GetDistinctActivitiesAsync(accountDetails.AccountId, filterDate);
            var reportData = await _smsRepository.GetActivitiesAsync(accountDetails.AccountId, activityIds);

            var reports = await GetReportModelsAsync(filterDate, reportData, accountDetails);
            if (!reports.Any()) { return null; }

            reports.Add(new DeliveryReportExportModel()
            {
                CampaignsRunDay = "Total  ",
                TotalSMSSent = reports.Sum(k => k.TotalSMSSent),
                Delivered = reports.Sum(k => k.Delivered),
                Dropped = reports.Sum(k => k.Dropped),
                SendFailed = reports.Sum(k => k.SendFailed),
                Pending = reports.Sum(k => k.Pending),
                UnConfirmed = reports.Sum(k => k.UnConfirmed)
            });
            return reports;
        }

        private async Task AddDropAndSendFileReasonAsync(DateTime filterDate, List<SMSErrorCodeDetailsDocumentModel> errorCodes,
            WeChatifyAccountModel accountDetails, List<DeliveryReportExportModel> reports, DataSet ds)
        {
            reports.Add(new DeliveryReportExportModel());
            var droppedReason = await GetDroppedReasonAsync(errorCodes, accountDetails.AccountId, filterDate);
            reports.AddRange(droppedReason);
            var sendFailedReason = await GetSendFailedReasonAsync(errorCodes, accountDetails.AccountId, filterDate);
            reports.AddRange(sendFailedReason);

            var dataTable = reports.ToDataTable();
            dataTable.TableName = accountDetails.AccountName;
            ds.Tables.Add(dataTable);

        }

        private static StringBuilder GetReportsToAppendEmailTable(List<DeliveryReportExportModel> reports)
        {
            var accName = new StringBuilder();
            if (reports.Any())
            {
                for (var i = 0; i < reports.Count; i++)
                {

                    if (i % 2 == 0)
                    {
                        accName.Append("<tr style='text-align:left;'>"
                                   + "<td style='padding:8px;'>" + reports[i].AccountName + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].ActionName + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].CampaignsRunDay + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].TotalSMSSent + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].Delivered + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].Dropped + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].Pending + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].UnConfirmed + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].SendFailed + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].DeliveredPercentage + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].DroppedPercentage + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].OthersPercentage + "</td> "
                                   + "</tr>");

                    }
                    else
                    {
                        accName.Append("<tr style='text-align:left;background-color: #f2f2f2;'>"
                                + "<td style='padding:8px;'>" + reports[i].AccountName + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].ActionName + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].CampaignsRunDay + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].TotalSMSSent + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].Delivered + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].Dropped + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].Pending + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].UnConfirmed + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].SendFailed + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].DeliveredPercentage + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].DroppedPercentage + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].OthersPercentage + "</td> "
                                + "</tr>");
                    }
                }
            }
            else
            {
                accName.Append("<tr><td colspan = '13'> No Records </td></tr>");
            }

            return accName;
        }

        private async Task<List<DeliveryReportExportModel>> GetReportModelsAsync(DateTime filterDate, List<ActivityInfoDocumentModel> reportData, WeChatifyAccountModel accountDetails)
        {
            var excelModels = reportData.GroupBy(y => y.JourneyId).ToDictionary(m => m.Key, c => c.ToList());
            var result = new List<DeliveryReportExportModel>();
            foreach (var k in excelModels)
            {
                var journeyDetails = _smsRepository.GetJourneyAsync(k.Key).Result;
                if (k.Value.IsNull() || !k.Value.Any())
                {
                    result.Add(new DeliveryReportExportModel()
                    {
                        AccountName = accountDetails.AccountName,
                        ActionName = journeyDetails.JourneyName,
                        CampaignsRunDay = filterDate.ToDateTimeString(),
                        DeliveredPercentage = "0.00 %",
                        DroppedPercentage = "0.00 %",
                        OthersPercentage = "0.00 %"
                    });
                    continue;
                }

                var logdata = await _smsRepository.GetFirstSMSLogAsync(accountDetails.AccountId, k.Key, filterDate);

                var pendingCount = await _smsRepository.GetSMSLogCountByDeliveryStatusAsync(accountDetails.AccountId, k.Key,
                    filterDate, DeliveryStatus.Pending).ConfigureAwait(false);

                var unconfirmedCount = await _smsRepository.GetSMSLogCountByDeliveryStatusAsync(accountDetails.AccountId,
                    k.Key, filterDate, DeliveryStatus.UnConfirmed).ConfigureAwait(false);

                var deliveredCount = await _smsRepository.GetSMSLogCountByDeliveryStatusAsync(accountDetails.AccountId,
                    k.Key, filterDate, DeliveryStatus.Delivered).ConfigureAwait(false);

                var droppedCount = await _smsRepository.GetSMSLogCountByDeliveryStatusAsync(accountDetails.AccountId,
                    k.Key, filterDate, DeliveryStatus.Dropped).ConfigureAwait(false);

                var sendFailedCount = await _smsRepository.GetSMSLogCountBySendStatusAsync(accountDetails.AccountId,
                    k.Key, filterDate, SendStatus.Failed).ConfigureAwait(false);

                var totalCount = await _smsRepository.GetSMSLogCountAsync(accountDetails.AccountId,
                    k.Key, filterDate).ConfigureAwait(false);

                var divTotalCount = totalCount == 0 ? 1 : totalCount;

                var deliveryPercentage = (float)(deliveredCount * 100 / divTotalCount);

                var droppedPercentage = (float)(droppedCount * 100 / divTotalCount);
                var othersPercentage =
                    (float)((sendFailedCount + pendingCount + unconfirmedCount) * 100 / divTotalCount);

                result.Add(new DeliveryReportExportModel()
                {
                    AccountName = accountDetails.AccountName,
                    ActionName = journeyDetails.JourneyName,
                    TotalSMSSent = totalCount,
                    UnConfirmed = unconfirmedCount,
                    CampaignsRunDay = logdata?.SendDate.ToDateTimeString(),
                    Delivered = deliveredCount,
                    Dropped = droppedCount,
                    SendFailed = sendFailedCount,
                    Pending = pendingCount,
                    DeliveredPercentage = deliveryPercentage.ToString(CultureInfo.InvariantCulture) + " %",
                    DroppedPercentage = droppedPercentage.ToString(CultureInfo.InvariantCulture) + " %",
                    OthersPercentage = othersPercentage.ToString(CultureInfo.InvariantCulture) + " %"
                });
                // return result;
            }

            return result;
        }

        private async Task<List<DeliveryReportExportModel>> GetDroppedReasonAsync(List<SMSErrorCodeDetailsDocumentModel> errorCodes, long accountId, DateTime dateFrom)
        {

            var droppedSmsLog = await _smsRepository.GetSMSLogByDeliveryStatusAsync(accountId, dateFrom, DeliveryStatus.Dropped);
            var unConfirmedLog = await _smsRepository.GetSMSLogByDeliveryStatusAsync(accountId, dateFrom, DeliveryStatus.UnConfirmed);

            droppedSmsLog.AddRange(unConfirmedLog);

            if (!droppedSmsLog.Any())
            {
                return new List<DeliveryReportExportModel>();
            }
            var excelReport = new List<DeliveryReportExportModel>
            {
                new DeliveryReportExportModel(),
                new DeliveryReportExportModel() {AccountName = "DROPPED NUMBERS WITH REASON"},
                new DeliveryReportExportModel(),
                new DeliveryReportExportModel()
                {
                    AccountName = "Mobile Number",
                    ActionName = "ActionName and Date",
                    CampaignsRunDay = "Dropped Reason"
                }
            };

            var journeyDic = droppedSmsLog.Where(y => y.JourneyId.IsNotNull()).GroupBy(k => k.JourneyId).ToDictionary(y => y.Key, m => m.ToList());

            foreach (var journeySmsLog in journeyDic)
            {
                var journeyDetails = await _smsRepository.GetJourneyAsync(journeySmsLog.Key);

                if (journeySmsLog.Value.IsNull()) { continue; }

                foreach (var log in journeySmsLog.Value)
                {
                    excelReport.Add(new DeliveryReportExportModel()
                    {
                        AccountName = log.MobileNumber,
                        ActionName = journeyDetails.JourneyName + " - " + log.SendDate.ToJsonString(),
                        CampaignsRunDay = errorCodes.FirstOrDefault(y => y.ErrorCode.Equals(log.DropErrorCode, StringComparison.InvariantCultureIgnoreCase)
                                                                         || y.ErrorCode.StartsWith(log.DropErrorCode?.Substring(0, 3) ?? "", StringComparison.InvariantCultureIgnoreCase)
                                                                         || y.ErrorCode.StartsWith(log.DropErrorCode?.Substring(0, 2) ?? "", StringComparison.InvariantCultureIgnoreCase))?.EnglishDescription
                    });
                }

            }

            return excelReport;
        }

        private async Task<List<DeliveryReportExportModel>> GetSendFailedReasonAsync(List<SMSErrorCodeDetailsDocumentModel> errorCodes, long accountId, DateTime dateFrom)
        {
            var sendFailed = await _smsRepository.GetSMSLogBySendStatusAsync(accountId, dateFrom, SendStatus.Failed);
            if (!sendFailed.Any())
            {
                return new List<DeliveryReportExportModel>();
            }
            var excelReport = new List<DeliveryReportExportModel>
            {
                new DeliveryReportExportModel(),
                new DeliveryReportExportModel() {AccountName = "SEND FAILED NUMBERS WITH REASON"},
                new DeliveryReportExportModel(),
                new DeliveryReportExportModel()
                {
                    AccountName = "Mobile Number",
                    ActionName = "ActionName and Date",
                    CampaignsRunDay = "Send Failed Reason"
                }
            };


            var journeyDic = sendFailed.GroupBy(k => k.JourneyId).ToDictionary(y => y.Key, m => m.ToList());

            foreach (var journeySmsLog in journeyDic)
            {
                var journeyDetails = await _smsRepository.GetJourneyAsync(journeySmsLog.Key);

                if (journeySmsLog.Value.IsNull()) { continue; }

                foreach (var log in journeySmsLog.Value)
                {
                    excelReport.Add(new DeliveryReportExportModel()
                    {
                        AccountName = log.MobileNumber,
                        ActionName = journeyDetails.JourneyName + " - " + log.SendDate.ToDateTimeString(),
                        CampaignsRunDay = errorCodes.FirstOrDefault(y => y.ErrorCode.Equals(log.DropErrorCode, StringComparison.InvariantCultureIgnoreCase) || y.ErrorCode.Equals(log.DropErrorCode?.Substring(0, 3).ToLower() + "xxxx", StringComparison.InvariantCultureIgnoreCase))?.EnglishDescription
                    });
                }

            }

            return excelReport;
        }

        public async Task<bool> ProcessSMSDeadLetterAsync(string queueData)
        {
            var model = JsonConvert.DeserializeObject<List<SMSDeadLetterModel>>(queueData);
            if (model.IsNotNull() && model.Count > 0)
            {
                return await SendJourneySMSAsync(model.FirstOrDefault()?.Data.ToJsonString());
            }
            return false;
        }
        public async Task<bool> ProcessSMSFailedQueueAsync(string queueData, ILogger log)
        {
            bool result = false;
            var smsRequestData = queueData.ConvertToModel<SFSendMessageRequestData>();
            var correlationId = smsRequestData.CorrelationId;
            var smsSendStatusTrack = await _azureStorageRepository.GetSMSSendStatusAsync(nameof(SMSSendStatusTrack), "RowKey", correlationId.ToString());

            if (smsSendStatusTrack.IsNull())
            {
                return await SendJourneySMSAsync(queueData);
            }

            if (smsSendStatusTrack.RetryCount >= QueueMaxRetries)
            {
                await _azureStorageRepository.AddQueueAsync(queueData, QueueName.smsretrylapsedqueue.ToString()).ConfigureAwait(false);
                return false;
            }

            switch (smsSendStatusTrack.Step)
            {
                case (int)SMSSendSteps.SMS_Sending:
                    return await SendJourneySMSAsync(queueData);
                case (int)SMSSendSteps.SMS_PersonalisationAPIRetry:
                    return await ProcessSMSJourneyWithoutPersonalisation(queueData, log);

                case (int)SMSSendSteps.SMS_Sent:
                    result = await LogSMSToCosmosAsync(smsSendStatusTrack, smsRequestData,
                        smsRequestData.InArguments.FirstOrDefault(), smsSendStatusTrack.Content,
                        smsSendStatusTrack.SubmailResponse.ConvertToModel<SubmailResponseModel>());
                    break;

                case (int)SMSSendSteps.SMS_Logging:
                    result = await _smsRepository.InsertSMSLogAsync(smsSendStatusTrack.CosmosLogPayload.ConvertToModel<SMSLogDocumentModel>());
                    smsSendStatusTrack.Step = (int)SMSSendSteps.SMS_Logged;
                    break;

                case (int)SMSSendSteps.SMS_Logged:
                    break;
                default:
                    break;
            }
            smsSendStatusTrack.RetryCount = smsSendStatusTrack.RetryCount + 1;
            await _azureStorageRepository.UpsertIntoTableAsync(smsSendStatusTrack, nameof(SMSSendStatusTrack));

            return result;
        }

        public async Task CustomQueryExecutorAsync(long accountId)
        {
            await _smsRepository.CustomQueryExecutorAsync(accountId);
        }

        public async Task<bool> SendVerificationSMSAsync(VerificationSMSModel model)
        {
            // var piiDetails = await _azureStorageRepository.GetPIIDetailsAsync(model.OpenId, nameof(PIIDetails), nameof(PIIDetails.OpenID), model.OpenId);
            var userDetails = await _wechatifyRepository.GetFollowerDetailsAsync(model.OpenId);

            var vendorSettings = await _settingsCosmosRepository.GetSMSVendorSettingsAsync(model?.AccountId ?? 0).ConfigureAwait(false);
            string content = $"Your Wechatify Verification Code: {model.VerificationCode}";
            var obj = new SMSXSendModel
            {
                Project = vendorSettings.VerificationTemplateId,
                AppKey = vendorSettings.AppKey,
                AppId = vendorSettings.AppId,
                To = model?.MobileNumber,
                Variables = new Dictionary<string, string> { { "code", model.VerificationCode } },
            };
            var response = await HttpHelper<SubmailResponseModel>
                        .HttpPostAsync(SubmailAPIUrls.SMSXSendUrl, obj.ToJsonString()).ConfigureAwait(false);

            var document = new VerificationSMSDocumentModel()
            {
                AccountId = model.AccountId,
                MobileNumber = model.MobileNumber,
                OpenId = model.OpenId,
                SMSContent = content,
                Province = userDetails.Province,
                City = userDetails.City,
                Country = userDetails.Country,
                SendDate = DateTime.UtcNow.ToChinaTime(),
                FollowerName = userDetails.NickName,
                Id = response?.SendId ?? Guid.NewGuid().ToString(),
                DropErrorCode = response?.ErrorCode,
                SendStatus = response?.SentStatus,
                DeliveryStatus = (response?.SentStatus == SendStatus.Success) ? DeliveryStatus.Pending : (DeliveryStatus?)null,

            };
            return await _smsRepository.SaveVerificationSMSAsync(document);
        }

        public async Task UpdateUnconfirmedStatusAsync(long accountId, DateTime fromDate)
        {
            for (var i = 0; i <= 10; i++)
            {
                var pendingSMS = await _smsRepository.GetPendingStatusSMSAsync(accountId, fromDate).ConfigureAwait(false);
                if (!pendingSMS.Any()) { break; }

                foreach (var item in pendingSMS)
                {
                    item.DeliveryDate = DateTime.UtcNow.ToChinaTime();
                    item.DeliveryStatus = DeliveryStatus.UnConfirmed;
                    item.ErrorMessage = DeliveryStatus.UnConfirmed.ToString();
                    item.DropErrorCode = DeliveryStatus.UnConfirmed.ToString();
                }
                var rr = await _smsRepository.UpdateUnConfirmedStatusAsync(pendingSMS);
            }
        }

        public async Task UpdateSMSMonthlyUsageAsync(DateTime startDate, DateTime endDate)
        {
            var smsVendorSettings = await _settingsCosmosRepository.GetAllSMSVendorSettingsAsync();            
            foreach (var setting in smsVendorSettings)
            {
                await UpdateSMSMonthlyUsageAsync(setting, startDate, endDate);
            }
        }

        public async Task UpdateSMSMonthlyUsageAsync(long accountId, DateTime startDate, DateTime endDate)
        {
            var setting = await _settingsCosmosRepository.GetSMSVendorSettingsAsync(accountId);
            await UpdateSMSMonthlyUsageAsync(setting, startDate, endDate);
            
        }

        private async Task UpdateSMSMonthlyUsageAsync(SMSVendorSettingsDocumentModel setting, DateTime startDate, DateTime endDate)
        {
            RechargeHistoryResponseModel rechargeHistory = await GetRechargeHistoryAsync(setting);

            if (rechargeHistory.Status != "success")
            {
                return;
            }
            var smsUsedCount = await _smsRepository.GetSMSUsedCountAsync(setting.AccountId, startDate, endDate);
            var currentBalance = await GetSMSBalanceAsync(setting.AccountId);

            SMSUsageDocumentModel model = new SMSUsageDocumentModel()
            {
                RechargeCount = rechargeHistory.RechargeHistories
                                          .Where(x => x.RechargeDateTime >= startDate && x.RechargeDateTime <= endDate)
                                          .Select(x => Convert.ToInt32(x.SmsAddCredits)).Sum(),
                AccountId = setting.AccountId,
                Balance = currentBalance.Balance,
                Month = (Months)startDate.Month,
                Year = startDate.Year,
                UsedCount = smsUsedCount
            };
            await _smsRepository.UpsertSMSUsageAsync(model);
        }

        public async Task<SMSUsageModel> GetSMSMonthlyUsageAsync(long accountId, int? year)
        {
            SMSUsageModel model = new SMSUsageModel();
            model.SMSUsage = await _smsRepository.GetSMSMonthlyUsageAsync(accountId, year ?? DateTime.UtcNow.ToChinaTime().Year);
            model.Years = await _smsRepository.GetSMSMonthlyUsageYearsAsync(accountId);
            return model;
        }

        private static async Task<RechargeHistoryResponseModel> GetRechargeHistoryAsync(SMSVendorSettingsDocumentModel setting)
        {
            var data = new BaseSubmailModel()
            {
                AppId = setting.AppId,
                AppKey = setting.AppKey
            };

            var rechargeHistory = await HttpHelper<RechargeHistoryResponseModel>
                .HttpPostAsync(SubmailAPIUrls.SMSRechargeHistoryUrl, data.ToJsonString()).ConfigureAwait(false);
            return rechargeHistory;
        }

        private async Task<List<SFJourneyDetailsModel>> GetAllSFJourneys(string restUrl, string accessToken)
        {
            List<SFJourneyDetailsModel> details = new List<SFJourneyDetailsModel>();
            int page = 1;
            while (true)
            {
                var url = restUrl + $"interaction/v1/interactions?$page={page}";
                var headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + accessToken } };
                var result = await HttpHelper<SFJourneyDetailsRoot>.HttpGetAsync(url, null, headers);
                if (result == null || !result.Items.Any())
                {
                    break;
                }
                details.AddRange(result?.Items);
                page++;
            };

            return details;
        }

        private async Task<List<OriginalDefintition>> GetAllJourneyDetailsFromSF(string restUrl, string accessToken, string key)
        {
            var url = restUrl + $"interaction/v1/interactions/key:{key}/audit/Publish";

            var headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + accessToken } };
            var result = await HttpHelper<OriginalDefintitionRoot>.HttpGetAsync(url, null, headers);

            return result?.Items;
        }

        public async Task<bool> AddOrUpdateJourneyFromSFAsync(string queueData)
        {
            var smsSFInteractionModel = JsonConvert.DeserializeObject<SMSSFInteractionModel>(queueData);
            if (smsSFInteractionModel.IsNull())
            {
                return false;
            }
            var accountId = smsSFInteractionModel.AccountId;
            var weChatifySFAccountDetail = await _wechatifyRepository.GetSFMappedAccountAsync(accountId);
            if (weChatifySFAccountDetail.IsNotNull())
            {
                var OrganizationId = weChatifySFAccountDetail.OrganizationId;
                var EnterPriseId = weChatifySFAccountDetail.ParentOrgId;

                var authenticationInfo = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", OrganizationId, "eq");
                if (authenticationInfo.IsNull())
                {
                    authenticationInfo = await _azureStorageRepository.GetSalesForceAuthenticationAsync("SalesforceAuthendication", "RowKey", EnterPriseId, "eq");
                }

                if (authenticationInfo.IsNull())
                {
                    return false;
                }

                var JourniesList = await GetAllSFJourneys(authenticationInfo.RestDomainUrl, authenticationInfo.AccessToken);

                var journeyKeys = JourniesList.Select(x => x.Key).ToList();

                List<OriginalDefintition> interactionList = new List<OriginalDefintition>();
                foreach (var item in journeyKeys)
                {
                    var interactionsFromSF = await GetAllJourneyDetailsFromSF(authenticationInfo.RestDomainUrl, authenticationInfo.AccessToken, item);
                    interactionList.AddRange(interactionsFromSF);
                }

                var actualJournies = interactionList.Where(x => x.Id == smsSFInteractionModel.Id).ToList();

                var publishedList = actualJournies.Where(x => x.PublishStatus == "PublishCompleted").ToList();

                foreach (var item in publishedList)
                {
                    var finalInteractions = interactionList.Where(x => x.OriginalDefinitionId == item.OriginalDefinitionId).ToList();

                    for (int i = 1; i <= finalInteractions.Count; i++)
                    {

                        var url = authenticationInfo.RestDomainUrl + "interaction/v1/interactions/" + item.OriginalDefinitionId + $"?versionNumber={i}";

                        var headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + authenticationInfo.AccessToken } };
                        var result = await HttpHelper<SFJourneyDetailsModel>.HttpGetAsync(url, null, headers);
                        if (result.Activities == null || !result.Activities.Any())
                        {
                            continue;
                        }

                        if (i == 1)
                        {
                            var journey = await _smsRepository.GetJourneyAsync(result.DefinitionId);
                            if (journey.IsNull())
                            {
                                JourneyInfoDocumentModel journeyDocument = new JourneyInfoDocumentModel()
                                {
                                    AccountId = accountId,
                                    Id = result.DefinitionId,
                                    JourneyKey = result.Key,
                                    JourneyName = result.JourneyName,
                                    LastTriggeredOn = result.LastPublishedDate,
                                    InitiatedDate = result.CreatedDate,
                                    Type = CosmosDocumentType.Journey
                                };
                                await _smsRepository.UpsertDocument(journeyDocument);
                            }
                        }

                        var interact = finalInteractions.FirstOrDefault(y => y.VersionNumber == i);
                        if (interact == null)
                        {
                            continue;
                        }

                        InteractionInfoDocumentModel interaction = new InteractionInfoDocumentModel()
                        {
                            AccountId = accountId,
                            CreatedOn = interact.TimeStamp,
                            JourneyId = interact.OriginalDefinitionId,
                            Version = interact.VersionNumber.ToString(),
                            Id = interact.Id,
                            PublishedDate = interact.TimeStamp,
                            Type = CosmosDocumentType.Interaction
                        };

                        await _smsRepository.UpsertDocument(interaction);

                        List<ActivityInfoDocumentModel> activities = new List<ActivityInfoDocumentModel>();
                        var activitiesSF = result.Activities.Where(_ => _.arguments.execute.IsNotNull()).ToList();

                        foreach (var act in activitiesSF)
                        {
                            var details = act?.arguments?.execute?.inArguments?.FirstOrDefault();
                            ActivityInfoDocumentModel model = new ActivityInfoDocumentModel()
                            {
                                JourneyId = interact.OriginalDefinitionId,
                                InteractionId = interact.Id,
                                Id = act?.id,
                                AccountId = accountId,
                                ActivityName = details?.actionName ?? "Unknown",
                                Type = CosmosDocumentType.Activity,
                                CreatedOn = interact.TimeStamp
                            };
                            activities.Add(model);
                        }
                        if (activities.Count > 0)
                            await _smsRepository.AddOrUpdateActivityInfoAsync(activities);
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public async Task ProcessNullLogsAsync(string queueData)
        {
            var smsSFInteractionModel = JsonConvert.DeserializeObject<SMSSFInteractionModel>(queueData);
            if (smsSFInteractionModel.IsNull())
            {
                return;
            }
            await _smsRepository.ProcessNullLogsAsync(smsSFInteractionModel);

        }

        public async Task<List<RechargeHistoryModel>> GetRechargeHistoryAsync(long accountId, int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1);
            var settings = await _settingsCosmosRepository.GetSMSVendorSettingsAsync(accountId);
            var rechargeHistory = await GetRechargeHistoryAsync(settings);
            if (rechargeHistory.Status != "success")
            {
                return new List<RechargeHistoryModel>();
            }
            var result = rechargeHistory.RechargeHistories.Where(x => x.RechargeDateTime >= startDate && x.RechargeDateTime <= endDate && Convert.ToInt32(x.SmsAddCredits) != 0).ToList();
            return result;
        }
    }
}
