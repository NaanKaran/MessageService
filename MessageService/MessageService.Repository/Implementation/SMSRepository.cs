using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.DataExtensionModel;
using MessageService.Models.Enum;
using MessageService.Models.SubmailModel;
using MessageService.Models.ExportModels;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;
using MessageService.Models.StoredProcedureModels;

namespace MessageService.Repository.Implementation
{
    public class SMSRepository : BaseRepository, ISMSRepository
    {
        private readonly string _weChatifyConnectionString;
        private readonly string _messageServiceConnectionString;
        private readonly string _currentQuadrantSMSLogTableName;
        public SMSRepository(string wechatify, string messageService)
        {
            _weChatifyConnectionString = wechatify;
            _messageServiceConnectionString = messageService;
            _currentQuadrantSMSLogTableName = DateTime.UtcNow.ToChinaTime().GetQuadrantSMSLogTableName();
        }
   
        public async Task<bool> SaveIncomingMessageAsync(IncomingMessageProcModel incomingMessage)
        {
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(incomingMessage);

            var result = await ExecuteProcAsync(StoredProcedure.SMSIncomingMessageInsert, parameters);

            return result != 0;
        }

        public async Task<string> GetJourneyNameByMobileNumberAsync(string mobileNumber)
        {

            var sql = $@" 
                        SELECT TOP 1 J.JourneyName FROM 
                                [dbo].[{_currentQuadrantSMSLogTableName}] L
                        JOIN
                                [dbo].[SMSJourneyInfo]  J ON (J.JourneyId = L.JourneyId)
                        WHERE 
                        L.MobileNumber = @mobileNumber
                        ORDER BY L.SendDate Desc
                        ";
            var journeyName = await QueryFirstOrDefaultAsync<string>(sql, new { @mobileNumber = mobileNumber });

            return journeyName;
        }

        public async Task<(IEnumerable<IncomingMessageModel>, int)> GetIncomingMessagesAsync(GetIncomingMessagesModel model)
        {
            var sql = @"SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[SMSIncomingMessages] WHERE [AccountId] = @AccountId ";

            if (model.IsOptOut)
            {
                sql += @" AND [IsOptOut] = @IsOptOut ";
            }

            if (model.MobileNumber.IsNotNullOrWhiteSpace())
            {
                sql += @" AND [MobileNumber] LIKE CONCAT('%',@MobileNumber,'%') ";
            }

            if (model.Content.IsNotNullOrWhiteSpace())
            {
                sql += @" AND [Content] LIKE CONCAT('%',@Content,'%') ";
            }

            if (model.JourneyName.IsNotNullOrWhiteSpace())
            {
                sql += @" AND [JourneyName] LIKE CONCAT('%',@JourneyName,'%') ";
            }

            if (model.FromDate.IsNotNull() && model.ToDate.IsNotNull())
            {
                sql += @" AND CreatedOn Between @FromDate AND @ToDate ";
            }

            sql += $@" ORDER BY CreatedOn {model.Sort}
                    OFFSET ((@pageno - 1) * @itemsperpage) ROWS
                    FETCH NEXT @itemsperpage ROWS ONLY ";
            var result = await QueryAsPagingAsync<IncomingMessageModel>(sql, model);
            return result;
        }

        public async Task<IEnumerable<IncomingMessageModel>> GetIncomingMessagesAsync(IncomingMessagesExportModel model, bool isOptOut)
        {
            var sql = @"SELECT * FROM dbo.[SMSIncomingMessages] WHERE [AccountId] = @AccountId AND (CreatedOn Between @fromDate AND @toDate) ";

            if (isOptOut)
            {
                sql += @" AND [IsOptOut] = @isOptOut ";
            }

            if (model.MobileNumber.IsNotNullOrWhiteSpace())
            {
                sql += @" AND MobileNumber LIKE CONCAT('%',@mobileNumber,'%') ";
            }
            if (model.Content.IsNotNullOrWhiteSpace())
            {
                sql += @" AND Content LIKE CONCAT('%',@content,'%') ";
            }
            if (model.JourneyName.IsNotNullOrWhiteSpace())
            {
                sql += @" AND JourneyName LIKE CONCAT('%',@journeyName,'%') ";
            }

            var result = await QueryAsync<IncomingMessageModel>(sql, 
                new {
                        @AccountId = model.AccountId, @fromDate = model.FromDate,
                        @toDate = model.ToDate, @mobileNumber = model.MobileNumber,
                        @content = model.Content, @journeyName = model.JourneyName,
                        isOptOut
                    });
            return result;
        }

