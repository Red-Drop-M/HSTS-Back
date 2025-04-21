using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;
using Domain.Repositories;
using Application.Features.DonorManagement.Queries;



namespace Application.Features.DonorManagement.Handlers
{
    public class GetAllDonorsHandler : IRequestHandler<GetAllDonorsQuery, (List<DonorDTO>? donors, int? total, BaseException? err)>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<GetAllDonorsHandler> _logger;

        public GetAllDonorsHandler(IDonorRepository donorRepository, ILogger<GetAllDonorsHandler> logger)
        {
            _donorRepository = donorRepository;
            _logger = logger;
        }

        public async Task<(List<DonorDTO>? donors, int? total, BaseException? err)> Handle(GetAllDonorsQuery Donor, CancellationToken cancellationToken)
        {
            try
            {
                var filter = new DonorFilter
                {
                    Name = Donor.Name,
                    BloodType = Donor.BloodType,
                    Email = Donor.Email,
                    LastDonationDate = Donor.LastDonationDate,
                    Address = Donor.Address,
                    NIN = Donor.NIN,
                    PhoneNumber = Donor.PhoneNumber,
                    BirthDate = Donor.BirthDate
                };

                var (donors, total) = await _donorRepository.GetAllAsync(Donor.Page, Donor.PageSize, filter);
                if (donors == null || !donors.Any())
                {
                    _logger.LogWarning("No donors found");
                    return (null, null, new NotFoundException("No donors found", "Fetching donors"));
                }

                var donorDtos = donors.Select(d => new DonorDTO
                {
                    Id = d.Id,
                    Name = d.Name,
                    Email = d.Email,
                    LastDonationDate = d.LastDonationDate,
                    BloodType = d.BloodType,
                    Address = d.Address,
                    NIN = d.NIN,
                    PhoneNumber = d.PhoneNumber,
                    DateOfBirth = d.DateOfBirth
                }).ToList();

                return (donorDtos, total, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error fetching donors");
                return (null, null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching donors");
                return (null, null, new InternalServerException("Failed to fetch donors", "fetching donors"));
            }
        }
    }
} 