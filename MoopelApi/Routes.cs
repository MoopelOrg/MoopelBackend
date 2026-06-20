// ReSharper disable MemberHidesStaticFromOuterClass
namespace MoopelApi;

public static class Routes
{
    public static class Auth
    {
        private const string Base = "auth";

        private static class Actions
        {
            public const string Login = "/login";
            public const string GuestLogin = "/guest-login";
            public const string Logout = "/logout";
            public const string Register = "/register";
            public const string Me = "/me";
        }

        public const string Login = Base + Actions.Login;
        public const string GuestLogin = Base + Actions.GuestLogin;
        public const string Logout = Base + Actions.Logout;
        public const string Register = Base + Actions.Register;
        public const string Me = Base + Actions.Me;
    }

    public static class User
    {
        private const string Base = "user";

        private static class Actions
        {
            public const string UserById = "/{userId:int}";
            public const string UserByName = "/{username}";
            public const string UpdateUser = "/update/{userId:int}";
            public const string DisableUser = "/disable/{userId:int}";
        }

        public const string UserById = Base + Actions.UserById;
        public const string UserByName = Base + Actions.UserByName;
        public const string UpdateUser = Base + Actions.UpdateUser;
        public const string DisableUser = Base + Actions.DisableUser;

        public static class Generate
        {
            public static string UserById(int userId) => User.UserById.Replace("{userId:int}", userId.ToString());
            public static string UserByName(string username) => User.UserByName.Replace("{username}", Uri.EscapeDataString(username));
            public static string DisableUser(int userId) => User.DisableUser.Replace("{userId:int}", userId.ToString());
        }
    }

    public static class UserSettings
    {
        private const string Base = "usersettings";

        private static class Actions
        {
            public const string MySettings = "/me";
            public const string SettingsById = "/{settingId:int}";
            public const string CreateSettings = "/create";
            public const string UpdateSettings = "/update";
            public const string DeleteSettings = "/delete/{settingId:int}";
        }

        public const string MySettings = Base + Actions.MySettings;
        public const string SettingsById = Base + Actions.SettingsById;
        public const string CreateSettings = Base + Actions.CreateSettings;
        public const string UpdateSettings = Base + Actions.UpdateSettings;
        public const string DeleteSettings = Base + Actions.DeleteSettings;

        public static class Generate
        {
            public static string SettingsById(int settingId) => UserSettings.SettingsById.Replace("{settingId:int}", settingId.ToString());
            public static string DeleteSettings(int settingId) => UserSettings.DeleteSettings.Replace("{settingId:int}", settingId.ToString());
        }
    }

    public static class Group
    {
        private const string Base = "group";

        private static class Actions
        {
            public const string MyGroups = "/mygroups";
            public const string GroupById = "/{groupId:int}";
            public const string CreateGroup = "/create";
            public const string UpdateGroup = "/update/{groupId:int}";
            public const string DeleteGroup = "/delete/{groupId:int}";
        }

        public const string MyGroups = Base + Actions.MyGroups;
        public const string GroupById = Base + Actions.GroupById;
        public const string CreateGroup = Base + Actions.CreateGroup;
        public const string UpdateGroup = Base + Actions.UpdateGroup;
        public const string DeleteGroup = Base + Actions.DeleteGroup;
        public static class Generate
        {
            public static string GroupById(int groupId) => Group.GroupById.Replace("{groupId:int}", groupId.ToString());
            public static string UpdateGroup(int groupId) => Group.UpdateGroup.Replace("{groupId:int}", groupId.ToString());
            public static string DeleteGroup(int groupId) => Group.DeleteGroup.Replace("{groupId:int}", groupId.ToString());
        }
    }

    public static class Note
    {
        private const string Base = "note";

        private static class Actions
        {
            public const string MyNotes = "/my";
            public const string NoteById = "/{noteId:int}";
            public const string CreateNote = "/create";
            public const string UpdateNote = "/update/{noteId:int}";
            public const string DeleteNote = "/delete/{noteId:int}";
        }

