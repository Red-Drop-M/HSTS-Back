
using Application.Interfaces;
using MediatR;
// Infrastructure/ExternalServices/Kafka/TopicDispatcher.cs
namespace Infrastructure.ExternalServices
{public class TopicDispatcher : ITopicDispatcher
{
    private readonly Dictionary<string, Type> _handlerRegistry = new();
    private readonly Dictionary<string, Type> _messageRegistry = new();

    public void Register<TCommand>(string topic) where TCommand : IRequest
    {
        var commandType = typeof(TCommand);
        var messageType = commandType
            .GetConstructors()
            .First()
            .GetParameters()
            .First()
            .ParameterType;

        _handlerRegistry[topic] = commandType;
        _messageRegistry[topic] = messageType;
    }

    public Type? GetHandlerType(string topic) => 
        _handlerRegistry.TryGetValue(topic, out var type) ? type : null;

    public Type? GetMessageType(string topic) => 
        _messageRegistry.TryGetValue(topic, out var type) ? type : null;
}}