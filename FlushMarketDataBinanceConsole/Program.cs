using NLog;
using System;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using FlushMarketDataBinanceModel.SettingsApp;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            logger.Info($"Запущен {MethodBase.GetCurrentMethod()}");

            Settings.InitConfig();
            InitSheduller();

            logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }


        private async static void InitSheduller()
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();

            #region scheduler FlushMarketData
            var jobFlushMarketData = JobBuilder.Create<FlushMarketData>()
                .WithIdentity("jobFlushMarketData")
                .Build();
            var triggerFlushMarketData = TriggerBuilder.Create()
                .WithIdentity("triggerFlushMarketData", "groupFlushMarketData")
                .StartAt(DateTime.UtcNow.AddMinutes(3))
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(Settings.IntervalFlushMarketData.Value)
                    .RepeatForever())
                .ForJob(jobFlushMarketData)
                .Build();
            #endregion

            #region scheduler FillProxyList
            var jobFillProxyList = JobBuilder.Create<FillProxyList>()
                .WithIdentity("jobFillProxyList")
                .Build();
            var triggerFillProxyList = TriggerBuilder.Create()
               .WithIdentity("triggerFillProxyList", "groupFillProxyList")
               .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(Settings.IntervalFillProxy.Value)
                    .RepeatForever())
               .ForJob(jobFillProxyList)
               .Build();
            #endregion

            await scheduler.ScheduleJob(jobFlushMarketData, triggerFlushMarketData);
            await scheduler.ScheduleJob(jobFillProxyList, triggerFillProxyList);
            await scheduler.Start();

            Console.ReadKey();

            await scheduler.Shutdown();

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }
    }
}