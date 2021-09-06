using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.RedisRepository.Interface;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;

namespace MessageService.Repository.Implementation
{
    public class SettingsRepository : BaseRepository, ISettingsRepository
    {
        private readonly string _weChatifyConnectionString;
        private readonly string _MMSConnectionString;
        private readonly IRedisRespository _redisRepository;
        private readonly IWeChatifyRepository _wechatifyRepository;

        public SettingsRepository(string wechatify, string messageService, IRedisRespository redisRepository, IWeChatifyRepository wechatifyRepository)
        {
            _weChatifyConnectionString = wechatify;
            _MMSConnectionString = messageService;
            _redisRepository = redisRepository;
            _wechatifyRepository = wechatifyRepository;
        }
        public async Task<string> GetErrorDescriptionAsync(string errorCode, string language)
        {

            var sql = (language == LanguageType.Chinese) ? @"SELECT ChineseDescription FROM [dbo].MMSErrorCode(NoLock) Where ErrorCode =@errorCode"
                                        : @"SELECT EnglishDescription FROM [dbo].MMSErrorCode(NoLock) Where ErrorCode =@errorCode";

            var result = await QueryFirstOrDefaultAsync<string>(sql, new { errorCode });
            return result;
        }
        public async Task<List<MMSErrorCodeModel>> GetErrorDescriptionAsync()
        {
            var sql = @"SELECT ErrorCode,EnglishDescription,ChineseDescription FROM MMSErrorCode(NoLock)";
            var result = await QueryAsync<MMSErrorCodeModel>(sql);
            return result.ToList();
        }

        public async Task<int> AddOrUpdateMMSVendorSettingsAsync(VendorSettingsModel model)
        {
            var sql = @"
                        IF NOT EXISTS(Select 1 from [dbo].[MMSVendorSettings] WHERE AccountId = @accountId )
                        Begin
                        INSERT INTO [dbo].[MMSVendorSettings]
                                           ([AccountId]
                                           ,[VendorId]
                                           ,[AppId]
                                           ,[AppKey]
                                           ,[CreatedOn]
                                           ,[UpdatedOn])
                                     VALUES
                                           (@AccountId,
                                           @VendorId, 
                                           @AppId, 
                                           @AppKey, 
                                           @CreatedOn, 
                                           null)
                                      End
                                  ELSE
                                     Begin
                                        UPDATE [dbo].[MMSVendorSettings]
                                                       SET 
                                                           [VendorId] = @VendorId
                                                          ,[AppId] = @AppId
                                                          ,[AppKey] = @AppKey   
                                                          ,[UpdatedOn] = @UpdatedOn
                                                     WHERE [AccountId] = @AccountId
                                     End ";
            await _redisRepository.UpdateMMSVendorSettingsAsync(model);
            return await ExecuteAsync(sql, model);

        }

        public async Task<VendorSettingsModel> GetMMSVendorSettingsAsync(long accountid)
        {
            var settingsFromCache = await _redisRepository.GetMMSVendorSettingsAsync(accountid);
            if (settingsFromCache != null)
            {
                return settingsFromCache;
            }

            var sql = @"SELECT * FROM MMSVendorSettings WHERE AccountId = @accountid";

            var result = await QueryFirstOrDefaultAsync<VendorSettingsModel>(sql, new { accountid });
            await _redisRepository.UpdateMMSVendorSettingsAsync(result);
            return result;
        }
        public async Task<List<VendorSettingsModel>> GetAllMMSVendorSettingsAsync()
        {
            var sql = @"SELECT * FROM MMSVendorSettings (nolock)";
            var result = await QueryAsync<VendorSettingsModel>(sql);
            return result.ToList();
        }
        public async Task<IEnumerable<long>> GetMMSMappedAccountIdsAsync()
        {
            var sql = @"SELECT AccountId FROM [dbo].MMSVendorSettings(NoLock)";
            var result = await QueryAsync<long>(sql, null);
            return result;
        }

        public async Task<bool> CheckMMSVendorSettingsExistsAsync(long accountid)
        {
            var sql = @"SELECT * FROM MMSVendorSettings WHERE AccountId = @accountid";

            var result = await QueryFirstOrDefaultAsync<VendorSettingsModel>(sql, new { accountid });

            return result != null;
        }

