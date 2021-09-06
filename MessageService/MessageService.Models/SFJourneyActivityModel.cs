using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MessageService.Models
{
    public class SFJourneyDetailsRoot
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
        [JsonProperty("items")]
        public List<SFJourneyDetailsModel> Items { get; set; }
    }
    
    public class OriginalDefintition
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("timeStamp")]
        public DateTime TimeStamp { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("versionNumber")]
        public int VersionNumber { get; set; }
        [JsonProperty("originalDefinitionId")]
        public string OriginalDefinitionId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("executionMode")]
        public string ExecutionMode { get; set; }
        [JsonProperty("publishStatus")]
        public string PublishStatus { get; set; }
        [JsonProperty("publishRequestId")]
        public string PublishRequestId { get; set; }
    }

    public class OriginalDefintitionRoot
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
        [JsonProperty("items")]
        public List<OriginalDefintition> Items { get; set; }
    }

    public class SFJourneyDetailsModel
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string JourneyName { get; set; }
        [JsonProperty("lastPublishedDate")] public DateTime LastPublishedDate { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("workflowApiVersion")] public string WorkflowApiVersion { get; set; }
        [JsonProperty("createdDate")] public DateTime CreatedDate { get; set; }
        [JsonProperty("modifiedDate")] public DateTime ModifiedDate { get; set; }
        [JsonProperty("activities")] public List<Activity> Activities { get; set; } = new List<Activity>();
        [JsonProperty("triggers")] public List<Trigger> Triggers { get; set; }
        [JsonProperty("goals")] public List<object> Goals { get; set; }
        [JsonProperty("exits")] public List<object> Exits { get; set; }
        [JsonProperty("stats")] public Stats Stats { get; set; }
        [JsonProperty("entryMode")] public string EntryMode { get; set; }
        [JsonProperty("defaults")] public Defaults Defaults { get; set; }
        [JsonProperty("metaData")] public MetaData4 MetaData { get; set; }
        [JsonProperty("executionMode")] public string ExecutionMode { get; set; }
        [JsonProperty("categoryId")] public string CategoryId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("definitionId")] public string DefinitionId { get; set; }

    }
    public class Arguments
    {
    }

    public class MetaData
    {
    }

    public class Outcome
    {
        public string key { get; set; }
        public string next { get; set; }
        public Arguments arguments { get; set; }
        public MetaData metaData { get; set; }
    }

    public class CustomData
    {
    }

    public class InArgumentSF
    {
        public string email { get; set; }
        public string contactKey { get; set; }
        public bool IsBuLoadRequired { get; set; }
        public string webhookUrl { get; set; }
        public string sendAccount { get; set; }
        public string sendBot { get; set; }
        public string sendKeyword { get; set; }
        public string enrtySource { get; set; }
        public string enrtySourceContactKey { get; set; }
        public string enrtySourcePhoneNumber { get; set; }
        public string enrtySourceId { get; set; }
        public string enrtySourceContactKeyId { get; set; }
        public string enrtySourcePhoneNumberId { get; set; }
        public string orgId { get; set; }
        public string phoneNumber { get; set; }
        public string journeyVersion { get; set; }
        public CustomData customData { get; set; }
        public string activityId { get; set; }
        public string actionName { get; set; }
    }

    public class Execute
    {
        public List<InArgumentSF> inArguments { get; set; }
        public List<object> outArguments { get; set; }
        public string url { get; set; }
        public string verb { get; set; }
        public string body { get; set; }
        public string header { get; set; }
        public string format { get; set; }
        public bool useJwt { get; set; }
        public int timeout { get; set; }
    }

    public class Arguments2
    {
        public string executionMode { get; set; }
        public string definitionId { get; set; }
        public string activityId { get; set; }
        public string contactKey { get; set; }
        public Execute execute { get; set; }
        public string testExecute { get; set; }
        public string startActivityKey { get; set; }
        public string definitionInstanceId { get; set; }
        public string requestObjectId { get; set; }
        public string waitEndDateAttributeDataBound { get; set; }
        public string waitDefinitionId { get; set; }
        public string waitForEventId { get; set; }
        public string waitQueueId { get; set; }
    }

    public class Save
    {
        public string url { get; set; }
        public string body { get; set; }
        public string verb { get; set; }
        public bool useJwt { get; set; }
    }

    public class Publish
    {
        public string url { get; set; }
        public string verb { get; set; }
        public string body { get; set; }
        public bool useJwt { get; set; }
    }

    public class Validate
    {
        public string url { get; set; }
        public string verb { get; set; }
        public string body { get; set; }
        public bool useJwt { get; set; }
    }

    public class ConfigurationArguments
    {
        public string applicationExtensionKey { get; set; }
        public string applicationExtensionId { get; set; }
        public Save save { get; set; }
        public string testSave { get; set; }
        public Publish publish { get; set; }
        public string testPublish { get; set; }
        public string unpublish { get; set; }
        public string stop { get; set; }
        public string testStop { get; set; }
        public string testUnpublish { get; set; }
        public string partnerActivityId { get; set; }
        public Validate validate { get; set; }
        public string testValidate { get; set; }
        public string outArgumentSchema { get; set; }
        public string executeSchema { get; set; }
        public string waitDuration { get; set; }
        public string waitUnit { get; set; }
        public string specifiedTime { get; set; }
        public string timeZone { get; set; }
        public string description { get; set; }
        public string waitEndDateAttributeExpression { get; set; }
        public string specificDate { get; set; }
        public string waitForEventKey { get; set; }
    }

    public class MetaData2
    {
        public bool isConfigured { get; set; }
        public string uiType { get; set; }
    }

    public class EndDate
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class WaitEndDateAttributeDataBound
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class WaitDefinitionId
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class WaitForEventId
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class ExecutionMode
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class StartActivityKey
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class WaitQueueId
    {
        public string dataType { get; set; }
        public bool isNullable { get; set; }
        public string direction { get; set; }
        public bool readOnly { get; set; }
        public string access { get; set; }
    }

    public class Arguments3
    {
        public EndDate endDate { get; set; }
        public WaitEndDateAttributeDataBound waitEndDateAttributeDataBound { get; set; }
        public WaitDefinitionId waitDefinitionId { get; set; }
        public WaitForEventId waitForEventId { get; set; }
        public ExecutionMode executionMode { get; set; }
        public StartActivityKey startActivityKey { get; set; }
        public WaitQueueId waitQueueId { get; set; }
    }

    public class Schema
    {
        public Arguments3 arguments { get; set; }
    }

    public class Activity
    {
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public List<Outcome> outcomes { get; set; }
        public Arguments2 arguments { get; set; }
        public ConfigurationArguments configurationArguments { get; set; }
        public MetaData2 metaData { get; set; }
        public Schema schema { get; set; }
    }

    public class Arguments4
    {
        public string startActivityKey { get; set; }
        public string dequeueReason { get; set; }
        public string lastExecutedActivityKey { get; set; }
        public string filterResult { get; set; }
    }

    public class ConfigurationArguments2
    {
        public string schemaVersionId { get; set; }
        public string criteria { get; set; }
        public string filterDefinitionId { get; set; }
    }

    public class MetaData3
    {
        public string sourceInteractionId { get; set; }
        public string eventDefinitionId { get; set; }
        public string eventDefinitionKey { get; set; }
        public string chainType { get; set; }
        public bool configurationRequired { get; set; }
        public string iconUrl { get; set; }
        public string title { get; set; }
        public string entrySourceGroupConfigUrl { get; set; }
    }

    public class Trigger
    {
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public List<object> outcomes { get; set; }
        public Arguments4 arguments { get; set; }
        public ConfigurationArguments2 configurationArguments { get; set; }
        public MetaData3 metaData { get; set; }
    }

    public class Stats
    {
        public string currentPopulation { get; set; }
        public string cumulativePopulation { get; set; }
        public string metGoal { get; set; }
        public string metExitCriteria { get; set; }
        public string goalPerformance { get; set; }
    }

    public class AnalyticsTracking
    {
        public bool enabled { get; set; }
        public string analyticsType { get; set; }
        public List<object> urlDomainsToTrack { get; set; }
    }

    public class Properties
    {
        public AnalyticsTracking analyticsTracking { get; set; }
    }

    public class Defaults
    {
        public List<string> mobileNumber { get; set; }
        public Properties properties { get; set; }
    }

    public class MetaData4
    {
    }


}
