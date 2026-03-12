using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Application.Common.Models.Alm;

public class AlmSubmitJobRequest
{
    [JsonPropertyName("schema")]
    public JsonElement Schema { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } = string.Empty;
}

public class AlmSubmitJobResponse
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("job_status")]
    public string JobStatus { get; set; } = string.Empty;
}

public class AlmCallbackPayload
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("job_status")]
    public string JobStatus { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public AlmJobResult? Result { get; set; }
}

public class AlmJobResult
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("forecast")]
    public List<AlmForecastItem>? Forecast { get; set; }
}

public class AlmForecastItem
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }

    [JsonPropertyName("step")]
    public int Step { get; set; }
}

public class AlmPcSchema
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = "UTCTime";

    [JsonPropertyName("timestamp_format")]
    public string TimestampFormat { get; set; } = "%Y-%m-%d %H:%M:%S";

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("globalpower")]
    public string GlobalPower { get; set; } = "LoadTotalPowerW";
}

public class AlmPgSchema
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = "UTCTime";

    [JsonPropertyName("timestamp_format")]
    public string TimestampFormat { get; set; } = "%Y-%m-%d %H:%M:%S";

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("pvpower")]
    public List<string> PvPower { get; set; } = new();
}

public class AlmJobStatusResponse
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("job_status")]
    public string JobStatus { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public AlmJobResult? Result { get; set; }
}
