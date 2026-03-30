using System.Data;
using MoopelApi.Models.Configuration;

namespace MoopelApi;

public class ConstantValues
{
    StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
    StringComparison InvariantIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
    StringComparison CurrentIgnoreCase = StringComparison.CurrentCultureIgnoreCase;

    #region Database
    #region Users
    public static readonly DatabaseField UserIdField =
        new() { Name = "UserId", Type = SqlDbType.Int, Nullable = false };
    public static readonly DatabaseField UserNameField =
        new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };
    public static readonly DatabaseField UserEmailField =
        new() { Name = "Email", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 200 };
    public static readonly DatabaseField UserCreatedAtUtc =
        new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false };
    public static readonly DatabaseField UserDeactivated =
        new() { Name = "Deactivated", Type = SqlDbType.Bit, Nullable = false };

    public static readonly DatabaseTable UsersTable = new() 
    { 
        Name  = "Users",
        Alias = "user",
        Fields =
        [
            UserIdField,
            UserNameField,
            UserEmailField,
            UserCreatedAtUtc,
            UserDeactivated,
        ]
    };
    #endregion

    #region UserSettings
    public static readonly DatabaseField UserSettingsIdField =
        new() { Name = "SettingId", Type = SqlDbType.Int, Nullable = false };
    public static readonly DatabaseField UserSettingsUserIdField =
        new() { Name = UserIdField.Name, Type = SqlDbType.Int, Nullable = false, Reference = UserIdField };

    public static readonly DatabaseTable UserSettingsTable = new()
    {
        Name = "UserSettings",
        Alias = "userSetting",
        Fields =
        [
            UserSettingsIdField,
            UserSettingsUserIdField,
        ]
    };
    #endregion

    #region Themes
    public static readonly DatabaseField ThemeIdField =
        new() { Name = "ThemeId", Type = SqlDbType.Int, Nullable = false };

    public static readonly DatabaseField ThemeNameField =
        new() { Name = "ThemeName", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 100 };

    public static readonly DatabaseField ThemeUserIdField =
        new() { Name = UserIdField.Name, Type = SqlDbType.Int, Nullable = false, Reference = UserIdField };

    public static readonly DatabaseField ThemeCreatedAtUtcField =
        new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false };

    public static readonly DatabaseTable ThemesTable = new()
    {
        Name = "Themes",
        Alias = "theme",
        Fields =
        [
            ThemeIdField,
            ThemeNameField,
            ThemeUserIdField,
            ThemeCreatedAtUtcField
        ]
    };
    #endregion

    #region LoginAttempts
    public static readonly DatabaseField LoginIdField =
        new() { Name = "LoginId", Type = SqlDbType.Int, Nullable = false };

    public static readonly DatabaseField LoginUserNameField =
        new() { Name = "Username", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = UserNameField.FieldSize };

    public static readonly DatabaseField LoginAttemptTimeField =
        new() { Name = "AttemptTime", Type = SqlDbType.DateTime2, Nullable = false };

    public static readonly DatabaseField LoginStatusField =
        new() { Name = "Status", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 50 };

    public static readonly DatabaseTable LoginAttemptsTable = new()
    {
        Name = "LoginAttempts",
        Alias = "login",
        Fields =
        [
            LoginIdField,
            LoginUserNameField,
            LoginAttemptTimeField,
            LoginStatusField
        ]
    };
    #endregion

    #region WorkItems
    public static readonly DatabaseField WorkItemIdField =
    new() { Name = "ItemId", Type = SqlDbType.Int, Nullable = false };

    public static readonly DatabaseField WorkItemUserIdField =
        new() { Name = UserIdField.Name, Type = SqlDbType.Int, Nullable = false, Reference = UserIdField };

    public static readonly DatabaseField WorkItemNameField =
        new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 20 };

    public static readonly DatabaseField WorkItemDescriptionField =
        new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 300 };

    public static readonly DatabaseField WorkItemPointsField =
        new() { Name = "Points", Type = SqlDbType.Decimal, Nullable = false };

    public static readonly DatabaseField WorkItemTypeField =
        new() { Name = "Type", Type = SqlDbType.Int, Nullable = false };

    public static readonly DatabaseTable WorkItemsTable = new()
    {
        Name = "WorkItems",
        Alias = "workItem",
        Fields =
        [
            WorkItemIdField,
            WorkItemUserIdField,
            WorkItemNameField,
            WorkItemDescriptionField,
            WorkItemPointsField,
            WorkItemTypeField
        ]
    };
    #endregion

    #region WorkItemParents
    public static readonly DatabaseField WorkItemParentIdField =
    new() { Name = "ParentId", Type = SqlDbType.Int, Nullable = false, Reference = WorkItemIdField };

    public static readonly DatabaseField WorkItemChildIdField =
        new() { Name = "ChildId", Type = SqlDbType.Int, Nullable = false, Reference = WorkItemIdField };

    public static readonly DatabaseTable WorkItemParentsTable = new()
    {
        Name = "WorkItemParents",
        Alias = "workItem",
        Fields =
        [
            WorkItemParentIdField,
            WorkItemChildIdField,
        ]
    };
    #endregion

    #endregion
}
