using System.Net.Http;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceApi
{
    public interface IAPIProcessor
    {
        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        Task<T> ProcessGetRequest<T>(HttpClient httpClient, BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class;
    }
}