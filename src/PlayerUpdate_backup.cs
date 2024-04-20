//private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
//{
//    orig(self, eu);

//    if (self != null && self.room != null && self.room.game != null && self.slugcatStats.name.ToString() == Constants.Technomancer)
//    {
//        if (!Constants.ScanLineMemory.TryGetValue(self, out var scanline)) Constants.ScanLineMemory.Add(self, scanline = new ScanLine());

//        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(self.room.game.rainWorld, out var Shaders))
//            Shaders.position = new Vector2((self.mainBodyChunk.pos.x) / (self.room.TileWidth * 20), self.mainBodyChunk.pos.y / (self.room.TileHeight * 20));

//        // if (scanline.debugbool)
//        // {
//        //     self.room.AddObject(new RoomController(self.room));
//        //     scanline.debugbool = false;
//        // }

//        // Nearby Train Rumbles

//        if (scanline.stunImmune > 0)
//        {
//            if (scanline.stunImmune < 10)
//                self.stun = 0;
//            scanline.stunImmune = Mathf.Max(scanline.stunImmune - 1, 0);
//        }

//        if (self != null && self.room != null && self.room.updateList != null && self.room.updateList.Count > 0)
//        {
//            List<TrainObject> trainPositions = [];

//            for (int i = 0; i < self.room.updateList.Count; i++)
//            {
//                if (self.room.updateList[i] is TrainObject)
//                {
//                    trainPositions.Add((self.room.updateList[i] as TrainObject));
//                }
//            }

//            if (trainPositions.Count > 0)
//            {
//                float minimumTrainDistance = float.MaxValue;
//                Vector2 closestTrainPosition = Vector2.zero;

//                for (int i = 0; i < trainPositions.Count; i++)
//                {
//                    if (trainPositions[i].velocity > 50)
//                    {
//                        float checkingDistance = RWCustom.Custom.Dist(trainPositions[i].pos, self.mainBodyChunk.pos);
//                        if (checkingDistance < minimumTrainDistance)
//                        {
//                            minimumTrainDistance = checkingDistance;
//                            closestTrainPosition = trainPositions[i].pos;
//                        }
//                    }
//                }

//                if (minimumTrainDistance < float.MaxValue)
//                {
//                    self.room.world.game.cameras[0].microShake = Mathf.Min(Mathf.Max(0.1f, -0.01f * minimumTrainDistance + 10.1f), 4f);
//                    self.room.world.game.cameras[0].screenShake = Mathf.Min(Mathf.Max(0.1f, -0.01f * minimumTrainDistance + 10.1f), 4f) / 2;

//                    // if (scanline.train_sound_timer % 40 == 0)
//                    // {
//                    //     self.room.PlaySound(SoundID.Screen_Shake_LOOP, self.mainBodyChunk, false, 1.5f, 0.85f);
//                    // }
//                    // scanline.train_sound_timer++;

//                    if (RWCustom.Custom.Dist(self.mainBodyChunk.pos, closestTrainPosition) < 700f)
//                    {
//                        self.Blink(10);
//                    }
//                    float num = Mathf.Max((-1f / 500f) * minimumTrainDistance, 0f);
//                    (self.graphicsModule as PlayerGraphics).tail[0].vel += RWCustom.Custom.DirVec((self.graphicsModule as PlayerGraphics).objectLooker.mostInterestingLookPoint, (self.graphicsModule as PlayerGraphics).drawPositions[1, 0]) * 5f * num;
//                    (self.graphicsModule as PlayerGraphics).tail[1].vel += RWCustom.Custom.DirVec((self.graphicsModule as PlayerGraphics).objectLooker.mostInterestingLookPoint, (self.graphicsModule as PlayerGraphics).drawPositions[1, 0]) * 3f * num;
//                    (self.graphicsModule as PlayerGraphics).player.aerobicLevel = Mathf.Max((self.graphicsModule as PlayerGraphics).player.aerobicLevel, Mathf.InverseLerp(0.5f, 1f, num) * 0.9f);
//                }
//            }
//        }

//        // End Nearby Train Rumbles

