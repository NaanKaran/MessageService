using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Models.APIModels
{
    public class SFAttribute
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class SFRequest
    {
        [JsonProperty("attributes")]
        public List<SFAttribute> Attributes { get; set; } = new List<SFAttribute>();
    }

    public class SFValue
    {
        [JsonProperty("items")]
        public List<string> Items { get; set; }
    }

    public class SFCondition
    {
        [JsonProperty("attribute")]
        public SFAttribute Attribute { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("value")]
        public SFValue Value { get; set; }
    }

    public class SFConditionSet
    {
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("conditionSets")]
        public List<object> ConditionSets { get; set; }
        [JsonProperty("conditions")]
        public List<SFCondition> conditions { get; set; } = new List<SFCondition>();
    }

    public class SalesForceSearch
    {
        [JsonProperty("request")]
        public SFRequest Request { get; set; } = new SFRequest();
        [JsonProperty("conditionSet")]
        public SFConditionSet ConditionSet { get; set; } = new SFConditionSet();
    }

    public class SFResponseValue
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

    public class SFResponseItem
    {
        [JsonProperty("values")]
        public List<SFResponseValue> Values { get; set; }
    }

    public class SFSearchResponse : SFResponse
    {
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("items")]
        public List<SFResponseItem> Items { get; set; }
    }

    public class SFResponse
    {
        [JsonProperty("documentation")]
        public string Documentation { get; set; }

        [JsonProperty("errorcode")]
        public int? ErrorCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        
    }
}
