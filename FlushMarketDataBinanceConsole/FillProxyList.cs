using Quartz;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceConsole
{
    public class FillProxyList : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using (var helper = new Helper())
            {
                await helper.FillProxy();
            }
        }
    }
}