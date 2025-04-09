using MediatR;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Features.BloodBagManagement.Queries
{
    public class GetBloodBagByIdQuery : IRequest<Result<BloodBag?>>
    {
        public Guid Id { get; set; }
    }
} 