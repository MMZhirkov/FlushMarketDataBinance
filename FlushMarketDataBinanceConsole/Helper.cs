using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel;
using FlushMarketDataBinanceModel.DAL;
using FlushMarketDataBinanceModel.DB;
using FlushMarketDataBinanceModel.SettingsApp;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
    public class Helper : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task FillListOrderBooks(BinanceClient client, Dictionary<string, OrderBookResponse> orderBooks, HttpClient httpClient)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            foreach (var symbol in Settings.Symbols)
            {
                try
                {
                    orderBooks.Add(symbol, await client.GetOrderBook(httpClient, symbol, 500));
                }
                catch (Exception ex)
                {
                    logger.Error($"err, symbol = {symbol}, {ex.Message}");
                }
            }

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }

        public void RecordOrderBooksInDB(Dictionary<string, OrderBookResponse> orderBooks)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            using (var db = new OrderBookContext())
            {
                var now = DateTime.Now;
                var listOrderBooksForRecord = orderBooks.Select(o => new OrderBook 
                {
                    Symbol = o.Key.ToString(),
                    Trades = o.Value.Bids.Select(b => new Trade { Price = b.Price, Quantity = b.Quantity, Bid = true })
                         .Concat(o.Value.Asks.Select(a => new Trade { Price = a.Price, Quantity = a.Quantity, Ask = true })).ToList(),
                    TimeTrade = now
                });
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.OrderBooks.AddRange(listOrderBooksForRecord);
                    db.SaveChanges();

                    transaction.Commit();

                    logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
                }
                catch(Exception ex)
                {
                    logger.Error($"err, при записи OrderBook в бд возникла ошибка: {ex.Message}");
                    transaction.Rollback();
                }
            }
        }

        public static void FillProxy()
        {
            FillProxyFromProxyScrape();
            FillProxyFromHidemy();
        }

        private static void FillProxyFromProxyScrape()
        {
            try
            {
                using (var httpClient = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                }))
                {
                    var response = httpClient.GetAsync(Settings.UrlProxyScrape, HttpCompletionOption.ResponseHeadersRead).Result;

                    if (!response.IsSuccessStatusCode)
                        return;

                    using (var stream = response.Content.ReadAsStreamAsync().Result)
                    using (var streamReader = new StreamReader(stream))
                    {
                        var uriArr = streamReader.ReadToEnd()?.Split("\r\n");

                        if (!uriArr.Any())
                            return;

                        foreach (var uri in uriArr)
                        {
                            if (!Uri.TryCreate($"http://{uri}", UriKind.Absolute, out Uri newUri))
                                continue;
                            
                            if (CheckProxy(newUri))
                                Settings.ProxyList.TryAdd(uri.Split(":")[0], new Proxy(newUri));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{MethodBase.GetCurrentMethod()}, Ошибка при заполнении прокси-листа - {ex.Message}");
                throw new Exception($"Ошибка при заполнении прокси-листа {ex.Message}");
            }
        }

        private static void FillProxyFromHidemy()
        {
            try
            {
                using (var httpClient = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                }))
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36 OPR/71.0.3770.323");
                    httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");

                    var result = httpClient.GetStringAsync(Settings.UrlProxyHidemy).Result;
                    var matches = Regex.Matches(result, @"(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}</td>)|(\d{2,5}</td>)");

                    for (int i = 0; i < matches.Count; i += 2)
                    {
                        if (!Regex.Match(matches[i].Value, @"([A-Za-z-])").Success || matches[i].Value.Contains("<"))
                        {
                            var ip = matches[i]?.Value?.Replace("</td>", "");
                            var strPort = matches[i + 1]?.Value?.Replace("</td>", string.Empty);
                            int.TryParse(strPort, out int port);
                            if (!Uri.TryCreate($"http://{ip}:{strPort}", UriKind.Absolute, out Uri newUri))
                                continue;

                            if (CheckProxy(newUri))
                                Settings.ProxyList.TryAdd(ip, new Proxy(newUri));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{MethodBase.GetCurrentMethod()}, Ошибка при заполнении прокси-листа - {ex.Message}");
                throw new Exception($"Ошибка при заполнении прокси-листа {ex.Message}");
            }
        }

        private static bool CheckProxy(Uri uriProxy)
        {
            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    UseProxy = true,
                    Proxy = new WebProxy(uriProxy)
                };

                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = new TimeSpan(0,0,0,0, 1200);
                var response = httpClient.GetStringAsync($"https://api.binance.com/api/v3/depth?symbol=ETHUSDT&limit=500").Result;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}