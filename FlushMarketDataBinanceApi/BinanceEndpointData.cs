using FlushMarketDataBinanceApi.Enums;
using System;

namespace FlushMarketDataBinanceApi
{
    public class BinanceEndpointData
    {
        public Uri Uri;
        public EndpointSecurityType SecurityType;

        public BinanceEndpointData(Uri uri, EndpointSecurityType securityType)
        {
            Uri = uri;
            SecurityType = securityType;
        }

        public override string ToString()
        {
            return Uri.AbsoluteUri;
        }
    }
}