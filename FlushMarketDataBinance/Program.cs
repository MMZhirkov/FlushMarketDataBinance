using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Websockets;
using FlushMarketDataBinance.DAL;
using FlushMarketDataBinance.Market;
using FlushMarketDataBinance.Model;
using Microsoft.Extensions.Configuration;

namespace FlushMarketDataBinance
{
    public class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static IConfiguration _iconfiguration;

        public static async Task Main(string[] args)
        {
            const string token = "LINKBTC";
            Logger.Info($"start, {token}");

            GetAppSettingsFile();

            IBinanceRestClient binanceRestClient = new BinanceRestClient(new BinanceClientConfiguration
            {
                ApiKey = "<nrsncyIhw0HvobcXC6OC0lYzMDmQj391G2y2rrDtMYRYPNMNfN3WTe7maqbbfQKV>",
                SecretKey = "KDqqt0CTOaFCfy3GFV1Bk6fBTrZ8lqCrN5Yvpbn8OY9xFr80JA95EswYseHOF96n"
            });

            var marketDepth = new MarketDepth(token);
            await TestConnection(binanceRestClient);

            System.Console.Clear();

            var lastUpdateTime = default(long);
            var slices = new List<Slice>();
            var maxSizeList = 0;

            marketDepth.MarketDepthChanged += (sender, e) =>
            {
                int n = 300;
                System.Console.WriteLine("Price : Volume");
               
                if (lastUpdateTime != e.UpdateTime)
                {
                    Logger.Info($"time = {e.UpdateTime}");
                    try
                    {
                        slices.Add(new Slice(
                            e.UpdateTime,
                            string.Join(" // ", e.Bids.Take(n).Select(s => $"{Math.Round(s.Price)} {Math.Round(s.Volume)}")),
                            string.Join(" // ", e.Asks.Take(n).Reverse().Select(s => $"{Math.Round(s.Price)} {Math.Round(s.Volume)}"))
                            ));

                        maxSizeList++;

                        if (maxSizeList > 5)
                        {
                            RecordSlices(slices);
                            maxSizeList = 0;
                            slices.Clear();
                        }
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

        static void RecordSlices(List<Slice> slices)
        {
            var sliceDAL = new SliceDAL(_iconfiguration);
            sliceDAL.AddSlice(slices);
        }

        static void GetAppSettingsFile()
        {
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _iconfiguration = builder.Build();
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
