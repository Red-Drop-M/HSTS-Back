using MediatR;
using Application.Common.Models;

namespace Application.Features.DonorManagement.Commands
{
    public class UpdateDonorCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BloodType { get; set; }
        // Add other properties as needed
    }
} 