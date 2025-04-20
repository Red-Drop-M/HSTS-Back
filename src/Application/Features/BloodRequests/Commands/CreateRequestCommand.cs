using MediatR;
using Domain.ValueObjects;
using Application.DTOs;

namespace Application.Features.BloodRequests.Commands
{
    public class CreateRequestCommand : IRequest<RequestDto>
    {
        public BloodType BloodGroup { get; }
        public BloodBagType BloodBagType { get; }
        public Priority Priority { get; }
        public DateOnly? DueDate { get; }
        public string? MoreDetails { get; }
        public Guid? ServiceId { get; }
        public int Quantity { get; }
        public Guid? DonorId { get; }
        public RequestStatus RequestStatus { get; }
        public DateOnly RequestDate { get; } // Default to DateTime.Now
        public int AquiredQty { get; }
        public int RequiredQty { get; }

        public CreateRequestCommand(
            BloodType bloodGroup,
            BloodBagType bloodBagType,
            Priority? priority = null, // Default to Standard if not provided
            DateOnly? dueDate = null,
            string? moreDetails = null,
            Guid? serviceId = null,
            int quantity = 0,
            Guid? donorId = null,
            RequestStatus? requestStatus = null, // Default to Pending if not provided
            DateOnly? requestDate = null,
            int aquiredQty = 0, // Default to 0
            int requiredQty = 0)
        {
            BloodGroup = bloodGroup;
            BloodBagType = bloodBagType;
            Priority = priority ?? Priority.Standard(); // Default to Standard
            DueDate = dueDate;
            MoreDetails = moreDetails;
            ServiceId = serviceId;
            Quantity = quantity;
            DonorId = donorId;
            RequestStatus = requestStatus ?? RequestStatus.Pending(); // Default to Pending
            RequestDate = requestDate ?? DateOnly.FromDateTime(DateTime.Now); // Default to current date
            AquiredQty = aquiredQty;
            RequiredQty = requiredQty;
        }
    }
}