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
using MessageService.Models.ExportModels;
using MessageService.Models.MMSModels;
using MessageService.Models.StoredProcedureModels;
using MessageService.Models.SubmailModel;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;

namespace MessageService.Repository.Implementation
{
    public class MMSRepository:BaseRepository, IMMSRepository
    {
        private readonly string _weChatifyConnectionString;
        private readonly string _mmsConnectionString;
        private readonly string _currentQuadrantMmsLogTableName;
        public MMSRepository(string wechatify, string messageService)
        {
            _weChatifyConnectionString = wechatify;
            _mmsConnectionString = messageService;
            _currentQuadrantMmsLogTableName = DateTime.UtcNow.ToChinaTime().GetQuadrantMMSLogTableName();
        }

        public async Task<bool> SaveMmsLogAsync(MMSLogModel mmsLog)
        {

            var inputData = new
            {
                mmsLog.AccountId,
                mmsLog.SendId,
                mmsLog.MobileNumber,
                mmsLog.ErrorMessage,
                mmsLog.DeliveryStatus,
                mmsLog.ActivityId,
                mmsLog.DynamicParamsValue,
                mmsLog.InteractionId,
                mmsLog.JourneyId,
                mmsLog.MMSTemplateId,
                mmsLog.DropErrorCode,
                mmsLog.SentStatus,
                mmsLog.SendDate,
                mmsLog.ActivityInteractionId
            };
            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(inputData);

            var procName = "sp_" + DateTime.UtcNow.ToChinaTime().GetQuadrantMMSLogTableName() + "_Insert";
            var result = await ExecuteProcAsync(procName, dynamicParams);

            return result != 0;
        }

        public async Task<bool> UpdateDeliveryReportInMMSLogAsync(MMSLogModel mmsLog)
        {

            var inputData = new
            {
                mmsLog.SendId,
                mmsLog.ErrorMessage,
                mmsLog.DeliveryStatus,
                mmsLog.DropErrorCode,
                mmsLog.DeliveryDate
            };
            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(inputData);

            var procName = "sp_" + DateTime.UtcNow.ToChinaTime().GetQuadrantMMSLogTableName() + "_Update";
            var result = await ExecuteProcAsync(procName, dynamicParams);

            return result != 0;
        }

        public async Task<bool> UpdateDeliveryReportInMMSLogAsync(List<MMSLogModel> mmsLogs)
        {

            var inputData = mmsLogs.Select(k=> new
            {
                k.SendId,
                k.ErrorMessage,
                k.DeliveryStatus,
                k.DropErrorCode,
                k.DeliveryDate
            });
            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(inputData);

            var procName = "sp_" + DateTime.UtcNow.ToChinaTime().GetQuadrantMMSLogTableName() + "_Update";
            var result = await ExecuteProcAsync(procName, dynamicParams);

            return result != 0;
        }

        public async Task<bool> UpdateCountInJourneyAsync(MMSLogModel mmsLogModel)
        {
            string sql;
            if(mmsLogModel.SentStatus == SendStatus.Success)
            { sql = @"
            UPDATE [dbo].[MMSJourneyInfo] SET TotalCount = TotalCount + 1 WHERE JourneyId =@JourneyId;
            UPDATE [dbo].[MMSInteractionInfo] SET TotalCount = TotalCount + 1 WHERE InteractionId =@InteractionId;
            UPDATE [dbo].[MMSActivityInfo] SET TotalCount = TotalCount + 1 WHERE ActivityId =@ActivityId;";
            }
            else
            {
                sql = @"
            UPDATE [dbo].[MMSJourneyInfo] SET TotalCount = TotalCount + 1 , SendFailedCount = SendFailedCount + 1 WHERE JourneyId =@journeyId;
            UPDATE [dbo].[MMSInteractionInfo] SET TotalCount = TotalCount + 1 , SendFailedCount = SendFailedCount + 1 WHERE InteractionId =@interactionId;
            UPDATE [dbo].[MMSActivityInfo] SET TotalCount = TotalCount + 1 , SendFailedCount = SendFailedCount + 1 WHERE ActivityId =@activityId;";
            }


            using (var con = new SqlConnection(_mmsConnectionString))
            {
                await con.OpenAsync();
                using(var tran = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                   var result = await con.ExecuteAsync(sql, new { mmsLogModel.JourneyId, mmsLogModel.InteractionId, mmsLogModel.ActivityId }, transaction: tran);
                    tran.Commit();
                    return result != 0;
                }
            }
            
        }

