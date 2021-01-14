using System;
using System.Linq;
using FlushMarketDataBinanceApi.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace FlushMarketDataBinanceApi
{
    public static class Endpoints
    {

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        /// <summary>
        /// Defaults to V1
        /// </summary>

        /// <summary>
        /// Defaults to API binance domain (https)
        /// </summary>
        internal static string APIBaseUrl = "https://api.binance.com/api";

        /// <summary>
        /// Defaults to WAPI binance domain (https)
        /// </summary>
        internal static string WAPIBaseUrl = "https://api.binance.com/wapi";

        private static string APIPrefix { get; } = $"{APIBaseUrl}";
        private static string WAPIPrefix { get; } = $"{WAPIBaseUrl}";

        public static class UserStream
        {
            internal static string ApiVersion = "v3";

            /// <summary>
            /// Start a user data stream
            /// </summary>
            public static BinanceEndpointData StartUserDataStream => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream"), EndpointSecurityType.ApiKey);

            /// <summary>
            /// Ping a user data stream to prevent a timeout
            /// </summary>
            public static BinanceEndpointData KeepAliveUserDataStream(string listenKey)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                    EndpointSecurityType.ApiKey);
            }

            /// <summary>
            /// Close a user data stream to prevent
            /// </summary>
            public static BinanceEndpointData CloseUserDataStream(string listenKey)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                    EndpointSecurityType.ApiKey);
            }
        }

        public static class General
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Test connectivity to the Rest API.
            /// </summary>
            public static BinanceEndpointData TestConnectivity => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);

            /// <summary>
            /// Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public static BinanceEndpointData ServerTime => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/time"), EndpointSecurityType.None);

            /// <summary>
            /// Current exchange trading rules and symbol information.
            /// </summary>
            public static BinanceEndpointData ExchangeInfo => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);

        }

        public static class MarketData
        {
            internal static string ApiVersion = "v3";

            /// <summary>
            /// Gets the order book with all bids and asks
            /// </summary>
            public static BinanceEndpointData OrderBook(string symbol, int limit, bool useCache = false)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// Latest price for all symbols.
            /// </summary>
            public static BinanceEndpointData AllSymbolsPriceTicker => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/allPrices"), EndpointSecurityType.ApiKey);

            /// <summary>
            /// Best price/qty on the order book for all symbols.
            /// </summary>
            public static BinanceEndpointData SymbolsOrderBookTicker => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/allBookTickers"), EndpointSecurityType.ApiKey);
        }

        public static class MarketDataV3
        {
            internal static string ApiVersion = "v3";

            /// <summary>
            /// Current Price
            /// </summary>
            public static BinanceEndpointData CurrentPrice(string symbol)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/price?symbol={symbol}"),
                    EndpointSecurityType.None);
            }
        }
    }
}
