using System;
using System.Threading.Tasks;

namespace Geex.Common.BackgroundJob.MessageQueue
{

    public interface IRabbitMqEventHandler<TEvent> where TEvent : class
    {
        Task Handle(TEvent @event);
    }
}