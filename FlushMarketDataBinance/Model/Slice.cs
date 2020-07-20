using System;
using System.Collections.Generic;
using System.Text;

namespace FlushMarketDataBinance.Model
{
    public class Slice
    {
        public long LastUpdate { get; set; }
        public string Bids { get; set; }
        public string Asks { get; set; }

        public Slice(long lastUpdate, string bids, string asks)
        {
            LastUpdate = lastUpdate;
            Bids = bids;
            Asks = asks;
        }
    }
}
