using NSpider.Core.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;

namespace NSpider.EventBusRabbitMq.PersistentConnection
{
    /// <summary>
    /// 默认rabbitmq持久连接类
    /// </summary>
    public class DefaultRabbitMqPersistentConnection
        : IRabbitMqPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private ILogger Logger { get; set; }

        IConnection _connection;
        bool _disposed;

        readonly object _syncRoot = new object();

        public DefaultRabbitMqPersistentConnection(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        /// <summary>
        /// 创建AMQP模型
        /// </summary>
        /// <returns></returns>
        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("没有可以执行此操作的RabbitMQ连接");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                Logger.Fatal(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            Logger.Info("RabbitMQ客户端正在尝试连接");

            lock (_syncRoot)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        Logger.Warn(ex.ToString());
                    }
                    );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                        .CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    Logger.Info($"RabbitMQ持久连接获得了一个 {_connection.Endpoint.HostName} 并订阅了异常失败事件");

                    return true;
                }
                Logger.Fatal("致命错误:无法创建和打开RabbitMQ连接");

                return false;
            }
        }

        /// <summary>
        /// 连接成功事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            Logger.Info("rabbitmq连接没有被打开,正在尝试连接");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            Logger.Warn("rabbitmq连接没有被打开,正在尝试连接");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            Logger.Warn("rabbitmq连接没有被打开,正在尝试连接");

            TryConnect();
        }
    }
}
