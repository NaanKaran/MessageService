namespace MessageService.Models.CosmosModel.ScaleModels
{
    public class ActiveCollection 
    {
        public ActiveCollection(string databaseName, string collectionName, int minimumRU)
        {
            DatabaseName = databaseName;
            CollectionName = collectionName;
            MinimumRU = minimumRU;
        }

        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public int MinimumRU { get; set; }
        public string MetaDataType { get; } = nameof(ActiveCollection);
    }
}
