using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;
using Application.DTOs;
namespace Application.Features.BloodRequests.Queries
{
    public record GetRequestsQuery(
        int Page,
        int PageSize,
        string? Priority,
        string? BloodBagType, 
        string? RequestDate ,
        string? DueDate ,
        string? DonorId ,
        string? ServiceId ,
        string? Status ,
        string? BloodGroup)
   : IRequest<(List<RequestDto>? requests,int? total, BaseException? err)>;
}