        public const string MyNotes = Base + Actions.MyNotes;
        public const string NoteById = Base + Actions.NoteById;
        public const string CreateNote = Base + Actions.CreateNote;
        public const string UpdateNote = Base + Actions.UpdateNote;
        public const string DeleteNote = Base + Actions.DeleteNote;
        public static class Generate
        {
            public static string NoteById(int noteId) => Note.NoteById.Replace("{noteId:int}", noteId.ToString());
            public static string UpdateNote(int noteId) => Note.UpdateNote.Replace("{noteId:int}", noteId.ToString());
            public static string DeleteNote(int noteId) => Note.DeleteNote.Replace("{noteId:int}", noteId.ToString());
        }
    }

    public static class Tag
    {
        private const string Base = "tag";

        private static class Actions
        {
            public const string UserTags = "/user";
            public const string UserTagById = "/user/{tagId:int}";
            public const string CreateUserTag = "/user/create";
            public const string UpdateUserTag = "/user/update/{tagId:int}";
            public const string DeleteUserTag = "/user/delete/{tagId:int}";

            public const string GroupTagsByGroupId = "/group/{groupId:int}";
            public const string GroupTagById = "/group/{tagId:int}";
            public const string CreateGroupTag = "/group/{groupId:int}/create";
            public const string UpdateGroupTag = "/group/update/{tagId:int}";
            public const string DeleteGroupTag = "/group/delete/{tagId:int}";

        }

        public const string UserTags = Base + Actions.UserTags;
        public const string UserTagById = Base + Actions.UserTagById;
        public const string CreateUserTag = Base + Actions.CreateUserTag;
        public const string UpdateUserTag = Base + Actions.UpdateUserTag;
        public const string DeleteUserTag = Base + Actions.DeleteUserTag;

        public const string GroupTagsByGroupId = Base + Actions.GroupTagsByGroupId;
        public const string GroupTagById = Base + Actions.GroupTagById;
        public const string CreateGroupTag = Base + Actions.CreateGroupTag;
        public const string UpdateGroupTag = Base + Actions.UpdateGroupTag;
        public const string DeleteGroupTag = Base + Actions.DeleteGroupTag;
        public static class Generate
        {
            public static string UserTagById(int tagId) => Tag.UserTagById.Replace("{tagId:int}", tagId.ToString());
            public static string UpdateUserTag(int tagId) => Tag.UpdateUserTag.Replace("{tagId:int}", tagId.ToString());
            public static string DeleteUserTag(int tagId) => Tag.DeleteUserTag.Replace("{tagId:int}", tagId.ToString());

            public static string GroupTagsByGroupId(int groupId) => Tag.GroupTagsByGroupId.Replace("{groupId:int}", groupId.ToString());
            public static string GroupTagById(int tagId) => Tag.GroupTagById.Replace("{tagId:int}", tagId.ToString());
            public static string CreateGroupTag(int groupId) => Tag.CreateGroupTag.Replace("{groupId:int}", groupId.ToString());
            public static string UpdateGroupTag(int tagId) => Tag.UpdateGroupTag.Replace("{tagId:int}", tagId.ToString());
            public static string DeleteGroupTag(int tagId) => Tag.DeleteGroupTag.Replace("{tagId:int}", tagId.ToString());
        }
    }

    public static class Theme
    {
        private const string Base = "theme";

        private static class Actions
        {
            public const string AllThemes = "";
            public const string ThemeById = "/{themeId:int}";
            public const string MyTheme = "/my";
            public const string CreateTheme = "/create";
            public const string UpdateTheme = "/update/{themeId:int}";
            public const string DeleteTheme = "/delete/{themeId:int}";
            public const string SetMyTheme = "/set/{themeId:int}";
        }

        public const string AllThemes = Base + Actions.AllThemes;
        public const string ThemeById = Base + Actions.ThemeById;
        public const string MyTheme = Base + Actions.MyTheme;
        public const string CreateTheme = Base + Actions.CreateTheme;
        public const string UpdateTheme = Base + Actions.UpdateTheme;
        public const string DeleteTheme = Base + Actions.DeleteTheme;
        public const string SetMyTheme = Base + Actions.SetMyTheme;
        public static class Generate
        {
            public static string ThemeById(int themeId) => Theme.ThemeById.Replace("{themeId:int}", themeId.ToString());
            public static string UpdateTheme(int themeId) => Theme.UpdateTheme.Replace("{themeId:int}", themeId.ToString());
            public static string DeleteTheme(int themeId) => Theme.DeleteTheme.Replace("{themeId:int}", themeId.ToString());
            public static string SetMyTheme(int themeId) => Theme.SetMyTheme.Replace("{themeId:int}", themeId.ToString());
        }
    }

