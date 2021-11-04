using System.Net.Http;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceApi
{
    public interface IAPIProcessor
    {

        /// <summary>
        /// Delegate for the messages returned by the websockets.
        /// </summary>
        /// <typeparam name="T">Type used to parsed the response message.</typeparam>
        /// <param name="messageData">Websocket response data.</param>
        public delegate void MessageHandler<T>(T messageData);

        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        Task<T> ProcessGetRequest<T>(HttpClient httpClient, BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class;

        /// <summary>
        /// Connects to a Websocket endpoint.
        /// </summary>
        /// <typeparam name="T">Type used to parsed the response message.</typeparam>
        /// <param name="parameters">Paremeters to send to the Websocket.</param>
        /// <param name="messageDelegate">Deletage to callback after receive a message.</param>
        /// <param name="useCustomParser">Specifies if needs to use a custom parser for the response message.</param>
        void ConnectToWebSocket<T>(string parameters, MessageHandler<T> messageDelegate, bool useCustomParser = false);
    }
}