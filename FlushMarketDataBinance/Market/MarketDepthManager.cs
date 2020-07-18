using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Websockets;
using System;
using System.Threading.Tasks;

namespace FlushMarketDataBinance.Market
{
    public class MarketDepthManager
    {
        private readonly IBinanceRestClient _restClient;
        private readonly IBinanceWebSocketClient _webSocketClient;

        /// <summary>
        /// Create instance of <see cref="MarketDepthManager"/>
        /// </summary>
        /// <param name="binanceRestClient">Binance REST client</param>
        /// <param name="webSocketClient">Binance WebSocket client</param>
        /// <exception cref="ArgumentNullException"><paramref name="binanceRestClient"/> cannot be <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="webSocketClient"/> cannot be <see langword="null"/></exception>
        public MarketDepthManager(IBinanceRestClient binanceRestClient, IBinanceWebSocketClient webSocketClient)
        {
            _restClient = binanceRestClient ?? throw new ArgumentNullException(nameof(binanceRestClient));
            _webSocketClient = webSocketClient ?? throw new ArgumentNullException(nameof(webSocketClient));
        }

        /// <summary>
        /// Build <see cref="MarketDepth"/>
        /// </summary>
        /// <param name="marketDepth">Market depth</param>
        /// <param name="limit">Limit of returned orders count</param>
        public async Task BuildAsync(MarketDepth marketDepth, int limit = 100)
        {
            if (marketDepth == null)
                throw new ArgumentNullException(nameof(marketDepth));
            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            OrderBookResponse orderBook = await _restClient.GetOrderBookAsync(marketDepth.Symbol, false, limit);

            marketDepth.UpdateDepth(orderBook.Asks, orderBook.Bids, orderBook.LastUpdateId);
        }

        /// <summary>
        /// Stream <see cref="MarketDepth"/> updates
        /// </summary>
        /// <param name="marketDepth">Market depth</param>
        public void StreamUpdates(MarketDepth marketDepth)
        {
            if (marketDepth == null)
                throw new ArgumentNullException(nameof(marketDepth));

            _webSocketClient.ConnectToDepthWebSocket(
                marketDepth.Symbol,
                marketData => marketDepth.UpdateDepth(marketData.AskDepthDeltas, marketData.BidDepthDeltas, marketData.UpdateId));
        }
    }
}
