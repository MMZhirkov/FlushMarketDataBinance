using System.Runtime.Serialization;

namespace FlushMarketDataBinanceApi.Enums
{
    public enum NewOrderResponseType
    {
        [EnumMember(Value = "RESULT")]
        Result,
        [EnumMember(Value = "ACK")]
        Acknowledge,
        [EnumMember(Value = "FULL")]
        Full,
    }
}