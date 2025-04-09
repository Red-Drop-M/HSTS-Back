using MediatR;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Features.DonorManagement.Queries
{
    public class GetDonorByIdQuery : IRequest<Result<Donor?>>
    {
        public Guid Id { get; set; }
    }
} 