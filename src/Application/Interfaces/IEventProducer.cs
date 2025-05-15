namespace Application.interfaces
{
    public interface IEventProducer
    {
        Task ProduceAsync<TEvent>(TEvent @event, string topic) where TEvent : class;
    }

}