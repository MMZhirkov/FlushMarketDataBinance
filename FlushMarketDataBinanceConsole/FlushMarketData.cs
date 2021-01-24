﻿using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
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
            using (var helper = new Helper())
            {
                var firstTimePair = Settings.ProxyList.OrderBy(s => s.Value.LastUseTime).FirstOrDefault();
                firstTimePair.Value.LastUseTime = DateTime.Now;

                var binanceClient = new BinanceClient(new ClientConfiguration()
                {
                    ApiKey = Settings.ApiKey,
                    SecretKey = Settings.SecretKey
                });

                var httpClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    UseProxy = true,
                    Proxy = new WebProxy(firstTimePair.Value.UriProxy)
                };

                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    var testResponse = await httpClient.GetStringAsync($"https://api.binance.com/api/v3/depth?symbol=ETHUSDT&limit=500");
                }
                catch (Exception ex)
                {

                    throw;
                }
               

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