//        if (self.submerged)
//        {
//            scanline.wetness = 1f;
//        }
//        else
//        {
//            scanline.wetness = Mathf.Max(scanline.wetness - 0.002f, 0f);
//        }

//        if (self != null && self.room != null && self.room.game != null)
//        {
//            Constants.DamagedShortcuts.TryGetValue(self.room.game, out var ShortcutTable);

//            //if (!Constants.ScanLineMemory.TryGetValue(self, out var scanline))
//            //{ Constants.ScanLineMemory.Add(self, scanline = new WeakTables.ScanLine()); }

//            if (scanline.correctVelocity > 0)
//            {
//                Plugin.DebugLog($"POINT POWER : {scanline.pointPower}");

//                for (int i = 0; i < self.bodyChunks.Length; i++)
//                {
//                    // Function points towards location of interest as expected, but seems to
//                    //    1. Increase the force by way too much
//                    //    2. Ignore the point power altogether (point straight at location of interest)
//                    // Definitely a way to fix it but im not sure at the moment
//                    self.bodyChunks[i].vel = Utilities.CalculateVector(self.bodyChunks[i].pos, scanline.pointTo, self.bodyChunks[i].vel, scanline.pointPower);
//                }
//                self.stun = 0;
//                scanline.correctVelocity--;
//            }

//            scanline.holdTime = self.input[0].pckp && (self.grasps[0] == null || self.grasps[0].grabbed is not IPlayerEdible) && (self.grasps[1] == null || self.grasps[1].grabbed is not IPlayerEdible) && Utilities.GetPositions(self.room, self.mainBodyChunk.pos, true).positions.Count > 0 ? scanline.holdTime + 1 : 0;
//            scanline.holdTime = (scanline.arrow == null && scanline.holdTime > Constants.timeReached) ? Constants.timeReached - 1 : scanline.holdTime;

//            if (scanline.arrow != null && scanline.holdTime < Constants.timeReached)
//            {
//                scanline.arrow.Destroy();
//                scanline.arrow = null;
//            }
//            else if (scanline.holdTime == Constants.timeReached)
//            {
//                var (positions, nearestObjectType, nearestCreature, nearestItem, nearestObject, nearestPosition) = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, false);

//                if (scanline.arrow == null)
//                {
//                    scanline.arrow = nearestObjectType switch
//                    {
//                        "shortcut" => new SlugArrow(nearestPosition/* + new Vector2(0f, 35f)*/, 0f, self.ShortCutColor()),
//                        "creature" => new SlugArrow(nearestPosition/* + new Vector2(0f, 35f)*/, 0f, self.ShortCutColor(), nearestCreature),
//                        "item" => new SlugArrow(nearestPosition/* + new Vector2(0f, 15f)*/, 0f, self.ShortCutColor(), nearestItem),
//                        "object" => new SlugArrow(nearestPosition/* + new Vector2(0f, 35f)*/, 0f, self.ShortCutColor(), nearestItem),
//                        _ => null
//                    };

//                    if (scanline.arrow != null)
//                    {
//                        self.room.AddObject(scanline.arrow);
//                    }
//                }
//            }

//            if (scanline.arrow != null)
//            {
//                //Check to make sure the arrow is in a valid position
//                var (positions, creatures, items, objects) = Utilities.GetEverything(self.room);

//                creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
//                items.ForEach(item => positions.Add(item.firstChunk.pos));

//                bool spotFound = false;
//                foreach (var position in positions)
//                {
//                    if (scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/ == position)
//                    {
//                        spotFound = true; break;
//                    }
//                }

//                var nearestObject = Utilities.GetPositions(self.room, scanline.arrow.pos, false);

//                if (!spotFound)
//                {
//                    scanline.arrow.creature = nearestObject.nearestObjectType == "creature" ? nearestObject.nearestCreature : null;
//                    scanline.arrow.item = nearestObject.nearestObjectType == "item" ? nearestObject.nearestItem : null;
//                    scanline.arrow._object = nearestObject.nearestObjectType == "object" ? nearestObject.nearestPlacedObject : null;
//                    if (scanline.arrow != null && scanline.arrow._object != null && scanline.arrow._object.data != null && scanline.arrow._object.data is TrainWarningBellData)
//                        scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition;// + new Vector2(0f, 35f);
//                    else
//                        scanline.arrow.pos = nearestObject.nearestPosition/* + new Vector2(0f, nearestObject.nearestObjectType == "shortcut" ? 35f : 15f)*/;
//                }
//            }

