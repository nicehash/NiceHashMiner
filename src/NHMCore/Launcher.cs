namespace NHMCore
{
    public static class Launcher
    {
        public static bool IsLauncher { get; private set; } = false;
        public static bool IsUpdated { get; private set; } = false;
        public static bool IsUpdatedFailed { get; private set; } = false;

        public static void SetIsLauncher(bool isLauncher)
        {
            IsLauncher = isLauncher;
        }

        public static void SetIsUpdated(bool isUpdated)
        {
            IsUpdated = isUpdated;
            if (IsUpdated)
            {
                Notifications.AvailableNotifications.CreateNhmWasUpdatedInfo(true);
            }
        }

        public static void SetIsUpdatedFailed(bool isUpdatedFailed)
        {
            IsUpdatedFailed = isUpdatedFailed;
            if (IsUpdatedFailed)
            {
                Notifications.AvailableNotifications.CreateNhmWasUpdatedInfo(false);
            }
        }
    }
}
