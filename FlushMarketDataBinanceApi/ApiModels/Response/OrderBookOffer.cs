using FlushMarketDataBinanceApi.Client;

namespace FlushMarketDataBinanceApi.ApiModels.Response
{
    public class OrderBookOffer : IResponse
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }
}