using ExpensesTracker.Models.Enums;

namespace ExpensesTracker.Services.DTOs
{
    public class EntrySortModel
    {
        public EntrySortOptions SortBy { get; set; }
        public OrderOptions OrderBy { get; set; }
    }
}
