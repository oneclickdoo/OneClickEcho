namespace OneClickEcho.Domain.Common.Identity;

public enum UserRole
{
    Administrator = 1,
    ContentManager = 2
}

public class UserRoles
{
    public static string[] GetAllUserRoles()
    {
        return [nameof(UserRole.Administrator), nameof(UserRole.ContentManager)];
    }
}