//            if (scanline.holdTime > Constants.timeReached && scanline.arrow != null)
//            {
//                List<Vector2> iconPositions = [];

//                if (scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/ == Vector2.zero || (scanline.arrow.item != null && !self.room.ViewedByAnyCamera(scanline.arrow.item.firstChunk.pos, 0f)) || (scanline.arrow.creature != null && !self.room.ViewedByAnyCamera(scanline.arrow.creature.mainBodyChunk.pos, 0f))) //&& !(scanline.arrow.creature is Vulture)))
//                {
//                    var (positions, nearestObjectType, nearestCreature, nearestItem, nearestObject, nearestPosition) = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, false);
//                    iconPositions = positions;
//                    scanline.arrow.creature = nearestObjectType == "creature" ? nearestCreature : null;
//                    scanline.arrow.item = nearestObjectType == "item" ? nearestItem : null;
//                    scanline.arrow._object = nearestObjectType == "object" ? nearestObject : null;
//                    if (scanline.arrow._object.data is TrainWarningBellData)
//                        scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition;// + new Vector2(0f, 35f);
//                    else
//                        scanline.arrow.pos = nearestPosition/* + new Vector2(0f, nearestObjectType == "shortcut" ? 35f : 15f)*/;
//                }

//                var playerGraphics = self.graphicsModule as PlayerGraphics;
//                playerGraphics.blink = 10;
//                var arrowPos = scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/;
//                var handIndex = ((arrowPos - self.mainBodyChunk.pos).x > 0) ? 1 : 0;
//                playerGraphics.LookAtPoint(arrowPos, 0f);
//                playerGraphics.hands[handIndex].absoluteHuntPos = arrowPos;
//                playerGraphics.hands[handIndex].reachingForObject = true;

//                if (scanline.x != 0 || scanline.y != 0)
//                {
//                    Vector2 searchDirection = Vector2.zero;

//                    searchDirection = new Vector2(
//                        scanline.x == 1 && !scanline.heldright ? 1 : scanline.x == -1 && !scanline.heldleft ? -1 : 0,
//                        scanline.y == 1 && !scanline.heldup ? 1 : scanline.y == -1 && !scanline.helddown ? -1 : 0
//                    );

//                    if (searchDirection != Vector2.zero)
//                    {
//                        var arrowPosition = scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/;
//                        var positions = Utilities.GetPositions(self.room, self.mainBodyChunk.pos, true).positions;

//                        var nearObject = Utilities.DetermineObjectFromPosition(
//                            Utilities.FindNearest(arrowPosition, Utilities.GetPointsInDirection(arrowPosition, searchDirection, positions), self.room),
//                            self.room
//                        );

//                        if (nearObject.nearestPosition == Vector2.zero)
//                        {
//                            nearObject = Utilities.DetermineObjectFromPosition(
//                                Utilities.FindNearest(self.mainBodyChunk.pos, Utilities.GetPointsInDirection(self.mainBodyChunk.pos, searchDirection, positions), self.room),
//                                self.room
//                            );
//                        }

//                        if (nearObject.nearestPosition != Vector2.zero && nearObject.nearestObjectType != "none")
//                        {
//                            scanline.arrow.creature = nearObject.nearestObjectType == "creature" ? nearObject.nearestCreature : null;
//                            scanline.arrow.item = nearObject.nearestObjectType == "item" ? nearObject.nearestItem : null;
//                            scanline.arrow._object = nearObject.nearestObjectType == "object" ? nearObject.nearestObject : null;
//                            if (scanline.arrow != null && scanline.arrow._object != null && scanline.arrow._object.data != null && scanline.arrow._object.data is TrainWarningBellData)
//                                scanline.arrow.pos = (scanline.arrow._object.data as TrainWarningBellData).owner.pos + (scanline.arrow._object.data as TrainWarningBellData).ArrowPosition;// + new Vector2(0f, 35f);
//                            else
//                                scanline.arrow.pos = nearObject.nearestPosition/* + new Vector2(0f, nearObject.nearestObjectType == "shortcut" ? 35f : 15f)*/;
//                        }
//                    }
//                }

