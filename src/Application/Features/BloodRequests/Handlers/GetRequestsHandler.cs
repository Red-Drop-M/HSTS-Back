using MediatR;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Domain.Repositories;
using Shared.Exceptions;
using Application.Features.BloodRequests.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects;

namespace Application.Features.BloodRequests.Handlers
{
    public class GetRequestsHandler : IRequestHandler<GetRequestsQuery, (List<RequestDto>? requests, int? total, BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<GetRequestsHandler> _logger;

        public GetRequestsHandler(
            IRequestRepository requestRepository,
            IBloodBagRepository bloodBagRepository,
            IServiceRepository serviceRepository,
            ILogger<GetRequestsHandler> logger)
        {
            _requestRepository = requestRepository;
            _bloodBagRepository = bloodBagRepository;
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<(List<RequestDto>? requests, int? total, BaseException? err)> Handle(
            GetRequestsQuery query, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Create an empty filter - this will only filter on !IsDeleted in the repository
                var filter = new RequestFilter();
                
                // Get all non-deleted requests
                var (requests, total) = await _requestRepository.GetAllAsync(query.Page, query.PageSize, filter);
                
                if (requests is null || requests.Count == 0)
                {
                    _logger.LogInformation("No blood requests found");
                    return (new List<RequestDto>(), total, null);
                }

                // Map to DTOs without fetching related entities
                var requestDtos = requests.Select(request => new RequestDto
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
                    DonorId = request.DonorId,
                    Service = request.Service?.Name,
                    BloodBags = new List<BloodBagDTO>() // Empty list, not fetching blood bags
                }).ToList();

                return (requestDtos, total, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving blood requests");
                return (null, null, new InternalServerException("Failed to retrieve blood requests", "get-all-requests"));
            }
        }
    }
}