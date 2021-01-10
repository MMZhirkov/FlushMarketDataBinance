using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DataModel
{
    public static class Settings
    {
        public static string ApiKey { get; private set; }
        public static string SecretKey { get; private set; }
        public static string ConnectionString { get; private set; }
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
        }
    }
}