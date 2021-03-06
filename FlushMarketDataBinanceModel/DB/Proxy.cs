﻿using System;

namespace FlushMarketDataBinanceModel.DB
{
    public class Proxy
    {
        public string IP { get; private set; }
        public int Port { get; private set; }
        public Uri UriProxy { get; private set; }
        /// <summary>
        /// Записывается время последнего запроса через тек proxy
        /// </summary>
        public DateTime LastUseTime { get; set; } = new DateTime();

        public Proxy(Uri uriProxy)
        {
            UriProxy = uriProxy;
        }

        public Proxy(string ip, int port)
        {
            IP = ip;
            Port = port;
        }
    }
}