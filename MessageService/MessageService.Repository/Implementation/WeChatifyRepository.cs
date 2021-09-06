using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.WeChatifyModels;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;

namespace MessageService.Repository.Implementation
{
    public class WeChatifyRepository :BaseRepository, IWeChatifyRepository
    {
        private readonly string _weChatifyConnectionString;

        public WeChatifyRepository(string wechatify)
        {
            _weChatifyConnectionString = wechatify;
        }

        public async Task<WeChatifyAccountModel> GetAccountDetailsAsync(long accountId)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT * from [dbo].WeChatAccounts WHERE AccountId = @accountId";

            return await QueryFirstOrDefaultAsync<WeChatifyAccountModel>(sql, new { accountId });
        }
        public async Task<WeChatUserModel> GetFollowerDetailsAsync(string openId)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT * from [dbo].WeChatUsers WHERE OpenId = @openId";

            return await QueryFirstOrDefaultAsync<WeChatUserModel>(sql, new { openId });
        }

        public async Task<IEnumerable<WeChatifySFAccountModel>> GetSFAccountDetailsAsync(long[] accountIds)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT     WCA.AccountId,
                                    WCA.AccountName,
                                    SF.LoadWithSpecificBU 
                                    FROM 
                                        [dbo].WeChatAccounts WCA
                                    LEFT JOIN
                                        [dbo].SfAccountInfo SF ON (SF.AccountId = WCA.AccountId)
                                    WHERE WCA.AccountId in @accountIds AND IsSaleForceAccount = 1";

            return await QueryAsync<WeChatifySFAccountModel>(sql, new { accountIds });
        }

        public async Task<string> GetUserIdAsync(string userName)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"SELECT Top 1 Id FROM [dbo].AspNetUsers(NoLock) WHERE UserName = @userName";
            var result = await QueryFirstOrDefaultAsync<string>(sql, new { @userName = userName });
            return result;
        }
        public async Task<WeChatifyUserModel> GetUserByIdAsync(string userId)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"SELECT * FROM [dbo].AspNetUsers(NoLock) WHERE Id = @userId";
            var result = await QueryFirstOrDefaultAsync<WeChatifyUserModel>(sql, new { userId = userId });
            return result;
        }

        public async Task<IEnumerable<WeChatifySFAccountModel>> GetSFMappedAccountsAsync(string userId, long[] accountIds)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"SELECT  WA.AccountId,
                                WA.AccountName,
                                SA.LoadWithSpecificBU  
                                    FROM    [dbo].WeChatAccounts WA 
                                    JOIN
                                            [dbo].UserAndRoleMapping UR ON (WA.AccountId = UR.AccountId)
                                    JOIN
                                            [dbo].SfAccountInfo SA ON (WA.AccountId = SA.AccountId)
                                    WHERE 
                                        UR.UserId = @userId
                                    AND
                                        WA.IsDeleted = 0
                                    AND 
                                        WA.IsSaleForceAccount = 1
                                    AND
                                        WA.AccountId in @accountIds";
            var result = await QueryAsync<WeChatifySFAccountModel>(sql, new { userId, @accountIds = accountIds });
            return result;
        }

        public async Task<WeChatifySFAccountModel> GetSFMappedAccountAsync(long accountId)
        {
            DbConnectionString = _weChatifyConnectionString;
                    var sql = @"SELECT  WA.AccountId,
                                        WA.AccountName,
                                        WA.WeChatId,
                                        SA.LoadWithSpecificBU,
                                        SA.UserId,
                                        SA.OrgId,
                                        SA.IsEnterpriseAccount,
                                        SA.IsSharedDEConfigured,
                                        SA.ParentOrgId
                                    FROM    
                                        [dbo].WeChatAccounts WA 
                                    JOIN
                                        [dbo].SfAccountInfo SA ON (WA.AccountId = SA.AccountId)
                                    WHERE 
                                       
                                        WA.IsDeleted = 0
                                    AND 
                                        WA.IsSaleForceAccount = 1
                                    AND
                                        WA.AccountId = @accountId";
            var result = await QueryFirstOrDefaultAsync<WeChatifySFAccountModel>(sql, new { @accountId = accountId });
            return result;
        }

        public async Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT DISTINCT(ANU.Id) as UserId, ANU.FirstName, ANU.LastName,  ANU.Email, URM.AccountId as AccountId ,UROLE.RoleName 
                             FROM UserAndRoleMapping URM
                             INNER JOIN AspNetUsers ANU ON URM.UserId = ANU.Id
                             INNER JOIN RoleLevelPermissions RLP ON RLP.RoleId = URM.RoleId 
                             INNER JOIN UserRoleMaster UROLE ON UROLE.RoleId = URM.RoleId 
                             WHERE URM.AccountId = @AccountId AND (RLP.ModuleId = @moduleId OR RLP.ModuleId = @smsModuleId)";
           
            return await QueryAsync<WeChatifyUserModel>(sql, new { @AccountId = accountId,
                @moduleId = (int)WeChatifyModules.MMS,
                @smsModuleId = (int)WeChatifyModules.SMS });
        }
        public async Task<IEnumerable<WeChatifyUserModel>> GetAllSMSWeChatifyUsersAsync(long accountId)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT DISTINCT(ANU.Id) as UserId, ANU.FirstName, ANU.LastName,  ANU.Email, URM.AccountId as AccountId ,UROLE.RoleName 
                             FROM UserAndRoleMapping URM
                             INNER JOIN AspNetUsers ANU ON URM.UserId = ANU.Id
                             INNER JOIN RoleLevelPermissions RLP ON RLP.RoleId = URM.RoleId 
                             INNER JOIN UserRoleMaster UROLE ON UROLE.RoleId = URM.RoleId 
                             WHERE URM.AccountId = @AccountId AND RLP.ModuleId = @moduleId ";

            return await QueryAsync<WeChatifyUserModel>(sql, new { @AccountId = accountId, @moduleId = (int)WeChatifyModules.SMS });
        }

        public async Task<IEnumerable<WeChatifyUserModel>> GetAllWeChatifyUsersAsync(long accountId,int pg, int numberOfRecords)
        {
            int offsetCount = ((pg - 1) * numberOfRecords);
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT DISTINCT(ANU.Id) as UserId, ANU.FirstName, ANU.LastName,  ANU.Email, URM.AccountId as AccountId ,UROLE.RoleName 
                             FROM UserAndRoleMapping URM
                             INNER JOIN AspNetUsers ANU ON URM.UserId = ANU.Id
                             INNER JOIN RoleLevelPermissions RLP ON RLP.RoleId = URM.RoleId 
                             INNER JOIN UserRoleMaster UROLE ON UROLE.RoleId = URM.RoleId 
                             WHERE URM.AccountId = @AccountId AND RLP.ModuleId = @moduleId
                            ORDER BY ANU.FIRSTNAME ASC
                             OFFSET @offsetCount Rows
                            FETCH NEXT @noOfRecords Rows ONLY
                            ";

            return await QueryAsync<WeChatifyUserModel>(sql, new { @AccountId = accountId, @moduleId = (int)WeChatifyModules.MMS, offsetCount = offsetCount, noOfRecords = numberOfRecords });
        }

        public async Task<WeChatifyUserModel> GetUsersAsync(string userId)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = $@"SELECT DISTINCT(ANU.Id) as UserId, ANU.FirstName, ANU.LastName,  ANU.Email, URM.AccountId as AccountId , UR.RoleName 
                             FROM [dbo].UserAndRoleMapping URM
                             INNER JOIN [dbo].AspNetUsers ANU ON URM.UserId = ANU.Id
                             INNER JOIN [dbo].RoleLevelPermissions RLP ON RLP.RoleId = URM.RoleId 
                             INNER JOIN [dbo].UserRoleMaster UR ON UR.RoleId = RLP.RoleId 
                             WHERE ANU.Id=@userId AND RLP.ModuleId = @moduleId ";

            return await QueryFirstOrDefaultAsync<WeChatifyUserModel>(sql, new { @userId, @moduleId = (int)WeChatifyModules.MMS });
        }

        public async Task<int> AddOrUpdateSMSVendorSettingsAsync(Models.CosmosModel.SMSVendorSettingsDocumentModel model)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"
                        DECLARE @VendorID1 varchar(50);
                        DECLARE @UserId varchar(50);
                        Select TOP 1 @VendorID1 = VendorId from CommunicationVendorMaster WHERE VendorName = 'Submail';
                        Select TOP 1 @UserId = Id from AspNetUsers;
                        IF NOT EXISTS(Select 1 from [dbo].[CommunicationVendorSettings] WHERE AccountId = @accountId )
                        Begin
                        INSERT INTO [dbo].[CommunicationVendorSettings]
                                           ([SettingsID]
                                           ,AccountId
                                           ,[VendorId]
                                           ,[VendorUserName]
                                           ,[VendorPassword]
                                            ,[SmsBalance]
                                           ,[SignatureText]
                                           ,[UnSubscribeText]
                                           ,[TemplateId]
                                           ,[VerificationTemplateId]
                                           ,[CreatedOn]
                                           ,[CreatedBy]
)
                                     VALUES
                                           (
                                           NEWID(),
                                           @AccountId,
                                           @VendorID1, 
                                           @AppId, 
                                           @AppKey, 
                                           0,
                                           @SignatureText,
                                           @UnSubscribeText,
                                           @TemplateId,
                                           @VerificationTemplateId,
                                           @CreatedOn,
                                            @UserId
                                           )
                                      End
                                  ELSE
                                     Begin
                                        UPDATE [dbo].[CommunicationVendorSettings]
                                                       SET 
                                                           [VendorId] = @VendorID1
                                                           ,[SmsBalance] = 0
                                                          ,[VendorUserName] = @AppId
                                                          ,[VendorPassword] = @AppKey   
                                                          ,[SignatureText] = @SignatureText
                                                          ,[UnSubscribeText] = @UnSubscribeText
                                                          ,[VerificationTemplateId] = @VerificationTemplateId
                                                          ,[TemplateId] = @TemplateId                                                        
                                                     WHERE [AccountId] = @AccountId
                                     End ";
            //await _redisRepository.UpdateSMSVendorSettingsAsync(model);
            return await ExecuteAsync(sql, model);

        }

        public async Task<IEnumerable<long>> GetSMSMappedAccountIdsAsync()
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"SELECT AccountId FROM [dbo].CommunicationVendorSettings(NoLock)";
            var result = await QueryAsync<long>(sql, null);
            return result;
        }

        public async Task<bool> UpdateSMSCampaignNumbersAsync(SMSLogDocumentModel logModel)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"update [dbo].SMSCampaignNumbers Set DeliveryDate = @deliveryDate, Status = @status Where ExternalID = @externalId  ";
            var result = await ExecuteAsync(sql, new { @deliveryDate = logModel.DeliveryDate, @externalId = logModel.Id, @status = logModel.DeliveryStatus?.ToString()});
            return result.IsNotDefault();
        }

        public async Task<IEnumerable<string>> GetWeChatifyAccountNamesAsync(List<long> accountIds)
        {
            DbConnectionString = _weChatifyConnectionString;
            var sql = @"SELECT AccountName FROM [dbo].WeChatAccounts(NoLock) WHERE AccountId in @accountIds";
            var result = await QueryAsync<string>(sql, new { @accountIds = accountIds });
            return result;
        }
    }
}
