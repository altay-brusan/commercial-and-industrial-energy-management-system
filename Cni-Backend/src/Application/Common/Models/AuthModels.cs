namespace Backend.Application.Common.Models;

public record LoginRequest(string Email, string Password, bool RememberMe = false);

public record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    LoginUserInfo User);

public record LoginUserInfo(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? AvatarUrl,
    List<string> Permissions);

public record RefreshTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn);

public record LogoutResponse(string Message);

public record AuthErrorResponse(AuthError Error);

public record AuthError(
    string Code,
    string Message,
    List<string>? Details = null,
    int? RetryAfter = null);
