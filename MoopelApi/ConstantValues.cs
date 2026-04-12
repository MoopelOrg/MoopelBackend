using System.Data;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Configuration;
using System.Text.RegularExpressions;
using MoopelApi.Models.Enums;

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

        #region Settings
        public static readonly DatabaseField SettingsIdField =
            new() { Name = "SettingId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseTable UserSettingsTable = new()
        {
            Name = "UserSettings",
            Alias = "setting",
            Fields =
            [
                SettingsIdField,
            ]
        };
        #endregion

        #region Themes
        public static readonly DatabaseField ThemeIdField =
            new() { Name = "ThemeId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField ThemeCreatorUserIdField =
            new() { Name = "Creator" + UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField ThemeNameField =
            new() { Name = "ThemeName", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField ThemeDataField =
            new() { Name = "ThemeData", Type = SqlDbType.Json, Nullable = false, DefaultValue = "'{}'" };

        public static readonly DatabaseField ThemeDescriptionField =
            new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 300, DefaultValue = "''" };

        public static readonly DatabaseField ThemeIsDarkField =
            new() { Name = "IsDark", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseField ThemeCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseField ThemeDeletedField =
            new() { Name = "Deleted", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseTable ThemesTable = new()
        {
            Name = "Themes",
            Alias = "theme",
            Fields =
            [
                ThemeIdField,
                ThemeCreatorUserIdField,
                ThemeNameField,
                ThemeDataField,
                ThemeDescriptionField,
                ThemeIsDarkField,
                ThemeCreatedAtUtcField,
                ThemeDeletedField
            ]
        };
        #endregion

        #region Users
        public static readonly DatabaseField UserIdField =
            new() { Name = "UserId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField UserNameField =
            new() { Name = "Username", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };
        public static readonly DatabaseField UserEmailField =
            new() { Name = "Email", Type = SqlDbType.NVarChar, Nullable = true, FieldSize = 200, DefaultValue = "NULL" };
        public static readonly DatabaseField UserPasswordHashField =
            new() { Name = "PasswordHash", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 3000 };
        public static readonly DatabaseField UserRoleField =
            new() { Name = "Role", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 50, DefaultValue = $"'{UserRole.Standard}'" };
        public static readonly DatabaseField UserCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };
        public static readonly DatabaseField UserLastLoginUtc =
            new() { Name = "LastLoginUtc", Type = SqlDbType.DateTime2, Nullable = true };
        public static readonly DatabaseField UserDeactivatedField =
            new() { Name = "Deactivated", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

        public static readonly DatabaseField UserThemeIdField =
            new() { Name = ThemeIdField.Name, Type = ThemeIdField.Type, Nullable = true, Reference = ThemeIdField };

        public static readonly DatabaseField UserSettingsIdField =
            new() { Name = SettingsIdField.Name, Type = SettingsIdField.Type, Nullable = true, Reference = SettingsIdField };

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
                UserThemeIdField,
                UserSettingsIdField,
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

        #region Logins
        public static readonly DatabaseField LoginIdField =
            new() { Name = "LoginId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField LoginUserNameField =
            new() { Name = "Username", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = UserNameField.FieldSize };

        public static readonly DatabaseField LoginAttemptTimeUtcField =
            new() { Name = "AttemptTimeUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

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
                LoginAttemptTimeUtcField,
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
            new() { Name = "Revoked", Type = SqlDbType.Bit, Nullable = false, DefaultValue = "0" };

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

        public static readonly DatabaseField WorkItemGroupIdField =
            new() { Name = GroupIdField.Name, Type = GroupIdField.Type, Nullable = false, Reference = GroupIdField };

        public static readonly DatabaseField WorkItemNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 20 };

        public static readonly DatabaseField WorkItemDescriptionField =
            new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 300 };

        public static readonly DatabaseField WorkItemPointsField =
            new() { Name = "Points", Type = SqlDbType.Decimal, Nullable = false, FieldSize = 5, Precision = 2, DefaultValue = "0.0" };

        public static readonly DatabaseField WorkItemCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseField WorkItemTypeField =
            new() { Name = "Type", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 20 };

        public static readonly DatabaseTable WorkItemsTable = new()
        {
            Name = "WorkItems",
            Alias = "workItem",
            Fields =
            [
                WorkItemIdField,
                WorkItemGroupIdField,
                WorkItemNameField,
                WorkItemDescriptionField,
                WorkItemPointsField,
                WorkItemTypeField,
                WorkItemCreatedAtUtcField,
            ]
        };
        #endregion

        #region WorkItemParents
        public static readonly DatabaseField WorkItemParentIdField =
            new() { Name = "ParentId", Type = WorkItemIdField.Type, Nullable = false, Reference = WorkItemIdField };

        public static readonly DatabaseField WorkItemChildIdField =
            new() { Name = "ChildId", Type = WorkItemIdField.Type, Nullable = false, Reference = WorkItemIdField };

        public static readonly DatabaseField WorkItemLinkGroupIdField =
            new() { Name = GroupIdField.Name, Type = GroupIdField.Type, Nullable = false, Reference = GroupIdField };

        public static readonly DatabaseTable WorkItemParentsTable = new()
        {
            Name = "WorkItemLinks",
            Alias = "workItemLink",
            Fields =
            [
                WorkItemParentIdField,
                WorkItemChildIdField,
                WorkItemLinkGroupIdField
            ]
        };
        #endregion

        #region UserTags
        public static readonly DatabaseField UserTagIdField =
            new() { Name = "UserTagId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField UserTagUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField UserTagNameField =
            new() { Name = "TagName", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };

        public static readonly DatabaseField UserTagColorField =
            new() { Name = "TagColor", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 7, DefaultValue = "'#000000'" };

        public static readonly DatabaseTable UserTagsTable = new()
        {
            Name = "UserTags",
            Alias = "userTag",
            Fields =
            [
                UserTagIdField,
                UserTagUserIdField,
                UserTagNameField,
                UserTagColorField,
            ]
        };
        #endregion

        #region GroupTags
        public static readonly DatabaseField GroupTagIdField =
            new() { Name = "GroupTagId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField GroupTagGroupIdField =
            new() { Name = GroupIdField.Name, Type = GroupIdField.Type, Nullable = false, Reference = GroupIdField };

        public static readonly DatabaseField GroupTagNameField =
            new() { Name = "TagName", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };

        public static readonly DatabaseField GroupTagColorField =
            new() { Name = "TagColor", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 7, DefaultValue = "'#000000'" };

        public static readonly DatabaseTable GroupTagsTable = new()
        {
            Name = "GroupTags",
            Alias = "groupTag",
            Fields =
            [
                GroupTagIdField,
                GroupTagGroupIdField,
                GroupTagNameField,
                GroupTagColorField,
            ]
        };
        #endregion

        #region WorkItemTagAssignments
        public static readonly DatabaseField WorkItemTagItemIdField =
            new() { Name = WorkItemIdField.Name, Type = WorkItemIdField.Type, Nullable = false, Reference = WorkItemIdField };
        public static readonly DatabaseField WorkItemTagIdField =
            new() { Name = GroupTagIdField.Name, Type = GroupTagIdField.Type, Nullable = false, Reference = GroupTagIdField };

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

        #region Notes
        public static readonly DatabaseField NoteIdField =
            new() { Name = "NoteId", Type = SqlDbType.Int, Nullable = false };
        public static readonly DatabaseField NoteUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField NoteTypeField =
            new() { Name = "Type", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 20 };

        public static readonly DatabaseField NoteTitleField = 
            new() { Name = "Title", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 50 };
        public static readonly DatabaseField NoteContentField =
            new() { Name = "Content", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 500 };

        public static readonly DatabaseField NoteCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable NotesTable = new()
        {
            Name = "Notes",
            Alias = "note",
            Fields =
            [
                NoteIdField,
                NoteUserIdField,
                NoteTypeField,
                NoteTitleField,
                NoteContentField,
                NoteCreatedAtUtcField
            ]
        };
        #endregion

        #region Storages
        public static readonly DatabaseField StorageIdField =
            new() { Name = "StorageId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField StorageUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField StorageNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField StorageCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable StoragesTable = new()
        {
            Name = "Storages",
            Alias = "storage",
            Fields =
            [
                StorageIdField,
                StorageUserIdField,
                StorageNameField,
                StorageCreatedAtUtcField
            ]
        };
        #endregion

        #region StorageItems
        public static readonly DatabaseField StorageItemIdField =
            new() { Name = "StorageItemId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField StorageItemStorageIdField =
            new() { Name = StorageIdField.Name, Type = StorageIdField.Type, Nullable = false, Reference = StorageIdField };

        public static readonly DatabaseField StorageItemNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField StorageItemCountField =
            new() { Name = "Count", Type = SqlDbType.Int, Nullable = false, DefaultValue = "1" };

        public static readonly DatabaseField StorageItemCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable StorageItemsTable = new()
        {
            Name = "StorageItems",
            Alias = "storageItem",
            Fields =
            [
                StorageItemIdField,
                StorageItemStorageIdField,
                StorageItemNameField,
                StorageItemCountField,
                StorageItemCreatedAtUtcField
            ]
        };
        #endregion

        #region Fridges
        public static readonly DatabaseField FridgeIdField =
            new() { Name = "FridgeId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField FridgeUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField FridgeNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField FridgeCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable FridgesTable = new()
        {
            Name = "Fridges",
            Alias = "fridge",
            Fields =
            [
                FridgeIdField,
                FridgeUserIdField,
                FridgeNameField,
                FridgeCreatedAtUtcField
            ]
        };
        #endregion

        #region FridgeItems
        public static readonly DatabaseField FridgeItemIdField =
            new() { Name = "FridgeItemId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField FridgeItemFridgeIdField =
            new() { Name = FridgeIdField.Name, Type = FridgeIdField.Type, Nullable = false, Reference = FridgeIdField };

        public static readonly DatabaseField FridgeItemNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField FridgeItemCountField =
            new() { Name = "Count", Type = SqlDbType.Int, Nullable = false, DefaultValue = "1" };

        public static readonly DatabaseField FridgeItemExpirationDateField =
            new() { Name = "ExpirationDate", Type = SqlDbType.Date, Nullable = true };

        public static readonly DatabaseField FridgeItemCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable FridgeItemsTable = new()
        {
            Name = "FridgeItems",
            Alias = "fridgeItem",
            Fields =
            [
                FridgeItemIdField,
                FridgeItemFridgeIdField,
                FridgeItemNameField,
                FridgeItemCountField,
                FridgeItemExpirationDateField,
                FridgeItemCreatedAtUtcField
            ]
        };
        #endregion

        #region BankAccounts
        public static readonly DatabaseField BankAccountIdField =
            new() { Name = "AccountId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField BankAccountUserIdField =
            new() { Name = UserIdField.Name, Type = UserIdField.Type, Nullable = false, Reference = UserIdField };

        public static readonly DatabaseField BankAccountNameField =
            new() { Name = "AccountName", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 20 };

        public static readonly DatabaseField BankAccountDescriptionField =
            new() { Name = "Description", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100, DefaultValue = "''" };

        public static readonly DatabaseField BankAccountCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable BankAccountsTable = new()
        {
            Name = "BankAccounts",
            Alias = "bankAccount",
            Fields =
            [
                BankAccountIdField,
                BankAccountUserIdField,
                BankAccountNameField,
                BankAccountDescriptionField,
                BankAccountCreatedAtUtcField
            ]
        };
        #endregion

        #region Transactions
        public static readonly DatabaseField TransactionIdField =
            new() { Name = "TransactionId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField TransactionAccountIdField =
            new() { Name = BankAccountIdField.Name, Type = BankAccountIdField.Type, Nullable = false, Reference = BankAccountIdField };

        public static readonly DatabaseField TransactionTagIdField =
            new() { Name = UserTagIdField.Name, Type = UserTagIdField.Type, Nullable = true, Reference = UserTagIdField };

        public static readonly DatabaseField TransactionAmountField =
            new() { Name = "Amount", Type = SqlDbType.Decimal, Nullable = false };

        public static readonly DatabaseField TransactionCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable TransactionsTable = new()
        {
            Name = "Transactions",
            Alias = "transaction",
            Fields =
            [
                TransactionIdField,
                TransactionAccountIdField,
                TransactionTagIdField,
                TransactionAmountField,
                TransactionCreatedAtUtcField
            ]
        };
        #endregion

        #region TransactionItems
        public static readonly DatabaseField TransactionItemIdField =
            new() { Name = "TransactionItemId", Type = SqlDbType.Int, Nullable = false };

        public static readonly DatabaseField TransactionItemTransactionIdField =
            new() { Name = TransactionIdField.Name, Type = TransactionIdField.Type, Nullable = false, Reference = TransactionIdField };

        public static readonly DatabaseField TransactionItemTagIdField =
            new() { Name = UserTagIdField.Name, Type = UserTagIdField.Type, Nullable = true, Reference = UserTagIdField };

        public static readonly DatabaseField TransactionItemNameField =
            new() { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField TransactionItemAmountField =
            new() { Name = "Amount", Type = SqlDbType.Decimal, Nullable = false };

        public static readonly DatabaseField TransactionItemCreatedAtUtcField =
            new() { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseTable TransactionItemsTable = new()
        {
            Name = "TransactionItems",
            Alias = "transactionItem",
            Fields =
            [
                TransactionItemIdField,
                TransactionItemTransactionIdField,
                TransactionItemTagIdField,
                TransactionItemNameField,
                TransactionItemAmountField
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
            UserTagsTable,
            GroupTagsTable,
            NotesTable,
            WorkItemTagAssignmentsTable,
            StoragesTable,
            StorageItemsTable,
            FridgesTable,
            FridgeItemsTable,
            BankAccountsTable,
            TransactionsTable,
            TransactionItemsTable,
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
