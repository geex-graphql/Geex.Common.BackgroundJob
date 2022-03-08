using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Geex.Common;
using Geex.Common.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Geex.Common.BackgroundJob.MessageQueue
{
    public class RabbitMqSubscriber : IRabbitMqSubscriber
    {
        private static readonly Func<Type, MethodInfo> _handlerMethodInfo = (type) => type.GetMethod(nameof(IRabbitMqEventHandler<object>.Handle))!;
        private ConcurrentDictionary<string, Type> EventSubscribes = new(StringComparer.OrdinalIgnoreCase);
        public void Subscribe<TEvent>(EventName eventName) where TEvent : class
        {
            if (!EventSubscribes.ContainsKey(eventName))
            {
                EventSubscribes.TryAdd(eventName, typeof(TEvent));
            }
            else
            {
                throw new BusinessException(GeexExceptionType.Conflict, message: $"重复添加MqEventHandler<{typeof(TEvent)}>: [{eventName}]");
            }
        }

        public ConnectionFactory Factory { get; }
        public string Name => this.Options.Name;

        public RabbitMqSubscribeOptions Options { get; }
        private readonly ILogger<RabbitMqSubscriber> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public void Start()
        {
            var channel = this.GetChannel();
            foreach (var eventName in EventSubscribes.Keys)
            {
                channel.QueueBind(this.Options.QueueName, this.Options.ExchangeName, eventName);
            }
        }

        public RabbitMqSubscriber(RabbitMqSubscribeOptions options, ILogger<RabbitMqSubscriber> logger, IServiceScopeFactory scopeFactory)
        {

            Options = options;
            this._logger = logger;
            this._scopeFactory = scopeFactory;
            this.Factory = new ConnectionFactory()
            {
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                Port = options.Port,
                HostName = options.ServerAddress,
                AutomaticRecoveryEnabled = options.AutomaticRecoveryEnabled,
                ClientProvidedName = options.ClientProvidedName,
            };
        }
        #region Connection
        private IConnection? _connection;
        protected IConnection Connection => _connection ??= Factory.CreateConnection();

        #endregion
        #region Channel
        private IModel _channel;
        protected IModel GetChannel()
        {
            if (_channel != null) return _channel;
            _channel = Connection.CreateModel();
            _channel.ConfirmSelect();
            _channel.ExchangeDeclare(this.Options.ExchangeName, ExchangeType.Fanout);
            _channel.QueueDeclare(this.Options.QueueName, false, false, false, null);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, e) =>
            {
                await EventReceivedAsync(_, e);
            };
            _channel.BasicConsume(this.Options.QueueName, false, consumer: consumer);
            return _channel;
        }
        #endregion
        private async Task EventReceivedAsync(object sender, BasicDeliverEventArgs args)
        {
            var jsonString = Encoding.UTF8.GetString(args.Body.Span);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sp = scope.ServiceProvider;
                using var uow = sp.GetService<IUnitOfWork>();
                if (this.EventSubscribes.TryGetValue(args.RoutingKey, out var eventType))
                {
                    var handlerType = typeof(IRabbitMqEventHandler<>).MakeGenericType(eventType);
                    var handler = sp.GetService(handlerType);
                    await handler.Call(_handlerMethodInfo(handlerType), new[] { jsonString.ToObject(eventType) }).As<Task>();
                    //await handler.Handle(jsonString.ToObject(null));
                    await uow.CommitAsync();
                    _channel.BasicAck(args.DeliveryTag, false);
                    _logger.LogInformation("完成mq订阅执行:" + Environment.NewLine + $"{args.Exchange}:{args.RoutingKey}:{jsonString}");
                }
                else
                {
                    _channel.BasicAck(args.DeliveryTag, false);
                    _logger.LogInformation("跳过mq订阅执行:" + Environment.NewLine + $"{args.Exchange}:{args.RoutingKey}:{jsonString}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("mq订阅执行失败:" + Environment.NewLine + $"{args.Exchange}:{args.RoutingKey}:{jsonString}");
                _logger.LogException(e);
                _channel.BasicNack(args.DeliveryTag, false, false);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
