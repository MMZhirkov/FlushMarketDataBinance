using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlushMarketDataConsole
{
    public class Log
    {
        public static Logger GetLogger()
        {
            return LogManager.GetCurrentClassLogger();
        }
    }
}
