using MessageService.Models.SubmailModel;

namespace MessageService.Models.MMSModels
{
    public class MMSDeliveryReportModel
    {
        public int RetryCount { get; set; } = 0;

        public SubmailStatusPushModel Data { get; set; }
    }
}
