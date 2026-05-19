using Microsoft.Data.SqlClient;

using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Helpers;

public static class DtoReaders
{
    private static string Col(string? alias, string fieldName)
    {
        if (alias is null)
            return fieldName;
        else
            return $"{alias}_{fieldName}";
    }

    public static User ReadUser(SqlDataReader reader, string? alias = null)
    {
        return new User
        {
            UserId = reader.GetInt(Col(alias, UserIdField.Name)),
            Username = reader.GetStringValue(Col(alias, UserNameField.Name)),
            PasswordHash = reader.GetStringValue(Col(alias, UserPasswordHashField.Name)),
            Role = reader.GetEnum<UserRole>(Col(alias, UserRoleField.Name)),
            Email = reader.GetNullableString(Col(alias, UserEmailField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, UserCreatedAtUtcField.Name)),
            LastLoginUtc = reader.GetNullableDateTime(Col(alias, UserLastLoginUtcField.Name)),
            Deactivated = reader.GetBool(Col(alias, UserDeactivatedField.Name)),
            ThemeId = reader.GetNullableInt(Col(alias, UserThemeIdField.Name))
        };
    }

    public static Group ReadGroup(SqlDataReader reader, string? alias = null)
    {
        return new Group
        {
            GroupId = reader.GetInt(Col(alias, GroupIdField.Name)),
            Name = reader.GetStringValue(Col(alias, GroupNameField.Name)),
            Description = reader.GetNullableString(Col(alias, GroupDescriptionField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, GroupCreatedAtUtcField.Name))
        };
    }

    public static Theme ReadTheme(SqlDataReader reader, string? alias = null)
    {
        return new Theme
        {
            ThemeId = reader.GetInt(Col(alias, ThemeIdField.Name)),
            CreatorDescriptor = reader.GetStringValue(Col(alias, ThemeCreatorDescriptorField.Name)),
            Name = reader.GetStringValue(Col(alias, ThemeNameField.Name)),
            Data = reader.GetStringValue(Col(alias, ThemeDataField.Name)),
            Description = reader.GetStringValue(Col(alias, ThemeDescriptionField.Name)),
            IsDark = reader.GetBool(Col(alias, ThemeIsDarkField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, ThemeCreatedAtUtcField.Name)),
            IsDeleted = reader.GetBool(Col(alias, ThemeDeletedField.Name))
        };
    }

    public static UserSettings ReadUserSettings(SqlDataReader reader, string? alias = null)
    {
        return new UserSettings
        {
            SettingId = reader.GetInt(Col(alias, SettingsIdField.Name)),
            UserId = reader.GetInt(Col(alias, SettingsUserIdField.Name)),
            Timezone = reader.GetNullableString(Col(alias, SettingTimezoneField.Name)),
            DateFormat = reader.GetNullableString(Col(alias, SettingDateFormatField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, SettingsCreatedAtUtcField.Name)),
            LastUpdatedAtUtc = reader.GetDateTime(Col(alias, SettingsLastUpdatedAtUtcField.Name))
        };
    }

    public static GroupUser ReadGroupUser(SqlDataReader reader, string? alias = null)
    {
        return new GroupUser
        {
            GroupId = reader.GetInt(Col(alias, GroupUsersGroupIdField.Name)),
            UserId = reader.GetInt(Col(alias, GroupUsersUserIdField.Name)),
            CanReadWorkItems = reader.GetBool(Col(alias, GroupUsersCanReadWorkItemsField.Name)),
            CanCreateWorkItems = reader.GetBool(Col(alias, GroupUsersCanCreateWorkItemsField.Name))
        };
    }

    public static Login ReadLogin(SqlDataReader reader, string? alias = null)
    {
        return new Login
        {
            LoginId = reader.GetInt(Col(alias, LoginIdField.Name)),
            Username = reader.GetStringValue(Col(alias, LoginUserNameField.Name)),
            AttemptTimeUtc = reader.GetDateTime(Col(alias, LoginAttemptTimeUtcField.Name)),
            Status = reader.GetEnum<LoginStatus>(Col(alias, LoginStatusField.Name))
        };
    }

