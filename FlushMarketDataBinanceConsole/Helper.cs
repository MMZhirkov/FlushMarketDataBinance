using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using DataModel;
using FlushMarketDataBinanceConsole.Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task FillListOrderBooks(BinanceClient client, Dictionary<string, OrderBookResponse> orderBooks)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            foreach (var symbol in Settings.Symbols)
            {
                try
                {
                    orderBooks.Add(symbol, await client.GetOrderBook(symbol, false, 500));
                }
                catch (Exception ex)
                {
                    logger.Error($"err, symbol = {symbol}, {ex.Message}");
                }
            }

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }

        public void RecordOrderBooksInDB(DbContextOptions<OrderBookContext> options, Dictionary<string, OrderBookResponse> orderBooks)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            using (var db = new OrderBookContext(options))
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
    }
}