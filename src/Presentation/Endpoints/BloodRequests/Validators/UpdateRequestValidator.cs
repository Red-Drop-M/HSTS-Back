using FastEndpoints;
using FluentValidation;
using Presentation.Endpoints.BloodRequests;
using Application.Features.BloodRequests.Commands;
using Application.DTOs;
using Domain.ValueObjects;
using Shared.Exceptions;
using System;
using Microsoft.Extensions.Logging;
namespace Presentation.Endpoints.BloodRequests.Validators
{
    public class UpdateRequestValidator : Validator<UpdateRequestRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");
            RuleFor(x => x.BloodBagType)
                .Must(x => BloodBagType.Convert(x) != null)
                .WithMessage("Invalid BloodBagType.");

            RuleFor(x => x.Priority)
                .Must(x => Priority.Convert(x)!= null)
                .WithMessage("Invalid Priority.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("DueDate is required.")
                .Must(x => DateOnly.TryParse(x.ToString(), out _))
                .WithMessage("Invalid DueDate.");

            RuleFor(x => x.MoreDetails)
                .MaximumLength(500).WithMessage("MoreDetails cannot exceed 500 characters.");
        }
    }
}