        public async Task<bool> UpdateDeliveryCountInJourneyAsync(SubmailStatusPushModel report)
        {
            var logSql = $@"SELECT Top 1 * FROM [dbo].[{_currentQuadrantMmsLogTableName}] WHERE SendId = @sendId";

            var mmsLogData = await QueryFirstOrDefaultAsync<MMSLogModel>(logSql, new { @sendId = report.SendId }).ConfigureAwait(false);
            if (mmsLogData.IsNull()) { return false; }

            var sql = "";
            if (report.DeliveryStatus == DeliveryStatus.Delivered)
            {
                sql = @"
            UPDATE [dbo].[MMSJourneyInfo] SET DeliveredCount = DeliveredCount + 1 WHERE JourneyId =@JourneyId;
            UPDATE [dbo].[MMSInteractionInfo] SET DeliveredCount = DeliveredCount + 1 WHERE InteractionId =@InteractionId;
            UPDATE [dbo].[MMSActivityInfo] SET DeliveredCount = DeliveredCount + 1 WHERE ActivityId =@ActivityId;";
            }
            else if(report.DeliveryStatus == DeliveryStatus.Dropped)
            {
                sql = @"
            UPDATE [dbo].[MMSJourneyInfo] SET DroppedCount = DroppedCount + 1 WHERE JourneyId =@JourneyId;
            UPDATE [dbo].[MMSInteractionInfo] SET DroppedCount = DroppedCount + 1 WHERE InteractionId =@InteractionId;
            UPDATE [dbo].[MMSActivityInfo] SET DroppedCount = DroppedCount + 1 WHERE ActivityId =@ActivityId;";
            }
            else
            {
                return false;
            }

            using (var con = new SqlConnection(_mmsConnectionString))
            {
                await con.OpenAsync();
                using (var tran = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    await con.ExecuteAsync(sql, new { mmsLogData.JourneyId, mmsLogData.InteractionId, mmsLogData.ActivityId }, transaction: tran);
                    tran.Commit();
                }
            }
            return true;
        }    

        public async Task<bool> SaveIncomingMessageAsync(IncomingMessageModel incomingMessage)
        {
            var sql = @"INSERT INTO [dbo].[MMSIncomingMessages]
                                       ([Id]
                                       ,[AccountId]
                                       ,[MobileNumber]
                                       ,[IsOptOut]
                                       ,[Content]
                                       ,[JourneyName]
                                       ,[CreatedOn])
                                 VALUES
                                       (@Id
                                       ,@AccountId
                                       ,@MobileNumber
                                       ,@IsOptOut
                                       ,@Content
                                       ,@JourneyName
                                       ,@CreatedOn)";
            var result = await ExecuteAsync(sql, incomingMessage);

            return result != 0;
        }

        public async Task<(IEnumerable<IncomingMessageModel>, int)> GetIncomingMessagesAsync(GetIncomingMessagesModel model)
        {
            var sql = @"SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[MMSIncomingMessages] WHERE [AccountId] = @AccountId ";   
            
            if(model.IsOptOut)
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
            var sql = @"SELECT * FROM dbo.[MMSIncomingMessages] WHERE [AccountId] = @AccountId AND (CreatedOn Between @fromDate AND @toDate) ";

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

            var result = await QueryAsync<IncomingMessageModel>(sql, new { @AccountId = model.AccountId, @fromDate = model.FromDate, @toDate = model.ToDate, @mobileNumber = model.MobileNumber, @content = model.Content, @journeyName = model.JourneyName, isOptOut });
            return result;
        }

