using MediatR;
using Domain.Repositories;
using Application.Features.GlobalStockManagement.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class DeleteGlobalStockHandler : IRequestHandler<DeleteGlobalStockCommand, BaseException?>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly ILogger<DeleteGlobalStockHandler> _logger;

        public DeleteGlobalStockHandler(
            IGlobalStockRepository globalStockRepository,
            ILogger<DeleteGlobalStockHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
            _logger = logger;
        }

        public async Task<BaseException?> Handle(DeleteGlobalStockCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var globalStock = await _globalStockRepository.GetByKeyAsync(
                    command.BloodType, command.BloodBagType);

                if (globalStock == null)
                {
                    _logger.LogWarning("Global stock not found for blood type {BloodType} and bag type {BagType}", 
                        command.BloodType.Value, command.BloodBagType.Value);
                    return new NotFoundException("Global stock not found", "delete_global_stock");
                }

                await _globalStockRepository.DeleteAsync(command.BloodType, command.BloodBagType);
                _logger.LogInformation("Deleted global stock for blood type {BloodType} and bag type {BagType}", 
                    command.BloodType.Value, command.BloodBagType.Value);

                return null;
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error deleting global stock");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting global stock");
                return new InternalServerException("Failed to delete global stock", "delete_global_stock");
            }
        }
    }
}