        public async Task<List<IncomingMessageModel>> GetIncomingMessagesToUpdateDEAsync(long accountId)
        {
            var sql = @"SELECT * FROM dbo.[SMSIncomingMessages] WHERE [AccountId] = @accountId AND [IsUpdatedIntoDE] = 0 ";

            var result = await QueryAsync<IncomingMessageModel>(sql, new { accountId });

            return result.ToList();
        }

        public async Task<bool> UpdateDataExtensionPushInIncomingMessageAsync(IList<WeChatifyDataExtensionIncomingLog> logModels)
        {
            var sql = $@"UPDATE dbo.[SMSIncomingMessages] SET IsUpdatedIntoDE = 1 WHERE Id = @Id";

            var result = await ExecuteAsync(sql, logModels).ConfigureAwait(false);

            return result != 0;
        }

        public async Task<bool> SaveJourneyAsync(JourneyProcModel journeyProcModel)
        {    
            var parameters = new DynamicParameters();            

            parameters.AddDynamicParams(journeyProcModel);

            var result = await ExecuteProcAsync(StoredProcedure.SMSJourneyUpsert, parameters);

            return result != 0;

        }

        public async Task<IEnumerable<VersionDropDownModel>> GetVersionsAsync(string journeyId, string quadrant)
        {
            var sql = @" 
                        SELECT InteractionId , Version 
                        FROM 
                        [dbo].[SMSInteractionInfo]  
                        WHERE 
                        JourneyId = @journeyId
                        AND
                        QuadrantInfo = @quadrant
                        ORDER BY Version Asc
                        ";
            var versionDropDownModels = await QueryAsync<VersionDropDownModel>(sql, new { journeyId, quadrant }).ConfigureAwait(false);

            return versionDropDownModels;
        }

        public async Task<IEnumerable<ActivityDropDownModel>> GetActivitiesAsync(string journeyId, string quadrant)
        {
            var sql = @" 
                        SELECT ActivityId , ActivityName 
                        FROM 
                        [dbo].[SMSActivityInfo]  
                        WHERE 
                        JourneyId = @journeyId
                        AND
                        QuadrantInfo = @quadrant
                        ORDER BY ActivityName Asc
                        ";
            var activityDropDownModels = await QueryAsync<ActivityDropDownModel>(sql, new { journeyId, quadrant }).ConfigureAwait(false);

            return activityDropDownModels;
        }

        public async Task<IEnumerable<QuadrantDropDownModel>> GetQuadrantsAsync()
        {
            var year = DateTime.UtcNow.ToChinaTime().Year;
            var quadrant = DateTime.UtcNow.ToChinaTime().GetQuadrantNumberInfo();

            var sql = @" 
                        SELECT TableName , Description 
                        FROM 
                        [dbo].[SMSLogTableInfo]  
                        WHERE 
                        Year <= @year
                        AND
                        QuadrantNumber <= @quadrant
                        ORDER BY Year desc, QuadrantNumber desc
                        ";
            var quadrantDropDownModels = await QueryAsync<QuadrantDropDownModel>(sql, new { year, quadrant }).ConfigureAwait(false);

            return quadrantDropDownModels;
        }

