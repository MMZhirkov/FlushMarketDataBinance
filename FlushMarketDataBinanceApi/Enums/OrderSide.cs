using System.Runtime.Serialization;

namespace FlushMarketDataBinanceApi.Enums
{
    public enum OrderSide
    {
        [EnumMember(Value = "BUY")]
        Buy,
        [EnumMember(Value = "SELL")]
        Sell,
    }
}