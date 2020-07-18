﻿using BinanceExchange.API.Enums;
using System;

namespace FlushMarketDataBinance.Market
{
    public class Quote
    {
        public Quote(decimal price, decimal volume, OrderSide direction)
        {
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));
            if (volume <= 0) throw new ArgumentOutOfRangeException(nameof(volume));

            Price = price;
            Volume = volume;
            Direction = direction;
        }

        /// <summary>
        /// Quote price
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Quote volume
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Direction (buy or sell)
        /// </summary>
        public OrderSide Direction { get; }
    }
}
