using FlushMarketDataBinanceModel.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace FlushMarketDataBinanceModel.SettingsApp
{
    public static class Settings
    {
        public static string ApiKey { get; private set; }
        public static string SecretKey { get; private set; }
        public static string ConnectionString { get; private set; }
        public static string CronExpression { get; private set; }

        /// <summary>
        /// url hidemy.name для парсинга нужных прокси по фильтру
        /// </summary>
        public static string UrlProxy { get; private set; }
        public static List<Proxy> ProxyList { get;  set; } = new List<Proxy>();

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
            Settings.CronExpression = config.GetSection("Cron:cronExpression")?.Value;
            Settings.Symbols = config.GetSection("BinanceApi:symbols")?.Value?.ToUpper()?.Replace(" ", string.Empty)?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Settings.UrlProxy = config.GetSection("Proxy:urlProxy")?.Value;

            if (string.IsNullOrEmpty(Settings.ConnectionString) || string.IsNullOrEmpty(Settings.ApiKey) || string.IsNullOrEmpty(Settings.SecretKey) || string.IsNullOrEmpty(Settings.CronExpression) || string.IsNullOrEmpty(Settings.UrlProxy))
                throw new Exception("Заполните обязательные параметры в конфиге");
        }
    }
}