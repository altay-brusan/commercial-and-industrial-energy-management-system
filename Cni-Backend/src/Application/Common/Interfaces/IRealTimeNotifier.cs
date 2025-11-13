using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Proto.DTO;

namespace Backend.Application.Common.Interfaces;
public interface IRealTimeNotifier
{
    Task SendFacilityUpdate(int facilityId, FacilityHubDto data);
}
