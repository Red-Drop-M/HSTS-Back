using Domain.ValueObjects;
namespace Domain.Events
{
    public class UpdateRequestEvent
    {
        public UpdateRequestEvent(
            Guid requestId,
            Priority? priority = null,
            int? acquiredQty = null,
            int? requiredQty = null,
            DateOnly? dueDate = null)
        {
            RequestId = requestId;
            AcquiredQty = acquiredQty;
            RequiredQty = requiredQty;
            DueDate = dueDate;
        }

        public Guid RequestId { get; }
        public int? AcquiredQty { get; }
        public int? RequiredQty { get; }
        public DateOnly? DueDate { get; }
    }
}