        public async Task<VendorSettingsModel> GetMMSVendorSettingsAsync(string appId)
        {
            var settingsFromCache = await _redisRepository.GetMMSVendorSettingsAsync(appId);
            if (settingsFromCache != null)
            {
                return settingsFromCache;
            }

            var sql = @"SELECT * FROM MMSVendorSettings WHERE AppId = @appId";

            var result = await QueryFirstOrDefaultAsync<VendorSettingsModel>(sql, new { appId });
            await _redisRepository.UpdateMMSVendorSettingsAsync(result);
            return result;
        }

        public async Task<bool> IsMMSVendorSettingsValidAsync(VendorSettingsModel model)
        {
            var sql = @"SELECT * FROM MMSVendorSettings WHERE (AppId = @appId OR AppKey = @appKey) and AccountId != @accountId";
            var result = await QueryFirstOrDefaultAsync<VendorSettingsModel>(sql, model);
            return result == null;
        }

        #region MMS Setting
        public async Task<int> InsertMMSNotificationUsers(NotifyUser param)
        {
            switch (param.NotificationType)
            {
                case (int)NotificationType.MMSDeliveryReportSetting:
                    {
                        string mmsDeliveryReportNotificaionSql = @"SELECT ID FROM [dbo].[MMSDeliveryReportNotification] WHERE AccountId=@accountId";
                        string mmsDeliveryReportNotificaionSqlResult = await QueryFirstOrDefaultAsync<string>(mmsDeliveryReportNotificaionSql, new { accountId = param.AccountId});
                        var listmodel = (from u in param.NotifyUserIds
                                         select new
                                         {
                                             notificationtype = param.NotificationType,
                                             deliveryreportid = mmsDeliveryReportNotificaionSqlResult,
                                             NotifyUserId = u
                                         });
                        if (!string.IsNullOrWhiteSpace(mmsDeliveryReportNotificaionSqlResult) && param.NotifyUserIds.Count > 0)
                        {
                            string InsertMMSNotificationUsersSql = @"INSERT INTO [dbo].[MMSNotificationUsers](NotifyUserId,Id,NotificationType,DeliveryReportId) VALUES
                                                        (@NotifyUserId,newid(),@notificationtype,@deliveryreportid)";
                            return await ExecuteAsync(InsertMMSNotificationUsersSql, listmodel);
                        }
                        else
                        {
                            return 98;
                        }
                    }
                    break;
                case (int)NotificationType.MMSBalanceThreshold:
                    {

                        string mmsBalanceThresholdSql = @"SELECT SettingId FROM [dbo].[MMSBalanceThreshold] WHERE AccountId=@accountId";
                        string mmsBalanceThresholdSqlResult = await QueryFirstOrDefaultAsync<string>(mmsBalanceThresholdSql, new { accountId = param.AccountId });

                        if (string.IsNullOrWhiteSpace(mmsBalanceThresholdSqlResult))
                        {
                            mmsBalanceThresholdSqlResult = Guid.NewGuid().ToString();
                            var insertToThershold = await InsertOrUpdateMMSBalanceThreshold(new MMSBalanceThreshold
                            {
                                AccountId = param.AccountId,
                                CreatedOn = DateTime.UtcNow.ToChinaTime(),
                                SettingId = mmsBalanceThresholdSqlResult,
                                ThresholdCount = 0,
                                CreatedBy = param.ChangedBy
                            });
                        }
                        
                        mmsBalanceThresholdSqlResult = await QueryFirstOrDefaultAsync<string>(mmsBalanceThresholdSql, new { accountId = param.AccountId });

                        if ( !string.IsNullOrWhiteSpace(mmsBalanceThresholdSqlResult) && param.NotifyUserIds.Count > 0)
                        {
                            var listmodel = (from u in param.NotifyUserIds
                                             select new
                                             {
                                                 notificationtype = param.NotificationType,
                                                 deliveryreportid = mmsBalanceThresholdSqlResult,
                                                 NotifyUserId = u
                                             });
                            string InsertMMSNotificationUsersSql = @"INSERT INTO [dbo].[MMSNotificationUsers](NotifyUserId,Id,NotificationType,SettingId) VALUES
                                                        (@NotifyUserId,newid(),@notificationtype,@deliveryreportid)";
                            return await ExecuteAsync(InsertMMSNotificationUsersSql, listmodel);
                        }

                        return 99;
                    }
                    break;
            }


            return 0;
        }