//                scanline.heldup = scanline.y == 1;
//                scanline.helddown = scanline.y == -1;
//                scanline.heldleft = scanline.x == -1;
//                scanline.heldright = scanline.x == 1;

//                if (scanline.thrw && !scanline.inputHoldThrw)
//                {
//                    // DebugLog("");
//                    // DebugLog("Debug Info Here");
//                    // DebugLog($" > scanline.arrow.item {scanline.arrow.item}");
//                    // DebugLog($" > scanline.arrow.creature {scanline.arrow.creature}");
//                    // DebugLog($" > scanline.arrow._object {scanline.arrow._object}");
//                    // DebugLog($" > scanline.arrow.pos {scanline.arrow.pos}");
//                    // DebugLog("");
//                    // Object Controls
//                    if (scanline.arrow._object != null && scanline.arrow._object.type.ToString() == "TrackHologram")
//                    {
//                        float explosionRadius = 80f;

//                        for (int j = 0; j < 20; j++)
//                        {
//                            Vector2 a = RWCustom.Custom.RNV();
//                            self.room.AddObject(new Spark(scanline.arrow.pos/* - new Vector2(0f, 35f)*/ + a * Random.value * 40f, a * Mathf.Lerp(14f, 20f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                        }
//                        self.room.AddObject(new Explosion(self.room, null, scanline.arrow._object.pos, 20, explosionRadius, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
//                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow._object.pos);
//                        self.room.PlaySound(SoundID.Zapper_Zap, scanline.arrow._object.pos);
//                    }
//                    // Shortcut Locking
//                    if (scanline.arrow.creature == null && scanline.arrow.item == null)
//                    {
//                        var (nearestObjectType, nearestCreature, nearestItem, nearestShortcut, nearestObject, nearestPosition) = Utilities.DetermineObjectFromPosition(scanline.arrow.pos/* - new Vector2(0f, 35f)*/, self.room);
//                        if (nearestObjectType == "shortcut" && !ShortcutTable.locks.Any(l => l.Shortcuts.Contains(nearestShortcut)))
//                        {
//                            int newRoom = self.room.abstractRoom.connections[nearestShortcut.destNode];
//                            if (newRoom > -1)
//                            {
//                                AbstractRoom abstractRoom = self.room.world.GetAbstractRoom(newRoom);
//                                while (abstractRoom.realizedRoom == null)
//                                {
//                                    abstractRoom.RealizeRoom(self.room.world, self.room.game);
//                                }
//                                var shortcutList = abstractRoom?.realizedRoom?.shortcuts?
//                                    .Where(element => element.destNode != -1 && element.destNode < abstractRoom.connections?.Length && abstractRoom.connections[element.destNode] != -1)
//                                    .ToList() ?? [];

//                                bool success = false;

//                                if (shortcutList.Count > 0)
//                                {
//                                    var exitIndex = abstractRoom.ExitIndex(self.room.abstractRoom.index);
//                                    if (exitIndex >= 0 && exitIndex < shortcutList.Count)
//                                    {
//                                        var shortcut = shortcutList[exitIndex];

//                                        ShortcutData[] shortcutDataArray = [nearestShortcut, shortcut];
//                                        Room[] roomArray = [self.room, abstractRoom.realizedRoom];

//                                        int lockTime = 10 * 40;

//                                        LockHologram[] hologramArray = [new(self.room.MiddleOfTile(nearestShortcut.StartTile), self.ShortCutColor(), lockTime), new LockHologram(abstractRoom.realizedRoom.MiddleOfTile(shortcut.StartTile), self.ShortCutColor(), lockTime)];

//                                        ShortcutTable.locks.Add(new Lock(shortcutDataArray, roomArray, lockTime, hologramArray));

//                                        self.room.AddObject(hologramArray[0]);
//                                        abstractRoom.realizedRoom.AddObject(hologramArray[1]);