        public async Task<string> GetJourneyNameByMobileNumberAsync(string mobileNumber)
        {
          
            var sql = $@" 
                        SELECT TOP 1 J.JourneyName FROM 
                                [dbo].[{_currentQuadrantMmsLogTableName}] L
                        JOIN
                                [dbo].[MMSJourneyInfo]  J ON (J.JourneyId = L.JourneyId)
                        WHERE 
                        L.MobileNumber = @mobileNumber
                        ORDER BY L.SendDate Desc
                        ";
            var journeyName = await QueryFirstOrDefaultAsync<string>(sql, new { @mobileNumber = mobileNumber });

            return journeyName;
        }

        public async Task<string> GetJourneyIdAsync(string interactionId)
        {

            var sql = $@"SELECT Top 1 JourneyId FROM [dbo].MMSInteractionInfo WHERE InteractionId = @interactionId";
            var journeyId = await QueryFirstOrDefaultAsync<string>(sql, new { @interactionId = interactionId });

            return journeyId;
        }

        public async Task<bool> AddJourneyEntryAsync(JourneyActivateModel journey)
        {
            var quadrantInfo = DateTime.UtcNow.ToChinaTime().GetQuadrantInfo();
            var createdOn = DateTime.UtcNow.ToChinaTime();
            var sql = @"
            IF NOT EXISTS(SELECT 1 FROM [dbo].[MMSJourneyInfo] WHERE JourneyId = @JourneyId)
            BEGIN
                        INSERT INTO [dbo].[MMSJourneyInfo] (JourneyId,lasttriggeredon) VALUES (@JourneyId,@createdOn);
            END
            INSERT INTO [dbo].[MMSInteractionInfo] (InteractionId,JourneyId, QuadrantInfo, CreatedOn) VALUES (@InteractionId,@JourneyId,@quadrantInfo,@createdOn);
            INSERT INTO [dbo].[MMSActivityInfo] (ActivityId,InteractionId,JourneyId, QuadrantInfo, CreatedOn) VALUES (@ActivityId,@InteractionId,@JourneyId,@quadrantInfo,@createdOn);";
          
            using (var con = new SqlConnection(_mmsConnectionString))
            {
                await con.OpenAsync();
                using (var tran = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    await con.ExecuteAsync(sql, new { journey.JourneyId, journey.InteractionId, journey.ActivityId, @quadrantInfo = quadrantInfo,@createdOn = createdOn },transaction: tran);
                    tran.Commit();
                }
            }
            return true;
        }

        public async Task<IEnumerable<JourneyInfoModel>> GetAllJourneyAsync(long accountId)
        {
            var sql = @" 
                        SELECT * From [dbo].[MMSJourneyInfo]  
                        WHERE 
                        AccountId = @accountId
                        ORDER BY [lasttriggeredon] Desc
                        ";
            var mmsJourneyInfoModel = await QueryAsync<JourneyInfoModel>(sql, new { accountId });

            return mmsJourneyInfoModel;
        }

        public async Task<JourneysPieChart> GetJourneysMMSCountAsync(JourneyFilterModel logFilterModel)
        {
            var sql = @"SELECT 
                                Sum(TotalCount) TotalCount,  
                                Sum(DeliveredCount) DeliveredCount, 
                                Sum(DroppedCount) DroppedCount, 
                                Sum(SendFailedCount) SendFailedCount 
                            FROM [dbo].[MMSJourneyInfo] 
                            WHERE AccountId = @accountId ";
            if (logFilterModel.JourneyId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND JourneyId = @journeyId ";
            }

            var journeyCount = await QueryFirstOrDefaultAsync<JourneysPieChart>(sql, logFilterModel);

            return journeyCount;
        }

        public async Task<(IEnumerable<JourneyInfoModel>,int)> GetJourneysAsync(JourneyFilterModel logFilterModel)
        {
            var sql = @" 
                        SELECT *, TotalCount = COUNT(1) OVER() From [dbo].[MMSJourneyInfo]  
                        WHERE 
                        AccountId = @accountId ";
                        if (logFilterModel.JourneyId.IsNotNullOrWhiteSpace())
                        {
                            sql += @" AND JourneyId = @journeyId " ;
                        }

                        sql += @" ORDER BY lasttriggeredon DESC
                        OFFSET ((@pageno - 1) * @itemsperpage) ROWS
                        FETCH NEXT @itemsperpage ROWS ONLY 
                        ";
            var mmsJourneyInfoModel = await QueryAsPagingAsync<JourneyInfoModel>(sql, logFilterModel);

            return mmsJourneyInfoModel;
        }

