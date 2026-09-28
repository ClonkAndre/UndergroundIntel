using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

using UndergroundIntel.Classes;
using UndergroundIntel.Classes.Json;

using Newtonsoft.Json;

using static IVSDKDotNet.Native.Natives;

namespace UndergroundIntel
{
    internal static class Core
    {

        #region Variables
        private static string scriptResourceFolder;

        public static List<DealerSpot> Dealers;
        public static List<Prompt> Prompts;

        public static bool WasAnyKeyPressed;
        #endregion

        #region Methods
        public static void Init(string theScriptResourceFolder)
        {
            scriptResourceFolder = theScriptResourceFolder;

            Dealers = new List<DealerSpot>();
            Prompts = new List<Prompt>();

            LoadPrompts();
            LoadDealers();
        }
        public static void Shutdown()
        {
            if (Dealers != null)
            {
                Dealers.Clear();
                Dealers = null;
            }
            if (Prompts != null)
            {
                Prompts.Clear();
                Prompts = null;
            }
        }

        public static void LoadPrompts()
        {
            try
            {
                // Build file path
                string path = Path.Combine(scriptResourceFolder, "prompts.json");

                // Check if file exists
                if (!File.Exists(path))
                {
                    Logging.LogWarning("Could not load prompts because the file 'prompts.json' could not be found!");
                    return;
                }

                // Clear
                Prompts.Clear();

                // Try read file and parse
                string content = File.ReadAllText(path);
                string jsonStringNoComments = Utils.RemoveCommentsFromJsonString(content);
                Prompts = JsonConvert.DeserializeObject<List<Prompt>>(jsonStringNoComments);
                Prompts.ForEach(x => x.Init());

                Logging.LogDebug("Loaded {0} prompts.", Prompts.Count);
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to load prompts! Details: {0}", ex);
            }
        }
        public static void LoadDealers()
        {
            try
            {
                // Build file path
                string path = Path.Combine(scriptResourceFolder, "dealers.json");

                // Check if file exists
                if (!File.Exists(path))
                {
                    Logging.LogWarning("Could not load dealers because the file 'dealers.json' could not be found!");
                    return;
                }

                // Clear
                Dealers.Clear();

                // Try read file and parse
                string content = File.ReadAllText(path);
                string jsonStringNoComments = Utils.RemoveCommentsFromJsonString(content);
                Dealers = JsonConvert.DeserializeObject<List<DealerSpot>>(jsonStringNoComments);
                Dealers.ForEach(x => x.Init());

                Logging.LogDebug("Loaded {0} dealers.", Dealers.Count);
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to load dealers! Details: {0}", ex);
            }
        }

        public static void SavePrompts()
        {
            try
            {
                // Build file path
                string path = Path.Combine(scriptResourceFolder, "prompts.json");

                // Try save file
                File.WriteAllText(path, JsonConvert.SerializeObject(Prompts, Formatting.Indented));

                Logging.LogDebug("Saved {0} prompts.", Prompts.Count);
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to save prompts! Details: {0}", ex);
            }
        }
        public static void SaveDealers()
        {
            try
            {
                // Build file path
                string path = Path.Combine(scriptResourceFolder, "dealers.json");

                // Try save file
                File.WriteAllText(path, JsonConvert.SerializeObject(Dealers, Formatting.Indented));

                Logging.LogDebug("Saved {0} dealers.", Dealers.Count);
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to save dealers! Details: {0}", ex);
            }
        }

        public static void VisualizeDealerStuff(Vector3 playerCoords)
        {
            for (int i = 0; i < Dealers.Count; i++)
            {
                DealerSpot spot = Dealers[i];

                if (Vector3.Distance(spot.Position, playerCoords) > 50f)
                    continue;

                // Visualize stuff
                DRAW_CORONA(spot.Position, 25f, 0, 0f, Color.Red);
                DRAW_CHECKPOINT_WITH_ALPHA(spot.Position, spot.ActivationDistance * 2f, Color.FromArgb(60, Color.Yellow));

                // Check if dealer NPC was found at expected position
                if (!spot.HasDealerHandle())
                {
                    Utils.DisplayListOfTextAtWorldPosition(spot.Position, Color.Red, 8f,
                        spot.UniqueName,
                        "No dealer interaction possible.",
                        "- The player might not be within the activation range yet.",
                        "- The NPC might've moved from its expected position.");
                }
                else
                {
                    // Get dealer world position
                    GET_CHAR_COORDINATES(spot.DealerHandle, out Vector3 dealerPos);

                    switch (spot.InteractionState)
                    {
                        case DealerInteractionState.Interactable:

                            Utils.DisplayListOfTextAtWorldPosition(dealerPos, Color.Green, 8f,
                                spot.UniqueName,
                                "Dealer can be interacted with.");

                            break;
                        case DealerInteractionState.InteractableButNoPromptWasSet:

                            Utils.DisplayListOfTextAtWorldPosition(dealerPos, Color.Yellow, 8f,
                                spot.UniqueName,
                                "Dealer can be interacted with, but no interaction prompt was set.");

                            break;
                        case DealerInteractionState.NotWithinInteractionDistance:

                            Utils.DisplayListOfTextAtWorldPosition(dealerPos, Color.Red, 8f,
                                spot.UniqueName,
                                "No dealer interaction possible.",
                                "- Player is not within interaction distance.");

                            break;
                        case DealerInteractionState.NotWithinDealersVision:

                            Utils.DisplayListOfTextAtWorldPosition(dealerPos, Color.Red, 8f,
                                spot.UniqueName,
                                "No dealer interaction possible.",
                                "- Player is not within dealers vision.");

                            break;
                        case DealerInteractionState.NotEnoughMoney:

                            Utils.DisplayListOfTextAtWorldPosition(dealerPos, Color.Red, 8f,
                                spot.UniqueName,
                                "No dealer interaction possible.",
                                "- Player does not have enough money.");

                            break;
                    }
                }
            }
        }
        #endregion

        #region Functions
        public static DealerSpot FindClosestDealerSpotFromPosition(Vector3 pos)
        {
            if (Dealers == null)
                return null;

            return Dealers.Where(d => Vector3.Distance(pos, d.Position) < d.ActivationDistance).FirstOrDefault();
        }
        public static bool TryGetPrompt(string key, out string prompt)
        {
            if (Prompts == null)
            {
                prompt = null;
                return false;
            }

            Prompt foundPrompt = Prompts.Where(x => x.Key == key).FirstOrDefault();

            if (foundPrompt == null)
            {
                prompt = null;
                return false;
            }

            prompt = foundPrompt.Value;
            return true;
        }
        #endregion

    }
}
