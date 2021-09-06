using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.InfraStructure.APIUrls;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.Enum;
using MessageService.Models.MMSModels;
using MessageService.Models.SubmailModel;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ISettingsRepository _settingsRepository;
        public TemplateService(ITemplateRepository templateRepository, ISettingsRepository settingsRepository)
        {
            _templateRepository = templateRepository;
            _settingsRepository = settingsRepository;
        }
        public async Task<MmsTemplateResponseModel> SaveAsync(MMSTemplateModel mmsTemplateModel)
        {

            var data = await GetSubmailMmsTemplateModel(mmsTemplateModel);

            var response =
                await HttpHelper<MmsTemplateResponseModel>.HttpPostAsync(SubmailAPIUrls.MMSTemplateUrl, data.ToJsonString());

            if (response.Status == "success")
            {
                mmsTemplateModel.Id = response.TemplateId;
                await _templateRepository.SaveAsync(mmsTemplateModel).ConfigureAwait(false);
            }

            return response;
        }

        public async Task<MmsTemplateResponseModel> UpdateAsync(MMSTemplateModel mmsTemplateModel)
        {

            var data = await GetSubmailMmsTemplateModel(mmsTemplateModel);

            var response =
                await HttpHelper<MmsTemplateResponseModel>.HttpPutAsync(SubmailAPIUrls.MMSTemplateUrl, data.ToJsonString());

            if (response.Status == "success")
            {
                mmsTemplateModel.Id = mmsTemplateModel.Id;
                await _templateRepository.SaveAsync(mmsTemplateModel).ConfigureAwait(false);
            }

            return response;
        }


        private async Task<SubmailMmsTemplateModel> GetSubmailMmsTemplateModel(MMSTemplateModel mmsTemplateModel)
        {
            //var pages = mmsTemplateModel.ContentList;
            //var templateData = new List<MmsContentModel>();
            //foreach (var content in pages)
            //{
            //   // var name = content.BlobUrl.Split('/').LastOrDefault();
            //   // var base64String = await _azureStorageRepository.GetBlobFileAsBase64Async(mmsTemplateModel.AccountId, name);
            //    AppendData(content, templateData);
            //}

            var vendorSetting = await _settingsRepository.GetMMSVendorSettingsAsync(mmsTemplateModel.AccountId);

            var data = new SubmailMmsTemplateModel()
            {
                AppId = vendorSetting.AppId,
                AppKey = vendorSetting.AppKey,
                Content = mmsTemplateModel.Content,
                Signature = mmsTemplateModel.Signature,
                Title = mmsTemplateModel.Title,
                TemplateName = mmsTemplateModel.TemplateName,
                TemplateId = mmsTemplateModel.Id
            };
            return data;
        }

        private static void AppendData(MMSContent content, List<MmsContentModel> templateData)
        {
            switch (content.Type)
            {
                case MediaType.text:
                    templateData.Add(new MmsContentModel()
                    {
                        TextMessage = content.Text
                    });
                    break;
                case MediaType.image:
                    templateData.Add(new MmsImageContentModel()
                    {
                        TextMessage = content.Text,
                        Image = new Base64ContentModel
                        {
                            Type = content.FileType+"/"+content.Extension,
                            Base64String = content.Base64String
                        }
                    });
                    break;
                case MediaType.audio:
                    templateData.Add(new MmsAudioContentModel()
                    {
                        TextMessage = content.Text,
                        Audio = new Base64ContentModel
                        {
                            Type = content.FileType + "/" + content.Extension,
                            Base64String = content.Base64String
                        }
                    });
                    break;
                case MediaType.video:
                    break;
            }
        }

        public async Task<bool> UpdateTemplateId(TemplateUpdateModel templateUpdate)
        {
            return await _templateRepository.UpdateTemplateStatusAsync(templateUpdate).ConfigureAwait(false);
        }

        public async Task<PagingModel<MMSTemplateModel>> GetAsync(GetTemplateModel model)
        {
            var (templateModel, totalCount) = await _templateRepository.GetAsync(model);            

            return new PagingModel<MMSTemplateModel>()
            {
                ItemsPerPage = model.ItemsPerPage,
                PageNumber = model.PageNo,
                TotalCount = totalCount,
                Items = templateModel
            };
        }

        public async Task<MMSTemplateModel> GetAsync(string id, long accountId)
        {
           return await _templateRepository.GetAsync(id, accountId);           
        }

        public async Task<IEnumerable<TemplateJourneyDropDownModel>> GetActiveTemplatesAsync(long accountId)
        {
            return await _templateRepository.GetActiveTemplatesAsync(accountId);
        }

        public async Task<int> DeleteTemplateAsync(string id, long accountId)
        {
            var vendorSetting = await _settingsRepository.GetMMSVendorSettingsAsync(accountId);


            var data = new Dictionary<string,string>()
            {
                {"appid",vendorSetting.AppId },
                {"signature", vendorSetting.AppKey },
                {"template_id", id}
            };

           var response = await HttpHelper<MmsTemplateResponseModel>.HttpDeleteAsync(SubmailAPIUrls.MMSTemplateUrl, data);

            return await _templateRepository.DeleteTemplateAsync(id, accountId);
        }
    }
}
