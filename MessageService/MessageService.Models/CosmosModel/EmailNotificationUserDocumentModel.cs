using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class EmailNotificationUserDocumentModel
    {
        protected bool Equals(EmailNotificationUserDocumentModel other)
        {
            return string.Equals(UserId, other.UserId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmailNotificationUserDocumentModel) obj);
        }

        public override int GetHashCode()
        {
            return (UserId != null ? UserId.GetHashCode() : 0);
        }

        public static bool operator ==(EmailNotificationUserDocumentModel left, EmailNotificationUserDocumentModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmailNotificationUserDocumentModel left, EmailNotificationUserDocumentModel right)
        {
            return !Equals(left, right);
        }

        [JsonProperty("userid")] public string UserId { get; set; }
        [JsonProperty("username")] public string UserName { get; set; }
        [JsonProperty("role")] public string Role { get; set; }
        [JsonProperty("emailid")] public string EmailId { get; set; }
    }
}
