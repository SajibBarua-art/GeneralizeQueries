namespace GeneralizeQueries.Core.Interfaces.RoleAuthorization;

public interface IRoleAuthorizationService
{
    bool IsAuthorized(string[] userRoles);
    List<string> GetAllowedRoles();
}