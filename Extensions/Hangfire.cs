using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geex.Common.Gql;
using Geex.Common.BackgroundJob;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    public static class HangfireExtension
    {
        public static string Run<TJob>(this IBackgroundJobClient client) where TJob : Job
        {
            return client.Create(Activator.CreateInstance<TJob>(), new EnqueuedState());
        }

        public static string Run<TJob, TParam>(this IBackgroundJobClient client, TParam param) where TJob : NamedJob<TJob, TParam>, new()
        {
            return client.Create(() => Activator.CreateInstance<TJob>().Run(param), new EnqueuedState());
        }


        public static void Schedule<TJob>(this IRecurringJobManager client, string cronExpression) where TJob : Job
        {
            client.AddOrUpdate(typeof(TJob).Name, Activator.CreateInstance<TJob>(), cronExpression, TimeZoneInfo.Local);
        }

        public static void Schedule<TJob, TParam>(this IRecurringJobManager client, TParam param, string cronExpression) where TJob : NamedJob<TJob, TParam>, new()
        {
            client.AddOrUpdate(typeof(TJob).Name, () => Activator.CreateInstance<TJob>().Run(param), cronExpression, TimeZoneInfo.Local);
        }
    }
}
