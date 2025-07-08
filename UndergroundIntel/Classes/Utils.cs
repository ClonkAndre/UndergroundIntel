using System;
using System.Linq;
using System.Numerics;

using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

namespace UndergroundIntel.Classes
{
    internal static class Utils
    {

        internal static string ToNiceLookingShortIslandName(Island island)
        {
            switch (island)
            {
                case Island.Alderney: return "Alderney";
                case Island.Algonquin_ColonyIsland_HappinessIsland: return "Algonquin";
                case Island.Dukes_Broker_Bohan_ChargeIsland: return "Broker, Dukes, and Bohan";
                case Island.LibertyCity: return "Liberty City";
            }

            return island.ToString();
        }
        internal static string ToNiceLookingFullIslandName(Island island)
        {
            switch (island)
            {
                case Island.Alderney: return "Alderney";
                case Island.Algonquin_ColonyIsland_HappinessIsland: return "Algonquin, Colony Island and Happiness Island";
                case Island.Dukes_Broker_Bohan_ChargeIsland: return "Dukes, Broker, Bohan and Charge Island";
                case Island.LibertyCity: return "Liberty City";
            }

            return island.ToString();
        }

        internal static IVPickup FindClosestPickup(Vector3 fromPos)
        {
            return IVPickups.Pickups.OrderBy(obj =>
            {
                if (obj.Position == Vector3.Zero)
                    return 100_000_000f;
                if (obj.RoomKey != 0)
                    return 100_000_000f;

                // Ignore pigons
                if ((ePickupType)obj.Type == ePickupType.PICKUP_TYPE_PIGEON)
                    return 100_000_000f;

                // Check for collision
                if (IVWorld.ProcessLineOfSight(obj.Position, obj.Position + new Vector3(0f, 0f, 10f), out IVLineOfSightResults results, 2))
                {
                    return 100_000_000f;
                }

                return Vector3.Distance(obj.Position, fromPos);
            }).FirstOrDefault();
        }

        internal static void SetIntelAsBoughtForIsland(Island island)
        {
            IVTheScripts.SetGlobal(GetGlobalVariablesIndexForIslandBasedOnEpisode(island), 1);
        }
        internal static bool WasIntelBoughtForIsland(Island island)
        {
            return IVTheScripts.GetGlobalInteger(GetGlobalVariablesIndexForIslandBasedOnEpisode(island)) == 1;
        }

        internal static int GetGlobalVariablesIndexForIslandBasedOnEpisode(Island island)
        {
            switch (NativeGame.CurrentEpisode)
            {
                case Episode.IV:

                    switch (island)
                    {
                        case Island.Alderney: return 57_510;
                        case Island.Algonquin_ColonyIsland_HappinessIsland: return 57_511;
                        case Island.Dukes_Broker_Bohan_ChargeIsland: return 57_512;
                        case Island.LibertyCity: return 57_516;
                    }

                    break;
                case Episode.TLaD:
                case Episode.TBoGT:

                    switch (island)
                    {
                        case Island.Alderney: return 28_978;
                        case Island.Algonquin_ColonyIsland_HappinessIsland: return 28_979;
                        case Island.Dukes_Broker_Bohan_ChargeIsland: return 28_980;
                        case Island.LibertyCity: return 28_981;
                    }

                    break;
            }

            return 0;
        }

        internal static bool DoAndCheckFadeScreenOut(uint time)
        {
            if (!IS_SCREEN_FADED_OUT() && !IS_SCREEN_FADING_OUT())
            {
                DO_SCREEN_FADE_OUT(time);
            }

            return IS_SCREEN_FADED_OUT();
        }
        internal static bool DoAndCheckFadeScreenIn(uint time)
        {
            if (!IS_SCREEN_FADED_IN() && !IS_SCREEN_FADING_IN())
            {
                DO_SCREEN_FADE_IN(time);
            }

            return IS_SCREEN_FADED_IN();
        }

        internal static void PlayAnimation(int handle, string animSet, string animName, float speed, int unknown, AnimationFlags flags)
        {
            _TASK_PLAY_ANIM_WITH_FLAGS(handle, animName, animSet, speed, unknown, (int)flags);
        }

    }
}
