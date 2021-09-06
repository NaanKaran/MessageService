using System;

namespace MessageService.Models.CosmosModel.ScaleModels
{
    public class ScaleOperation
    {
        public int ScaledFrom { get; set; }
        public int ScaledTo { get; set; }
        public DateTimeOffset OperationTime { get; set; } = DateTime.Now;
        public bool ScaledSuccess { get; set; } = true;
        public string ScaleFailReason { get; set; }
    }
}
