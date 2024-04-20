namespace Slugpack
{
    internal static class PlayerHooks
    {
        internal static void Apply()
        {
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            On.Player.checkInput += Player_checkInput;
            On.Player.Update += Player_Update;
            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.TerrainImpact += Player_TerrainImpact;
        }

        private static void Player_TerrainImpact(On.Player.orig_TerrainImpact orig, Player self, int chunk, RWCustom.IntVector2 direction, float speed, bool firstContact)
        {
            if (self.IsTechy() && Constants.ScanLineMemory.TryGetValue(self, out var scanline) && scanline.stunImmune > 0)
                return; // No dying!!!
            orig(self, chunk, direction, speed, firstContact);
        }

        private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            if (self.IsTechy())
            {
                return false;
            }
            else
            {
                return orig(self, testObj);
            }
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);
            if (self != null && Constants.ScanLineMemory.TryGetValue(self, out var scanline))
            {
                if (scanline.holdTime > Constants.timeReached)
                {
                    scanline.stunImmune = 10;

                    if (!scanline.roomControllerGenerated)
                    {
                        RoomController roomController = new(self.room, self.ShortCutColor(), scanline.arrow);
                        scanline.roomController = roomController;
                        self.room.AddObject(roomController);
                        scanline.roomControllerGenerated = true;
                    }

                    //self.mushroomCounter = 3;
                    self.mushroomEffect = RWCustom.Custom.LerpAndTick(self.mushroomEffect, 8f, 0.05f, 1f);

                    scanline.x = self.input[0].x;
                    scanline.y = self.input[0].y;

                    scanline.thrw = self.input[0].thrw;
                    scanline.jmp = self.input[0].jmp;

                    scanline.pckp = self.input[0].pckp;

                    self.input[0].x = 0;
                    self.input[0].y = 0;
                    self.input[0].jmp = false;
                    self.input[0].thrw = false;
                }
                else
                {
                    if (self != null && scanline != null)
                    {
                        if (scanline.roomControllerGenerated)
                        {
                            if (self.room == scanline.roomController.room)
                                scanline.roomController.Destroy();
                            scanline.roomController = null;
                            scanline.roomControllerGenerated = false;
                        }

                        // DebugLog("Clearing Tech Icons");
                        for (int i = 0; i < scanline.TechIcons.Count; i++)
                            self.room.RemoveObject(scanline.TechIcons[i]);
                        scanline.TechIcons.Clear();
                        for (int i = 0; i < scanline.TechConnections.Count; i++)
                            self.room.RemoveObject(scanline.TechConnections[i]);
                        scanline.TechConnections.Clear();
                        scanline.generatedIcons = false;
                        self.mushroomEffect = RWCustom.Custom.LerpAndTick(self.mushroomEffect, 0f, 0.05f, 1f);
                    }
                }
            }
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!Null.Check(self, 1) || !(self.IsTechy() || self.IsVoyager())) return; // Return if null or not either slug

            if (self.IsTechy()) goto Technomancer;
            if (self.IsVoyager()) goto Voyager;
            return;

        Technomancer:

            // Need to compact somehow
            if (!Constants.ScanLineMemory.TryGetValue(self, out var scanline)) Constants.ScanLineMemory.Add(self, scanline = new ScanLine());
            bool ShadersAreValid = Constants.SlugpackShaders.TryGetValue(self.room.game.rainWorld, out var Shaders);

            // This too
            if (Constants.shaders_enabled && ShadersAreValid)
                Shaders.position = new Vector2((self.mainBodyChunk.pos.x) / (self.room.TileWidth * 20), self.mainBodyChunk.pos.y / (self.room.TileHeight * 20));

            // Good-ish rn
            scanline.stunImmune = Mathf.Max(scanline.stunImmune - 1, 0);
            self.stun = (scanline.stunImmune < 10) ? 0 : self.stun;

            if (Null.Check(self, 2))
            {
                var (minimumTrainDistance, yHeight) = Utilities.closestTrainPosition(self);
                if (minimumTrainDistance < float.MaxValue)
                {
                    // self.room.world.game.cameras[0].microShake = Mathf.Min(Mathf.Max(0.1f, -0.01f * minimumTrainDistance + 10.1f), 4f);
                    // self.room.world.game.cameras[0].screenShake = Mathf.Min(Mathf.Max(0.1f, -0.01f * minimumTrainDistance + 10.1f), 4f) / 2;

                    // if (scanline.train_sound_timer % 40 == 0)
                    // {
                    //     self.room.PlaySound(SoundID.Screen_Shake_LOOP, self.mainBodyChunk, false, 1.5f, 0.85f);
                    // }
                    // scanline.train_sound_timer++;

                    if (minimumTrainDistance < 700f)
                    {
                        self.Blink(10);
                        float num = (Mathf.Min((-1f / 500f) * minimumTrainDistance * yHeight, 0f)) / 500f;
                        Plugin.DebugLog(num);
                        (self.graphicsModule as PlayerGraphics).tail[0].vel += RWCustom.Custom.DirVec((self.graphicsModule as PlayerGraphics).objectLooker.mostInterestingLookPoint, (self.graphicsModule as PlayerGraphics).drawPositions[1, 0]) * 5f * num;
                        (self.graphicsModule as PlayerGraphics).tail[1].vel += RWCustom.Custom.DirVec((self.graphicsModule as PlayerGraphics).objectLooker.mostInterestingLookPoint, (self.graphicsModule as PlayerGraphics).drawPositions[1, 0]) * 3f * num;
                        (self.graphicsModule as PlayerGraphics).player.aerobicLevel = Mathf.Max((self.graphicsModule as PlayerGraphics).player.aerobicLevel, Mathf.InverseLerp(0.5f, 1f, num) * 0.9f);
                    }
                }
            }

            return;

        Voyager:
            return;
        }

        private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (Constants.ScanLineMemory.TryGetValue(self, out var scanline) && scanline.holdTime > Constants.timeReached)
            {
                return;
            }
            orig(self, grasp, eu);
        }
    }
}