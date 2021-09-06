using System.Collections.Generic;

namespace MessageService.Models.APIModels
{
    public class PagingModel<T> where T : class, new()
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
