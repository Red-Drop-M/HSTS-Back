using MediatR;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Features.DonorManagement.Queries
{
    public class GetAllDonorsQuery : IRequest<Result<List<Donor?>>>
    {
    }
} 