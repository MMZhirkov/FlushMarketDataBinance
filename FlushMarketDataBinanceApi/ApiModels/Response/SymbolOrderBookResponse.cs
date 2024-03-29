using System.Runtime.Serialization;
using FlushMarketDataBinanceApi.Client;
using Newtonsoft.Json;

namespace FlushMarketDataBinanceApi.ApiModels.Response
{
    [DataContract]
    public class SymbolOrderBookResponse : IResponse
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public decimal BidPrice { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "bidQty")]
        public decimal BidQuantity { get; set; }

        [DataMember(Order = 4)]
        public decimal AskPrice { get; set; }

        [DataMember(Order = 5)]
        [JsonProperty(PropertyName = "askQty")]
        public decimal AskQuantity { get; set; }
    }
}