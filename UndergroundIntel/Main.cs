using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using CCL.GTAIV;

using UndergroundIntel.Classes;
using UndergroundIntel.Classes.Cutscenes;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

namespace UndergroundIntel
{
    public class Main : Script
    {

        #region Variables
        internal static Main Instance;

        // Lists
        private readonly Dictionary<string, Island> zoneToIslandDict = new Dictionary<string, Island>()
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
        private readonly List<int> ignoredPickupIndexes = new List<int>()
        {
            28
        };
        private readonly List<short> modelIndexesToIgnore = new List<short>()
        {
            1262, // TBOGT Parachute
        };
        private readonly List<uint> roomKeysToIgnore = new List<uint>() // Mostly savehouses
        {
            // IV
            1113953995,
            1828622139,

            // TLAD
            2559376887,
            3146055151,
            3733736628,

            // TBOGT
            3728725503,
        };
        private List<int> pickupsWithCustomTempBlip;
        private List<uint> allowedRooms;

#if DEBUG
        public bool ShowModDebug;
        public bool ShowPickupDebug;
        public bool HidePigeonPickups;
        public bool HideInvalidPickups;
        public float ShowPickupDebugStuffAtDistance = 50f;
#endif

        // Pickup stuff
        public float ShowPickupsInRange = 500000000f;

        // Bouncer stuff
        private int currentBouncerHandle;
        private NativeBlip currentBouncerBlip;
        private bool noMessageSound;

        // Player stuff
        private int playerPedHandle;
        private Vector3 playerCoords;
        private Island currentIslandPlayerIsOn;

        // Other
        private bool isUsingController;
        private bool forcePickupsToShow;

        #endregion

        #region Constructor
        public Main()
        {
            Instance = this;

            // Lists
            pickupsWithCustomTempBlip = new List<int>(16);

            // IV-SDK .NET stuff
            Initialized += Main_Initialized;
            Uninitialize += Main_Uninitialize;
#if DEBUG
            OnImGuiRendering += Main_OnImGuiRendering;
#endif
            Tick += Main_Tick;
        }
        #endregion

        #region Methods
        private void RemoveAllCustomTempBlips()
        {
            if (pickupsWithCustomTempBlip.Count == 0)
                return;

            // Get all pickup slots
            IVPickup[] pickups = IVPickups.Pickups;

            for (int i = 0; i < pickupsWithCustomTempBlip.Count; i++)
            {
                i = RemoveCustomBlipFromList(i, pickups[pickupsWithCustomTempBlip[i]]);
            }
        }
        private void CanTempBlipsStillExist(IVPickup[] pickups)
        {
            if (IS_PAUSE_MENU_ACTIVE())
                return;

            for (int i = 0; i < pickupsWithCustomTempBlip.Count; i++)
            {
                IVPickup pickup = pickups[pickupsWithCustomTempBlip[i]];

                // Check if player is still within the range of the scan for pickups check
                if (Vector3.Distance(playerCoords, pickup.Position) > ShowPickupsInRange)
                {
                    // Remove custom blip if not
                    i = RemoveCustomBlipFromList(i, pickup);
                    continue;
                }

                // Check if pickup has a valid type
                if (pickup.Type == 0)
                {
                    // Remove custom blip if not
                    i = RemoveCustomBlipFromList(i, pickup);
                    continue;
                }

                // Check if pickup still has a blip
                if (pickup.Blip == -1)
                {
                    // Remove custom blip if not
                    i = RemoveCustomBlipFromList(i, pickup);
                    continue;
                }

                // Check if pickup still has a world object
                if (pickup.WorldObject == UIntPtr.Zero && !forcePickupsToShow)
                {
                    // Remove custom blip if pickup has no world object
                    i = RemoveCustomBlipFromList(i, pickup);
                    continue;
                }

                //// Check conditions
                //i = CheckConditions(i, pickup);

                if (!ModSettings.IntelAlwaysUnlocked)
                    i = RemoveCustomBlipFromList(i, pickup);
            }
        }

