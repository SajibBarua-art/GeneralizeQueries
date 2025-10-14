using GeneralizeQueries.Core.Configuration;
using GeneralizeQueries.Core.Interfaces.RoleAuthorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GeneralizeQueries.Application.Services.RoleAuthorization;

public class RoleAuthorizationService : IRoleAuthorizationService
{
    private readonly ILogger<RoleAuthorizationService> _logger;
    private readonly RoleManagementSettings _roleSettings;

    public RoleAuthorizationService(
        IOptions<RoleManagementSettings> roleSettings,
        ILogger<RoleAuthorizationService> logger)
    {
        _roleSettings = roleSettings.Value;
        _logger = logger;
    }

    public bool IsAuthorized(string[] userRoles)
    {
        if (userRoles == null || userRoles.Length == 0)
        {
            _logger.LogWarning("Authorization check failed: No user roles provided");
            return false;
        }

        // Check if user has any of the globally allowed roles
        var isAuthorized = userRoles.Any(userRole =>
            _roleSettings.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase));

        if (isAuthorized)
            _logger.LogInformation("User authorized with roles: {UserRoles}", string.Join(", ", userRoles));
        else
            _logger.LogWarning("User not authorized. User roles: {UserRoles}, Allowed roles: {AllowedRoles}",
                string.Join(", ", userRoles), string.Join(", ", _roleSettings.AllowedRoles));

        return isAuthorized;
    }

    public List<string> GetAllowedRoles()
    {
        _logger.LogInformation("Retrieving allowed roles");
        return _roleSettings.AllowedRoles;
    }
}