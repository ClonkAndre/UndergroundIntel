using System;
using System.Collections.Generic;
using System.Drawing;
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

        public readonly static Dictionary<string, Island> ZoneToIslandDict = new Dictionary<string, Island>()
        {
            // Alderney
            { "WESDY", Island.Alderney },
            { "LEFWO", Island.Alderney },
            { "ALDCI", Island.Alderney },
            { "BERCH", Island.Alderney },
            { "NORMY", Island.Alderney },
            { "ACTRR", Island.Alderney },
            { "PORTU", Island.Alderney },
            { "TUDOR", Island.Alderney },
            { "ACTIP", Island.Alderney },
            { "ALSCF", Island.Alderney },

            // Algonquin
            { "NORWO", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "EAHOL", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "NOHOL", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "VASIH", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "LANCA", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "MIDPE", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "MIDPA", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "MIDPW", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "PUGAT", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "HATGA", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "LANCE", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "STARJ", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "WESMI", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "TMEQU", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "THTRI", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "EASON", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "THPRES", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "FISSN", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "FISSO", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "LOWEA", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "LITAL", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "SUFFO", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "CASGC", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "CITH" , Island.Algonquin_ColonyIsland_HappinessIsland },
            { "CHITO", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "THXCH", Island.Algonquin_ColonyIsland_HappinessIsland },
            { "CASGR", Island.Algonquin_ColonyIsland_HappinessIsland },

            // Bohan
            { "BOULE", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "NRTGA", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "LTBAY", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "FORSI", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "INSTI", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "STHBO", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "CHAPO", Island.Dukes_Broker_Bohan_ChargeIsland },

            // Dukes
            { "STEIN", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "MEADP", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "FRANI", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "WILLI", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "MEADH", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "EISLC", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "BOAB" , Island.Dukes_Broker_Bohan_ChargeIsland },
            { "CERHE", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "BEECW", Island.Dukes_Broker_Bohan_ChargeIsland },

            // Broker
            { "SCHOL", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "DOWTW", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "ROTTH", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "ESHOO", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "OUTL",  Island.Dukes_Broker_Bohan_ChargeIsland },
            { "SUTHS", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "HOBEH", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "FIREP", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "FIISL", Island.Dukes_Broker_Bohan_ChargeIsland },
            { "BEGGA", Island.Dukes_Broker_Bohan_ChargeIsland },

            // Happiness Island
            { "HAPIN", Island.Algonquin_ColonyIsland_HappinessIsland },

            // Charge Island
            { "CHISL", Island.Dukes_Broker_Bohan_ChargeIsland },

            // Colony Island
            { "COISL", Island.Algonquin_ColonyIsland_HappinessIsland },

            // Bridges, tunnels etc TODO
            { "BRALG", Island.LibertyCity },
            { "BRBRO", Island.LibertyCity },
            { "BREBB", Island.LibertyCity },
            { "BRDBB", Island.LibertyCity },
            { "NOWOB", Island.LibertyCity },
            { "HIBRG", Island.LibertyCity },
            { "LEAPE", Island.LibertyCity },
            { "BOTU", Island.LibertyCity },

            // Liberty City
            { "LIBERTY", Island.LibertyCity }
        };

        //internal static bool TryFindRandomPickupAroundPositionOnIsland(Vector3 position, float range, Island targetIsland, out IVPickup? foundPickup)
        //{
        //    // Filter out invalid pickups
        //    IVPickup[] pickups = IVPickups.Pickups.Where(x =>
        //    {

        //        // Ignore "non-initialized" pickups
        //        if (x.Position == Vector3.Zero)
        //            return false;

        //        // Pickups have to be outside
        //        if (x.RoomKey != 0)
        //            return false;

        //        // Ignore certain types
        //        switch ((ePickupType)x.Type)
        //        {
        //            case ePickupType.PICKUP_TYPE_PIGEON:
        //            case ePickupType.PICKUP_TYPE_MONEY:
        //            case ePickupType.PICKUP_TYPE_MONEY2:
        //                return false;
        //        }

        //        // Ignore certain model indexes in certain episodes
        //        switch (NativeGame.CurrentEpisode)
        //        {
        //            case Episode.TBoGT:

        //                if (x.ModelIndex == 1262 /* Parachute */)
        //                    return false;

        //                break;
        //        }

        //        // Check distance
        //        if (Vector3.Distance(x.Position, position) > range)
        //            return false;

        //        // Get current island pickup is on
        //        ZoneToIslandDict.TryGetValue(GET_NAME_OF_ZONE(x.Position), out Island currentIslandPickupIsOn);

        //        // Ignore pickups that are not on the target island
        //        if (currentIslandPickupIsOn != targetIsland)
        //            return false;

        //        // This checks if there is enough space above the pickup so the camera does not clip into a ceiling or something
        //        if (IVWorld.ProcessLineOfSight(x.Position, x.Position + new Vector3(0f, 0f, 20f), out IVLineOfSightResults results, 1))
        //            return false;

        //        return true;

        //    }).ToArray();

        //    // No pickups found...
        //    if (pickups.Length == 0)
        //    {
        //        foundPickup = null;
        //        return false;
        //    }

        //    // Return random pickup
        //    foundPickup = pickups[GENERATE_RANDOM_INT_IN_RANGE(0, pickups.Length)];
        //    return true;
        //}
        internal static IVPickup FindClosestPickup(Vector3 fromPos)
        {
            return IVPickups.Pickups.OrderBy(x =>
            {

                // Ignore "non-initialized" pickups
                if (x.Position == Vector3.Zero)
                    return float.MaxValue;

                // Pickups have to be outside
                if (x.RoomKey != 0)
                    return float.MaxValue;

                // Ignore certain types
                switch ((ePickupType)x.Type)
                {
                    case ePickupType.PICKUP_TYPE_PIGEON:
                    case ePickupType.PICKUP_TYPE_MONEY:
                    case ePickupType.PICKUP_TYPE_MONEY2:
                        return float.MaxValue;
                }

                // Ignore certain model indexes in certain episodes
                switch (NativeGame.CurrentEpisode)
                {
                    case Episode.TBoGT:

                        if (x.ModelIndex == 1262 /* Parachute */)
                            return float.MaxValue;

                        break;
                }

                // This checks if there is enough space above the pickup so the camera does not clip into a ceiling or something
                if (IVWorld.ProcessLineOfSight(x.Position, x.Position + new Vector3(0f, 0f, 10f), out IVLineOfSightResults results, 2))
                    return float.MaxValue;

                // Finally, sort by distance.
                return Vector3.Distance(x.Position, fromPos);

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

        internal static string RemoveCommentsFromJsonString(string str)
        {
            return string.Join(Environment.NewLine,
                    str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                    .Where(line => !line.TrimStart().StartsWith("//") && !line.TrimStart().StartsWith("#")));
        }

        internal static float GetAspectRatio()
        {
            GET_SCREEN_RESOLUTION(out var pX, out var pY);
            return pX / pY;
        }

        internal static void DisplayTextAtScreenPosition(string text, Vector2 screenCoord, Color color, float size = 10f)
        {
            if (screenCoord == Vector2.Zero)
                return;

            GET_SCREEN_RESOLUTION(out Vector2 res);
            Vector2 finalPos = screenCoord / res;

            float w = size * 0.02f * GetAspectRatio();
            SET_TEXT_BACKGROUND(value: false);
            SET_TEXT_FONT(0u);
            SET_TEXT_EDGE(displayEdge: true, 0u, 0u, 0u, 255u);
            SET_TEXT_CENTRE(false);
            SET_TEXT_DROPSHADOW(displayShadow: false, 0u, 0u, 0u, 0u);
            SET_TEXT_PROPORTIONAL(value: true);
            SET_TEXT_COLOUR(color.R, color.G, color.B, color.A);
            SET_TEXT_SCALE(w, size * 0.03f);
            SET_TEXT_EDGE(displayEdge: true, 0u, 0u, 0u, 255u);
            SET_TEXT_USE_UNDERSCORE(value: true);
            DISPLAY_TEXT_WITH_LITERAL_STRING(finalPos.X, finalPos.Y, "STRING", text);
        }
        internal static void DisplayListOfTextAtWorldPosition(Vector3 pos, Color color, float size, params string[] strings)
        {
            // World pos to screen pos
            Vector2 screenPos = NativeDrawing.CoordToScreen(pos);

            if (screenPos == Vector2.Zero)
                return;

            for (int i = 0; i < strings.Length; i++)
            {
                DisplayTextAtScreenPosition(strings[i], screenPos + new Vector2(0f, i * 15.5f), color, size);
            }
        }

    }
}
