﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasyCronJob.Abstractions;

using Geex.Common.Abstractions;
using Geex.Common.BackgroundJob;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Microsoft_Extensions_DependencyInjection
    {
        /// <summary>
        /// 注册job, 如果不传入cron表达式则默认读取配置[BackgroundJobModuleOptions:JobConfigs:<Job名称>]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="cronExp"></param>
        /// <exception cref="Exception"></exception>
        public static void AddJob<T>(this IServiceCollection services, string? cronExp = default) where T : CronJob<T>
        {
            var jobType = typeof(T);
            var jobName = jobType.Name;
            cronExp ??= services.GetRequiredService<BackgroundJobModuleOptions>().JobConfigs.GetValueOrDefault(jobName);
            if (cronExp.IsNullOrEmpty())
            {
                throw new Exception($"[{jobName}]未配置, 查找配置结点: [{nameof(BackgroundJobModuleOptions)}:{nameof(BackgroundJobModuleOptions.JobConfigs)}:{jobName}]");
            }
            services.AddHostedService(x => Activator.CreateInstance(jobType, x, cronExp).As<IHostedService>());
        }
    }
}
