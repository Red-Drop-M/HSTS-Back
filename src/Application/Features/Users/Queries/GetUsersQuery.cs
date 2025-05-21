using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using System.Collections.Generic;

namespace Application.Features.Users.Queries
{
    public class GetUsersQuery : IRequest<(List<UserDTO> users, BaseException? error)>
    {
        // No parameters needed for getting all users
    }
}