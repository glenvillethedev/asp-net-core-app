using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.Enums;

namespace ExpensesTracker.Services.DTOs
{
    public class EntryAddRequest
    {
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? Details { get; set; }
        public EntryCategoryOptions? Category { get; set; }
        public Guid UserId { get; set; }

        public Entry ToEntry()
        {
            return new Entry { Name = this.Name, Amount = this.Amount.GetValueOrDefault(), Date = this.Date.GetValueOrDefault(), Details = this.Details, Category = this.Category.GetValueOrDefault(), CreateDt = DateTime.Now, UserId = this.UserId };
        }
    }
}
