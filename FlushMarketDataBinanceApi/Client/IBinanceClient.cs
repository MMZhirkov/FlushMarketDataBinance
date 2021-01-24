using FlushMarketDataBinanceApi.ApiModels.Response;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
        Task<EmptyResponse> TestConnectivity(HttpClient httpClient);

        /// <summary>
        /// Get the current server time (UTC)
        /// </summary>
        /// <returns><see cref="ServerTimeResponse"/></returns>
        Task<ServerTimeResponse> GetServerTime(HttpClient httpClient);

        /// <summary>
        /// Gets the current order book for the specified symbol
        /// </summary>
        /// <param name="symbol">The symbole to retrieve the order book for</param>
        /// <param name="limit">Amount to request</param>
        /// <returns></returns>
        Task<OrderBookResponse> GetOrderBook(HttpClient httpClient, string symbol, int limit);

        /// <summary>
        /// Gets all prices for all symbols
        /// </summary>
        /// <returns></returns>
        Task<List<SymbolPriceResponse>> GetSymbolsPriceTicker(HttpClient httpClient);

        /// <summary>
        /// Gets the best and quantity on the order book for all symbols
        /// </summary>
        /// <returns></returns>
        Task<List<SymbolOrderBookResponse>> GetSymbolOrderBookTicker(HttpClient httpClient);

        /// <summary>
        /// Gets the current price for the provided symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        Task<SymbolPriceResponse> GetPrice(HttpClient httpClient, string symbol);
    }
}
