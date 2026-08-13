using MoopelObjects.Dto;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace MoopelApi.Helpers;

public static class CheckMapping
{
    private const string e = "";
    private const int i = 0;

#pragma warning disable CS0649 // Always have default value
    private static readonly DateTime t;

#pragma warning disable CS0649 // Always have default value
    public static void CheckRequests()
    {
        CheckBank();
        CheckFridge();
        CheckTransaction();
        CheckGroup();
        CheckNote();
        CheckStorage();
        CheckTag();
        CheckTheme();
        CheckImage();
        CheckUserSettings();
        CheckWorkItem();
    }
    public static void CheckDatabaseFields()
    {

    }

    #region Requests
    private static void CheckBank()
    {
        BankAccountCreateRequest baCreate = new()
        {
            Description = e,
            Name = e
        };
        BankAccountUpdateRequest baUpdate = new()
        {
            Name = e,
            Description = e
        };


        BankAccount ba1 = new()
        {
            AccountId = i,
            UserId = i,
            Name = baCreate.Name,
            Description = baCreate.Description,
            Balance = i,
            CreatedAtUtc = t,
        };

        BankAccount ba2 = new()
        {
            AccountId = i,
            UserId = i,
            Name = baUpdate.Name,
            Description = baUpdate.Description,
            Balance = i,
            CreatedAtUtc = t,
        };

        TransactionItemCreateRequest tiCreate = new()
        {
            Amount = i,
            Name = e,
        };
        TransactionItemUpdateRequest tiUpdate = new()
        {
            Amount = i,
            Name = e,
        };

        TransactionItem ti1 = new()
        {
            ItemId = i,
            TransactionId = i,
            Name = tiCreate.Name,
            Amount = tiCreate.Amount,
            CreatedAtUtc = t,
        };
        TransactionItem ti2 = new()
        {
            ItemId = i,
            TransactionId = i,
            Name = tiUpdate.Name,
            Amount = tiUpdate.Amount,
            CreatedAtUtc = t,
        };

    }

    private static void CheckFridge()
    {
        FridgeCreateRequest fCreate = new()
        {
            Name = e,
            Description = e
        };

        FridgeUpdateRequest fUpdate = new()
        {
            Name = e,
            Description = e
        };

        Fridge f1 = new()
        {
            UserId = i,
            FridgeId = i,
            Name = fCreate.Name,
            Description = fCreate.Description,
            CreatedAtUtc = t
        };

        FridgeItemCreateRequest fiCreate = new()
        {
            Name = e,
            Quantity = i
        };

        FridgeItemUpdateRequest fiUpdate = new()
        {
            Name = e,
            Quantity = i
        };

        FridgeItem fi1 = new()
        {
            FridgeItemId = i,
            FridgeId = i,
            Name = fiCreate.Name,
            Quantity = fiCreate.Quantity,
            CreatedAtUtc = t
        };
    }

    private static void CheckTransaction()
    {
        TransactionCreateRequest txCreate = new()
        {
            Name = e,
            Description = e
        };

        TransactionUpdateRequest txUpdate = new()
        {
            Name = e,
            Description = e
        };

        Transaction tx1 = new()
        {
            TransactionId = i,
            AccountId = i,
            Amount = i,
            Name = txCreate.Name,
            Description = txCreate.Description,
            CreatedAtUtc = t
        };

        TransactionItemCreateRequest tiCreate = new()
        {
            Name = e,
            Amount = i
        };

        TransactionItemUpdateRequest tiUpdate = new()
        {
            Name = e,
            Amount = i
        };

        TransactionItem ti1 = new()
        {
            ItemId = i,
            TransactionId = i,
            Name = tiCreate.Name,
            Amount = tiCreate.Amount,
            CreatedAtUtc = t
        };
    }

    private static void CheckGroup()
    {
        GroupCreateRequest gCreate = new()
        {
            Name = e,
            Description = e
        };

        GroupUpdateRequest gUpdate = new()
        {
            Name = e,
            Description = e
        };

        GroupUserUpdateRequest guUpdate = new()
        {
            CanCreateWorkItems = true,
            CanViewWorkItems = true,
            CanDeleteWorkItems = true,
            CanUpdateWorkItems = true,

            CanUpdateTags = true,
            CanDeleteTags = true,
            CanCreateTags = true
        };

        Group g1 = new()
        {
            GroupId = i,
            Name = gCreate.Name,
            Description = gCreate.Description,
            CreatedAtUtc = t
        };

        GroupUser gu1 = new()
        {
            GroupId = i,
            UserId = i,

            CanCreateWorkItems = guUpdate.CanCreateWorkItems,
            CanReadWorkItems = guUpdate.CanViewWorkItems,
            CanUpdateWorkItems = guUpdate.CanUpdateWorkItems,
            CanDeleteWorkItems = guUpdate.CanDeleteWorkItems,

            CanCreateTags = guUpdate.CanCreateTags,
            CanUpdateTags = guUpdate.CanUpdateTags,
            CanDeleteTags = guUpdate.CanDeleteTags,

            IsOwner = false,
            IsAdmin = false,

        };

        GroupTag gt1 = new()
        {
            GroupId = i,
            TagId = i,
            Name = e,
            Color = e
        };
    }

