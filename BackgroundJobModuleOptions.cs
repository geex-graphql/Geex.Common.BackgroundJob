using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using HotChocolate.Execution.Options;

namespace Geex.Common.BackgroundJob
{
    public class BackgroundJobModuleOptions : IGeexModuleOption<BackgroundJobModule>
    {
        public string ConnectionString { get; set; }
        public int WorkerCount { get; set; } = 1;
    }
}
