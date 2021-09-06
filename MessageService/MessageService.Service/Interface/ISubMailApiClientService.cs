using System.Threading.Tasks;
using MessageService.Models.SubmailModel;

namespace MessageService.Service.Interface
{
    public interface ISubMailApiClientService
    {
        Task<T> SendMMS<T>(SubmailMMSPostModel postModel, string contentType = "application/json");
    }
}
