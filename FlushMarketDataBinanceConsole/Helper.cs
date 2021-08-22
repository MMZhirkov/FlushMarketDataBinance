using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel;
using FlushMarketDataBinanceModel.DAL;
using FlushMarketDataBinanceModel.DB;
using FlushMarketDataBinanceModel.Enums;
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

        public async Task FillListOrderBooks(BinanceClient client, Dictionary<string, OrderBook> orderBooks, HttpClient httpClient)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}, ");

            foreach (var symbol in Settings.Symbols)
            {
                try
                {
                    var orderBookResponseFuture = await client.GetOrderBookFuture(httpClient, symbol, 500);
                    var now = DateTime.Now;
                    orderBooks.Add($"F_{symbol}", new OrderBook()
                    {
                        Symbol = symbol,
                        Trades =
                        orderBookResponseFuture.Bids.Select(b => new Trade { Price = b.Price, Quantity = b.Quantity, OrderBookSide = OrderBookSide.Buy }).Concat(
                        orderBookResponseFuture.Asks.Select(a => new Trade { Price = a.Price, Quantity = a.Quantity, OrderBookSide = OrderBookSide.Sell })).ToList(),
                        TimeTrade = now,
                        TypeActive = FinActive.Future
                    });

                    var orderBookResponseStock = await client.GetOrderBookStock(httpClient, symbol, 500);
                    orderBooks.Add(symbol, new OrderBook()
                    {
                        Symbol = symbol,
                        Trades =
                         orderBookResponseStock.Bids.Select(b => new Trade { Price = b.Price, Quantity = b.Quantity, OrderBookSide = OrderBookSide.Buy }).Concat(
                         orderBookResponseStock.Asks.Select(a => new Trade { Price = a.Price, Quantity = a.Quantity, OrderBookSide = OrderBookSide.Sell })).ToList(),
                        TimeTrade = now,
                        TypeActive = FinActive.Stock
                    });

                    Console.WriteLine($"Success request ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bad request, symbol = {symbol}");

                    logger.Error($"err, symbol = {symbol}, {ex.Message}");
                }
            }

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }

        public void RecordOrderBooksInDB(Dictionary<string, OrderBook> orderBooks)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            using (var db = new OrderBookContext())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.OrderBooks.AddRange(orderBooks.Values);
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
    }
}