using FlushMarketDataBinanceModel.SettingsApp;
using Microsoft.EntityFrameworkCore;

namespace FlushMarketDataBinanceModel.DAL
{
    public class OrderBookContext : DbContext
    {
        public DbSet<OrderBook> OrderBooks { get; set; }

        public OrderBookContext() : base()
        {
             Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(Settings.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderBook>().ToTable("OrderBooks");
        }
    }
}