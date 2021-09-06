using System.Collections.Generic;

namespace MessageService.Models.CosmosModel.ScaleModels
{
    public class BulkInsertOpeartionResult
    {
        public bool OperationSuccess { get; set; }
        public List<ScaleOperation> ScaleOperations { get; set; } = new List<ScaleOperation>();
        public string OperationFailReason { get; set; }
    }
}