    public static class Session
    {
        private const string Base = "session";

        private static class Actions
        {
            public const string MySessions = "/my";
            public const string SessionById = "/{sessionId:int}";
            public const string RevokeSession = "/revoke/{sessionId:int}";
            public const string RevokeAll = "/revoke-all";
        }

        public const string MySessions = Base + Actions.MySessions;
        public const string SessionById = Base + Actions.SessionById;
        public const string RevokeSession = Base + Actions.RevokeSession;
        public const string RevokeAll = Base + Actions.RevokeAll;
        public static class Generate
        {
            public static string SessionById(int sessionId) => Session.SessionById.Replace("{sessionId:int}", sessionId.ToString());
            public static string RevokeSession(int sessionId) => Session.RevokeSession.Replace("{sessionId:int}", sessionId.ToString());
        }
    }

    public static class WorkItem
    {
        private const string Base = "workitem";

        private static class Actions
        {
            public const string WorkItemsByGroupId = "/group/{groupId:int}";
            public const string WorkItemsByGroupIdAndType = "/group/{groupId:int}/type/{type}";
            public const string WorkItemById = "/{workItemId:int}";
            public const string CreateWorkItem = "/create/{groupId:int}";
            public const string UpdateWorkItem = "/update/{workItemId:int}";
            public const string DeleteWorkItem = "/delete/{workItemId:int}";
            public const string AddLink = "/link/add";
            public const string RemoveLink = "/link/remove";
        }

        public const string WorkItemsByGroupId = Base + Actions.WorkItemsByGroupId;
        public const string WorkItemsByGroupIdAndType = Base + Actions.WorkItemsByGroupIdAndType;
        public const string WorkItemById = Base + Actions.WorkItemById;
        public const string CreateWorkItem = Base + Actions.CreateWorkItem;
        public const string UpdateWorkItem = Base + Actions.UpdateWorkItem;
        public const string DeleteWorkItem = Base + Actions.DeleteWorkItem;
        public const string AddLink = Base + Actions.AddLink;
        public const string RemoveLink = Base + Actions.RemoveLink;
        public static class Generate
        {
            public static string WorkItemsByGroupId(int groupId) => WorkItem.WorkItemsByGroupId.Replace("{groupId:int}", groupId.ToString());
            public static string WorkItemsByGroupIdAndType(int groupId, string type) => WorkItem.WorkItemsByGroupIdAndType.Replace("{groupId:int}", groupId.ToString()).Replace("{type}", Uri.EscapeDataString(type));
            public static string WorkItemById(int workItemId) => WorkItem.WorkItemById.Replace("{workItemId:int}", workItemId.ToString());
            public static string CreateWorkItem(int groupId) => WorkItem.CreateWorkItem.Replace("{groupId:int}", groupId.ToString());
            public static string UpdateWorkItem(int workItemId) => WorkItem.UpdateWorkItem.Replace("{workItemId:int}", workItemId.ToString());
            public static string DeleteWorkItem(int workItemId) => WorkItem.DeleteWorkItem.Replace("{workItemId:int}", workItemId.ToString());
        }
    }

    public static class Storage
    {
        private const string Base = "storage";

        private static class Actions
        {
            public const string MyStorages = "/my";
            public const string StorageById = "/{storageId:int}";
            public const string CreateStorage = "/create";
            public const string UpdateStorage = "/update/{storageId:int}";
            public const string DeleteStorage = "/delete/{storageId:int}";

            public const string CreateStorageItem = "/{storageId:int}/items/create";
            public const string UpdateStorageItem = "/items/update/{storageItemId:int}";
            public const string DeleteStorageItem = "/items/delete/{storageItemId:int}";
        }

        public const string MyStorages = Base + Actions.MyStorages;
        public const string StorageById = Base + Actions.StorageById;
        public const string CreateStorage = Base + Actions.CreateStorage;
        public const string UpdateStorage = Base + Actions.UpdateStorage;
        public const string DeleteStorage = Base + Actions.DeleteStorage;

