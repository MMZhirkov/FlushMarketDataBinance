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
            var jobFlushMarketData = JobBuilder.Create<FlushMarketData>()
                .WithIdentity("jobFlushMarketData", "groupFlushMarketData")
                .Build();
            var triggerFlushMarketData = TriggerBuilder.Create()
                .WithIdentity("triggerFlushMarketData", "groupFlushMarketData")
                .StartAt(DateTime.UtcNow.AddMinutes(5))
                .WithSimpleSchedule(x => x
                    .RepeatForever())
                    .WithCronSchedule(Settings.CronExpressionFlushMarketData)
                .Build();
            var jobFillProxyList = JobBuilder.Create<FillProxyList>()
                .WithIdentity("jobFillProxyList", "groupFillProxyList")
                .Build();
            var triggerFillProxyList = TriggerBuilder.Create()
               .WithIdentity("triggerFillProxyList", "groupFillProxyList")
               .StartNow()
               .WithSimpleSchedule(x => x
                   .RepeatForever())
                   .WithCronSchedule(Settings.CronExpressionFillProxy)
               .Build();
            var jobFillProxyListOnlyStartProgram = JobBuilder.Create<FillProxyList>()
               .WithIdentity("jobFillProxyListOnlyStartProgram", "groupFillProxyListOnlyStartProgram")
               .Build();
            var triggerFillProxyListOnlyStartProgram = TriggerBuilder.Create()
              .WithIdentity("triggerFillProxyListOnlyStartProgram", "groupFillProxyListOnlyStartProgram")
              .StartNow()
              .Build();

            await scheduler.ScheduleJob(jobFillProxyListOnlyStartProgram, triggerFillProxyListOnlyStartProgram);
            await scheduler.ScheduleJob(jobFlushMarketData, triggerFlushMarketData);
            await scheduler.ScheduleJob(jobFillProxyList, triggerFillProxyList);
            await scheduler.Start();

            Console.ReadKey();

            await scheduler.Shutdown();

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");
        }
    }
}