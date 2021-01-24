using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel.SettingsApp;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
	public class FlushMarketData : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
            using (var helper = new Helper())
            {
                //var dataMap = context.MergedJobDataMap;
                //var options = (DbContextOptions<OrderBookContext>)dataMap["options"];
                Settings.ProxyList = Settings.ProxyList.OrderBy(p => p.LastUseTime).ToList();
                var lastUsedProxy = Settings.ProxyList[0];
                Settings.ProxyList[0].LastUseTime = DateTime.Now;
                var binanceClient = new BinanceClient(new ClientConfiguration()
                {
                    ApiKey = Settings.ApiKey,
                    SecretKey = Settings.SecretKey
                });

                var httpClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    UseProxy = true,
                    Proxy = helper.GetProxy(lastUsedProxy)
                };

                var httpClient = new HttpClient();//httpClientHandler
                //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var n = await httpClient.GetStringAsync($"https://www.google.ru/");

                //var n = await httpClient.GetStringAsync($"https://api.binance.com/api/v3/depth?symbol=ETHUSDT&limit=500");

                var orderBooks = new Dictionary<string, OrderBookResponse>();

                await helper.FillListOrderBooks(binanceClient, orderBooks, httpClient);

                if (!orderBooks.Any())
                    return;

                helper.RecordOrderBooksInDB(orderBooks);
            }

            Console.WriteLine($"Task done, {DateTime.Now}");
		}
	}
}