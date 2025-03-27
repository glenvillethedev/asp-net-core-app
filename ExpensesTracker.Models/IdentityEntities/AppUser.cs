using Microsoft.AspNetCore.Identity;

namespace ExpensesTracker.Models.IdentityEntities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public DateTime? CreateDt { get; set; }
        public DateTime? UpdateDt { get; set; }
    }
}
