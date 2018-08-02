using RabbitMQ.Client;
using System;

namespace NSpider.EventBusRabbitMq.PersistentConnection
{
    /// <summary>
    /// Rabbitmq持久连接
    /// </summary>
    public interface IRabbitMqPersistentConnection : IDisposable
    {
        /// <summary>
        /// 是否连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 尝试连接
        /// </summary>
        /// <returns></returns>
        bool TryConnect();

        IModel CreateModel();
    }
}
