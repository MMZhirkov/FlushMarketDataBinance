using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceConsole.Context;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
	public class FlushMarketData : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var dataMap = context.MergedJobDataMap;
			var client = (BinanceClient)dataMap["client"];
            var options = (DbContextOptions<OrderBookContext>)dataMap["options"];
            
            using (var helper = new Helper())
            {
                var orderBooks = new Dictionary<string, OrderBookResponse>();
                
                await helper.FillListOrderBooks(client, orderBooks);

                if (!orderBooks.Any())
                    return;

                helper.RecordOrderBooksInDB(options, orderBooks);
            }

            Console.WriteLine($"Task done, {DateTime.Now}");
		}
	}
}