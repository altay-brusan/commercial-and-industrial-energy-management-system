using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities;

namespace Backend.Domain.Events;

public class DeviceCreatedEvent : BaseEvent
{
    public DeviceCreatedEvent(Device item)
    {
        Item = item;
    }

    public Device Item { get; }
}
