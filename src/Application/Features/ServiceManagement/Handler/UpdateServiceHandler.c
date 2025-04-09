using Domain.Repositories;
using MediatR;

namespace Application.Features.ServiceManagement.Handler
{
    public class UpdateServiceHandler : IRequestHandler<UpdateServiceCommand, Result<bool>>
    {
        private readonly IServiceRepository _repository;

        public UpdateServiceHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _repository.GetByIdAsync(request.Id);
            if (service == null) return Result.Failure<bool>("Service not found");

            typeof(Domain.Entities.Service)
                .GetProperty("Name")!
                .SetValue(service, request.Name);

            await _repository.UpdateAsync(service);
            return Result.Success(true);
        }
    }
}
