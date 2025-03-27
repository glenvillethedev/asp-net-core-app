using Microsoft.AspNetCore.Identity;

namespace ExpensesTracker.Models.IdentityEntities
{
    public class AppRole : IdentityRole<Guid>
    {
        public DateTime? CreateDt { get; set; }
        public DateTime? UpdateDt { get; set; }
    }
}
