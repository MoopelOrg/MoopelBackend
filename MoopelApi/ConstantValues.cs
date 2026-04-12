using System.Data;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Configuration;
using System.Text.RegularExpressions;

namespace MoopelApi;

public class ConstantValues
{
    public const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
    public const StringComparison InvariantIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
    public const StringComparison CurrentIgnoreCase = StringComparison.CurrentCultureIgnoreCase;

    // Issues here: This does not create the database.
    public const string DB_DOCKER_RUN =
        "docker run --name MoopelDB -e ACCEPT_EULA=Y -e SA_PASSWORD=YourStrong!Passw0rd -e MSSQL_PID=Express -p 1433:1433 -v sql_data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest";
    public const string DB_DOCKER_CONNECTION = "Server=localhost,1433;Database=MoopelDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

    public static class Database
    {

        #region Groups
        public static readonly DatabaseField GroupIdField =
        new() { Name = "GroupId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField GroupNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };

        public static readonly DatabaseField GroupDescriptionField =
            new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 300 };
        public static readonly DatabaseField GroupCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable GroupsTable = new()
        {
            Name = "Groups",
            Alias = "group",
            Fields =
            [
                GroupIdField,
            GroupNameField,
            GroupDescriptionField,
            GroupCreatedAtUtcField,
        ]
        };
        #endregion

