using Quartz;
using System;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
    public class FillProxyList : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Task FillProxy start, {DateTime.Now}");

            using (var helper = new Helper())
            {
                await helper.FillProxy();
            }

            Console.WriteLine($"Task FillProxy done, {DateTime.Now}");
        }
    }
}