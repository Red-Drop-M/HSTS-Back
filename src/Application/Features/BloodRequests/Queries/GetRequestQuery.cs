using MediatR;
using Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace Application.Features.BloodRequests.Queries
{
    public record GetRequestQuery(Guid Id) : IRequest<(RequestDto? request,BaseException? err)>;
}
