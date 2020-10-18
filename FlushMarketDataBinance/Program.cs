using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.WebSocket;
using FlushMarketDataBinance.DAL;
using FlushMarketDataBinance.Model;
using Microsoft.Extensions.Configuration;

namespace FlushMarketDataBinance
{
    public class Program
    {
        private static ApiClient apiClient = new ApiClient("@YourApiKey", "@YourApiSecret");
        private static BinanceClient binanceClient = new BinanceClient(apiClient, false);

        public static async Task Main(string[] args)
        {

            binanceClient.ListenDepthEndpoint("ethusdt", DepthHandler);





           

            //marketDepth.MarketDepthChanged += (sender, e) =>
            //{
            //    int n = 300;
            //    System.Console.WriteLine("Price : Volume");
               
            //    if (lastUpdateTime != e.UpdateTime)
            //    {
            //        Logger.Info($"time = {e.UpdateTime}");
            //        try
            //        {
            //            slices.Add(new Slice(
            //                e.UpdateTime,
            //                string.Join(" // ", e.Bids.Take(n).Select(s => $"{Math.Round(s.Price)} {Math.Round(s.Volume)}")),
            //                string.Join(" // ", e.Asks.Take(n).Reverse().Select(s => $"{Math.Round(s.Price)} {Math.Round(s.Volume)}"))
            //                ));

            //            maxSizeList++;

            //            //if (maxSizeList > 5)
            //            //{
            //            //    RecordSlices(slices);
            //            //    maxSizeList = 0;
            //            //    slices.Clear();
            //            //}
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.Error($"Message error = {ex.Message},StackTrace = {ex.StackTrace} ");
            //            throw;
            //        }
            //    }

            //    System.Console.WriteLine("Press Enter to stop streaming market depth...");
            //    System.Console.SetCursorPosition(0, 0);
            //};



            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }

        private static void DepthHandler(DepthMessage messageData)
        {
            var depthData = messageData;
        }

        //static void RecordSlices(List<Slice> slices)
        //{
        //    var sliceDAL = new SliceDAL(_iconfiguration);
        //    sliceDAL.AddSlice(slices);
        //}

    }
}
