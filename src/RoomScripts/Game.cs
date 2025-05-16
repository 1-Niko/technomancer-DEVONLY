namespace Slugpack;

public static class GameHooks
{
    public static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RoomRealizer.CanAbstractizeRoom += RoomRealizer_CanAbstractizeRoom;
        On.RainWorldGame.Update += RainWorldGame_Update;
        //On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        //On.Region.EquivalentRegion += Region_EquivalentRegion;
        //On.Region.GetVanillaEquivalentRegionAcronym += Region_GetVanillaEquivalentRegionAcronym;
        On.ShortcutGraphics.Update += ShortcutGraphics_Update;
        On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
        On.AboveCloudsView.ctor += AboveCloudsView_ctor;

        On.ShortcutHandler.Update += ShortcutHandler_Update;

        On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        On.Creature.SpitOutOfShortCut += Creature_SpitOutOfShortCut;

        On.RoomCamera.MoveCamera2 += RoomCamera_MoveCamera2;

        On.RoomCamera.MoveCamera2 += (orig, self, roomName, camPos) =>
        {
            orig(self, roomName, camPos);
            if (DamagedShortcuts.TryGetValue(self.game, out var CameraPosition))
            {
                CameraPosition.camPosition = camPos;
                CameraPosition.room = roomName;
            }
        };
    }

    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (self.room.world.game.GetStorySession?.saveStateNumber.value == Technomancer)
        {
            // self.elements.Remove(self.daySky);
            // self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_Sky-technomancer", new Vector2(683f, 384f));
            // self.AddElement(self.daySky);

            self.daySky.illustrationName = "atc_sky-technomancer";
            self.LoadGraphic("atc_sky-technomancer", true, true);
            self.duskSky.illustrationName = "atc_dusksky-technomancer";
            self.LoadGraphic("atc_dusksky-technomancer", true, true);
            self.nightSky.illustrationName = "atc_nightsky-technomancer";
            self.LoadGraphic("atc_nightsky-technomancer", true, true);

            int[] removeList = { 29, 28, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 13, 12, 11, 10, 9, 8, 7, 6, 4 };

            for (int i = 0; i < removeList.Length; i++)
            {
                self.elements.RemoveAt(removeList[i]);
            }

            // (self.elements[4] as AboveCloudsView.DistantBuilding).assetName = "atc_structure1-technomancer";
            // self.LoadGraphic("atc_structure1-technomancer", true, true);

            float depth = 160f;
            self.elements.Add(new AnimatedIterator(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-520f, -85f), depth), depth, -20f));
            depth = 110f;
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-594f, 152f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), new UnityEngine.Color(1f, 0f, 0f, 1f), 54, 0.06f, 77, 0.53f, 3085, false, false, false, false));
            depth = 385;
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(330f, 90f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), new UnityEngine.Color(1f, 0f, 0f, 1f), 61, 0.23f, 76, 0.28f, 3066, false, false, false, false));
            depth = 9999;
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(28f, 1f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.35f), new UnityEngine.Color(1f, 0f, 0f, 0.35f), 64, 0.43f, 62, 0.73f, 2980, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-9f, -3f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.15f), new UnityEngine.Color(1f, 0f, 0f, 0.15f), 56, 0.22f, 93, 0.32f, 2979, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-155f, -3f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.35f), new UnityEngine.Color(1f, 0f, 0f, 0.35f), 59, 0.34f, 78, 0.35f, 3015, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-167f, 5f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.15f), new UnityEngine.Color(1f, 0f, 0f, 0.15f), 60, 0.18f, 84, 0.44f, 2984, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-176f, -3f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.35f), new UnityEngine.Color(1f, 0f, 0f, 0.35f), 75, 0.08f, 78, 0.57f, 2993, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-281f, -3f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.15f), new UnityEngine.Color(1f, 0f, 0f, 0.15f), 63, 0.3f, 64, 0.65f, 3042, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-353f, 9f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.15f), new UnityEngine.Color(1f, 0f, 0f, 0.15f), 66, 0.32f, 75, 0.37f, 3074, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-370f, 32f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.75f), new UnityEngine.Color(1f, 0f, 0f, 0.75f), 53, 0.22f, 91, 0.54f, 3037, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-402f, 16f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.85f), new UnityEngine.Color(1f, 0f, 0f, 0.85f), 77, 0.43f, 72, 0.59f, 3034, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-408f, -3f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.6f), new UnityEngine.Color(1f, 0f, 0f, 0.6f), 60, 0.04f, 85, 0.28f, 3017, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-617f, -3f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.15f), new UnityEngine.Color(1f, 0f, 0f, 0.15f), 71, 0.08f, 69, 0.55f, 2944, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-672f, 22f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.35f), new UnityEngine.Color(1f, 0f, 0f, 0.35f), 52, 0.11f, 79, 0.63f, 3019, false, false, false, true));
            self.elements.Add(new DistantBlinkingLight(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-677f, -2f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 0.15f), new UnityEngine.Color(1f, 0f, 0f, 0.15f), 72, 0.16f, 85, 0.57f, 3010, false, false, false, true));

            self.elements.Add(new AdTower(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(-241f, -28f), 204f), 204f, -23f));

            self.elements.Add(new SkyProjector(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(245f, -73f), 130f), 130f, -14f));
            self.elements.Add(new Karma10Projection(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(245f, 180f), 130f), 130f, -14f));

            /*
            var orbitalPathFunctions = new Func<float, float>[]
            {
                oc => (-0.0000363267f * oc * oc) + (0.1710931268f * oc) - 86.5630594383f,  // Original orbital 0
                oc => (-0.0000334082f * oc * oc) + (0.1712030144f * oc) - 65.9813493144f,  // Original orbital 1
                oc => (-0.0000323288f * oc * oc) + (0.1710666561f * oc) - 47.8399195652f,  // Original orbital 2
                oc => (-0.0000303659f * oc * oc) + (0.1711115019f * oc) - 32.1212810071f,  // Original orbital 3
                oc => (-0.0000287223f * oc * oc) + (0.1710530134f * oc) - 18.2210235126f,  // Original orbital 4
                oc => (-0.0000274961f * oc * oc) + (0.1709293376f * oc) - 5.8558448939f,   // Original orbital 5
                oc => (-0.0000260650f * oc * oc) + (0.1710802118f * oc) + 5.2381700178f,    // Original orbital 6
                oc => (-0.0000250335f * oc * oc) + (0.1710988528f * oc) + 15.1542853523f,   // Original orbital 7
                oc => (-0.0000239273f * oc * oc) + (0.1713665901f * oc) + 24.2633553128f,   // Original orbital 8
                oc => (-0.0000234322f * oc * oc) + (0.1708555706f * oc) + 32.5331710154f,   // Original orbital 9
                oc => (-0.0000216621f * oc * oc) + (0.1711151146f * oc) + 39.9845200978f,   // Original orbital 10
                oc => (-0.0000220592f * oc * oc) + (0.1708293211f * oc) + 46.8635180885f,   // Original orbital 11
                oc => (-0.0000216171f * oc * oc) + (0.1705425743f * oc) + 53.2632652169f,   // Original orbital 12
                oc => (-0.0000208150f * oc * oc) + (0.1707844536f * oc) + 59.1532616606f,   // Original orbital 13
                oc => (-0.0000202718f * oc * oc) + (0.1706248968f * oc) + 64.7266484433f,   // Original orbital 14
                oc => (-0.0000189377f * oc * oc) + (0.1708087470f * oc) + 69.5016064993f,   // Original orbital 15
                oc => (-0.0000185779f * oc * oc) + (0.1707352130f * oc) + 74.3526367362f,   // Original orbital 16
                oc => (-0.0000183122f * oc * oc) + (0.1707690558f * oc) + 78.7941796466f,   // Original orbital 17
                oc => (-0.0000172209f * oc * oc) + (0.1707718221f * oc) + 82.7937619301f,   // Original orbital 18
                oc => (-0.0000163887f * oc * oc) + (0.1709075477f * oc) + 86.5130019370f,   // Original orbital 19
                oc => (-0.0000167171f * oc * oc) + (0.1708766189f * oc) + 90.4854475576f,   // Original orbital 20
                oc => (-0.0000159250f * oc * oc) + (0.1712625822f * oc) + 93.7098254026f,   // Original orbital 21
                oc => (-0.0000155831f * oc * oc) + (0.1712748680f * oc) + 96.9214488030f,   // Original orbital 22
                oc => (-0.0000156110f * oc * oc) + (0.1713169652f * oc) + 99.9895828877f,   // Original orbital 23
                oc => (-0.0000145937f * oc * oc) + (0.1710636304f * oc) + 102.7652850157f,  // Original orbital 24
                oc => (-0.0000146095f * oc * oc) + (0.1710226112f * oc) + 105.6145134023f,  // Original orbital 25
                oc => (-0.0000141062f * oc * oc) + (0.1710418138f * oc) + 108.1091811642f,  // Original orbital 26
                oc => (-0.0000136275f * oc * oc) + (0.1712705948f * oc) + 110.5913379160f   // Original orbital 27
            };

            var orbitalMovementSpeeds = new float[orbitalPathFunctions.Length];
            for (int i = 0; i < orbitalMovementSpeeds.Length; i++)
            {
                orbitalMovementSpeeds[i] = 4f / Mathf.Pow(2f, i);
            }

            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[0], orbitalMovementSpeeds[0]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[1], orbitalMovementSpeeds[1]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[2], orbitalMovementSpeeds[2]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[3], orbitalMovementSpeeds[3]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[4], orbitalMovementSpeeds[4]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[5], orbitalMovementSpeeds[5]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[6], orbitalMovementSpeeds[6]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[7], orbitalMovementSpeeds[7]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[8], orbitalMovementSpeeds[8]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[9], orbitalMovementSpeeds[9]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[10], orbitalMovementSpeeds[10]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[11], orbitalMovementSpeeds[11]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[12], orbitalMovementSpeeds[12]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[13], orbitalMovementSpeeds[13]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[14], orbitalMovementSpeeds[14]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[15], orbitalMovementSpeeds[15]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[16], orbitalMovementSpeeds[16]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[17], orbitalMovementSpeeds[17]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[18], orbitalMovementSpeeds[18]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[19], orbitalMovementSpeeds[19]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[20], orbitalMovementSpeeds[20]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[21], orbitalMovementSpeeds[21]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[22], orbitalMovementSpeeds[22]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[23], orbitalMovementSpeeds[23]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[24], orbitalMovementSpeeds[24]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[25], orbitalMovementSpeeds[25]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[26], orbitalMovementSpeeds[26]));
            self.elements.Add(new DistantSatellite(self, self.PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 0f), depth), depth, 10f, new UnityEngine.Color(0f, 1f, 0f, 1f), 63, 0.9f, 3000, true, orbitalPathFunctions[27], orbitalMovementSpeeds[27]));
            */
        }
    }

    private static void Creature_SpitOutOfShortCut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, RWCustom.IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        // Creature attempts to exit pipe

        if (self.IsPipeProcessing())
        {
            orig(self, pos, newRoom, spitOutAllSticks);
        }
        else
        {
            self.SetPipeProcessing();

            bool creatureMayExit;

            if (PIPE_LOCK_CREATURE_HANDLER)
                Plugin.DebugLog($"---------START OF {self.abstractCreature.creatureTemplate.type}---------");

            if (PipeIsLocked(newRoom.world.game, pos, newRoom))
            {
                if (PIPE_LOCK_CREATURE_HANDLER)
                    Plugin.DebugLog("Check 1: Pipe is locked");
                if (self.HasPassthroughAllowance()) // Creature has already bounced and may exit
                {
                    if (PIPE_LOCK_CREATURE_HANDLER)
                        Plugin.DebugLog("Check 2: Creature has passthrough allowance");
                    self.RevokePassthroughAllowance();
                    creatureMayExit = true;
                }
                else // Send them back
                {
                    if (PIPE_LOCK_CREATURE_HANDLER)
                        Plugin.DebugLog("Check 2: Creature does not have passthrough allowance");
                    creatureMayExit = false;
                    // Spawn effects
                }
            }
            else // Pipe is not locked
            {
                if (PIPE_LOCK_CREATURE_HANDLER)
                    Plugin.DebugLog("Check 1: Pipe was not locked");
                creatureMayExit = true;
            }

            orig(self, pos, newRoom, spitOutAllSticks);
            if (!creatureMayExit)
            {
                if (PIPE_LOCK_CREATURE_HANDLER)
                    Plugin.DebugLog("End Result: Creature was sent back");
                // Send the wretched beast back
                self.GrantPassthroughAllowance();
                self.SuckedIntoShortCut(pos, false);
            }
            else
            {
                if (PIPE_LOCK_CREATURE_HANDLER)
                    Plugin.DebugLog("End Result: Creature was allowed to exit");
            }
            if (PIPE_LOCK_CREATURE_HANDLER)
                Plugin.DebugLog($"---------END OF {self.abstractCreature.creatureTemplate.type}---------");

            self.EndPipeProcessing();
        }
    }

    private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, RWCustom.IntVector2 entrancePos, bool carriedByOther)
    {
        if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
            Plugin.DebugLog(">   CREATURE SUCKED_INTO_SHORTCUT BEGIN");

        if (Null.Check(self, 3))
        {
            if (PipeIsLocked(self.room.world.game, entrancePos, self.room))
            {
                if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                    Plugin.DebugLog("    Check 1: Pipe is locked");
                if (self.HasPassthroughAllowance())
                {
                    if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                        Plugin.DebugLog("    Check 2: Creature has passthrough allowance, letting them through");
                }
                else
                {
                    if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                        Plugin.DebugLog("    Check 2: Creature does not have passthrough allowance, blocking them");
                    self.enteringShortCut = null;
                    self.inShortcut = false;
                    if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                        Plugin.DebugLog("    CREATURE SUCKED_INTO_SHORTCUT END");
                    return;
                }
            }
            else
            {
                if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                    Plugin.DebugLog("    Check 1: Pipe is not locked");
                if (self is Player player)
                {
                    if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                        Plugin.DebugLog("    Check 2: Creature is player");
                    if (player.IsTechy(out var scanline))
                    {
                        if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                            Plugin.DebugLog("    Check 3: Creature is Techy");
                        if (scanline.holdTime > timeReached)
                        {
                            if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                                Plugin.DebugLog("    Check 4: Hold time is reached, forbidding access");
                            self.enteringShortCut = null;
                            self.inShortcut = false;
                            if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                                Plugin.DebugLog("    CREATURE SUCKED_INTO_SHORTCUT END");
                            return;
                        }
                        else
                        {
                            if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                                Plugin.DebugLog("    Check 4: Hold time is not reached, aborting");
                        }
                    }
                    else
                    {
                        if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                            Plugin.DebugLog("    Check 3: Creature is not Techy, aborting");
                    }
                }
                else
                {
                    if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                        Plugin.DebugLog("    Check 2: Creature is not player, aborting");
                }
            }
        }
        else
        {
            if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
                Plugin.DebugLog("    NULL CHECK FAILED, ALLOWING CREATURE THROUGH TO PREVENT ANY EXCEPTIONS");
        }

        if (PIPE_LOCK_CREATURE_HANDLER && self.IsPipeProcessing())
            Plugin.DebugLog("    CREATURE SUCKED_INTO_SHORTCUT END");

        orig(self, entrancePos, carriedByOther);
    }

    private static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        // Even though it doesn't work with creatures already in the pipe, it does still keep creatures not yet in the pipe from passing through, so is still useful
        bool runOrigHere = true;

        DamagedShortcuts.TryGetValue(self.game, out var ShortcutTable);

        for (int i = self.transportVessels.Count - 1; i >= 0; i--)
        {
            if (self.transportVessels[i].room.realizedRoom != null)
            {
                for (int j = 0; j < ShortcutTable.locks.Count; j++)
                {
                    for (int k = 0; k < ShortcutTable.locks[j].Shortcuts.Length; k++)
                    {
                        if (self.transportVessels[i].pos == ShortcutTable.locks[j].Shortcuts[k].connection.StartTile)
                        {
                            // The fact that we are here at all means that the shortcut is in the locked list, so we don't need to check for that explicitly
                            runOrigHere = false;
                            Room realizedRoom = self.transportVessels[i].room.realizedRoom;
                            self.transportVessels[i].pos = ShortcutHandler.NextShortcutPosition(self.transportVessels[i].lastPos, self.transportVessels[i].pos, realizedRoom);
                        }
                    }
                }
            }
        }

        if (runOrigHere)
        {
            orig(self);
        }

    }

    private static void RoomCamera_MoveCamera2(On.RoomCamera.orig_MoveCamera2 orig, RoomCamera self, string roomName, int camPos)
    {
        orig(self, roomName, camPos);

        string tMaskImageFileName = $"{roomName}_{camPos + 1}_TMASK.png";

        // DO NOT REMOVE THIS
        // It will cause it to carry the mask over between screens even if that screen shouldn't have a mask
        Texture2D tMaskImage = new(1, 1);
        tMaskImage.SetPixel(0, 0, Color.black);
        tMaskImage.Apply();

        // Resolve the file path
        string filePath = AssetManager.ResolveFilePath($"world/{roomName.Split('_')[0].ToLower()}-rooms/{tMaskImageFileName}");

        // Plugin.DebugLog(filePath);

        // Check if the TMASK image file exists and load it
        if (File.Exists(filePath))
        {
            // Load the image from the file
            byte[] fileData = File.ReadAllBytes(filePath);
            tMaskImage = new Texture2D(2, 2); // Width and height are placeholders
            _ = tMaskImage.LoadImage(fileData); // LoadImage auto-resizes the texture dimensions
            fileData = null;
        }

        // Here it will be added to the shaders
        if (Null.Check(self, 4) && SlugpackShaders.TryGetValue(self.room.game.rainWorld, out var Shaders))
        {
            if (Shaders._shadowMask != null) UnityEngine.Object.Destroy(Shaders._shadowMask);
            Shaders._shadowMask = tMaskImage;

            // Above Clouds View Satellite sky mask
            if (self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.AboveCloudsView) == 1f)
            {
                Texture2D skyMaskImage = new(1, 1);
                skyMaskImage.SetPixel(0, 0, Color.red);
                skyMaskImage.Apply();

                // Resolve the file path
                string PATH = AssetManager.ResolveFilePath("illustrations/masks/atc_sky-technomancer-overlaymask.png");

                // Plugin.DebugLog(filePath);

                // Check if the TMASK image file exists and load it
                if (File.Exists(PATH))
                {
                    // Load the image from the file
                    byte[] skyFileData = File.ReadAllBytes(PATH);
                    skyMaskImage = new Texture2D(2, 2); // Width and height are placeholders
                    _ = skyMaskImage.LoadImage(skyFileData); // LoadImage auto-resizes the texture dimensions
                    skyFileData = null;
                }
                Shaders._skymask = skyMaskImage;
            }
            else
            { UnityEngine.Object.Destroy(Shaders._skymask); }
        }
    }

    private static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);
        if (ModManager.MSC && self.room.abstractRoom.name == "GATE_TL_OE")
        {
            self.karmaRequirements[0] = RegionGate.GateRequirement.ThreeKarma;
            self.karmaRequirements[1] = MoreSlugcatsEnums.GateRequirement.OELock;
        }
    }

    private static void ShortcutGraphics_Update(On.ShortcutGraphics.orig_Update orig, ShortcutGraphics self)
    {
        orig(self);

        for (int i = 0; i < self.entranceSpriteColors.Length; i++)
        {
            if (DamagedShortcuts.TryGetValue(self.room.game, out var ShortcutTable) && ShortcutTable.locks.Any(lockObj => lockObj.Shortcuts.Contains(self.room.shortcuts[i])))
            {
                self.entranceSpriteColors[i] = Custom.RGB2RGBA(new Color(0f, 0f, 0f), Mathf.Max(self.entranceSpriteColors[i].a, (float)32 / (ShortcutTable.locks.FirstOrDefault(lockObj => lockObj.Shortcuts.Contains(self.room.shortcuts[i])).SinceFlicker + 32)));
            }
        }
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!DamagedShortcuts.TryGetValue(self, out var _))
        { DamagedShortcuts.Add(self, _ = new ShortcutList()); }
    }

    private static bool RoomRealizer_CanAbstractizeRoom(On.RoomRealizer.orig_CanAbstractizeRoom orig, RoomRealizer self, RoomRealizer.RealizedRoomTracker tracker)
    {
        if (DamagedShortcuts.TryGetValue(self.world.game, out var ShortcutTable))
        {
            using List<AbstractCreature>.Enumerator enumerator = tracker.room.world.game.NonPermaDeadPlayers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                for (int i = 0; i < ShortcutTable.locks.Count; i++)
                {
                    for (int r = 0; r < 2; r++)
                    {
                        if (ShortcutTable.locks[i].Rooms[r].abstractRoom == enumerator.Current.Room)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return orig(self, tracker);
    }

    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (DamagedShortcuts.TryGetValue(self, out var ShortcutTable))
        {
            for (int i = 0; i < ShortcutTable.locks.Count; i++)
            {
                if (ShortcutTable.locks[i].Time > 0)
                {
                    for (int r = 0; r < ShortcutTable.locks[i].Shortcuts.Length; r++)
                    {
                        isLocked[ShortcutTable.locks[i].Shortcuts[r]] = true;
                    }
                    ShortcutTable.locks[i].Time--;
                    ShortcutTable.locks[i].SinceFlicker++;
                    for (int r = 0; r < 2; r++)
                    {
                        if (Random.Range(0, 20) == 0 && ShortcutTable.locks[i].Rooms[r].abstractRoom.realizedRoom != null)
                        {
                            for (int j = 0; j < Random.Range(10, 30); j++)
                            {
                                Vector2 a = Custom.RNV();
                                ShortcutTable.locks[i].Rooms[r].AddObject(new Spark(ShortcutTable.locks[i].Rooms[r].MiddleOfTile(ShortcutTable.locks[i].Shortcuts[r].StartTile) + (a * Random.value * 40f), a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 16, 30));
                            }
                            ShortcutTable.locks[i].SinceFlicker = 0;
                        }
                    }
                }
                else
                {
                    for (int r = 0; r < ShortcutTable.locks[i].Shortcuts.Length; r++)
                    {
                        isLocked[ShortcutTable.locks[i].Shortcuts[r]] = false;
                    }
                    ShortcutTable.locks.RemoveAt(i);
                    break;
                }
            }
        }
    }

    /*private static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
    {
        string text = baseAcronym;
        if (character.ToString() == Technomancer)
        {
            Dictionary<string, string> replacements = new() { { "SL", "LM" }, { "SB", "TL" } };

            if (replacements.ContainsKey(text))
            {
                text = replacements[text];
                foreach (var path in AssetManager.ListDirectory("World", true, false)
                    .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
                    .Where(File.Exists)
                    .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
                {
                    var parts = path.Contains("-") ? path.Split('-') : [path];
                    if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], StringComparison.OrdinalIgnoreCase)))
                    {
                        text = Path.GetFileName(path).ToUpper();
                        break;
                    }
                }
                return text;
            }
        }
        return orig(character, baseAcronym);
    }

    private static string Region_GetVanillaEquivalentRegionAcronym(On.Region.orig_GetVanillaEquivalentRegionAcronym orig, string baseAcronym)
    {
        if (baseAcronym == "TL")
            return "SB";
        return orig(baseAcronym);
    }

    private static bool Region_EquivalentRegion(On.Region.orig_EquivalentRegion orig, string regionA, string regionB)
    {
        if ((regionA == "SB" || regionA == "TL") && (regionB == "TL" || regionB == "SB"))
            return true;
        return orig(regionA, regionB);
    }*/
}