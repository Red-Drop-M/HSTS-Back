using Domain.ValueObjects;
using MediatR;
using Application.Features.BloodBagManagement.Commands;
using Microsoft.Extensions.Logging;
using Domain.Repositories;
using Domain.Entities;
using Application.Interfaces;
using Domain.Events;
using Microsoft.Extensions.Options;
using Infrastructure.ExternalServices.Kafka;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class CleanupExpiredBloodBagsHandler : IRequestHandler<CleanupExpiredBloodBagsCommand>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<CleanupExpiredBloodBagsHandler> _logger;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IWebPushService _webPushService;
        private readonly IEventProducer _eventProducer;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly IBloodTransferCenterRepository _centerRepository;
        
        public CleanupExpiredBloodBagsHandler(
            IBloodBagRepository bloodBagRepository,
            ILogger<CleanupExpiredBloodBagsHandler> logger,
            IGlobalStockRepository globalStockRepository,
            IEventProducer eventProducer,
            IOptions<KafkaSettings> kafkaSettings,
            IWebPushService webPushService,
            IBloodTransferCenterRepository centerRepository)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
            _globalStockRepository = globalStockRepository;
            _webPushService = webPushService;
            _centerRepository = centerRepository;
        }
        
        public async Task<Unit> Handle(CleanupExpiredBloodBagsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting expired blood bags cleanup process");
            
            // Use a filter to get all non-expired blood bags
            var filter = new BloodBagFilter
            {
                Status = BloodBagStatus.Ready()
            };
            
            // Get all blood bags with a status that is not expired
            var (bloodBags, _) = await _bloodBagRepository.GetAllAsync(1, int.MaxValue, filter);
            
            _logger.LogInformation("Retrieved {Count} blood bags for expiration check", bloodBags.Count);
            
            if (!bloodBags.Any())
            {
                _logger.LogInformation("No non-expired blood bags found to check");
                return Unit.Value;
            }

            // Group blood bags by blood type AND blood bag type
            var groupedBloodBags = bloodBags
                .GroupBy(b => new { BloodType = b.BloodType, BloodBagType = b.BloodBagType })
                .ToDictionary(
                    g => g.Key, 
                    g => g.ToList()
                );

            _logger.LogInformation("Grouped blood bags into {Count} type combinations", groupedBloodBags.Count);

            // Track changes for global stock update
            var expiredCounts = new Dictionary<(BloodType, BloodBagType), int>();

            // Current date for comparison
            var today = DateOnly.FromDateTime(DateTime.Now);
            _logger.LogInformation("Checking expiration against current date: {Today}", today);

            // Process each group
            foreach (var group in groupedBloodBags)
            {
                var bloodType = group.Key.BloodType;
                var bloodBagType = group.Key.BloodBagType;
                var count = 0;

                _logger.LogInformation("Processing group: BloodType={BloodType}, BloodBagType={BloodBagType}, Count={Count}", 
                    bloodType, bloodBagType, group.Value.Count);

                foreach (var bloodBag in group.Value)
                {
                    // Check if the blood bag has an expiration date and if it's expired
                    if (bloodBag.ExpirationDate.HasValue && bloodBag.ExpirationDate.Value < today)
                    {
                        // Only update if it's not already expired
                        if (bloodBag.Status.Value != BloodBagStatus.Expired().Value)
                        {
                            _logger.LogInformation("Blood bag {Id} is expired. Expiration date: {ExpirationDate}, Current date: {Today}", 
                                bloodBag.Id, bloodBag.ExpirationDate, today);
                            
                            // Update the status to expired
                            bloodBag.UpdateStatus(BloodBagStatus.Expired());
                            await _bloodBagRepository.UpdateAsync(bloodBag);
                            count++;
                            
                            _logger.LogInformation("Blood bag {Id} marked as expired", bloodBag.Id);
                        }
                    }
                }

                if (count > 0)
                {
                    expiredCounts[(bloodType, bloodBagType)] = count;
                    _logger.LogInformation("Marked {Count} {BloodType} {BloodBagType} blood bags as expired", 
                        count, bloodType, bloodBagType);
                }
            }

            // Update global stock for each affected blood type and bag type
            foreach (var item in expiredCounts)
            {
                var bloodType = item.Key.Item1;
                var bloodBagType = item.Key.Item2;
                var count = item.Value;

                _logger.LogInformation("Updating global stock for {BloodType} {BloodBagType}, decreasing by {Count}", 
                    bloodType, bloodBagType, count);

                var globalStock = await _globalStockRepository.GetByKeyAsync(
                    bloodType, bloodBagType);
                    
                if (globalStock != null)
                {
                    globalStock.DecrementAvailableCount(count);
                    await _globalStockRepository.UpdateAsync(globalStock);

                    // Check stock levels and send notifications
                    if (globalStock.IsCritical())
                    {
                        _logger.LogWarning("CRITICAL: Stock for {BloodType} {BloodBagType} is critically low at {Count}", 
                            bloodType, bloodBagType, globalStock.ReadyCount);
                            
                        // Send critical stock notification (global notification - no specific userId)
                        await _webPushService.SendNotificationAsync(
                            title: "CRITICAL BLOOD STOCK ALERT",
                            message: $"Stock for {bloodType.Value} {bloodBagType.Value} is critically low at {globalStock.ReadyCount} units. Immediate action required.",
                            type: "StockCritical",
                            userId: null, // Send to all users
                            link: $"/global-stocks/by-key?BloodType={bloodType.Value}&BloodBagType={bloodBagType.Value}"
                        );
                    }
                    else if (globalStock.IsMinimal())
                    {
                        _logger.LogWarning("MINIMAL: Stock for {BloodType} {BloodBagType} is minimal at {Count}", 
                            bloodType, bloodBagType, globalStock.ReadyCount);
                            
                        // Send minimal stock notification (global notification - no specific userId)
                        await _webPushService.SendNotificationAsync(
                            title: "Blood Stock Running Low",
                            message: $"Stock for {bloodType.Value} {bloodBagType.Value} is below minimum level at {globalStock.ReadyCount} units.",
                            type: "StockMinimal",
                            userId: null, // Send to all users
                            link: $"/global-stocks/by-key?BloodType={bloodType.Value}&BloodBagType={bloodBagType.Value}"
                        );
                    }
                }
            }

            // Publish Kafka messages for all global stocks (not just the ones with notifications)
            var topic = _kafkaSettings.Value.Topics["GlobalStock"];
            var hospital = await _centerRepository.GetPrimaryAsync();

            if (hospital == null)
            {
                _logger.LogWarning("Could not find primary blood center for Kafka events");
            }
            else
            {
                // Get all global stocks to send the latest data
                var allStocks = await _globalStockRepository.GetAllAsync();
                
                foreach (var stock in allStocks)
                {
                    // Create the Kafka message with current stock data
                    var stockData = new GlobalStockData(
                        stock.BloodType,
                        stock.BloodBagType,
                        stock.ReadyCount + stock.CountExpiring + stock.CountExpired,
                        stock.ReadyCount,
                        stock.MinStock,
                        stock.CountExpired
                    );
                    
                    var globalStockEvent = new GlobalStockEvent(
                        hospital.Id,
                        stockData
                    );
                    
                    // Publish to Kafka
                    await _eventProducer.ProduceAsync(topic, System.Text.Json.JsonSerializer.Serialize(globalStockEvent));
                    _logger.LogInformation("Published global stock update for {BloodType} {BloodBagType} to Kafka", 
                        stock.BloodType.Value, stock.BloodBagType.Value);
                }
            }

            _logger.LogInformation("Blood bag expiration check completed. Updated {TotalCount} blood bags",
                expiredCounts.Values.Sum());
                
            return Unit.Value;
        }
    }
}