        public async Task<int> DeleteMMSNotificationUsers(NotifyUser param)
        {
            var listmodel = (from u in param.NotifyUserIds
                             select new
                             {
                                 NotificationType = param.NotificationType,
                                 NotifyUserId = u
                             });
            if (param.NotifyUserIds.Count > 0)
            {
                string deleteBalanceThreshold = @"DELETE FROM [dbo].[MMSNotificationUsers]
                                                            WHERE NotificationType=@NotificationType AND NotifyUserId=@NotifyUserId";
                return await ExecuteAsync(deleteBalanceThreshold, listmodel);
            }
            return 0;
        }

        public async Task<int> InsertOrUpdateMMSDeliveryReportNotification(MMSDeliveryReportNotification model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = Guid.NewGuid().ToString();
            }
            string insertMMSDeliveryReportNotificationSql = @"IF NOT EXISTS(Select 1 from [dbo].[MMSDeliveryReportNotification] WHERE AccountId = @AccountId )
                                                             Begin
                                                          INSERT INTO [dbo].[MMSDeliveryReportNotification]
                                                                    ([Id]
                                                                    ,[AccountId]
                                                                    ,[RunBy]
                                                                    ,[RunDay] 
                                                                    ,[RunOnTime]
                                                                    ,[CreatedOn]
                                                                    ,[Percentage])
                                                              VALUES
                                                                    (@Id
                                                                    ,@AccountId
                                                                    ,@RunBy
                                                                    ,@RunDay
                                                                    ,@RunOnTime
                                                                    ,@CreatedOn
                                                                    ,@Percentage)
                                                            End
                                                           ELSE
                                                           Begin
                                                                UPDATE [dbo].[MMSDeliveryReportNotification]
                                                                   SET 
                                                                      [RunBy] = @RunBy
                                                                      ,[RunDay] = @RunDay
                                                                      ,[RunOnTime] = @RunOnTime
                                                                      ,[Percentage] =@Percentage
                                                                 WHERE [AccountId] =@AccountId
                                                             End";

            return await ExecuteAsync(insertMMSDeliveryReportNotificationSql, model);
        }

        public async Task<List<NotifyUserList>> InsertOrUpdateMMSBalanceThreshold(MMSBalanceThreshold model)
        {
            bool isSentEmail = false;

            var balanceThreshold = await GetMMSBalanceThresholdByAccountId(model.AccountId);
            if (balanceThreshold != null && balanceThreshold.ThresholdCount > 0 && model.ThresholdCount != balanceThreshold.ThresholdCount)
            {
                isSentEmail = true;
            }

            if (string.IsNullOrEmpty(model.SettingId))
            {
                model.SettingId = Guid.NewGuid().ToString();
            }
            
            var insertMMSBalanceThresholdSql = @"IF NOT EXISTS(Select 1 from [dbo].[MMSBalanceThreshold] WHERE AccountId = @AccountId )
                                                  Begin
                                                INSERT INTO [dbo].[MMSBalanceThreshold]
                                                           ([SettingId]
                                                           ,[AccountId]
                                                           ,[ThresholdCount]
                                                           ,[CreatedBy]
                                                           ,[CreatedIP]
                                                           ,[CreatedOn]
                                                           ,[ModifiedBy]
                                                           ,[ModifiedIP]
                                                           ,[ModifiedOn])
                                                     VALUES
                                                           (Newid()
                                                           ,@AccountId
                                                           ,@ThresholdCount
                                                           ,@CreatedBy
                                                           ,@CreatedIP
                                                           ,@CreatedOn
                                                           ,@ModifiedBy
                                                           ,@ModifiedIP
                                                           ,@ModifiedOn)
                                                 End
                                                ELSE
                                                Begin
                                                    UPDATE [dbo].[MMSBalanceThreshold]
                                                       SET 
                                                          [ThresholdCount] = @ThresholdCount
                                                          ,[ModifiedBy] = @ModifiedBy
                                                          ,[ModifiedIP] = @ModifiedIP
                                                          ,[ModifiedOn] = @ModifiedOn
                                                     WHERE [AccountId] =@AccountId
                                                     End";
            var result = await ExecuteAsync(insertMMSBalanceThresholdSql, model);
            if (isSentEmail && result > 0)
            {
                var notifyUser = await GetNotificationToUsers(model.AccountId, NotificationType.MMSBalanceThreshold);
                if (notifyUser != null && notifyUser.Count > 0)
                {
                    return notifyUser;
                }
            }
            return null;
        }



