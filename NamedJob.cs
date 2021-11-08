using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hangfire.Common;

namespace Geex.Common.BackgroundJob
{
    public abstract class NamedJob<TJob, TParam> : Job where TJob : Job, new()
    {
        public abstract Task Run(TParam param);
        protected NamedJob(TParam param = default) : base(typeof(TJob).GetMethod(nameof(Run)), param)
        {
        }
    }
    public abstract class NamedJob<TJob> : Job where TJob : Job, new()
    {
        public abstract Task Run();
        protected NamedJob() : base(typeof(TJob).GetMethod(nameof(Run)))
        {
        }
    }
}
