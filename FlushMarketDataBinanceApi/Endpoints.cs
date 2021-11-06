using System;
using FlushMarketDataBinanceApi.Enums;

namespace FlushMarketDataBinanceApi
{
    public static class Endpoints
    {
        /// <summary>
        /// Defaults to API binance domain (https)
        /// </summary>
        internal static string APIBaseUrl = "https://www.binance.com";
        private static string APIPrefix { get; } = $"{APIBaseUrl}";

        public static class General
        {
            private static string ApiVersion = "api/v3";
            /// <summary>
            /// Test connectivity to the Rest API.
            /// </summary>
            public static BinanceEndpointData TestConnectivity => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);

            /// <summary>
            /// Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public static BinanceEndpointData ServerTime => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/time"), EndpointSecurityType.None);
        }

        public static class MarketDataV3
        {
            private static string ApiVersion = "/api/v3";
            /// <summary>
            /// Gets the order book with all bids and asks
            /// </summary>
            public static BinanceEndpointData OrderBook(string symbol, int limit)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// Gets 24 hour price change statistics for all currency pair symbols.
            /// </summary>
            public static BinanceEndpointData HR24()
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}{ApiVersion}/ticker/24hr"), EndpointSecurityType.None);
            }

            /// <summary>
            /// Gets 24 hour price change statistics for all currency pair symbols.
            /// </summary>
            public static BinanceEndpointData Klines(string symbol, string interval)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}{ApiVersion}/klines?symbol={symbol}&interval={interval}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// Current Price
            /// </summary>
            public static BinanceEndpointData CurrentPrice(string symbol)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/price?symbol={symbol}"),
                    EndpointSecurityType.None);
            }
        }

        public static class MarketDataV1
        {
            private static string ApiVersion = "api/v1";
            /// <summary>
            /// Latest price for all symbols.
            /// </summary>
            public static BinanceEndpointData AllSymbolsPriceTicker => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/allPrices"), EndpointSecurityType.ApiKey);

            /// <summary>
            /// Best price/qty on the order book for all symbols.
            /// </summary>
            public static BinanceEndpointData SymbolsOrderBookTicker => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/allBookTickers"), EndpointSecurityType.ApiKey);
        }

        public static class MarketDataFutureV1
        {
            private static string ApiVersion = "fapi/v1";
            /// <summary>
            /// Gets the order book with all bids and asks
            /// </summary>

            public static BinanceEndpointData OrderBook(string symbol, int limit)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None);
            }
        }
    }
}