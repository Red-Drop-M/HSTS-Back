namespace Application.interfaces
{
    public interface IEventProducer
    {
        Task PublishAsync<TEvent>(TEvent @event, string topic) where TEvent : class;
    }

}