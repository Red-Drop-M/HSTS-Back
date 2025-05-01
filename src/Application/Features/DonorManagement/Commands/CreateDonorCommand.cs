using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.DonorManagement.Commands
{
    public class CreateDonorCommand : IRequest<(DonorDTO? donor, Exception? err)>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get;  }
        public string Email { get;  }
        public BloodType BloodType { get;  } 
        public DateOnly? LastDonationDate { get;  } 
        public string Address { get;  }
        public string NIN { get;  } 
        public string PhoneNumber { get;  }
        public DateOnly DateOfBirth { get;  }

        public CreateDonorCommand(
            string name,
            string email,
            DateOnly dateOfBirth,
            BloodType bloodType,
            string address ,
            string nin  , 
            string phoneNumber,
            DateOnly? lastDonationDate = null )
        {
            Name = name;
            Email = email;
            BloodType = bloodType ; // Default to A+ if not provided
            LastDonationDate = lastDonationDate ; // Default to current date
            Address = address;
            NIN = nin ; 
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth ; 
        }
    }

    
}