using MediatR;
using Application.Common.Models;

namespace Application.Features.DonorManagement.Commands
{
    public class CreateDonorCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; }
        public string BloodType { get; set; }
        // Add other properties as needed
    }
} 