using MediatR;
namespace  Application.Interfaces
{
    public interface ITopicDispatcher
    {
        void Register<TCommand>(string topic) where TCommand : IRequest;
        Type GetHandlerType(string topic);
        Type GetMessageType(string topic);
    }
}