//                                        for (int j = 0; j < 20; j++)
//                                        {
//                                            Vector2 a = RWCustom.Custom.RNV();
//                                            self.room.AddObject(new Spark(scanline.arrow.pos/* - new Vector2(0f, 35f)*/ + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 16, 18));
//                                        }

//                                        success = true;
//                                    }
//                                }

//                                if (!success)
//                                {
//                                    // do failure animation

//                                    /*DebugLog("PIPE LOCK FAILURE");
//                                    for (int i = 0; i < self.room.shortcuts.Length; i++)
//                                    {
//                                        if (self.room.shortcuts[i].Equals(nearestShortcut))
//                                        {
//                                            for (int j = 0; j < 100; j++)
//                                            {
//                                                Vector2 a = RWCustom.Custom.RNV();
//                                                self.room.AddObject(new Spark(scanline.arrow.pos - new Vector2(0f, 35f) + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 16, 18));
//                                            }
//                                        }
//                                    }*/
//                                }
//                            }
//                        }
//                    }

//                    if (scanline.arrow.item is DataPearl)// && !scanline.inputHoldThrw)
//                    {
//                        scanline.slugVector = new Vector2[self.bodyChunks.Length];
//                        for (int i = 0; i < self.bodyChunks.Length; i++)
//                            scanline.slugVector[i] = self.bodyChunks[i].vel;

//                        float explosionRadius = 80f;
//                        Vector2 pearlPosition = scanline.arrow.item.firstChunk.pos;

//                        for (int j = 0; j < 20; j++)
//                        {
//                            Vector2 a = RWCustom.Custom.RNV();
//                            self.room.AddObject(new Spark((scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/) + a * Random.value * 40f, a * Mathf.Lerp(4f, 5f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                        }
//                        self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, explosionRadius, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
//                        self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

//                        scanline.arrow.item.RemoveFromRoom();
//                        self.room.RemoveObject(scanline.arrow.item);
//                        scanline.arrow.item.Destroy();

//                        scanline.arrow.item = null;
//                    }

//                    if (scanline.arrow.creature != null && scanline.arrow.creature.stun == 0)
//                    {
//                        // Creature Events
//                        if (scanline.arrow.creature is MirosBird)
//                        {
//                            self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.creature.mainBodyChunk.pos, 0.7f, 2.3f);
//                            for (int j = 0; j < 20; j++)
//                            {
//                                Vector2 a = RWCustom.Custom.RNV();
//                                self.room.AddObject(new Spark(scanline.arrow.creature.mainBodyChunk.pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                            }
//                            int temp = Random.Range(3, 5);
//                            scanline.arrow.creature.stun = (temp * 40);
//                            scanline.arrow.creature.blind = ((Random.Range(3, 5) + 1) * 40);
//                        }
//                        else if (scanline.arrow.creature is VultureGrub && (scanline.arrow.creature as VultureGrub).signalWaitCounter == 0 && (scanline.arrow.creature as VultureGrub).singalCounter == 0)
//                        {
//                            (scanline.arrow.creature as VultureGrub).InitiateSignal();
//                            for (int j = 0; j < 5; j++)
//                            {
//                                Vector2 a = RWCustom.Custom.RNV();
//                                self.room.AddObject(new Spark(scanline.arrow.creature.mainBodyChunk.pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 12f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                            }
//                        }
//                        else if (scanline.arrow.creature is Vulture)
//                        {
//                            for (int j = 0; j < 20; j++)
//                            {
//                                Vector2 a = RWCustom.Custom.RNV();
//                                self.room.AddObject(new Spark(scanline.arrow.creature.mainBodyChunk.pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                            }

//                            if (!Constants.VultureStuff.TryGetValue((scanline.arrow.creature as Vulture), out var vulturestuff))
//                            { Constants.VultureStuff.Add((scanline.arrow.creature as Vulture), vulturestuff = new VultureStuff()); }

