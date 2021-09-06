using System.Collections.Generic;

namespace MessageService.Models.MMSModels
{
    public class MMSThersholdNotifyUser
    {
        public string AccountName { get; set; }
        public long ThresholdCount { get; set; }
        public List<NotifyUserList> NotifyUserDetails { get; set; }
    }
}
