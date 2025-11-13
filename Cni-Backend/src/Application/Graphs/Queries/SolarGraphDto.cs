using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Graphs.Queries;

public class SolarGraphDto
{
    public List<TimeDataPoint> DataPoints { get; set; } = new();
}