    public static Session ReadSession(SqlDataReader reader, string? alias = null)
    {
        return new Session
        {
            SessionId = reader.GetInt(Col(alias, SessionIdField.Name)),
            UserId = reader.GetInt(Col(alias, SessionUserIdField.Name)),
            LoginId = reader.GetInt(Col(alias, SessionLoginIdField.Name)),
            HashedToken = reader.GetStringValue(Col(alias, SessionTokenField.Name)),
            IpIssuedTo = reader.GetStringValue(Col(alias, SessionIpIssuedToField.Name)),
            IssuedAtUtc = reader.GetDateTime(Col(alias, SessionIssuedAtUtcField.Name)),
            ExpiresAtUtc = reader.GetDateTime(Col(alias, SessionExpiresAtUtcField.Name)),
            Revoked = reader.GetBool(Col(alias, SessionRevokedField.Name))
        };
    }

    public static WorkItem ReadWorkItem(SqlDataReader reader, string? alias = null)
    {
        return new WorkItem
        {
            ItemId = reader.GetInt(Col(alias, WorkItemIdField.Name)),
            GroupId = reader.GetInt(Col(alias, WorkItemGroupIdField.Name)),
            Name = reader.GetStringValue(Col(alias, WorkItemNameField.Name)),
            Description = reader.GetStringValue(Col(alias, WorkItemDescriptionField.Name)),
            Points = reader.GetDecimal(Col(alias, WorkItemPointsField.Name)),
            Type = reader.GetEnum<WorkItemType>(Col(alias, WorkItemTypeField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, WorkItemCreatedAtUtcField.Name))
        };
    }

    public static UserTag ReadUserTag(SqlDataReader reader, string? alias = null)
    {
        return new UserTag
        {
            TagId = reader.GetInt(Col(alias, UserTagIdField.Name)),
            UserId = reader.GetInt(Col(alias, UserTagUserIdField.Name)),
            Name = reader.GetStringValue(Col(alias, UserTagNameField.Name)),
            Color = reader.GetStringValue(Col(alias, UserTagColorField.Name))
        };
    }

    public static GroupTag ReadGroupTag(SqlDataReader reader, string? alias = null)
    {
        return new GroupTag
        {
            TagId = reader.GetInt(Col(alias, GroupTagIdField.Name)),
            GroupId = reader.GetInt(Col(alias, GroupTagGroupIdField.Name)),
            Name = reader.GetStringValue(Col(alias, GroupTagNameField.Name)),
            Color = reader.GetStringValue(Col(alias, GroupTagColorField.Name))
        };
    }

    public static Note ReadNote(SqlDataReader reader, string? alias = null)
    {
        return new Note
        {
            NoteId = reader.GetInt(Col(alias, NoteIdField.Name)),
            UserId = reader.GetInt(Col(alias, NoteUserIdField.Name)),
            Type = reader.GetEnum<NoteType>(Col(alias, NoteTypeField.Name)),
            Title = reader.GetStringValue(Col(alias, NoteTitleField.Name)),
            Content = reader.GetStringValue(Col(alias, NoteContentField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, NoteCreatedAtUtcField.Name))
        };
    }

    public static Storage ReadStorage(SqlDataReader reader, string? alias = null)
    {
        return new Storage
        {
            StorageId = reader.GetInt(Col(alias, StorageIdField.Name)),
            UserId = reader.GetInt(Col(alias, StorageUserIdField.Name)),
            Name = reader.GetStringValue(Col(alias, StorageNameField.Name)),
            Description = reader.GetStringValue(Col(alias, StorageDescriptionField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, StorageCreatedAtUtcField.Name))
        };
    }

    public static StorageItem ReadStorageItem(SqlDataReader reader, string? alias = null)
    {
        return new StorageItem
        {
            ItemId = reader.GetInt(Col(alias, StorageItemIdField.Name)),
            StorageId = reader.GetInt(Col(alias, StorageItemStorageIdField.Name)),
            Name = reader.GetStringValue(Col(alias, StorageItemNameField.Name)),
            Quantity = reader.GetInt(Col(alias, StorageItemQuantityField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, StorageItemCreatedAtUtcField.Name))
        };
    }

