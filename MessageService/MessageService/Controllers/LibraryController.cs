using System;
using System.Threading.Tasks;
using MessageService.ActionFilters;
using MessageService.Models.APIModels;
using MessageService.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageService.Controllers
{
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryService _libraryService;
        private readonly ILogger<LibraryController> _log;
        public LibraryController(ILibraryService libraryService, ILogger<LibraryController> logger)
        {
            _libraryService = libraryService;
            _log = logger;
        }

        [HttpPost]
        //[ValidateModel]
        [Route("api/[controller]/SaveFile")]
        public async Task<IActionResult> SaveFile([FromForm]FileUploadModel model)
        {
            try
            {
                var result = await _libraryService.UploadFileToBlobAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/DeleteFile")]
        public async Task<IActionResult> DeleteFile([FromForm]string id)
        {
            try
            {
                var result = await _libraryService.DeleteAsync(id);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetLibrary")]
        [ValidateModel]
        public async Task<IActionResult> GetLibrary(GetLibraryModel model)
        {
            try
            {
                var result = await _libraryService.GetAsync(model);
                return Ok(APIResponse.Success(result));
            }
            catch (AggregateException agg)
            {
                _log.LogError(agg.Flatten(), agg.Flatten().Message);
                return Ok(APIResponse.Error(agg.Flatten().Message));
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                return Ok(APIResponse.Error(e.Message));
            }
        }
    }
}