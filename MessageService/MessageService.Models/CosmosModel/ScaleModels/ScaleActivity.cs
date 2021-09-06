using System;

namespace MessageService.Models.CosmosModel.ScaleModels
{
    public class ScaleActivity
    {
        public ScaleActivity(string databaseName, string collectionName, int rU, DateTimeOffset scaleTime)
        {
            DatabaseName = databaseName;
            CollectionName = collectionName;
            RU = rU;
            ScaleTime = scaleTime;
        }

        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public int RU { get; set; }
        public DateTimeOffset ScaleTime { get; set; }
        public string MetaDataType { get; } = nameof(ScaleActivity);
    }
}
