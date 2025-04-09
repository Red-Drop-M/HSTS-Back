using MediatR;
using Application.Common.Models;

namespace Application.Features.BloodBagManagement.Commands
{
    public class UpdateBloodBagCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string BloodType { get; set; }
        public int Quantity { get; set; }
        // Add other properties as needed
    }
} 