using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel;
using FlushMarketDataBinanceModel.DAL;
using FlushMarketDataBinanceModel.DB;
using FlushMarketDataBinanceModel.SettingsApp;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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

        public  void RecordOrderBooksInDB(Dictionary<string, OrderBookResponse> orderBooks)
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

        public WebProxy GetProxy(Proxy proxy)
        {
            return new WebProxy(proxy.IP, proxy.Port)
            {
                UseDefaultCredentials = true
            };
        }
    }
}