using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.BackgroundJob.MessageQueue;

using Microsoft.Extensions.DependencyInjection;

namespace Geex.Common.BackgroundJob.MessageQueue
{
    public static class MessageQueueExtensions
    {
        public static IServiceCollection AddMqEventHandler<TEvent, THandler>(this IServiceCollection services) where TEvent : class where THandler : class, IRabbitMqEventHandler<TEvent>
        {
            services.AddScoped<IRabbitMqEventHandler<TEvent>, THandler>();
            return services;
        }
    }
}
