using System.Runtime.Serialization;

namespace BinanceExchange.API.Models.Response
{
    /// <summary>
    ///     Acknowledge Response following a call to the Create Order endpoint
    /// </summary>
    [DataContract]
    public class AcknowledgeCreateOrderResponse : BaseCreateOrderResponse
    {
    }
}