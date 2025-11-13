
using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Graphs.Queries;

namespace Backend.Infrastructure.Services;

public class ExternalDataService : IExternalDataService
{
    private const string JsonFileName = "external.json";
    private readonly Lazy<Dictionary<string, List<TimeSeriesData>>> _cachedData;

    public ExternalDataService()
    {
        _cachedData = new Lazy<Dictionary<string, List<TimeSeriesData>>>(() =>
            LoadData(DateTime.Today).GetAwaiter().GetResult());
    }

    public async Task<List<TimeSeriesData>> GetTimeSeriesDataAsync(string name)
    {
        var data = await LoadData(DateTime.Today);
        return data.GetValueOrDefault(name, new List<TimeSeriesData>());
    }

    public List<TimeSeriesData> GetTimeSeriesData(string name)
    {
        return _cachedData.Value.GetValueOrDefault(name, new List<TimeSeriesData>());
    }

    private static async Task<Dictionary<string, List<TimeSeriesData>>> LoadData(DateTime baseDate)
    {
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExternalValues", JsonFileName);

            await using var fs = File.OpenRead(filePath);
            var jsonData = await JsonSerializer.DeserializeAsync<List<ExternalData>>(fs);

            return ProcessJsonData(jsonData!, baseDate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading external data: {ex.Message}");
            return new Dictionary<string, List<TimeSeriesData>>();
        }
    }

    private static Dictionary<string, List<TimeSeriesData>> ProcessJsonData(List<ExternalData> jsonData, DateTime baseDate)
    {
        var result = new Dictionary<string, List<TimeSeriesData>>();

        if (jsonData == null) return result;

        foreach (var item in jsonData)
        {
            var timeSeries = new List<TimeSeriesData>();
            var currentTime = baseDate.Date; // Günün başlangıcı 00:00

            foreach (var value in item.value!)
            {
                timeSeries.Add(new TimeSeriesData
                {
                    Timestamp = currentTime,
                    Value = value
                });
                currentTime = currentTime.AddMinutes(1);
            }

            result[item.name!] = timeSeries;
        }

        return result;
    }
    public double? GetValueByDateTime(string name, DateTime dateTime)
    {
        var data = GetTimeSeriesData(name);

        if (data.Count == 0)
            return 0;

        TimeSpan timeOfDay = dateTime.TimeOfDay;
        int totalMinutes = (int)timeOfDay.TotalMinutes;

        if (totalMinutes < 0 || totalMinutes >= data.Count)
            return 0;

        return data[totalMinutes].Value;
    }
    private class ExternalData
    {
        public string? name { get; set; }
        public List<double>? value { get; set; }
    }
}

