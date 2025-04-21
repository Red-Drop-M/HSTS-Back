using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;
using Application.DTOs;

namespace Application.Features.BloodBagManagement.Queries
{
    /*public record GetAllBloodBagsQuery(
        int Page,
        int PageSize,
        string? BloodType,
        string? BloodBagType,
        string? ExpirationDate,
        string? DonorId,
        string? RequestId) : IRequest<(List<BloodBagDTO>? BloodBags, int total, BaseException? err)>;
*/
        public class GetAllBloodBagsQuery : IRequest<(List<BloodBagDTO>? BloodBags, int total, BaseException? err)>
    {
        public BloodType? BloodType { get; set; }
        public BloodBagType? BloodBagType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; }
        public BloodBagStatus? Status { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? RequestId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
} 


/*
using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Queries
{
    public class GetAllBloodBagsQuery : IRequest<(List<BloodBagDTO>? BloodBags, int total, BaseException? err)>
    {
        public string? BloodType { get; set; }
        public string? BloodBagType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? RequestId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
*/