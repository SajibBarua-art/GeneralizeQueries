namespace GeneralizeQueries.Core.Configuration;

public class RoleManagementSettings
{
    public const string SectionName = "RoleManagement";

    public List<string> AllowedRoles { get; set; } = [];
}