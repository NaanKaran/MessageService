using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.DataExtensionModel
{
   public class DataExtensionIncomingSMSLog
    {
        public string Id { get; set; }
        public string Entity { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string DateTime { get; set; }
        public string WeChatAccountId { get; set; }
        public long WeChatifyAccountId { get; set; }
        public string WeChatifyAccountName { get; set; }
        public string UnionId { get; set; }
    }

   public class DataExtensionOptOutSMSLog
   {
       public string Entity { get; set; }
       public string Type { get; set; }
       public string DateTime { get; set; }
       public string WeChatAccountId { get; set; }
       public long WeChatifyAccountId { get; set; }
       public string WeChatifyAccountName { get; set; }
       public string ActionName { get; set; }
       public string ContactKey { get; set; }
       public string UnionId { get; set; }
   }
}
