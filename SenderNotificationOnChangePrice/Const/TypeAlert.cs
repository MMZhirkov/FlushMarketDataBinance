using System.Runtime.Serialization;

namespace SenderNotificationOnChangePrice.Const
{
    public enum TypeAlert
    {
        [EnumMember(Value = "BigVolume")] BigVolume,
        [EnumMember(Value = "ChangePriceProcent")] ChangePriceProcent,
        [EnumMember(Value = "BigOrderbook")] BigOrderbook,
    }
}