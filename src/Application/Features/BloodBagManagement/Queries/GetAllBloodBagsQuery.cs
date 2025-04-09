using MediatR;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Features.BloodBagManagement.Queries
{
    public class GetAllBloodBagsQuery : IRequest<Result<List<BloodBag?>>>
    {
    }
} 