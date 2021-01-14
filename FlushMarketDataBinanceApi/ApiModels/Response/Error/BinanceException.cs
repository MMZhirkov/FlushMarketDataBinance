using System;

namespace FlushMarketDataBinanceApi.ApiModels.Response.Error
{
    public class BinanceException: Exception
    {
        public BinanceError ErrorDetails { get; set; }

        public BinanceException(string message, BinanceError errorDetails):base(message)
        {
            ErrorDetails = errorDetails;
        }
    }
}