﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Websockets;
using FlushMarketDataBinance.Market;
using Newtonsoft.Json;

namespace FlushMarketDataBinance
{
    public class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            const string token = "LINKBTC";
            const string token2 = "ETHBTC";

            IBinanceRestClient binanceRestClient = new BinanceRestClient(new BinanceClientConfiguration
            {
                ApiKey = "<nrsncyIhw0HvobcXC6OC0lYzMDmQj391G2y2rrDtMYRYPNMNfN3WTe7maqbbfQKV>",
                SecretKey = "KDqqt0CTOaFCfy3GFV1Bk6fBTrZ8lqCrN5Yvpbn8OY9xFr80JA95EswYseHOF96n"
            });

            var marketDepth = new MarketDepth(token);
            await TestConnection(binanceRestClient);

            System.Console.Clear();

            var lastUpdateTime = default(long);

            marketDepth.MarketDepthChanged += (sender, e) =>
            {
                int n = 100;
                System.Console.WriteLine("Price : Volume");
                
                Logger.Info($"{e.UpdateTime}");
                if (lastUpdateTime != e.UpdateTime)
                {
                    try
                    {
                        System.Console.WriteLine(
                        JsonConvert.SerializeObject(
                           new
                           {
                               LastUpdate = e.UpdateTime,
                               Asks = e.Asks.Take(n).Reverse().Select(s => $"{s.Price} : {s.Volume}"),
                               Bids = e.Bids.Take(n).Select(s => $"{s.Price} : {s.Volume}"),
                               token = "LINKBTC"
                           },
                           Formatting.Indented));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Message error = {ex.Message},StackTrace = {ex.StackTrace} ");
                        throw;
                    }
                }

                System.Console.WriteLine("Press Enter to stop streaming market depth...");
                System.Console.SetCursorPosition(0, 0);
            };

            var marketDepthManager = new MarketDepthManager(binanceRestClient, new BinanceWebSocketClient(binanceRestClient, Logger));
            // build order book
            await marketDepthManager.BuildAsync(marketDepth);
            // stream order book updates
            marketDepthManager.StreamUpdates(marketDepth);

            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }

      
        private static async Task TestConnection(IBinanceRestClient binanceRestClient)
        {
            Logger.Info("Testing connection...");
            IResponse testConnectResponse = await binanceRestClient.TestConnectivityAsync();
            if (testConnectResponse != null)
            {
                ServerTimeResponse serverTimeResponse = await binanceRestClient.GetServerTimeAsync();
                Logger.Info($"Connection is established. Approximate ping time: {DateTime.UtcNow.Subtract(serverTimeResponse.ServerTime).TotalMilliseconds:F0} ms");
            }
        }
    }
}
