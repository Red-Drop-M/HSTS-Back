using Application.Interfaces;
using Confluent.Kafka;
using MediatR;
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
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly CancellationTokenSource _cts = new();

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
                EnableAutoCommit = false,
                EnablePartitionEof = true
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_settings.Topics.Keys);
            
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                        if (result?.IsPartitionEOF ?? true) continue;

                        await ProcessMessageAsync(result);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"Consume error: {ex.Error.Reason}");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            finally
            {
                _consumer.Close();
            }
        }

        private async Task ProcessMessageAsync(ConsumeResult<Ignore, string> result)
        {
            try
            {
                var topic = result.Topic;
                var handlerType = _dispatcher.GetHandlerType(topic);
                var messageType = _dispatcher.GetMessageType(topic);

                if (handlerType == null || messageType == null)
                {
                    _logger.LogError($"No handler registered for topic {topic}");
                    return;
                }

                var message = JsonSerializer.Deserialize(result.Message.Value, messageType);
                if (message == null)
                {
                    _logger.LogError($"Failed to deserialize message for topic {topic}");
                    return;
                }

                var request = Activator.CreateInstance(handlerType, message);
                if (request is not IRequest mediatorRequest)
                {
                    _logger.LogError($"Invalid request type for topic {topic}");
                    return;
                }

                await _mediator.Send(mediatorRequest);
                _consumer.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            _cts?.Dispose();
            base.Dispose();
        }
    }
}