using FastEndpoints;
using Domain.Entities;
namespace Domain.Events
{ 
    public sealed record AutoReuqestResolverEvent(
        Request   Request
    ):IEvent;
}