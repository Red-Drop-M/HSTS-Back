using Application.Interfaces;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IMediator _mediator;
        private readonly KafkaSettings _settings;
        private readonly ITopicDispatcher _dispatcher;
        private readonly CancellationTokenSource _cts = new();
        private IConsumer<Ignore, string>? _consumer;

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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Move actual work to a separate method
            await Task.Factory.StartNew(async () => {
                await RunConsumerLoop(stoppingToken);
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task RunConsumerLoop(CancellationToken stoppingToken)
        {
            // Create consumer inside ExecuteAsync instead of constructor
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId ?? $"{Environment.MachineName}-consumer-group",
                AutoOffsetReset = Enum.TryParse<AutoOffsetReset>(_settings.AutoOffsetReset, true, out var offsetReset) 
                    ? offsetReset 
                    : AutoOffsetReset.Latest,
                EnableAutoCommit = false,
                EnablePartitionEof = true
            };

            try
            {
                _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                
                // Test connection to Kafka before subscribing
                _logger.LogInformation("Attempting to connect to Kafka at {BrokerAddress}", _settings.BootstrapServers);
                
                // Get the list of topics we should subscribe to - REMOVE DUPLICATES
                if (_settings.ConsumerTopics?.Any() == true)
                {
                    // Important: Remove duplicate topics to avoid Kafka errors
                    var uniqueTopics = _settings.ConsumerTopics.Distinct().ToList();
                    _logger.LogInformation("Configured topics for subscription: {Topics}", 
                        string.Join(", ", uniqueTopics));
                    
                    try {
                        // Subscribe directly - don't try to create topics
                        _consumer.Subscribe(uniqueTopics);
                        _logger.LogInformation("Successfully subscribed to topics");
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Failed to subscribe to topics");
                        return; // Exit early but allow the application to continue
                    }
                    
                    // Process messages
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
                else
                {
                    _logger.LogWarning("No consumer topics configured.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Kafka consumer service. Service will stop, but application will continue.");
            }
            finally
            {   
                _logger.LogInformation("Closing Kafka consumer");
                _consumer?.Close();
            }
        }

        private async Task ProcessMessageAsync(ConsumeResult<Ignore, string> result)
        {
            try
            {
                // Log the raw message for debugging
                _logger.LogInformation("Received message from topic {Topic}: {Message}", 
                    result.Topic, result.Message.Value);
                    
                var topic = result.Topic;
                var handlerType = _dispatcher.GetHandlerType(topic);
                var messageType = _dispatcher.GetMessageType(topic);

                if (handlerType == null || messageType == null)
                {
                    _logger.LogError($"No handler registered for topic {topic}");
                    return;
                }

                // Add custom JSON options with all needed converters
                var options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new DateOnlyConverter());
                options.Converters.Add(new PledgeStatusConverter());

                // Use the custom options for deserialization
                var message = JsonSerializer.Deserialize(result.Message.Value, messageType, options);
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

        // Add this custom DateOnly converter class
        private class DateOnlyConverter : JsonConverter<DateOnly>
        {
            private const string Format = "yyyy-MM-dd";

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    return default;

                // Try parsing with various formats
                if (DateOnly.TryParse(value, out var result))
                    return result;

                if (DateOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return result;

                if (DateTime.TryParse(value, out var dateTime))
                    return DateOnly.FromDateTime(dateTime);

                // Log the actual value that couldn't be parsed
                throw new FormatException($"Could not parse DateOnly from '{value}'");
            }

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format));
            }
        }

        // Update the PledgeStatusConverter class
        private class PledgeStatusConverter : JsonConverter<Domain.ValueObjects.PledgeStatus>
        {
            public override Domain.ValueObjects.PledgeStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return Domain.ValueObjects.PledgeStatus.Pledged; // Default value if empty

                    try
                    {
                        // Use the FromString factory method on your value object
                        return Domain.ValueObjects.PledgeStatus.FromString(stringValue);
                    }
                    catch (Exception ex)
                    {
                        //_logger.LogWarning(ex, "Failed to parse PledgeStatus from '{Status}', using default value", stringValue);
                        return Domain.ValueObjects.PledgeStatus.Pledged; // Default to Pledged on failure
                    }
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // For object representation, try to extract the Value property
                    string? value = null;
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName && 
                            reader.GetString() == "Value" && reader.Read() && 
                            reader.TokenType == JsonTokenType.String)
                        {
                            value = reader.GetString();
                            break;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            return Domain.ValueObjects.PledgeStatus.FromString(value);
                        }
                        catch
                        {
                            return Domain.ValueObjects.PledgeStatus.Pledged;
                        }
                    }
                }

                // Default when we don't know what to do
               // _logger.LogWarning("Unhandled token type {TokenType} when parsing PledgeStatus, using default value", reader.TokenType);
                return Domain.ValueObjects.PledgeStatus.Pledged;
            }

            public override void Write(Utf8JsonWriter writer, Domain.ValueObjects.PledgeStatus value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}