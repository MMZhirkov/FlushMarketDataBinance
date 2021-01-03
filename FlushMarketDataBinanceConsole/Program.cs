using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using BinanceExchange.API;
using BinanceExchange.API.Client;
using BinanceExchange.API.Client.Interfaces;
using BinanceExchange.API.Models.WebSocket;
using BinanceExchange.API.Utility;
using BinanceExchange.API.Websockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using FlushMarketDataBinanceConsole.Model;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            // установка пути к текущему каталогу
            builder.SetBasePath(Directory.GetCurrentDirectory());
            // получаем конфигурацию из файла appsettings.json
            builder.AddJsonFile("appsettings.json");
            // создаем конфигурацию
            var config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<Context.OrderBookContext>();
            var options = optionsBuilder
                .UseSqlServer(connectionString)
                .Options;

            using (Context.OrderBookContext db = new Context.OrderBookContext(options))
            {
                // создаем два объекта User
                OrderBook user1 = new OrderBook { Bids = "Tom", Asks = 33 };
                OrderBook user2 = new OrderBook { Bids = "Alice", Asks = 26 };

                // добавляем их в бд
                db.OrderBooks.Add(user1);
                db.OrderBooks.Add(user2);
                db.SaveChanges();
                Console.WriteLine("Объекты успешно сохранены");

                // получаем объекты из бд и выводим на консоль
                var users = db.OrderBooks.ToList();
                Console.WriteLine("Список объектов:");
                foreach (OrderBook u in users)
                {
                    Console.WriteLine($"{u.Id}.{u.Bids} - {u.Asks}");
                }
            }


            logger.Info("Start FlushMarket");

            string apiKey = "YOUR_API_KEY";
            string secretKey = "YOUR_SECRET_KEY";

            var client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = apiKey,
                SecretKey = secretKey
            });

            var depthResults = await client.GetOrderBook("BNBBTC", true, 1000);

            logger.Info("End FlushMarket");
        }

        private static async Task<Dictionary<string, DepthCacheObject>> BuildLocalDepthCache(IBinanceClient client)
        {

            var depthResults = await client.GetOrderBook("BNBBTC", true, 1000);

            return new Dictionary<string, DepthCacheObject>();
        }
    }
}
