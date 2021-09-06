using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;
using MessageService.Repository.Interface;
using MessageService.Repository.Utility;

namespace MessageService.Repository.Implementation
{
    public class LibraryRepository : BaseRepository, ILibraryRepository
    {
        private readonly string _weChatifyConnectionString;
        private readonly string _MMSConnectionString;

        public LibraryRepository(string wechatify, string messageService)
        {
            _weChatifyConnectionString = wechatify;
            _MMSConnectionString = messageService;
        }

        public async Task<int> AddAsync(MMSLibraryModel model)
        {
            string sql = @"INSERT INTO [dbo].[MMSLibrary]
                            ([Id]
                            ,[Type]
                            ,[Extension]
                            ,[BlobUrl]
                            ,[Base64String]
                            ,[AccountId]
                            ,[FileSize]
                            ,[CreatedOn]
                            ,[Filename])
                        VALUES
                            (@Id,
                            @Type,
                            @Extension,
                            @BlobUrl,
                            @Base64String,
                            @AccountId, 
                            @FileSize,
                            @CreatedOn,
                            @Filename)";

            return await ExecuteAsync(sql, model);
        }

        public async Task<int> DeleteAsync(string id)
        {
            string sql = @"DELETE FROM [dbo].[MMSLibrary] WHERE Id = @id";

            return await ExecuteAsync(sql, new { id });
        }

        public async Task<(IEnumerable<MMSLibraryModel>, int)> GetAsync(GetLibraryModel model)
        {
            var sql = @"SELECT *, TotalCount = COUNT(1) OVER() FROM dbo.[MMSLibrary] WHERE AccountId = @AccountId ";

            if(model.Types.IsNotNull() && model.Types.Count > 0)
            {
                sql += @" AND [Type] in @types ";
            }

            if (!string.IsNullOrWhiteSpace(model.Filename)) 
            {
                sql += @" AND [Filename] like '%'+@Filename+'%'";
            }

            sql += @" ORDER BY CreatedOn DESC
                    OFFSET ((@pageno - 1) * @itemsperpage) ROWS
                    FETCH NEXT @itemsperpage ROWS ONLY ";           
            var result = await QueryAsPagingAsync<MMSLibraryModel>(sql, model);
            return result;
        }
    }
}
