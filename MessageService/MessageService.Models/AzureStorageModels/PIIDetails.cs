using Microsoft.WindowsAzure.Storage.Table;

namespace MessageService.Models.AzureStorageModels
{
    public class PIIDetails : TableEntity
    {

        public PIIDetails()
        {

        }

        public PIIDetails(string openId, string infoId)
        {
            PartitionKey = openId;
            RowKey = infoId;
        }

        public string OpenID { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }

        public bool IsMobileVerified { get; set; }

        public bool IsEmailVerified { get; set; }

        public string MobileVerificationCode { get; set; }

        public string EmailVerificationCode { get; set; }

        public string CountryCode { get; set; }

        public long? AccountID { get; set; }

    }
}
