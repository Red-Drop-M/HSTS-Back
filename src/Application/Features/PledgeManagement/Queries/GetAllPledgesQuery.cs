using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.PledgeManagement.Queries
{
    public record GetAllPledgesQuery(
        int Page, 
        int PageSize,
        string? Status = null, 
        string? DonorId = null,
        string? RequestId = null,
        string? BloodType = null
    ) : IRequest<(List<DonorPledgeListDTO>? pledges, int? total, BaseException? err)>;
}