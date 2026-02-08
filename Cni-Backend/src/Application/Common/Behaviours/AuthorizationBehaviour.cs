using System.Reflection;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IMultiTenantService _multiTenantService;
    private readonly IPermissionService _permissionService;

    public AuthorizationBehaviour(
        IUser user,
        IIdentityService identityService,
        IMultiTenantService multiTenantService,
        IPermissionService permissionService)
    {
        _user = user;
        _identityService = identityService;
        _multiTenantService = multiTenantService;
        _permissionService = permissionService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        var isInRole = await _identityService.IsInRoleAsync(_user.Id, role.Trim());
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                // Must be a member of at least one role in roles
                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }

            // Policy-based authorization
            var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    var authorized = await _identityService.AuthorizeAsync(_user.Id, policy);

                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // Permission-based authorization
        var permissionAttributes = request.GetType().GetCustomAttributes<RequirePermissionAttribute>();

        if (permissionAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Administrators bypass all permission checks
            var isAdmin = await _identityService.IsInRoleAsync(_user.Id, Roles.Administrator);
            if (!isAdmin)
            {
                var tenantId = _multiTenantService.GetEffectiveTenantId()
                    ?? _multiTenantService.GetAuthenticatedTenantId();

                if (tenantId == null)
                {
                    throw new ForbiddenAccessException();
                }

                foreach (var attr in permissionAttributes)
                {
                    // Resolve feature key
                    var featureKey = attr.Feature
                        ?? await _permissionService.GetFeatureForPermissionAsync(attr.Permission, cancellationToken);

                    if (featureKey == null)
                    {
                        throw new ForbiddenAccessException();
                    }

                    // Check tenant's plan includes the feature
                    var tenantHasFeature = await _permissionService.TenantHasFeatureAsync(tenantId.Value, featureKey, cancellationToken);
                    if (!tenantHasFeature)
                    {
                        throw new ForbiddenAccessException();
                    }

                    // TenantAdmins get all permissions within their plan
                    var isTenantAdmin = await _identityService.IsInRoleAsync(_user.Id, Roles.TenantAdmin);
                    if (!isTenantAdmin)
                    {
                        // Check user's tenant role includes the permission
                        var hasPermission = await _permissionService.UserHasPermissionAsync(
                            _user.Id, tenantId.Value, attr.Permission, cancellationToken);
                        if (!hasPermission)
                        {
                            throw new ForbiddenAccessException();
                        }
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