        private void SearchAndAddTemporaryPickupBlips(IVPickup[] pickups)
        {
            for (int i = 0; i < pickups.Length; i++)
            {
                IVPickup pickup = pickups[i];

                if (pickup.Position == Vector3.Zero)
                    continue;


                // Check if pickup type is allowed
                switch (pickup.Type)
                {
                    // All that are allowed
                    case 2:     // Regular pickups (Like health/armor pickups)
                    case 15:    // Weapons
                        break;

                    // All that are NOT allowed
                    default:
                        continue;
                }

                //if (pickup.Type == 0 || (pickup.Type != 2 && pickup.Type != 15)) // NONE, Regular, Weapons. Maybe make it so it checks the pickup model instead of the type?
                //    continue;

                // Check if pickup already has a blip
                if (pickup.Blip != -1)
                    continue;

                // Check if this pickup index is not within the list of indexes to ignore
                if (ignoredPickupIndexes.Contains(i))
                    continue;

                // Check if this pickup room key is not within the list of room keys to ignore
                if (roomKeysToIgnore.Contains(pickup.RoomKey))
                    continue;

                // Check if this pickup model index is not within the list of model indexes to ignore
                if (modelIndexesToIgnore.Contains(pickup.ModelIndex))
                    continue;

                // Skip some checks if pause menu is active
                if (!IS_PAUSE_MENU_ACTIVE())
                {
                    // Check if pickup has a world object assigned to it
                    if (pickup.WorldObject == UIntPtr.Zero)
                        continue;
                }

                // Check actual distance - If greater then the search distance then skip adding the temp blip to the pickup
                if (Vector3.Distance(playerCoords, pickup.Position) > ShowPickupsInRange)
                    continue;

                // Get current island pickup is on
                zoneToIslandDict.TryGetValue(GET_NAME_OF_ZONE(pickup.Position), out Island currentIslandPickupIsOn);

                // Check if intel for pickups on the current island was unlocked
                if (!Utils.WasIntelBoughtForIsland(currentIslandPickupIsOn) && !ModSettings.IntelAlwaysUnlocked)
                    continue;

                // Check conditions
                //if (!AlwaysShowPickups && !forcePickupsToShow)
                //{
                //    bool didWantedConditionMet = ModSettings.OnlyShowPickupsWhenWanted && wantedLevel != 0;

                //    // Can add more conditions here to this check
                //    bool didAnyConditionMet = didWantedConditionMet;

                //    // If the pickups are not set to always show, and no condition met, then never shop pickups on the radar
                //    if (!didAnyConditionMet)
                //        continue;
                //}

                // Create temp blip
                pickup.Blip = IVPickups.CreateTemporaryRadarBlipForPickup(pickup.Position, i);

                // Add to list of custom temp blips
                pickupsWithCustomTempBlip.Add(i);
            }
        }

        private void GetRidOfCurrentBouncerBlip()
        {
            if (currentBouncerBlip == null)
                return;

            currentBouncerBlip.Delete();
            currentBouncerBlip = null;
        }
        private void CreateCurrentBouncerBlip()
        {
            if (currentBouncerBlip != null)
                return;

#if !DEBUG
            // If intel was already bought for current island then dont add blip to bouncer
            if (Utils.WasIntelBoughtForIsland(currentIslandPlayerIsOn))
                return;
#endif

            // Create blip
            currentBouncerBlip = NativeBlip.AddBlip(currentBouncerHandle);
            currentBouncerBlip.Color = eBlipColor.BLIP_COLOR_YELLOW;
            currentBouncerBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_ONLY;
        }
        private void HandleBouncerInteraction()
        {
            if (BouncerPickupIntelCutscene.IsCutsceneActive)
                return;

            // Check the current interior
            GET_KEY_FOR_CHAR_IN_ROOM(playerPedHandle, out uint currentRoomKey);

            if (!allowedRooms.Contains(currentRoomKey))
            {
                GetRidOfCurrentBouncerBlip();
                noMessageSound = false;
                return;
            }

            // Find bouncer within current interior
            currentBouncerHandle = FindBouncerWithinRoom();

            if (currentBouncerHandle == 0)
            {
                GetRidOfCurrentBouncerBlip();
                noMessageSound = false;
                return;
            }

            // Check if intel was already bought for current island
#if !DEBUG
            if (Utils.WasIntelBoughtForIsland(currentIslandPlayerIsOn))
            {
                noMessageSound = false;
                return;
            }
#endif

            CreateCurrentBouncerBlip();

            // Check distance to bouncer
            GET_CHAR_COORDINATES(currentBouncerHandle, out Vector3 bouncerPedCoords);

            if (Vector3.Distance(playerCoords, bouncerPedCoords) > 1f)
            {
                noMessageSound = false;
                return;
            }

            // Check if bouncer is facing the player for the interaction
            if (!IS_CHAR_FACING_CHAR(currentBouncerHandle, playerPedHandle, 25f))
            {
                noMessageSound = false;
                return;
            }

            string message = null;

            // Check money
#if !DEBUG
            STORE_SCORE(CONVERT_INT_TO_PLAYERINDEX(GET_PLAYER_ID()), out uint score);
            if (score < ModSettings.PickupIntelFee)
            {
                // Show message to player
                message = "You currently dont have enough money to buy intel about available pickups.";
                NativeGame.DisplayCustomHelpMessage(message, noMessageSound);
                noMessageSound = true;
                return;
            }
#endif

            // Show message to player
            string amount = ModSettings.PickupIntelFee.ToString("$#,0", System.Globalization.CultureInfo.InvariantCulture);
            message = string.Format("Press {0} to unlock intel about weapon and health pickups in {1} for {2}.", isUsingController ? "~INPUT_FRONTEND_ACCEPT~" : "~INPUT_PICKUP~", Utils.ToNiceLookingShortIslandName(currentIslandPlayerIsOn), amount);
            NativeGame.DisplayCustomHelpMessage(message, noMessageSound);
            noMessageSound = true;

            // Bouncer should look at the player so the player knows they can interact with the bouncer
            _TASK_LOOK_AT_CHAR(currentBouncerHandle, playerPedHandle, 1000, 0);

            // Check if accept key it pressed
            IVPad pad = IVPad.GetPad();
            bool pressedAcceptKey = false;

            if (isUsingController)
                pressedAcceptKey = pad.Values[(int)ePadControls.INPUT_FRONTEND_ACCEPT].CurrentValue == 255;
            else
                pressedAcceptKey = pad.Values[(int)ePadControls.INPUT_PICKUP].CurrentValue == 255;

            if (!pressedAcceptKey)
                return;

            // Signal cutscene to start now
            BouncerPickupIntelCutscene.Start();
        }
        #endregion

