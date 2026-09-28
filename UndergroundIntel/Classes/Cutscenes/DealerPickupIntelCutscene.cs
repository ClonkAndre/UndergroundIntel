using System;
using System.Numerics;

using CCL.GTAIV;

using UndergroundIntel.Classes.Json;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

namespace UndergroundIntel.Classes.Cutscenes
{
    internal static class DealerPickupIntelCutscene
    {

        #region Variables
        public static bool IsCutsceneActive;
        private static bool wasCutsceneMessageShown;
        private static bool shouldCutsceneEnd;
        private static bool wasMoneyRemoved;
        private static DealerCutsceneState bouncerCutsceneState;
        private static NativeCamera cutsceneCam;
        private static Vector3 cutsceneTargetLerpPos;
        private static IVPickup closestPickup;
        private static DateTime cutsceneEndTime;
        private static int moneyPropModel;
        private static int moneyObject;
        private static Vector3 objectOffset = new Vector3(0.1f, 0f, 0f);
        private static bool storedHudState;
        private static uint storedRadarState;
        #endregion

        #region Methods
        public static void Start()
        {
            IsCutsceneActive = true;
            moneyPropModel = (int)RAGE.AtStringHash("cj_cash_pile_1");
            storedHudState = IVMenuManager.HudOn;
            storedRadarState = IVMenuManager.RadarMode;
            bouncerCutsceneState = DealerCutsceneState.Beginning;
        }
        public static void Process(int playerPedHandle, DealerSpot currentDealerSpot, Island currentIslandPlayerIsOn)
        {
            if (!IsCutsceneActive)
                return;

            switch (bouncerCutsceneState)
            {
                case DealerCutsceneState.Beginning:
                    {
                        CLEAR_HELP();
                        SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), false);
                        CLEAR_CHAR_TASKS(playerPedHandle);
                        DISABLE_PAUSE_MENU(true);

                        // Change state
                        bouncerCutsceneState = DealerCutsceneState.WaitForAssetsToBeLoaded;
                    }
                    break;

                case DealerCutsceneState.WaitForAssetsToBeLoaded:
                    {
                        // Load model
                        if (!HAS_MODEL_LOADED(moneyPropModel))
                        {
                            REQUEST_MODEL(moneyPropModel);
                            return;
                        }

                        // Load anims
                        if (!HAVE_ANIMS_LOADED("missbrian_2"))
                        {
                            REQUEST_ANIMS("missbrian_2");
                            return;
                        }

                        // Change state
                        bouncerCutsceneState = DealerCutsceneState.GiveMoney;
                    }
                    break;