        public async Task<MMSDeliveryReportNotification> GetMMSDeliveryReportNotificationSettingByAccountId(long accountId)
        {
            string deliveryReportNotifSql = @"SELECT * FROM MMSDeliveryReportNotification
                                              WHERE AccountId=@AccountId";
            var List = await QueryFirstOrDefaultAsync<MMSDeliveryReportNotification>(deliveryReportNotifSql, new { AccountId = accountId });

            return List;
        }
        public async Task<MMSBalanceThreshold> GetMMSBalanceThresholdByAccountId(long accountId)
        {
            string deliveryReportNotifSql = @"SELECT * FROM MMSBalanceThreshold
                                              WHERE AccountId=@AccountId";
            var List = await QueryFirstOrDefaultAsync<MMSBalanceThreshold>(deliveryReportNotifSql, new { AccountId = accountId });
            return List;
        }
        public async Task<List<string>> GetMMSNotificationUsers(long accountId, NotificationType Type)
        {
            string NotifSql = "";
            switch (Type)
            {
                case NotificationType.MMSBalanceThreshold:
                    {
                        NotifSql = @"SELECT NotifyUserId FROM MMSNotificationUsers A
                                              INNER JOIN MMSBalanceThreshold B ON A.SettingId=B.SettingId
                                              WHERE b.AccountId=@AccountId AND A.NotificationType=@NotificationType";
                    }
                    break;
                case NotificationType.MMSDeliveryReportSetting:
                    {
                        NotifSql = @"SELECT NotifyUserId FROM MMSNotificationUsers A
                                              INNER JOIN MMSDeliveryReportNotification B ON A.DeliveryReportId=B.Id
                                              WHERE b.AccountId=@AccountId AND A.NotificationType=@NotificationType";
                    }
                    break;
            }
            var usersList = await QueryAsync<string>(NotifSql, new { AccountId = accountId, NotificationType = ((int)Type) });
            return usersList.ToList();
        }
        public async Task<List<NotifyUserList>> GetNotificationToUsers(long accountId, NotificationType Type)
        {
            var wechatifyUsers = await _wechatifyRepository.GetAllWeChatifyUsersAsync(accountId);
            switch (Type)
            {
                case NotificationType.MMSBalanceThreshold:
                    {
                        string deliveryReportNotifSql = @"SELECT NotifyUserId FROM MMSNotificationUsers A
                                              INNER JOIN MMSBalanceThreshold B ON A.SettingId=B.SettingId
                                              WHERE b.AccountId=@AccountId AND A.NotificationType=@NotificationType";
                        var usersList = await QueryAsync<string>(deliveryReportNotifSql, new { AccountId = accountId, NotificationType = ((int)Type) });
                        var data = (from u in usersList
                                    join wu in wechatifyUsers on new { UserId = u } equals new { UserId = wu.UserId }
                                    orderby wu.Name ascending
                                    select new NotifyUserList
                                    {
                                        Email = wu.Email,
                                        Name = wu.Name,
                                        UserId = wu.UserId,
                                        Role = wu.RoleName
                                    }).ToList();
                        return data;
                    }
                case NotificationType.MMSDeliveryReportSetting:
                    {
                        string deliveryReportNotifSql = @"SELECT NotifyUserId FROM MMSNotificationUsers A
                                              INNER JOIN MMSDeliveryReportNotification B ON A.DeliveryReportId=B.Id
                                              WHERE b.AccountId=@AccountId AND A.NotificationType=@NotificationType";
                        var usersList = await QueryAsync<string>(deliveryReportNotifSql, new { AccountId = accountId, NotificationType = ((int)Type) });
                        var data = (from u in usersList
                                    join wu in wechatifyUsers on new { UserId = u } equals new { UserId = wu.UserId }
                                    orderby wu.Name ascending
                                    select new NotifyUserList
                                    {
                                        Email = wu.Email,
                                        Name = wu.Name,
                                        UserId = wu.UserId,
                                        Role = wu.RoleName
                                    }).ToList();
                        return data;
                    }
            }
            return null;
           
        }

