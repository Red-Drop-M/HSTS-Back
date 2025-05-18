using MediatR;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Domain.Repositories;
using Shared.Exceptions;
using Application.Features.PledgeManagement.Queries;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PledgeManagement.Handlers
{
    public class GetAllPledgesHandler : IRequestHandler<GetAllPledgesQuery, (List<DonorPledgeListDTO>? pledges, int? total, BaseException? err)>
    {
        private readonly IPledgeRepository _pledgeRepository;
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<GetAllPledgesHandler> _logger;

        public GetAllPledgesHandler(
            IPledgeRepository pledgeRepository,
            IDonorRepository donorRepository,
            ILogger<GetAllPledgesHandler> logger)
        {
            _pledgeRepository = pledgeRepository;
            _donorRepository = donorRepository;
            _logger = logger;
        }

        public async Task<(List<DonorPledgeListDTO>? pledges, int? total, BaseException? err)> Handle(GetAllPledgesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var (pledges, total) = await _pledgeRepository.GetAllAsync(
                    query.Page,
                    query.PageSize,
                    query.Status,
                    query.DonorId != null ? Guid.Parse(query.DonorId) : null,
                    query.RequestId != null ? Guid.Parse(query.RequestId) : null,
                    query.BloodType);

                if (pledges == null || !pledges.Any())
                {
                    _logger.LogWarning("No pledges found with the specified criteria");
                    return (null, null, new NotFoundException("No pledges found", "get-all-pledges"));
                }

                var pledgeDtos = new List<DonorPledgeListDTO>();

                foreach (var pledge in pledges)
                {
                    var donor = await _donorRepository.GetByIdAsync(pledge.DonorId);
                    
                    pledgeDtos.Add(new DonorPledgeListDTO
                    {
                        DonorId = pledge.DonorId,
                        DonorName = donor?.Name ?? "Unknown",
                        RequestId = pledge.RequestId,
                        BloodType = pledge.Request?.BloodType?.Value ?? "Unknown",
                        PledgeDate = pledge.PledgeDate,
                        Status = pledge.Status.Value
                    });
                }

                return (pledgeDtos, total, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error fetching pledges");
                return (null, null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching pledges");
                return (null, null, new InternalServerException("Failed to fetch pledges", "get-all-pledges"));
            }
        }
    }
}