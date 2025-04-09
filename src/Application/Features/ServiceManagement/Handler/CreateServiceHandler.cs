using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Features.ServiceManagement.Handler
{
    public class CreateServiceHandler : IRequestHandler<CreateServiceCommand, Result<Guid>>
    {
        private readonly IServiceRepository _repository;

        public CreateServiceHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {   
            var service = new Service(request.Name);
            await _repository.AddAsync(service);
            return Result.Success(service.Id);
        }
    }
}