        public const string CreateStorageItem = Base + Actions.CreateStorageItem;
        public const string UpdateStorageItem = Base + Actions.UpdateStorageItem;
        public const string DeleteStorageItem = Base + Actions.DeleteStorageItem;
        public static class Generate
        {
            public static string StorageById(int storageId) => Storage.StorageById.Replace("{storageId:int}", storageId.ToString());
            public static string UpdateStorage(int storageId) => Storage.UpdateStorage.Replace("{storageId:int}", storageId.ToString());
            public static string DeleteStorage(int storageId) => Storage.DeleteStorage.Replace("{storageId:int}", storageId.ToString());

            public static string CreateStorageItem(int storageId) => Storage.CreateStorageItem.Replace("{storageId:int}", storageId.ToString());
            public static string UpdateStorageItem(int storageItemId) => Storage.UpdateStorageItem.Replace("{storageItemId:int}", storageItemId.ToString());
            public static string DeleteStorageItem(int storageItemId) => Storage.DeleteStorageItem.Replace("{storageItemId:int}", storageItemId.ToString());
        }
    }

    public static class Fridge
    {
        private const string Base = "fridge";

        private static class Actions
        {
            public const string MyFridges = "/my";
            public const string FridgeById = "/{fridgeId:int}";
            public const string CreateFridge = "/create";
            public const string UpdateFridge = "/update/{fridgeId:int}";
            public const string DeleteFridge = "/delete/{fridgeId:int}";

            public const string CreateFridgeItem = "/{fridgeId:int}/items/create";
            public const string UpdateFridgeItem = "/items/update/{fridgeItemId:int}";
            public const string DeleteFridgeItem = "/items/delete/{fridgeItemId:int}";
        }

        public const string MyFridges = Base + Actions.MyFridges;
        public const string FridgeById = Base + Actions.FridgeById;
        public const string CreateFridge = Base + Actions.CreateFridge;
        public const string UpdateFridge = Base + Actions.UpdateFridge;
        public const string DeleteFridge = Base + Actions.DeleteFridge;

        public const string CreateFridgeItem = Base + Actions.CreateFridgeItem;
        public const string UpdateFridgeItem = Base + Actions.UpdateFridgeItem;
        public const string DeleteFridgeItem = Base + Actions.DeleteFridgeItem;

        public static class Generate
        {
            public static string FridgeById(int fridgeId) => Fridge.FridgeById.Replace("{fridgeId:int}", fridgeId.ToString());
            public static string UpdateFridge(int fridgeId) => Fridge.UpdateFridge.Replace("{fridgeId:int}", fridgeId.ToString());
            public static string DeleteFridge(int fridgeId) => Fridge.DeleteFridge.Replace("{fridgeId:int}", fridgeId.ToString());

            public static string CreateFridgeItem(int fridgeId) => Fridge.CreateFridgeItem.Replace("{fridgeId:int}", fridgeId.ToString());
            public static string UpdateFridgeItem(int fridgeItemId) => Fridge.UpdateFridgeItem.Replace("{fridgeItemId:int}", fridgeItemId.ToString());
            public static string DeleteFridgeItem(int fridgeItemId) => Fridge.DeleteFridgeItem.Replace("{fridgeItemId:int}", fridgeItemId.ToString());
        }
    }

    public static class Bank
    {
        private const string Base = "bank";

        private static class Actions
        {
            public const string MyAccounts = "/accounts/my";
            public const string AccountById = "/accounts/{accountId:int}";
            public const string CreateAccount = "/accounts/create";
            public const string UpdateAccount = "/accounts/update/{accountId:int}";
            public const string DeleteAccount = "/accounts/delete/{accountId:int}";
        }

        public const string MyAccounts = Base + Actions.MyAccounts;
        public const string AccountById = Base + Actions.AccountById;
        public const string CreateAccount = Base + Actions.CreateAccount;
        public const string UpdateAccount = Base + Actions.UpdateAccount;
        public const string DeleteAccount = Base + Actions.DeleteAccount;
        public static class Generate
        {
            public static string AccountById(int accountId) => Bank.AccountById.Replace("{accountId:int}", accountId.ToString());
            public static string UpdateAccount(int accountId) => Bank.UpdateAccount.Replace("{accountId:int}", accountId.ToString());
            public static string DeleteAccount(int accountId) => Bank.DeleteAccount.Replace("{accountId:int}", accountId.ToString());
        }
    }

