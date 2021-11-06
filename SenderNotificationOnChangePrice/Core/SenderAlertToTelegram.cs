using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceApi.Const;
using FlushMarketDataBinanceModel.SettingsApp;
using SenderNotificationOnChangePrice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace SenderNotificationOnChangePrice
{
    public class SenderAlertToTelegram
    {
        private static TelegramBotClient telegramClient;
        private static long chatId;

        private static BinanceClient binanceClient;
        private static HttpClient httpClient;

        private static List<Alert> alerts = new List<Alert>();
        private static List<PriceCash> priceCash = new List<PriceCash>();

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
            binanceClient = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });

            httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

            #region Api
            while (true)
            {
                SendMessageResponseFromApi();
                Thread.Sleep(20000);
            }
            #endregion

            #region WebSocket
            //foreach (var nameSymbol in Settings.Symbols)
            //    binanceClient.ListenKlineEndpoint(nameSymbol, TimeInterval.minutes15, SendMessageResponseFromStream);
            #endregion
        }

        private void SendMessageResponseFromApi()
        {
            var allPrices24hInfo = binanceClient.GetAllPrices(httpClient).GetAwaiter().GetResult().Where(p => Settings.Symbols.Contains(p.Symbol));
            var now = DateTime.Now;
            var ago15minute = now.AddMinutes(-15);
            var ago30minute = now.AddMinutes(-30);
            foreach (var price24hInfo in allPrices24hInfo)
            {
                var symbol = price24hInfo.Symbol;
                var bidPrice = price24hInfo.BidPrice;
                
                priceCash.Add(new PriceCash() { Symbol = symbol, Price = bidPrice, CreateOn = now });
                if (alerts.Any(a => a.Symbol == symbol && a.CreateOn > ago15minute))
                    continue;

                var firstPriceIn15min = priceCash.Where(n => n.Symbol == symbol && n.CreateOn > ago15minute).OrderBy(n=>n.CreateOn).FirstOrDefault();
                var firstPriceIn30min = priceCash.Where(n => n.Symbol == symbol && n.CreateOn > ago30minute).OrderBy(n => n.CreateOn).FirstOrDefault();
                if (firstPriceIn15min == null && firstPriceIn30min == null)
                    continue;

                var procentChangePriceProcent15min = CalcChangePriceProcent(firstPriceIn15min.Price, bidPrice);
                var procentChangePriceProcent30min = CalcChangePriceProcent(firstPriceIn30min.Price, bidPrice);
                Console.WriteLine(DateTime.Now.Second + symbol + procentChangePriceProcent15min);
                if (procentChangePriceProcent15min > Settings.ProcentChangePrice15min || procentChangePriceProcent30min > Settings.ProcentChangePrice30min)
                {
                    telegramClient.SendTextMessageAsync(chatId, $"{symbol}, {procentChangePriceProcent15min}, {procentChangePriceProcent30min}");
                    alerts.Add(new Alert() { Symbol = symbol, ProcentChangePrice = procentChangePriceProcent15min, CreateOn = DateTime.Now });
                }
            }
        }

        private void SendMessageResponseFromStream(KlineMessage messageData)
        {
            var procentChangePrice = CalcChangePriceProcent(messageData.KlineInfo.Open, messageData.KlineInfo.Close);
            var timeNowToCompare = DateTime.UtcNow.AddMinutes(-15);
            var symbol = messageData.Symbol;
            if (procentChangePrice > 1 && !alerts.Any(a => a.Symbol == symbol && a.CreateOn > timeNowToCompare))
            {
                telegramClient.SendTextMessageAsync(chatId, $"{symbol}, {procentChangePrice}");
                alerts.Add(new Alert() { Symbol = symbol, ProcentChangePrice = procentChangePrice , CreateOn = DateTime.UtcNow });
            }

            Console.WriteLine(DateTime.Now.Second + symbol + procentChangePrice);
        }

        private double CalcChangePriceProcent(double priceOpen, double priceClose) => 100 * Math.Abs(priceOpen - priceClose) / priceOpen;
    }
}