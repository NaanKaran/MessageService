using System;
using MessageService.InfraStructure.Helpers;

namespace MessageService.Models.MMSModels
{
    public class MMSLogTableInfoModel
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int Year { get; set; }
        public int QuadrantNumber { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
    }
}
