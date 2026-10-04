namespace WorkFlow.Api.Services;

// Samlet på ett sted så vi slipper skrivefeil i [Authorize(Roles = "...")]
public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string AdminOrManager = "Administrator,Manager";
    public const string Staff = "Administrator,Manager,Employee";
}