using Domain.Repositories;
using MediatR;
using Application.Common.Models;
namespace Application.Features.ServiceManagement.Handler
{
    public class DeleteServiceHandler : IRequestHandler<DeleteServiceCommand, Result<bool>>
    {
        private readonly IServiceRepository _repository;

        public DeleteServiceHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _repository.GetByIdAsync(request.Id);
            if (service == null) return Result.Failure<bool>("Service not found");

            await _repository.DeleteAsync(request.Id);
            return Result.Success(true);
        }
    }
}