    public static Fridge ReadFridge(SqlDataReader reader, string? alias = null)
    {
        return new Fridge
        {
            FridgeId = reader.GetInt(Col(alias, FridgeIdField.Name)),
            UserId = reader.GetInt(Col(alias, FridgeUserIdField.Name)),
            Name = reader.GetStringValue(Col(alias, FridgeNameField.Name)),
            Description = reader.GetStringValue(Col(alias, FridgeDescriptionField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, FridgeCreatedAtUtcField.Name))
        };
    }

    public static FridgeItem ReadFridgeItem(SqlDataReader reader, string? alias = null)
    {
        return new FridgeItem
        {
            FridgeItemId = reader.GetInt(Col(alias, FridgeItemIdField.Name)),
            FridgeId = reader.GetInt(Col(alias, FridgeItemFridgeIdField.Name)),
            Name = reader.GetStringValue(Col(alias, FridgeItemNameField.Name)),
            Quantity = reader.GetInt(Col(alias, FridgeItemQuantityField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, FridgeItemCreatedAtUtcField.Name))
        };
    }

    public static BankAccount ReadBankAccount(SqlDataReader reader, string? alias = null)
    {
        return new BankAccount
        {
            AccountId = reader.GetInt(Col(alias, BankAccountIdField.Name)),
            UserId = reader.GetInt(Col(alias, BankAccountUserIdField.Name)),
            Name = reader.GetStringValue(Col(alias, BankAccountNameField.Name)),
            Description = reader.GetStringValue(Col(alias, BankAccountDescriptionField.Name)),
            Balance = reader.GetDecimal(Col(alias, BankAccountBalanceField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, BankAccountCreatedAtUtcField.Name))
        };
    }

    public static Transaction ReadTransaction(SqlDataReader reader, string? alias = null)
    {
        return new Transaction
        {
            TransactionId = reader.GetInt(Col(alias, TransactionIdField.Name)),
            AccountId = reader.GetInt(Col(alias, TransactionBankAccountIdField.Name)),
            Amount = reader.GetDecimal(Col(alias, TransactionAmountField.Name)),
            Name = reader.GetStringValue(Col(alias, TransactionNameField.Name)),
            Description = reader.GetStringValue(Col(alias, TransactionDescriptionField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, TransactionCreatedAtUtcField.Name))
        };
    }

    public static TransactionItem ReadTransactionItem(SqlDataReader reader, string? alias = null)
    {
        return new TransactionItem
        {
            ItemId = reader.GetInt(Col(alias, TransactionItemIdField.Name)),
            TransactionId = reader.GetInt(Col(alias, TransactionItemTransactionIdField.Name)),
            Name = reader.GetStringValue(Col(alias, TransactionItemNameField.Name)),
            Amount = reader.GetDecimal(Col(alias, TransactionItemAmountField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, TransactionItemCreatedAtUtcField.Name))
        };
    }

    public static Image ReadImage(SqlDataReader reader, string? alias = null)
    {
        return new Image
        {
            ImageId = reader.GetInt(Col(alias, ImageIdField.Name)),
            Name = reader.GetStringValue(Col(alias, ImageNameField.Name)),
            ContentType = reader.GetStringValue(Col(alias, ImageContentTypeField.Name)),
            Data = reader.GetBytes(Col(alias, ImageDataField.Name)),
            Thumbnail = reader.GetNullableBytes(Col(alias, ImageThumbnailField.Name)),
            SizeBytes = reader.GetLong(Col(alias, ImageSizeBytesField.Name)),
            Width = reader.GetNullableInt(Col(alias, ImageWidthField.Name)),
            Height = reader.GetNullableInt(Col(alias, ImageHeightField.Name)),
            CreatedAtUtc = reader.GetDateTime(Col(alias, ImageCreatedAtUtcField.Name))
        };
    }
}
