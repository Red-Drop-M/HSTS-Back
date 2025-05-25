// filepath: /home/hiki-zrx/Desktop/HSTS-Back/src/Application/Features/BloodTransferCenterManagement/Commands/CreateBloodTransferCenterCommand.cs
using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodTransferCenterManagement.Commands
{
    public class CreateBloodTransferCenterCommand : IRequest<(BloodTransferCenterDTO? center, BaseException? err)>
    {
        public string Name { get; }
        public string Address { get; }
        public string Email { get; }
        public string PhoneNumber { get; }
        public Guid WilayaId { get; }
        public bool IsPrimary { get; }

        public CreateBloodTransferCenterCommand(
            string name,
            string address,
            string email,
            string phoneNumber,
            Guid wilayaId,
            bool isPrimary = false)
        {
            Name = name;
            Address = address;
            Email = email;
            PhoneNumber = phoneNumber;
            WilayaId = wilayaId;
            IsPrimary = isPrimary;
        }
    }
}