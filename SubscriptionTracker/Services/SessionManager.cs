using SubscriptionTracker.Models;

namespace SubscriptionTracker.Services
{
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;
    }
}
