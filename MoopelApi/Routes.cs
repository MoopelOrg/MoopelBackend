namespace MoopelApi;

public static class Routes
{
    public static class User
    {
        public const string Base = "user";

        public static class Actions
        {
            public const string Login = "/login";
            public const string Logout = "/logout";
            public const string UserById = "/{userid:int}";
            public const string Me = "/me";
        }

        public const string Login = Base + Actions.Login;
        public const string Logout = Base + Actions.Logout;
        public const string UserById = Base + Actions.UserById;
        public const string Me = Base + Actions.Me;
    }
}
