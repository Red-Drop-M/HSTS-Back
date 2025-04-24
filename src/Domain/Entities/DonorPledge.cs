using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Entities 
{
    public class DonorPledge 
    {
        public  Guid DonorId {get;set;}
        public Guid RequestId { get; set; }
        public required PledgeStatus Status { get; set; }
        public DateOnly PledgeDate {get;set;}
        public Donor? Donor { get; set; }
        public Request? Request { get; set; }
    }
}