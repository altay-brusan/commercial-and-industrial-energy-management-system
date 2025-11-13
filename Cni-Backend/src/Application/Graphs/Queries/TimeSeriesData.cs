using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Graphs.Queries;

public class TimeSeriesData
{
    public DateTime Timestamp { get; set; }
    public double? Value { get; set; }
}

public class TimeDataPoint
{
    public DateTimeOffset Time { get; set; } // "HH:mm" formatında
    public double? Prediction { get; set; }
    public double? Production { get; set; }
}
