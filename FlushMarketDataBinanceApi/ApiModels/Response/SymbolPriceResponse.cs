using FlushMarketDataBinanceApi.Client;
using System.Runtime.Serialization;

namespace FlushMarketDataBinanceApi.ApiModels.Response
{
    /// <summary>
    /// Symbol price information
    /// </summary>
    [DataContract]
    public class SymbolPriceResponse : IResponse
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public decimal Price { get; set; }
    }
}