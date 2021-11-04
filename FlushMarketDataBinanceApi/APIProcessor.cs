using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FlushMarketDataBinanceApi.ApiModels.Response.Error;
using FlushMarketDataBinanceApi.Converter;
using FlushMarketDataBinanceApi.Enums;
using Newtonsoft.Json;
using WebSocketSharp;

namespace FlushMarketDataBinanceApi
{
    /// <summary>
    /// The API Processor is the chief piece of functionality responsible for handling and creating requests to the API
    /// </summary>
    public class APIProcessor : IAPIProcessor
    {
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _webSocketEndpoint;

        /// <summary>
        /// Used to store all the opened web sockets.
        /// </summary>
        public List<WebSocket> _openSockets;

        public APIProcessor(string apiKey, string secretKey)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
            _webSocketEndpoint = @"wss://stream.binance.com:9443/ws/";
            _openSockets = new List<WebSocket>();
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message, string requestMessage) where T : class
        {
            if (message.IsSuccessStatusCode)
            {
                var messageJson = await message.Content.ReadAsStringAsync();
                T messageObject = null;
                try
                {
                    messageObject = JsonConvert.DeserializeObject<T>(messageJson);
                }
                catch (Exception ex)
                {
                    string deserializeErrorMessage = $"Unable to deserialize message from: {requestMessage}. Exception: {ex.Message}";
                    throw new BinanceException(deserializeErrorMessage, new BinanceError()
                    {
                        RequestMessage = requestMessage,
                        Message = ex.Message
                    });
                }

                if (messageObject == null)
                {
                    throw new Exception("Unable to deserialize to provided type");
                }
;
                return messageObject;
            }
            var errorJson = await message.Content.ReadAsStringAsync();
            var errorObject = JsonConvert.DeserializeObject<BinanceError>(errorJson);
            if (errorObject == null) throw new BinanceException("Unexpected Error whilst handling the response", null);
            errorObject.RequestMessage = requestMessage;
            var exception = CreateBinanceException(message.StatusCode, errorObject);
            throw exception;
        }

        private BinanceException CreateBinanceException(HttpStatusCode statusCode, BinanceError errorObject)
        {
            if (statusCode == HttpStatusCode.GatewayTimeout)
            {
                return new BinanceTimeoutException(errorObject);
            }
            var parsedStatusCode = (int)statusCode;
            if (parsedStatusCode >= 400 && parsedStatusCode <= 500)
            {
                return new BinanceBadRequestException(errorObject);
            }
            return parsedStatusCode >= 500 ? 
                new BinanceServerException(errorObject) : 
                new BinanceException("Binance API Error", errorObject);
        }

        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessGetRequest<T>(HttpClient httpClient, BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            HttpResponseMessage message;
            switch (endpoint.SecurityType) { 
                case EndpointSecurityType.ApiKey:
                case EndpointSecurityType.None:
                    message = await RequestClient.GetRequest(httpClient, endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await RequestClient.SignedGetRequest(httpClient, endpoint.Uri, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString());
        }

        /// <summary>
        /// Connects to a Websocket endpoint.
        /// </summary>
        /// <typeparam name="T">Type used to parsed the response message.</typeparam>
        /// <param name="parameters">Paremeters to send to the Websocket.</param>
        /// <param name="messageDelegate">Deletage to callback after receive a message.</param>
        /// <param name="useCustomParser">Specifies if needs to use a custom parser for the response message.</param>
        public void ConnectToWebSocket<T>(string parameters, IAPIProcessor.MessageHandler<T> messageHandler, bool useCustomParser = false)
        {
            var finalEndpoint = _webSocketEndpoint + parameters;

            var ws = new WebSocket(finalEndpoint);

            ws.OnMessage += (sender, e) =>
            {
                dynamic eventData;

                if (useCustomParser)
                {
                    var customParser = new CustomParser();
                    eventData = customParser.GetParsedDepthMessage(JsonConvert.DeserializeObject<dynamic>(e.Data));
                }
                else
                {
                    eventData = JsonConvert.DeserializeObject<T>(e.Data);
                }

                messageHandler(eventData);
            };

            ws.OnClose += (sender, e) =>
            {
                _openSockets.Remove(ws);
            };

            ws.OnError += (sender, e) =>
            {
                _openSockets.Remove(ws);
            };

            ws.Connect();
            _openSockets.Add(ws);
        }
    }
}