using Domain.Repositories;
using MediatR;
using Domain.Entities;
using Application.Common.Models;


namespace Application.Features.ServiceManagement.Handler
{
    public class GetAllServicesHandler : IRequestHandler<GetAllServicesQuery, Result<List<Service?>>>
    {
        private readonly IServiceRepository _repository;

        public GetAllServicesHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<Service?>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
        {
            var services = await _repository.GetServicesAsync();
            return Result.Success(services);
        }
    }
}
