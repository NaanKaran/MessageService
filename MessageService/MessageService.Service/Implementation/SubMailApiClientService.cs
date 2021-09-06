using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MessageService.InfraStructure.APIUrls;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.SubmailModel;
using MessageService.Service.Interface;
using Newtonsoft.Json;

namespace MessageService.Service.Implementation
{
    public class SubMailApiClientService : ISubMailApiClientService
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public SubMailApiClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task<T> SendMMS<T>(SubmailMMSPostModel postModel, string contentType = "application/json")
        {
            var client = _httpClientFactory.CreateClient("SubMailClient");
            var content = new StringContent(postModel.ToJsonString(), Encoding.UTF8, contentType);
            using (var response = await client.PostAsync(SubmailAPIUrls.MMSXSendUrl, content))
            {
                var result =await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
        }
    }
}
