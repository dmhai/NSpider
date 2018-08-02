using NSpider.EventBusRabbitMq.Events;
using NSpider.EventBusRabbitMq.Handler;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSpider.EventBusRabbitMq
{
    public interface IEventBus
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        /// <summary>
        /// 订阅动态事件
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        /// <summary>
        /// 取消订阅动态事件
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="event"></param>
        /// <param name="handler"></param>
        void Subscribe(Type @event, Type handler);

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="event"></param>
        void Publish(IntegrationEvent @event);
    }
}
