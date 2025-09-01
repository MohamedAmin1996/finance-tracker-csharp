using Microsoft.EntityFrameworkCore;
using FinanceTrackerApp.Models;

namespace FinanceTrackerApp.Data
{
    public class FinanceDbContext : DbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().Property(t => t.Type).HasConversion<string>();
            modelBuilder.Entity<Transaction>().HasIndex(t => t.CreatedAt);
            modelBuilder.Entity<Transaction>().HasIndex(t => t.Category);
        }
    }
}
