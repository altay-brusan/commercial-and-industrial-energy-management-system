using Backend.Application.Graphs.Queries;

namespace Backend.Application.Common.Interfaces;

public interface IExternalDataService
{
    Task<List<TimeSeriesData>> GetTimeSeriesDataAsync(string name);
    List<TimeSeriesData> GetTimeSeriesData(string name);
    double? GetValueByDateTime(string name, DateTime dateTime);
}
