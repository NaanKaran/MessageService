using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageService.InfraStructure.APIUrls;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.Enum;
using MessageService.Models.ExportModels;
using MessageService.Models.MMSModels;
using MessageService.Models.StoredProcedureModels;
using MessageService.Models.SubmailModel;
using MessageService.Models.ViewModel;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;
using Attribute = MessageService.Models.DataExtensionModel.Attribute;

namespace MessageService.Service.Implementation
{
    public class MMSService : IMMSService
    {
        private readonly IMMSRepository _mmsRepository;
        private readonly IEmailService _mmsEmailService;
        private readonly IAzureStorageRepository _azureStorageRepository;
        private readonly IWeChatifyRepository _wechatifyRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITemplateRepository _templateRepository;
        public MMSService(IMMSRepository mmsRepository, IAzureStorageRepository azureStorageRepository, ISettingsRepository settingsRepository, IWeChatifyRepository wechatifyRepository, IEmailService mmsEmailService, ITemplateRepository templateRepository)
        {
            _mmsRepository = mmsRepository;
            _mmsEmailService = mmsEmailService;
            _templateRepository = templateRepository;
            _azureStorageRepository = azureStorageRepository;
            _settingsRepository = settingsRepository;
            _wechatifyRepository = wechatifyRepository;
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
            return await _wechatifyRepository.GetSFMappedAccountsAsync(userId, accountIds.ToArray());
        }

        public async Task<bool> SendJourneyMMSAsync(string journeyData)
        {
            try
            {
                var mmsRequestData = journeyData.ConvertToModel<SFSendMessageRequestData>();

                var inArgumentsData = mmsRequestData.InArguments.FirstOrDefault();

                inArgumentsData.VariablesDic = inArgumentsData.Variables.ToDictionary();

                var vendorSettings = inArgumentsData.VendorSettings.IsNotNull() ? inArgumentsData.VendorSettings : await _settingsRepository.GetMMSVendorSettingsAsync(inArgumentsData?.AccountId ?? 0).ConfigureAwait(false);

                // need to convert as model for time being keep it as anonymous 
                var obj = new MmsXSendModel
                {
                    AppKey = vendorSettings.AppKey,
                    AppId = vendorSettings.AppId,
                    To = inArgumentsData?.MobileNumber?.GetLast(11),
                    Project = inArgumentsData?.TemplateId,
                    Variables = inArgumentsData.VariablesDic
                };

                SubmailResponseModel submailResponse;

                if (HasPersonalizationValues(inArgumentsData))
                {
                    submailResponse = await HttpHelper<SubmailResponseModel>.HttpPostAsync(SubmailAPIUrls.MMSXSendUrl, obj.ToJsonString())
                        .ConfigureAwait(false);
                }
                else
                {
                    submailResponse = new SubmailResponseModel()
                    {
                        Status = "error",
                        ErrorCode = "variablemissing",
                        ErrorMessage = " Personalization content is missing"
                    };
                    ReplaceMissingVariable(inArgumentsData);
                }


                //ReplaceMissingVariable(inArgumentsData);
                var mmsLog = new MMSLogModel()
                {
                    AccountId = inArgumentsData?.AccountId ?? 0,
                    ActivityInteractionId = mmsRequestData.ActivityObjectId,
                    ActivityId = mmsRequestData.ActivityId,
                    InteractionId = mmsRequestData.InteractionId,
                    SendId = submailResponse.SendId ?? Guid.NewGuid().ToString("N"),
                    SentStatus = submailResponse.SentStatus,
                    DeliveryStatus = (submailResponse.SentStatus == SendStatus.Success) ? DeliveryStatus.Pending : (DeliveryStatus?)null,
                    DropErrorCode = submailResponse.ErrorCode,
                    ErrorMessage = submailResponse.ErrorMessage,
                    MobileNumber = inArgumentsData?.MobileNumber,
                    MMSTemplateId = inArgumentsData?.TemplateId,
                    OrgId = inArgumentsData?.OrgId,
                    DynamicParamsValue = inArgumentsData.VariablesDic?.ToJsonString()

                };

                await _azureStorageRepository.AddQueueAsync(mmsLog.ToJsonString(), QueueName.mmslogaddqueue.ToString()).ConfigureAwait(false);
                return true;
            }
            catch
            {
                await _azureStorageRepository.AddQueueAsync(journeyData, QueueName.mmsjourneyfailedqueue.ToString()).ConfigureAwait(false);
                throw;
            }
        }

        private static void ReplaceMissingVariable(InArgument inArgumentsData)
        {
            if (!inArgumentsData.VariablesDic.Any(k => k.Value.IsNullOrWhiteSpace()))
            {
                return;
            }

            var newDic = new Dictionary<string, string>();

            foreach (var (key, value) in inArgumentsData.VariablesDic)
            {
                newDic[key] = inArgumentsData.VariablesDic[key];
                if (value.IsNullOrWhiteSpace()) newDic[key] = inArgumentsData.DataExtensionVariablesDic[key];
            }
            inArgumentsData.VariablesDic = newDic;
        }

        private static bool HasPersonalizationValues(InArgument inArgumentsData)
        {
            if (!inArgumentsData.VariablesDic.Any()) // if template not contains variable then no need to check 
            {
                return true;
            }
            return inArgumentsData.VariablesDic.Count == 1 || inArgumentsData.VariablesDic.Any(k => k.Value.IsNotNullOrWhiteSpace());
        }

