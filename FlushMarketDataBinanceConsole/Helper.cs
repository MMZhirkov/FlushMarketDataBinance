using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceModel;
using FlushMarketDataBinanceModel.DAL;
using FlushMarketDataBinanceModel.Enums;
using FlushMarketDataBinanceModel.SettingsApp;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
    public class Helper : IDisposable
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void FillListOrderBooks(BinanceClient client, ConcurrentBag<OrderBook> orderBooks, HttpClient httpClient)
        {
            _logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            List<Task> TaskList = new List<Task>();
            foreach (var symbol in Settings.Symbols)
            {
                TaskList.Add(Task.Run(async () =>
                {
                    var orderBookResponseFuture = await client.GetOrderBookFuture(httpClient, symbol, 500);
                    var now = DateTime.Now;
                    orderBooks.Add(new OrderBook()
                    {
                        Symbol = symbol,
                        Trades =
                        orderBookResponseFuture.Bids.Select(b => new Trade { Price = b.Price, Quantity = b.Quantity, OrderBookSide = OrderBookSide.Buy }).Concat(
                        orderBookResponseFuture.Asks.Select(a => new Trade { Price = a.Price, Quantity = a.Quantity, OrderBookSide = OrderBookSide.Sell })).ToList(),
                        TimeTrade = now,
                        TypeActive = FinActive.Future
                    });
                }));

                TaskList.Add(Task.Run(async () =>
                {
                    var orderBookResponseFuture = await client.GetOrderBookStock(httpClient, symbol, 500);
                    var now = DateTime.Now;
                    orderBooks.Add(new OrderBook()
                    {
                        Symbol = symbol,
                        Trades =
                        orderBookResponseFuture.Bids.Select(b => new Trade { Price = b.Price, Quantity = b.Quantity, OrderBookSide = OrderBookSide.Buy }).Concat(
                        orderBookResponseFuture.Asks.Select(a => new Trade { Price = a.Price, Quantity = a.Quantity, OrderBookSide = OrderBookSide.Sell })).ToList(),
                        TimeTrade = now,
                        TypeActive = FinActive.Stock
                    });
                }));
            }

            Task.WaitAll(TaskList.ToArray());

            _logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }

        public async void RecordOrderBooksInDB(List<OrderBook> orderBooks)
        {
            _logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            using (var db = new OrderBookContext())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.OrderBooks.AddRange(orderBooks);
                    await db.SaveChangesAsync();

                    transaction.Commit();

                    _logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
                }
                catch (Exception ex)
                {
                    _logger.Error($"err, при записи OrderBook в бд возникла ошибка: {ex.Message}, {ex.InnerException}");
                    transaction.Rollback();
                }
            }
        }
    }
}