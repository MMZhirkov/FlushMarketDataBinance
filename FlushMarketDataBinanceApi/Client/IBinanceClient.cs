using FlushMarketDataBinanceApi.ApiModels.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceApi.Client
{
    public interface IBinanceClient
    {
        /// <summary>
        /// Gets or sets the Timestamp offset of the Binance Client
        /// </summary>
        TimeSpan TimestampOffset { get; set; }

        /// <summary>
        /// Test the connectivity to the API
        /// </summary>
        Task<EmptyResponse> TestConnectivity();

        /// <summary>
        /// Get the current server time (UTC)
        /// </summary>
        /// <returns><see cref="ServerTimeResponse"/></returns>
        Task<ServerTimeResponse> GetServerTime();

        /// <summary>
        /// Gets the current order book for the specified symbol
        /// </summary>
        /// <param name="symbol">The symbole to retrieve the order book for</param>
        /// <param name="useCache"></param>
        /// <param name="limit">Amount to request - defaults to 100</param>
        /// <returns></returns>
        Task<OrderBookResponse> GetOrderBook(string symbol, bool useCache = false, int limit = 100);

        /// <summary>
        /// Gets all prices for all symbols
        /// </summary>
        /// <returns></returns>
        Task<List<SymbolPriceResponse>> GetSymbolsPriceTicker();

        /// <summary>
        /// Gets the best and quantity on the order book for all symbols
        /// </summary>
        /// <returns></returns>
        Task<List<SymbolOrderBookResponse>> GetSymbolOrderBookTicker();

        /// <summary>
        /// Gets the current price for the provided symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Task<SymbolPriceResponse> GetPrice(string symbol);
    }
}
