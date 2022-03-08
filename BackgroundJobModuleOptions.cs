using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;
using Geex.Common.BackgroundJob.MessageQueue;

using HotChocolate.Execution.Options;

namespace Geex.Common.BackgroundJob
{
    public class BackgroundJobModuleOptions : IGeexModuleOption<BackgroundJobModule>
    {
        public string ConnectionString { get; set; }
        public int WorkerCount { get; set; } = 1;
        public List<RabbitMqSubscribeOptions> MqOptions { get; set; } = new();
    }
}
