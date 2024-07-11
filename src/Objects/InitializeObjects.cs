namespace Slugpack;

internal static class InitializeObjects
{
    internal static void Apply()
    {
        try
        {
            if (!initialized)
            {
                RegisterManagedObject<TrackHologram, TrackHologramData, ManagedRepresentation>("TrackHologram", "Technomancer");

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
                // RegisterManagedObject<ProceduralTrainObject, TrainObjectData, ManagedRepresentation>("DEBUGTRAINOBJECT", "Technomancer");
                RegisterManagedObject<FakeSubregionPopupObject, FakeSubregionPopupData, ManagedRepresentation>("FakeSubregionPopup", "Technomancer");
                RegisterManagedObject<WaterKillBox, WaterKillBoxData, ManagedRepresentation>("LethalWaterKillbox", "Technomancer");
                RegisterManagedObject<LeakyPipe, LeakyPipeData, ManagedRepresentation>("LeakyPipe", "Technomancer");
                RegisterManagedObject<RoomBackground, RoomBackgroundData, ManagedRepresentation>("ImageBackground", "Technomancer");
                RegisterManagedObject<RadioLight, RadioLightObjectData, ManagedRepresentation>("BackgroundRadioLights", "Technomancer");
                RegisterManagedObject<RadioLightExtender, RadioLightExtenderData, ManagedRepresentation>("BackgroundRadioExtender", "Technomancer");

                // RegisterManagedObject<ColourObject, ColourObjectData, ManagedRepresentation>("COLOURGRABBER", "Technomancer");

                Plugin.DebugWarning("Technomancer Objects Initialized!");

                initialized = true;
            }
        }
        catch (Exception ex)
        {
            Plugin.DebugError(ex);
            Debug.LogException(ex);
        }
    }

    private static bool initialized;
}