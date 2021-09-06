using System;
using System.IO;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using MessageService.Models;
using MessageService.Models.APIModels;
using MessageService.Models.MMSModels;
using MessageService.Repository.Interface;
using MessageService.Service.Interface;

namespace MessageService.Service.Implementation
{
    public class LibraryService : ILibraryService
    {
        private readonly IAzureStorageRepository _azureStorageRepository;
        private readonly ILibraryRepository _libraryRepository;
        public LibraryService(
            IAzureStorageRepository azureStorageRepository, 
            ILibraryRepository libraryRepository)
        {
            _azureStorageRepository = azureStorageRepository;
            _libraryRepository = libraryRepository;
        }

        public async Task<int> DeleteAsync(string id)
        {
            return await _libraryRepository.DeleteAsync(id);
        }

        public async Task<PagingModel<MMSLibraryModel>> GetAsync(GetLibraryModel model)
        {
            var (libraryModel, totalCount) = await _libraryRepository.GetAsync(model);

            return new PagingModel<MMSLibraryModel>()
            {
                ItemsPerPage = model.ItemsPerPage,
                PageNumber = model.PageNo,
                TotalCount = totalCount,
                Items = libraryModel
            };
        }

        public async Task<MMSLibraryModel> UploadFileToBlobAsync(FileUploadModel model)
        {
            Stream stream = model.File.OpenReadStream();

            string fileName = Path.GetFileNameWithoutExtension(model.File.FileName);
            string[] mimeType = model.File.ContentType.Split("/");
            MMSLibraryModel mmsLibraryModel = new MMSLibraryModel()
            {
                AccountId = model.AccountId,               
                Type = mimeType[0],
                Extension = mimeType[1],
                FileSize = model.File.Length,
                Filename= model.File.FileName

            };
            //string name = model.File.FileName;
            string name = mmsLibraryModel.Id.ToString();
            string blobUrl = await _azureStorageRepository.UploadStreamToBlobAsync(stream, model.AccountId, name);
            mmsLibraryModel.BlobUrl = blobUrl;
            mmsLibraryModel.Base64String = Convert.ToBase64String(stream.ReadToEnd());
            await _libraryRepository.AddAsync(mmsLibraryModel);
            return mmsLibraryModel;           
        }
    }
}
