using static Pom.Pom;
using UnityEngine;

namespace Slugpack
{
    static class InitializeObjects
    {
        internal static void Apply()
        {
            On.RainWorld.OnModsInit += (orig, self) =>
            {
                orig(self);

                TrackHologram.Apply();
                RegisterManagedObject<TrackHologram, TrackHologramData, ManagedRepresentation>("TrackHologram", "Technomancer");
                // RegisterManagedObject<DebugTrainItem, DebugTrainItemData, ManagedRepresentation>("TrainTrack", "Technomancer");
                RegisterManagedObject<TrainTrack, TrainTrackData, ManagedRepresentation>("TrainTrack", "Technomancer");
                RegisterManagedObject<TrainWarningBell, TrainWarningBellData, ManagedRepresentation>("TrainBell", "Technomancer");
                RegisterManagedObject<PartyBanner, PartyBannerData, ManagedRepresentation>("Banners", "Technomancer");
                RegisterManagedObject<WaterTrash, WaterTrashData, ManagedRepresentation>("WaterTrash", "Technomancer");
                RegisterManagedObject<DragonSkull, DragonSkullData, ManagedRepresentation>("DragonSkull", "Technomancer");
                RegisterManagedObject<EffectChanger, EffectChangerData, ManagedRepresentation>("EffectColourChanger", "Technomancer");
            };
        }
    }
}