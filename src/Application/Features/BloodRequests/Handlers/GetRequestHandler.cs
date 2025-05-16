using MediatR;
using Application.DTOs;
using Domain.ValueObjects;
using Application.Features.BloodRequests.Queries;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using System;
namespace Application.Features.BloodRequests.Handlers
{
    public class  GetRequestHandler : IRequestHandler<GetRequestQuery, (RequestDto? request, BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<GetRequestHandler> _logger;

        public GetRequestHandler(IRequestRepository  requestRepository, ILogger<GetRequestHandler> logger)
        {
            _requestRepository = requestRepository;
            _logger = logger;
        }
        public async Task<(RequestDto? request, BaseException?err)> Handle(GetRequestQuery Query ,CancellationToken ct)
        {
            try{
                var request = await _requestRepository.GetByIdAsync(Query.Id);
                if(request == null)
                { 
                    _logger.LogError("Request not found");
                    return( null,new NotFoundException("Request not found", "get request"));
                }
                var requestDto = new RequestDto
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
                return (requestDto, null);
            }catch(BaseException ex)
            {
                _logger.LogError(ex.Message, "an internal exception occurred");
                return (null, ex);
            }
        }
    }
}