        public async Task<IEnumerable<LogViewModel>> GetSMSLogAsync(LogFilterModel logFilter)
        {
            var sql = $@" 
                            SELECT
                                J.JourneyName,
                                I.Version,
                                A.ActivityName,
                                L.SendId,
                                L.MobileNumber,
                                --L.MMSTemplateId,
                                --T.TemplateName as MMSTemplateName,
                                --L.DynamicParamsValue,
                                L.SentStatus,
                                L.DeliveryStatus,
                                L.SendDate,
                                L.DeliveryDate,
                                EC.EnglishDescription as ErrorMessage,
                                L.DropErrorCode,
                                TotalCount = COUNT(1) OVER()
                            FROM 
                                [dbo].[{logFilter.QuadrantTableName}] L
                            JOIN
                                [dbo].[SMSJourneyInfo] J ON (J.JourneyId = L.JourneyId AND J.AccountId =@accountId AND J.JourneyId = @journeyId )
                            JOIN
                                [dbo].[SMSInteractionInfo] I ON (I.InteractionId = L.InteractionId AND I.AccountId =@accountId AND I.JourneyId = @journeyId)
                            JOIN
                                [dbo].[SMSActivityInfo] A ON (A.ActivityId = L.ActivityId AND A.AccountId = @accountId)
                            JOIN
                                [dbo].[SMSContent] SC ON (SC.SendId = L.SendId AND L.AccountId = @accountId)
                            LEFT JOIN 
								[dbo].SMSErrorCode EC ON (EC.ErrorCode = L.DropErrorCode)
                            WHERE
                            L.AccountId= @accountId
                            AND
                            L.JourneyId = @journeyId ";
            if (logFilter.DeliveryStatus.IsNotNull())
            {
                sql += @" AND L.DeliveryStatus =@DeliveryStatus ";
            }
            if (logFilter.SendStatus.IsNotNull())
            {
                sql += @" AND L.SentStatus =@SendStatus ";
            }
            if (logFilter.MobileNumber.IsNotNullOrWhiteSpace())
            {
                sql += @" AND L.MobileNumber =@MobileNumber ";
            }
            if (logFilter.InteractionId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND L.InteractionId =@InteractionId ";
            }
            if (logFilter.ActivityId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND L.ActivityId =@ActivityId ";
            }
            if (logFilter.SendDateFrom.IsNotNull() && logFilter.SendDateTo.IsNotNull())
            {
                sql += @" AND L.SendDate Between @SendDateFrom And @SendDateTo ";
            }
            if (logFilter.DeliveryDateFrom.IsNotNull() && logFilter.DeliveryDateTo.IsNotNull())
            {
                sql += @" AND L.DeliveryDate Between @DeliveryDateFrom And @DeliveryDateTo ";
            }

            var logViewModels = await QueryAsync<LogViewModel>(sql, logFilter).ConfigureAwait(false);

            return logViewModels;
        }

        public async Task<string> GetJourneyIdAsync(string interactionId)
        {

            var sql = $@"SELECT Top 1 JourneyId FROM [dbo].SMSInteractionInfo WHERE InteractionId = @interactionId";
            var journeyId = await QueryFirstOrDefaultAsync<string>(sql, new { @interactionId = interactionId });

            return journeyId;
        }
        public async Task<bool> SaveSMSLogAsync(SMSLogInsertProcModel smsLog, string storedProcedure)
        {
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(smsLog);

            var result = await ExecuteProcAsync(storedProcedure, parameters);

            return result != 0;
        }

        public async Task<IEnumerable<JourneyInfoModel>> GetAllJourneysAsync(JourneyExportModel model)
        {
            var sql = @"SELECT * From [dbo].[SMSJourneyInfo]  
                        WHERE 
                        AccountId = @accountId 
                        ORDER BY [lasttriggeredon] Desc";
            if (model.JourneyId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND JourneyId = @journeyId ";
            }

            return await QueryAsync<JourneyInfoModel>(sql, model);
        }

        public async Task<bool> UpdateCountInJourneyAsync(SMSLogModel logModel)
        {
            string sql;
            if (logModel.SentStatus == SendStatus.Success)
            {
                sql = @"
            UPDATE [dbo].[SMSJourneyInfo] SET TotalCount = TotalCount + 1 WHERE JourneyId =@JourneyId;
            UPDATE [dbo].[SMSInteractionInfo] SET TotalCount = TotalCount + 1 WHERE InteractionId =@InteractionId;
            UPDATE [dbo].[SMSActivityInfo] SET TotalCount = TotalCount + 1 WHERE ActivityId =@ActivityId;";
            }
            else
            {
                sql = @"
            UPDATE [dbo].[SMSJourneyInfo] SET TotalCount = TotalCount + 1 , SendFailedCount = SendFailedCount + 1 WHERE JourneyId =@journeyId;
            UPDATE [dbo].[SMSInteractionInfo] SET TotalCount = TotalCount + 1 , SendFailedCount = SendFailedCount + 1 WHERE InteractionId =@interactionId;
            UPDATE [dbo].[SMSActivityInfo] SET TotalCount = TotalCount + 1 , SendFailedCount = SendFailedCount + 1 WHERE ActivityId =@activityId;";
            }


            using (var con = new SqlConnection(_messageServiceConnectionString))
            {
                await con.OpenAsync();
                using (var tran = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var result = await con.ExecuteAsync(sql, new { logModel.JourneyId, logModel.InteractionId, logModel.ActivityId }, transaction: tran);
                    tran.Commit();
                    return result != 0;
                }
            }
        }

