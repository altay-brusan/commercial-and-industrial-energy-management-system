using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities.Huawei;
using Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Huawei;

public class HuaweiTokenManager
{
    private readonly IHuaweiApiClient _apiClient;
    private readonly HuaweiCredentialEncryptor _encryptor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HuaweiTokenManager> _logger;
    private readonly int _cacheMinutes;

    private static string CacheKey(Guid accountId) => $"huawei-token-{accountId}";

    public HuaweiTokenManager(
        IHuaweiApiClient apiClient,
        HuaweiCredentialEncryptor encryptor,
        IMemoryCache cache,
        IOptions<HuaweiSettings> settings,
        ILogger<HuaweiTokenManager> logger)
    {
        _apiClient = apiClient;
        _encryptor = encryptor;
        _cache = cache;
        _logger = logger;
        _cacheMinutes = settings.Value.TokenCacheMinutes;
    }

    public async Task<string> GetTokenAsync(HuaweiAccount account, CancellationToken ct = default)
    {
        var key = CacheKey(account.Id);

        if (_cache.TryGetValue<string>(key, out var cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken;
        }

        return await RefreshTokenAsync(account, ct);
    }

    public async Task<string> RefreshTokenAsync(HuaweiAccount account, CancellationToken ct = default)
    {
        var systemCode = _encryptor.Decrypt(account.EncryptedSystemCode);
        var token = await _apiClient.LoginAsync(account.BaseUrl, account.UserName, systemCode, ct);

        var key = CacheKey(account.Id);
        _cache.Set(key, token, TimeSpan.FromMinutes(_cacheMinutes));

        _logger.LogDebug("Refreshed Huawei token for account {AccountId}", account.Id);
        return token;
    }

    public void InvalidateToken(Guid accountId)
    {
        _cache.Remove(CacheKey(accountId));
    }
}
