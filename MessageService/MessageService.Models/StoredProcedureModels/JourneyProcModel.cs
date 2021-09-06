using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.StoredProcedureModels
{
    public class JourneyProcModel
    {
        public string ActivityId { get; set; }
        public string InteractionId { get; set; }
        public string Version { get; set; }
        public string JourneyId { get; set; }
        public string QuadrantInfo { get; set; }
        public string ActivityName { get; set; }
        public long AccountId { get; set; }
        public string JourneyKey { get; set; }
        public string JourneyName { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
