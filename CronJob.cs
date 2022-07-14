using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Cronos;

using EasyCronJob.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Geex.Common.BackgroundJob
{
    public abstract class CronJob<TImplementation> : CronJobService
    {
        private readonly ILogger<TImplementation> _logger;

        /// <inheritdoc />
        public CronJob(IServiceProvider sp, string cronExp)
            : base(cronExp, TimeZoneInfo.Local, cronExp.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length >= 6 ? CronFormat.IncludeSeconds : CronFormat.Standard)
        {
            this._logger = sp.GetService<ILogger<TImplementation>>();
        }

        /// <inheritdoc />
        protected override Task ScheduleJob(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Job scheduled: [{JobName}]", typeof(TImplementation).Name);
            return base.ScheduleJob(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Job starting: [{JobName}]", typeof(TImplementation).Name);
            try
            {
                await base.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogErrorWithData(e, "Job failed to start: [{JobName}]", typeof(TImplementation).Name);
            }
            _logger.LogDebug("Job started: [{JobName}]", typeof(TImplementation).Name);
        }
        /// <summary>
        /// 真正执行逻辑
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task Run(CancellationToken cancellationToken);

        /// <inheritdoc />
        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Job processing: [{JobName}]", typeof(TImplementation).Name);
            try
            {
                await this.Run(cancellationToken);
                _logger.LogDebug("Job processed: [{JobName}]", typeof(TImplementation).Name);
            }
            catch (Exception e)
            {
                _logger.LogErrorWithData(e, "Job failed: [{JobName}]", typeof(TImplementation).Name);
            }
        }

        /// <inheritdoc />
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Job stopping: [{JobName}]", typeof(TImplementation).Name);
            try
            {
                await base.StopAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogErrorWithData(e, "Job failed to stop: [{JobName}]", typeof(TImplementation).Name);
            }
            _logger.LogDebug("Job stopped: [{JobName}]", typeof(TImplementation).Name);

        }
    }
}
