using MediatR;
using Shared.Exceptions;
using Application.DTOs;
using Domain.Entities;

namespace Application.Features.DonorManagement.Queries
{
    public record GetDonorByIdQuery(Guid Id) : IRequest<(DonorDTO? donor, BaseException? err)>;
} 