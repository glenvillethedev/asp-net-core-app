using ExpensesTracker.Models.Enums;

namespace ExpensesTracker.Services.DTOs
{
    public class EntrySearchModel
    {
        public string? SearchString { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public EntryCategoryOptions? Category { get; set; }
        public Guid UserId { get; set; }
        public int TotalRecords { get; set; }
        public string? CurrentRecords { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public List<object> PageList { get; set; } = new List<object>();
        public bool PagesExceedLimit { get; set; }
        public bool IsPagination { get; set; }
    }
}
