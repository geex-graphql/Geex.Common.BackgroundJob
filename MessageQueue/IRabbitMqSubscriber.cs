using System;
using System.Threading.Tasks;

namespace Geex.Common.BackgroundJob.MessageQueue
{
    /// <summary>
    /// rabbitMq订阅对象
    /// </summary>
    public interface IRabbitMqSubscriber : IDisposable
    {
        /// <summary>
        /// 开启订阅
        /// </summary>
        void Start();
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="eventName"></param>
        public void Subscribe<TEvent>(EventName eventName) where TEvent : class;

        public string Name { get; }
        RabbitMqSubscribeOptions Options { get; }
    }
}