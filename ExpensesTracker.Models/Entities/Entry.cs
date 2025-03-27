using ExpensesTracker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpensesTracker.Models.Entities
{
    public class Entry
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        [StringLength(100)]
        public string? Details { get; set; }
        public EntryCategoryOptions Category { get; set; }
        public DateTime? CreateDt { get; set; }
        public DateTime? UpdateDt { get; set; }
        public Guid UserId { get; set; }
    }
}
