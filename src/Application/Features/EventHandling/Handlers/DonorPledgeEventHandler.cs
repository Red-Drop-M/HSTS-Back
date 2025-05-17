using MediatR;
using System.Text.Json;
using Domain.Events;
using Domain.ValueObjects;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
namespace Application.Features.EventHandling.Handlers
{

    using Application.Interfaces;

    public class DonorPledgeEventHandler
    {   
        private readonly IRequestRepository _requestRepository;
        private readonly IPledgeRepository _PledgeRepository;
        private readonly ITopicDispatcher _topicDispatcher;
        private readonly IEventProducer _eventProducer;
        private readonly ILogger<DonorPledgeEventHandler> _logger;

        public DonorPledgeEventHandler(ITopicDispatcher topicDispatcher, IEventProducer eventProducer,
            IRequestRepository requestRepository, IPledgeRepository pledgeRepository,
            ILogger<DonorPledgeEventHandler> logger)
        {
            _requestRepository = requestRepository;
            _PledgeRepository = pledgeRepository;
            _logger = logger;
            _topicDispatcher = topicDispatcher;
            _eventProducer = eventProducer;
        }
    }
}   