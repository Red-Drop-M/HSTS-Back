using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Application.DTOs;
using Application.Features.BloodRequests.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using MediatR;
namespace Application.Features.BloodRequests.Handlers
{
    public class UpdateRequestHandler : IRequest<(RequestDto? request , BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<UpdateRequestHandler> _logger;
        public UpdateRequestHandler(IRequestRepository requestRepository,ILogger<UpdateRequestHandler>logger)
        {
            _requestRepository = requestRepository;
            _logger = logger;
        }
        public async Task<(RequestDto? request,BaseException? err)> Handle(UpdateRequestCommand command,CancellationToken ct)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(command.Id);
                if(request ==null)
                {
                    _logger.LogError("request not found ");
                    throw new NotFoundException("no request found with the provided id","updating request");
                }
                request.UpdateDetails(command.BloodBagType,command.Priority,command.DueDate,command.MoreDetails,command.RequiredQty);
                await _requestRepository.UpdateAsync(request);
                _logger.LogInformation("request updated successfully");
                var requestDto= new RequestDto
                    {
                        Id = request.Id,
                        Priority = request.Priority,
                        BloodType = request.BloodType,
                        BloodBagType = request.BloodBagType,
                        RequestDate = request.RequestDate,
                        DueDate = request.DueDate,
                        Status = request.Status,
                        MoreDetails = request.MoreDetails,
                        RequiredQty = request.RequiredQty,
                        AquiredQty = request.AquiredQty,
                        ServiceId = request.ServiceId,
                        DonorId = request.DonorId
                    };
                return(requestDto,null);

            }catch(BaseException ex)
            {
                _logger.LogError("error while updating request");
                return (null,ex);
            }
        }
    }
}