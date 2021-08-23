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
using System.Threading.Tasks;
using System.Collections.Generic;

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
                var lastSendTimeRequest = DateTime.Now;
                while (true)
                {
                    try
                    {
                        if (lastDateTimeRecordDB.AddSeconds(3) < DateTime.Now)
                        {
                            var orderBooksOnRecord = new List<OrderBook>(orderBooks);
                            orderBooks.Clear();
                            var t = Task.Run(() => helper.RecordOrderBooksInDB(orderBooksOnRecord));
                            lastDateTimeRecordDB = DateTime.Now;
                            helper.FillListOrderBooks(binanceClient, orderBooks, _httpClient);
                            t.Wait();
                            continue;
                        }

                        if (lastSendTimeRequest.AddMilliseconds(500) < DateTime.Now)
                        {
                            helper.FillListOrderBooks(binanceClient, orderBooks, _httpClient);
                            lastSendTimeRequest = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message);
                        continue;
                    }
                }
            }

            _logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }
    }
}