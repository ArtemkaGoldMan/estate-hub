using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EstateHub.SharedKernel.API.Authorization.Attributes;

/// <summary>
/// Custom authorization attribute that checks for specific permissions.
/// This follows ASP.NET Core best practices for authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!PermissionChecker.HasPermission(user, _permission))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Custom authorization attribute that checks for multiple permissions (ANY).
/// User must have at least one of the specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireAnyPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public RequireAnyPermissionAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!PermissionChecker.HasAnyPermission(user, _permissions))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Custom authorization attribute that checks for multiple permissions (ALL).
/// User must have all of the specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireAllPermissionsAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public RequireAllPermissionsAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!PermissionChecker.HasAllPermissions(user, _permissions))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
