using Backend.Domain.Entities.Topology;

namespace Backend.Domain.Events;

public class TopologyNodeCreatedEvent : BaseEvent
{
    public TopologyNodeCreatedEvent(TopologyNode item)
    {
        Item = item;
    }

    public TopologyNode Item { get; }
}
