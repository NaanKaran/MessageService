using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.StoredProcedureModels
{
    public class IncomingMessageProcModel
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MobileNumber { get; set; }
        public object JourneyName { get; set; }
        public bool IsOptOut { get; set; }
    }
}
