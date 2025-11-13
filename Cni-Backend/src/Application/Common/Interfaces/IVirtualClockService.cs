using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Common.Interfaces;

public interface IVirtualClockService
{
    DateTime CurrentTime { get; }
    void SetInitialTime(DateTime setTime);
    void Start();
    void Stop();
    event Action TimeAdvanced;
}
