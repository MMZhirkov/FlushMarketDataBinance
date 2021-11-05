using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceApi.Const;
using FlushMarketDataBinanceModel.SettingsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace SenderNotificationOnChangePrice
{
    public class SenderAlertToTelegram
    {
        private static TelegramBotClient telegramClient;
        private static long chatId;
        private static List<Alert> alerts = new List<Alert>();

        public SenderAlertToTelegram(TelegramBotClient tClient)
        {
            telegramClient = tClient;
        }

        public void OnMessageHandler(object sender, MessageEventArgs e)
        {
            chatId = e.Message.Chat.Id;
            if (e.Message.From.Username != Settings.TelegramUser)
            {
                telegramClient.SendTextMessageAsync(chatId, "Бот работает только для определенных лиц, удачи");
                return;
            }

            telegramClient.SendTextMessageAsync(chatId, "Поиск запущен");
            StartTrackingBinancePrices();
        }

        private void StartTrackingBinancePrices()
        {
            var binanceClient = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });

            foreach (var nameSymbol in Settings.Symbols)
                binanceClient.ListenKlineEndpoint(nameSymbol, TimeInterval.minutes15, SendMessageResponse);
        }

        private void SendMessageResponse(KlineMessage messageData)
        {
            var procentChangePrice = CalcChangePriceProcent(messageData.KlineInfo.Open, messageData.KlineInfo.Close);
            var timeNowToCompare = DateTime.UtcNow.AddMinutes(-15);
            if (procentChangePrice > 1m && !alerts.Any(a => a.CreateOn > timeNowToCompare))
            {
                var symbol = messageData.Symbol;
                telegramClient.SendTextMessageAsync(chatId, $"{symbol}, {procentChangePrice}");
                alerts.Add(new Alert() { Symbol = symbol, ProcentChangePrice = procentChangePrice , CreateOn = DateTime.UtcNow });
            }
        }

        private decimal CalcChangePriceProcent(decimal priceOpen, decimal priceClose) => 100m * Math.Abs(priceOpen - priceClose) / priceOpen;
    }
}