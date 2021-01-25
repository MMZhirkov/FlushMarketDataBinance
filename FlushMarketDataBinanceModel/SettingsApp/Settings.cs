using FlushMarketDataBinanceModel.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace FlushMarketDataBinanceModel.SettingsApp
{
    public static class Settings
    {
        public static string ApiKey { get; private set; }
        public static string SecretKey { get; private set; }
        public static string ConnectionString { get; private set; }
        public static string CronExpressionFlushMarketData { get; private set; }
        public static string CronExpressionFillProxy { get; private set; }
        /// <summary>
        /// url hidemy.name для парсинга нужных прокси по фильтру
        /// </summary>
        public static string UrlProxyHidemy { get; private set; }
        public static string UrlProxyScrape { get; private set; }
        public static ConcurrentDictionary<string, Proxy> ProxyList { get;  set; } = new ConcurrentDictionary<string, Proxy>();

        /// <summary>
        /// Наименования пар
        /// </summary>
        public static string[] Symbols { get; private set; }

        public static void InitConfig()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");

            var config = builder.Build();
            Settings.ConnectionString = config.GetConnectionString("DefaultConnection");
            Settings.ApiKey = config.GetSection("BinanceApi:apiKey")?.Value;
            Settings.SecretKey = config.GetSection("BinanceApi:secretKey")?.Value;
            Settings.CronExpressionFlushMarketData = config.GetSection("Cron:cronExpressionFlushMarketData")?.Value;
            Settings.CronExpressionFillProxy = config.GetSection("Cron:cronExpressionFillProxy")?.Value;
            Settings.Symbols = config.GetSection("BinanceApi:symbols")?.Value?.ToUpper()?.Replace(" ", string.Empty)?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Settings.UrlProxyHidemy = config.GetSection("Proxy:urlProxyHidemy")?.Value;
            Settings.UrlProxyScrape = config.GetSection("Proxy:urlProxyScrape")?.Value;

            if (string.IsNullOrEmpty(Settings.ConnectionString) || string.IsNullOrEmpty(Settings.ApiKey) || string.IsNullOrEmpty(Settings.SecretKey) || string.IsNullOrEmpty(Settings.CronExpressionFlushMarketData) || string.IsNullOrEmpty(Settings.CronExpressionFillProxy) || string.IsNullOrEmpty(Settings.UrlProxyHidemy) || string.IsNullOrEmpty(Settings.UrlProxyScrape))
                throw new Exception("Заполните обязательные параметры в конфиге");
        }
    }
}