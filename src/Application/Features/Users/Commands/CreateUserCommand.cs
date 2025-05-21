using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;
using System;

namespace Application.Features.Users.Commands
{
    public class CreateUserCommand : IRequest<(UserDTO? user, BaseException? error)>
    {
        public string Name { get; }
        public string Email { get; }
        public string Password { get; }
        public UserRole Role { get; }
        public DateTime DateOfBirth { get; }
        public string PhoneNumber { get; }
        public string Address { get; }

        public CreateUserCommand(
            string name,
            string email,
            string password,
            UserRole role,
            DateTime dateOfBirth,
            string phoneNumber,
            string address)
        {
            Name = name;
            Email = email;
            Password = password;
            Role = role;
            DateOfBirth = dateOfBirth;
            PhoneNumber = phoneNumber;
            Address = address;
        }
    }
}