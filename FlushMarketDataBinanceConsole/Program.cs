using NLog;
using System;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using FlushMarketDataBinanceModel.SettingsApp;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using FlushMarketDataBinanceModel.DB;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            logger.Info($"Запущен {MethodBase.GetCurrentMethod()}");

            Settings.InitConfig();

            GetProxy();
            InitSheduller();

            logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }


        private async static void InitSheduller()
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            var jobData = new JobDataMap();
            //jobData.Put("options", options);


            var job = JobBuilder.Create<FlushMarketData>()
                .WithIdentity("job1", "group1")
                .SetJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .RepeatForever())
                    .WithCronSchedule(Settings.CronExpression)
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();

            Console.ReadKey();

            await scheduler.Shutdown();

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }

        private async static void GetProxy()
        {
            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36 OPR/71.0.3770.323");
                    httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
                    var sw = new Stopwatch();
                    sw.Start();
                    var result = httpClient.GetStringAsync($"https://hidemy.name/ru/proxy-list/?country=US&maxtime=600&type=hs#list").Result;
                    sw.Stop();
                    var output = string.Empty;
                    var matches = Regex.Matches(result, @"(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}</td>)|(\d{2,5}</td>)");
                    int matchesCount = matches.Count;
                    for (int i = 0; i < matchesCount; i += 2)
                    {
                        if (!Regex.Match(matches[i].Value, @"([A-Za-z-])").Success || matches[i].Value.Contains("<"))
                        {
                            var ip = matches[i]?.Value?.Replace("</td>", "");
                            var strPort = matches[i + 1]?.Value?.Replace("</td>", string.Empty);
                            int.TryParse(strPort, out int port);
                            Settings.ProxyList.Add(new Proxy(ip, port));
                        }
                    }
                       
                            

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}