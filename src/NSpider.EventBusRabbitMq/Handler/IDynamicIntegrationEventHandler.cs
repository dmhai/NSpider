using System.Threading.Tasks;

namespace NSpider.EventBusRabbitMq.Handler
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
