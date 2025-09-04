using Inflow.Models;
using Microsoft.EntityFrameworkCore;

namespace Inflow
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Account { get; set; }
    }
}
