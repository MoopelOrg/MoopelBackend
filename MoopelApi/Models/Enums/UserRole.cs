namespace MoopelApi.Models.Enums;

public enum UserRole
{
    Guest,
    Standard,
    Admin
}

public static class Roles
{
    public static readonly string Guest = UserRole.Guest.ToString();
    public static readonly string Standard = UserRole.Standard.ToString();
    public static readonly string Admin = UserRole.Admin.ToString();
}