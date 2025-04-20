using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.BloodRequests;
using Application.DTOs;
using System;
using Application.Features.BloodRequests.Commands;
namespace Application.Features.BloodRequests.Handlers
{
    public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, RequestDto>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<CreateRequestHandler> _logger;

        public CreateRequestHandler(IRequestRepository requestRepository, ILogger<CreateRequestHandler> logger)
        {
            _requestRepository = requestRepository;
            _logger = logger;
        }

        public async Task<RequestDto> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
        {
           try
           {
             var newRequest = new Request(
                request.Priority,
                request.BloodBagType,
                request.RequestDate,
                request.DueDate,
                request.RequestStatus,
                request.MoreDetails,
                request.RequiredQty,
                request.AquiredQty,
                request.ServiceId,
                request.DonorId);

            await _requestRepository.AddAsync(newRequest);
            return new RequestDto
            {
                Id = newRequest.Id,
                Priority = newRequest.Priority,
                BloodGroup = newRequest.BloodGroup,
                BloodBagType = newRequest.BloodBagType,
                RequestDate = newRequest.RequestDate,
                DueDate = newRequest.DueDate,
                Status = newRequest.Status,
                MoreDetails = newRequest.MoreDetails,
                RequiredQty = newRequest.RequiredQty,
                AquiredQty = newRequest.AquiredQty,
                ServiceId = newRequest.ServiceId,
                DonorId = newRequest.DonorId
            };
           }catch (Exception ex)
           {
               _logger.LogError(ex, "Error creating request");
               throw;
           }
            
        }
    }
}