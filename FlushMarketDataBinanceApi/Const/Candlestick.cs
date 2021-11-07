using FlushMarketDataBinanceApi.Converter;
using Newtonsoft.Json;
using System;

namespace FlushMarketDataBinanceApi.Const
{
    [JsonConverter(typeof(ObjectToArrayConverter<Candlestick>))]
    public class Candlestick
    {
        [JsonProperty("openTime", Order = 1)]
        public long OpenTime { get; set; }
        [JsonProperty("open", Order = 2)]
        public double Open { get; set; }
        [JsonProperty("high", Order = 3)]
        public double High { get; set; }
        [JsonProperty("low", Order = 4)]
        public double Low { get; set; }
        [JsonProperty("close", Order = 5)]
        public double Close { get; set; }
        [JsonProperty("volume", Order = 6)]
        public decimal Volume { get; set; }
        [JsonProperty("closeTime", Order = 7)]
        public long CloseTime { get; set; }
        [JsonProperty("quoteAssetVolume", Order = 8)]
        public double QuoteAssetVolume { get; set; }
        [JsonProperty("numberTrades", Order = 9)]
        public long NumberTrades { get; set; }
        [JsonProperty("takerBase", Order = 10)]
        public double TakerBase { get; set; }
        [JsonProperty("takerQuote", Order = 11)]
        public double TakerQuote { get; set; }
        public DateTime Date => DateTimeOffset.FromUnixTimeMilliseconds(CloseTime).DateTime;
    }
}
