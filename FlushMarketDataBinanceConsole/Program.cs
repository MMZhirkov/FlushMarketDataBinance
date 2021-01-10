using Microsoft.Extensions.Configuration;
using NLog;
using System;
using BinanceExchange.API.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using FlushMarketDataBinanceConsole.Context;
using System.Reflection;
using BinanceExchange.API.Models.Response;
using DataModel;
using System.Linq;
using System.Threading;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            logger.Info($"Запущен {MethodBase.GetCurrentMethod()}");

            Settings.InitConfig();

            if (string.IsNullOrEmpty(Settings.ConnectionString) || string.IsNullOrEmpty(Settings.ApiKey) || string.IsNullOrEmpty(Settings.SecretKey))
                return;

            var options = GetOptionsDBContext();
            if (options == null)
            {
                logger.Info("options null");
                return;
            }
            
            var client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });
            var orderBooks = new Dictionary<string, OrderBookResponse>();
            var today = DateTime.Today; 

            while (DateTime.Now < new DateTime(today.Year, today.Month, today.Day, 23, 58, 00))
            {
                using (var helper = new Helper())
                {
                    await helper.GetOrderBooks(client, orderBooks);

                    if (!orderBooks.Any())
                    {
                        logger.Info($"{nameof(orderBooks)} пустой");
                        continue;
                    }

                    helper.RecordOrderBooksInDB(options, orderBooks);
                    orderBooks.Clear();
                }

                Thread.Sleep(700);
                Console.WriteLine(DateTime.Now);
            }

            logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
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
    }
}