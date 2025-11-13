using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class  AppGrpcSystemDataDeletedEvent : BaseEvent
{
    public AppGrpcSystemDataDeletedEvent(AppGrpcSystemData item)
    {
        Item = item;
    }

    public AppGrpcSystemData Item { get; }
}
