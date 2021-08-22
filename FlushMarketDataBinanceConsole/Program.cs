using NLog;
using System;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using FlushMarketDataBinanceModel.SettingsApp;
using FlushMarketDataBinanceApi.Client;
using System.Collections.Generic;
using FlushMarketDataBinanceModel;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using System.Linq;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            _logger.Info($"Запущен {MethodBase.GetCurrentMethod()}");

            Settings.InitConfig();

            var binanceClient = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };


            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var orderBooks = new Dictionary<string, OrderBook>();

            using (var helper = new Helper())
            {
                while (true)
                {
                    helper.FillListOrderBooks(binanceClient, orderBooks, httpClient).GetAwaiter().GetResult();
                    if (!orderBooks.Any())
                        return;

                    //helper.RecordOrderBooksInDB(orderBooks);

                    orderBooks.Clear();
                    Console.WriteLine($"Task done, {DateTime.Now}");
                }
            }

            _logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }
    }
}