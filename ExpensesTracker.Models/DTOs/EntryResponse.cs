using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.Enums;

namespace ExpensesTracker.Services.DTOs
{
    public class EntryResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Details { get; set; }
        public EntryCategoryOptions Category { get; set; }

        public override string ToString()
        {
            return $"{this.Id} | {this.Name} | {this.Amount} | {this.Date} | {this.Details}";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(EntryResponse)) return false;

            EntryResponse other = (EntryResponse)obj;
            return (this.Id == other.Id && this.Name == other.Name && this.Amount == other.Amount);
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }

    public static class EntryExtensionMethods
    {
        public static EntryResponse ToEntryResponse(this Entry entry)
        {
            return new EntryResponse { Id = entry.Id, Name = entry.Name, Amount = entry.Amount, Date = entry.Date, Details = entry.Details, Category = entry.Category };
        }
    }
}