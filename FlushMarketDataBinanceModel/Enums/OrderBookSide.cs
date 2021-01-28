using System.Runtime.Serialization;

namespace FlushMarketDataBinanceModel.Enums
{
    public enum OrderBookSide
    {
        [EnumMember(Value = "BUY")]
        Buy,
        [EnumMember(Value = "SELL")]
        Sell,
    }
}