namespace MoopelApi;

public static class Routes
{
    public static class Auth
    {
        public const string Base = "auth";

        public static class Actions
        {
            public const string Login = "/login";
            public const string Logout = "/logout";
            public const string Register = "/register";
            public const string Me = "/me";
        }

        public const string Login = Base + Actions.Login;
        public const string Logout = Base + Actions.Logout;
        public const string Register = Base + Actions.Register;
        public const string Me = Base + Actions.Me;
    }

    public static class User
    {
        public const string Base = "user";
        public static class Actions
        {
            public const string UserById = "/{userid:int}";
            public const string CreateUser = "/create";
            public const string UpdateUser = "/update/{userId:int}";
            public const string DisableUser = "/disable/{userId:int}";
        }

        public const string UserById = Base + Actions.UserById;
        public const string CreateUser = Base + Actions.CreateUser;
        public const string UpdateUser = Base + Actions.UpdateUser;
        public const string DisableUser = Base + Actions.DisableUser;
    }

    public static class Group
    {
        public const string Base = "group";
        public static class Actions
        {
            public const string MyGroups = "/mygroups";
            public const string GroupById = "/{groupid:int}";
            public const string CreateGroup = "/create";
            public const string UpdateGroup = "/update/{groupid:int}";
            public const string DeleteGroup = "/delete/{groupid:int}";
        }
        public const string MyGroups = Base + Actions.MyGroups;
        public const string GroupById = Base + Actions.GroupById;
        public const string CreateGroup = Base + Actions.CreateGroup;
        public const string UpdateGroup = Base + Actions.UpdateGroup;
        public const string DeleteGroup = Base + Actions.DeleteGroup;
    }

    public static class Note
    {
        public const string Base = "note";
        public static class Actions
        {
            public const string NotesByGroupId = "/group/{groupid:int}";
            public const string NoteById = "/{noteid:int}";
            public const string CreateNote = "/create";
            public const string UpdateNote = "/update/{noteid:int}";
            public const string DeleteNote = "/delete/{noteid:int}";
        }

        public const string NotesByGroupId = Base + Actions.NotesByGroupId;
        public const string NoteById = Base + Actions.NoteById;
        public const string CreateNote = Base + Actions.CreateNote;
        public const string UpdateNote = Base + Actions.UpdateNote;
        public const string DeleteNote = Base + Actions.DeleteNote;
    }

    public static class Tag
    {
        public const string Base = "tag";
        public static class Actions
        {
            public const string TagsByGroupId = "/group/{groupid:int}";
            public const string TagById = "/{tagid:int}";
            public const string CreateTag = "/create";
            public const string UpdateTag = "/update/{tagid:int}";
            public const string DeleteTag = "/delete/{tagid:int}";
        }
        public const string TagsByGroupId = Base + Actions.TagsByGroupId;
        public const string TagById = Base + Actions.TagById;
        public const string CreateTag = Base + Actions.CreateTag;
        public const string UpdateTag = Base + Actions.UpdateTag;
        public const string DeleteTag = Base + Actions.DeleteTag;
    }

    public static class WorkItem
    {
        public const string Base = "workitem";

        public static class Actions
        {
            public const string WorkItemsByGroupId = "/group/{groupid:int}";
            public const string WorkItemById = "/{workitemid:int}";
            public const string CreateWorkItem = "/create";
            public const string UpdateWorkItem = "/update/{workitemid:int}";
            public const string DeleteWorkItem = "/delete/{workitemid:int}";
        }
        public const string WorkItemsByGroupId = Base + Actions.WorkItemsByGroupId;
        public const string WorkItemById = Base + Actions.WorkItemById;
        public const string CreateWorkItem = Base + Actions.CreateWorkItem;
        public const string UpdateWorkItem = Base + Actions.UpdateWorkItem;
        public const string DeleteWorkItem = Base + Actions.DeleteWorkItem;
    }
}
