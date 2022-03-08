using System.Threading.Tasks;

using RabbitMQ.Client;

namespace Geex.Common.BackgroundJob.MessageQueue
{
    public class RabbitMqSubscribeOptions
    {
        /// <summary>
        /// 配置名称, 用以区分多个订阅
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 挂载事件订阅的exchange名称
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// 连接用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 连接用户密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 虚拟host名称
        /// </summary>
        public string VirtualHost { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 5672;
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerAddress { get; set; }
        /// <summary>
        /// 是否启动自动恢复
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; }
        /// <summary>
        /// 客户端识别名称
        /// </summary>
        public string ClientProvidedName { get; set; }
    }
}
