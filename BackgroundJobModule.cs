using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;

using HotChocolate.Execution.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace Geex.Common.BackgroundJob
{
    public class BackgroundJobModule : GeexModule<BackgroundJobModule>
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var options = this.ModuleOptions as BackgroundJobModuleOptions;
            context.Services.AddHangfire(configuration =>
            {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseFilter(new AutomaticRetryAttribute()
                    { Attempts = 3, DelaysInSeconds = new[] { 30, 3600, 3600 * 24 } })
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    //.UseActivator(new AspNetCoreJobActivator(context.Services.GetSingletonInstance<IServiceScopeFactory>()))
                    .UseMongoStorage(options.ConnectionString, new MongoStorageOptions()
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
                x.WorkerCount = options.WorkerCount;
                x.SchedulePollingInterval = TimeSpan.FromSeconds(3);
            });
            base.ConfigureServices(context);
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            app.UseHangfireDashboard();
            app.UseEndpoints(endpoints =>
             {
                 endpoints.MapHangfireDashboard();
             });
            base.OnApplicationInitialization(context);
        }
    }
}
