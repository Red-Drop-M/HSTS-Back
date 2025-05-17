// Infrastructure/ExternalServices/Kafka/KafkaConsumerService.cs
using Confluent.Kafka;
using MediatR;
using Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IMediator _mediator;
        private readonly KafkaSettings _settings;
        private readonly ITopicDispatcher _dispatcher;
        private IConsumer<Ignore, string> _consumer;

        public KafkaConsumerService(
            ILogger<KafkaConsumerService> logger,
            IOptions<KafkaSettings> settings,
            IMediator mediator,
            ITopicDispatcher dispatcher)
        {
            _logger = logger;
            _mediator = mediator;
            _settings = settings.Value;
            _dispatcher = dispatcher;

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = $"{Environment.MachineName}-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _consumer.Subscribe(_settings.Topics.Keys);
    
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(ct);
                    var topic = result.Topic;
            
                    // Null check for topic registration
                    var handlerType = _dispatcher.GetHandlerType(topic);
                    var messageType = _dispatcher.GetMessageType(topic);
            
                    if (handlerType is null || messageType is null)
                    {
                        _logger.LogError($"No handler registered for topic {topic}");
                        continue;
                    }

                    // Deserialize with null check
                    var message = JsonSerializer.Deserialize(
                        result.Message.Value, 
                        messageType
                    );

                    if (message is null)
                    {
                        _logger.LogError($"Failed to deserialize message for topic {topic}");
                        continue;
                    }

                    // Safe instance creation
                    var request = Activator.CreateInstance(handlerType, message);
                    if (request is null)
                    {
                        _logger.LogError($"Failed to create handler instance for topic {topic}");
                        continue;
                    }

                    // Explicit type casting
                    await _mediator.Send((IRequest)request, ct);
                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError($"Consume error: {ex.Error.Reason}");
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Deserialization error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka consumption error");
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}