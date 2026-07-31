using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using MoopelApi.Models.Configuration;
using MoopelApi.Models.Enums;

namespace MoopelApi;

public class ConstantValues
{
    public const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
    public const StringComparison InvariantIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
    public const StringComparison CurrentIgnoreCase = StringComparison.CurrentCultureIgnoreCase;


    public static void ConfigureJsonSerializerOptions(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.WriteIndented = false;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.Converters.Add(new JsonStringEnumConverter());
        options.ReadCommentHandling = JsonCommentHandling.Skip;
    }

    // Issues here: This does not create the database.
    public const string DB_DOCKER_RUN =
        "docker run --name MoopelDB -e ACCEPT_EULA=Y -e SA_PASSWORD=YourStrong!Passw0rd -e MSSQL_PID=Express -p 1433:1433 -v sql_data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest";
    public const string DB_DOCKER_CONNECTION = "Server=localhost,1433;Database=MoopelDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

    public static class Database
    {
        #region Groups

        public static readonly DatabaseField GroupIdField =
            new()
            {
                Name = "GroupId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField GroupNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50
            };

        public static readonly DatabaseField GroupDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = 300
            };

        public static readonly DatabaseField GroupCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

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
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_Groups_Name",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [GroupNameField]
                }
            ]
        };
        #endregion

        #region Themes
        public static readonly DatabaseField ThemeIdField =
            new()
            {
                Name = "ThemeId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField ThemeCreatorDescriptorField =
            new()
            {
                Name = "CreatorDescriptor",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };

        public static readonly DatabaseField ThemeNameField =
            new()
            {
                Name = "ThemeName",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };

        public static readonly DatabaseField ThemeDataField =
            new()
            {
                Name = "ThemeData",
                Type = SqlDbType.NVarChar, // SQL Server does not support JSON type directly
                Nullable = false,
                FieldSize = null,
                DefaultValue = "'{}'"
            };

        public static readonly DatabaseField ThemeDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField ThemeIsDarkField =
            new()
            {
                Name = "IsDark",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField ThemeCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseField ThemeDeletedField =
            new()
            {
                Name = "Deleted",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseTable ThemesTable = new()
        {
            Name = "Themes",
            Alias = "theme",
            Fields =
            [
                ThemeIdField,
                ThemeCreatorDescriptorField,
                ThemeNameField,
                ThemeDataField,
                ThemeDescriptionField,
                ThemeIsDarkField,
                ThemeCreatedAtUtcField,
                ThemeDeletedField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_Themes_Name_Creator",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [ThemeNameField, ThemeCreatorDescriptorField]
                }
            ]
        };
        #endregion

        #region Users
        public static readonly DatabaseField UserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField UserNameField =
            new()
            {
                Name = "Username",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50
            };

        public static readonly DatabaseField UserPasswordHashField =
            new()
            {
                Name = "PasswordHash",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 3000
            };

        public static readonly DatabaseField UserRoleField =
            new()
            {
                Name = "Role",
                Type = SqlDbType.VarChar,
                Nullable = false,
                FieldSize = 50,
                DefaultValue = $"'{UserRole.Standard}'"
            };

        public static readonly DatabaseField UserEmailField =
            new()
            {
                Name = "Email",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = 200
            };

        public static readonly DatabaseField UserCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseField UserLastLoginUtcField =
            new()
            {
                Name = "LastLoginUtc",
                Type = SqlDbType.DateTime2,
                Nullable = true
            };

        public static readonly DatabaseField UserDeactivatedField =
            new()
            {
                Name = "Deactivated",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField UserThemeIdField =
            new()
            {
                Name = "ThemeId",
                Type = SqlDbType.Int,
                Nullable = true
            };

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
                UserLastLoginUtcField,
                UserDeactivatedField,
                UserThemeIdField,
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_Users_Username",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [UserNameField]
                },
                new DatabaseConstraint
                {
                    Name = "UQ_Users_Email",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [UserEmailField],
                    FilterExpression = $"{UserEmailField.SelectName} IS NOT NULL"
                },
                new DatabaseConstraint
                {
                    Name = "FK_Users_Themes",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [UserThemeIdField],
                    ReferenceTable = ThemesTable,
                    ReferenceFields = [ThemeIdField],
                    OnDelete = "SET NULL"
                },
                new DatabaseConstraint
                {
                    Name = "CHK_Users_Role",
                    Type = DatabaseConstraintType.Check,
                    Fields = [UserRoleField],
                    CheckSql = $"{UserRoleField.SelectName} IN ('Admin','Standard')"
                }
            ]
        };
        #endregion

        #region Settings
        public static readonly DatabaseField SettingsIdField =
            new()
            {
                Name = "SettingId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };
        public static readonly DatabaseField SettingsUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };
        public static readonly DatabaseField SettingTimezoneField =
            new()
            {
                Name = "Timezone",
                Type = SqlDbType.VarChar,
                FieldSize = 30,
                Nullable = true,
            };
        public static readonly DatabaseField SettingDateFormatField =
            new()
            {
                Name = "DateFormat",
                Type = SqlDbType.VarChar,
                FieldSize = 30,
                Nullable = true,
            };
        public static readonly DatabaseField SettingsCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };
        public static readonly DatabaseField SettingsLastUpdatedAtUtcField =
            new()
            {
                Name = "LastUpdatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable SettingsTable = new()
        {
            Name = "UserSettings",
            Alias = "setting",
            Fields =
            [
                SettingsIdField,
                SettingsUserIdField,
                SettingTimezoneField,
                SettingsCreatedAtUtcField,
                SettingsLastUpdatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_UserSettings_UserId",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [SettingsUserIdField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_UserSettings_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [SettingsUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                }
            ]
        };
        #endregion

        #region GroupUsers
        public static readonly DatabaseField GroupUsersGroupIdField =
            new()
            {
                Name = "GroupId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField GroupUsersUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField GroupUsersIsOwnerField =
            new()
            {
                Name = "IsOwner",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersIsAdminField =
            new()
            {
                Name = "IsAdmin",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanEditGroupField =
            new()
            {
                Name = "CanEditGroup",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanInviteUsersField =
            new()
            {
                Name = "CanInviteUsers",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanReadWorkItemsField =
            new()
            {
                Name = "CanReadWorkItems",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "1"
            };

        public static readonly DatabaseField GroupUsersCanCreateWorkItemsField =
            new()
            {
                Name = "CanCreateWorkItems",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanUpdateWorkItemsField =
            new()
            {
                Name = "CanUpdateWorkItems",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanDeleteWorkItemsField =
            new()
            {
                Name = "CanDeleteWorkItems",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanCreateTagsField =
            new()
            {
                Name = "CanCreateTags",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanUpdateTagsField =
            new()
            {
                Name = "CanUpdateTags",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField GroupUsersCanDeleteTagsField =
            new()
            {
                Name = "CanDeleteTags",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseTable GroupUsersTable = new()
        {
            Name = "GroupUsers",
            Alias = "groupUser",
            Fields =
            [
                GroupUsersGroupIdField,
                GroupUsersUserIdField,

                GroupUsersCanReadWorkItemsField,
                GroupUsersCanCreateWorkItemsField,
                GroupUsersCanUpdateWorkItemsField,
                GroupUsersCanDeleteWorkItemsField,

                GroupUsersCanCreateTagsField,
                GroupUsersCanUpdateTagsField,
                GroupUsersCanDeleteTagsField,

                GroupUsersIsAdminField,
                GroupUsersIsOwnerField,
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "PK_GroupUsers",
                    Type = DatabaseConstraintType.CompositePrimaryKey,
                    Fields = [GroupUsersGroupIdField, GroupUsersUserIdField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_GroupUsers_Groups",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [GroupUsersGroupIdField],
                    ReferenceTable = GroupsTable,
                    ReferenceFields = [GroupIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "FK_GroupUsers_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [GroupUsersUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                }
            ]
        };
        #endregion

        #region Logins
        public static readonly DatabaseField LoginIdField =
            new()
            {
                Name = "LoginId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField LoginUserNameField =
            new()
            {
                Name = "Username",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50
            };

        public static readonly DatabaseField LoginAttemptTimeUtcField =
            new()
            {
                Name = "AttemptTimeUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseField LoginStatusField =
            new()
            {
                Name = "Status",
                Type = SqlDbType.VarChar,
                Nullable = false,
                FieldSize = 50
            };

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
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "CHK_Logins_Status",
                    Type = DatabaseConstraintType.Check,
                    Fields = [LoginStatusField],
                    CheckSql = $"{LoginStatusField.SelectName} IN ('Successful','Failed')"
                }
            ]
        };
        #endregion

        #region Sessions
        public static readonly DatabaseField SessionIdField =
            new()
            {
                Name = "SessionId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField SessionUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = true
            };

        public static readonly DatabaseField SessionLoginIdField =
            new()
            {
                Name = "LoginId",
                Type = SqlDbType.Int,
                Nullable = true
            };

        public static readonly DatabaseField SessionTokenField =
            new()
            {
                Name = "HashedToken",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 3000
            };

        public static readonly DatabaseField SessionIpIssuedToField =
            new()
            {
                Name = "IpIssuedTo",
                Type = SqlDbType.VarChar,
                Nullable = false,
                FieldSize = 45
            };

        public static readonly DatabaseField SessionIssuedAtUtcField =
            new()
            {
                Name = "IssuedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseField SessionExpiresAtUtcField =
            new()
            {
                Name = "ExpiresAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "(DATEADD(MINUTE, 120, SYSUTCDATETIME()))"
            };

        public static readonly DatabaseField SessionRevokedField =
            new()
            {
                Name = "Revoked",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField SessionIsGuestField =
            new()
            {
                Name = "IsGuest",
                Type = SqlDbType.Bit,
                Nullable = false,
                DefaultValue = "0"
            };

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
                SessionIsGuestField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_Sessions_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [SessionUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "FK_Sessions_Logins",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [SessionLoginIdField],
                    ReferenceTable = LoginsTable,
                    ReferenceFields = [LoginIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "UQ_Sessions_Token",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [SessionTokenField]
                }
            ]
        };
        #endregion

        #region WorkItems
        public static readonly DatabaseField WorkItemIdField =
            new()
            {
                Name = "ItemId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField WorkItemGroupIdField =
            new()
            {
                Name = "GroupId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField WorkItemNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 20
            };

        public static readonly DatabaseField WorkItemDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField WorkItemPointsField =
            new()
            {
                Name = "Points",
                Type = SqlDbType.Decimal,
                Nullable = false,
                FieldSize = 5,
                Precision = 2,
                DefaultValue = "0.0"
            };

        public static readonly DatabaseField WorkItemTypeField =
            new()
            {
                Name = "Type",
                Type = SqlDbType.VarChar,
                Nullable = false,
                FieldSize = 20
            };

        public static readonly DatabaseField WorkItemCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

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
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_WorkItems_Group_Item",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [WorkItemGroupIdField, WorkItemIdField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_WorkItems_Groups",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [WorkItemGroupIdField],
                    ReferenceTable = GroupsTable,
                    ReferenceFields = [GroupIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "CHK_WorkItems_Points",
                    Type = DatabaseConstraintType.Check,
                    Fields = [WorkItemPointsField],
                    CheckSql = $"{WorkItemPointsField.SelectName} >= 0"
                },
                new DatabaseConstraint
                {
                    Name = "CHK_WorkItems_Type",
                    Type = DatabaseConstraintType.Check,
                    Fields = [WorkItemTypeField],
                    CheckSql = $"{WorkItemTypeField.SelectName} IN ('Task','Story','Epic','Project')"
                }
            ]
        };
        #endregion

        #region WorkItemParents
        public static readonly DatabaseField WorkItemLinkGroupIdField =
            new()
            {
                Name = "GroupId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField WorkItemParentIdField =
            new()
            {
                Name = "ParentId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField WorkItemChildIdField =
            new()
            {
                Name = "ChildId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseTable WorkItemLinksTable = new()
        {
            Name = "WorkItemLinks",
            Alias = "workItemLink",
            Fields =
            [
                WorkItemLinkGroupIdField,
                WorkItemParentIdField,
                WorkItemChildIdField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "PK_WorkItemLinks",
                    Type = DatabaseConstraintType.CompositePrimaryKey,
                    Fields = [WorkItemLinkGroupIdField, WorkItemParentIdField, WorkItemChildIdField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_WorkItemLinks_Parent",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [WorkItemLinkGroupIdField, WorkItemParentIdField],
                    ReferenceTable = WorkItemsTable,
                    ReferenceFields = [WorkItemGroupIdField, WorkItemIdField],
                    OnDelete = "NO ACTION"
                },
                new DatabaseConstraint
                {
                    Name = "FK_WorkItemLinks_Child",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [WorkItemLinkGroupIdField, WorkItemChildIdField],
                    ReferenceTable = WorkItemsTable,
                    ReferenceFields = [WorkItemGroupIdField, WorkItemIdField],
                    OnDelete = "NO ACTION"
                },
                new DatabaseConstraint
                {
                    Name = "CHK_WorkItemLinks_NotSelf",
                    Type = DatabaseConstraintType.Check,
                    Fields = [WorkItemParentIdField, WorkItemChildIdField],
                    CheckSql = $"{WorkItemParentIdField.SelectName} <> {WorkItemChildIdField.SelectName}"
                }
            ]
        };
        #endregion

        #region UserTags
        public static readonly DatabaseField UserTagIdField =
            new()
            {
                Name = "UserTagId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField UserTagUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField UserTagNameField =
            new()
            {
                Name = "TagName",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50
            };

        public static readonly DatabaseField UserTagColorField =
            new()
            {
                Name = "TagColor",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 7,
                DefaultValue = "'#000000'"
            };

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
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_UserTags_User_TagName",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [UserTagUserIdField, UserTagNameField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_UserTags_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [UserTagUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                }
            ]
        };
        #endregion

        #region GroupTags
        public static readonly DatabaseField GroupTagIdField =
            new()
            {
                Name = "GroupTagId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField GroupTagGroupIdField =
            new()
            {
                Name = "GroupId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField GroupTagNameField =
            new()
            {
                Name = "TagName",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50
            };

        public static readonly DatabaseField GroupTagColorField =
            new()
            {
                Name = "TagColor",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 7,
                DefaultValue = "'#000000'"
            };

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
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "UQ_GroupTags_Group_TagName",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [GroupTagGroupIdField, GroupTagNameField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_GroupTags_Groups",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [GroupTagGroupIdField],
                    ReferenceTable = GroupsTable,
                    ReferenceFields = [GroupIdField],
                    OnDelete = "CASCADE"
                }
            ]
        };
        #endregion

        #region WorkItemTagAssignments
        public static readonly DatabaseField WorkItemTagItemIdField =
            new()
            {
                Name = "ItemId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField WorkItemTagIdField =
            new()
            {
                Name = "GroupTagId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseTable WorkItemTagAssignmentsTable = new()
        {
            Name = "WorkItemTagAssignments",
            Alias = "workItemTagAssignment",
            Fields =
            [
                WorkItemTagItemIdField,
                WorkItemTagIdField,
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "PK_WorkItemTagAssignments",
                    Type = DatabaseConstraintType.CompositePrimaryKey,
                    Fields = [WorkItemTagItemIdField, WorkItemTagIdField]
                },
                new DatabaseConstraint
                {
                    Name = "FK_WorkItemTagAssignments_WorkItems",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [WorkItemTagItemIdField],
                    ReferenceTable = WorkItemsTable,
                    ReferenceFields = [WorkItemIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "FK_WorkItemTagAssignments_GroupTags",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [WorkItemTagIdField],
                    ReferenceTable = GroupTagsTable,
                    ReferenceFields = [GroupTagIdField],
                    OnDelete = "NO ACTION"
                }
            ]
        };
        #endregion

        #region Notes
        public static readonly DatabaseField NoteIdField =
            new()
            {
                Name = "NoteId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField NoteUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField NoteTypeField =
            new()
            {
                Name = "Type",
                Type = SqlDbType.VarChar,
                Nullable = false,
                FieldSize = 20
            };

        public static readonly DatabaseField NoteTitleField =
            new()
            {
                Name = "Title",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50
            };

        public static readonly DatabaseField NoteContentField =
            new()
            {
                Name = "Content",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 500
            };

        public static readonly DatabaseField NoteCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

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
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_Notes_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [NoteUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "CHK_Notes_Type",
                    Type = DatabaseConstraintType.Check,
                    Fields = [NoteTypeField],
                    CheckSql = $"{NoteTypeField.SelectName} IN ('General','WorkItem','Personal')"
                }
            ]
        };
        #endregion

        #region Storages
        public static readonly DatabaseField StorageIdField =
            new()
            {
                Name = "StorageId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField StorageUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField StorageNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };

        public static readonly DatabaseField StorageDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField StorageCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable StoragesTable = new()
        {
            Name = "Storages",
            Alias = "storage",
            Fields =
            [
                StorageIdField,
                StorageUserIdField,
                StorageNameField,
                StorageDescriptionField,
                StorageCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_Storages_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [StorageUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "UQ_Storages_User_Name",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [StorageUserIdField, StorageNameField]
                }
            ]
        };
        #endregion

        #region StorageItems
        public static readonly DatabaseField StorageItemIdField =
            new()
            {
                Name = "StorageItemId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField StorageItemStorageIdField =
            new()
            {
                Name = "StorageId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField StorageItemNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };

        public static readonly DatabaseField StorageItemDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField StorageItemQuantityField =
            new()
            {
                Name = "Quantity",
                Type = SqlDbType.Int,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField StorageItemCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable StorageItemsTable = new()
        {
            Name = "StorageItems",
            Alias = "storageItem",
            Fields =
            [
                StorageItemIdField,
                StorageItemStorageIdField,
                StorageItemNameField,
                StorageItemQuantityField,
                StorageItemCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_StorageItems_Storages",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [StorageItemStorageIdField],
                    ReferenceTable = StoragesTable,
                    ReferenceFields = [StorageIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "UQ_StorageItems_Storage_Name",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [StorageItemStorageIdField, StorageItemNameField]
                },
                new DatabaseConstraint
                {
                    Name = "CHK_StorageItems_Quantity",
                    Type = DatabaseConstraintType.Check,
                    Fields = [StorageItemQuantityField],
                    CheckSql = $"{StorageItemQuantityField.SelectName} >= 0"
                }
            ]
        };
        #endregion

        #region Fridges
        public static readonly DatabaseField FridgeIdField =
            new()
            {
                Name = "FridgeId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField FridgeUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField FridgeNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };
        public static readonly DatabaseField FridgeDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField FridgeCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable FridgesTable = new()
        {
            Name = "Fridges",
            Alias = "fridge",
            Fields =
            [
                FridgeIdField,
                FridgeUserIdField,
                FridgeNameField,
                FridgeDescriptionField,
                FridgeCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_Fridges_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [FridgeUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "UQ_Fridges_User_Name",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [FridgeUserIdField, FridgeNameField]
                }
            ]
        };
        #endregion

        #region FridgeItems
        public static readonly DatabaseField FridgeItemIdField =
            new()
            {
                Name = "FridgeItemId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField FridgeItemFridgeIdField =
            new()
            {
                Name = "FridgeId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField FridgeItemNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };

        public static readonly DatabaseField FridgeItemQuantityField =
            new()
            {
                Name = "Quantity",
                Type = SqlDbType.Int,
                Nullable = false,
                DefaultValue = "0"
            };

        public static readonly DatabaseField FridgeItemCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable FridgeItemsTable = new()
        {
            Name = "FridgeItems",
            Alias = "fridgeItem",
            Fields =
            [
                FridgeItemIdField,
                FridgeItemFridgeIdField,
                FridgeItemNameField,
                FridgeItemQuantityField,
                FridgeItemCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_FridgeItems_Fridges",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [FridgeItemFridgeIdField],
                    ReferenceTable = FridgesTable,
                    ReferenceFields = [FridgeIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "UQ_FridgeItems_Fridge_Name",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [FridgeItemFridgeIdField, FridgeItemNameField]
                },
                new DatabaseConstraint
                {
                    Name = "CHK_FridgeItems_Quantity",
                    Type = DatabaseConstraintType.Check,
                    Fields = [FridgeItemQuantityField],
                    CheckSql = $"{FridgeItemQuantityField.SelectName} >= 0"
                }
            ]
        };
        #endregion

        #region BankAccounts
        public static readonly DatabaseField BankAccountIdField =
            new()
            {
                Name = "BankAccountId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField BankAccountUserIdField =
            new()
            {
                Name = "UserId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField BankAccountNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };
        public static readonly DatabaseField BankAccountDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField BankAccountBalanceField =
            new()
            {
                Name = "Balance",
                Type = SqlDbType.Decimal,
                Nullable = false,
                FieldSize = 18,
                Precision = 2,
                DefaultValue = "0.00"
            };

        public static readonly DatabaseField BankAccountCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable BankAccountsTable = new()
        {
            Name = "BankAccounts",
            Alias = "bankAccount",
            Fields =
            [
                BankAccountIdField,
                BankAccountUserIdField,
                BankAccountNameField,
                BankAccountBalanceField,
                BankAccountCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_BankAccounts_Users",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [BankAccountUserIdField],
                    ReferenceTable = UsersTable,
                    ReferenceFields = [UserIdField],
                    OnDelete = "CASCADE"
                },
                new DatabaseConstraint
                {
                    Name = "UQ_BankAccounts_User_Name",
                    Type = DatabaseConstraintType.Unique,
                    Fields = [BankAccountUserIdField, BankAccountNameField]
                }
            ]
        };
        #endregion

        #region Transactions
        public static readonly DatabaseField TransactionIdField =
            new()
            {
                Name = "TransactionId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField TransactionBankAccountIdField =
            new()
            {
                Name = "BankAccountId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField TransactionNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 50,
                DefaultValue = "''"
            };

        public static readonly DatabaseField TransactionAmountField =
            new()
            {
                Name = "Amount",
                Type = SqlDbType.Decimal,
                Nullable = false,
                FieldSize = 18,
                Precision = 2
            };

        public static readonly DatabaseField TransactionDescriptionField =
            new()
            {
                Name = "Description",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 300,
                DefaultValue = "''"
            };

        public static readonly DatabaseField TransactionCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable TransactionsTable = new()
        {
            Name = "Transactions",
            Alias = "transaction",
            Fields =
            [
                TransactionIdField,
                TransactionBankAccountIdField,
                TransactionAmountField,
                TransactionNameField,
                TransactionDescriptionField,
                TransactionCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_Transactions_BankAccounts",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [TransactionBankAccountIdField],
                    ReferenceTable = BankAccountsTable,
                    ReferenceFields = [BankAccountIdField],
                    OnDelete = "CASCADE"
                }
            ]
        };
        #endregion

        #region TransactionItems
        public static readonly DatabaseField TransactionItemIdField =
            new()
            {
                Name = "TransactionItemId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField TransactionItemTransactionIdField =
            new()
            {
                Name = "TransactionId",
                Type = SqlDbType.Int,
                Nullable = false
            };

        public static readonly DatabaseField TransactionItemNameField =
            new()
            {
                Name = "Name",
                Type = SqlDbType.NVarChar,
                Nullable = false,
                FieldSize = 100
            };

        public static readonly DatabaseField TransactionItemAmountField =
            new()
            {
                Name = "Amount",
                Type = SqlDbType.Decimal,
                Nullable = false,
                FieldSize = 18,
                Precision = 2
            };

        public static readonly DatabaseField TransactionItemCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime2,
                Nullable = false,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseTable TransactionItemsTable = new()
        {
            Name = "TransactionItems",
            Alias = "transactionItem",
            Fields =
            [
                TransactionItemIdField,
                TransactionItemTransactionIdField,
                TransactionItemNameField,
                TransactionItemAmountField,
                TransactionItemCreatedAtUtcField
            ],
            Constraints =
            [
                new DatabaseConstraint
                {
                    Name = "FK_TransactionItems_Transactions",
                    Type = DatabaseConstraintType.ForeignKey,
                    Fields = [TransactionItemTransactionIdField],
                    ReferenceTable = TransactionsTable,
                    ReferenceFields = [TransactionIdField],
                    OnDelete = "CASCADE"
                }
            ]
        };
        #endregion

        #region Images
        public static readonly DatabaseField ImageIdField = new()
        { Name = "ImageId", Type = SqlDbType.Int, Nullable = false, IsPrimaryKey = true, IsIdentity = true };

        public static readonly DatabaseField ImageNameField = new()
        { Name = "Name", Type = SqlDbType.NVarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField ImageContentTypeField = new()
        { Name = "ContentType", Type = SqlDbType.VarChar, Nullable = false, FieldSize = 100 };

        public static readonly DatabaseField ImageDataField = new()
        { Name = "Data", Type = SqlDbType.VarBinary, Nullable = false, FieldSize = null }; // VARBINARY(MAX)

        public static readonly DatabaseField ImageThumbnailField = new()
        { Name = "Thumbnail", Type = SqlDbType.VarBinary, Nullable = true, FieldSize = null };// VARBINARY(MAX)

        public static readonly DatabaseField ImageCreatedAtUtcField = new()
        { Name = "CreatedAtUtc", Type = SqlDbType.DateTime2, Nullable = false, DefaultValue = "SYSUTCDATETIME()" };

        public static readonly DatabaseField ImageSizeBytesField = new()
        { Name = "SizeBytes", Type = SqlDbType.BigInt, Nullable = false };

        public static readonly DatabaseField ImageWidthField = new()
        { Name = "Width", Type = SqlDbType.Int, Nullable = true };

        public static readonly DatabaseField ImageHeightField = new()
        { Name = "Height", Type = SqlDbType.Int, Nullable = true };

        public static readonly DatabaseTable ImagesTable = new()
        {
            Name = "Images",
            Alias = "image",
            Fields =
            [
                ImageIdField,
                ImageNameField,
                ImageContentTypeField,
                ImageDataField,
                ImageThumbnailField,
                ImageSizeBytesField,
                ImageWidthField,
                ImageHeightField,
                ImageCreatedAtUtcField
            ],
            Constraints =
            [
            ]
        };
        #endregion

        #region LogEvents
        public static readonly DatabaseField LogEventIdField =
            new()
            {
                Name = "LogId",
                Type = SqlDbType.Int,
                Nullable = false,
                IsPrimaryKey = true,
                IsIdentity = true
            };

        public static readonly DatabaseField LogEventMessageField =
            new()
            {
                Name = "Message",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = null // NVARCHAR(MAX)
            };

        public static readonly DatabaseField LogEventMessageTemplateField =
            new()
            {
                Name = "MessageTemplate",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = null // NVARCHAR(MAX)
            };

        public static readonly DatabaseField LogEventLevelField =
            new()
            {
                Name = "Level",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = 16
            };

        public static readonly DatabaseField LogEventCreatedAtUtcField =
            new()
            {
                Name = "CreatedAtUtc",
                Type = SqlDbType.DateTime,
                Nullable = true,
                DefaultValue = "SYSUTCDATETIME()"
            };

        public static readonly DatabaseField LogEventExceptionField =
            new()
            {
                Name = "Exception",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = null // NVARCHAR(MAX)
            };

        public static readonly DatabaseField LogEventPropertiesField =
            new()
            {
                Name = "Properties",
                Type = SqlDbType.NVarChar,
                Nullable = true,
                FieldSize = null // NVARCHAR(MAX)
            };

        public static readonly DatabaseTable LogEventsTable = new()
        {
            Name = "LogEvents",
            Alias = "logEvent",
            Fields =
            [
                LogEventIdField,
                LogEventLevelField,
                LogEventMessageField,
                LogEventMessageTemplateField,
                LogEventCreatedAtUtcField,
                LogEventExceptionField,
                LogEventPropertiesField
            ],
            Constraints =
            [
            ]
        };
        #endregion

        public static readonly List<DatabaseTable> DatabaseTables =
        [
            GroupsTable,
            ThemesTable,
            UsersTable,
            SettingsTable,

            LoginsTable,
            SessionsTable,

            GroupUsersTable,

            WorkItemsTable,
            WorkItemLinksTable,

            UserTagsTable,
            GroupTagsTable,
            WorkItemTagAssignmentsTable,

            NotesTable,

            StoragesTable,
            StorageItemsTable,

            FridgesTable,
            FridgeItemsTable,

            BankAccountsTable,
            TransactionsTable,
            TransactionItemsTable,

            ImagesTable,

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

    public static class ErrorMessages
    {
        public static string InvalidLogin(string userName) => $"Login failed for {userName} : Incorrect username or password.";
    }
}
