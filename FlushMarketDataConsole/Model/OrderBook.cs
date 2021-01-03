using System;
using System.Collections.Generic;
using System.Text;

namespace FlushMarketDataConsole.Model
{
    public class OrderBook
    {
        public long Id { get; set; }
        public string Bids { get; set; }
        public int Asks { get; set; }
    }
}
