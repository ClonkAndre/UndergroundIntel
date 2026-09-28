using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

using CCL.GTAIV;

using UndergroundIntel.Classes;
using UndergroundIntel.Classes.Cutscenes;
using UndergroundIntel.Classes.Json;

using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Hooking;
using static IVSDKDotNet.Native.Natives;

namespace UndergroundIntel
{
    public class Main : Script
    {

        #region Variables

        // Lists
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

#if DEBUG
        [Separator("Debugging stuff")]
        public bool ShowModDebugOverlay;
        public bool ShowPickupDebugOverlay;
        public float ShowPickupDebugStuffAtDistance = 50f;
        public bool HidePigeonPickups;
        public bool HideInvalidPickups;
        public bool DisableAddingTempBlipsCode;
        public bool DisableTempBlipRemovalChecks;
#endif

        // UI
#if DEBUG
        [Separator]
#endif
        public bool EditorOpened;
        private string promptKey, promptValue;

        // Pickup stuff
        private IVPickup[] pickups;

        // Dealer stuff
        private DealerSpot currentDealerSpot;
        private bool noMessageSound;

        [HelpMarker("Visualizes dealer positions, ranges and states. Helpful for debugging.")]
#if DEBUG
        public bool VisualizeDealerStuff = true;
#else
        public bool VisualizeDealerStuff = false;
#endif

        // Player stuff
        private int playerPedHandle;
        private Vector3 playerCoords;
        private Island currentIslandPlayerIsOn;

        // Other
        private bool isUsingController;

        #endregion

        #region Hooks

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool CPickups_readSave_Delegate();
        private CPickups_readSave_Delegate originalCPickupsReadSaveFunc;
        private CPickups_readSave_Delegate hookedCPickupsReadSaveFunc;
        private bool HookedCPickupsReadSaveFunc()
        {
            // Calls the original function of the game which will read all stored pickups from the save file
            bool result = originalCPickupsReadSaveFunc();

            Logging.LogDebug("HookedCPickupsReadSaveFunc called. Original func result: {0}", result);

            // Resets all blips from the read pickups
            IVPickup[] arr = IVPickups.Pickups;
            for (int i = 0; i < arr.Length; i++)
            {
                IVPickup pickup = arr[i];

                if (pickup.Blip != -1)
                    pickup.Blip = -1;
            }

            return result;
        }

        #endregion

        #region Constructor
        public Main()
        {
            // Lists
            pickupsWithCustomTempBlip = new List<int>(16);

            // IV-SDK .NET stuff
            Initialized += Main_Initialized;
            Uninitialize += Main_Uninitialize;
            OnImGuiRendering += Main_OnImGuiRendering;
            ProcessPad += Main_ProcessPad;
            Tick += Main_Tick;
            ProcessAutomobile += Main_ProcessAutomobile;
        }

        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
            IVPool vehPool = IVPools.GetVehiclePool();
            for (int i = 0; i < vehPool.Count; i++)
            {
                UIntPtr ptr = vehPool.Get(i);

                if (ptr == UIntPtr.Zero)
                    continue;

                unsafe
                {
                    // 0x14E8 - float m_fWaterCannonOrientation;
                    // 0x14EC - float m_fWaterCannonElevation;
                    // 0x1510 - Vector3 m_vecWaterCannonDirection;

                    //IVVehicle veh = IVVehicle.FromUIntPtr(ptr);

                    //veh.GetBoneMatrix2(41);

                    //if (veh.Driver != UIntPtr.Zero)
                    //{
                    //    *(float*)(ptr.ToUInt32() + 0x14E8) = 1f;
                    //    *(float*)(ptr.ToUInt32() + 0x14EC) = 1f;
                    //    *(uint*)(ptr.ToUInt32() + 0x14C4) |= 0x40;

                    //    *(uint*)(veh.Driver.ToUInt32() + 0x24) = 1;
                        
                    //}
                }
            }
        }
        #endregion