//                            vulturestuff.timer = 60;
//                        }
//                        else if (scanline.arrow.creature is MoreSlugcats.Inspector)
//                        {
//                            (scanline.arrow.creature as MoreSlugcats.Inspector).stun = 80;
//                            (scanline.arrow.creature as MoreSlugcats.Inspector).anger = 8f;
//                        }
//                        else if (scanline.arrow.creature is Overseer)
//                        {
//                            scanline.arrow.creature.Die();
//                        }
//                    }
//                }
//                else if (scanline.jmp && !scanline.inputHoldJmp)
//                {
//                    // Shortcut Unlocking
//                    if (scanline.arrow.creature == null && scanline.arrow.item == null)
//                    {
//                        var (nearestObjectType, nearestCreature, nearestItem, nearestShortcut, nearestObject, nearestPosition) = Utilities.DetermineObjectFromPosition(scanline.arrow.pos/* - new Vector2(0f, 35f)*/, self.room);
//                        if (nearestObjectType == "shortcut" && ShortcutTable.locks.Any(l => l.Shortcuts.Contains(nearestShortcut)))
//                        {
//                            for (int i = 0; i < ShortcutTable.locks.Count; i++)
//                            {
//                                for (int r = 0; r < 2; r++)
//                                {
//                                    if (ShortcutTable.locks[i].Shortcuts[r].Equals(nearestShortcut))
//                                    {
//                                        for (int h = 0; h < 2; h++)
//                                        {
//                                            ShortcutTable.locks[i].Holograms[h].Destroy();
//                                        }

//                                        ShortcutTable.locks.RemoveAt(i);

//                                        /*for (int j = 0; j < 20; j++)
//                                        {
//                                            Vector2 a = RWCustom.Custom.RNV();
//                                            self.room.AddObject(new Spark(scanline.arrow.pos - new Vector2(0f, 35f) + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 16, 18));
//                                        }*/

//                                        break;
//                                    }
//                                }
//                            }
//                        }
//                    }

//                    if (scanline.arrow.item != null)
//                    {
//                        if (scanline.arrow.item is OverseerCarcass)
//                        {
//                            for (int j = 0; j < 20; j++)
//                            {
//                                Vector2 a = RWCustom.Custom.RNV();
//                                self.room.AddObject(new Spark((scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/) + a * Random.value * 40f, a * Mathf.Lerp(4f, 5f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                            }
//                            self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, 80f, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
//                            self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

//                            scanline.arrow.item.RemoveFromRoom();
//                            self.room.RemoveObject(scanline.arrow.item);
//                            scanline.arrow.item.Destroy();

//                            scanline.arrow.item = null;
//                        }
//                        else if (scanline.arrow.item is SSOracleSwarmer)
//                        {
//                            for (int j = 0; j < 20; j++)
//                            {
//                                Vector2 a = RWCustom.Custom.RNV();
//                                self.room.AddObject(new Spark((scanline.arrow.pos/* - new Vector2(0f, (scanline.arrow.item == null) ? 35f : 15f)*/) + a * Random.value * 40f, a * Mathf.Lerp(4f, 5f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
//                            }
//                            self.room.AddObject(new Explosion(self.room, scanline.arrow.item, scanline.arrow.item.firstChunk.pos, 20, 80f, 70f, 0.2f, 2f, 0f, null, 0f, 0.1f, 4f));
//                            self.room.PlaySound(SoundID.Bomb_Explode, scanline.arrow.item.firstChunk.pos);

//                            scanline.arrow.item.RemoveFromRoom();
//                            self.room.RemoveObject(scanline.arrow.item);
//                            scanline.arrow.item.Destroy();

//                            scanline.arrow.item = null;

//                            var (positions, creatures, items, objects) = Utilities.GetEverything(self.room);

//                            for (int i = 0; i < creatures.Count; i++)
//                            {
//                                if (creatures[i] is MoreSlugcats.Inspector)
//                                {
//                                    (creatures[i] as MoreSlugcats.Inspector).anger = 2f;
//                                }
//                            }

//                            scanline.murdered_neurons++;
//                        }
//                    }
//                    else if (scanline.arrow.creature != null)
//                    {
//                    }
//                }
//            }

//            scanline.inputHoldThrw = !scanline.thrw;
//            scanline.inputHoldJmp = !scanline.jmp;
//        }
//        Utilities.Identify(scanline.arrow);
//    }
//}