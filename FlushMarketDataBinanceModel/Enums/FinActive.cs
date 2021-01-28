using System.Runtime.Serialization;

namespace FlushMarketDataBinanceModel.Enums
{
    public enum FinActive
    {
        [EnumMember(Value = "FUTURE")]
        Future,
        [EnumMember(Value = "STOCK")]
        Stock,
    }
}
