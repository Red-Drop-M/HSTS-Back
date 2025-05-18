using Domain.Events;
namespace Domain.Events
{
        public record PledgeFailedEvent(
        DonorPledgeEvent OriginalEvent,
        string ErrorMessage,
        DateTime FailedAt,
        Guid CorrelationId
    );
}