        public async Task<IEnumerable<JourneyInfoModel>> GetAllJourneysAsync(JourneyExportModel model)
        {
            var sql = @"SELECT * From [dbo].[MMSJourneyInfo]  
                        WHERE 
                        AccountId = @accountId ";
            if (model.JourneyId.IsNotNullOrWhiteSpace())
            {
                sql += @" AND JourneyId = @journeyId ";
            }

            sql += " ORDER BY lasttriggeredon Desc";
            return await QueryAsync<JourneyInfoModel>(sql, model);
        }      

        public async Task<IEnumerable<VersionDropDownModel>> GetVersionsAsync(string journeyId, string quadrant)
        {
            var sql = @" 
                        SELECT InteractionId , Version 
                        FROM 
                        [dbo].[MMSInteractionInfo]  
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
                        [dbo].[MMSActivityInfo]  
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
                        [dbo].[MMSLogTableInfo]  
                        WHERE 
                        Year <= @year
                        AND
                        QuadrantNumber <= @quadrant
                        ORDER BY Year desc, QuadrantNumber desc
                        ";
            var quadrantDropDownModels = await QueryAsync<QuadrantDropDownModel>(sql,new{year, quadrant }).ConfigureAwait(false);

            return quadrantDropDownModels;
        }

        public async Task<(IEnumerable<LogViewModel>,int)> GetMmsLogForCurrentQuadrantAsync(long accountId,string journeyId, string quadrantTableName)
        {

            var inputData = new
            {
                AccountId= accountId,
                ActivityId=  (string)null ,
                DeliveryDateFrom= (DateTime?)null,
                DeliveryDateTo=  (DateTime?)null ,
                DeliveryStatus=  (short?)null ,
                InteractionId= (string)null,
                ItemsPerPage=   10,
                JourneyId= journeyId,
                MobileNumber= (string)null,
                PageNo=   1,
                QuadrantTableName= quadrantTableName,
                SendDateFrom= (DateTime?)null,
                SendDateTo= (DateTime?)null,
                SendStatus = (short?)null
            };
            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(inputData);

            var logViewModels = await QueryProcAsPagingListAsync<LogViewModel>(StoredProcedure.GetMMSLog, dynamicParams);

            return logViewModels;
        }

        public async Task<(IEnumerable<LogViewModel>, int)> GetMmsLogByQuadrantAsync(LogFilterModel logFilter)
        {
            var inputData = new
            {
                logFilter.AccountId,
                logFilter.ActivityId,
                logFilter.DeliveryDateFrom,
                DeliveryDateTo = logFilter.DeliveryDateTo != null ? logFilter.DeliveryDateTo.Value.AddDays(1) : logFilter.DeliveryDateTo,
                DeliveryStatus = logFilter.DeliveryStatus != null ? (short?)logFilter.DeliveryStatus : null,
                logFilter.InteractionId,
                logFilter.ItemsPerPage,
                logFilter.JourneyId,
                logFilter.MobileNumber,
                logFilter.PageNo,
                logFilter.QuadrantTableName,
                logFilter.SendDateFrom,
                SendDateTo = logFilter.SendDateTo != null ? logFilter.SendDateTo.Value.AddDays(1) : logFilter.SendDateTo,
                SendStatus = logFilter.SendStatus != null ? (short?)logFilter.SendStatus : null
            };
            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(inputData);

            var logViewModels = await QueryProcAsPagingListAsync<LogViewModel>(StoredProcedure.GetMMSLog, dynamicParams);

            return logViewModels;
        }

