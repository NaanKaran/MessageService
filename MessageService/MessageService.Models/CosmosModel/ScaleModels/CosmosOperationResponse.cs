using System.Collections.Generic;

namespace MessageService.Models.CosmosModel.ScaleModels
{
    public class CosmosOperationResponse
    {
        public bool Success { get; set; }
        public List<ScaleOperation> ScaleOperations { get; set; } = new List<ScaleOperation>();
        public int TotalRetries { get; set; }
    }
}
