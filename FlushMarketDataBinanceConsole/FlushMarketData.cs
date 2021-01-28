using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel;
using FlushMarketDataBinanceModel.SettingsApp;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
	public class FlushMarketData : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		 {
            Console.WriteLine($"Task start, {DateTime.Now}");

            using (var helper = new Helper())
            {
                var binanceClient = new BinanceClient(new ClientConfiguration()
                {
                    ApiKey = Settings.ApiKey,
                    SecretKey = Settings.SecretKey
                }); 
                
                var httpClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                };

                var keyFirstTimePair = string.Empty;
                var hasProxyList = Settings.ProxyList.Any();
                if (hasProxyList)
                {
                    var firstTimePair = Settings.ProxyList.OrderBy(s => s.Value.LastUseTime).FirstOrDefault();
                    firstTimePair.Value.LastUseTime = DateTime.Now;
                    keyFirstTimePair = firstTimePair.Key;

                    httpClientHandler.UseProxy = true;
                    httpClientHandler.Proxy = new WebProxy(firstTimePair.Value.UriProxy);
                }

                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = new TimeSpan(0,0,0,0,1000);

                var orderBooks = new Dictionary<string, OrderBook>();

                await helper.FillListOrderBooks(binanceClient, orderBooks, httpClient, keyFirstTimePair);
               
                if (!orderBooks.Any())
                    return;

                helper.RecordOrderBooksInDB(orderBooks);

                Console.WriteLine($"Task done, {DateTime.Now}, ProxyList.Any - {hasProxyList}");
            }
		}
	}
}