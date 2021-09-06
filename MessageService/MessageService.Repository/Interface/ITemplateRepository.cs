using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;

namespace MessageService.Repository.Interface
{
    public interface ITemplateRepository
    {
        Task<bool> SaveAsync(MMSTemplateModel mmsTemplateModel);
        Task<bool> UpdateTemplateStatusAsync(TemplateUpdateModel templateUpdate);
        Task<(IEnumerable<MMSTemplateModel>, int)> GetAsync(GetTemplateModel model);
        Task<MMSTemplateModel> GetAsync(string id, long accountid);
        Task<IEnumerable<TemplateJourneyDropDownModel>> GetActiveTemplatesAsync(long accountid);
        Task<int> DeleteTemplateAsync(string id, long accountid);
    }
}
