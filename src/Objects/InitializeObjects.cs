using System.ComponentModel;
using static Pom.Pom;

namespace Slugpack
{
    static class InitializeObjects
    {
        internal static void Apply()
        {
            try
            {
                if (!initialized)
                {
                    RegisterManagedObject<TrackHologramObject, TrackHologramData, ManagedRepresentation>("TrackHologram", "Technomancer");

                    // RegisterManagedObject<DebugTrainItem, DebugTrainItemData, ManagedRepresentation>("TrainTrack", "Technomancer");
                    RegisterManagedObject<TrainTrack, TrainTrackData, ManagedRepresentation>("TrainTrack", "Technomancer");
                    RegisterManagedObject<TrainWarningBell, TrainWarningBellData, ManagedRepresentation>("TrainBell", "Technomancer");
                    RegisterManagedObject<PartyBanner, PartyBannerData, ManagedRepresentation>("Banners", "Technomancer");
                    RegisterManagedObject<WaterTrash, WaterTrashData, ManagedRepresentation>("WaterTrash", "Technomancer");
                    RegisterManagedObject<DragonSkull, DragonSkullData, ManagedRepresentation>("DragonSkull", "Technomancer");
                    RegisterManagedObject<EffectChanger, EffectChangerData, ManagedRepresentation>("EffectColourChanger", "Technomancer");
                    RegisterManagedObject<ModifiedLightBeam, ModifiedLightBeamData, ManagedRepresentation>("ModifiedLightBeam", "Technomancer");
                    RegisterManagedObject<ModifiedLightSource, ModifiedLightSourceData, ManagedRepresentation>("ModifiedLightSource", "Technomancer");
                    RegisterManagedObject<HypothermiaRadius, HypothermiaRadiusData, ManagedRepresentation>("HypothermiaRadius", "Technomancer");
                    // RegisterManagedObject<TrainLeftHead, TrainTrackObjectData, ManagedRepresentation>("DEBUGTRAINOBJECT", "Technomancer");
                    RegisterManagedObject<SpinningFan, SpinningFanData, ManagedRepresentation>("Spinning Fan", "Technomancer");
                    RegisterManagedObject<ProceduralTrainObject, TrainObjectData, ManagedRepresentation>("DEBUGTRAINOBJECT", "Technomancer");
                    RegisterManagedObject<FakeSubregionPopupObject, FakeSubregionPopupData, ManagedRepresentation>("FakeSubregionPopup", "Technomancer");

                    Debug.LogWarning("Technomancer Objects Initialized!");

                    initialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogException(ex);
            }
        }

        static bool initialized;
    }
}