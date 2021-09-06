using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.DataExtensionModel
{
    public class DataExtensionSMSLogModel
    {
        public string Id { get; set; } // key
        public string ContactKey { get; set; } // key
        public string Entity { get; set; }    
        public string Type { get; set; }    
        public string SendStatus { get; set; }    
        public string SendDateTime { get; set; }    
        public string DeliveryStatus { get; set; }    
        public string DeliveryDateTime { get; set; }
        public string WeChatAccountId { get; set; }
        public string WeChatifyAccountId { get; set; }    
        public string WeChatifyAccountName { get; set; }    
        public string ActionName { get; set; }
        public string ActivityName { get; set; }
        public string UnionId { get; set; }    
        public string CreatedOn { get; set; }    

    }
}
