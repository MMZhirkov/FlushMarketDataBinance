using System;
using System.Collections.Generic;

namespace DataModel
{
    public class OrderBook
    {
        public int Id { get; set; }

        public string Symbol { get; set; }
        
        public List<Trade> Trades { get; set; }

        public DateTime TimeTrade { get; set; }
    }
}