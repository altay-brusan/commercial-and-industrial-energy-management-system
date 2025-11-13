using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;
public class DeviceDeletedEvent : BaseEvent
{
    public DeviceDeletedEvent(Device item)
    {
        Item = item;
    }

    public Device Item { get; }
}
