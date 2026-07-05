using System;

namespace MadnessCubedOffline
{
    public static class CoreEvents
    {
        public static Action<string, int> OnItemPurchased;
        public static Action<string> OnPurchaseFailed;
        public static Action<int, int> OnBalanceChanged;

        public static Action<string> OnMissionCompleted;
        public static Action OnMissionStarted;

        public static Action<int, int> OnWeaponEquipped;

        public static Action OnProfileLoaded;
        public static Action OnProfileSaved;
        public static Action OnInventoryChanged;
    }

    public static class CoreManager
    {
        public static bool IsInitialized { get; private set; }
        public static OfflineServer Server { get; private set; }
        public static LocalPlayerData PlayerData { get; private set; }
        public static OfflineConfig Config { get; private set; }

        public static void Initialize(OfflineServer server, LocalPlayerData data, OfflineConfig config)
        {
            Server = server;
            PlayerData = data;
            Config = config;
            IsInitialized = true;
        }
    }
}
