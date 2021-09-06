using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models
{
    public class NotifyUser
    {
        public long AccountId { get; set; }
        public byte NotificationType { get; set; }
        public List<string> NotifyUserIds { get; set; }
        public string ChangedBy { get; set; }
    }

    public class NotifyUserList
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int TotalCount { get; set; }
    }
}
