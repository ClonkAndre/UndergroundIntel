using System.Numerics;

using Newtonsoft.Json;
using CCL.GTAIV;

using IVSDKDotNet.Enums;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace UndergroundIntel.Classes.Json
{
    internal class DealerSpot
    {

        #region Variables
        [JsonIgnore] public int DealerHandle;
        [JsonIgnore] public NativeBlip DealerBlip;

        [JsonIgnore] public string UniqueNameEdit;
        public string UniqueName;

        [JsonIgnore] public DealerInteractionState InteractionState;

        public Vector3 Position;
        public bool PlayerNeedsToBeInDealersVision;
        public float DealersFOV;
        public float ActivationDistance;
        public float InteractionDistance;

        [JsonIgnore] public string InteractionPromptKeyEdit;
        public string InteractionPromptKey;

        [JsonIgnore] public string CutscenePromptKeyEdit;
        public string CutscenePromptKey;
        #endregion

        #region Constructor
        public DealerSpot(int rndNumber)
        {
            UniqueNameEdit = string.Concat("Unnamed Spot #", rndNumber);
            UniqueName = string.Concat("Unnamed Spot #", rndNumber);
            InteractionPromptKey = string.Empty;
            CutscenePromptKey = string.Empty;
        }
        public DealerSpot()
        {
            UniqueNameEdit = "Unnamed Spot";
            UniqueName = "Unnamed Spot";
            InteractionPromptKey = string.Empty;
            CutscenePromptKey = string.Empty;
        }
        #endregion

        public void Init()
        {
            UniqueNameEdit = UniqueName;
            InteractionPromptKeyEdit = InteractionPromptKey;
            CutscenePromptKeyEdit = CutscenePromptKey;
        }

        public void Reset()
        {
            DestroyBlip();
            DealerHandle = 0;
        }

        public void CreateBlip()
        {
            // Don't create it twice
            if (DealerBlip != null)
                return;

            DealerBlip = NativeBlip.AddBlip(DealerHandle);
            DealerBlip.Color = eBlipColor.BLIP_COLOR_YELLOW;
            DealerBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_ONLY;
        }
        public void DestroyBlip()
        {
            if (DealerBlip == null)
                return;

            DealerBlip.Delete();
            DealerBlip = null;
        }

        public bool HasDealerHandle()
        {
            return DealerHandle != 0;
        }

    }
}

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value