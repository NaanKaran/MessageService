using System;

namespace MessageService.Models.MMSModels
{
    public class MMSErrorCodeModel
    {
        public int Id { get; set; }
        public string EnglishDescription { get; set; }
        public string ChineseDescription { get; set; }
        public string ErrorCode { get; set; }
        public int VendorId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
