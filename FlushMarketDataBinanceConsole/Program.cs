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
using FlushMarketDataBinanceConsole.Context;
using System.Reflection;
using BinanceExchange.API.Models.Response;
using DataModel;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            logger.Info("Start FlushMarket");

            InitConfig();
            if (string.IsNullOrEmpty(Settings.ConnectionString) || string.IsNullOrEmpty(Settings.ApiKey) || string.IsNullOrEmpty(Settings.SecretKey) || string.IsNullOrEmpty(Settings.StrSymbols))
                return;

            var options = GetOptionsDBContext();
            if (options == null)
            {
                logger.Info("options null");
                return;
            }

            var symbols = Settings.StrSymbols.Replace(" ", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            var client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });

            var orderBooks = new List<OrderBookResponse>();

            foreach (var symbol in symbols)
            {
                try
                {
                    orderBooks.Add(await client.GetOrderBook(symbol, true, 1000));
                }
                catch (Exception ex)
                {
                    logger.Info($"err, {ex.Message}");
                    throw;
                }
            }

            using (var db = new OrderBookContext(options))
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

            logger.Info("End FlushMarket");
        }

        private static void InitConfig()
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");

            var config = builder.Build();
            Settings.ConnectionString = config.GetConnectionString("DefaultConnection");
            Settings.ApiKey = config.GetSection("BinanceApi:apiKey")?.Value;
            Settings.SecretKey = config.GetSection("BinanceApi:secretKey")?.Value;
            Settings.StrSymbols = config.GetSection("BinanceApi:symbols")?.Value;

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }

        private static DbContextOptions<OrderBookContext> GetOptionsDBContext()
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");
            
            var options = new DbContextOptionsBuilder<Context.OrderBookContext>()
                .UseSqlServer(Settings.ConnectionString)
                .Options;

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");

            return options;
        }

        private static async Task<Dictionary<string, DepthCacheObject>> BuildLocalDepthCache(IBinanceClient client)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            var depthResults = await client.GetOrderBook("BNBBTC", true, 1000);

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");

            return new Dictionary<string, DepthCacheObject>();
        }
    }
}