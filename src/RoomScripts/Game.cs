using MoreSlugcatsEnums = MoreSlugcats.MoreSlugcatsEnums;

namespace Slugpack;

internal static class GameHooks
{
    internal static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RoomRealizer.CanAbstractizeRoom += RoomRealizer_CanAbstractizeRoom;
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        On.ShortcutGraphics.Update += ShortcutGraphics_Update;
        On.RoomCamera.SpriteLeaser.RemoveAllSpritesFromContainer += SpriteLeaser_RemoveAllSpritesFromContainer;
        On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;

        On.ShortcutHandler.Update += ShortcutHandler_Update;

        On.RoomCamera.MoveCamera2 += RoomCamera_MoveCamera2;

        On.RoomCamera.MoveCamera2 += (orig, self, roomName, camPos) =>
        {
            orig(self, roomName, camPos);
            if (Constants.DamagedShortcuts.TryGetValue(self.game, out var CameraPosition))
            {
                CameraPosition.camPosition = camPos;
            }
        };
    }

    private static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        // I might be able to do something that sets it so if the next position is a locked pipe, it just sends them back?
        bool runOrigHere = true;

        Constants.DamagedShortcuts.TryGetValue(self.game, out var ShortcutTable);

        for (int i = self.transportVessels.Count - 1; i >= 0; i--)
        {
            if (self.transportVessels[i].room.realizedRoom != null)
            {
                for (int j = 0; j < ShortcutTable.locks.Count; i++)
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

        // Initialize a default 1x1 black pixel image
        Texture2D tMaskImage = new(1, 1);
        tMaskImage.SetPixel(0, 0, Color.black);
        tMaskImage.Apply();

        // Resolve the file path
        string filePath = AssetManager.ResolveFilePath($"world/{roomName.Split('_')[0].ToLower()}-rooms/{tMaskImageFileName}");

        DebugLog(filePath);

        // Check if the TMASK image file exists and load it
        if (File.Exists(filePath))
        {
            // Load the image from the file
            byte[] fileData = File.ReadAllBytes(filePath);
            tMaskImage = new Texture2D(2, 2); // Width and height are placeholders
            _ = tMaskImage.LoadImage(fileData); // LoadImage auto-resizes the texture dimensions
        }

        // Here it will be added to the shaders

        if (self != null && self.room != null && self.room.game != null && self.room.game.rainWorld != null && Constants.SlugpackShaders.TryGetValue(self.room.game.rainWorld, out var Shaders))
        {
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
        catch
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
        }
    }

    private static void ShortcutGraphics_Update(On.ShortcutGraphics.orig_Update orig, ShortcutGraphics self)
    {
        orig(self);

        for (int i = 0; i < self.entranceSpriteColors.Length; i++)
        {
            if (Constants.DamagedShortcuts.TryGetValue(self.room.game, out var ShortcutTable) && ShortcutTable.locks.Any(lockObj => lockObj.Shortcuts.Contains(self.room.shortcuts[i])))
            {
                self.entranceSpriteColors[i] = RWCustom.Custom.RGB2RGBA(new Color(0f, 0f, 0f), Mathf.Max(self.entranceSpriteColors[i].a, (float)32 / (ShortcutTable.locks.FirstOrDefault(lockObj => lockObj.Shortcuts.Contains(self.room.shortcuts[i])).SinceFlicker + 32)));
            }
        }
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!Constants.DamagedShortcuts.TryGetValue(self, out var _))
        { Constants.DamagedShortcuts.Add(self, _ = new ShortcutList()); }
    }

    private static bool RoomRealizer_CanAbstractizeRoom(On.RoomRealizer.orig_CanAbstractizeRoom orig, RoomRealizer self, RoomRealizer.RealizedRoomTracker tracker)
    {
        if (Constants.DamagedShortcuts.TryGetValue(self.world.game, out var ShortcutTable))
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
        if (Constants.DamagedShortcuts.TryGetValue(self, out var ShortcutTable))
        {
            for (int i = 0; i < ShortcutTable.locks.Count; i++)
            {
                if (ShortcutTable.locks[i].Time > 0)
                {
                    ShortcutTable.locks[i].Time--;
                    ShortcutTable.locks[i].SinceFlicker++;
                    for (int r = 0; r < 2; r++)
                    {
                        if (Random.Range(0, 20) == 0 && ShortcutTable.locks[i].Rooms[r].abstractRoom.realizedRoom != null)
                        {
                            for (int j = 0; j < Random.Range(10, 30); j++)
                            {
                                Vector2 a = RWCustom.Custom.RNV();
                                ShortcutTable.locks[i].Rooms[r].AddObject(new Spark(ShortcutTable.locks[i].Rooms[r].MiddleOfTile(ShortcutTable.locks[i].Shortcuts[r].StartTile) + (a * Random.value * 40f), a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 16, 30));
                            }
                            ShortcutTable.locks[i].SinceFlicker = 0;
                        }
                    }
                }
                else
                {
                    ShortcutTable.locks.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
    {
        string text = baseAcronym;
        if (character.ToString() == Constants.Technomancer && text == "SL")
        {
            text = "LM";
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
        if (character.ToString() == Constants.Technomancer && text == "SB")
        {
            text = "TL";
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
        return orig(character, baseAcronym);
    }
}