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

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            logger.Info("Start FlushMarket");

            InitConfig();

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

            using (var helper = new Helper()) 
            {
                var orderBooks = new List<OrderBookResponse>();

                helper.GetOrderBooks(client, orderBooks);
                helper.RecordOrderBooksInDB(options, orderBooks);

                orderBooks.Clear();
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
            Settings.Symbols = config.GetSection("BinanceApi:symbols")?.Value?.ToUpper()?.Replace(" ", string.Empty)?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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
    }
}