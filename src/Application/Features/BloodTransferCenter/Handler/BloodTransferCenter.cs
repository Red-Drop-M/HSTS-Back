using MediatR;
using Domain.Entities;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.BloodTransferCenterManagement.Commands;
using Shared.Exceptions;
using Application.Interfaces;
using Infrastructure.ExternalServices.Kafka;
using Microsoft.Extensions.Options;
using Domain.Events;

namespace Application.Features.BloodTransferCenterManagement.Handlers
{
    public class CreateBloodTransferCenterHandler : IRequestHandler<CreateBloodTransferCenterCommand, (BloodTransferCenterDTO? center, BaseException? err)>
    {
        private readonly IBloodTransferCenterRepository _centerRepository;
        private readonly IEventProducer _eventProducer;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly IWilayaRepository _wilayaRepository;
        private readonly ILogger<CreateBloodTransferCenterHandler> _logger;

        public CreateBloodTransferCenterHandler(
            IBloodTransferCenterRepository centerRepository,
            IWilayaRepository wilayaRepository,
            ILogger<CreateBloodTransferCenterHandler> logger,
            IGlobalStockRepository globalStockRepository,
            IEventProducer eventProducer,
            IOptions<KafkaSettings> kafkaSettings)
        {
            _centerRepository = centerRepository;
            _eventProducer = eventProducer;
            _globalStockRepository = globalStockRepository;
            _kafkaSettings = kafkaSettings;
            _wilayaRepository = wilayaRepository;
            _logger = logger;
        }

        public async Task<(BloodTransferCenterDTO? center, BaseException? err)> Handle(
            CreateBloodTransferCenterCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating blood transfer center with name: {Name}", command.Name);
                
                // Check if wilaya exists
                var wilaya = await _wilayaRepository.GetByIdAsync(command.WilayaId);
                if (wilaya == null)
                {
                    return (null, new NotFoundException($"Wilaya with ID {command.WilayaId} not found", "CreateBloodTransferCenter"));
                }
                
                // Check if a center with the same name already exists
                var existingCenter = await _centerRepository.GetByNameAsync(command.Name);
                if (existingCenter != null)
                {
                    return (null, new BadRequestException($"Blood transfer center with name '{command.Name}' already exists", "CreateBloodTransferCenter"));
                }

                var newCenter = new BloodTransferCenter(
                    command.Name,
                    command.Address,
                    command.Email,
                    command.PhoneNumber,
                    command.WilayaId,
                    command.IsPrimary);

                await _centerRepository.AddAsync(newCenter);
                
                // If this is marked as primary, we need to update other centers
                if (command.IsPrimary)
                {
                    await _centerRepository.SetAsPrimaryAsync(newCenter.Id);
                }

                _logger.LogInformation("Blood transfer center created successfully with ID: {Id}", newCenter.Id);
                var topic = _kafkaSettings.Value.Topics["CTSInit"];
                var stocks = await _globalStockRepository.GetAllAsync();
                var globalStocks = new List<GlobalStockData>();
                foreach (var stock in stocks)
                {
                    var eventMessage = new GlobalStockData(
                     stock.BloodType,
                     stock.BloodBagType,
                     stock.ReadyCount + stock.CountExpired + stock.CountExpiring,
                     stock.ReadyCount,
                     stock.MinStock,
                     stock.CountExpired
                     );
                     globalStocks.Add(eventMessage);

                }
                var cts = await _centerRepository.GetPrimaryAsync();
                var initEvent = new CtsData(
                    newCenter.Id,
                    newCenter.Name,
                    newCenter.Address,
                    newCenter.Email,
                    newCenter.PhoneNumber,
                    newCenter.Wilaya.Name,
                    wilaya.Name,
                    globalStocks
                );
                await _eventProducer.ProduceAsync(topic, initEvent);

                return (new BloodTransferCenterDTO
                {
                    Id = newCenter.Id,
                    Name = newCenter.Name,
                    Address = newCenter.Address,
                    Email = newCenter.Email,
                    PhoneNumber = newCenter.PhoneNumber,
                    WilayaId = newCenter.WilayaId,
                    WilayaName = wilaya.Name,
                    IsPrimary = newCenter.IsPrimary
                }, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating blood transfer center");
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blood transfer center");
                return (null, new InternalServerException("An error occurred while creating the blood transfer center", "CreateBloodTransferCenter"));
            }
        }
    }
}