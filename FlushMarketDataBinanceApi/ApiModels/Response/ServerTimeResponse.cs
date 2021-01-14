using System;
using System.Runtime.Serialization;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceApi.Converter;
using Newtonsoft.Json;

namespace FlushMarketDataBinanceApi.ApiModels.Response
{
    /// <summary>
    /// The current server time
    /// </summary>
    [DataContract]
    public class ServerTimeResponse: IResponse
    {
        [DataMember(Order = 1)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime ServerTime { get; set; }
    }
}