        public async Task<IEnumerable<LogViewModel>> GetMmsLogAsync(LogFilterModel logFilter)
        {
            var sql = $@" 
                            SELECT
                                J.JourneyName,
                                I.Version,
                                A.ActivityName,
                                L.SendId,
                                L.MobileNumber,
                                L.MMSTemplateId,
                                T.TemplateName as MMSTemplateName,
                                L.DynamicParamsValue,
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
                                [dbo].[MMSJourneyInfo] J ON (J.JourneyId = L.JourneyId AND J.AccountId =@accountId AND J.JourneyId = @journeyId )
                            JOIN
                                [dbo].[MMSInteractionInfo] I ON (I.InteractionId = L.InteractionId AND I.AccountId =@accountId AND I.JourneyId = @journeyId)
                            JOIN
                                [dbo].[MMSActivityInfo] A ON (A.ActivityId = L.ActivityId AND A.AccountId =@accountId)
                            JOIN
                                [dbo].[MMSTemplate] T ON (T.Id = L.MMSTemplateId AND T.AccountId =@accountId)
                            LEFT JOIN 
								[dbo].MMSErrorCode EC ON (EC.ErrorCode = L.DropErrorCode)
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
                logFilter.SendDateTo = logFilter.SendDateTo.Value.AddDays(1);
                sql += @" AND L.SendDate Between @SendDateFrom And @SendDateTo ";
            }
            if (logFilter.DeliveryDateFrom.IsNotNull() && logFilter.DeliveryDateTo.IsNotNull())
            {
                logFilter.DeliveryDateTo = logFilter.DeliveryDateTo.Value.AddDays(1);
                sql += @" AND L.DeliveryDate Between @DeliveryDateFrom And @DeliveryDateTo ";
            }

            var logViewModels = await QueryAsync<LogViewModel>(sql, logFilter).ConfigureAwait(false);

            return logViewModels;
        }

