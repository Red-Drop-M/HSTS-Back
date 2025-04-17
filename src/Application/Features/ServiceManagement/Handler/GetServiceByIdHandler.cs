using Domain.Repositories;
using MediatR;
using Domain.Entities;
using Application.Common.Models;
using Application.Features.ServiceManagement.Queries;


namespace Application.Features.ServiceManagement.Handler
{
    public class GetServiceByIdHandler : IRequestHandler<GetServiceByIdQuery, Result<Service?>>
    {
        private readonly IServiceRepository _repository;

        public GetServiceByIdHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Service?>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
        {
            var service = await _repository.GetByIdAsync(request.Id);
            return Result<Service?>.Success(service);
        }
    }
}
