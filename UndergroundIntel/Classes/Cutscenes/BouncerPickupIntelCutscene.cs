using System;
using System.Numerics;

using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

namespace UndergroundIntel.Classes.Cutscenes
{
    internal static class BouncerPickupIntelCutscene
    {

        #region Variables
        public static bool IsCutsceneActive;
        private static bool wasCutsceneMessageShown;
        private static bool wasMoneyRemoved;
        private static BouncerCutsceneState bouncerCutsceneState;
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
            bouncerCutsceneState = BouncerCutsceneState.Beginning;
        }
        public static void HandleBouncerCutscene(int playerPedHandle, int currentBouncerHandle, Island currentIslandPlayerIsOn)
        {
            if (!IsCutsceneActive)
                return;

            switch (bouncerCutsceneState)
            {
                case BouncerCutsceneState.Beginning:
                    {
                        CLEAR_HELP();
                        SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), false);
                        CLEAR_CHAR_TASKS(playerPedHandle);

                        // Change state
                        bouncerCutsceneState = BouncerCutsceneState.WaitForAssetsToBeLoaded;
                    }
                    break;

                case BouncerCutsceneState.WaitForAssetsToBeLoaded:
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
                        bouncerCutsceneState = BouncerCutsceneState.GiveMoney;
                    }
                    break;

                case BouncerCutsceneState.GiveMoney:
                    {
                        if (!IS_CHAR_PLAYING_ANIM(currentBouncerHandle, "missbrian_2", "take_obj"))
                        {
                            Utils.PlayAnimation(currentBouncerHandle, "missbrian_2", "take_obj", 1f, 0, AnimationFlags.None);
                        }
                        else
                        {
                            GET_CHAR_ANIM_CURRENT_TIME(currentBouncerHandle, "missbrian_2", "take_obj", out float bouncerAnimValue);

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
                                        bouncerCutsceneState = BouncerCutsceneState.PrepareForPickupReveil;
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
                                        ATTACH_OBJECT_TO_PED(moneyObject, currentBouncerHandle, (uint)eBone.BONE_RIGHT_HAND, objectOffset, Vector3.Zero, 0);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case BouncerCutsceneState.PrepareForPickupReveil:
                    {
                        // Fade screen out
                        if (!Utils.DoAndCheckFadeScreenOut(2000))
                            return;

                        // Get closest pickup
                        GET_CHAR_COORDINATES(playerPedHandle, out Vector3 playerCoords);
                        closestPickup = Utils.FindClosestPickup(playerCoords);

                        // Create cutscene cam
                        cutsceneCam = NativeCamera.Create();
                        cutsceneCam.Position = closestPickup.Position + new Vector3(0f, 0f, 6f);
                        cutsceneCam.PointAtCoord(closestPickup.Position);
                        cutsceneCam.Activate();

                        cutsceneTargetLerpPos = cutsceneCam.Position - new Vector3(0f, 0f, 3f);

                        // Screen is faded out here, load area
                        LOAD_SCENE(closestPickup.Position);

                        // Change state
                        bouncerCutsceneState = BouncerCutsceneState.LoadingScene;
                    }
                    break;

                case BouncerCutsceneState.LoadingScene:
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
                        bouncerCutsceneState = BouncerCutsceneState.ProcessPickupReveil;
                    }
                    break;

                case BouncerCutsceneState.ProcessPickupReveil:
                    {
                        IVMenuManager.HudOn = false;
                        IVMenuManager.RadarMode = 0;

                        // Slowly lower the cam
                        cutsceneCam.Position = Vector3.Lerp(cutsceneCam.Position, cutsceneTargetLerpPos, 0.00025f);

                        if (!wasCutsceneMessageShown)
                        {
                            NativeGame.DisplayCustomHelpMessage(string.Format("Intel about available pickups unlocked for {0}!", Utils.ToNiceLookingFullIslandName(currentIslandPlayerIsOn)));
                            wasCutsceneMessageShown = true;
                        }

                        // Check if cutscene should end
                        if (DateTime.UtcNow > cutsceneEndTime)
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
                            bouncerCutsceneState = BouncerCutsceneState.Ending;
                        }
                    }
                    break;

                case BouncerCutsceneState.Ending:
                    {
                        IVMenuManager.HudOn = storedHudState;
                        IVMenuManager.RadarMode = storedRadarState;

                        // Fade screen in
                        if (!Utils.DoAndCheckFadeScreenIn(2000))
                            return;

                        SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), true);
                        SAY_AMBIENT_SPEECH(currentBouncerHandle, "THANKS", true, false, 0);

                        TRIGGER_MISSION_COMPLETE_AUDIO((int)eMissionCompleteAudio.SMC_35);

                        wasCutsceneMessageShown = false;
                        wasMoneyRemoved = false;
                        IsCutsceneActive = false;
                    }
                    break;
            }
        }
        #endregion

    }
}