        #region Functions
        private bool RemoveBlipFromPickup(IVPickup pickup)
        {
            int blipHandle = pickup.Blip;

            if (blipHandle == -1)
                return false;

            if (!DOES_BLIP_EXIST(blipHandle))
            {
                pickup.Blip = -1;
                return true;
            }

            REMOVE_BLIP(blipHandle);
            pickup.Blip = -1;
            return true;
        }
        private int RemoveCustomBlipFromList(int atIndex, IVPickup pickup)
        {
            // Remove blip from pickup
            RemoveBlipFromPickup(pickup);

            // Remove blip from list
            pickupsWithCustomTempBlip.RemoveAt(atIndex);
            return atIndex - 1;
        }

        //private int CheckConditions(int i, IVPickup pickup)
        //{
        //    // Skip checking conditions if pickups are currently forced to shop up
        //    if (forcePickupsToShow)
        //        return i;

        //    // Check conditions
        //    bool didWantedConditionMet = ModSettings.OnlyShowPickupsWhenWanted && wantedLevel == 0;

        //    // Can add more conditions here to this check
        //    bool didAnyConditionMet = didWantedConditionMet;

        //    // Remove blip as no condition met
        //    if (!AlwaysShowPickups || didAnyConditionMet)
        //        return RemoveCustomBlipFromList(i, pickup);

        //    return i;
        //}

        private int FindBouncerWithinRoom()
        {
            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr == UIntPtr.Zero)
                    continue;
                if (ptr == IVPlayerInfo.FindThePlayerPed())
                    continue;

                int pedHandle = (int)pedPool.GetIndex(ptr);

                if (IS_CHAR_DEAD(pedHandle) || IS_PED_IN_COMBAT(pedHandle))
                    continue;

                GET_KEY_FOR_CHAR_IN_ROOM(pedHandle, out uint currentRoomKey);

                if (!allowedRooms.Contains(currentRoomKey))
                    continue;

                if (!IS_CHAR_USING_SCENARIO(pedHandle, "SCENARIO_STANDING"))
                    continue;

                return pedHandle;

                //GET_CHAR_MODEL(pedHandle, out uint model);

                //if (model == RAGE.AtStringHash("M_Y_BOUNCER_01")
                //    || model == RAGE.AtStringHash("M_Y_GTRI_LO_01"))
                //    return pedHandle;
            }

            return 0;
        }
        #endregion

        private void Main_Uninitialize(object sender, EventArgs e)
        {
            if (!CLR.CLRBridge.IsShuttingDown)
                RemoveAllCustomTempBlips();

            GetRidOfCurrentBouncerBlip();

            if (pickupsWithCustomTempBlip != null)
            {
                pickupsWithCustomTempBlip.Clear();
                pickupsWithCustomTempBlip = null;
            }
            if (allowedRooms != null)
            {
                allowedRooms.Clear();
                allowedRooms = null;
            }
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            ModSettings.Load(Settings);
        }