    public static class Transaction
    {
        private const string Base = "transaction";

        private static class Actions
        {
            public const string TransactionsByAccountId = "/account/{accountId:int}";
            public const string TransactionById = "/{transactionId:int}";
            public const string CreateTransaction = "/create";
            public const string UpdateTransaction = "/update/{transactionId:int}";
            public const string DeleteTransaction = "/delete/{transactionId:int}";
            public const string CreateTransactionItem = "/{accountId:int}/items/create";
            public const string UpdateTransactionItem = "/items/update/{transactionItemId:int}";
            public const string DeleteTransactionItem = "/items/delete/{transactionItemId:int}";
        }

        public const string TransactionsByAccountId = Base + Actions.TransactionsByAccountId;
        public const string TransactionById = Base + Actions.TransactionById;
        public const string CreateTransaction = Base + Actions.CreateTransaction;
        public const string UpdateTransaction = Base + Actions.UpdateTransaction;
        public const string DeleteTransaction = Base + Actions.DeleteTransaction;

        public const string CreateTransactionItem = Base + Actions.CreateTransactionItem;
        public const string UpdateTransactionItem = Base + Actions.UpdateTransactionItem;
        public const string DeleteTransactionItem = Base + Actions.DeleteTransactionItem;
        public static class Generate
        {
            public static string TransactionsByAccountId(int accountId) => Transaction.TransactionsByAccountId.Replace("{accountId:int}", accountId.ToString());
            public static string TransactionById(int transactionId) => Transaction.TransactionById.Replace("{transactionId:int}", transactionId.ToString());
            public static string UpdateTransaction(int transactionId) => Transaction.UpdateTransaction.Replace("{transactionId:int}", transactionId.ToString());
            public static string DeleteTransaction(int transactionId) => Transaction.DeleteTransaction.Replace("{transactionId:int}", transactionId.ToString());

            public static string CreateTransactionItem(int accountId) => Transaction.CreateTransactionItem.Replace("{accountId:int}", accountId.ToString());
            public static string UpdateTransactionItem(int transactionItemId) => Transaction.UpdateTransactionItem.Replace("{transactionItemId:int}", transactionItemId.ToString());
            public static string DeleteTransactionItem(int transactionItemId) => Transaction.DeleteTransactionItem.Replace("{transactionItemId:int}", transactionItemId.ToString());
        }
    }

    public static class Report
    {
        private const string Base = "report";

        private static class Actions
        {
            public const string Summary = "/summary";
            public const string UserSummary = "/user/{userId:int}";
            public const string GroupSummary = "/group/{groupId:int}";
            public const string FinanceSummary = "/finance";
            public const string StorageSummary = "/storage";
        }

        public const string Summary = Base + Actions.Summary;
        public const string UserSummary = Base + Actions.UserSummary;
        public const string GroupSummary = Base + Actions.GroupSummary;
        public const string FinanceSummary = Base + Actions.FinanceSummary;
        public const string StorageSummary = Base + Actions.StorageSummary;
        public static class Generate
        {
            public static string UserSummary(int userId) => Report.UserSummary.Replace("{userId:int}", userId.ToString());
            public static string GroupSummary(int groupId) => Report.GroupSummary.Replace("{groupId:int}", groupId.ToString());
        }
    }

    public static class Image
    {
        private const string Base = "image";

        private static class Actions
        {
            public const string Upload = "/upload";
            public const string ImageById = "/{imageId:int}";
            public const string DeleteImage = "/delete/{imageId:int}";
        }

        public const string Upload = Base + Actions.Upload;
        public const string ImageById = Base + Actions.ImageById;
        public const string DeleteImage = Base + Actions.DeleteImage;
        public static class Generate
        {
            public static string ImageById(int imageId) => Image.ImageById.Replace("{imageId:int}", imageId.ToString());
            public static string DeleteImage(int imageId) => Image.DeleteImage.Replace("{imageId:int}", imageId.ToString());
        }
    }
}