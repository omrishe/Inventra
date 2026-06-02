using Inventra.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inventra.API.Authorization;

/// <summary>
/// Attribute that restricts a controller action to users who possess the specified permission.
/// Applied AFTER authentication — always combine with [Authorize].
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = user.Claims
            .Any(c => c.Type == Permissions.ClaimType && c.Value == permission);

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new
            {
                error = "Forbidden",
                message = $"Required permission '{permission}' is not granted to your account."
            })
            { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
