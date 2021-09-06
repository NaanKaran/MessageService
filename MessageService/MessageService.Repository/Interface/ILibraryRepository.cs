using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;

namespace MessageService.Repository.Interface
{
    public interface ILibraryRepository
    {
        Task<int> AddAsync(MMSLibraryModel model);
        Task<(IEnumerable<MMSLibraryModel>, int)> GetAsync(GetLibraryModel model);
        Task<int> DeleteAsync(string id);
    }
}
