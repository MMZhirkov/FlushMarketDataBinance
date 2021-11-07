using FlushMarketDataBinanceApi.ApiModels.Response;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceApi.Const;
using FlushMarketDataBinanceModel.SettingsApp;
using SenderNotificationOnChangePrice.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace SenderNotificationOnChangePrice
{
    public class ScannerOnChangePrice
    {
        private static TelegramBotClient telegramClient;
        private long chatId;

        private static BinanceClient binanceClient;
        private static HttpClient httpClient;

        private Dictionary<string, decimal> allPrices24hInfo;
        private List<Alert> alerts = new List<Alert>();

        public ScannerOnChangePrice(TelegramBotClient tClient)
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
            allPrices24hInfo = binanceClient.GetAllPrices(httpClient).GetAwaiter().GetResult().Where(p => Settings.Symbols.Contains(p.Symbol)).ToDictionary(p => p.Symbol, p => p.Volume);

            while (true)
            {
                SendAlertInfoOrderbook();
                SendAlertInfoKline();
            }

            #endregion

            #region WebSocket
            //foreach (var nameSymbol in Settings.Symbols)
            //    binanceClient.ListenKlineEndpoint(nameSymbol, TimeInterval.minutes15, SendMessageResponseFromStream);
            #endregion
        }

        private void SendAlertInfoOrderbook()
        {
            var now = DateTime.Now;
            var ago5minute = now.AddMinutes(-5);

            foreach (var symbol in Settings.SymbolsOrderBook)
            {
                if (alerts.Any(a => a.Symbol == symbol && a.CreateOn > ago5minute && a.TypeAlert == TypeAlert.BigOrderbook))
                    continue;

                var orderbook = binanceClient.GetOrderBookStock(httpClient, symbol, 70).GetAwaiter().GetResult();
                var bids = orderbook.Bids.Sum(s => s.Quantity);
                var asks = orderbook.Asks.Sum(s => s.Quantity);
                var firstPrice = orderbook.Bids.FirstOrDefault().Price;

                if ((bids * firstPrice > 1500000m || asks * firstPrice > 1500000m) && (bids * 2 < asks || bids > asks * 2))
                {
                    telegramClient.SendTextMessageAsync(chatId, $"{symbol}, {TypeAlert.BigOrderbook}, bids/ask = {bids/ asks}, bids = {bids * firstPrice}, asks = {bids * firstPrice}");
                    alerts.Add(new Alert() { Symbol = symbol, ProcentChangePrice = 0, CreateOn = DateTime.Now, TypeAlert = TypeAlert.BigOrderbook });
                }

                Console.WriteLine($"{DateTime.Now}, bids = {bids * firstPrice}, asks = {bids * firstPrice}");
            }
        }

        private void SendAlertInfoKline()
        {
            var now = DateTime.Now;
            var ago30minute = now.AddMinutes(-30);
            var alertsInfoKline = new TypeAlert[] { TypeAlert.BigVolume, TypeAlert.ChangePriceProcent };
            foreach (var symbol in Settings.Symbols)
            {
                if (alerts.Any(a => a.Symbol == symbol && a.CreateOn > ago30minute && alertsInfoKline.Contains(a.TypeAlert)))
                    continue;

                var lastKline = binanceClient.GetKline(httpClient, symbol, TimeInterval.minutes30).GetAwaiter().GetResult().FirstOrDefault();
                if (allPrices24hInfo.TryGetValue(symbol, out decimal maxDayVolume) && lastKline.Volume > maxDayVolume * 0.7m)
                {
                    telegramClient.SendTextMessageAsync(chatId, $"{symbol}, TypeAlert = {TypeAlert.BigVolume}, volume = {lastKline.Volume}");
                    alerts.Add(new Alert() { Symbol = symbol, ProcentChangePrice = 0, CreateOn = DateTime.Now, TypeAlert = TypeAlert.BigVolume });
                }

                var procentChangePriceProcent = CalcChangePriceProcent(lastKline.Open, lastKline.Close);
                if (procentChangePriceProcent > Settings.ProcentChangePrice15min)
                {
                    telegramClient.SendTextMessageAsync(chatId, $"{symbol}, {Math.Round(procentChangePriceProcent, 1)}, {TypeAlert.ChangePriceProcent}");
                    alerts.Add(new Alert() { Symbol = symbol, ProcentChangePrice = procentChangePriceProcent, CreateOn = DateTime.Now, TypeAlert = TypeAlert.ChangePriceProcent });
                }

                Console.WriteLine($"{symbol}, {procentChangePriceProcent}, {DateTime.Now} ");
            }
        }

        [Obsolete("old method, deprecated")]
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