using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;

namespace MessageService.Service.Interface
{
    public interface ILibraryService
    {
        Task<MMSLibraryModel> UploadFileToBlobAsync(FileUploadModel model);
        Task<PagingModel<MMSLibraryModel>> GetAsync(GetLibraryModel model);
        Task<int> DeleteAsync(string id);
    }
}