        public async Task<bool> AddEntryToMmsLogTableInfoAsync(MMSLogTableInfoModel logTableInfoModel)
        {
            var sql = @"INSERT INTO [dbo].[MMSLogTableInfo]
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

        public async Task<bool> UpdateDataExtensionPushInMMSLogAsync(IList<WeChatifyDataExtensionMMSLog> logModels)
        {
            var sql = $@"UPDATE [dbo].[{_currentQuadrantMmsLogTableName}] SET IsUpdatedIntoDE =1 WHERE SendId = @SendId";

            var result = await ExecuteAsync(sql, logModels).ConfigureAwait(false);

            return result != 0;
        }

        public async Task<IList<MMSLogModel>> GetPendingStatusMMSAsync(long accountId, DateTime fromDate)
        {
            var sql =
                $@"SELECT Top 1000 * FROM [dbo].[{_currentQuadrantMmsLogTableName}] WHERE DeliveryStatus = 0 AND AccountId=@accountId AND SendDate < @fromDate ";

           var data = await QueryAsync<MMSLogModel>(sql, new {accountId, fromDate}).ConfigureAwait(false);

           return data.ToList();

        }

        public async Task<IList<LogViewModel>> GetMmsLogForUpdateToDEAsync(long accountId)
        {
            var sql = $@" 
                            SELECT TOP 10000
                                J.JourneyName,
                                I.Version,
                                A.ActivityName,
                                T.TemplateName as MMSTemplateName,
                                L.*
                            FROM 
                                [dbo].[{_currentQuadrantMmsLogTableName}] L
                            JOIN
                                [dbo].[MMSJourneyInfo] J ON (J.JourneyId = L.JourneyId AND J.AccountId =@accountId  )
                            JOIN
                                [dbo].[MMSInteractionInfo] I ON (I.InteractionId = L.InteractionId AND I.AccountId =@accountId )
                            JOIN
                                [dbo].[MMSActivityInfo] A ON (A.ActivityId = L.ActivityId AND A.AccountId =@accountId)
                            JOIN
                                [dbo].[MMSTemplate] T ON (T.Id = L.MMSTemplateId AND T.AccountId =@accountId)
                            WHERE
                            L.AccountId= @accountId
                            AND
                            L.IsUpdatedIntoDE = 0 ";
         

            var logViewModels = await QueryAsync<LogViewModel>(sql, new{@accountId = accountId}).ConfigureAwait(false);

            return logViewModels.ToList();
        }

        public async Task<List<IncomingMessageModel>> GetIncomingMessagesToUpdateDEAsync(long accountId)
        {
            var sql = @"SELECT Top 10000 * FROM dbo.[MMSIncomingMessages] WHERE [AccountId] = @accountId AND [IsUpdatedIntoDE] = 0 ";


            var result = await QueryAsync<IncomingMessageModel>(sql, new {accountId});
            return result.ToList();
        }

        public async Task<bool> UpdateDataExtensionPushInIncomingMessageAsync(IList<WeChatifyDataExtensionIncomingLog> logModels)
        {
            var sql = $@"UPDATE dbo.[MMSIncomingMessages] SET IsUpdatedIntoDE =1 WHERE Id = @Id";

            var result = await ExecuteAsync(sql, logModels).ConfigureAwait(false);

            return result != 0;
        }
        public async Task<bool> SaveJourneyAsync(JourneyProcModel journeyProcModel)
        {
            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(journeyProcModel);

            var result = await ExecuteProcAsync(StoredProcedure.MMSJourneyUpsert, parameters);

            return result != 0;

        }

        public async Task CreateQuadrantObjectsAsync(string sql)
        {
            await ExecuteAsync(sql);
        }
        public async Task<List<MMSActivityInfoModel>> GetReports(long accountId, string filterDate)
        {
                string query = $@"SELECT A.*,B.JourneyName,B.InitiatedDate FROM MMSActivityInfo  A 
                                   Inner Join MMSJourneyInfo B on A.JourneyId =B.JourneyId
                                WHERE A.AccountId=@accountId AND B.AccountId=@accountId and
                                A.CreatedOn >= Cast(@filterDate as datetime)";
            var data = await QueryAsync<MMSActivityInfoModel>(query, new { accountId = accountId, filterDate = filterDate });
            return data.ToList();
        }

        public async Task<List<DropReasonModel>> GetMMSLogForDroppedReason(long accountId, string filterDate,string neededQuadTable)
        {
            string query = $@"SELECT  A.MobileNumber,J.JourneyName as ActionName,A.DropErrorCode as ErrorCode,B.EnglishDescription AS Description, A.SendDate FROM " +_currentQuadrantMmsLogTableName+ @" A
                            INNER JOIN  MMSJourneyInfo J ON A.JourneyId=J.JourneyId			               
                            LEFT JOIN MMSErrorCode B ON A.DropErrorCode=B.ErrorCode
			                 WHERE A.AccountId=@accountId AND  A.DropErrorCode is NOT NULL AND 
                                A.SendDate >= Cast( @filterDate  as datetime)";
            var data = await QueryAsync<DropReasonModel>(query,new { accountId = accountId , filterDate = filterDate });
            if (neededQuadTable.ToLower()!= _currentQuadrantMmsLogTableName.ToLower())
            {
                string prevquery = $@"SELECT  A.MobileNumber,J.JourneyName as ActionName,A.DropErrorCode as ErrorCode,B.EnglishDescription AS Description, A.SendDate FROM " + neededQuadTable + @" A
                            INNER JOIN  MMSJourneyInfo J ON A.JourneyId=J.JourneyId			               
                            LEFT JOIN MMSErrorCode B ON A.DropErrorCode=B.ErrorCode
			                 WHERE A.AccountId=@accountId AND  A.DropErrorCode is NOT NULL AND 
                                A.SendDate >= Cast( @filterDate  as datetime)";
                var prevquerydata = await QueryAsync<DropReasonModel>(prevquery, new { accountId = accountId, filterDate = filterDate });

                if (prevquerydata!=null && prevquerydata.Count()>0)
                {
                   return  data.Concat(prevquerydata).ToList();
                }
            }
            return data.ToList();
        }
        public async Task<List<MMSLogModel>> GetMMSLogByDate(long accountId, string filterDate,string neededQuadTable)
        {
            string query = $@"SELECT * FROM " + _currentQuadrantMmsLogTableName + @"   
                                WHERE AccountId=@accountId AND
                                SendDate >= Cast(@filterDate as datetime)";
          
            var data = await QueryAsync<MMSLogModel>(query, new { accountId = accountId, filterDate = filterDate });

            if (neededQuadTable.ToLower() != _currentQuadrantMmsLogTableName.ToLower() )
            {
                string prevQuery = $@"SELECT * FROM " + neededQuadTable + @"   
                                WHERE AccountId=@accountId AND
                                SendDate >= Cast(@filterDate as datetime)";
                var prevData = await QueryAsync<MMSLogModel>(prevQuery, new { accountId = accountId, filterDate = filterDate });
                var mmsLogModels = prevData.ToList();
                if (mmsLogModels.Any())
                {
                    var returnData = data.Concat(mmsLogModels);
                    return returnData.ToList();
                }
            }
            return data.ToList();
        }

        public async Task<int> GetUsedMMSCountAsync(long accountId, DateTime fromDate, DateTime toDate)
        {
            var tableName = fromDate.GetQuadrantMMSLogTableName();
            var sql = $@"SELECT Count(SendId) 
                            FROM dbo.[{tableName}] 
                            WHERE 
                            [AccountId] = @accountId 
                            AND SentStatus = @sendStatus
                            AND DeliveryStatus in @deliveryStatus
                            AND SendDate >= @fromDate 
                            AND SendDate < @toDate";

            var data = await QueryFirstOrDefaultAsync<int>(sql, 
                new { accountId,
                    @fromDate = fromDate,
                    @toDate = toDate.AddDays(1),
                    @deliveryStatus = new[]
                    {
                        DeliveryStatus.UnConfirmed,
                        DeliveryStatus.Delivered
                    },
                    @sendStatus = SendStatus.Success
                }).ConfigureAwait(false);

            return data;
        }




        public async Task<List<MMSUsageModel>> GetMMSUsageDetailAsync(long accountId, int year)
        {
            var sql = $@"SELECT * 
                            FROM [dbo].[MMSUsageDetails] 
                            WHERE 
                            [AccountId] = @accountId AND Year = @year ORDER BY Month ASC";

            var data = await QueryAsync<MMSUsageModel>(sql,
                new
                {
                    accountId,
                    year
                }).ConfigureAwait(false);

            return data.ToList();
        }

        public async Task<List<int>> GetMMSUsageYearsAsync(long accountId)
        {
            var sql = $@"SELECT distinct Year 
                            FROM [dbo].[MMSUsageDetails] 
                            WHERE 
                            [AccountId] = @accountId";

            var data = await QueryAsync<int>(sql,
                new
                {
                    accountId
                }).ConfigureAwait(false);

            return data.ToList();
        }

        public async Task<bool> AddOrUpdateMMSUsageDetailAsync(MMSUsageModel model)
        {
            var sql = $@"IF EXISTS (Select 1 From [dbo].[MMSUsageDetails] WHERE AccountId = @AccountId AND Year = @Year AND Month= @Month)
                                    BEGIN
		                                    UPDATE [dbo].[MMSUsageDetails]
		                                       SET [RechargeCount] = @RechargeCount
			                                      ,[UsedCount] = @UsedCount
			                                      ,[Balance] = @Balance
		                                     WHERE AccountId = @AccountId AND Year = @Year AND Month= @Month
                                    END
                                    Else
                                    BEGIN
		                                    INSERT INTO [dbo].[MMSUsageDetails]
				                                       ([AccountId]
				                                       ,[Year]
				                                       ,[Month]
				                                       ,[RechargeCount]
				                                       ,[UsedCount]
				                                       ,[Balance])
			                                     VALUES
				                                       (@AccountId
				                                       ,@Year
				                                       ,@Month
				                                       ,@RechargeCount
				                                       ,@UsedCount
				                                       ,@Balance)
                                    END ";

            var data = await ExecuteAsync(sql, model).ConfigureAwait(false);

            return data.IsNotDefault();
        }
    }
}
