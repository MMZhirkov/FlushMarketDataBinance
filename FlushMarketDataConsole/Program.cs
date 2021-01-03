using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using BinanceExchange.API;
using BinanceExchange.API.Client;
using BinanceExchange.API.Client.Interfaces;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Market;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Models.Response.Error;
using BinanceExchange.API.Models.WebSocket;
using BinanceExchange.API.Utility;
using BinanceExchange.API.Websockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FlushMarketDataConsole
{
    public class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
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
    }
}