using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace FlushMarketDataBinanceModel.SettingsApp
{
    public static class Settings
    {
        public static string ApiKey { get; private set; }
        public static string SecretKey { get; private set; }
        public static string ConnectionString { get; private set; }
        public static string TelegramToken { get; private set; }
        public static string TelegramUser { get; private set; }
        public static int ProcentChangePrice15min { get; private set; }
        public static int ProcentChangePrice30min { get; private set; }
        /// <summary>
        /// Names Symbols
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
            Settings.Symbols = config.GetSection("BinanceApi:symbols")?.Value?.ToUpper()?.Replace(" ", string.Empty)?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Settings.TelegramToken = config.GetSection("Telegram:token")?.Value;
            Settings.TelegramUser = config.GetSection("Telegram:user")?.Value;
            Settings.ProcentChangePrice15min = int.Parse(config.GetSection("Query:ProcentChangePrice15min")?.Value);
            Settings.ProcentChangePrice30min = int.Parse(config.GetSection("Query:ProcentChangePrice30min")?.Value);

            if (string.IsNullOrEmpty(Settings.ConnectionString) || string.IsNullOrEmpty(Settings.ApiKey) || string.IsNullOrEmpty(Settings.SecretKey))
                throw new Exception("Заполните обязательные параметры в конфиге");
        }
    }
}