        #region Methods
        // Custom Pickup Blips
        private void RemoveAllCustomPickupBlips()
        {
            if (pickupsWithCustomTempBlip.Count == 0)
                return;

            for (int i = 0; i < pickupsWithCustomTempBlip.Count; i++)
            {
                i = RemoveCustomBlipFromList(i, pickups[pickupsWithCustomTempBlip[i]]);
            }
        }
        private void CanCustomPickupBlipsStillExists()
        {
            if (IS_PAUSE_MENU_ACTIVE())
                return;

            for (int i = 0; i < pickupsWithCustomTempBlip.Count; i++)
            {
                IVPickup pickup = pickups[pickupsWithCustomTempBlip[i]];

                // Check if pickup still has a blip
                if (pickup.Blip == -1)
                {
                    // Remove custom blip if not
                    i = RemoveCustomBlipFromList(i, pickup);
                    continue;
                }

                // Check if pickup still has a world object
                if (pickup.WorldObject == UIntPtr.Zero)
                {
                    // Remove custom blip if pickup has no world object
                    i = RemoveCustomBlipFromList(i, pickup);
                    continue;
                }

                // Check if pickups were actually unlocked for the current island they are on
                if (!ModSettings.IntelAlwaysUnlocked)
                {
                    // Get current island pickup is on
                    Utils.ZoneToIslandDict.TryGetValue(GET_NAME_OF_ZONE(pickup.Position), out Island currentIslandPickupIsOn);

                    // Check if intel for pickups on the current island was unlocked
                    if (!Utils.WasIntelBoughtForIsland(currentIslandPickupIsOn) && !ModSettings.IntelAlwaysUnlocked)
                    {
                        i = RemoveCustomBlipFromList(i, pickup);
                        continue;
                    }
                }
            }
        }
        private void AddCustomPickupBlips()
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

                // Check if pickups should always be unlocked
                if (!ModSettings.IntelAlwaysUnlocked)
                {
                    // Get current island pickup is on
                    Utils.ZoneToIslandDict.TryGetValue(GET_NAME_OF_ZONE(pickup.Position), out Island currentIslandPickupIsOn);

                    // Check if intel for pickups on the current island was unlocked
                    if (!Utils.WasIntelBoughtForIsland(currentIslandPickupIsOn) && !ModSettings.IntelAlwaysUnlocked)
                        continue;
                }

                // Create and assign temp blip
                pickup.Blip = IVPickups.CreateTemporaryRadarBlipForPickup(pickup.Position, i);

