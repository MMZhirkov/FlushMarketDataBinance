﻿using NLog;

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