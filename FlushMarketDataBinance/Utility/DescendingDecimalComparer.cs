﻿using System.Collections.Generic;

namespace FlushMarketDataBinance.Utility
{
    internal class DescendingDecimalComparer : IComparer<decimal>
    {
        public int Compare(decimal x, decimal y) =>
            decimal.Compare(x, y) * -1;
    }
}
