using MediatR;
using Application.Common.Models;

namespace Application.Features.BloodBagManagement.Commands
{
    public class CreateBloodBagCommand : IRequest<Result<Guid>>
    {
        public string BloodType { get; set; }
        public int Quantity { get; set; }
        // Add other properties as needed
    }
} 