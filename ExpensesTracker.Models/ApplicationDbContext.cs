using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ExpensesTracker.Models
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public virtual DbSet<Entry> Entries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entry>().ToTable("Entries"); // map to table

            string jsonEntries = File.ReadAllText("entries.json");
            List<Entry>? entries = JsonSerializer.Deserialize<List<Entry>>(jsonEntries);
            if(entries != null)
            {
                modelBuilder.Entity<Entry>().HasData(entries); // seed data
            }
        }
    }
}