                // Add to list of custom temp blips
                pickupsWithCustomTempBlip.Add(i);
            }
        }

        // Dealer
        private void ResetCurrentDealer()
        {
            if (currentDealerSpot == null)
                return;

            currentDealerSpot.Reset();
            currentDealerSpot = null;
        }
        private void FindDealer()
        {
            // Prevent interaction if intel should always be unlocked, or if it was already unlock for the current island
#if !DEBUG
            if (ModSettings.IntelAlwaysUnlocked || Utils.WasIntelBoughtForIsland(currentIslandPlayerIsOn))
#else
            if (ModSettings.IntelAlwaysUnlocked)
#endif
            {
                ResetCurrentDealer();
                noMessageSound = false;
                return;
            }

            // Try find the closest dealer spot from the current player position
            DealerSpot foundDealerSpot = Core.FindClosestDealerSpotFromPosition(playerCoords);

            if (foundDealerSpot == null)
                return;

            // Try to find dealer NPC at expected position if no one was found yet for this spot
            foundDealerSpot.DealerHandle = FindDealerAtPosition(foundDealerSpot.Position);

            if (!foundDealerSpot.HasDealerHandle())
                return;

            // Set current dealer
            currentDealerSpot = foundDealerSpot;
        }
        private void HandleDealerInteraction()
        {
            if (!currentDealerSpot.HasDealerHandle())
                return;

            // Make blip appear so the player knows they can interact with the dealer
            currentDealerSpot.CreateBlip();

            // Check if player is within interaction distance
            GET_CHAR_COORDINATES(currentDealerSpot.DealerHandle, out Vector3 dealerCoords);

            // Visualize the range around the dealer which the player has to be in to be able to interact with the dealer
            if (VisualizeDealerStuff)
                DRAW_CHECKPOINT_WITH_ALPHA(dealerCoords, currentDealerSpot.InteractionDistance * 2f, Color.FromArgb(60, Color.Green));

            if (Vector3.Distance(playerCoords, dealerCoords) > currentDealerSpot.InteractionDistance)
            {
                currentDealerSpot.InteractionState = DealerInteractionState.NotWithinInteractionDistance;
                noMessageSound = false;
                return;
            }

            // Check if dealer is facing the player for the interaction if required
            if (currentDealerSpot.PlayerNeedsToBeInDealersVision)
            {
                if (!IS_CHAR_FACING_CHAR(currentDealerSpot.DealerHandle, playerPedHandle, currentDealerSpot.DealersFOV))
                {
                    currentDealerSpot.InteractionState = DealerInteractionState.NotWithinDealersVision;
                    noMessageSound = false;
                    return;
                }
            }

#if !DEBUG
            // Check money
            STORE_SCORE(CONVERT_INT_TO_PLAYERINDEX(GET_PLAYER_ID()), out uint score);
            if (score < ModSettings.PickupIntelFee)
            {
                currentDealerSpot.InteractionState = DealerInteractionState.NotEnoughMoney;

                // Show message to player
                NativeGame.DisplayCustomHelpMessage("You currently dont have enough money to buy intel about available pickups.", noMessageSound);
                noMessageSound = true;

                return;
            }
#endif

            // Show message to player
            if (Core.TryGetPrompt(currentDealerSpot.InteractionPromptKey, out string prompt))
            {
                currentDealerSpot.InteractionState = DealerInteractionState.Interactable;

                string amount = ModSettings.PickupIntelFee.ToString("$#,0", System.Globalization.CultureInfo.InvariantCulture);
                NativeGame.DisplayCustomHelpMessage(string.Format(prompt, isUsingController ? "~INPUT_FRONTEND_ACCEPT~" : "~INPUT_PICKUP~", amount), noMessageSound);
                noMessageSound = true;
            }
            else
            {
                currentDealerSpot.InteractionState = DealerInteractionState.InteractableButNoPromptWasSet;
            }

            // Dealer should look at the player for immersion
            _TASK_LOOK_AT_CHAR(currentDealerSpot.DealerHandle, playerPedHandle, 1000, 0);

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
            DealerPickupIntelCutscene.Start();
        }
        private void CheckCurrentDealer()
        {
            // Check if dealer even has a handle
            if (!currentDealerSpot.HasDealerHandle())
            {
                ResetCurrentDealer();
                return;
            }

            // Check if dealer is still valid
            if (!DOES_CHAR_EXIST(currentDealerSpot.DealerHandle))
            {
                ResetCurrentDealer();
                return;
            }

            // Check if dealer is dead
            if (IS_CHAR_DEAD(currentDealerSpot.DealerHandle))
            {
                ResetCurrentDealer();
                return;
            }

            // Check if dealer is in combat
            if (IS_PED_IN_COMBAT(currentDealerSpot.DealerHandle))
            {
                ResetCurrentDealer();
                return;
            }

            // Check if dealer left activation range
            GET_CHAR_COORDINATES(currentDealerSpot.DealerHandle, out Vector3 dealerCoords);
            if (Vector3.Distance(dealerCoords, currentDealerSpot.Position) > currentDealerSpot.ActivationDistance)
            {
                ResetCurrentDealer();
                return;
            }

            // Check if player left activation range
            if (Vector3.Distance(playerCoords, currentDealerSpot.Position) > currentDealerSpot.ActivationDistance)
            {
                ResetCurrentDealer();
                return;
            }
        }

        // Hooking
        private void CreateHooks()
        {
            uint address = 0;

            switch (MemoryAccess.GameVersion)
            {
                case eGameVersion.VERSION_1070:

                    address = MemoryAccess.BaseAddress + 0x534E30;

                    break;
                case eGameVersion.VERSION_1080:

                    address = MemoryAccess.BaseAddress + 0x589CB0;

                    break;

                default:
                    break;
            }

            hookedCPickupsReadSaveFunc = new CPickups_readSave_Delegate(HookedCPickupsReadSaveFunc);
            Logging.LogDebug("CreateHook Result: {0}", ManagedMinHook.CreateHook(address, hookedCPickupsReadSaveFunc, out originalCPickupsReadSaveFunc));
            Logging.LogDebug("EnableHook Result: {0}", ManagedMinHook.EnableHook(address));
        }
        private void DisableHooks()
        {
            uint address = 0;

            switch (MemoryAccess.GameVersion)
            {
                case eGameVersion.VERSION_1070:

                    address = MemoryAccess.BaseAddress + 0x534E30;

                    break;
                case eGameVersion.VERSION_1080:

                    address = MemoryAccess.BaseAddress + 0x589CB0;

                    break;

                default:
                    break;
            }

            Logging.LogDebug("DisableHook Result: {0}", ManagedMinHook.DisableHook(address));
        }

        // UI
