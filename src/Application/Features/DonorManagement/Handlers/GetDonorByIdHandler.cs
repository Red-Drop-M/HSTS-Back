using MediatR;
using Domain.Repositories;
using Application.Features.DonorManagement.Queries;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.DonorManagement.Handlers
{
    public class GetDonorByIdHandler : IRequestHandler<GetDonorByIdQuery, (DonorDTO? donor, BaseException? err)>
    {
        private readonly IDonorRepository _donorRepository;

        public GetDonorByIdHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<(DonorDTO? donor, BaseException? err)> Handle(GetDonorByIdQuery Donor, CancellationToken cancellationToken)
        {
            try 
            {
                var donor = await _donorRepository.GetByIdAsync(Donor.Id);
                if (donor == null)
                {
                    return (null, new NotFoundException("Donor not found", "get donor by id"));
                }

                var donorDto = new DonorDTO
                {
                    Id = donor.Id,
                    Name = donor.Name,
                    Email = donor.Email,
                    LastDonationDate = donor.LastDonationDate,
                    BloodType = donor.BloodType,
                    Address = donor.Address,
                    NIN = donor.NIN,
                    PhoneNumber = donor.PhoneNumber,
                    DateOfBirth = donor.DateOfBirth
                };

                return (donorDto, null);
            }
            catch (BaseException ex)
            {
                return (null, ex);
            }
        }
    }
} 