#if DEBUG
        private void Main_OnImGuiRendering(IntPtr devicePtr, ImGuiIV_DrawingContext ctx)
        {
            if (ShowModDebug)
            {
                ImGuiIV.Begin("Underground Intel Debug", ref ShowModDebug, eImGuiWindowFlags.None, eImGuiWindowFlagsEx.NoMouseEnable);

                ImGuiIV.TextUnformatted("CurrentIslandPlayerIsOn: {0}", currentIslandPlayerIsOn);
                ImGuiIV.TextUnformatted("WasIntelBoughtForCurrentIsland: {0}", Utils.WasIntelBoughtForIsland(currentIslandPlayerIsOn));
                ImGuiIV.TextUnformatted("Global Variables Value For Current Island: {0}", IVTheScripts.GetGlobalInteger(Utils.GetGlobalVariablesIndexForIslandBasedOnEpisode(currentIslandPlayerIsOn)));

                ImGuiIV.End();
            }

            if (!ShowPickupDebug)
                return;

            IVPickup[] arr = IVPickups.Pickups;

            for (int i = 0; i < arr.Length; i++)
            {
                IVPickup pickup = arr[i];

                if (pickup.Position == Vector3.Zero)
                    continue;

                if (HidePigeonPickups && pickup.Type == 3)
                    continue;

                if (HideInvalidPickups)
                {
                    // Check if this pickup room key is not within the list of room keys to ignore
                    if (roomKeysToIgnore.Contains(pickup.RoomKey))
                        continue;

                    // Check if this pickup model index is not within the list of model indexes to ignore
                    if (modelIndexesToIgnore.Contains(pickup.ModelIndex))
                        continue;
                }

                // Check distance
                if (Vector3.Distance(playerCoords, pickup.Position) > ShowPickupDebugStuffAtDistance)
                    continue;

                Vector2 screenPos = NativeDrawing.CoordToScreen(pickup.Position);

                if (screenPos == Vector2.Zero)
                    continue;

                ImGuiIV.SetNextWindowPos(screenPos);
                if (ImGuiIV.Begin(string.Format("ADVANCEDPICKUPBLIPS_DBG##{0}", i), eImGuiWindowFlags.NoDecoration | eImGuiWindowFlags.NoInputs | eImGuiWindowFlags.AlwaysAutoResize, eImGuiWindowFlagsEx.NoMouseEnable))
                {
                    int handle = IVPickups.ConvertIndexToHandle(i);

                    ImGuiIV.TextColored(Color.Yellow, "Index: {0}", i);
                    ImGuiIV.TextColored(Color.Yellow, "Handle: {0}", handle);
                    ImGuiIV.TextColored(Color.Yellow, "Memory Address: {0}", pickup.GetUIntPtr().ToUInt32().ToString("X"));

                    ImGuiIV.TextUnformatted("field_0: {0}", pickup.field_0);
                    ImGuiIV.TextUnformatted("WorldObject: {0}", pickup.WorldObject);
                    ImGuiIV.TextUnformatted("field_8: {0}", pickup.field_8);
                    ImGuiIV.TextUnformatted("RoomKey: {0}", pickup.RoomKey);
                    ImGuiIV.TextUnformatted("Blip: {0}", pickup.Blip);
                    ImGuiIV.TextUnformatted("LastPickedUpTime: {0}", pickup.LastPickedUpTime);
                    ImGuiIV.TextUnformatted("Position: {0}", pickup.Position);
                    ImGuiIV.TextUnformatted("ModelIndex: {0}", pickup.ModelIndex);
                    ImGuiIV.TextUnformatted("field_42: {0}", pickup.field_42);
                    ImGuiIV.TextUnformatted("Type: {0} ({1})", pickup.Type, (ePickupType)pickup.Type);

                    ImGuiIV.End();
                }
            }
        }
#endif

        private void Main_Tick(object sender, EventArgs e)
        {
            if (allowedRooms == null)
            {
                allowedRooms = new List<uint>()
                {
                    GET_HASH_KEY_2("GtaMloRoom01"),     // For Bohan and Alderney
                    GET_HASH_KEY_2("Room_gunlobby"),    // For Bohan and Alderney
                    GET_HASH_KEY_2("Room_GunChina")     // For Algonquin
                };
            }

            // Get stuff
            playerPedHandle = NativeGame.GetPlayerPedHandle();
            GET_CHAR_COORDINATES(playerPedHandle, out playerCoords);
            isUsingController = IS_USING_CONTROLLER();

            // Get current island player is on
            zoneToIslandDict.TryGetValue(GET_NAME_OF_ZONE(playerCoords), out currentIslandPlayerIsOn);

            // Loop through all pickup slots
            IVPickup[] pickups = IVPickups.Pickups;

            // Goes through the list of all custom temporary blips for pickups and checks if they can still exist
            CanTempBlipsStillExist(pickups);

            // Add temporary pickup blips
            SearchAndAddTemporaryPickupBlips(pickups);

            // Handle bouncer stuff
            HandleBouncerInteraction();
            BouncerPickupIntelCutscene.HandleBouncerCutscene(playerPedHandle, currentBouncerHandle, currentIslandPlayerIsOn);
        }

    }
}
