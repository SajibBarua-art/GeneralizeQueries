using GeneralizeQueries.Core.Interfaces.RoleAuthorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Platform.Infrastructure.Common.Security;
// <-- Add this using statement

// <-- Add this for LINQ methods like string.Join

namespace GeneralizeQueries.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RoleAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // It's not possible to inject via constructor in attributes, so we resolve services from the context.
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<RoleAuthorizationAttribute>>();

        var roleAuthService = context.HttpContext.RequestServices
            .GetRequiredService<IRoleAuthorizationService>();

        var userContextProvider = context.HttpContext.RequestServices
            .GetRequiredService<IUserContextProvider>();

        var userContext = userContextProvider.GetUserContext();

        // Use a placeholder for the user's identity (e.g., UserId or Name)
        var userIdForLogging = userContext.UserId;
        var userRolesForLogging = userContext.Roles != null ? string.Join(", ", userContext.Roles) : "No Roles";

        logger.LogInformation("Performing role authorization check for user '{UserId}' with roles: [{UserRoles}]",
            userIdForLogging, userRolesForLogging);

        if (!roleAuthService.IsAuthorized(userContext.Roles ?? Array.Empty<string>()))
        {
            var requiredRoles = roleAuthService.GetAllowedRoles();
            var requiredRolesForLogging = requiredRoles != null ? string.Join(", ", requiredRoles) : "None Specified";

            // Log a warning for failed authorization attempts. This is a significant security event.
            logger.LogWarning(
                "Authorization FAILED for user '{UserId}'. Access denied. User Roles: [{UserRoles}], Required Roles: [{RequiredRoles}]",
                userIdForLogging,
                userRolesForLogging,
                requiredRolesForLogging);

            context.Result = new ObjectResult(new
            {
                message = "You do not have permission to access this resource.", requiredRoles
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return; // Short-circuit the pipeline
        }

        // If the code reaches here, authorization was successful.
        logger.LogInformation("Authorization SUCCEEDED for user '{UserId}'.", userIdForLogging);
    }
}