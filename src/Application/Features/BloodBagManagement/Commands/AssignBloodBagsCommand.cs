using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;

namespace Application.Features.BloodBagManagement.Commands
{
    public class AssignBloodBagsCommand : IRequest<(int assignedCount, BaseException? err)>
    {
        public Guid RequestId { get; }
        public List<Guid> BloodBagIds { get; }

        public AssignBloodBagsCommand(Guid requestId, List<Guid> bloodBagIds)
        {
            RequestId = requestId;
            BloodBagIds = bloodBagIds;
        }
    }
}