        public async Task<List<NotifyUserList>> GetDeliveryReportNotificationUsers(FilterParam param)
        {
            var wechatifyUsers = await _wechatifyRepository.GetAllWeChatifyUsersAsync(param.accountId);
            string deliveryReportNotifSql = @"SELECT NotifyUserId FROM MMSNotificationUsers A
                                              INNER JOIN MMSDeliveryReportNotification B ON A.DeliveryReportId=B.Id
                                              WHERE b.AccountId=@AccountId AND A.NotificationType=@NotificationType";

            var usersList = await QueryAsync<string>(deliveryReportNotifSql, new { AccountId = param.accountId, NotificationType = ((int)param.Type) });
            if (usersList != null)
            {
                var data = (from u in usersList
                            join wu in wechatifyUsers on new { UserId = u } equals new { UserId = wu.UserId }
                            orderby wu.Name ascending
                            select new NotifyUserList
                            {
                                Email = wu.Email,
                                Name = wu.Name,
                                UserId = wu.UserId,
                                Role = wu.RoleName
                            });
                int totalCount = data.Count();
                if (param.sortType.ToLower() == "asc")
                {
                    var returnList = data.OrderBy(x => x.Name).Skip((param.pageNumber.Value - 1) * param.recordPerPage.Value).Take(param.recordPerPage.Value).ToList();
                    if (returnList.Count() > 0 && returnList[0] != null)
                        returnList[0].TotalCount = totalCount;
                    return returnList;
                }
                else
                {
                    var returnList = data.OrderByDescending(x => x.Name).Skip((param.pageNumber.Value - 1) * param.recordPerPage.Value).Take(param.recordPerPage.Value).ToList();
                    if (returnList.Count() > 0 && returnList[0] != null)
                        returnList[0].TotalCount = totalCount;
                    return returnList;
                }
            }
            return null;
        }
        public async Task<List<NotifyUserList>> GetMMSBalanceThreshold(FilterParam param)
        {
            var wechatifyUsers = await _wechatifyRepository.GetAllWeChatifyUsersAsync(param.accountId);
            string deliveryReportNotifSql = @"SELECT NotifyUserId FROM MMSNotificationUsers A
                                              INNER JOIN MMSBalanceThreshold B ON A.SettingId=B.SettingId
                                              WHERE b.AccountId=@AccountId AND A.NotificationType=@NotificationType";

            var usersList = await QueryAsync<string>(deliveryReportNotifSql, new { AccountId = param.accountId, NotificationType = ((int)param.Type) });
            if (usersList != null)
            {
                var data = (from u in usersList
                            join wu in wechatifyUsers on new { UserId = u } equals new { UserId = wu.UserId }
                            select new NotifyUserList
                            {
                                Email = wu.Email,
                                Name = wu.Name,
                                UserId = wu.UserId,
                                Role = wu.RoleName
                            });

                int totalCount = data.Count();
                if (param.sortType.ToLower() == "asc")
                {
                    var returnList = data.OrderBy(x => x.Name).Skip((param.pageNumber.Value - 1) * param.recordPerPage.Value).Take(param.recordPerPage.Value).ToList();
                    if (returnList.Count() > 0 && returnList[0] != null)
                        returnList[0].TotalCount = totalCount;
                    return returnList;
                }
                else
                {
                    var returnList = data.OrderByDescending(x => x.Name).Skip((param.pageNumber.Value - 1) * param.recordPerPage.Value).Take(param.recordPerPage.Value).ToList();
                    if (returnList.Count() > 0 && returnList[0] != null)
                        returnList[0].TotalCount = totalCount;
                    return returnList;
                }
            }
            return null;
        }
        #endregion


    }
}
