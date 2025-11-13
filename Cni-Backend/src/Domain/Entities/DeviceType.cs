using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Entities;
public class DeviceType : BaseAuditableEntity
{
    public required string Name { get; set; }
    public bool HasGrpcStream { get; set; }
    public bool IsDataCollectedViaOtherDevice { get; set; }
}
