using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Events;
public class DeviceTypeDeletedEvent : BaseEvent
{
    public DeviceTypeDeletedEvent(DeviceType item)
    {
        Item = item;
    }

    public DeviceType Item { get; }
}
