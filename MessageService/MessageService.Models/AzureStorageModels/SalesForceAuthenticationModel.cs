using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Models.AzureStorageModels
{

    public class SalesForceAuthenticationModel : TableEntity
    {
        public SalesForceAuthenticationModel()
        {

        }

        public SalesForceAuthenticationModel(string organizationId, string infoId)
        {
            this.PartitionKey = organizationId;
            this.RowKey = infoId;
        }


        public string OrganizationId { get; set; }

        public string AccessToken { get; set; }

        public string LegacyToken { get; set; }

        public string RefreshToken { get; set; }

        public string AuthEndPoint { get; set; }

        public bool IsUnderProcessing { get; set; } = false;

        public DateTime UpdatedTime { get; set; }

        public bool IsLegacyEndPoint { get; set; }

        public string SoapEndPointUrl { get; set; }

        public string RestDomainUrl { get; set; }

    }
}
