using FlushMarketDataConsole.Model;
using Microsoft.EntityFrameworkCore;

namespace FlushMarketDataConsole
{
    public class ApplicationContext : DbContext
    {
        public DbSet<OrderBook> OrderBooks { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OrderBooks;Trusted_Connection=True;");
        }
    }
}