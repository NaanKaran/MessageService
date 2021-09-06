namespace MessageService.Models.MMSModels
{
    public class RawMMSSendModel
    {
        public int AppId { get; set; }
        public string AppKey { get; set; }
        public long AccountId { get; set; }
        public string TemplateId { get; set; }
    }
}
