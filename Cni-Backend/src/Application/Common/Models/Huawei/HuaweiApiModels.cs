using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Application.Common.Models.Huawei;

public class HuaweiApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("failCode")]
    public int FailCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

public class HuaweiLoginResponse
{
    [JsonPropertyName("csrfToken")]
    public string? CsrfToken { get; set; }
}

public class HuaweiStationInfo
{
    [JsonPropertyName("plantCode")]
    public string PlantCode { get; set; } = string.Empty;

    [JsonPropertyName("plantName")]
    public string PlantName { get; set; } = string.Empty;

    [JsonPropertyName("plantAddress")]
    public string? PlantAddress { get; set; }

    [JsonPropertyName("capacity")]
    public double? Capacity { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}

public class HuaweiStationListResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("pageCount")]
    public int PageCount { get; set; }

    [JsonPropertyName("list")]
    public List<HuaweiStationInfo> List { get; set; } = [];
}

public class HuaweiDeviceInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("devName")]
    public string DevName { get; set; } = string.Empty;

    [JsonPropertyName("devTypeId")]
    public int DevTypeId { get; set; }

    [JsonPropertyName("esnCode")]
    public string? EsnCode { get; set; }

    [JsonPropertyName("invType")]
    public string? InvType { get; set; }

    [JsonPropertyName("softwareVersion")]
    public string? SoftwareVersion { get; set; }

    [JsonPropertyName("stationCode")]
    public string StationCode { get; set; } = string.Empty;
}

public class HuaweiDevRealKpiData
{
    [JsonPropertyName("devId")]
    public string DevId { get; set; } = string.Empty;

    [JsonPropertyName("dataItemMap")]
    public JsonElement DataItemMap { get; set; }
}

public class HuaweiStationRealKpiData
{
    [JsonPropertyName("stationCode")]
    public string StationCode { get; set; } = string.Empty;

    [JsonPropertyName("dataItemMap")]
    public JsonElement DataItemMap { get; set; }
}
