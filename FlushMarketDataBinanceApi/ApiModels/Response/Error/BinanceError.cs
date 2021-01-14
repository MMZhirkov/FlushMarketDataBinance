using Newtonsoft.Json;

namespace FlushMarketDataBinanceApi.ApiModels.Response.Error
{
    public class BinanceError
    {
        public int Code { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }

        public string RequestMessage { get; set; }

        public override string ToString()
        {
            return $"{Code}: {Message}";
        }
    }
}
