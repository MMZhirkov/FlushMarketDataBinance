﻿using FlushMarketDataBinanceApi.Enums;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlushMarketDataBinanceApi
{
    internal static class RequestClient
    {
        //private static readonly HttpClient HttpClient;
        private static SemaphoreSlim _rateSemaphore;
        private static int _limit = 10;
        /// <summary>
        /// Number of seconds the for the Limit of requests (10 seconds for 10 requests etc)
        /// </summary>
        public static int SecondsLimit = 10;
        private static bool RateLimitingEnabled = false;
        private const string APIHeader = "X-MBX-APIKEY";
        private static readonly Stopwatch Stopwatch;
        private static int _concurrentRequests = 0;
        private static TimeSpan _timestampOffset;
        private static readonly object LockObject = new object();

        static RequestClient()
        {
            //var httpClientHandler = new HttpClientHandler
            //{
            //    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            //};

            //HttpClient = new HttpClient(httpClientHandler);
            //HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _rateSemaphore = new SemaphoreSlim(_limit, _limit);
            Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Used to adjust the client timestamp
        /// </summary>
        /// <param name="time">TimeSpan to adjust timestamp by</param>
        public static void SetTimestampOffset(TimeSpan time)
        {
            _timestampOffset = time;
        }

        /// <summary>
        /// Sets whether Rate limiting is enabled or disabled
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetRateLimiting(bool enabled)
        {
            RateLimitingEnabled = enabled;
        }

        /// <summary>
        /// Assigns a new seconds limit
        /// </summary>
        /// <param name="key">Your API Key</param>
        //public static void SetAPIKey(string key)
        //{
        //    if (HttpClient.DefaultRequestHeaders.Contains(APIHeader))
        //    {
        //        lock (LockObject)
        //        {
        //            if (HttpClient.DefaultRequestHeaders.Contains(APIHeader))
        //            {
        //                HttpClient.DefaultRequestHeaders.Remove(APIHeader);
        //            }
        //        }
        //    }
        //    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(APIHeader, new[] { key });
        //}

        /// <summary>
        /// Create a generic GetRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetRequest(HttpClient httpClient, Uri endpoint)
        {
            return await CreateRequest(httpClient, endpoint);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>s
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SignedGetRequest(HttpClient httpClient, Uri endpoint, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(httpClient, uri, HttpVerb.GET);
        }

        /// <summary>
        /// Create a generic PostRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostRequest(HttpClient httpClient, Uri endpoint)
        {
            return await CreateRequest(httpClient, endpoint, HttpVerb.POST);
        }

        /// <summary>
        /// Create a generic PutRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PutRequest(HttpClient httpClient, Uri endpoint)
        {
            return await CreateRequest(httpClient, endpoint, HttpVerb.PUT);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SignedPostRequest(HttpClient httpClient, Uri endpoint, string apiKey, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(httpClient, uri, HttpVerb.POST);
        }

        /// <summary>
        /// Creates a valid Uri with signature
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        /// 
        private static Uri CreateValidUri(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow)
        {
            var timestamp = DateTimeOffset.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ToUnixTimeMilliseconds().ToString();
            var qsDataProvided = !string.IsNullOrEmpty(signatureRawData);
            var argEnding = $"timestamp={timestamp}&recvWindow={receiveWindow}";
            var adjustedSignature = !string.IsNullOrEmpty(signatureRawData) ? $"{signatureRawData.Substring(1)}&{argEnding}" : $"{argEnding}";
            var hmacResult = CreateHMACSignature(secretKey, adjustedSignature);
            var connector = !qsDataProvided ? "?" : "&";
            var uri = new Uri($"{endpoint}{connector}{argEnding}&signature={hmacResult}");
            return uri;
        }

        /// <summary>
        /// Creates a HMACSHA256 Signature based on the key and total parameters
        /// </summary>
        /// <param name="key">The secret key</param>
        /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
        /// <returns></returns>
        private static string CreateHMACSignature(string key, string totalParams)
        {
            var messageBytes = Encoding.UTF8.GetBytes(totalParams);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = new HMACSHA256(keyBytes);
            var computedHash = hash.ComputeHash(messageBytes);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Makes a request to the specifed Uri, only if it hasn't exceeded the call limit 
        /// </summary>
        /// <param name="endpoint">Endpoint to request</param>
        /// <param name="verb"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> CreateRequest(HttpClient httpClient, Uri endpoint, HttpVerb verb = HttpVerb.GET)
        {
            Task<HttpResponseMessage> task = null;

            if (RateLimitingEnabled)
            {
                await _rateSemaphore.WaitAsync();
                if (Stopwatch.Elapsed.Seconds >= SecondsLimit || _rateSemaphore.CurrentCount == 0 || _concurrentRequests == _limit)
                {
                    var seconds = (SecondsLimit - Stopwatch.Elapsed.Seconds) * 1000;
                    var sleep = seconds > 0 ? seconds : seconds * -1;
                    Thread.Sleep(sleep);
                    _concurrentRequests = 0;
                    Stopwatch.Restart();
                }
                ++_concurrentRequests;
            }
            var taskFunction = new Func<Task<HttpResponseMessage>, Task<HttpResponseMessage>>(t =>
            {
                if (!RateLimitingEnabled) return t;
                _rateSemaphore.Release();
                if (_rateSemaphore.CurrentCount != _limit || Stopwatch.Elapsed.Seconds < SecondsLimit) return t;
                Stopwatch.Restart();
                --_concurrentRequests;
                return t;
            });
            switch (verb)
            {
                case HttpVerb.GET:
                    task = await httpClient.GetAsync(endpoint)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.POST:
                    task = await httpClient.PostAsync(endpoint, null)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.DELETE:
                    task = await httpClient.DeleteAsync(endpoint)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.PUT:
                    task = await httpClient.PutAsync(endpoint, null)
                        .ContinueWith(taskFunction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verb), verb, null);
            }
            return await task;
        }
    }
}