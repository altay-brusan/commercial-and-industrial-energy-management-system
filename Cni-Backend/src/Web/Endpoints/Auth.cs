using System.Security.Cryptography;
using System.Text;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Domain.Entities;
using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Data;
using Backend.Web.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Web.Endpoints;

public class Auth : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Auth")
            .WithGroupName("Auth")
            .WithTags("Auth");

        group.MapPost("/Login", Login)
            .RequireRateLimiting("auth-login");

        group.MapPost("/RefreshToken", RefreshToken);

        group.MapPost("/Logout", Logout)
            .RequireAuthorization();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IPermissionService permissionService,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> jwtSettings)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "VALIDATION_ERROR",
                "Email and password are required.")),
                statusCode: 400);
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_CREDENTIALS",
                "Email or password is incorrect.")),
                statusCode: 401);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "ACCOUNT_LOCKED",
                "Account is locked due to multiple failed login attempts. Please try again later.",
                RetryAfter: 300)),
                statusCode: 423);
        }

        if (!result.Succeeded)
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_CREDENTIALS",
                "Email or password is incorrect.")),
                statusCode: 401);
        }

        var roles = await userManager.GetRolesAsync(user);
        var tenantIdStr = user.TenantId?.ToString();

        // Get permissions - use empty list if no tenant
        var permissions = user.TenantId.HasValue
            ? await permissionService.GetUserPermissionsAsync(user.Id, user.TenantId.Value)
            : new List<string>();

        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email!, tenantIdStr, roles, permissions);

        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshTokenValue);

        var settings = jwtSettings.Value;
        var refreshTokenEntity = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(settings.RefreshTokenExpirationDays),
            IsRevoked = false,
            CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString()
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        // Set refresh token as httpOnly cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/Auth",
        };

        if (request.RememberMe)
        {
            cookieOptions.MaxAge = TimeSpan.FromDays(settings.RefreshTokenExpirationDays);
        }
        // If not RememberMe, no MaxAge/Expires = session cookie

        httpContext.Response.Cookies.Append("refreshToken", refreshTokenValue, cookieOptions);

        var response = new LoginResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: settings.AccessTokenExpirationMinutes * 60,
            User: new LoginUserInfo(
                Id: user.Id,
                Email: user.Email!,
                FirstName: user.Name ?? "",
                LastName: user.Surname ?? "",
                Role: roles.FirstOrDefault() ?? "User",
                AvatarUrl: null,
                Permissions: permissions));

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshToken(
        HttpContext httpContext,
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> jwtSettings)
    {
        // Read refresh token from cookie
        var refreshTokenValue = httpContext.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_REFRESH_TOKEN",
                "No refresh token provided.")),
                statusCode: 401);
        }

        // Read access token from Authorization header (can be expired)
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_TOKEN",
                "Access token is required in Authorization header.")),
                statusCode: 401);
        }

        var accessToken = authHeader["Bearer ".Length..];
        var principal = jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_TOKEN",
                "Invalid access token.")),
                statusCode: 401);
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_TOKEN",
                "Invalid access token claims.")),
                statusCode: 401);
        }

        // Look up stored refresh token by hash
        var refreshTokenHash = HashToken(refreshTokenValue);
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenHash && rt.UserId == userId);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_REFRESH_TOKEN",
                "Invalid or expired refresh token.")),
                statusCode: 401);
        }

        // Revoke old refresh token
        storedToken.IsRevoked = true;

        // Get fresh user data
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.Json(new AuthErrorResponse(new AuthError(
                "INVALID_TOKEN",
                "User not found.")),
                statusCode: 401);
        }

        var roles = await userManager.GetRolesAsync(user);
        var tenantIdStr = user.TenantId?.ToString();

        var permissions = user.TenantId.HasValue
            ? await permissionService.GetUserPermissionsAsync(user.Id, user.TenantId.Value)
            : new List<string>();

        // Generate new tokens
        var newAccessToken = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email!, tenantIdStr, roles, permissions);

        var newRefreshTokenValue = jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = HashToken(newRefreshTokenValue);

        var settings = jwtSettings.Value;

        // Link old -> new for token rotation chain
        storedToken.ReplacedByToken = newRefreshTokenHash;

        var newRefreshTokenEntity = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(settings.RefreshTokenExpirationDays),
            IsRevoked = false,
            CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString()
        };

        dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await dbContext.SaveChangesAsync();

        // Set new refresh cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/Auth",
            MaxAge = TimeSpan.FromDays(settings.RefreshTokenExpirationDays)
        };

        httpContext.Response.Cookies.Append("refreshToken", newRefreshTokenValue, cookieOptions);

        var response = new RefreshTokenResponse(
            AccessToken: newAccessToken,
            TokenType: "Bearer",
            ExpiresIn: settings.AccessTokenExpirationMinutes * 60);

        return Results.Ok(response);
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        ApplicationDbContext dbContext)
    {
        // Read refresh token from cookie
        var refreshTokenValue = httpContext.Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            var refreshTokenHash = HashToken(refreshTokenValue);
            var storedToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenHash && !rt.IsRevoked);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await dbContext.SaveChangesAsync();
            }
        }

        // Clear the refresh token cookie
        httpContext.Response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/Auth",
            MaxAge = TimeSpan.Zero
        });

        return Results.Ok(new LogoutResponse("Successfully logged out"));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