        public async Task CreateQuadrantObjectsAsync(string sql)
        {
            await ExecuteAsync(sql);
        }

        public async Task<bool> UpdateDeliveryReportInSMSLogAsync(SMSLogUpdateProcModel model)
        {
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(model);

            var result = await ExecuteProcAsync("sp_" + DateTime.UtcNow.ToChinaTime().GetQuadrantSMSLogTableName() +"_Update", parameters);

            return result != 0;
        }

        public async Task<bool> UpdateDeliveryCountInJourneyAsync(SubmailStatusPushModel report)
        {
            var logSql = $@"SELECT Top 1 * FROM [dbo].[{_currentQuadrantSMSLogTableName}] WHERE SendId = @sendId";

            var smsLogData = await QueryFirstOrDefaultAsync<SMSLogModel>(logSql, new { @sendId = report.SendId }).ConfigureAwait(false);
            if (smsLogData.IsNull()) { return false; }

            var sql = "";
            if (report.DeliveryStatus == DeliveryStatus.Delivered)
            {
                sql = @"
            UPDATE [dbo].[SMSJourneyInfo] SET DeliveredCount = DeliveredCount + 1 WHERE JourneyId = @JourneyId;
            UPDATE [dbo].[SMSInteractionInfo] SET DeliveredCount = DeliveredCount + 1 WHERE InteractionId = @InteractionId;
            UPDATE [dbo].[SMSActivityInfo] SET DeliveredCount = DeliveredCount + 1 WHERE ActivityId = @ActivityId;";
            }
            else if (report.DeliveryStatus == DeliveryStatus.Dropped)
            {
                sql = @"
            UPDATE [dbo].[SMSJourneyInfo] SET DroppedCount = DroppedCount + 1 WHERE JourneyId = @JourneyId;
            UPDATE [dbo].[SMSInteractionInfo] SET DroppedCount = DroppedCount + 1 WHERE InteractionId = @InteractionId;
            UPDATE [dbo].[SMSActivityInfo] SET DroppedCount = DroppedCount + 1 WHERE ActivityId = @ActivityId;";
            }
            else
            {
                return false;
            }

            using (var con = new SqlConnection(_messageServiceConnectionString))
            {
                await con.OpenAsync();
                using (var tran = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    await con.ExecuteAsync(sql, new { smsLogData.JourneyId, smsLogData.InteractionId, smsLogData.ActivityId }, transaction: tran);
                    tran.Commit();
                }
            }
            return true;
        }

        public async Task<(IEnumerable<JourneyInfoModel>, int)> GetJourneysAsync(JourneyFilterModel logFilterModel)
        {
            var sql = @" 
                        SELECT *, TotalCount = COUNT(1) OVER() From [dbo].[SMSJourneyInfo]  
                        WHERE 
                        AccountId = @accountId ";
            if (logFilterModel.JourneyId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND JourneyId = @journeyId ";
            }

            sql += @" ORDER BY lasttriggeredon DESC
                        OFFSET ((@pageno - 1) * @itemsperpage) ROWS
                        FETCH NEXT @itemsperpage ROWS ONLY 
                        ";
            var smsJourneyInfoModel = await QueryAsPagingAsync<JourneyInfoModel>(sql, logFilterModel);

            return smsJourneyInfoModel;
        }

        public async Task<IEnumerable<JourneyInfoModel>> GetAllJourneyAsync(long accountId)
        {
            var sql = @"SELECT * From [dbo].[SMSJourneyInfo]  
                        WHERE 
                        AccountId = @accountId
                        ORDER BY [LastTriggeredOn] Desc
                        ";
            return await QueryAsync<JourneyInfoModel>(sql, new { accountId });
        }

