﻿using DataModel;
using Microsoft.EntityFrameworkCore;

namespace FlushMarketDataBinanceConsole.Context
{
    public class OrderBookContext : DbContext
    {
        public DbSet<OrderBook> OrderBooks { get; set; }

        public OrderBookContext(DbContextOptions<OrderBookContext> options)
          : base(options)
        {
           
            Database.EnsureCreated();
        }
    }
}