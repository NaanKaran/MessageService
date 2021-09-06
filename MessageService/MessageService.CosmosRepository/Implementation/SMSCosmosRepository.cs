using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageService.CosmosRepository.Interface;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using MessageService.CosmosRepository.Utility;
using MessageService.Models.APIModels;
using MessageService.Models.CosmosModel;
using MessageService.Models.ExportModels;
using MessageService.Models.StoredProcedureModels;
using MessageService.Models.SMSModels;

namespace MessageService.CosmosRepository.Implementation
{
    public class SMSCosmosRepository : CosmosBaseRepository, ISMSCosmosRepository
    {

        public async Task<(List<IncomingMessageDocumentModel>, int)> GetIncomingMessagesAsync(GetIncomingMessagesModel model)
        {
            var query = new StringBuilder($@"SELECT [PROJECTION] FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");
            if (model.IsOptOut)
            {
                query.Append(" AND c.isoptout = @isoptout ");
            }
            if (model.FromDate.IsNotNull())
            {
                query.Append(" AND c.createdon >= @fromdate ");
            }
            if (model.ToDate.IsNotNull())
            {
                query.Append(" AND c.createdon <= @todate ");
            }
            if (model.JourneyName.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.journeyname = @journeyname ");
            }
            if (model.MobileNumber.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.mobilenumber = @mobilenumber ");
            }
            if (model.Content.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.content = @content ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.IncomingMessage },
                { "@partitionkey", CosmosDocumentType.IncomingMessage.ToString() },
                { "@accountid", model.AccountId },
                { "@isoptout", model.IsOptOut },
                { "@journeyname", model.JourneyName },
                { "@mobilenumber", model.MobileNumber },
                { "@content", model.Content },
                { "@fromdate", model.FromDate },
                { "@todate", model.ToDate },
                { "@pageno", (model.PageNo - 1) * model.ItemsPerPage },
                { "@itemsperpage", model.ItemsPerPage }
            };

            var count = (await ReadDocumentsAsync<int>(query.ToString().Replace("[PROJECTION]", " VALUE COUNT(1) "), parameters)).Sum();

            query.Append(" ORDER BY c.createdon desc OFFSET @pageno LIMIT @itemsperpage ");

            var result = await ReadDocumentsAsync<IncomingMessageDocumentModel>(query.ToString().Replace("[PROJECTION]", " * "), parameters);

            return (result, count);
        }

        public async Task<bool> SaveIncomingMessageAsync(IncomingMessageDocumentModel incomingMessage)
        {
            var result = await InsertDocumentAsync(incomingMessage);
            return result.IsNotNull();
        }

        public async Task<(List<VerificationSMSDocumentModel>, int)> GetVerificationSMSAsync(GetVerificationSMSModel model)
        {
            var query = new StringBuilder($@"SELECT [PROJECTION] FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");
            if (model.FollowerName.IsNotNullOrWhiteSpace())
            {        
                
                query.Append(" AND CONTAINS(LOWER(c.followername), @followername) ");
            }
            if (model.FromDate.IsNotNull())
            {
                query.Append(" AND c.senddate >= @fromdate ");
            }
            if (model.ToDate.IsNotNull())
            {
                query.Append(" AND c.senddate <= @todate ");
            }
            if (model.MobileNumber.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.mobilenumber = @mobilenumber ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VerificationSMSLog },
                { "@partitionkey", CosmosDocumentType.VerificationSMSLog.ToString() },
                { "@accountid", model.AccountId },
                { "@followername", model.FollowerName.ToLower() },
                { "@mobilenumber", model.MobileNumber },
                { "@fromdate", model.FromDate },
                { "@todate", model.ToDate },
                { "@pageno", (model.PageNo - 1) * model.ItemsPerPage },
                { "@itemsperpage", model.ItemsPerPage }
            };

            var count = (await ReadDocumentsAsync<int>(query.ToString().Replace("[PROJECTION]", " VALUE COUNT(1) "), parameters)).Sum();

            query.Append(" ORDER BY c.senddate desc OFFSET @pageno LIMIT @itemsperpage ");

            var result = await ReadDocumentsAsync<VerificationSMSDocumentModel>(query.ToString().Replace("[PROJECTION]", " * "), parameters);

