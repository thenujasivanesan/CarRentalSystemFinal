namespace CarRentalSystem.Helpers
{
    public static class SessionHelper
    {
        public const string UserIdKey = "UserId";
        public const string UserRoleKey = "UserRole";
        public const string UsernameKey = "Username";

        public static void SetUserSession(ISession session, int userId, string role, string username)
        {
            session.SetInt32(UserIdKey, userId);
            session.SetString(UserRoleKey, role);
            session.SetString(UsernameKey, username);
        }

        public static int? GetUserId(ISession session)
        {
            return session.GetInt32(UserIdKey);
        }

        public static string? GetUserRole(ISession session)
        {
            return session.GetString(UserRoleKey);
        }

        public static string? GetUsername(ISession session)
        {
            return session.GetString(UsernameKey);
        }

        public static bool IsLoggedIn(ISession session)
        {
            return session.GetInt32(UserIdKey).HasValue;
        }

        public static bool IsAdmin(ISession session)
        {
            return GetUserRole(session) == "Admin";
        }

        public static bool IsCustomer(ISession session)
        {
            return GetUserRole(session) == "Customer";
        }

        public static void ClearSession(ISession session)
        {
            session.Clear();
        }
    }
}
