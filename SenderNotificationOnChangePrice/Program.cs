using FlushMarketDataBinanceModel.SettingsApp;
using System;
using Telegram.Bot;

namespace SenderNotificationOnChangePrice
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.InitConfig();

            var telegramClient = new TelegramBotClient(Settings.TelegramToken);
            var senderAlertToTelegram = new ScannerOnChangePrice(telegramClient);

            telegramClient.StartReceiving();
            telegramClient.OnMessage += senderAlertToTelegram.OnMessageHandler;

            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;
            } while (key != ConsoleKey.Escape);
        }
    }
}