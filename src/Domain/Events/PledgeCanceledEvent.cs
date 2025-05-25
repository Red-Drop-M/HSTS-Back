using System;

namespace Domain.Events
{
    public record PledgeCanceledEvent(
        Guid DonorId,
        Guid RequestId
    );
}