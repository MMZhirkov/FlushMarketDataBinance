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
            Helper.FillProxy();
            InitSheduller();

            logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }


        private async static void InitSheduller()
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            var jobData = new JobDataMap();
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
    }
}