        #region Users
        public static readonly DatabaseField UserIdField =
            new() { Name = "UserId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField UserNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };
        public static readonly DatabaseField UserEmailField =
            new() { Name = "Email", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 200, DefaultValue = "NULL" };
        public static readonly DatabaseField UserPasswordHashField =
            new() { Name = "PasswordHash", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 3000 };
        public static readonly DatabaseField UserRoleField =
            new() { Name = "Role", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 50, DefaultValue = "'User'" };
        public static readonly DatabaseField UserCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };
        public static readonly DatabaseField UserLastLoginUtc =
            new() { Name = "LastLoginUtc", Type = SqlDbType.DateTime2, Nullable = true};
        public static readonly DatabaseField UserDeactivatedField =
            new() { Name = "Deactivated", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseTable UsersTable = new()
        {
            Name = "Users",
            Alias = "user",
            Fields =
            [
                UserIdField,
                UserNameField,
                UserPasswordHashField,
                UserRoleField,
                UserEmailField,
                UserCreatedAtUtcField,
                UserLastLoginUtc,
                UserDeactivatedField,
            ]
        };
        #endregion

        #region GroupUsers
        public static readonly DatabaseField GroupUsersGroupIdField =
            new() { Name = GroupIdField.Name, Type = GroupIdField.Type, Nullable = false, Reference = GroupIdField };
        public static readonly DatabaseField GroupUsersUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField GroupUsersCanReadWorkItemsField =
            new() { Name = "CanReadWorkItems", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };
        public static readonly DatabaseField GroupUsersCanWriteWorkItemsField =
            new() { Name = "CanWriteWorkItems", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseTable GroupUsersTable = new()
        {
            Name = "GroupUsers",
            Alias = "groupUser",
            Fields =
            [
                GroupUsersGroupIdField,
            GroupUsersUserIdField,
            GroupUsersCanReadWorkItemsField,
            GroupUsersCanWriteWorkItemsField
            ]
        };
        #endregion

        #region UserSettings
        public static readonly DatabaseField UserSettingsIdField =
            new() { Name = "SettingId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField UserSettingsUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

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

        public static readonly DatabaseField ThemeUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField ThemeNameField =
            new() { Name = "ThemeName", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField ThemeSettingsField =
            new() { Name = "ThemeSettings", Type = SqlDbType.Json, Nullable = false, DefaultValue = "'{}'"};

        public static readonly DatabaseField ThemeDescriptionField = 
            new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 300 };

        public static readonly DatabaseField ThemeIsDarkField =
            new() { Name = "IsDark", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseField ThemeCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable ThemesTable = new()
        {
            Name = "Themes",
            Alias = "theme",
            Fields =
            [
                ThemeIdField,
                ThemeUserIdField,
                ThemeNameField,
                ThemeSettingsField,
                ThemeDescriptionField,
                ThemeIsDarkField,
                ThemeCreatedAtUtcField
            ]
        };
        #endregion

        #region Logins
        public static readonly DatabaseField LoginIdField =
            new() { Name = "LoginId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField LoginUserNameField =
            new() { Name = "Username", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = UserNameField.FieldSize };

        public static readonly DatabaseField LoginAttemptTimeField =
            new() { Name = "AttemptTime", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseField LoginStatusField =
            new() { Name = "Status", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 50 };

        public static readonly DatabaseTable LoginsTable = new()
        {
            Name = "Logins",
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

        #region Sessions
        public static readonly DatabaseField SessionIdField =
            new() { Name = "SessionId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField SessionUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };
        public static readonly DatabaseField SessionLoginIdField =
            new() { Name = LoginIdField.Name, Type = LoginIdField.Type, Nullable = false, Reference = LoginIdField };
        public static readonly DatabaseField SessionTokenField =
            new() { Name = "HashedToken", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 3000 };
        public static readonly DatabaseField SessionIpIssuedToField =
            new() { Name = "IpIssuedTo", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 45 };
        public static readonly DatabaseField SessionIssuedAtUtcField =
            new() { Name = "IssuedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };
        public static readonly DatabaseField SessionExpiresAtUtcField =
            new() { Name = "ExpiresAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "(DATEADD(MINUTE, 120, SYSUTCDATETIME()))" };
        public static readonly DatabaseField SessionRevokedField =
            new() { Name = "Revoked", Type = SqlDbType.Bit, Nullable = false };

        public static readonly DatabaseTable SessionsTable = new()
        {
            Name = "Sessions",
            Alias = "session",
            Fields =
            [
                SessionIdField,
            SessionUserIdField,
            SessionLoginIdField,
            SessionTokenField,
            SessionIpIssuedToField,
            SessionIssuedAtUtcField,
            SessionExpiresAtUtcField,
            SessionRevokedField,
        ]
        };
        #endregion

        #region WorkItems
        public static readonly DatabaseField WorkItemIdField =
        new() { Name = "ItemId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField WorkItemUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField WorkItemNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 20 };

        public static readonly DatabaseField WorkItemDescriptionField =
            new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 300 };

        public static readonly DatabaseField WorkItemPointsField =
            new() { Name = "Points", Type = SqlDbType.Decimal, Nullable = false, FieldSize = 5, Precision = 2 };

        public static readonly DatabaseField WorkItemTypeField =
            new() { Name = "Type", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 20 };

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
            new() { Name = "ParentId", Type = WorkItemIdField.Type, Nullable = false, Reference = WorkItemIdField };

        public static readonly DatabaseField WorkItemChildIdField =
            new() { Name = "ChildId", Type = WorkItemIdField.Type, Nullable = false, Reference = WorkItemIdField };

        public static readonly DatabaseTable WorkItemParentsTable = new()
        {
            Name = "WorkItemParents",
            Alias = "workItemParent",
            Fields =
            [
                WorkItemParentIdField,
            WorkItemChildIdField,
        ]
        };
        #endregion

        #region Tags 
        public static readonly DatabaseField TagIdField =
            new() { Name = "TagId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField TagGroupIdField =
            new() { Name = GroupIdField.Name, Type = GroupIdField.Type, Nullable = false };
        public static readonly DatabaseField TagNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };
        public static readonly DatabaseField TagColorField =
            new() { Name = "Color", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 7, DefaultValue = "'#000000'" };

        public static readonly DatabaseField TagForNotesField = 
            new() { Name = "ForNotes", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };
        public static readonly DatabaseField TagForWorkItemsField =
            new() { Name = "ForWorkItems", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };
        public static readonly DatabaseField TagForTransactionsField =
            new() { Name = "ForTransactions", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseField TagCreatedAtUtc =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable TagsTable = new()
        {
            Name = "Tags",
            Alias = "tag",
            Fields =
            [
                TagIdField,
                TagGroupIdField,
                TagNameField,
                TagColorField,
                TagForNotesField,
                TagForWorkItemsField,
                TagForTransactionsField,
                TagCreatedAtUtc,
            ]
        };
        #endregion

        #region WorkItemTagAssignments
        public static readonly DatabaseField WorkItemTagItemIdField =
            new() { Name = WorkItemIdField.Name, Type = WorkItemIdField.Type, Nullable = false, Reference = WorkItemIdField };
        public static readonly DatabaseField WorkItemTagIdField =
            new() { Name = TagIdField.Name, Type = TagIdField.Type, Nullable = false, Reference = TagIdField };

        public static readonly DatabaseTable WorkItemTagAssignmentsTable = new()
        {
            Name = "WorkItemTagAssignments",
            Alias = "workItemTagAssignment",
            Fields =
            [
                WorkItemTagItemIdField,
                WorkItemTagIdField,
        ]
        };
        #endregion

        #region LogEvents
        public static readonly DatabaseField LogEventIdField =
        new() { Name = "LogId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField LogEventMessageField =
            new() { Name = "Message", Type = SqlDbType.NVarChar, Nullable = true };

        public static readonly DatabaseField LogEventMessageTemplateField =
            new() { Name = "MessageTemplate", Type = SqlDbType.NVarChar, Nullable = true };

        public static readonly DatabaseField LogEventLevelField =
            new() { Name = "Level", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 16 };

        public static readonly DatabaseField LogEventCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime, Nullable = true };

        public static readonly DatabaseField LogEventExceptionField =
            new() { Name = "Exception", Type = SqlDbType.NVarChar, Nullable = true };

        public static readonly DatabaseField LogEventPropertiesField =
            new() { Name = "Properties", Type = SqlDbType.NVarChar, Nullable = true };

        public static readonly DatabaseTable LogEventsTable = new()
        {
            Name = "LogEvents",
            Alias = "logEvent",
            Fields =
            [
                LogEventIdField,
            LogEventMessageField,
            LogEventMessageTemplateField,
            LogEventLevelField,
            LogEventCreatedAtUtcField,
            LogEventExceptionField,
            LogEventPropertiesField
            ]
        };
        #endregion

        public static readonly List<DatabaseTable> DatabaseTables =
        [
            GroupsTable,
        UsersTable,
        GroupUsersTable,
        UserSettingsTable,
        ThemesTable,
        LoginsTable,
        SessionsTable,
        WorkItemsTable,
        WorkItemParentsTable,
        TagsTable,
        WorkItemTagAssignmentsTable,
        LogEventsTable,
    ];
    }

    public static class ValidationRegex
    {
        public const string PasswordValidationPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$";

        public const string PasswordValidationExplanation =
            "Password must be at least 8 characters long and " +
            "include at least one uppercase letter, one lowercase letter, one number, and one special character.";

        public static readonly Regex PasswordRegex = new(PasswordValidationPattern, RegexOptions.Compiled);

        public const string UsernameValidationPattern =
            @"^(?!_)[\p{L}\p{N}_]{3,50}(?<!_)$";

        public const string UsernameValidationExplanation =
            "Username must be between 3 and 50 characters long, " +
            "contain only letters, numbers, and underscores " +
            "and cannot start or end with an underscore.";

        public static readonly Regex UsernameRegex = new(UsernameValidationPattern, RegexOptions.Compiled);
    }
}
