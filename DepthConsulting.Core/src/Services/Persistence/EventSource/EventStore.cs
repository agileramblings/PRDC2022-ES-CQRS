using DepthConsulting.Core.DDD;
using DepthConsulting.Core.DDD.Exceptions;
using DepthConsulting.Core.Messaging;
using DepthConsulting.Core.Services.Messaging.EventSource;
using Microsoft.Extensions.Logging;

namespace DepthConsulting.Core.Services.Persistence.EventSource;

public class EventStore<T> : IEventStore<T> where T : AggregateBase
{
    private static readonly object Lock = new();
    private readonly IEventDescriptorStorage<T> _descriptorStorage;
    private readonly ILogger _logger;
    private readonly IEventPublisher _publisher;

    public EventStore(IEventPublisher publisher, IEventDescriptorStorage<T> descriptorStorage,
        ILogger<EventStore<T>> logger)
    {
        _publisher = publisher;
        _descriptorStorage = descriptorStorage;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetAggregateIdsAsync(int page, int count)
    {
        LogAction(nameof(GetAggregateIdsAsync));
        return await _descriptorStorage.GetAggregateIdsAsync(page, count);
    }

    public async Task PutAsync(string aggregateId, AggregateBase aggregateType, IEnumerable<EventBase> events,
        int expectedVersion, bool failOnConcurrency, bool batchSave = false)
    {
        LogAction(nameof(PutAsync));

        //TODO: DW - Remove this lock and push this up to specific eventstore impl?
        lock (Lock)
        {
            var lastVersion = _descriptorStorage.GetMostRecentVersion(aggregateId).GetAwaiter().GetResult();
            if (lastVersion >= 0)
            {
                // check whether latest event version matches current aggregate version
                // otherwise -> throw exception
                if (failOnConcurrency && lastVersion != expectedVersion) throw new ConcurrencyException(aggregateId, expectedVersion, lastVersion);
                expectedVersion = lastVersion;
            }
            else
            {
                expectedVersion = -1;
            }

            var i = expectedVersion;

            if (batchSave)
            {
                var descriptors = new List<EventDescriptor>();
                foreach (var @event in events)
                {
                    i++;
                    @event.Version = i;
                    descriptors.Add(new EventDescriptor(aggregateId, $"{aggregateType}", @event, i, @event.ReceivedOn,
                        @event.MessageId, @event.CorrelationId, @event.CausationId));
                }

                _descriptorStorage.AddDescriptorsAsync(aggregateId, descriptors)
                    .GetAwaiter().GetResult();
            }
            else
            {
                // iterate through current aggregate events increasing version with each processed event
                foreach (var @event in events)
                {
                    i++;
                    @event.Version = i;
                    // push event to the event descriptors list for current aggregate
                    _descriptorStorage.AddDescriptorAsync(aggregateId,
                        new EventDescriptor(aggregateId, $"{aggregateType}", @event, i, @event.ReceivedOn,
                            @event.MessageId, @event.CorrelationId, @event.CausationId)).GetAwaiter().GetResult();
                }
            }
        }

        foreach (var @event in events)
            // publish current event to the bus for further processing by subscribers
            await PublishEvent(@event);
    }

    public async Task RepublishEventsForAggregate(string aggregateId)
    {
        LogAction(nameof(RepublishEventsForAggregate));

        var eventsToRepublish = await GetEventsForAggregateAsync(aggregateId);
        foreach (var @event in eventsToRepublish)
            // publish current event to the bus for further processing by subscribers
            await PublishEvent(@event);
    }

    // collect all processed events for given aggregate and return them as a list
    // used to build up an aggregate from its history (Domain.LoadsFromHistory)
    public async Task<IEnumerable<EventBase>> GetEventsForAggregateAsync(string aggregateId)
    {
        LogAction(nameof(GetEventsForAggregateAsync));

        var eventDescriptors = (await _descriptorStorage.GetEventDescriptorsAsync(aggregateId)).ToList();
        if (!eventDescriptors.Any()) throw new AggregateNotFoundException(aggregateId);

        var events = eventDescriptors
            .Select(desc => desc.EventData);
        return events;
    }

    private async Task PublishEvent(EventBase @event)
    {
        LogAction(nameof(PublishEvent));

        var publishMethods = from m in typeof(IEventPublisher).GetMethods()
            where m.Name == "PublishAsync" &&
                  m.ContainsGenericParameters &&
                  m.IsGenericMethod &&
                  m.IsGenericMethodDefinition
            select m;
        var publishMethod = publishMethods.First();
        var genericMethod = publishMethod.MakeGenericMethod(@event.GetType());
        var task = Task.FromResult(genericMethod.Invoke(_publisher, new object[] { @event }));

        await task.ConfigureAwait(false);
    }

    private void LogAction(string operation)
    {
        _logger.LogDebug("{Component} {Operation} executed", nameof(EventStore<T>), operation);
    }
}