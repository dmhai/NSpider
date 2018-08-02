using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSpider.Core.Logging;
using NSpider.EventBusRabbitMq.Events;
using NSpider.EventBusRabbitMq.Handler;
using NSpider.EventBusRabbitMq.PersistentConnection;
using NSpider.EventBusRabbitMq.SubscriptionsManager;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace NSpider.EventBusRabbitMq
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {

        const string BrokerName = "nspider_event_bus";

        const string QueueName = "nspider_event_bus_queue";

        private readonly IRabbitMqPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly ILifetimeScope _autofac;
        private readonly string AUTOFAC_SCOPE_NAME = "nspider_event_bustwo";

        private readonly bool _isServer;

        private IModel _consumerChannel;
        private string _queueName;

        private ILogger _logger;

        public EventBusRabbitMQ(IRabbitMqPersistentConnection persistentConnection,
            ILifetimeScope autofac, IEventBusSubscriptionsManager subsManager, ILogger logger, bool isServer, Action<IEventBus> action = null)
        {
            _isServer = isServer;
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _autofac = autofac;
            _queueName = QueueName;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            _logger = logger;
            if (!isServer)
            {
                action?.Invoke(this);
                _consumerChannel = CreateConsumerChannel();
            }


        }


        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: BrokerName,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.Warn(ex.ToString());
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = @event.GetType()
                    .Name;

                channel.ExchangeDeclare(exchange: BrokerName,
                    type: "direct");

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                policy.Execute(() =>
                {
                    var property = channel.CreateBasicProperties();
                    property.Persistent = true;
                    channel.BasicPublish(exchange: BrokerName,
                        routingKey: eventName,
                        basicProperties: property,
                        body: body);
                });
            }
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            DoInternalSubscription(eventName);
            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);
            _subsManager.AddSubscription<T, TH>();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: _queueName,
                        exchange: BrokerName,
                        routingKey: eventName);
                }
            }
        }

        public void Subscribe(Type @event, Type handler)
        {
            if (_subsManager.HasSubscriptionsForEvent(@event.Name))
                return;
            DoInternalSubscription(@event.Name);
            _subsManager.AddSubscription(@event, handler);
        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }


        public void Dispose()
        {
            _consumerChannel?.Dispose();

            _subsManager.Clear();
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: BrokerName,
                type: "direct");
            //均发,同一时间只处理一个消息
            channel.BasicQos(0, 1, false);

            _queueName = channel.QueueDeclare(QueueName, true, false, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                ProcessEvent(eventName, message).ConfigureAwait(false).GetAwaiter().GetResult();
                //交付
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: QueueName,
                autoAck: false,
                consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME))
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            var handler = scope.ResolveOptional(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                            dynamic eventData = JObject.Parse(message);
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var handler = scope.ResolveOptional(subscription.HandlerType);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                        }
                    }
                }
            }

        }
    }
}
