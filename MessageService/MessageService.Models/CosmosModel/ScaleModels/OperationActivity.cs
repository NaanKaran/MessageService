using System;


namespace MessageService.Models.CosmosModel.ScaleModels
{
    public class OperationActivity
    {

        public OperationActivity(string databaseName, string collectionName, DateTimeOffset activityTime, ActivityStrength activityStrength)
        {
            DatabaseName = databaseName;
            CollectionName = collectionName;
            ActivityTime = activityTime;
            ActivityStrength = activityStrength;
        }

        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public DateTimeOffset ActivityTime { get; set; }
        public ActivityStrength ActivityStrength { get; set; }
        public string MetaDataType { get; } = nameof(OperationActivity);

        protected bool Equals(OperationActivity other)
        {
            return ActivityStrength == other.ActivityStrength;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationActivity)obj);
        }

        public override int GetHashCode()
        {
            return (int)ActivityStrength;
        }

        public static bool operator ==(OperationActivity left, OperationActivity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OperationActivity left, OperationActivity right)
        {
            return !Equals(left, right);
        }

    }
}
