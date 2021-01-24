using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Utility;

namespace FlushMarketDataBinanceApi.Client
{
    /// <summary>
    /// The Binance Client used to communicate with the official Binance API. For more information on underlying API calls see:
    /// https://www.binance.com/restapipub.html
    /// </summary>
    public class BinanceClient : IBinanceClient
    {
        public TimeSpan TimestampOffset {
            get => _timestampOffset;
            set
            {
                _timestampOffset = value;
                RequestClient.SetTimestampOffset(_timestampOffset);
            }
        }
        private TimeSpan _timestampOffset;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly IAPIProcessor _apiProcessor;

        /// <summary>
        /// Create a new Binance Client based on the configuration provided
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="apiCache"></param>
        public BinanceClient(ClientConfiguration configuration, IAPIProcessor apiProcessor = null)
        {
            Guard.AgainstNull(configuration);
            Guard.AgainstNullOrEmpty(configuration.ApiKey);
            Guard.AgainstNull(configuration.SecretKey);

            _apiKey = configuration.ApiKey;
            _secretKey = configuration.SecretKey;
            RequestClient.SetTimestampOffset(configuration.TimestampOffset);
            RequestClient.SetRateLimiting(configuration.EnableRateLimiting);
            //RequestClient.SetAPIKey(_apiKey);
            if (apiProcessor == null)
            {
                _apiProcessor = new APIProcessor(_apiKey, _secretKey);
            }
            else
            {
                _apiProcessor = apiProcessor;
            }
        }

        #region General
        /// <summary>
        /// Test the connectivity to the API
        /// </summary>
        public async Task<EmptyResponse> TestConnectivity(HttpClient httpClient)
        {
            return await _apiProcessor.ProcessGetRequest<EmptyResponse>(httpClient, Endpoints.General.TestConnectivity);
        }

        /// <summary>
        /// Get the current server time (UTC)
        /// </summary>
        /// <returns><see cref="ServerTimeResponse"/></returns>
        public async Task<ServerTimeResponse> GetServerTime(HttpClient httpClient)
        {
            return await _apiProcessor.ProcessGetRequest<ServerTimeResponse>(httpClient, Endpoints.General.ServerTime);
        }
        #endregion

        #region Market Data
        /// <summary>
        /// Gets the current depth order book for the specified symbol
        /// </summary>
        /// <param name="symbol">The symbole to retrieve the order book for</param>
        /// <param name="useCache"></param>
        /// <param name="limit">Amount to request - defaults to 100</param>
        /// <returns></returns>
        public async Task<OrderBookResponse> GetOrderBook(HttpClient httpClient, string symbol, int limit = 100)
        {
            Guard.AgainstNull(symbol);
            if (limit > 1000)
            {
                throw new ArgumentException("When requesting the order book, you can't request more than 1000 at a time.", nameof(limit));
            }
            return await _apiProcessor.ProcessGetRequest<OrderBookResponse>(httpClient, Endpoints.MarketDataV3.OrderBook(symbol, limit));
        }

        /// <summary>
        /// Gets all prices for all symbols
        /// </summary>
        /// <returns></returns>
        public async Task<List<SymbolPriceResponse>> GetSymbolsPriceTicker(HttpClient httpClient)
        {
             return await _apiProcessor.ProcessGetRequest<List<SymbolPriceResponse>>(httpClient, Endpoints.MarketDataV1.AllSymbolsPriceTicker);
        }

        /// <summary>
        /// Gets the best and quantity on the order book for all symbols
        /// </summary>
        /// <returns></returns>
        public async Task<List<SymbolOrderBookResponse>> GetSymbolOrderBookTicker(HttpClient httpClient)
        {
             return await _apiProcessor.ProcessGetRequest<List<SymbolOrderBookResponse>>(httpClient, Endpoints.MarketDataV1.SymbolsOrderBookTicker);
        }

        /// <summary>
        /// Gets the price for the provided symbol.  This is lighter weight than the daily ticker
        /// data.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<SymbolPriceResponse> GetPrice(HttpClient httpClient, string symbol)
        {
            Guard.AgainstNull(symbol);

            return await _apiProcessor.ProcessGetRequest<SymbolPriceResponse>(httpClient, Endpoints.MarketDataV3.CurrentPrice(symbol));
        }
        #endregion
    }
}