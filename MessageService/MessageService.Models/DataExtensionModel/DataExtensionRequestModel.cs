
using Newtonsoft.Json;

namespace MessageService.Models.DataExtensionModel
{

    public class DataExtensionRequestModel
    {
        [JsonProperty("attributes")]
        public Attribute[] Attributes { get; set; }
    }

    public class Attribute
    {

        [JsonProperty("key")]
        public string Key { get; set; }
    }


    public class Attribute2
    {

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class Value
    {

        [JsonProperty("items")]
        public string[] Items { get; set; }
    }

    public class Condition
    {

        [JsonProperty("attribute")]
        public Attribute2 Attribute { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public Value Value { get; set; }
    }

    public class ConditionSet
    {

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("conditionSets")]
        public object[] ConditionSets { get; set; }

        [JsonProperty("conditions")]
        public Condition[] Conditions { get; set; }
    }
}
