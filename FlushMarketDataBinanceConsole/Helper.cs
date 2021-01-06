using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using DataModel;
using FlushMarketDataBinanceConsole.Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task GetOrderBooks(BinanceClient client, List<OrderBookResponse> orderBooks)
        {
            foreach (var symbol in Settings.Symbols)
            {
                try
                {
                    orderBooks.Add(await client.GetOrderBook(symbol, true, 1000));
                }
                catch (Exception ex)
                {
                    logger.Info($"err, {ex.Message}");
                }
            }
        }

        public void RecordOrderBooksInDB(DbContextOptions<OrderBookContext> options, List<OrderBookResponse> orderBooks)
        {
            using (var db = new OrderBookContext(options))
            {
                var now = DateTime.Now;

                orderBooks.ForEach(orderBook => db.OrderBooks.Add(
                    new OrderBook
                    {
                        Trades = orderBook.Bids.Select(b => new Trade { Price = b.Price, Quantity = b.Quantity, Bid = true })
                         .Concat(orderBook.Asks.Select(a => new Trade { Price = a.Price, Quantity = a.Quantity, Ask = true })).ToList(),
                        TimeTrade = now
                    }));

                db.SaveChanges();
            }
        }
    }
}