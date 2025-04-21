using MediatR;
using Application.Features.DonorManagement.Commands;
using Domain.Repositories;
using Application.DTOs;
using Shared.Exceptions;


namespace Application.Features.DonorManagement.Handlers
{
    public class DeleteDonorHandler : IRequestHandler<DeleteDonorCommand, (DonorDTO? donor, BaseException? err)>
    {
        private readonly IDonorRepository _donorRepository;

        public DeleteDonorHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<(DonorDTO? donor, BaseException? err)> Handle(DeleteDonorCommand Donor, CancellationToken cancellationToken)
        {
            try
            {
                var donorEntity = await _donorRepository.GetByIdAsync(Donor.Id);
                if (donorEntity == null)
                {
                    return (null, new NotFoundException($"Donor {Donor.Id} not found", "delete donor"));
                }

                await _donorRepository.DeleteAsync(donorEntity.Id);

                var donorDto = new DonorDTO
                {
                    Id = donorEntity.Id,
                    Name = donorEntity.Name,
                    Email = donorEntity.Email,
                    LastDonationDate = donorEntity.LastDonationDate,
                    BloodType = donorEntity.BloodType,
                    Address = donorEntity.Address,
                    NIN = donorEntity.NIN,
                    PhoneNumber = donorEntity.PhoneNumber,
                    DateOfBirth = donorEntity.DateOfBirth
                };

                return (donorDto, null);
            }
            catch (BaseException ex)
            {
                return (null, ex);
            }
            catch 
            {
                return (null, new InternalServerException("Failed to delete donor", "delete donor"));
            }
        }
    }
} 
