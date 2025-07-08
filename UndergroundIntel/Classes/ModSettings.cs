using IVSDKDotNet;
using IVSDKDotNet.Attributes;

namespace UndergroundIntel.Classes
{
    [ShowStaticFieldsInInspector]
    internal class ModSettings
    {

        #region Variables

        // Pickups
        public static bool IntelAlwaysUnlocked;

        // Fees
        public static int PickupIntelFee;

        #endregion

        public static void Load(SettingsFile settings)
        {
            // Pickups
            IntelAlwaysUnlocked = settings.GetBoolean("Pickups", "AlwaysUnlocked", false);

            // Fees
            PickupIntelFee = settings.GetInteger("Fees", "PickupIntelFee", 100_000);
        }

    }
}
