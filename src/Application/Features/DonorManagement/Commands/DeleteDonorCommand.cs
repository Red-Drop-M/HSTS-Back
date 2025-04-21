using MediatR;
using Shared.Exceptions;
using Application.DTOs;

namespace Application.Features.DonorManagement.Commands
{
    public record DeleteDonorCommand(Guid Id) : IRequest<(DonorDTO? donor, BaseException? err)>;
} 