        public async Task<(IEnumerable<LogViewModel>, int)> GetSMSLogForCurrentQuadrantAsync(long accountId, string journeyId)
        {
            var sql = $@" 
                            SELECT 
                                I.Version,
                                A.ActivityName,
                                L.SendId,
                                L.MobileNumber,
                                --L.SMSTemplateId,
                                --L.DynamicParamsValue,
                                L.SentStatus,
                                L.DeliveryStatus,
                                L.SendDate,
                                L.DeliveryDate,
                                L.ErrorMessage,
                                L.DropErrorCode,
                                TotalCount = COUNT(1) OVER()
                            FROM 
                                [dbo].[{_currentQuadrantSMSLogTableName}] L
                            JOIN
                                [dbo].[SMSInteractionInfo] I ON (I.InteractionId = L.InteractionId AND I.AccountId = @accountId)
                            JOIN
                                [dbo].[SMSActivityInfo] A ON (A.ActivityId = L.ActivityId AND A.AccountId = @accountId)
                            WHERE
                            L.AccountId = @accountId
                            AND
                            L.JourneyId = @journeyId
                    ORDER BY L.SendDate DESC
                    OFFSET ((@pageno - 1) * @itemsperpage) ROWS
                    FETCH NEXT @itemsperpage ROWS ONLY
                        ";
            var logViewModels = await QueryAsPagingAsync<LogViewModel>(sql, new { accountId, journeyId, @pageno = 1, @itemsperpage = 10 }).ConfigureAwait(false);

            return logViewModels;
        }
        public async Task<bool> AddSMSLogTableInfo(SMSLogTableInfoModel logTableInfoModel)
        {
            var sql = @"INSERT INTO [dbo].[SMSLogTableInfo]
                                       ([TableName]
                                       ,[Year]
                                       ,[QuadrantNumber]
                                       ,[Description]
                                       ,[CreatedOn])
                                 VALUES
                                       (@TableName
                                       ,@Year
                                       ,@QuadrantNumber
                                       ,@Description
                                       ,@CreatedOn)";

            var result = await ExecuteAsync(sql, logTableInfoModel).ConfigureAwait(false);

            return result != 0;
        }

        public async Task<SMSContentModel> GetSMSContentAsync(string sendId)
        {
            var sql = @"SELECT * From [dbo].[SMSContent]  
                        WHERE 
                        SendId = @sendId
                        ";
            return await QueryFirstOrDefaultAsync<SMSContentModel>(sql, new { sendId });
        }

        public async Task<(IEnumerable<LogViewModel>, int)> GetSMSLogByQuadrantAsync(LogFilterModel logFilter)
        {
            var sql = $@" 
                            SELECT 
                                I.Version,
                                A.ActivityName,
                                L.SendId,
                                L.MobileNumber,
                                --L.MMSTemplateId,
                                --L.DynamicParamsValue,
                                L.SentStatus,
                                L.DeliveryStatus,
                                L.SendDate,
                                L.DeliveryDate,
                                L.ErrorMessage,
                                L.DropErrorCode,
                                TotalCount = COUNT(1) OVER()
                            FROM 
                                [dbo].[{logFilter.QuadrantTableName}] L
                            JOIN
                                [dbo].[SMSInteractionInfo] I ON (I.InteractionId = L.InteractionId AND I.AccountId =@accountId)
                            JOIN
                                [dbo].[SMSActivityInfo] A ON (A.ActivityId = L.ActivityId AND A.AccountId =@accountId)
                            WHERE
                            L.AccountId= @accountId
                            AND
                            L.JourneyId = @journeyId ";
            if (logFilter.DeliveryStatus.IsNotNull())
            {
                sql += @" AND L.DeliveryStatus =@DeliveryStatus ";
            }
            if (logFilter.SendStatus.IsNotNull())
            {
                sql += @" AND L.SentStatus =@SendStatus ";
            }
            if (logFilter.MobileNumber.IsNotNullOrWhiteSpace())
            {
                sql += @" AND L.MobileNumber =@MobileNumber ";
            }
            if (logFilter.InteractionId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND L.InteractionId =@InteractionId ";
            }
            if (logFilter.ActivityId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND L.ActivityId =@ActivityId ";
            }
            if (logFilter.SendDateFrom.IsNotNull() && logFilter.SendDateTo.IsNotNull())
            {
                sql += @" AND L.SendDate Between @SendDateFrom And @SendDateTo ";
            }
            if (logFilter.DeliveryDateFrom.IsNotNull() && logFilter.DeliveryDateTo.IsNotNull())
            {
                sql += @" AND L.DeliveryDate Between @DeliveryDateFrom And @DeliveryDateTo ";
            }

            sql += $@" ORDER BY L.SendDate DESC
                    OFFSET ((@pageno - 1) * @itemsperpage) ROWS
                    FETCH NEXT @itemsperpage ROWS ONLY ";
            var logViewModels = await QueryAsPagingAsync<LogViewModel>(sql, logFilter).ConfigureAwait(false);

            return logViewModels;
        }
    }
}
