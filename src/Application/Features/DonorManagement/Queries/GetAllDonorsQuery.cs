using MediatR;
using Domain.Entities;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Application.Features.DonorManagement.Queries
{
    public record GetAllDonorsQuery(
        int Page,
        int PageSize,
        string? Name,
        BloodType? BloodType,
        DateOnly? LastDonationDate,
        string? Address,
        string? NIN,
        string? PhoneNumber,
        DateOnly? BirthDate,
        string? Email) : IRequest<(List<DonorDTO>? donors, int? total, BaseException? err)>;
} 