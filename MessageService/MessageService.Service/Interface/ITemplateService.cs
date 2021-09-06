using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;
using MessageService.Models.SubmailModel;

namespace MessageService.Service.Interface
{
    public interface ITemplateService
    {
        Task<MmsTemplateResponseModel> SaveAsync(MMSTemplateModel mmsTemplateModel);
        Task<bool> UpdateTemplateId(TemplateUpdateModel templateUpdate);
        Task<PagingModel<MMSTemplateModel>> GetAsync(GetTemplateModel model);
        Task<MMSTemplateModel> GetAsync(string id, long accountId);
        Task<IEnumerable<TemplateJourneyDropDownModel>> GetActiveTemplatesAsync(long accountId);
        Task<int> DeleteTemplateAsync(string id, long accountId);

        Task<MmsTemplateResponseModel> UpdateAsync(MMSTemplateModel mmsTemplateModel);
    }
}
