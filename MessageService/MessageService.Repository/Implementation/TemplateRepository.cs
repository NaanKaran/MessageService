using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;

namespace MessageService.Repository.Implementation
{
    public class TemplateRepository :BaseRepository, ITemplateRepository
    {
        private readonly string _weChatifyConnectionString;
        private readonly string _MMSConnectionString;

        public TemplateRepository(string wechatify, string messageService)
        {
            _weChatifyConnectionString = wechatify;
            _MMSConnectionString = messageService;
        }
        public async Task<bool> SaveAsync(MMSTemplateModel mmsTemplateModel)
        {
            var inputData = new
            {
                mmsTemplateModel.AccountId,
                mmsTemplateModel.Id,
                mmsTemplateModel.TemplateName,
                mmsTemplateModel.Signature,
                mmsTemplateModel.Title,
                mmsTemplateModel.Content,
                mmsTemplateModel.Status,
                mmsTemplateModel.Variables,
                mmsTemplateModel.IsDeleted,
                mmsTemplateModel.CreatedBy,
                mmsTemplateModel.CreatedOn
            };
            var dynamicParams = new DynamicParameters();
            dynamicParams.AddDynamicParams(inputData);
            var result = await ExecuteProcAsync(StoredProcedure.MMSTemplateUpsert, dynamicParams);
            return result != 0;
        }

        public async Task<(IEnumerable<MMSTemplateModel>, int)> GetAsync(GetTemplateModel model)
        {

            var dynamicParams = new DynamicParameters();

            var statusTable = new DataTable("dbo.StatusUserDefineType");
            statusTable.Columns.Add("Id", typeof(short));

            foreach (var status in model.Status)
            {
                statusTable.Rows.Add(status);
            }

            var inputData = new
            {
                model.AccountId,
                model.TemplateName,
                model.ItemsPerPage,
                model.PageNo,
                Status = statusTable
            };
            dynamicParams.AddDynamicParams(inputData);

            var result = await QueryProcAsPagingListAsync<MMSTemplateModel>(StoredProcedure.MMSGetTemplates,dynamicParams);
            return result;

        }

        public async Task<bool> UpdateTemplateStatusAsync(TemplateUpdateModel templateUpdate)
        {
            var sql = @"Update [dbo].MMSTemplate set Comments = @comments, Status = @status, UpdatedOn = @updatedOn, UpdatedBy = @updatedBy Where Id = @TemplateId";
            var result = await ExecuteAsync(sql, templateUpdate);
            return (result != 0);
        }

        public async Task<MMSTemplateModel> GetAsync(string id, long accountId)
        {
            var sql = @"SELECT * FROM dbo.[MMSTemplate] WHERE Id = @id ;";
            return await QueryFirstOrDefaultAsync<MMSTemplateModel>(sql, new { id, accountId });
        }

        public async Task<IEnumerable<TemplateJourneyDropDownModel>> GetActiveTemplatesAsync(long accountId)
        {
            var sql = @"SELECT * FROM dbo.[MMSTemplate] WHERE AccountId = @accountId and Status = @status AND IsDeleted = 0;";

            return await QueryAsync<TemplateJourneyDropDownModel>(sql, new { accountId, @status = TemplateStatus.Approved });
        }

        public async Task<int> DeleteTemplateAsync(string id, long accountId)
        {
            var sql = @"Update [dbo].MMSTemplate SET IsDeleted = 1 WHERE Id= @id AND AccountId = @accountId";
            var result = await ExecuteAsync(sql, new { id , accountId });
            return result;
        }
    }
}