                case DealerCutsceneState.GiveMoney:
                    {
                        if (!IS_CHAR_PLAYING_ANIM(currentDealerSpot.DealerHandle, "missbrian_2", "take_obj"))
                        {
                            Utils.PlayAnimation(currentDealerSpot.DealerHandle, "missbrian_2", "take_obj", 1f, 0, AnimationFlags.None);
                        }
                        else
                        {
                            GET_CHAR_ANIM_CURRENT_TIME(currentDealerSpot.DealerHandle, "missbrian_2", "take_obj", out float bouncerAnimValue);

                            if (bouncerAnimValue > 0.5f) // At this point the gun store owner reaches out his hand to the player
                            {
                                // Process player animation
                                if (!IS_CHAR_PLAYING_ANIM(playerPedHandle, "missbrian_2", "give_obj"))
                                {
                                    // Create and give player money prop
                                    if (moneyObject == 0)
                                    {
                                        GET_CHAR_COORDINATES(playerPedHandle, out Vector3 playerCoords);
                                        CREATE_OBJECT(moneyPropModel, playerCoords, out moneyObject, true);
                                        ATTACH_OBJECT_TO_PED(moneyObject, playerPedHandle, (uint)eBone.BONE_RIGHT_HAND, objectOffset, Vector3.Zero, 0);
                                    }

                                    Utils.PlayAnimation(playerPedHandle, "missbrian_2", "give_obj", 1f, 0, AnimationFlags.None);
                                }
                                else
                                {
                                    GET_CHAR_ANIM_CURRENT_TIME(playerPedHandle, "missbrian_2", "give_obj", out float playerAnimValue);

                                    if (playerAnimValue > 0.9f) // Can change state now
                                    {
                                        // Delete money object and mark stuff as no longer needed
                                        if (moneyObject != 0)
                                        {
                                            DETACH_OBJECT(moneyObject, true);
                                            DELETE_OBJECT(ref moneyObject);
                                            MARK_MODEL_AS_NO_LONGER_NEEDED(moneyPropModel);
                                        }

                                        // Change state
                                        bouncerCutsceneState = DealerCutsceneState.PrepareForPickupReveil;
                                    }
                                    else if (playerAnimValue.InRange(0.4f, 0.45f)) // Players hands over money
                                    {
#if !DEBUG
                                        // Remove money
                                        if (!wasMoneyRemoved)
                                        {
                                            wasMoneyRemoved = true;
                                            ADD_SCORE(CONVERT_INT_TO_PLAYERINDEX(GET_PLAYER_ID()), -1 * ModSettings.PickupIntelFee);
                                        }
#endif

                                        // Set state for current save file
                                        Utils.SetIntelAsBoughtForIsland(currentIslandPlayerIsOn);

                                        DETACH_OBJECT(moneyObject, true);
                                        ATTACH_OBJECT_TO_PED(moneyObject, currentDealerSpot.DealerHandle, (uint)eBone.BONE_RIGHT_HAND, objectOffset, Vector3.Zero, 0);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case DealerCutsceneState.PrepareForPickupReveil:
                    {
                        // Fade screen out
                        if (!Utils.DoAndCheckFadeScreenOut(2000))
                            return;

                        // Get closest pickup
                        GET_CHAR_COORDINATES(playerPedHandle, out Vector3 playerCoords);

                        // This should never return false as there should ALWAYS be a pickup but incase there is not we assert
                        //System.Diagnostics.Debug.Assert(Utils.TryFindRandomPickupAroundPositionOnIsland(playerCoords, 380f, currentIslandPlayerIsOn, out IVPickup? foundPickup));

                        closestPickup = Utils.FindClosestPickup(playerCoords);

                        // Create cutscene cam
                        cutsceneCam = NativeCamera.Create();
                        cutsceneCam.Position = closestPickup.Position + new Vector3(0f, 0f, 6f);
                        cutsceneCam.PointAtCoord(closestPickup.Position);
                        cutsceneCam.Activate();

                        cutsceneTargetLerpPos = cutsceneCam.Position - new Vector3(0f, 0f, 3f);

                        // Load Scene
                        NativeWorld.LoadEnvironmentNow(closestPickup.Position, true);

                        // Change state
                        bouncerCutsceneState = DealerCutsceneState.LoadingScene;
                    }
                    break;

                case DealerCutsceneState.LoadingScene:
                    {
                        IVMenuManager.HudOn = false;
                        IVMenuManager.RadarMode = 0;

                        // Slowly lower the cam
                        cutsceneCam.Position = Vector3.Lerp(cutsceneCam.Position, cutsceneTargetLerpPos, 0.0002f);

                        // Fade screen in
                        if (!Utils.DoAndCheckFadeScreenIn(2000))
                            return;

                        // Set cutscene end time
                        cutsceneEndTime = DateTime.UtcNow.AddSeconds(10d);

                        // Change state
                        bouncerCutsceneState = DealerCutsceneState.ProcessPickupReveil;
                    }
                    break;

                case DealerCutsceneState.ProcessPickupReveil:
                    {
                        IVMenuManager.HudOn = false;
                        IVMenuManager.RadarMode = 0;

                        // Slowly lower the cam
                        cutsceneCam.Position = Vector3.Lerp(cutsceneCam.Position, cutsceneTargetLerpPos, 0.00025f);

                        if (!wasCutsceneMessageShown)
                        {
                            if (Core.TryGetPrompt(currentDealerSpot.CutscenePromptKey, out string prompt))
                                NativeGame.DisplayCustomHelpMessage(prompt);
                            
                            wasCutsceneMessageShown = true;
                        }
                        
                        // Check if cutscene should end
                        if (DateTime.UtcNow > cutsceneEndTime || Core.WasAnyKeyPressed)
                            shouldCutsceneEnd = true;

                        // Handle cutscene end
                        if (shouldCutsceneEnd)
                        {
                            // Fade screen out
                            if (!Utils.DoAndCheckFadeScreenOut(2000))
                                return;

                            // Get rid of cutscene cam
                            if (cutsceneCam != null)
                            {
                                cutsceneCam.Deactivate();
                                cutsceneCam.Delete();
                                cutsceneCam = null;
                            }

                            // Change state
                            bouncerCutsceneState = DealerCutsceneState.Ending;
                        }
                    }
                    break;

                case DealerCutsceneState.Ending:
                    {
                        IVMenuManager.HudOn = storedHudState;
                        IVMenuManager.RadarMode = storedRadarState;

                        // Fade screen in
                        if (!Utils.DoAndCheckFadeScreenIn(2000))
                            return;

                        SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), true);
                        SAY_AMBIENT_SPEECH(currentDealerSpot.DealerHandle, "THANKS", true, false, 0);
                        DISABLE_PAUSE_MENU(false);

                        switch (NativeGame.CurrentEpisode)
                        {
                            case Episode.IV:
                                TRIGGER_MISSION_COMPLETE_AUDIO((int)eMissionCompleteAudio.SMC_35);
                                break;
                            case Episode.TLaD:
                                TRIGGER_MISSION_COMPLETE_AUDIO(81);
                                break;
                            case Episode.TBoGT:
                                TRIGGER_MISSION_COMPLETE_AUDIO(82);
                                break;
                        }

                        wasMoneyRemoved = false;
                        wasCutsceneMessageShown = false;
                        shouldCutsceneEnd = false;
                        IsCutsceneActive = false;
                    }
                    break;
            }
        }
        #endregion

    }
}
