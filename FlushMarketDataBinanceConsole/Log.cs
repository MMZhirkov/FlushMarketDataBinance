using NLog;

namespace FlushMarketDataBinanceConsole
{
    public class Log
    {
        public static Logger GetLogger()
        {
            return LogManager.GetCurrentClassLogger();
        }
    }
}