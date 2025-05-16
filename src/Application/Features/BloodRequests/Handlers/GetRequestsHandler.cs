using Application.DTOs;
using Application.Features.BloodRequests.Queries;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Repositories;
using Shared.Exceptions;
using System.Collections.Generic;
namespace Application.Features.BloodRequests.Handlers
{
    public class GetRequestsHandler : IRequestHandler<GetRequestsQuery,(List<RequestDto>? requests, int? total,BaseException? err)>
    {
        private readonly IRequestRepository _bloodRequestRepository;
        private readonly ILogger<GetRequestsHandler> _logger;

        public GetRequestsHandler(IRequestRepository bloodRequestRepository, ILogger<GetRequestsHandler> logger)
        {
            _bloodRequestRepository = bloodRequestRepository;
            _logger = logger;
        }

        public async Task<(List<RequestDto>? requests,int? total, BaseException? err)> Handle(GetRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var filter = new RequestFilter
                {   
                  
                    Priority = request.Priority,
                    BloodBagType = request.BloodBagType,
                    RequestDate = request.RequestDate,
                    DueDate = request.DueDate,
                    DonorId = request.DonorId,
                    ServiceId = request.ServiceId,
                    Status = request.Status,
                    BloodType = request.BloodType
                };

                var (requests,total) = await _bloodRequestRepository.GetAllAsync(request.Page,request.PageSize,filter);
                if (requests == null || requests.Count == 0)
                {
                    _logger.LogWarning("No blood requests found");
                    return (null, null, new NotFoundException("No blood requests found", "Fetching blood requests"));
                }
                var requestDtos = requests.Select(r => new RequestDto
{
                    Id = r.Id,
                    DonorId = r.DonorId,
                    ServiceId = r.ServiceId,
                    Priority = r.Priority.Value,
                    Status = r.Status.Value,
                    BloodType = r.BloodType.Value,
                    BloodBagType = r.BloodBagType.Value,
                    RequestDate = r.RequestDate,
                    DueDate = r.DueDate,
                    RequiredQty = r.RequiredQty,
                    AquiredQty = r.AquiredQty,
                    MoreDetails = r.MoreDetails
    // Add other properties as needed
                }).ToList();
                return (requestDtos,total, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error fetching blood requests");
                return (null,null, ex);
            }
        }
    }
}