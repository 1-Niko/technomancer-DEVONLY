namespace Slugpack;

internal static class GameHooks
{
    internal static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RoomRealizer.CanAbstractizeRoom += RoomRealizer_CanAbstractizeRoom;
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        On.Region.EquivalentRegion += Region_EquivalentRegion;
        On.Region.GetVanillaEquivalentRegionAcronym += Region_GetVanillaEquivalentRegionAcronym;
        On.ShortcutGraphics.Update += ShortcutGraphics_Update;
        On.RoomCamera.SpriteLeaser.RemoveAllSpritesFromContainer += SpriteLeaser_RemoveAllSpritesFromContainer;
        On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;

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
        }
    }

    private static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);
        if (ModManager.MSC && self.room.abstractRoom.name == "GATE_TL_OE")
        {
            self.karmaRequirements[0] = MoreSlugcatsEnums.GateRequirement.OELock;
            self.karmaRequirements[1] = MoreSlugcatsEnums.GateRequirement.OELock;
        }
    }

    private static void SpriteLeaser_RemoveAllSpritesFromContainer(On.RoomCamera.SpriteLeaser.orig_RemoveAllSpritesFromContainer orig, RoomCamera.SpriteLeaser self)
    {
        try
        {
            orig(self);
        }
        catch (Exception ex)
        {
            if (self != null && self.sprites != null && self.sprites.Length > 0)
            {
                for (int i = 0; i < self.sprites.Length; i++)
                {
                    self.sprites[i].RemoveFromContainer();
                }
                if (self.containers != null)
                {
                    for (int j = 0; j < self.containers.Length; j++)
                    {
                        self.containers[j].RemoveFromContainer();
                    }
                }
            }
            Plugin.DebugError(ex);
            Debug.LogException(ex);
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

    private static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
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
    }
}