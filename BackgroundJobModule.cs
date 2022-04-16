using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstraction;
using Geex.Common.Abstractions;
using Geex.Common.BackgroundJob.MessageQueue;

using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;
using Hangfire.JobsLogger;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;

using HotChocolate.Execution.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace Geex.Common.BackgroundJob
{
    [DependsOn(typeof(GeexCoreModule))]
    public class BackgroundJobModule : GeexModule<BackgroundJobModule>
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var moduleOptions = Configuration.GetModuleOptions<BackgroundJobModuleOptions>();

            foreach (var mqOptions in moduleOptions.MqOptions)
            {
                context.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IRabbitMqSubscriber, RabbitMqSubscriber>(x => new RabbitMqSubscriber(mqOptions, x.GetService<ILogger<RabbitMqSubscriber>>(), x.GetService<IServiceScopeFactory>())));
            }

            context.Services.AddHangfire(configuration =>
            {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseJobsLogger()
                    .UseFilter(new AutomaticRetryAttribute()
                    { Attempts = 3, DelaysInSeconds = new[] { 30, 3600, 3600 * 24 } })
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    //.UseActivator(new AspNetCoreJobActivator(context.Services.GetSingletonInstance<IServiceScopeFactory>()))
                    .UseMongoStorage(moduleOptions.ConnectionString, new MongoStorageOptions()
                    {
                        MigrationOptions = new MongoMigrationOptions()
                        {
                            MigrationStrategy = new MigrateMongoMigrationStrategy()
                        }
                    });
            });

            // Add the processing server as IHostedService
            context.Services.AddHangfireServer(x =>
            {
                x.WorkerCount = moduleOptions.WorkerCount;
                x.SchedulePollingInterval = TimeSpan.FromSeconds(3);
            });
            base.ConfigureServices(context);
        }

        /// <inheritdoc />
        public override Task OnPreApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            app.UseEndpoints(endpoints => endpoints.MapHangfireDashboard());

            return base.OnPreApplicationInitializationAsync(context);
        }

        /// <inheritdoc />
        public override Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var subscribers = context.ServiceProvider.GetServices<IRabbitMqSubscriber>();
            foreach (var subscriber in subscribers)
            {
                subscriber.Start();
            }
            return base.OnPostApplicationInitializationAsync(context);
        }
    }
}
