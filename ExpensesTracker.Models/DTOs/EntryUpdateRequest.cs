using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.Enums;

namespace ExpensesTracker.Services.DTOs
{
    public class EntryUpdateRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? Details { get; set; }
        public EntryCategoryOptions? Category { get; set; }
        public Guid UserId { get; set; }

        public Entry ToEntry()
        {
            return new Entry { Id = this.Id, Name = this.Name, Amount = this.Amount.GetValueOrDefault(), Date = this.Date.GetValueOrDefault(), Details = this.Details, Category = this.Category.GetValueOrDefault(), UpdateDt = DateTime.Now, UserId = this.UserId };
        }
    }
}
