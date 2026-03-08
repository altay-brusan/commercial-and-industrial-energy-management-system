using Backend.Application.Common.Models.Huawei;

namespace Backend.Application.Common.Interfaces;

public interface IHuaweiApiClient
{
    Task<string> LoginAsync(string baseUrl, string userName, string systemCode, CancellationToken ct = default);
    Task<HuaweiStationListResponse> GetStationsAsync(string baseUrl, string token, int pageNo = 1, CancellationToken ct = default);
    Task<List<HuaweiDeviceInfo>> GetDeviceListAsync(string baseUrl, string token, string stationCode, CancellationToken ct = default);
    Task<List<HuaweiDevRealKpiData>> GetDevRealKpiAsync(string baseUrl, string token, IReadOnlyList<long> devIds, int devTypeId, CancellationToken ct = default);
    Task<List<HuaweiStationRealKpiData>> GetStationRealKpiAsync(string baseUrl, string token, IReadOnlyList<string> stationCodes, CancellationToken ct = default);
}
