using NLog;
using System;
using System.Reflection;
using FlushMarketDataBinanceModel.SettingsApp;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Collections.Concurrent;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static HttpClient _httpClient = new HttpClient(
                                                new HttpClientHandler
                                                {
                                                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                                                });

        public static void Main(string[] args)
        {
            _logger.Info($"Запущен {MethodBase.GetCurrentMethod()}");

            Settings.InitConfig();

            var binanceClient = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });



            var orderBooks = new ConcurrentBag<OrderBook>();

            using (var helper = new Helper())
            {
                var lastDateTimeRecordDB = DateTime.Now;
                while (true)
                {
                    helper.FillListOrderBooks(binanceClient, orderBooks, _httpClient).GetAwaiter().GetResult();
                    if (!orderBooks.Any())
                        return;

                    if (lastDateTimeRecordDB.AddSeconds(25) < DateTime.Now)
                    {

                        helper.RecordOrderBooksInDB(orderBooks);
                        lastDateTimeRecordDB = DateTime.Now;
                    }
                }
            }

            _logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }
    }
}