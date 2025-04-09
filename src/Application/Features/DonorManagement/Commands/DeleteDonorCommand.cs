using MediatR;
using Application.Common.Models;

namespace Application.Features.DonorManagement.Commands
{
    public class DeleteDonorCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
} 