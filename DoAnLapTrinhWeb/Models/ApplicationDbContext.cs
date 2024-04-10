using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace DoAnLapTrinhWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions <ApplicationDbContext> options) : base(options)
        {
      
        }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }

    }
}
