using Geex.Common.Abstractions;

namespace Geex.Common.BackgroundJob.MessageQueue
{
    public abstract class EventName : Enumeration<EventName, string>
    {
        public EventName(string value) : base(value)
        {

        }
    }
}
