using Newtonsoft.Json;

namespace MessageService.Models.DataExtensionModel
{
    public class DataExtenstionResultModel
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("requestServiceMessageID")]
        public string RequestServiceMessageId { get; set; }

        [JsonProperty("responseDateTime")]
        public string ResponseDateTime { get; set; }

        [JsonProperty("resultMessages")]
        public object[] ResultMessages { get; set; }

        [JsonProperty("serviceMessageID")]
        public string ServiceMessageId { get; set; }
    }

    public class DataExtenstionValue
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class Item
    {

        [JsonProperty("values")]
        public DataExtenstionValue[] Values { get; set; }
    }

    public class Links
    {
    }
}
