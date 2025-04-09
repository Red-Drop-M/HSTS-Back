using MediatR;
using Application.Common.Models;

namespace Application.Features.BloodBagManagement.Commands
{
    public class DeleteBloodBagCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
} 