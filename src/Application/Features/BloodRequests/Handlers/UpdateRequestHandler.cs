using Domain.Repositories;
using Domain.Events;
using Infrastructure.ExternalServices.Kafka;
using Application.Interfaces;
using Microsoft.Extensions.Options;
using Application.DTOs;
using Application.Features.BloodRequests.Commands;
using Shared.Exceptions;


using MediatR;
using System.Text.Json;
namespace Application.Features.BloodRequests.Handlers
{
    public class UpdateRequestHandler : IRequest<(RequestDto? request , BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IEventProducer _eventProducer;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<UpdateRequestHandler> _logger;
        public UpdateRequestHandler(IRequestRepository requestRepository,ILogger<UpdateRequestHandler>logger,IEventProducer eventProducer,IOptions<KafkaSettings> kafkaSettings)
        {
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
        
            _requestRepository = requestRepository;
            _logger = logger;
        }
        public async Task<(RequestDto? request,BaseException? err)> Handle(UpdateRequestCommand command,CancellationToken ct)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(command.Id);
                if(request ==null)
                {
                    _logger.LogError("request not found ");
                    throw new NotFoundException("no request found with the provided id","updating request");
                }
                if (command.DueDate != null)
                {
                    var topic = _kafkaSettings.Value.Topics["UpdateRequest"];
                    var updateRequestEvent = new UpdateRequestEvent(
                        command.Id,
                        command.Priority,
                        null,
                        null,
                        command.DueDate
                    );
                    var message = JsonSerializer.Serialize(updateRequestEvent);
                    await _eventProducer.ProduceAsync(topic, message);
                    _logger.LogInformation("request updated successfully");
                }
                request.UpdateDetails(command.BloodBagType,command.Priority,command.DueDate,command.MoreDetails,command.RequiredQty);
                await _requestRepository.UpdateAsync(request);
                _logger.LogInformation("request updated successfully");
                var requestDto= new RequestDto
                    {
                        Id = request.Id,
                        Priority = request.Priority.Value,
                        BloodType = request.BloodType.Value,
                        BloodBagType = request.BloodBagType.Value,
                        RequestDate = request.RequestDate,
                        DueDate = request.DueDate,
                        Status = request.Status.Value,
                        MoreDetails = request.MoreDetails,
                        RequiredQty = request.RequiredQty,
                        AquiredQty = request.AquiredQty,
                        ServiceId = request.ServiceId,
                        DonorId = request.DonorId
                    };
                return(requestDto,null);

            }catch(BaseException ex)
            {
                _logger.LogError("error while updating request");
                return (null,ex);
            }
        }
    }
}