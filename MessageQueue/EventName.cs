using Geex.Common.Abstractions;

namespace Geex.Common.BackgroundJob.MessageQueue
{
    public abstract class EventName : Enumeration<EventName>
    {
        public EventName(string value) : base(value)
        {

        }
    }
}