        private static async Task<string> GetDataExtensionAttributeValueAsync(string subscriptionKey, string dataExtenstionName,
            string dataExtensionProperty, string accessToken, string contactKey, string restApiUrl)
        {

            var request = new DataExtensionRequestModel
            {
                Attributes = new[]
                {
                    new Attribute {Key = dataExtenstionName + "." + dataExtensionProperty}
                }
            };
            var conditionSet = new ConditionSet
            {
                Operator = "And",
                ConditionSets = new object[0],
                Conditions = new[]
                {
                    new Condition
                    {
                        Attribute = new Attribute2
                        {
                            Key = dataExtenstionName + "." + contactKey
                        },
                        Operator = "Equals",
                        Value = new Value
                        {
                            Items = new[] {subscriptionKey}
                        }
                    }
                }
            };

            var headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + accessToken } };
            var payload = new { request, conditionSet };
            var result = await HttpHelper<DataExtenstionResultModel>.HttpPostAsync(restApiUrl + "contacts/v1/attributes/search", payload.ToJsonString(), headers).ConfigureAwait(false);
            var value = result.Items[0].Values[0].Value;
            return value ?? " ";
        }

        public async Task<bool> UpdateTemplateStatusFromSubmailAsync(SubmailStatusPushModel statusPushModel)
        {
            var data = new TemplateUpdateModel()
            {
                TemplateId = statusPushModel.TemplateId,
                Comments = statusPushModel.TemplateRejectReason
            };

            switch (statusPushModel.EventType)
            {
                case SubmailEventType.template_accept:
                    data.Status = TemplateStatus.Approved;
                    await _templateRepository.UpdateTemplateStatusAsync(data);
                    break;
                case SubmailEventType.template_reject:
                    data.Status = TemplateStatus.Rejected;
                    await _templateRepository.UpdateTemplateStatusAsync(data);
                    break;
                default:
                    break;
            }

            return true;
        }

        public async Task CreateMmsTableAsync(DateTime onDateTime)
        {
            var tableName = onDateTime.GetQuadrantMMSLogTableName();
            var query = new QuadrantQueryModel();
            var sql = query.CreateMMSLogTableQuery.Replace(@"@@TABLENAME", tableName);
            await _mmsRepository.CreateQuadrantObjectsAsync(sql);
        }

        public async Task CreateMMSStoredProcedureAsync(DateTime onDateTime)
        {
            var tableName = onDateTime.GetQuadrantMMSLogTableName();
            QuadrantQueryModel query = new QuadrantQueryModel();
            var sql = query.CreateMMSLogInsertProcQuery.Replace(@"@@TABLENAME", tableName);
            await _mmsRepository.CreateQuadrantObjectsAsync(sql);
            sql = query.CreateMMSLogUpdateProcQuery.Replace(@"@@TABLENAME", tableName);
            await _mmsRepository.CreateQuadrantObjectsAsync(sql);
        }

        public async Task<bool> AddEntryInMMSLogTableInfoAsync()
        {
            var nextMonth = DateTime.UtcNow.ToChinaTime().AddDays(30);

            var model = new MMSLogTableInfoModel()
            {
                Description = nextMonth.GetQuadrantMonthInfo(),
                QuadrantNumber = nextMonth.GetQuadrantNumberInfo(),
                TableName = nextMonth.GetQuadrantMMSLogTableName(),
                Year = nextMonth.Year
            };
            var result = await _mmsRepository.AddEntryToMmsLogTableInfoAsync(model);
            return result;
        }

        public async Task SubmailStatusUpdateAddToQueueAsync(SubmailStatusPushModel statusPushModel)
        {

            var deliveryReportModel = new MMSDeliveryReportModel()
            {
                Data = statusPushModel,
                RetryCount = 0
            };
            await _azureStorageRepository.AddQueueAsync(deliveryReportModel.ToJsonString(), QueueName.mmslogupdatequeue.ToString()).ConfigureAwait(false);

        }

        public async Task<PagingModel<IncomingMessageModel>> GetIncomingMessagesAsync(GetIncomingMessagesModel model)
        {

            var (incomingMessageModel, totalCount) = await _mmsRepository.GetIncomingMessagesAsync(model);

            return new PagingModel<IncomingMessageModel>()
            {
                ItemsPerPage = model.ItemsPerPage,
                PageNumber = model.PageNo,
                TotalCount = totalCount,
                Items = incomingMessageModel
            };
        }

        public async Task<bool> UpdateMmsLogAsync(string queueDataString)
        {
            try
            {
                var queueData = queueDataString.ConvertToModel<MMSDeliveryReportModel>();

                var isUpdated = await UpdateStatusFromSubmailAsync(queueData.Data);
                if (!isUpdated)
                {
                    if (queueData.RetryCount <= 10)
                    {
                        queueData.RetryCount += 1;
                        await _azureStorageRepository.AddQueueAsync(queueData.ToJsonString(), QueueName.mmslogupdatequeue.ToString()).ConfigureAwait(false);
                    }
                    else
                    {
                        await _azureStorageRepository.AddQueueAsync(queueDataString, QueueName.mmslogupdatefailedqueue.ToString()).ConfigureAwait(false);
                    }

                }

                return isUpdated;
            }
            catch (Exception ex)
            {
                await _azureStorageRepository.AddQueueAsync(queueDataString, QueueName.mmslogupdatefailedqueue.ToString()).ConfigureAwait(false);

                await AddAzureToTable("mmsLogUpdate()", MMSFailedQueueType.UpdateMMSLog,
                    queueDataString, ex.Message, ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> AddMmsLogAsync(string queueDataString)
        {
            var logModel = queueDataString.ConvertToModel<MMSLogModel>();
            try
            {
                logModel.JourneyId = await _mmsRepository.GetJourneyIdAsync(logModel.InteractionId);
                var isInserted = await _mmsRepository.SaveMmsLogAsync(logModel).ConfigureAwait(false);
                if (isInserted)
                {
                    await _mmsRepository.UpdateCountInJourneyAsync(logModel);
                }
                return isInserted;
            }
            catch (Exception ex)
            {
                await _azureStorageRepository.AddQueueAsync(queueDataString, QueueName.mmslogaddfailedqueue.ToString()).ConfigureAwait(false);
                await AddAzureToTable(logModel.AccountId.ToString(), MMSFailedQueueType.AddMMSLog, queueDataString,
                    ex.Message, ex.StackTrace).ConfigureAwait(false);
                return false;
            }
        }

        private async Task AddAzureToTable(string accountId, MMSFailedQueueType type, string data, string errorMsg = "", string stackTrace = "")
        {
            var model = new MMSFailedQueueLog(type.ToString())
            {
                AccountId = accountId,
                Type = type,
                Data = data,
                ErrorMessage = errorMsg,
                StackTrace = stackTrace
            };
            await _azureStorageRepository.InsertIntoTableAsync(model, nameof(MMSFailedQueueLog));
        }


        public async Task<bool> UpdateStatusFromSubmailAsync(SubmailStatusPushModel statusPushModel)
        {

            switch (statusPushModel.EventType)
            {
                case SubmailEventType.delivered:
                case SubmailEventType.dropped:
                case SubmailEventType.unknown:
                    return await MMSDeliveryReportUpdateAsync(statusPushModel).ConfigureAwait(false);
                case SubmailEventType.mo:
                    var vendorSetting = await _settingsRepository.GetMMSVendorSettingsAsync(statusPushModel.AppId);
                    var journeyName = await _mmsRepository.GetJourneyNameByMobileNumberAsync(statusPushModel.MobileNumber);
                    var incomingMsg = new IncomingMessageModel()
                    {
                        AccountId = vendorSetting.AccountId,
                        Content = statusPushModel.Content,
                        CreatedOn = statusPushModel.EventDateTime,
                        MobileNumber = statusPushModel.MobileNumber,
                        JourneyName = journeyName
                    };
                    return await _mmsRepository.SaveIncomingMessageAsync(incomingMsg).ConfigureAwait(false);
                default:
                    break;
            }

            return false;
        }

        private async Task<bool> MMSDeliveryReportUpdateAsync(SubmailStatusPushModel statusPushModel)
        {
            var mmsLog = new MMSLogModel()
            {
                SendId = statusPushModel.SendId,
                DeliveryDate = statusPushModel.EventDateTime,
                DropErrorCode = statusPushModel.DropCode,
                ErrorMessage = statusPushModel.DropCode,
                DeliveryStatus = statusPushModel.DeliveryStatus
            };
            var isUpdated = await _mmsRepository.UpdateDeliveryReportInMMSLogAsync(mmsLog).ConfigureAwait(false);
            if (isUpdated)
            {
                await _mmsRepository.UpdateDeliveryCountInJourneyAsync(statusPushModel).ConfigureAwait(false);
            }

            return isUpdated;
        }

        public async Task<MMSBalanceModel> GetMMSBalanceAsync(long accountId)
        {
            var settings = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);
            if (settings.IsNull())
            {
                return new MMSBalanceModel { ErrorCode = "-99", ErrorMessage = "invalid vendor settings", Status = "error" };
            }
            var postBody = new { appid = settings.AppId, signature = settings.AppKey };
            return await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSBalanceUrl, postBody.ToJsonString());
        }
        public async Task<MMSBalanceModel> GetMMSTopupHistoryAsync(long accountId)
        {
            var settings = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);
            if (settings.IsNull())
            {
                return new MMSBalanceModel { ErrorCode = "-99", ErrorMessage = "invalid vendor settings", Status = "error" };
            }
            var postBody = new { appid = settings.AppId, signature = settings.AppKey };
            var result = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSTopupHistoryUrl, postBody.ToJsonString());

            var mmsRechargehistory = result.RechargeHistory.Where(x => x.MoneyAdd != 0).ToList();
            foreach (var item in mmsRechargehistory)
            {
                item.MoneyAdd = Math.Round(item.MoneyAdd / 0.18m);
                item.MoneyBeforeRecharge = Math.Round(item.MoneyBeforeRecharge / 0.18m);
                item.MoneyAfterRecharge = item.MoneyBeforeRecharge + item.MoneyAdd;
                item.RechargeDate = Convert.ToDateTime(item.RechargeDate).ToDateTimeString();
            }

            result.RechargeHistory = mmsRechargehistory;

            return result;
        }

        public async Task<bool> AddJourneyEntryAsync(JourneyActivateModel journeyModel)
        {
            await _azureStorageRepository.AddQueueAsync(journeyModel.ToJsonString(), "journeyacitivateinfo");

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
                QuadrantInfo = currentDate.GetQuadrantMMSLogTableName(),
                Version = journeyModel.InteractionVersion
            };

            var data = await _mmsRepository.SaveJourneyAsync(journeyProcModel);

            return data;
        }

        public async Task AddQueueForIncomingMessagesExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.mmsincomingmessagesexportqueue.ToString());
        }

        public async Task AddQueueForJourneysExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.mmsjouneyexportqueue.ToString());
        }
        public async Task AddQueueForMMSLogExportAsync(LogFilterModel logFilter)
        {
            await _azureStorageRepository.AddQueueAsync(logFilter.ToJsonString(), QueueName.mmslogexportqueue.ToString());
        }
        public async Task AddQueueForOptOutMessagesExportAsync(EmailExportModel model)
        {
            await _azureStorageRepository.AddQueueAsync(model.ToJsonString(), QueueName.mmsoptoutmessagesexportqueue.ToString());
        }

        public async Task<bool> ExportJourneysAsync(string queueData, string directoryPath, string templatePath)
        {
            var emailExportModel = queueData.ConvertToModel<JourneyExportModel>();
            emailExportModel.ToDate = emailExportModel.ToDate.Date.AddDays(1);
            var data = await _mmsRepository.GetAllJourneysAsync(emailExportModel).ConfigureAwait(false);
            DataSet ds = GetJourneysExcelDataSet(data.ToList());

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(emailExportModel.AccountId);

            string fileNamePrefix = "Journeys";
            string subject = "MMS Journey Export";
            string heading = subject;

            string filePath = await SendEmail(directoryPath, templatePath, emailExportModel, accDetails, ds, fileNamePrefix, subject, heading);

            // remove local file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }

        private DataSet GetJourneysExcelDataSet(List<JourneyInfoModel> data)
        {
            var ds = new DataSet();
            var sheet = data.Select(_ => new
            {
                JourneyName = _.JourneyName,
                InitiatedDate = _.InitiatedDate.IsNull() ? string.Empty : _.InitiatedDate.Value.ToDateTimeString(),
                TotalCount = _.TotalCount,
                SuccessCount = _.DeliveredCount,
                DroppedCount = _.DroppedCount,
                SendFailedCount = _.SendFailedCount
            }).ToList();

            sheet = sheet.OrderBy(x => x.InitiatedDate).ToList();
            var dataTable = sheet.ToDataTable();
            dataTable.TableName = "MMS Journey Report";

            ds.Tables.Add(dataTable);

            return ds;
        }

        public async Task<bool> ExportIncomingMessagesAsync(bool isOptOut, string queueData, string directoryPath, string templatePath)
        {
            var emailExportModel = queueData.ConvertToModel<IncomingMessagesExportModel>();
            emailExportModel.ToDate = emailExportModel.ToDate.Date.AddDays(1);
            var data = await _mmsRepository.GetIncomingMessagesAsync(emailExportModel, isOptOut).ConfigureAwait(false);
            DataSet ds = GetIncomingMessagesExcelDataSet(data.ToList(), isOptOut);

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(emailExportModel.AccountId);

            string fileNamePrefix = isOptOut ? "OptOutMessages" : "IncomingMessages";
            string subject = isOptOut ? "MMS OptOut Messages Export" : "MMS Incoming Messages Export";
            string heading = subject;

            string filePath = await SendEmail(directoryPath, templatePath, emailExportModel, accDetails, ds, fileNamePrefix, subject, heading);

            // remove local file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }

        public async Task<bool> ExportMMSLogAsync(string queueData, string directoryPath, string templatePath)
        {
            var mmsLogExportModel = queueData.ConvertToModel<LogFilterModel>();
            var data = await _mmsRepository.GetMmsLogAsync(mmsLogExportModel).ConfigureAwait(false);
            DataSet ds = GetMMSLogExcelDataSet(data.ToList());

            var accDetails = await _wechatifyRepository.GetAccountDetailsAsync(mmsLogExportModel.AccountId);

            string fileNamePrefix = "MMSLog";
            string subject = "MMS Log Export";
            string heading = subject;

            var emailExportModel = new EmailExportModel()
            {
                AccountId = mmsLogExportModel.AccountId,
                EmailIds = mmsLogExportModel.EmailIds,
                FromDate = DateTime.UtcNow.ToChinaTime().Date,
                ToDate = DateTime.UtcNow.ToChinaTime().Date
            };

            string filePath = await SendEmail(directoryPath, templatePath, emailExportModel, accDetails, ds, fileNamePrefix, subject, heading);

            // remove local file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }

        private async Task<string> SendEmail(string appDirectoryPath, string templatePath, EmailExportModel emailExportModel, WeChatifyAccountModel accDetails, DataSet ds, string fileNamePrefix, string subject, string heading)
        {
            string dateInterval = "(" + emailExportModel.FromDate.ToString("yyyy-MM-dd") + " to " + emailExportModel.ToDate.AddDays(-1).ToString("yyyy-MM-dd") + ")";

            if (emailExportModel.FromDate.ToString("yyyy-MM-dd").Equals(emailExportModel.ToDate.AddDays(-1).ToString("yyyy-MM-dd")))
            {
                dateInterval = "(" + emailExportModel.FromDate.ToString("yyyy-MM-dd") + ")";
            }

            var fileName = accDetails.AccountName + "_" + fileNamePrefix + "_" + dateInterval + "_" + Guid.NewGuid().ToString().Substring(0, 5) + ".xlsx";

            var filePath = appDirectoryPath + "\\" + fileName;

            CreateExcelDocument(ds, filePath);

            string link;
            using (FileStream stream = File.Open(filePath, FileMode.Open))
            {
                var uri = await _azureStorageRepository.UploadStreamToBlobAsync(stream, emailExportModel.AccountId, fileName, "MMS/ExcelExport");
                link = $@"<a href='{uri}'>Click here</a>";
            }

            await _mmsEmailService.SendExportEmailAsync(emailExportModel.EmailIds, link, accDetails.AccountName, templatePath, dateInterval, subject, heading).ConfigureAwait(false);
            return filePath;
        }

        private void CreateExcelDocument(DataSet ds, string filePath)
        {

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            CreateExcel.CreateExcelDocument(ds, filePath);
        }

        private DataSet GetIncomingMessagesExcelDataSet(List<IncomingMessageModel> incomingMessages, bool isOptout)
        {
            var ds = new DataSet();
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

            ds.Tables.Add(dataTable);

            return ds;
        }

        private DataSet GetMMSLogExcelDataSet(List<LogViewModel> logViewModel)
        {
            var ds = new DataSet();

            var sheet = logViewModel.Select(_ => new MMSLogExportModel
            {
                ActivityName = _.ActivityName,
                DroppedReason = _.ErrorMessage,
                JourneyName = _.JourneyName,
                MobileNumber = _.MobileNumber,
                Version = _.VersionName,
                DeliveryDate = _.DeliveryDateString,
                DeliveryStatus = _.DeliveryStatus?.ToString(),
                MMSTemplateName = _.MMSTemplateName,
                Quadrants = _.QuadrantInfo,
                SendDate = _.SendDateString,
                SentStatus = _.SentStatus.ToString()
            }).ToList();

            sheet = sheet.OrderBy(x => x.MobileNumber).ToList();
            var dataTable = sheet.ToDataTable();

            dataTable.TableName = "MMS Log Report";

            ds.Tables.Add(dataTable);

            return ds;
        }

        public async Task<IEnumerable<JourneyInfoModel>> GetAllJourneyAsync(long accountId)
        {
            return await _mmsRepository.GetAllJourneyAsync(accountId);
        }

        public async Task<MMSJourneyInfoViewModel> GetJourneysAsync(JourneyFilterModel journeyFilter)
        {

            var (log, count) = await _mmsRepository.GetJourneysAsync(journeyFilter);
            var pieChartInfo = await _mmsRepository.GetJourneysMMSCountAsync(journeyFilter);

            var result = new MMSJourneyInfoViewModel()
            {
                JourneysInfo = new PagingModel<JourneyInfoModel>()
                {
                    Items = log.ToList(),
                    ItemsPerPage = journeyFilter.ItemsPerPage,
                    PageNumber = journeyFilter.PageNo,
                    TotalCount = count
                },
                PieChartInfo = pieChartInfo
            };


            return result;
        }

        public async Task<LogGridModel> GetJourneyDetailsAsync(long accountId, string journeyId, string quadrantTableName)
        {
            var currentQuadrant = quadrantTableName ?? DateTime.UtcNow.ToChinaTime().GetQuadrantMMSLogTableName();
            var versionDdlTask = _mmsRepository.GetVersionsAsync(journeyId, currentQuadrant);
            var activityDdlTask = _mmsRepository.GetActivitiesAsync(journeyId, currentQuadrant);
            var quadrantDdlTask = _mmsRepository.GetQuadrantsAsync();
            var logModelsTask = _mmsRepository.GetMmsLogForCurrentQuadrantAsync(accountId, journeyId, currentQuadrant);

            await Task.WhenAll(versionDdlTask, activityDdlTask, quadrantDdlTask, logModelsTask);
            var gridModel = new LogGridModel()
            {
                ActivityDdl = activityDdlTask.Result,
                VersionDdl = versionDdlTask.Result,
                QuadrantDdl = quadrantDdlTask.Result,
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

        public async Task<PagingModel<LogViewModel>> GetMmsLogDetailsAsync(LogFilterModel logFilter)
        {
            var (mmslog, totalCount) = await _mmsRepository.GetMmsLogByQuadrantAsync(logFilter);

            return new PagingModel<LogViewModel>()
            {
                ItemsPerPage = logFilter.ItemsPerPage,
                PageNumber = logFilter.PageNo,
                TotalCount = totalCount,
                Items = mmslog
            };
        }

        public async Task UpdateUnconfirmedStatusAsync(long accountId, DateTime fromDate)
        {
            for (var i = 0; i <= 10; i++)
            {
                var pendingMms = await _mmsRepository.GetPendingStatusMMSAsync(accountId, fromDate).ConfigureAwait(false);
                if (!pendingMms.Any()) { break; }
                var updateModels = pendingMms.Select(k => new MMSLogModel()
                {
                    SendId = k.SendId,
                    DeliveryStatus = DeliveryStatus.UnConfirmed,
                    ErrorMessage = DeliveryStatus.UnConfirmed.ToString(),
                    DropErrorCode = DeliveryStatus.UnConfirmed.ToString(),
                    DeliveryDate = DateTime.UtcNow.ToChinaTime()
                }).ToList();
                foreach (var model in updateModels)
                {
                    var rr = await _mmsRepository.UpdateDeliveryReportInMMSLogAsync(model);
                }
            }

        }

        public async Task ReprocessQueueAsync(string queueName)
        {
            var queueNameEnum = queueName.ToLower().ToEnum<QueueName>();

            switch (queueNameEnum)
            {
                case QueueName.mmsjourneyfailedqueue:
                    while (true)
                    {
                        var addFailedQueues = await _azureStorageRepository.GetQueuesAsync(queueName);
                        if (!addFailedQueues.Any())
                        {
                            return;
                        }
                        foreach (var failedQueue in addFailedQueues)
                        {
                            await SendJourneyMMSAsync(failedQueue).ConfigureAwait(false);
                        }
                    }

                case QueueName.mmslogaddfailedqueue:
                    while (true)
                    {
                        var addFailedQueues = await _azureStorageRepository.GetQueuesAsync(queueName);
                        if (!addFailedQueues.Any())
                        {
                            return;
                        }
                        foreach (var failedQueue in addFailedQueues)
                        {
                            await _azureStorageRepository.AddQueueAsync(failedQueue, QueueName.mmslogaddqueue.ToString()).ConfigureAwait(false);
                        }
                    }

                case QueueName.mmslogupdatefailedqueue:

                    while (true)
                    {
                        var addFailedQueues = await _azureStorageRepository.GetQueuesAsync(queueName);
                        if (!addFailedQueues.Any())
                        {
                            return;
                        }
                        foreach (var failedQueue in addFailedQueues)
                        {
                            await _azureStorageRepository.AddQueueAsync(failedQueue, QueueName.mmslogupdatequeue.ToString()).ConfigureAwait(false);
                        }
                    }
            }


        }
        public async Task SendMailDeliveryRateBelowPercent(string templatePath, string xlsPath)
        {
            var accountDetails = await _settingsRepository.GetMMSMappedAccountIdsAsync();
            TimeZoneInfo ChinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

            foreach (var accountId in accountDetails)
            {
                var vendorSetting = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);
                var accountData = await _wechatifyRepository.GetAccountDetailsAsync(accountId);
                vendorSetting.AccountName = accountData.AccountName;
                var notificationSetting = await _settingsRepository.GetMMSDeliveryReportNotificationSettingByAccountId(accountId);
                if (notificationSetting != null)
                {
                    MMSDeliveryReportRunBy runBy = (MMSDeliveryReportRunBy)notificationSetting.RunBy;
                    var chinaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChinaZone);
                    switch (runBy)
                    {
                        case MMSDeliveryReportRunBy.Daily:
                            {
                                var day = 1;
                                if (chinaTime.Hour == notificationSetting.RunOnTime)
                                {
                                    var filterDate = chinaTime.AddDays(-day);
                                    await SendNotificationMail(templatePath, vendorSetting, notificationSetting, filterDate, chinaTime, (int)notificationSetting.Percentage, xlsPath);
                                }
                            }
                            break;
                        case MMSDeliveryReportRunBy.Weekly:
                            {
                                var day = 7;
                                DayOfWeek weekDay = (DayOfWeek)notificationSetting.RunDay;
                                DayOfWeek today = DateTime.Today.DayOfWeek;
                                if (today == weekDay && chinaTime.Hour == notificationSetting.RunOnTime)
                                {
                                    var filterDate = chinaTime.AddDays(-day);
                                    await SendNotificationMail(templatePath, vendorSetting, notificationSetting, filterDate, chinaTime, (int)notificationSetting.Percentage, xlsPath);
                                }
                            }
                            break;
                        case MMSDeliveryReportRunBy.Monthly:
                            {
                                //month end configuration
                                if (notificationSetting.RunDay >= 29)
                                {
                                    var lastDayOfMonth = new DateTime(chinaTime.Year, chinaTime.Month, DateTime.DaysInMonth(chinaTime.Year, chinaTime.Month));
                                    UInt16 uIntRunDay = Convert.ToUInt16(lastDayOfMonth.ToString("dd"));
                                    notificationSetting.RunDay = Convert.ToInt16(uIntRunDay);
                                }

                                var date = chinaTime.ToString("dd");
                                if (notificationSetting.RunDay == Convert.ToUInt32(date) && chinaTime.Hour == notificationSetting.RunOnTime)
                                {
                                    var filterDate = chinaTime.AddMonths(-1);
                                    await SendNotificationMail(templatePath, vendorSetting, notificationSetting, filterDate, chinaTime, (int)notificationSetting.Percentage, xlsPath);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private async Task SendNotificationMail(string templatePath, VendorSettingsModel vendorSetting, MMSDeliveryReportNotification setting, DateTime filterDate, DateTime toDatetime, int percentage, string xlsPath)
        {
            List<ReportModel> reports = await DownloadExcelSheet(vendorSetting, filterDate, percentage, xlsPath);
            if (reports == null)
            {
                Console.WriteLine("No Report for this Account");
                return;
            }

            var reportdata = FillExcelData(reports);
            var users = await _settingsRepository.GetNotificationToUsers(vendorSetting.AccountId, NotificationType.MMSDeliveryReportSetting);
            if (!users.Any())
            {
                Console.WriteLine("No Users for this Account");
                return;
            }
            var toUser = users.Select(x => x.Email).ToArray();
            await _mmsEmailService.SendMMSDeliveryReportAlertMail(templatePath, toUser, reportdata, xlsPath, filterDate, toDatetime);
        }
        private async Task<List<ReportModel>> DownloadExcelSheet(VendorSettingsModel submailAccount, DateTime fromDate, int percentage, string ExcelFileName)
        {
            var ds = new DataSet();
            List<ReportModel> reportModels = new List<ReportModel>();
            var filterDate = fromDate.Date.ToString("yyyy-MM-dd");
            //var errorCodes = await _settingsRepository.GetErrorDescriptionAsync();
            var reportData = await _mmsRepository.GetReports(submailAccount.AccountId, filterDate);
            string quadTable = fromDate.GetQuadrantMMSLogTableName();
            var logData = await _mmsRepository.GetMMSLogByDate(submailAccount.AccountId, filterDate, quadTable);
            var result = reportData.Select(x => new ReportModel
            {
                AccountName = submailAccount.AccountName,
                ActionName = x.JourneyName,
                CampaignsRunDay = x.CreatedOn.ToString("yyyy-MM-dd"),
                TotalSentMMS = x.TotalCount,
                Delivered = x.DeliveredCount,
                Dropped = x.DroppedCount,
                Pending = logData.Where(y => y.JourneyId == x.JourneyId && y.DeliveryStatus == DeliveryStatus.Pending).Count(),
                SendFailed = x.SendFailedCount,
                Unconfirmed = logData.Where(y => y.JourneyId == x.JourneyId && y.DeliveryStatus == DeliveryStatus.UnConfirmed).Count(),
                DeliveredPercentage = (x.DeliveredCount != 0) ? ((float)(x.DeliveredCount) * 100 / x.TotalCount).ToString("0.00") + "%" : 0 + "%",
                DroppedPercentage = (x.DroppedCount != 0) ? ((float)(x.DroppedCount) * 100 / x.TotalCount).ToString("0.00") + "%" : 0 + "%",
                OthersPercentage = (100 - (((x.DeliveredCount != 0) ? ((float)(x.DeliveredCount) * 100 / x.TotalCount) : 0) + ((x.DroppedCount != 0) ? ((float)(x.DroppedCount) * 100 / x.TotalCount) : 0))).ToString("0.00") + "%"
            }).OrderBy(m => m.CampaignsRunDay).ToList();

            result = result.Where(
               k => Convert.ToDecimal(k.DeliveredPercentage.Replace("%", string.Empty)) <= Convert.ToDecimal(percentage)).ToList();
            if (!result.Any())
            {
                return null;
            }
            result.Add(new ReportModel()
            {
                CampaignsRunDay = "Total ",
                TotalSentMMS = result.Sum(k => k.TotalSentMMS),
                Delivered = result.Sum(k => k.Delivered),
                Dropped = result.Sum(k => k.Dropped),
                Pending = result.Sum(k => k.Pending),
                SendFailed = result.Sum(k => k.SendFailed),
                Unconfirmed = result.Sum(_ => _.Unconfirmed)
            });
            reportModels.AddRange(result);

            var dataTable = ConvertToDataTable(result);
            dataTable.Columns.Remove("AccountId");
            dataTable.TableName = submailAccount.AccountName;
            ds.Tables.Add(dataTable);
            CreateExcelDocument(ds, ExcelFileName);
            return reportModels;
        }
        private DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }
        private static StringBuilder FillExcelData(List<ReportModel> reports)
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
                                   + "<td style='padding:8px;'>" + reports[i].TotalSentMMS + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].Delivered + "</td> "
                                   + "<td style='padding:8px;'>" + reports[i].Dropped + "</td>"
                                   + "<td style='padding:8px;'>" + reports[i].Pending + "</td> "
                                   //   + "<td style='padding:8px;'>" + reports[i].Invalid + "</td>"
                                   //   + "<td style='padding:8px;'>" + reports[i].UnSubscribed + "</td> "
                                   //   + "<td style='padding:8px;'>" + reports[i].Unconfirmed + "</td> "
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
                                + "<td style='padding:8px;'>" + reports[i].TotalSentMMS + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].Delivered + "</td> "
                                + "<td style='padding:8px;'>" + reports[i].Dropped + "</td>"
                                + "<td style='padding:8px;'>" + reports[i].Pending + "</td> "
                                //   + "<td style='padding:8px;'>" + reports[i].Invalid + "</td>"
                                //   + "<td style='padding:8px;'>" + reports[i].UnSubscribed + "</td> "
                                //   + "<td style='padding:8px;'>" + reports[i].Unconfirmed + "</td> "
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

        public async Task SendEmailForMMSBalanceThreshold(string TemplatePath)
        {
            TimeZoneInfo ChinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            var chinaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChinaZone);
            if (chinaTime.Hour == 7)
            {
                List<VendorSettingsModel> allVendorSettings = await _settingsRepository.GetAllMMSVendorSettingsAsync();
                foreach (var item in allVendorSettings)
                {
                    MMSBalanceThreshold thresHoldSettingDetail = await _settingsRepository.GetMMSBalanceThresholdByAccountId(item.AccountId);
                    var postBody = new { appid = item.AppId, signature = item.AppKey };
                    var mmsBalance = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSBalanceUrl, postBody.ToJsonString());
                    if (mmsBalance != null && thresHoldSettingDetail != null && Convert.ToInt64(mmsBalance.MMSBalance) < thresHoldSettingDetail.ThresholdCount)
                    {
                        List<NotifyUserList> userList = await _settingsRepository.GetNotificationToUsers(item.AccountId, NotificationType.MMSBalanceThreshold);
                        if (userList != null && userList.Count > 0)
                        {
                            WeChatifyAccountModel accountDetail = await _wechatifyRepository.GetAccountDetailsAsync(item.AccountId);
                            MMSThersholdNotifyUser detail = new MMSThersholdNotifyUser()
                            {
                                AccountName = accountDetail?.AccountName,
                                ThresholdCount = thresHoldSettingDetail.ThresholdCount,
                                NotifyUserDetails = userList
                            };
                            await _mmsEmailService.SendMMSBalanceThresholdAlertMail(detail, TemplatePath, "MMS Threshold Notification");
                        }
                    }
                }
            }
        }

        public async Task MMSUsageCountUpdateAsync(long accountId)
        {

            var settings = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);

            var postBody = new { appid = settings.AppId, signature = settings.AppKey };
            var balance = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSBalanceUrl, postBody.ToJsonString());
            var mmsBalanceHistory = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSTopupHistoryUrl, postBody.ToJsonString());

            var lastMonth = DateTime.UtcNow.ToChinaTime().AddMonths(-1);
            var startDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usedCount = await _mmsRepository.GetUsedMMSCountAsync(accountId, startDate, endDate);

            var mmsRechargeHistory = mmsBalanceHistory.RechargeHistory.Where(x => x.MoneyAdd != 0).ToList();
            foreach (var item in mmsRechargeHistory)
            {
                item.MoneyAdd = Math.Round(item.MoneyAdd / 0.18m);
                item.MoneyBeforeRecharge = Math.Round(item.MoneyBeforeRecharge / 0.18m);
                item.MoneyAfterRecharge = item.MoneyBeforeRecharge + item.MoneyAdd;
                item.RechargeDate = item.RechargeDateTime.ToDateTimeString();
            }

            var rechargeCount = mmsRechargeHistory.Where(k =>
                    k.RechargeDateTime.Date >= startDate.Date && k.RechargeDateTime.Date < endDate.Date)
                .Sum(y => y.MoneyAdd);

            var usageModel = new MMSUsageModel()
            {
                Year = lastMonth.Year,
                Balance = balance.MMSBalance,
                UsedCount = usedCount,
                AccountId = accountId,
                Month = (Months)lastMonth.Month,
                RechargeCount = Convert.ToInt32(rechargeCount)
            };

            await _mmsRepository.AddOrUpdateMMSUsageDetailAsync(usageModel);

        }

        public async Task MMSUsageCountUpdateAsync(long accountId, DateTime dateToCalculate)
        {

            var settings = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);

            var postBody = new { appid = settings.AppId, signature = settings.AppKey };
            var balance = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSBalanceUrl, postBody.ToJsonString());
            var mmsBalanceHistory = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSTopupHistoryUrl, postBody.ToJsonString());

            var startDate = new DateTime(dateToCalculate.Year, dateToCalculate.Month, 1);
            var endDate = startDate.AddMonths(1);

            var usedCount = await _mmsRepository.GetUsedMMSCountAsync(accountId, startDate, endDate);

            var mmsRechargeHistory = mmsBalanceHistory.RechargeHistory.Where(x => x.MoneyAdd != 0).ToList();
            foreach (var item in mmsRechargeHistory)
            {
                item.MoneyAdd = Math.Round(item.MoneyAdd / 0.18m);
                item.MoneyBeforeRecharge = Math.Round(item.MoneyBeforeRecharge / 0.18m);
                item.MoneyAfterRecharge = item.MoneyBeforeRecharge + item.MoneyAdd;
                item.RechargeDate = item.RechargeDateTime.ToDateTimeString();
            }

            var rechargeCount = mmsRechargeHistory.Where(k =>
                    k.RechargeDateTime.Date >= startDate.Date && k.RechargeDateTime.Date < endDate.Date)
                .Sum(y => y.MoneyAdd);

            var usageModel = new MMSUsageModel()
            {
                Year = dateToCalculate.Year,
                Balance = balance.MMSBalance,
                UsedCount = usedCount,
                AccountId = accountId,
                Month = (Months)dateToCalculate.Month,
                RechargeCount = Convert.ToInt32(rechargeCount)
            };

            await _mmsRepository.AddOrUpdateMMSUsageDetailAsync(usageModel);

        }

        public async Task<MMSUsageViewModel> GetMmsUsageDetailAsync(long accountId, int? year)
        {

            var currentYear = year ?? DateTime.UtcNow.ToChinaTime().Year;

            var mmsUsage = await _mmsRepository.GetMMSUsageDetailAsync(accountId, currentYear);

            var years = await _mmsRepository.GetMMSUsageYearsAsync(accountId);

            if (!years.Any()) { years = new List<int>(DateTime.Now.Year); }

            var result = new MMSUsageViewModel
            {
                MmsUsage = mmsUsage,
                Years = years
            };

            return result;
        }

        public async Task<List<MMSTopupHistoryModel>> GetMMSTopUpHistoryAsync(long accountId, int year, int month)
        {
            var settings = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);
            if (settings.IsNull())
            {
                return null;
            }
            var postBody = new { appid = settings.AppId, signature = settings.AppKey };
            var result = await HttpHelper<MMSBalanceModel>.HttpPostAsync(SubmailAPIUrls.MMSTopupHistoryUrl, postBody.ToJsonString());

            var mmsRechargeHistory = result.RechargeHistory.Where(x => x.MoneyAdd != 0).ToList();
            foreach (var item in mmsRechargeHistory)
            {
                item.MoneyAdd = Math.Round(item.MoneyAdd / 0.18m);
                item.MoneyBeforeRecharge = Math.Round(item.MoneyBeforeRecharge / 0.18m);
                item.MoneyAfterRecharge = item.MoneyBeforeRecharge + item.MoneyAdd;
                item.RechargeDate = item.RechargeDateTime.ToDateTimeString();
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var data = mmsRechargeHistory.Where(k =>
                k.RechargeDateTime.Date >= startDate.Date && k.RechargeDateTime.Date < endDate.Date).ToList();

            return data;

        }

    }
}
