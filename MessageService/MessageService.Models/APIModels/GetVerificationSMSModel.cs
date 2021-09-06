using System;
using System.ComponentModel.DataAnnotations;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{ 
    public class GetVerificationSMSModel
    {
        private DateTime? _toDate;

        [Required]
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("fromdate")]
        public DateTime? FromDate { get; set; }
        [JsonProperty("todate")]
        public DateTime? ToDate
        {
            get
            {
                if (_toDate.IsNull())
                {
                    return _toDate;
                }
                return _toDate.Value.Date.AddDays(1);
            }
            set
            {
                _toDate = value;
            }
        }
        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }
        //[JsonProperty("smscontent")]
        //public string SMSContent { get; set; }   
        [JsonProperty("followername")]
        public string FollowerName { get; set; }
        [JsonProperty("pageno")]
        public int PageNo { get; set; }
        [JsonProperty("itemsperpage")]
        public int ItemsPerPage { get; set; }
        [JsonProperty("sort")]
        public string Sort { get; set; } = "asc";
    }
}
