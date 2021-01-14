using System;

namespace FlushMarketDataBinanceApi.Client
{
    public class ClientConfiguration
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public bool EnableRateLimiting { get; set; }
        public TimeSpan TimestampOffset { get; set; } = TimeSpan.FromMilliseconds(0);
        public int DefaultReceiveWindow { get; set; } = 5000;
    }
}