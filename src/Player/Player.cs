using UnityEngine.UIElements;

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
            if (self.IsTechy(out var scanline) && scanline.stunImmune > 0)
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
            if (!self.IsTechy(out var scanline)) return;
            if (self != null)
            {
                scanline.isHeld = false;
                if (scanline.holdTime > Constants.timeReached)
                {
                    scanline.isHeld = true;
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

            Technomancer:

            (self as Player).IsTechy(out var scanline);

            if (scanline.isHeld && !scanline.isQueued)
            {
                scanline.queueThrw = scanline.thrw;
                scanline.queueJmp = scanline.jmp;
                scanline.isQueued = true;
            }

            // Need to compact somehow
            Constants.DamagedShortcuts.TryGetValue(self.room.game, out var ShortcutTable);
            bool ShadersAreValid = Constants.SlugpackShaders.TryGetValue(self.room.game.rainWorld, out var Shaders);

            // This too
            if (Constants.shaders_enabled && ShadersAreValid)
                Shaders.position = new Vector2((self.mainBodyChunk.pos.x) / (self.room.TileWidth * 20), self.mainBodyChunk.pos.y / (self.room.TileHeight * 20));

            // Good-ish rn
            scanline.stunImmune = Mathf.Max(scanline.stunImmune - 1, 0);
            self.stun = (scanline.stunImmune < 10) ? 0 : self.stun;

            // Not done
            if (Null.Check(self, 2))
            {
                var (minimumTrainDistance, yHeight) = Utilities.ClosestTrainPosition(self);
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

            // Good
            scanline.wetness = (self.submerged) ? 1f : Mathf.Max(scanline.wetness - 0.002f, 0f);

            // Could probably use a helper function here
            scanline.holdTime = self.input[0].pckp && (self.grasps[0] == null || self.grasps[0].grabbed is not IPlayerEdible) && (self.grasps[1] == null || self.grasps[1].grabbed is not IPlayerEdible) && Utilities.GetPositions(self.room, self.mainBodyChunk.pos, true).positions.Count > 0 ? scanline.holdTime + 1 : 0;
            scanline.holdTime = (scanline.arrow == null && scanline.holdTime > Constants.timeReached) ? Constants.timeReached - 1 : scanline.holdTime;

            // Original code, needs compacting
            if (scanline.arrow != null && scanline.holdTime < Constants.timeReached)
            {
                scanline.arrow.Destroy();
                scanline.arrow = null;
            }
            else if (scanline.holdTime == Constants.timeReached)
            {
                var (positions, nearestObjectType, nearestCreature, nearestItem, nearestObject, nearestPosition) = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, false);

                if (scanline.arrow == null)
                {
                    scanline.arrow = nearestObjectType switch
                    {
                        "shortcut" => new SlugArrow(nearestPosition/* + new Vector2(0f, 35f)*/, 0f, self.ShortCutColor()),
                        "creature" => new SlugArrow(nearestPosition/* + new Vector2(0f, 35f)*/, 0f, self.ShortCutColor(), nearestCreature),
                        "item" => new SlugArrow(nearestPosition/* + new Vector2(0f, 15f)*/, 0f, self.ShortCutColor(), nearestItem),
                        "object" => new SlugArrow(nearestPosition/* + new Vector2(0f, 35f)*/, 0f, self.ShortCutColor(), nearestItem),
                        _ => null
                    };

                    if (scanline.arrow != null)
                    {
                        self.room.AddObject(scanline.arrow);
                    }
                }
            }

            // This too
            if (scanline.arrow != null)
            {
                //Check to make sure the arrow is in a valid position
                var (positions, creatures, items, objects) = Utilities.GetEverything(self.room);

                creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
                items.ForEach(item => positions.Add(item.firstChunk.pos));

                bool spotFound = false;
                foreach (var position in positions)
                {
                    if (scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/ == position)
                    {
                        spotFound = true; break;
                    }
                }

                var nearestObject = Utilities.GetPositions(self.room, scanline.arrow.pos, false);

                if (!spotFound)
                {
                    scanline.arrow.creature = nearestObject.nearestObjectType == "creature" ? nearestObject.nearestCreature : null;
                    scanline.arrow.item = nearestObject.nearestObjectType == "item" ? nearestObject.nearestItem : null;
                    scanline.arrow._object = nearestObject.nearestObjectType == "object" ? nearestObject.nearestPlacedObject : null;
                    if (scanline.arrow != null && scanline.arrow._object != null && scanline.arrow._object.data != null && scanline.arrow._object.data is TrainWarningBellData)
                    {
                        scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition; // + new Vector2(0f, 35f);
                    }
                    else
                        scanline.arrow.pos = nearestObject.nearestPosition/* + new Vector2(0f, nearestObject.nearestObjectType == "shortcut" ? 35f : 15f)*/;
                }
            }

            // Alright but can still be better
            if (!(scanline.holdTime > Constants.timeReached && scanline.arrow != null)) return;

            // A lot of this needs rewriting
            List<Vector2> iconPositions = [];
            if (scanline.arrow.pos == Vector2.zero || (scanline.arrow.item != null && !self.room.ViewedByAnyCamera(scanline.arrow.item.firstChunk.pos, 0f)) || (scanline.arrow.creature != null && !self.room.ViewedByAnyCamera(scanline.arrow.creature.mainBodyChunk.pos, 0f))) //&& !(scanline.arrow.creature is Vulture)))
            {
                var (positions, nearestObjectType, nearestCreature, nearestItem, nearestObject, nearestPosition) = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, false);
                iconPositions = positions;
                scanline.arrow.creature = nearestObjectType == "creature" ? nearestCreature : null;
                scanline.arrow.item = nearestObjectType == "item" ? nearestItem : null;
                scanline.arrow._object = nearestObjectType == "object" ? nearestObject : null;
                if (scanline.arrow._object.data is TrainWarningBellData)
                    scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition;// + new Vector2(0f, 35f);
                else
                    scanline.arrow.pos = nearestPosition/* + new Vector2(0f, nearestObjectType == "shortcut" ? 35f : 15f)*/;
            }

            if (!self.room.ViewedByAnyCamera(scanline.arrow.pos, 0f))
            {
                var (positions, nearestObjectType, nearestCreature, nearestItem, nearestObject, nearestPosition) = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, false);
                iconPositions = positions;
                scanline.arrow.creature = nearestObjectType == "creature" ? nearestCreature : null;
                scanline.arrow.item = nearestObjectType == "item" ? nearestItem : null;
                scanline.arrow._object = nearestObjectType == "object" ? nearestObject : null;
                if (scanline.arrow._object != null && scanline.arrow._object.data is TrainWarningBellData)
                    scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition;// + new Vector2(0f, 35f);
                else
                    scanline.arrow.pos = nearestPosition/* + new Vector2(0f, nearestObjectType == "shortcut" ? 35f : 15f)*/;
            }

            // I believe this is the pointing code? Will rewrite later
            var playerGraphics = self.graphicsModule as PlayerGraphics;
            playerGraphics.blink = 10;
            var arrowPos = scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/;
            var handIndex = ((arrowPos - self.mainBodyChunk.pos).x > 0) ? 1 : 0;
            playerGraphics.LookAtPoint(arrowPos, 0f);
            playerGraphics.hands[handIndex].absoluteHuntPos = arrowPos;
            playerGraphics.hands[handIndex].reachingForObject = true;

            // This is the nearest object search code by the looks of it, can be compacted
            if (scanline.x != 0 || scanline.y != 0)
            {
                Vector2 searchDirection = Vector2.zero;

                searchDirection = new Vector2(
                    scanline.x == 1 && !scanline.heldright ? 1 : scanline.x == -1 && !scanline.heldleft ? -1 : 0,
                    scanline.y == 1 && !scanline.heldup ? 1 : scanline.y == -1 && !scanline.helddown ? -1 : 0
                );

                if (searchDirection != Vector2.zero)
                {
                    var arrowPosition = scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/;
                    var positions = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, true).positions;

                    var nearObject = Utilities.DetermineObjectFromPosition(
                        Utilities.FindNearest(arrowPosition, Utilities.GetPointsInDirection(arrowPosition, searchDirection, positions), self.room),
                        self.room
                    );

                    if (nearObject.nearestPosition == Vector2.zero)
                    {
                        nearObject = Utilities.DetermineObjectFromPosition(
                            Utilities.FindNearest(self.mainBodyChunk.pos, Utilities.GetPointsInDirection(self.mainBodyChunk.pos, searchDirection, positions), self.room),
                            self.room
                        );
                    }

                    if (nearObject.nearestPosition != Vector2.zero && nearObject.nearestObjectType != "none")
                    {
                        scanline.arrow.creature = nearObject.nearestObjectType == "creature" ? nearObject.nearestCreature : null;
                        scanline.arrow.item = nearObject.nearestObjectType == "item" ? nearObject.nearestItem : null;
                        scanline.arrow._object = nearObject.nearestObjectType == "object" ? nearObject.nearestObject : null;
                        if (scanline.arrow != null && scanline.arrow._object != null && scanline.arrow._object.data != null && scanline.arrow._object.data is TrainWarningBellData)
                            scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition;// + new Vector2(0f, 35f);
                        else
                            scanline.arrow.pos = nearObject.nearestPosition/* + new Vector2(0f, nearObject.nearestObjectType == "shortcut" ? 35f : 15f)*/;
                    }
                }
            }

            scanline.heldup = scanline.y == 1;
            scanline.helddown = scanline.y == -1;
            scanline.heldleft = scanline.x == -1;
            scanline.heldright = scanline.x == 1;

            if (scanline.isQueued)
            {
                scanline.thrw = scanline.queueThrw;
                scanline.jmp = scanline.queueJmp;
                scanline.inputHoldThrw = false;
                scanline.inputHoldJmp = false;
                scanline.isQueued = false;
            }

            // Each individual case needs to be compacted, but the overall system is good
            switch (Utilities.Identify(scanline.arrow, scanline.thrw, scanline.jmp, scanline.inputHoldThrw, scanline.inputHoldJmp))
            {
                case 0: // Null condition, nothing should happen
                    { }
                    break;

                // Yellow Lizard
                case 64:
                    { }
                    break;
                case 65:
                    { }
                    break;

                // Vulture
                case 66:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.creature.mainBodyChunk.pos, 4f, 30f, 4);

                        if (!Constants.VultureStuff.TryGetValue((scanline.arrow.creature as Vulture), out var vulturestuff))
                        { Constants.VultureStuff.Add((scanline.arrow.creature as Vulture), vulturestuff = new VultureStuff()); }

                        vulturestuff.timer = 60;
                    }
                    break;
                case 67:
                    { }
                    break;

                // Brother Long Legs
                case 68:
                    { }
                    break;
                case 69:
                    { }
                    break;

                // Leviathan
                case 70:
                    { }
                    break;
                case 71:
                    { }
                    break;

                // Miros Birds
                case 72:
                    {
                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.creature.mainBodyChunk.pos, 0.7f, 2.3f);
                        Utilities.Spark(self.room, 20, scanline.arrow.creature.mainBodyChunk.pos, 4f, 30f, 4);
                        int temp = Random.Range(3, 5);
                        scanline.arrow.creature.stun = (temp * 40);
                        scanline.arrow.creature.blind = ((Random.Range(3, 5) + 1) * 40);
                    }
                    break;
                case 73:
                    { }
                    break;

                // Daddy Long Legs
                case 74:
                    { }
                    break;
                case 75:
                    { }
                    break;

                // Vulture Grubs
                case 76:
                    {
                        (scanline.arrow.creature as VultureGrub).InitiateSignal();
                        Utilities.Spark(self.room, 5, scanline.arrow.creature.mainBodyChunk.pos, 4f, 12f, 4);
                    }
                    break;
                case 77:
                    { }
                    break;

                // Cyan Lizards
                case 82:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.creature.mainBodyChunk.pos, 4f, 30f, 4);

                        self.room.PlaySound(SoundID.Cyan_Lizard_Small_Jump, scanline.arrow.creature.mainBodyChunk.pos);
                        Vector2 vector = self.room.MiddleOfTile((scanline.arrow.creature as Lizard).mainBodyChunk.pos);
                        Vector2 b = (scanline.arrow.creature as Lizard).bodyChunks[2].pos - (scanline.arrow.creature as Lizard).bodyChunks[1].pos;
                        for (int j = 0; j < (scanline.arrow.creature as Lizard).jumpModule.lizard.bodyChunks.Length; j++)
                        {
                            (scanline.arrow.creature as Lizard).jumpModule.lizard.bodyChunks[j].pos += b;
                            (scanline.arrow.creature as Lizard).jumpModule.lizard.bodyChunks[j].vel = b * 100f;
                        }
                        if ((scanline.arrow.creature as Lizard).jumpModule.smoke == null)
                        {
                            (scanline.arrow.creature as Lizard).jumpModule.smoke = new Smoke.CyanLizardSmoke((scanline.arrow.creature as Lizard).jumpModule.room);
                            (scanline.arrow.creature as Lizard).jumpModule.room.AddObject((scanline.arrow.creature as Lizard).jumpModule.smoke);
                        }
                        for (int k = 0; k < 7; k++)
                        {
                            (scanline.arrow.creature as Lizard).jumpModule.smoke.EmitSmoke(vector, -(scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.initVel * Random.value * 0.5f + RWCustom.Custom.RNV() * Mathf.Lerp(6f, 23f, (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power), (scanline.arrow.creature as Lizard).jumpModule.lizard.graphicsModule as LizardGraphics, true, Mathf.Lerp(30f, 140f, (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power));
                        }
                        for (int l = (int)((scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power * 10f * Random.value); l >= 0; l--)
                        {
                            (scanline.arrow.creature as Lizard).jumpModule.room.AddObject(new LizardBubble((scanline.arrow.creature as Lizard).jumpModule.lizard.graphicsModule as LizardGraphics, 1f, 0f, (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power * 10f));
                        }
                        (scanline.arrow.creature as Lizard).jumpModule.room.AddObject(new LizardJumpModule.JumpLight(vector - (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.initVel.normalized * 10f, (scanline.arrow.creature as Lizard).jumpModule.lizard.graphicsModule as LizardGraphics, (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power));
                        (scanline.arrow.creature as Lizard).jumpModule.room.AddObject(new ShockWave(vector - (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.initVel.normalized * 15f, Mathf.Lerp(40f, 120f, (scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power), 0.07f, 6 + (int)((scanline.arrow.creature as Lizard).jumpModule.actOnJump.bestJump.power * 4f), false));

                        (scanline.arrow.creature as Lizard).stun = 120;
                    }
                    break;
                case 83:
                    { }
                    break;

                // King Vultures
                case 84:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.creature.mainBodyChunk.pos, 4f, 30f, 4);

                        if (!Constants.VultureStuff.TryGetValue((scanline.arrow.creature as Vulture), out var vulturestuff))
                        { Constants.VultureStuff.Add((scanline.arrow.creature as Vulture), vulturestuff = new VultureStuff()); }

                        vulturestuff.timer = 60;
                    }
                    break;
                case 85:
                    { }
                    break;

                // Red Centipedes
                case 86:
                    { }
                    break;
                case 87:
                    { }
                    break;

                // Overseer
                case 88:
                    {
                        scanline.arrow.creature.Die();
                    }
                    break;
                case 89:
                    { }
                    break;

                // Neuron
                case 90:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.pos, 4f, 5f, 4);
                        self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, 80f, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

                        var (positions, creatures, items, objects) = Utilities.GetEverything(self.room);

                        for (int i = 0; i < creatures.Count; i++)
                        {
                            if (creatures[i] is MoreSlugcats.Inspector)
                            {
                                (creatures[i] as MoreSlugcats.Inspector).anger = 2f;
                            }
                        }

                        scanline.murdered_neurons++;

                        (scanline.arrow.item as SSOracleSwarmer).currentBehavior.life = 0f;
                        scanline.arrow.item.abstractPhysicalObject.destroyOnAbstraction = true;
                        scanline.arrow.item.RemoveFromRoom();
                        self.room.RemoveObject(scanline.arrow.item);
                        scanline.arrow.item.Destroy();

                        scanline.arrow.item = null;
                    }
                    break;
                case 91:
                    { }
                    break;

                // Shortcuts
                case 128:
                    {
                        var (nearestObjectType, nearestCreature, nearestItem, nearestShortcut, nearestObject, nearestPosition) = Utilities.DetermineObjectFromPosition(scanline.arrow.pos/* - new Vector2(0f, 35f)*/, self.room);
                        if (nearestObjectType == "shortcut" && !ShortcutTable.locks.Any(l => l.Shortcuts.Contains(nearestShortcut)))
                        {
                            int newRoom = self.room.abstractRoom.connections[nearestShortcut.destNode];
                            if (newRoom > -1)
                            {
                                AbstractRoom abstractRoom = self.room.world.GetAbstractRoom(newRoom);
                                while (abstractRoom.realizedRoom == null)
                                {
                                    abstractRoom.RealizeRoom(self.room.world, self.room.game);
                                }
                                var shortcutList = abstractRoom?.realizedRoom?.shortcuts?
                                    .Where(element => element.destNode != -1 && element.destNode < abstractRoom.connections?.Length && abstractRoom.connections[element.destNode] != -1)
                                    .ToList() ?? [];

                                bool success = false;

                                if (shortcutList.Count > 0)
                                {
                                    var exitIndex = abstractRoom.ExitIndex(self.room.abstractRoom.index);
                                    if (exitIndex >= 0 && exitIndex < shortcutList.Count)
                                    {
                                        var shortcut = shortcutList[exitIndex];

                                        ShortcutData[] shortcutDataArray = [nearestShortcut, shortcut];
                                        Room[] roomArray = [self.room, abstractRoom.realizedRoom];

                                        int lockTime = 10 * 40;

                                        LockHologram[] hologramArray = [new(self.room.MiddleOfTile(nearestShortcut.StartTile), self.ShortCutColor(), lockTime), new LockHologram(abstractRoom.realizedRoom.MiddleOfTile(shortcut.StartTile), self.ShortCutColor(), lockTime)];

                                        ShortcutTable.locks.Add(new Lock(shortcutDataArray, roomArray, lockTime, hologramArray));

                                        self.room.AddObject(hologramArray[0]);
                                        abstractRoom.realizedRoom.AddObject(hologramArray[1]);

                                        Utilities.Spark(self.room, 20, scanline.arrow.pos, 4f, 30f, 16);

                                        success = true;
                                    }
                                }

                                if (!success)
                                {
                                    // do failure animation
                                }
                            }
                        }
                    }
                    break;
                case 129:
                    {
                        var (nearestObjectType, nearestCreature, nearestItem, nearestShortcut, nearestObject, nearestPosition) = Utilities.DetermineObjectFromPosition(scanline.arrow.pos/* - new Vector2(0f, 35f)*/, self.room);
                        if (nearestObjectType == "shortcut" && ShortcutTable.locks.Any(l => l.Shortcuts.Contains(nearestShortcut)))
                        {
                            for (int i = 0; i < ShortcutTable.locks.Count; i++)
                            {
                                for (int r = 0; r < 2; r++)
                                {
                                    if (ShortcutTable.locks[i].Shortcuts[r].Equals(nearestShortcut))
                                    {
                                        for (int h = 0; h < 2; h++)
                                        {
                                            ShortcutTable.locks[i].Holograms[h].Destroy();
                                        }

                                        ShortcutTable.locks.RemoveAt(i);

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;

                // Gates
                case 130:
                    { }
                    break;
                case 131:
                    { }
                    break;

                // Miros Vultures
                case 192:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.creature.mainBodyChunk.pos, 4f, 30f, 4);

                        if (!Constants.VultureStuff.TryGetValue((scanline.arrow.creature as Vulture), out var vulturestuff))
                        { Constants.VultureStuff.Add((scanline.arrow.creature as Vulture), vulturestuff = new VultureStuff()); }

                        vulturestuff.timer = 60;
                    }
                    break;
                case 193:
                    { }
                    break;

                // Mother Long Legs
                case 194:
                    { }
                    break;
                case 195:
                    { }
                    break;

                // Inspectors
                case 196:
                    {
                        (scanline.arrow.creature as MoreSlugcats.Inspector).stun = 80;
                        (scanline.arrow.creature as MoreSlugcats.Inspector).anger = 8f;
                    }
                    break;
                case 197:
                    { }
                    break;

                // Pearl
                case 256:
                    {
                        scanline.slugVector = new Vector2[self.bodyChunks.Length];
                        for (int i = 0; i < self.bodyChunks.Length; i++)
                            scanline.slugVector[i] = self.bodyChunks[i].vel;

                        float explosionRadius = 135f;
                        Vector2 pearlPosition = scanline.arrow.item.firstChunk.pos;

                        Utilities.Spark(self.room, 20, scanline.arrow.pos, 4f, 5f, 4);
                        self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, explosionRadius, 150f, 2.4f, 5f, 0f, null, 0f, 0.7f, 4f));
                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

                        scanline.arrow.item.RemoveFromRoom();
                        self.room.RemoveObject(scanline.arrow.item);
                        scanline.arrow.item.Destroy();

                        scanline.arrow.item = null;
                    }
                    break;
                case 257:
                    { /*This will be the pearl hologram visualizer*/ }
                    break;

                // Overseer Eye
                case 258:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.pos, 4f, 5f, 4);
                        self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, 80f, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

                        scanline.arrow.item.RemoveFromRoom();
                        self.room.RemoveObject(scanline.arrow.item);
                        scanline.arrow.item.Destroy();

                        scanline.arrow.item = null;
                    }
                    break;
                case 259:
                    { }
                    break;

                // Singularity Bomb
                case 320:
                    { }
                    break;
                case 321:
                    { }
                    break;

                // Rarefaction Cell
                case 322:
                    { }
                    break;
                case 323:
                    { }
                    break;

                // Inspector Eye
                case 324:
                    {
                        Utilities.Spark(self.room, 20, scanline.arrow.pos, 4f, 5f, 4);
                        self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, 80f, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

                        scanline.arrow.item.RemoveFromRoom();
                        self.room.RemoveObject(scanline.arrow.item);
                        scanline.arrow.item.Destroy();

                        scanline.arrow.item = null;
                    }
                    break;
                case 325:
                    { }
                    break;

                // Train Warnings
                case 448:
                    { }
                    break;
                    { }
                case 449:
                    break;

                // Hologram Advertisements
                case 450:
                    {
                        if (!Constants.TrackHologramMessage.TryGetValue(scanline.arrow._object, out var messenger)) Constants.TrackHologramMessage.Add(scanline.arrow._object, messenger = new WeakTables.TrackHologramMessenger());

                        if (!messenger.onCooldown && !messenger.playerInteracted)
                        {
                            messenger.playerInteracted = true;

                            float explosionRadius = 180f;

                            Utilities.Spark(self.room, 20, scanline.arrow.pos + (scanline.arrow._object.data as TrackHologramData).handle[1], 14f, 20f, 4);
                            self.room.AddObject(new Explosion(self.room, null, scanline.arrow._object.pos + (scanline.arrow._object.data as TrackHologramData).handle[1], 20, explosionRadius, 380f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
                            self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow._object.pos + (scanline.arrow._object.data as TrackHologramData).handle[1]);
                            self.room.PlaySound(SoundID.Zapper_Zap, scanline.arrow._object.pos + (scanline.arrow._object.data as TrackHologramData).handle[1]);

                            // So you can actually use it to launch
                            self.stun = 0;
                        }
                    }
                    break;
                case 451:
                    { }
                    break;
            }

            scanline.inputHoldThrw = !scanline.thrw;
            scanline.inputHoldJmp = !scanline.jmp;

            return;

        Voyager:
            return;
        }

        private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (self.IsTechy(out var scanline) && scanline.holdTime > Constants.timeReached)
            {
                return;
            }
            orig(self, grasp, eu);
        }
    }
}