    private static void CheckNote()
    {
        NoteCreateRequest nCreate = new()
        {
            Title = e,
            Content = e,
            Type = default
        };

        NoteUpdateRequest nUpdate = new()
        {
            Title = e,
            Content = e,
            Type = default
        };

        Note n1 = new()
        {
            NoteId = i,
            UserId = i,
            Type = nCreate.Type,
            Title = nCreate.Title,
            Content = nCreate.Content,
            CreatedAtUtc = t
        };

        Note n2 = new()
        {
            NoteId = i,
            UserId = i,
            Type = nUpdate.Type,
            Title = nUpdate.Title,
            Content = nUpdate.Content,
            CreatedAtUtc = t
        };
    }

    private static void CheckStorage()
    {
        StorageCreateRequest sCreate = new()
        {
            Name = e,
            Description = e,
        };

        StorageUpdateRequest sUpdate = new()
        {
            Name = e,
            Description = e,
        };

        Storage s1 = new()
        {
            StorageId = i,
            UserId = i,
            Name = sCreate.Name,
            Description = sCreate.Description,
            CreatedAtUtc = t
        };

        Storage s2 = new()
        {
            StorageId = i,
            UserId = i,
            Name = sUpdate.Name,
            Description = sUpdate.Description,
            CreatedAtUtc = t
        };

        StorageItem si1 = new()
        {
            ItemId = i,
            StorageId = i,
            Name = sCreate.Name,
            Description = sCreate.Description,
            Quantity = i,
            CreatedAtUtc = t
        };
    }

    private static void CheckTag()
    {
        TagCreateRequest tc = new()
        {
            Name = e,
            Color = e
        };

        TagUpdateRequest tu = new()
        {
            Name = e,
            Color = e
        };

        Tag t1 = new()
        {
            TagId = i,
            Name = tc.Name,
            Color = tc.Color
        };

        UserTag ut1 = new()
        {
            UserId = i,
            TagId = i,
            Name = e,
            Color = e
        };
    }

    private static void CheckTheme()
    {
        ThemeCreateRequest thc = new()
        {
            Name = e,
            Data = e,
            Description = e,
            IsDark = false
        };

        ThemeUpdateRequest thu = new()
        {
            Name = e,
            Data = e,
            Description = e,
            IsDark = false
        };

        Theme th1 = new()
        {
            ThemeId = i,
            CreatorDescriptor = e,
            Name = thc.Name,
            Data = thc.Data,
            Description = thc.Description,
            IsDark = thc.IsDark,
            CreatedAtUtc = t,
            IsDeleted = false
        };
    }

    private static void CheckImage()
    {
        Image img = new()
        {
            ImageId = i,
            Name = e,
            ContentType = e,
            Data = [],
            Thumbnail = [],
            SizeBytes = 0,
            Width = null,
            Height = null,
            CreatedAtUtc = t
        };
    }

    private static void CheckUserSettings()
    {
        UserSettingsCreateRequest usc = new()
        {
            Timezone = e,
            DateFormat = e
        };

        UserSettingsUpdateRequest usu = new()
        {
            Timezone = e,
            DateFormat = e
        };

        UserSettings us = new()
        {
            SettingId = i,
            UserId = i,
            Timezone = usc.Timezone,
            DateFormat = usc.DateFormat,
            CreatedAtUtc = t,
            LastUpdatedAtUtc = t
        };
    }

    private static void CheckWorkItem()
    {
        WorkItemCreateRequest wic = new()
        {
            Name = e,
            Description = e,
            Points = i,
            Type = default
        };

        WorkItem wi = new()
        {
            ItemId = i,
            GroupId = i,
            Name = wic.Name,
            Description = wic.Description,
            Points = wic.Points,
            Type = wic.Type,
            CreatedAtUtc = t
        };
    }
    #endregion
#pragma warning restore IDE0059 // Unnecessary assignment of a value

}
