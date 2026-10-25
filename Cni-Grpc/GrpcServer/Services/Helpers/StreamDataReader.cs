using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace GrpcServer.Services.Helpers
{
    public static class StreamDataReader
    {
        private const string JsonPath = "GrpcJsonValues/proto.json";
        private static readonly ConcurrentDictionary<string, object[]> _dataCache = new();
        private static DateTime _dataLoadTime = DateTime.MinValue;
        private static readonly object _loadLock = new();

        public static T GetValue<T>(string streamName)
        {
            // Her gün verileri yeniden yükle
            if (DateTime.Now.Date > _dataLoadTime.Date)
            {
                lock (_loadLock)
                {
                    // Double-check after acquiring lock
                    if (DateTime.Now.Date > _dataLoadTime.Date)
                    {
                        LoadAllData();
                        _dataLoadTime = DateTime.Now; // Update load time after loading
                    }
                }
            }

            var values = _dataCache.GetOrAdd(streamName, _ => Array.Empty<object>());
            var index = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var value= values[index];
            if (typeof(T) == typeof(string))
            {
                // Değeri string'e çevir
                return (T)(object)values[index].ToString();
            }
            else if (typeof(T) == typeof(int))
            {
                int result = int.Parse(values[index].ToString(), CultureInfo.InvariantCulture);
                return (T)(object)result;
            }
            else if (typeof(T) == typeof(double))
            {
                double result = double.Parse(values[index].ToString(), CultureInfo.InvariantCulture);
                return (T)(object)result;
            }
            else if (typeof(T) == typeof(bool))
            {
                // Değeri string olarak al, 0/1 kontrolü yap
                string boolString = values[index].ToString().Trim();
                if (boolString == "1")
                {
                    return (T)(object)true;
                }
                else if (boolString == "0")
                {
                    return (T)(object)false;
                }
                else
                {
                    // Standart bool dönüşümü
                    bool result = bool.Parse(boolString);
                    return (T)(object)result;
                }
            }
            else
            {
                // Diğer tipler için InvariantCulture kullanılarak dönüşüm
                return (T)Convert.ChangeType(values[index], typeof(T), CultureInfo.InvariantCulture);
            }
        }

        private static void LoadAllData()
        {
            var json = File.ReadAllText(JsonPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var streams = JsonSerializer.Deserialize<List<StreamData>>(json, options);

            foreach (var stream in streams)
            {
                _dataCache[stream.Name] = stream.Value.ToArray();
            }
        }

        private class StreamData
        {
            public string Name { get; set; }
            public List<object> Value { get; set; }
        }
    }
}