#if DEBUG
        private void ModDebugOverlay()
        {
            if (!ShowModDebugOverlay)
                return;

            ImGuiIV.Begin("Underground Intel Debug", ref ShowModDebugOverlay, eImGuiWindowFlags.None, eImGuiWindowFlagsEx.NoMouseEnable);

            ImGuiIV.TextUnformatted("CurrentIslandPlayerIsOn: {0}", currentIslandPlayerIsOn);
            ImGuiIV.TextUnformatted("WasIntelBoughtForCurrentIsland: {0}", Utils.WasIntelBoughtForIsland(currentIslandPlayerIsOn));
            ImGuiIV.TextUnformatted("Global Variables Value For Current Island: {0}", IVTheScripts.GetGlobalInteger(Utils.GetGlobalVariablesIndexForIslandBasedOnEpisode(currentIslandPlayerIsOn)));

            ImGuiIV.TextUnformatted("WasAnyKeyPressed: {0}", Core.WasAnyKeyPressed);

            if (currentDealerSpot != null)
            {
                ImGuiIV.Spacing(3);
                ImGuiIV.TextColored(Color.Green, "Found dealer spot");
                ImGuiIV.TextUnformatted("DealerHandle: {0}", currentDealerSpot.DealerHandle);
            }
            else
            {
                ImGuiIV.Spacing(3);
                ImGuiIV.TextColored(Color.Red, "No current dealer spot");
            }

            ImGuiIV.End();
        }
        private void PickupDebugOverlay()
        {
            if (!ShowPickupDebugOverlay)
                return;

            for (int i = 0; i < pickups.Length; i++)
            {
                IVPickup pickup = pickups[i];

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
                    ImGuiIV.TextUnformatted("Blip: {0} ({1}, {2})", pickup.Blip, pickup.Blip >> 16, pickup.Blip & 0xFFFF);
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

        private void EditorUI()
        {
            if (!EditorOpened)
                return;

            ImGuiIV.Begin("Underground Intel", ref EditorOpened);

            if (ImGuiIV.BeginTabBar("##UGITabBar"))
            {
                PromptsTab();
                DealersTab();
            }
            ImGuiIV.EndTabBar();

            ImGuiIV.End();
        }
        private void PromptsTab()
        {
            if (ImGuiIV.BeginTabItem("Prompts"))
            {
                ImGuiIV.TextUnformatted("Edit interaction or cutscene prompts.");

                ImGuiIV.Spacing(2);
                ImGuiIV.SeparatorText("Control");

                if (ImGuiIV.Button("Save Prompts"))
                {
                    Core.SavePrompts();
                }
                ImGuiIV.SameLine();
                if (ImGuiIV.Button("Load Prompts"))
                {
                    Core.LoadPrompts();
                }

                ImGuiIV.Spacing(2);
                ImGuiIV.SeparatorText("Prompts");

                ImGuiIV.Spacing(2);
                ImGuiIV.TextUnformatted("Add a new prompt");
                ImGuiIV.InputText("Key", ref promptKey);
                ImGuiIV.InputText("Value", ref promptValue);
                if (ImGuiIV.Button("Add prompt"))
                {
                    if (!string.IsNullOrWhiteSpace(promptKey))
                    {
                        if (Core.TryGetPrompt(promptKey, out string p))
                        {
                            ShowSubtitleMessage("This key already exists in the prompts list!", 5000);
                        }
                        else
                        {
                            Core.Prompts.Add(new Prompt(promptKey, promptValue));
                            ShowSubtitleMessage("Key added!");
                        }
                    }
                    else
                    {
                        ShowSubtitleMessage("Key cannot be empty!", 5000);
                    }
                }

                ImGuiIV.Spacing(4);
                ImGuiIV.TextDisabled("There are currently {0} prompts.", Core.Prompts.Count);

                for (int i = 0; i < Core.Prompts.Count; i++)
                {
                    Prompt prompt = Core.Prompts[i];

                    if (ImGuiIV.CollapsingHeader(string.Format("{0}##UGIPrompts{1}", prompt.Key, i)))
                    {
                        if (ImGuiIV.Button("Delete this prompt"))
                        {
                            Core.Prompts.RemoveAt(i);
                            i--;
                            continue;
                        }
                        if (ImGuiIV.Button("Apply changes"))
                        {
                            prompt.Key = prompt.KeyEdit;
                            prompt.Value = prompt.ValueEdit.Replace("\n", " ");
                        }

                        ImGuiIV.Spacing(3);

                        ImGuiIV.InputText(string.Format("Key##UGIPromptKey{0}", i), ref prompt.KeyEdit);
                        ImGuiIV.InputTextMultiline(string.Format("Value##UGIPromptValue{0}", i), ref prompt.ValueEdit);
                    }
                }

                ImGuiIV.EndTabItem();
            }
        }
        private void DealersTab()
        {
            if (ImGuiIV.BeginTabItem("Dealers"))
            {
                ImGuiIV.TextUnformatted("Edit or add new dealers.");

                ImGuiIV.Spacing(2);
                ImGuiIV.SeparatorText("Control");

                if (ImGuiIV.Button("Save Dealers"))
                {
                    Core.SaveDealers();
                }
                ImGuiIV.SameLine();
                if (ImGuiIV.Button("Load Dealers"))
                {
                    Core.LoadDealers();
                }

                ImGuiIV.Spacing(2);
                ImGuiIV.SeparatorText("Dealers");

                ImGuiIV.Spacing(2);
                ImGuiIV.TextUnformatted("Add a new dealer");
                if (ImGuiIV.Button("Add dealer"))
                {
                    Core.Dealers.Add(new DealerSpot(GENERATE_RANDOM_INT()));
                }

                ImGuiIV.Spacing(4);
                ImGuiIV.TextDisabled("There are currently {0} dealers.", Core.Dealers.Count);

                for (int i = 0; i < Core.Dealers.Count; i++)
                {
                    DealerSpot dealer = Core.Dealers[i];

                    if (ImGuiIV.CollapsingHeader(string.Format("{0}##UGIDealer{1}", dealer.UniqueName, i)))
                    {
                        if (ImGuiIV.Button("Delete this dealer"))
                        {
                            Core.Dealers.RemoveAt(i);
                            i--;
                            continue;
                        }
                        if (ImGuiIV.Button("Apply changes"))
                        {
                            dealer.UniqueName = dealer.UniqueNameEdit;
                            dealer.InteractionPromptKey = dealer.InteractionPromptKeyEdit;
                            dealer.CutscenePromptKey = dealer.CutscenePromptKeyEdit;
                        }

                        ImGuiIV.Spacing(3);

                        ImGuiIV.HelpMarker("Defines a unique name for the dealer to make them easier to identify in the in-game editor.");
                        ImGuiIV.SameLine();
                        ImGuiIV.InputText(string.Format("UniqueName##UGIDealer{0}", i), ref dealer.UniqueNameEdit);

                        ImGuiIV.HelpMarker("Defines the expected world position of the dealer. Used to locate the NPC at that position for enabling interaction.");
                        ImGuiIV.SameLine();
                        if (ImGuiIV.Button("Set to player pos"))
                        {
                            dealer.Position = playerCoords;
                        }
                        ImGuiIV.SameLine();
                        ImGuiIV.DragFloat3(string.Format("Position##UGIDealer{0}", i), ref dealer.Position, 0.1f);

                        ImGuiIV.HelpMarker("Defines whether the player must be within the dealer's vision to enable interaction.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox(string.Format("PlayerNeedsToBeInDealersVision##UGIDealer{0}", i), ref dealer.PlayerNeedsToBeInDealersVision);

                        ImGuiIV.HelpMarker("Defines the dealers field of view. Only used if \"PlayerNeedsToBeInDealersVision\" is set to true.");
                        ImGuiIV.SameLine();
                        ImGuiIV.SliderFloat(string.Format("DealersFOV##UGIDealer{0}", i), ref dealer.DealersFOV, 0f, 180f);

                        ImGuiIV.HelpMarker("Defines how close the player must be to the dealer for the interaction logic to activate.");
                        ImGuiIV.SameLine();
                        ImGuiIV.DragFloat(string.Format("ActivationDistance##UGIDealer{0}", i), ref dealer.ActivationDistance);

                        ImGuiIV.HelpMarker("Defines how close the player must be to the dealer to enable interaction.");
                        ImGuiIV.SameLine();
                        ImGuiIV.DragFloat(string.Format("InteractionDistance##UGIDealer{0}", i), ref dealer.InteractionDistance);

                        ImGuiIV.HelpMarker("Defines the prompt text displayed when the player is within interaction range of the dealer.\n" +
                            "Prompts are located in the 'prompts.json' file. Recommended to be set for improving user experience.");
                        ImGuiIV.SameLine();
                        ImGuiIV.InputText(string.Format("InteractionPrompt##UGIDealer{0}", i), ref dealer.InteractionPromptKeyEdit);

                        ImGuiIV.HelpMarker("Defines the prompt text shown after the player interacts with the dealer and enters the cutscene.\n" +
                            "Prompts are located in the 'prompts.json' file.");
                        ImGuiIV.SameLine();
                        ImGuiIV.InputText(string.Format("CutscenePrompt##UGIDealer{0}", i), ref dealer.CutscenePromptKeyEdit);
                    }
                }

                ImGuiIV.EndTabItem();
            }
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

        private int FindDealerAtPosition(Vector3 pos)
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

                if (!DOES_CHAR_EXIST(pedHandle))
                    continue;

                if (IS_CHAR_DEAD(pedHandle) || IS_PED_IN_COMBAT(pedHandle))
                    continue;

                GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);

                if (Vector3.Distance(pedCoords, pos) > 2f)
                    continue;

                return pedHandle;
            }

            return 0;
        }
        #endregion

        private void Main_Uninitialize(object sender, EventArgs e)
        {
            DisableHooks();

            if (!CLR.CLRBridge.IsShuttingDown)
                RemoveAllCustomPickupBlips();

            ResetCurrentDealer();

            if (pickupsWithCustomTempBlip != null)
            {
                pickupsWithCustomTempBlip.Clear();
                pickupsWithCustomTempBlip = null;
            }

            Core.Shutdown();
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            CreateHooks();
            ModSettings.Load(Settings);
            Core.Init(ScriptResourceFolder);
        }

        private void Main_OnImGuiRendering(IntPtr devicePtr, ImGuiIV_DrawingContext ctx)
        {
#if DEBUG
            ModDebugOverlay();
            PickupDebugOverlay();
#endif

            EditorUI();
        }

        private void Main_ProcessPad(UIntPtr padPtr)
        {
            IVPad pad = IVPad.FromUIntPtr(padPtr);

            if (pad != null)
            {
                Core.WasAnyKeyPressed = pad.Values.Any(x => x.CurrentValue == 255);
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            // Get player stuff
            playerPedHandle = NativeGame.GetPlayerPedHandle();
            GET_CHAR_COORDINATES(playerPedHandle, out playerCoords);
            isUsingController = IS_USING_CONTROLLER();

            // Get current island player is on
            Utils.ZoneToIslandDict.TryGetValue(GET_NAME_OF_ZONE(playerCoords), out currentIslandPlayerIsOn);

            // Get all pickups and store them
            pickups = IVPickups.Pickups;

            // Goes through the list of all custom pickup blips and checks if they can still exist
#if DEBUG
            if (!DisableTempBlipRemovalChecks)
            {
#endif
                CanCustomPickupBlipsStillExists();
#if DEBUG
            }
#endif

            // Add custom pickup blips
#if DEBUG
            if (!DisableAddingTempBlipsCode)
            {
#endif
                AddCustomPickupBlips();
#if DEBUG
            }
#endif

            // Handle dealer interaction
            if (currentDealerSpot == null)
            {
                FindDealer();
            }
            else
            {
                // If the reveil cutscene is currently active we want to prevent interaction
                if (!DealerPickupIntelCutscene.IsCutsceneActive)
                    HandleDealerInteraction();
                
                // Handle dealer cutscene
                if (currentDealerSpot.HasDealerHandle())
                    DealerPickupIntelCutscene.Process(playerPedHandle, currentDealerSpot, currentIslandPlayerIsOn);

                CheckCurrentDealer();
            }

            // Visualize all dealer positions, ranges and states
            if (VisualizeDealerStuff)
                Core.VisualizeDealerStuff(playerCoords);
        }

    }
}