            return (result, count);
        }

        public async Task<List<VerificationSMSDocumentModel>> GetVerificationSMSAsync(VerificationSMSExportModel model)
        {
            var query = new StringBuilder($@"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");
            if (model.FollowerName.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND CONTAINS(LOWER(c.followername), @followername) ");
            }
            if (model.FromDate.IsNotNull())
            {
                query.Append(" AND c.senddate >= @fromdate ");
            }
            if (model.ToDate.IsNotNull())
            {
                query.Append(" AND c.senddate <= @todate ");
            }
            //if (model.MobileNumber.IsNotNullOrWhiteSpace())
            //{
            //    query.Append(" AND c.mobilenumber = @mobilenumber ");
            //} 

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.VerificationSMSLog },
                { "@partitionkey", CosmosDocumentType.VerificationSMSLog.ToString() },
                { "@accountid", model.AccountId },
                { "@followername", model.FollowerName.ToLower() },
                //{ "@mobilenumber", model.MobileNumber },            
                { "@fromdate", model.FromDate },
                { "@todate", model.ToDate }
            };

            var result = await ReadDocumentsAsync<VerificationSMSDocumentModel>(query.ToString().Replace("[PROJECTION]", " * "), parameters);

            return result;
        }

        public async Task<bool> SaveVerificationSMSAsync(VerificationSMSDocumentModel verificationSMS)
        {
            var result = await InsertDocumentAsync(verificationSMS);
            return result.IsNotNull();
        }

        public async Task<bool> UpdateUnConfirmedStatusAsync(List<SMSLogDocumentModel> model)
        {
            var result = await BulkInsertDocumentsAsync(model, true);
            return result.IsNotNull();
        }

        public async Task<List<IncomingMessageDocumentModel>> GetIncomingMessagesAsync(IncomingMessagesExportModel model, bool isOptOut)
        {
            var query = new StringBuilder($@"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");
            if (isOptOut)
            {
                query.Append(" AND c.isoptout = @isoptout ");
            }
            if (model.FromDate.IsNotNull())
            {
                query.Append(" AND c.createdon >= @fromdate ");
            }
            if (model.ToDate.IsNotNull())
            {
                query.Append(" AND c.createdon <= @todate ");
            }
            if (model.JourneyName.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.journeyname = @journeyname ");
            }
            if (model.MobileNumber.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.mobilenumber = @mobilenumber ");
            }
            if (model.Content.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.content = @content ");
            }
            query.Append(" ORDER BY c.createdon ");

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.IncomingMessage },
                { "@partitionkey", CosmosDocumentType.IncomingMessage.ToString() },
                { "@accountid", model.AccountId },
                { "@isoptout", isOptOut },
                { "@journeyname", model.JourneyName },
                { "@mobilenumber", model.MobileNumber },
                { "@content", model.Content },
                { "@fromdate", model.FromDate },
                { "@todate", model.ToDate }
            };

            var result = await ReadDocumentsAsync<IncomingMessageDocumentModel>(query.ToString(), parameters);

            return result;
        }

        public async Task<List<IncomingMessageDocumentModel>> GetIncomingMessagesToUpdateDEAsync(long accountId)
        {
            var query = @"SELECT TOP 1000 * FROM c WHERE c.type = @type 
                          AND c.partitionkey = @partitionkey AND c.isupdatedintode = @isupdatedintode 
                          AND c.accountid = @accountid ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.IncomingMessage },
                { "@partitionkey", CosmosDocumentType.IncomingMessage.ToString() },
                { "@isupdatedintode", false },
                { "@accountid", accountId }
            };

            var result = await ReadDocumentsAsync<IncomingMessageDocumentModel>(query, parameters);

            return result;
        }

        public async Task<bool> UpdateDataExtensionPushInIncomingMessageAsync(List<IncomingMessageDocumentModel> logModels)
        {
            var result = await BulkInsertDocumentsAsync(logModels, true);
            return result.OperationSuccess;
        }

        public async Task<bool> UpdateDataExtensionPushInSMSLogAsync(List<SMSLogDocumentModel> logModels)
        {
            var result = await BulkInsertDocumentsAsync(logModels, true);
            return result.OperationSuccess;
        }

        public async Task<bool> AddOrUpdateActivityInfoAsync(List<ActivityInfoDocumentModel> models)
        {
            var result = await BulkInsertDocumentsAsync(models, true);
            return result.OperationSuccess;
        }

        public async Task<bool> AddOrUpdateJourneyDetailsAsync(JourneyProcModel journeyInfo)
        {
            var query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.id = @id";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@id", journeyInfo.JourneyId },
            };

            var journeyData = await ReadDocumentAsync<JourneyInfoDocumentModel>(query, parameters);

            if (journeyData.IsNull())
            {
                var newJourney = new JourneyInfoDocumentModel()
                {
                    Id = journeyInfo.JourneyId,
                    AccountId = journeyInfo.AccountId,
                    JourneyKey = journeyInfo.JourneyKey,
                    JourneyName = journeyInfo.JourneyName,
                    LastTriggeredOn = journeyInfo.CreatedOn,
                    InitiatedDate = journeyInfo.CreatedOn
                };
                await InsertDocumentAsync(newJourney);
            }
            else
            {
                journeyData.LastTriggeredOn = journeyInfo.CreatedOn;
                await UpsertDocumentAsync(journeyData);
            }

            query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.id = @id";
            parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Interaction },
                { "@partitionkey", CosmosDocumentType.Interaction.ToString() },
                { "@id", journeyInfo.InteractionId },
            };

            var interaction = await ReadDocumentAsync<InteractionInfoDocumentModel>(query, parameters);
            if (interaction.IsNull())
            {
                var newInteraction = new InteractionInfoDocumentModel()
                {
                    Id = journeyInfo.InteractionId,
                    Version = journeyInfo.Version,
                    JourneyId = journeyInfo.JourneyId,
                    AccountId = journeyInfo.AccountId,
                    PublishedDate = journeyInfo.CreatedOn,
                    CreatedOn = journeyInfo.CreatedOn
                };
                await InsertDocumentAsync(newInteraction);
            }
            else
            {
                interaction.CreatedOn = journeyInfo.CreatedOn;
                await UpsertDocumentAsync(interaction);
            }
            await SaveActivityInfo(journeyInfo);
            return true;
        }

        private async Task SaveActivityInfo(JourneyProcModel journeyInfo)
        {
            var query = "SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.id = @id";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@id", journeyInfo.ActivityId },
            };

            var activity = await ReadDocumentAsync<InteractionInfoDocumentModel>(query, parameters);
            if (activity.IsNull())
            {
                var newActivity = new ActivityInfoDocumentModel()
                {
                    Id = journeyInfo.ActivityId,
                    InteractionId = journeyInfo.InteractionId,
                    ActivityName = journeyInfo.ActivityName,
                    JourneyId = journeyInfo.JourneyId,
                    Type = CosmosDocumentType.Activity,
                    AccountId = journeyInfo.AccountId,
                    CreatedOn = journeyInfo.CreatedOn
                };
                await InsertDocumentAsync(newActivity);
            }
            else
            {
                activity.CreatedOn = journeyInfo.CreatedOn;
                await UpsertDocumentAsync(activity);
            }
        }

        public async Task<bool> AddOrUpdateJourneyInfoAsync(JourneyInfoDocumentModel model)
        {
            var result = await UpsertDocumentAsync(model);
            return result.IsNotNull();
        }

        public async Task<List<JourneyInfoDocumentModel>> GetJourneysAsync(JourneyExportModel model)
        {
            var query = new StringBuilder("SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey and c.accountid = @accountid ");

            string journeyIds = "";
            if (model.JourneyIds!=null && model.JourneyIds.Count() > 0)
            {
                query.Append(" AND c.id in (@journeyid) ");
                journeyIds = $"'{string.Join("','", model.JourneyIds)}'";
            }
            query.Append(" order by c.lasttriggeredon desc");

           var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@accountid", model.AccountId },
                //{ "@journeyid", model.JourneyId }
            };
            string finalQuery = query.ToString().Replace("@journeyid", journeyIds);
            var result = await ReadDocumentsAsync<JourneyInfoDocumentModel>(finalQuery.ToString(), parameters);

            return result;
        }
        public async Task<List<SMSLogDocumentModel>> GetSMSLogForExportAsync(JourneyExportModel model, string journeyId)
        {
            var query = new StringBuilder("SELECT * FROM c WHERE c.type = @type AND  c.accountid = @accountid AND c.partitionkey = @partitionkey");
            if (model.FromDate.IsNotNull())
            {
                query.Append(" AND c.senddate >= @fromdate ");
            }
            if (model.ToDate.IsNotNull())
            {
                query.Append(" AND c.senddate <= @todate ");
            }
            query.Append(" order by c.senddate desc");
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@accountid", model.AccountId },
                { "@fromdate", model.FromDate },
                { "@todate", model.ToDate },
                { "@partitionkey", journeyId }
            };
            var result = await ReadDocumentsWithScaleUpAsync<SMSLogDocumentModel>(query.ToString(), parameters,-1,true);
            return result;
        }

        public async Task<List<string>> GetDistinctJourneyForExportAsync(JourneyExportModel model)
        {
            var query = new StringBuilder("SELECT Distinct Value c.journeyid FROM c WHERE c.type = @type AND  c.accountid = @accountid ");
            if (model.FromDate.IsNotNull())
            {
                query.Append(" AND c.senddate >= @fromdate ");
            }
            if (model.ToDate.IsNotNull())
            {
                query.Append(" AND c.senddate <= @todate ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@accountid", model.AccountId },
                { "@fromdate", model.FromDate },
                { "@todate", model.ToDate }
            };
            var result = await ReadDocumentsWithScaleUpAsync<string>(query.ToString(), parameters, -1, true);
            return result;
        }

        public async Task<List<ActivityInfoDocumentModel>> GetActivitiesAsync(long accountId, DateTime dateFrom)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid AND c.createdon >= @datefrom ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@accountid", accountId },
                { "@datefrom", dateFrom },
            };

            var result = await ReadDocumentsAsync<ActivityInfoDocumentModel>(query, parameters);
            return result;
        }

        public async Task<List<ActivityInfoDocumentModel>> GetActivitiesAsync(long accountId, List<string> activityIds)
        {


            var ids = "( '" + string.Join("','", activityIds) + "' )";

            var query = $@"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid AND c.id in {ids} ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@accountid", accountId }
            };

            var result = await ReadDocumentsAsync<ActivityInfoDocumentModel>(query, parameters);
            return result;
        }

        public async Task<List<string>> GetDistinctActivitiesAsync(long accountId, DateTime dateFrom)
        {
            var query = @"SELECT Distinct value c.activityid FROM c WHERE c.type = @type 
                             AND c.accountid = @accountid AND c.senddate >= @datefrom ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@accountid", accountId },
                { "@datefrom", dateFrom },
            };

            var result = await ReadDocumentsAsync<string>(query, parameters,-1,true);
            return result;
        }

        public async Task<string> GetJourneyIdAsync(string interactionId)
        {
            var query = @"SELECT VALUE c.journeyid FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.id = @interactionid ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Interaction },
                { "@partitionkey", CosmosDocumentType.Interaction.ToString() },
                { "@interactionid", interactionId }
            };

            var result = await ReadDocumentAsync<string>(query, parameters);
            return result;

        }

        public async Task<JourneyInfoDocumentModel> GetJourneyAsync(string id)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.id = @id ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@id", id }
            };

            var result = await ReadDocumentAsync<JourneyInfoDocumentModel>(query, parameters);
            return result;

        }

        public async Task<ActivityInfoDocumentModel> GetActivityAsync(string id)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.id = @id ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@id", id }
            };

            var result = await ReadDocumentAsync<ActivityInfoDocumentModel>(query, parameters);
            return result;

        }
        public async Task<List<ActivityInfoDocumentModel>> GetActivityListAsync(long accountId, string[] ids)
        {
            var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            query += $" AND c.id in ('{ string.Join("','", ids) }')";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentsAsync<ActivityInfoDocumentModel>(query, parameters, -1, true);
            return result;
        }
        public async Task<bool> InsertSMSLogAsync(SMSLogDocumentModel logModel)
        {
            var isInserted = await InsertDocumentAsync(logModel);
            return isInserted.IsNotNull();
        }
        public async Task<bool> UpsertSMSUsageAsync(SMSUsageDocumentModel model)
        {
            var isInserted = await UpsertDocumentAsync(model);
            return isInserted.IsNotNull();
        }

        ////public async Task<bool> UpdateCountInJourneyAsync(SMSLogDocumentModel logModel)
        ////{
        ////    var journeyInfo = await SMSJourneyInfoCollection.FirstOrDefaultAsync(k => k.Id == logModel.JourneyId);
        ////    var interactionInfo = await SMSInteractionInfoCollection.FirstOrDefaultAsync(k => k.Id == logModel.InteractionId);
        ////    var activityInfo = await SMSActivityInfoCollection.FirstOrDefaultAsync(k => k.Id == logModel.ActivityId);

        ////    if (logModel.SentStatus == SendStatus.Success)
        ////    {
        ////        journeyInfo.TotalCount += 1;
        ////        interactionInfo.TotalCount += 1;
        ////        activityInfo.TotalCount += 1;
        ////    }
        ////    else
        ////    {
        ////        journeyInfo.TotalCount += 1; journeyInfo.SendFailedCount += 1;
        ////        interactionInfo.TotalCount += 1; interactionInfo.SendFailedCount += 1;
        ////        activityInfo.TotalCount += 1; activityInfo.SendFailedCount += 1;
        ////    }

        ////    await UpsertDocumentAsync(journeyInfo);
        ////    await UpsertDocumentAsync(interactionInfo);
        ////    await UpsertDocumentAsync(activityInfo);

        ////    return true;
        ////}

        public async Task<string> GetJourneyNameByMobileNumberAsync(string mobileNumber, long accountId)
        {
            var query = @"SELECT TOP 1 * FROM c WHERE c.type = @type AND c.accountid = @accountid 
                             AND c.mobilenumber = @mobilenumber ORDER BY c.senddate desc ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@accountid", accountId },
                { "@mobilenumber", mobileNumber }
            };

            var smsLog = await ReadDocumentAsync<SMSLogDocumentModel>(query, parameters, -1, true);

            query = @"SELECT TOP 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey and c.id = @journeyid ";

            parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@journeyid", smsLog.JourneyId },
            };


            var journey = await ReadDocumentAsync<JourneyInfoDocumentModel>(query, parameters);

            return journey.JourneyName;
        }

        public async Task<SMSLogDocumentModel> UpdateDeliveryReportInSMSLogAsync(SMSLogDocumentModel model)
        {
            var query = @"SELECT TOP 1 * FROM c WHERE c.type = @type AND c.id = @id ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@id", model.Id }
            };

            var smsLog = await ReadDocumentAsync<SMSLogDocumentModel>(query, parameters, 1, true);
            if (smsLog.IsNotNull())
            {
                smsLog.DeliveryStatus = model.DeliveryStatus;
                smsLog.DeliveryDate = model.DeliveryDate;
                smsLog.ErrorMessage = model.ErrorMessage;
                smsLog.DropErrorCode = model.DropErrorCode;
                var result = await UpsertDocumentAsync(smsLog);
                return smsLog;

            }
            return null;

        }

        public async Task<bool> UpdateVerificationSMSAsync(SMSLogDocumentModel model)
        {
            var query = @"SELECT TOP 1 * FROM c WHERE c.type = @type AND c.id = @id AND c.partitionkey = @partitionkey ";

            var parameters = new Dictionary<string, object>() {
                { "@partitionkey", CosmosDocumentType.VerificationSMSLog.ToString() },
                { "@type", CosmosDocumentType.VerificationSMSLog },
                { "@id", model.Id }
            };

            var smsLog = await ReadDocumentAsync<VerificationSMSDocumentModel>(query, parameters, 1);
            if (smsLog.IsNotNull())
            {
                smsLog.DeliveryStatus = model.DeliveryStatus;
                smsLog.DeliveryDate = model.DeliveryDate;                
                smsLog.DropErrorCode = model.DropErrorCode;
                var result = await UpsertDocumentAsync(smsLog);
                return result.IsNotNull();

            }
            return false;

        }

        ////public async Task<bool> UpdateDeliveryCountInJourneyAsync(SubmailStatusPushModel statusPushModel)
        ////{
        ////    var smsLog = await SMSLogCollection.FirstOrDefaultAsync(k => k.Id == statusPushModel.SendId);
        ////    if (smsLog.IsNull())
        ////    {
        ////        return false;
        ////    }
        ////    var journeyInfo = await SMSJourneyInfoCollection.FirstOrDefaultAsync(k => k.Id == smsLog.JourneyId);
        ////    var interactionInfo = await SMSInteractionInfoCollection.FirstOrDefaultAsync(k => k.Id == smsLog.InteractionId);
        ////    var activityInfo = await SMSActivityInfoCollection.FirstOrDefaultAsync(k => k.Id == smsLog.ActivityId);

        ////    if (statusPushModel.DeliveryStatus == DeliveryStatus.Delivered)
        ////    {
        ////        journeyInfo.DeliveredCount += 1;
        ////        interactionInfo.DeliveredCount += 1;
        ////        activityInfo.DeliveredCount += 1;
        ////    }
        ////    else if (statusPushModel.DeliveryStatus == DeliveryStatus.Dropped)
        ////    {
        ////        journeyInfo.DroppedCount += 1;
        ////        interactionInfo.DroppedCount += 1;
        ////        activityInfo.DroppedCount += 1;
        ////    }
        ////    else
        ////    {
        ////        return false;
        ////    }
        ////    await UpsertDocumentAsync(journeyInfo);
        ////    await UpsertDocumentAsync(interactionInfo);
        ////    await UpsertDocumentAsync(activityInfo);

        ////    return true;
        ////}

        public async Task<(List<JourneyInfoDocumentModel>, List<JourneyInfoDocumentModel>, int)> GetJourneysAsync(JourneyFilterModel logFilterModel)
        {
            var query = new StringBuilder($@"SELECT [PROJECTION] FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");
            string journeyIds = "";
            if (logFilterModel.JourneyIdList.Count>0)
            {
                query.Append(" AND c.id in (@journeyid) ");
                journeyIds = $"'{string.Join("','",logFilterModel.JourneyIdList)}'";                
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@accountid", logFilterModel.AccountId },
                { "@journeyid",journeyIds },
                { "@pageno", (logFilterModel.PageNo - 1) * logFilterModel.ItemsPerPage },
                { "@itemsperpage", logFilterModel.ItemsPerPage }
            };

            string finalQuery = query.ToString().Replace("@journeyid", journeyIds);

            var count = (await ReadDocumentsAsync<int>(finalQuery.Replace("[PROJECTION]", " VALUE COUNT(1) "), parameters)).Sum();
            //query.Append("ORDER BY c.lasttriggeredon desc OFFSET @pageno LIMIT @itemsperpage ");
            var pieChartResult = await ReadDocumentsAsync<JourneyInfoDocumentModel>(finalQuery.Replace("[PROJECTION]", " * "), parameters);
            var result = pieChartResult.OrderByDescending(x => x.LastTriggeredOn).Skip((logFilterModel.PageNo - 1) * logFilterModel.ItemsPerPage).Take(logFilterModel.ItemsPerPage).ToList();
            return (result, pieChartResult, count);
        }

        public async Task<List<VersionDropDownModel>> GetVersionsAsync(long accountId, string journeyId)
        {
            var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.journeyid = @journeyid AND c.accountid = @accountid ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Interaction },
                { "@partitionkey", CosmosDocumentType.Interaction.ToString() },
                { "@journeyid", journeyId },
                { "@accountid", accountId },
            };

            var result = await ReadDocumentsAsync<InteractionInfoDocumentModel>(query, parameters);
            var data = result.Where(k => k.JourneyId == journeyId).Select(y =>
                new VersionDropDownModel()
                {
                    Version = y.Version,
                    InteractionId = y.Id
                }).ToList();

            return data;
        }


        public async Task<List<ActivityInfoDocumentModel>> GetActivitiesAsync(long accountId, string journeyId)
        {
            var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.journeyid = @journeyid AND c.accountid = @accountid";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@journeyid", journeyId },
                { "@accountid", accountId },
            };

            var result = await ReadDocumentsAsync<ActivityInfoDocumentModel>(query, parameters);

            return result;
        }

        public async Task<List<JourneyInfoDocumentModel>> GetJourneysAsync(long accountId)
        {
            var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@accountid", accountId },
            };

            var result = await ReadDocumentsAsync<JourneyInfoDocumentModel>(query, parameters);
            return result;
        }
        public async Task<List<JourneyInfoDocumentModel>> GetJourneysListAsync(long accountId,string[] journeyId)
        {
            var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.accountid = @accountid";
            query += $" AND c.id in ('{ string.Join("','", journeyId) }')";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentsAsync<JourneyInfoDocumentModel>(query, parameters,-1,true);
            return result;
        }

        public async Task<List<InteractionInfoDocumentModel>> GetVersionsListByIdsAsync(long accountId, string[] ids)
        {
            var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey  AND c.accountid = @accountid ";
            query += $" AND c.id in ('{ string.Join("','", ids) }')";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Interaction },
                { "@partitionkey", CosmosDocumentType.Interaction.ToString() },
                { "@accountid", accountId },
            };
            var result = await ReadDocumentsAsync<InteractionInfoDocumentModel>(query, parameters);
            return result;
        }

        public async Task<(List<LogViewModel>, int)> GetSMSLogAsync(long accountId, string journeyId)
        {

            var query = new StringBuilder($@"SELECT [PROJECTION] FROM c WHERE c.partitionkey = @partitionkey AND c.type = @type
                             AND c.accountid = @accountid");


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId },
                { "@accountid", accountId },
                { "@pageno", 0 },
                { "@itemsperpage", 10 }
            };


            var count = (await ReadDocumentsWithScaleUpAsync<int>(query.ToString().Replace("[PROJECTION]", " VALUE COUNT(1) "), parameters)).Sum();
            query.Append(" ORDER BY c.senddate desc OFFSET @pageno LIMIT @itemsperpage ");
            var result = await ReadDocumentsWithScaleUpAsync<SMSLogDocumentModel>(query.ToString().Replace("[PROJECTION]", " * "), parameters);
            var logModels = result.Select(k => new LogViewModel()
            {
                SMSContent = k.SMSContent,
                ActivityId = k.ActivityId,
                InteractionId = k.InteractionId,
                AccountId = k.AccountId,
                JourneyId = k.JourneyId,
                DeliveryStatus = k.DeliveryStatus,
                SentStatus = k.SentStatus,
                SendId = k.Id,
                SendDate = k.SendDate,
                DeliveryDate = k.DeliveryDate,
                ErrorMessage = k.ErrorMessage,
                DropErrorCode = k.DropErrorCode,
                MobileNumber = k.MobileNumber
            }).ToList();

            await SetVersionAndActivityName(logModels);

            return (logModels, count);
        }


        public async Task<int> GetSMSLogCountBySendStatusAsync(long accountId, string journeyId, DateTime dateFrom, SendStatus sendStatus)
        {
            var query = @"SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey
                             AND c.accountid = @accountid AND c.sentstatus = @sendstatus AND c.senddate >= @senddate";


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@accountid", accountId},
                { "@sendstatus", sendStatus},
                { "@senddate", dateFrom},
            };
            var count = (await ReadDocumentsAsync<int>(query.ToString(), parameters)).Sum();
            return count;
        }

        public async Task<int> GetSMSLogCountBySendStatusAsync(string journeyId, SendStatus sendStatus, long? accountId = null)
        {
            StringBuilder query = new StringBuilder(@"SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey
                             AND c.sentstatus = @sendstatus ");

            if (accountId.IsNotNull())
            {
                query.Append(" AND c.accountid = @accountid ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@sendstatus", sendStatus},
                { "@accountid", accountId},
            };
            var count = (await ReadDocumentsAsync<int>(query.ToString(), parameters)).Sum();
            return count;
        }

        public async Task<int> GetSMSLogCountByDeliveryStatusAsync(long accountId, string journeyId, DateTime dateFrom,
            DeliveryStatus deliveryStatus)
        {
            var query = @"SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey
                             AND c.accountid = @accountid AND c.deliverystatus = @deliverystatus AND c.senddate >= @senddate ";


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@accountid", accountId},
                { "@deliverystatus", deliveryStatus},
                { "@senddate", dateFrom}
            };
            var count = (await ReadDocumentsAsync<int>(query.ToString(), parameters)).Sum();
            return count;
        }

        public async Task<SMSLogDocumentModel> GetFirstSMSLogAsync(long accountId, string journeyId, DateTime dateFrom)
        {
            var query = @"SELECT Top 1 * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey
                             AND c.accountid = @accountid  AND c.senddate >= @senddate  order by c.senddate asc";


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@accountid", accountId},
                { "@senddate", dateFrom}
            };
            var data = await ReadDocumentAsync<SMSLogDocumentModel>(query, parameters);
            return data;
        }

        public async Task<int> GetSMSLogCountByDeliveryStatusAsync(string journeyId, DeliveryStatus deliveryStatus, long? accountId = null)
        {
            StringBuilder query = new StringBuilder(@"SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey
                             AND c.deliverystatus = @deliverystatus ");

            if (accountId.IsNotNull())
            {
                query.Append(" AND c.accountid = @accountid ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@deliverystatus", deliveryStatus},
                { "@accountid", accountId},
            };
            var count = (await ReadDocumentsAsync<int>(query.ToString(), parameters)).Sum();
            return count;
        }

        public async Task<int> GetSMSLogCountAsync(long accountId, string journeyId, DateTime dateFrom)
        {
            var query = @"SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey
                             AND c.accountid = @accountid AND c.senddate >= @senddate";


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@accountid", accountId},
                { "@senddate", dateFrom},
            };
            var count = (await ReadDocumentsAsync<int>(query.ToString(), parameters)).Sum();
            return count;
        }

        public async Task<int> GetSMSLogCountAsync(string journeyId, long? accountId = null)
        {
            StringBuilder query = new StringBuilder(@"SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey ");

            if (accountId.IsNotNull())
            {
                query.Append(" AND c.accountid = @accountid ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", journeyId},
                { "@accountid", accountId},
            };
            var count = (await ReadDocumentsAsync<int>(query.ToString(), parameters)).Sum();
            return count;
        }

        public async Task<List<SMSLogDocumentModel>> GetSmsLogForUpdateToDEAsync(long accountId)
        {
            var query = @"SELECT TOP 500 * FROM c WHERE c.type = @type  
                             AND c.isupdatedtode = @isupdatedtode 
                             AND c.accountid = @accountid ";

            var parameters = new Dictionary<string, object>() {
                { "@type", (int)CosmosDocumentType.Log },
                { "@isupdatedtode", false },
                { "@accountid", accountId },
            };

            var result = await ReadDocumentsAsync<SMSLogDocumentModel>(query, parameters, 1000, true);

            return result;
        }

        public async Task<List<JourneyInfoDocumentModel>> GetJourneysAsync(List<string> journeyIds)
        {

            var ids = "( '" + string.Join("','", journeyIds) + "' )";

            var query = $@"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey AND c.id in {ids} ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Journey },
                { "@partitionkey", CosmosDocumentType.Journey.ToString() }
            };

            var result = await ReadDocumentsAsync<JourneyInfoDocumentModel>(query, parameters);
            return result;
        }

        public async Task<List<SMSLogDocumentModel>> GetSMSLogByDeliveryStatusAsync(long accountId, DateTime dateFrom, DeliveryStatus status)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type AND c.deliverystatus = @deliverystatus
                             AND c.accountid = @accountid AND c.senddate >= @senddate";


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@deliverystatus", status},
                { "@accountid", accountId},
                { "@senddate", dateFrom},
            };
            var result = await ReadDocumentsAsync<SMSLogDocumentModel>(query.ToString(), parameters, -1, true);
            return result;
        }

        public async Task<List<SMSLogDocumentModel>> GetSMSLogBySendStatusAsync(long accountId, DateTime dateFrom, SendStatus status)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type AND c.deliverystatus = @deliverystatus
                             AND c.accountid = @accountid AND c.senddate >= @senddate";


            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@sentstatus", status},
                { "@accountid", accountId},
                { "@senddate", dateFrom},
            };
            var result = await ReadDocumentsAsync<SMSLogDocumentModel>(query.ToString(), parameters, -1, true);
            return result;
        }

        public async Task<List<LogViewModel>> GetSMSLogAsync(LogFilterModel logFilter)
        {
            var query = new StringBuilder($@"SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");


            if (logFilter.DeliveryStatus.IsNotNull())
            {
                query.Append(" AND c.deliverystatus = @deliverystatus ");
            }
            if (logFilter.SendStatus.IsNotNull())
            {
                query.Append(" AND c.sentstatus = @sentstatus ");
            }
            if (logFilter.MobileNumber.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.mobilenumber = @mobilenumber ");
            }
            if (logFilter.InteractionId.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.interactionid = @interactionid ");
            }
            if (logFilter.ActivityId.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.activityid = @activityid ");
            }
            if (logFilter.SendDateFrom.IsNotNull())
            {
                logFilter.SendDateTo = logFilter.SendDateTo.Value.AddDays(1);
                query.Append(" AND c.senddate >= @senddatefrom AND c.senddate <= @senddateto ");
            }
            if (logFilter.DeliveryDateFrom.IsNotNull())
            {
                logFilter.DeliveryDateTo = logFilter.DeliveryDateTo.Value.AddDays(1);
                query.Append(" AND c.deliverydate >= @deliverydatefrom AND c.deliverydate <= @deliverydateto ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", logFilter.JourneyId },
                { "@accountid", logFilter.AccountId },
                { "@deliverystatus", logFilter.DeliveryStatus },
                { "@sentstatus", logFilter.SendStatus },
                { "@mobilenumber", logFilter.MobileNumber },
                { "@interactionid", logFilter.InteractionId },
                { "@activityid", logFilter.ActivityId },
                { "@senddatefrom", logFilter.SendDateFrom },
                { "@senddateto", logFilter.SendDateTo },
                { "@deliverydatefrom", logFilter.DeliveryDateFrom },
                { "@deliverydateto", logFilter.DeliveryDateTo },
            };

            var result = await ReadDocumentsAsync<SMSLogDocumentModel>(query.ToString(), parameters);

            var logModels = result.Select(k => new LogViewModel()
            {
                SMSContent = k.SMSContent,
                ActivityId = k.ActivityId,
                InteractionId = k.InteractionId,
                AccountId = k.AccountId,
                JourneyId = k.JourneyId,
                DeliveryStatus = k.DeliveryStatus,
                SentStatus = k.SentStatus,
                SendId = k.Id,
                SendDate = k.SendDate,
                DeliveryDate = k.DeliveryDate,
                ErrorMessage = k.ErrorMessage,
                DropErrorCode = k.DropErrorCode,
                MobileNumber = k.MobileNumber
            }).ToList();

            await SetVersionAndActivityName(logModels);

            return logModels;
        }

        public async Task<(List<LogViewModel>, int)> GetSMSLogWithPagingAsync(LogFilterModel logFilter)
        {
            var query = new StringBuilder($@"SELECT [PROJECTION] FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey 
                             AND c.accountid = @accountid ");


            if (logFilter.DeliveryStatus.IsNotNull())
            {
                query.Append(" AND c.deliverystatus = @deliverystatus ");
            }
            if (logFilter.SendStatus.IsNotNull())
            {
                query.Append(" AND c.sentstatus = @sentstatus ");
            }
            if (logFilter.MobileNumber.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.mobilenumber = @mobilenumber ");
            }
            if (logFilter.InteractionId.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.interactionid = @interactionid ");
            }
            if (logFilter.ActivityId.IsNotNullOrWhiteSpace())
            {
                query.Append(" AND c.activityid = @activityid ");
            }
            if (logFilter.SendDateFrom.IsNotNull())
            {
                logFilter.SendDateTo = logFilter.SendDateTo.Value.AddDays(1);
                query.Append(" AND c.senddate >= @senddatefrom AND c.senddate <= @senddateto ");
            }
            if (logFilter.DeliveryDateFrom.IsNotNull())
            {
                logFilter.DeliveryDateTo = logFilter.DeliveryDateTo.Value.AddDays(1);
                query.Append(" AND c.deliverydate >= @deliverydatefrom AND c.deliverydate <= @deliverydateto ");
            }

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@partitionkey", logFilter.JourneyId },
                { "@accountid", logFilter.AccountId },
                { "@deliverystatus", logFilter.DeliveryStatus },
                { "@sentstatus", logFilter.SendStatus },
                { "@mobilenumber", logFilter.MobileNumber },
                { "@interactionid", logFilter.InteractionId },
                { "@activityid", logFilter.ActivityId },
                { "@senddatefrom", logFilter.SendDateFrom },
                { "@senddateto", logFilter.SendDateTo },
                { "@deliverydatefrom", logFilter.DeliveryDateFrom },
                { "@deliverydateto", logFilter.DeliveryDateTo },
                { "@pageno", (logFilter.PageNo - 1) * logFilter.ItemsPerPage },
                { "@itemsperpage", logFilter.ItemsPerPage }
            };

            var count = (await ReadDocumentsWithScaleUpAsync<int>(query.ToString().Replace("[PROJECTION]", " VALUE COUNT(1) "), parameters)).Sum();

            query.Append(" ORDER BY c.senddate desc OFFSET @pageno LIMIT @itemsperpage ");

            var result = await ReadDocumentsWithScaleUpAsync<SMSLogDocumentModel>(query.ToString().Replace("[PROJECTION]", " * "), parameters);

            var logModels = result.Select(k => new LogViewModel()
            {
                SMSContent = k.SMSContent,
                ActivityId = k.ActivityId,
                InteractionId = k.InteractionId,
                AccountId = k.AccountId,
                JourneyId = k.JourneyId,
                DeliveryStatus = k.DeliveryStatus,
                SentStatus = k.SentStatus,
                SendId = k.Id,
                SendDate = k.SendDate,
                DeliveryDate = k.DeliveryDate,
                ErrorMessage = k.ErrorMessage,
                DropErrorCode = k.DropErrorCode,
                MobileNumber = k.MobileNumber
            }).ToList();

            await SetVersionAndActivityName(logModels);

            return (logModels, count);
        }

        private async Task SetVersionAndActivityName(List<LogViewModel> logModels)
        {
            var activities = logModels.Select(k => k.ActivityId).Distinct().ToList();
            List<ActivityInfoDocumentModel> activityInfos = new List<ActivityInfoDocumentModel>();
            foreach (var item in activities)
            {
                var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey and c.id = @id";
                var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Activity },
                { "@partitionkey", CosmosDocumentType.Activity.ToString() },
                { "@id", item }
                };

                var activity = await ReadDocumentAsync<ActivityInfoDocumentModel>(query, parameters);
                if(activity.IsNotNull())
                {
                    activityInfos.Add(activity);
                }                
            }

            logModels.ForEach(x => x.ActivityName = activityInfos.FirstOrDefault(y => y.Id == x.ActivityId)?.ActivityName);

            var interactions = logModels.Select(k => k.InteractionId).Distinct().ToList();
            List<InteractionInfoDocumentModel> interactionInfos = new List<InteractionInfoDocumentModel>();
            foreach (var item in interactions)
            {
                var query = "SELECT * FROM c WHERE c.type = @type AND c.partitionkey = @partitionkey and c.id = @id";
                var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Interaction },
                { "@partitionkey", CosmosDocumentType.Interaction.ToString() },
                { "@id", item }
            };

                var interaction = await ReadDocumentAsync<InteractionInfoDocumentModel>(query, parameters);
                interactionInfos.Add(interaction);
            }

            logModels.ForEach(x => x.Version = interactionInfos.Where(y => y.Id == x.InteractionId).Select(z => z.Version).FirstOrDefault());

        }

        public async Task CustomQueryExecutorAsync(long accountId)
        {
            Dictionary<string, int> list = new Dictionary<string, int>();
            var query = $@"SELECT * FROM c WHERE c.type = 0 and c.partitionkey = 'Journey' and c.accountid = {accountId} ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.IncomingMessage },
                { "@accountId", accountId }
            };

            var result = await ReadDocumentsAsync<JourneyInfoDocumentModel>(query, parameters);
            foreach (var item in result)
            {
                query = $@"SELECT Value count(1) FROM c WHERE c.type = 3 and c.partitionkey = '{item.Id}' ";
                var count = (await ReadDocumentsAsync<int>(query, parameters)).Sum();
                if(count == 0)
                {
                    list.Add(item.Id, count);
                }
            }
            var res = list.Select(x => x.Key).ToJsonString();
        }
        public async Task<bool> UpsertDocument(object document)
        {
            var response = await UpsertDocumentAsync(document);
            return response.Success;
        }

        public async Task<List<SMSLogDocumentModel>> GetPendingStatusSMSAsync(long accountId, DateTime fromDate)
        {
            var query = @"SELECT TOP 1000 * FROM c WHERE c.type = @type AND c.deliverystatus = 0 AND
                          c.accountid = @accountid AND c.senddate < @fromdate ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@accountid", accountId},
                { "@fromdate", fromDate},
            };

            var result = await ReadDocumentsAsync<SMSLogDocumentModel>(query, parameters, 1000, true);

            return result;
        }

        public async Task<List<string>> GetDistinctJourneyIdsAsync(DateTime dateFrom)
        {
            var query = @"SELECT Distinct value c.journeyid FROM c WHERE c.type = @type 
                             AND c.senddate >= @datefrom ";
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@datefrom", dateFrom },
            };

            var result = await ReadDocumentsAsync<string>(query, parameters, -1, true);
            return result;
        }

        public async Task<int> GetSMSUsedCountAsync(long accountId, DateTime startDate, DateTime endDate)
        {
            var query = $@"SELECT VALUE c.credit FROM c WHERE c.type = @type
                             AND c.senddate >= @startDate AND c.senddate <= @endDate
                             AND (c.deliverystatus = @delivered OR c.deliverystatus = @unconfirmed)
                             AND c.accountid = @accountid ";
            
            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },                
                { "@accountid", accountId },
                { "@delivered", DeliveryStatus.Delivered },
                { "@unconfirmed", DeliveryStatus.UnConfirmed },
                { "@startDate", startDate },
                { "@endDate", endDate }                
            };

            var count = (await ReadDocumentsAsync<int>(query, parameters, -1, true)).Sum();
            return count;
        }

        public async Task<List<SMSUsageDocumentModel>> GetSMSMonthlyUsageAsync(long accountId, int year)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type
                          AND c.partitionkey = @partitionkey 
                          AND c.accountid = @accountid AND c.year = @year ORDER BY c.month ASC ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Usage },
                { "@partitionkey", CosmosDocumentType.Usage.ToString() },
                { "@accountid", accountId},
                { "@year", year},
            };

            var result = await ReadDocumentsAsync<SMSUsageDocumentModel>(query, parameters);

            return result;
        }

        public async Task ProcessNullLogsAsync(SMSSFInteractionModel smsSFInteractionModel)
        {
            var query = @"SELECT * FROM c WHERE c.type = @type and c.partitionkey = @partitionkey and c.interactionid = @interactionid and c.accountid = @accountid ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Log },
                { "@interactionid", smsSFInteractionModel.Id },
                { "@partitionkey", null },
                { "@accountid", smsSFInteractionModel.AccountId },               
            };

            var result = await ReadDocumentsAsync<SMSLogDocumentModel>(query, parameters);          

            query = @"SELECT TOP 1 * from c where c.id = @interactionid and c.partitionkey = @partitionkey 
                      AND c.type = @type and c.accountid = @accountid ";
            parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Interaction },
                { "@interactionid", smsSFInteractionModel.Id },
                { "@partitionkey", CosmosDocumentType.Interaction.ToString() },
                { "@accountid", smsSFInteractionModel.AccountId },
            };

            var interactionInfo = await ReadDocumentAsync<InteractionInfoDocumentModel>(query, parameters);
            if(interactionInfo.IsNotNull() && interactionInfo.JourneyId.IsNotNullOrWhiteSpace())
            {
                foreach (var item in result)
                {
                    item.JourneyId = interactionInfo.JourneyId;
                    item.PartitionKey = interactionInfo.JourneyId;
                }

                await BulkInsertDocumentsAsync(result, true);
               
                foreach (var item in result)
                {
                    var res = await DeleteDocumentAsync(item.Id, null);
                }
            }           
        }

        public async Task<List<int>> GetSMSMonthlyUsageYearsAsync(long accountId)
        {
            var query = @"SELECT DISTINCT VALUE c.year FROM c WHERE c.type = @type
                          AND c.partitionkey = @partitionkey 
                          AND c.accountid = @accountid ORDER BY c.year ASC ";

            var parameters = new Dictionary<string, object>() {
                { "@type", CosmosDocumentType.Usage },
                { "@partitionkey", CosmosDocumentType.Usage.ToString() },
                { "@accountid", accountId}                
            };

            var result = await ReadDocumentsAsync<int>(query, parameters);

            return result;
        }
    }
}
