using NLog;
using System;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using FlushMarketDataBinanceModel;
using FlushMarketDataBinanceApi.Client;
using FlushMarketDataBinanceConsole.Context;

namespace FlushMarketDataBinanceConsole
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            logger.Info($"Запущен {MethodBase.GetCurrentMethod()}");

            Settings.InitConfig();

            var options = GetOptionsDBContext();
            if (options == null)
            {
                logger.Info("options null");
                return;
            }
            
            var client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = Settings.ApiKey,
                SecretKey = Settings.SecretKey
            });

            InitSheduller(options, client);

            logger.Info($"{MethodBase.GetCurrentMethod()} отработал");
        }

        private static DbContextOptions<OrderBookContext> GetOptionsDBContext()
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");
            
            var options = new DbContextOptionsBuilder<Context.OrderBookContext>()
                .UseSqlServer(Settings.ConnectionString)
                .Options;

            logger.Debug($"{MethodBase.GetCurrentMethod()} успешно отработал");

            return options;
        }

        private async static void InitSheduller(DbContextOptions<OrderBookContext> options, BinanceClient client)
        {
            logger.Debug($"Запущен {MethodBase.GetCurrentMethod()}");

            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            var jobData = new JobDataMap();
            jobData.Put("client", client);
            jobData.Put("options", options);

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