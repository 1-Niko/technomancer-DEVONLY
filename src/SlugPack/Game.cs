using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using MoreSlugcats;
using UnityEngine;

[module: System.Security.UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Slugpack
{
    static class GameHooks
    {
        internal static void Apply()
        {
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RoomRealizer.CanAbstractizeRoom += RoomRealizer_CanAbstractizeRoom;
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
            On.ShortcutGraphics.Update += ShortcutGraphics_Update;
            On.RoomCamera.SpriteLeaser.RemoveAllSpritesFromContainer += SpriteLeaser_RemoveAllSpritesFromContainer;
            On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;

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

        private static void RoomCamera_MoveCamera2(On.RoomCamera.orig_MoveCamera2 orig, RoomCamera self, string roomName, int camPos)
        {
            orig(self, roomName, camPos);

            string tMaskImageFileName = $"{roomName}_{camPos + 1}_TMASK.png";

            // Initialize a default 1x1 black pixel image
            Texture2D tMaskImage = new Texture2D(1, 1);
            tMaskImage.SetPixel(0, 0, Color.black);
            tMaskImage.Apply();

            // Resolve the file path
            string filePath = AssetManager.ResolveFilePath($"world/{(roomName.Split('_')[0]).ToLower()}-rooms/{tMaskImageFileName}");

            Debug.Log(filePath);

            // Check if the TMASK image file exists and load it
            if (File.Exists(filePath))
            {
                // Load the image from the file
                byte[] fileData = File.ReadAllBytes(filePath);
                tMaskImage = new Texture2D(2, 2); // Width and height are placeholders
                tMaskImage.LoadImage(fileData); // LoadImage auto-resizes the texture dimensions
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
            catch (System.NullReferenceException ex)
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
                if (Constants.DamagedShortcuts.TryGetValue(self.room.game, out var ShortcutTable) && ShortcutTable.locks.Any(lockObj => lockObj.shortcuts.Contains(self.room.shortcuts[i])))
                {
                    self.entranceSpriteColors[i] = RWCustom.Custom.RGB2RGBA(new Color(0f, 0f, 0f), Mathf.Max(self.entranceSpriteColors[i].a, 32 / (ShortcutTable.locks.FirstOrDefault(lockObj => lockObj.shortcuts.Contains(self.room.shortcuts[i])).sinceFlicker + 32)));
                }
            }
        }

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            if (!Constants.DamagedShortcuts.TryGetValue(self, out var _))
            { Constants.DamagedShortcuts.Add(self, _ = new WeakTables.ShortcutList()); }
        }

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            // Non-Train atlases
            Futile.atlasManager.LoadAtlas("atlases/slugpackatlas");
            Futile.atlasManager.LoadAtlas("atlases/hologramatlas");

            // Train atlases
            Futile.atlasManager.LoadAtlas("atlases/trainatlas");
            // Futile.atlasManager.LoadAtlas("atlases/train_accessory_top_atlas");

            if (Constants.shaders_enabled)
            {
                if (!Constants.SlugpackShaders.TryGetValue(self, out var _))
                { Constants.SlugpackShaders.Add(self, _ = new WeakTables.Shaders()); }

                if (Constants.SlugpackShaders.TryGetValue(self, out var Shaders))
                {
                    // AssetBundle SlugShaders = Utilities.LoadFromEmbeddedResource("Slugpack.slugpack");
                    Shaders.SlugShaders = Utilities.LoadFromEmbeddedResource("Slugpack.slugpack");

                    if (Shaders.SlugShaders != null)
                    {
                        // Utilities.InPlaceTryCatch(ref Shaders._shadowMask, Shaders.SlugShaders.LoadAsset<Texture2D>("Assets/TL_V01.png"), "Technomancer (SlugPack/Game.cs/%ln): Texture \"_shadowMask\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders._effectMask, Shaders.SlugShaders.LoadAsset<Texture2D>("Assets/EFFECT_MASK.png"), "Technomancer (SlugPack/Game.cs/%ln): Texture \"_effectMask\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders._RGB2HSL, Shaders.SlugShaders.LoadAsset<Texture2D>("Assets/RGB2HSL.png"), "Technomancer (SlugPack/Game.cs/%ln): Texture \"_RGB2HSL\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders._HSL2RGB, Shaders.SlugShaders.LoadAsset<Texture2D>("Assets/HSL2RGB.png"), "Technomancer (SlugPack/Game.cs/%ln): Texture \"_HSL2RGB\" Failed to set!");

                        Utilities.InPlaceTryCatch(ref Shaders.Redify, FShader.CreateShader("Redify", Shaders.SlugShaders.LoadAsset<Shader>("Assets/NoTex.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"Redify\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ShadowMask, FShader.CreateShader("ShadowMask", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ShadowMask.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ShadowMask\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.HologramA, FShader.CreateShader("RadialA", Shaders.SlugShaders.LoadAsset<Shader>("Assets/Hologram.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"HologramA\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.HologramB, FShader.CreateShader("RadialB", Shaders.SlugShaders.LoadAsset<Shader>("Assets/Hologram.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"HologramB\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.Distances, FShader.CreateShader("Distances", Shaders.SlugShaders.LoadAsset<Shader>("Assets/DistancePoints.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"Distances\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ProjectionLinesA, FShader.CreateShader("ProjetionLinesA", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ProjectionLinesA\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ProjectionLinesB, FShader.CreateShader("ProjetionLinesB", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ProjectionLinesB\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ProjectionLinesC, FShader.CreateShader("ProjetionLinesC", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ProjectionLinesC\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ProjectionLinesD, FShader.CreateShader("ProjetionLinesD", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ProjectionLinesD\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ColourChangerShader, FShader.CreateShader("ColourChangerShader", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ChangeEffectColour.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ColourChangerShader\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.SelectionShader, FShader.CreateShader("SelectionShader", Shaders.SlugShaders.LoadAsset<Shader>("Assets/SelectionObject.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"SelectionShader\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ConnectingLine, FShader.CreateShader("ConnectingLine", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ConnectingLine.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ConnectingLine\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.ModifiedLightBeamShader, FShader.CreateShader("ModifiedLightBeamShader", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ModifiedLightBeam.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"ModifiedLightBeamShader\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.DynamicTrain, FShader.CreateShader("DynamicTrainShader", Shaders.SlugShaders.LoadAsset<Shader>("Assets/dynamicTrains.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"DynamicTrain\" Failed to set!");
                        Utilities.InPlaceTryCatch(ref Shaders.SpinningFan, FShader.CreateShader("SpinningFan", Shaders.SlugShaders.LoadAsset<Shader>("Assets/fanBlade.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"SpinningFan\" Failed to set!");
                    }
                    else
                    {
                        Debug.Log("Technomancer: Error loading shaders or shader assets!");
                    }
                }
            }
            else
            {
                Debug.Log("Technomancer: Attempted to load shaders, but shaders are disabled!");
            }
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
                            if (ShortcutTable.locks[i].rooms[r].abstractRoom == enumerator.Current.Room)
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
                    if (ShortcutTable.locks[i].time > 0)
                    {
                        ShortcutTable.locks[i].time--;
                        ShortcutTable.locks[i].sinceFlicker++;
                        for (int r = 0; r < 2; r++)
                        {
                            if (Random.Range(0, 20) == 0)
                            {
                                if (ShortcutTable.locks[i].rooms[r].abstractRoom.realizedRoom != null)
                                {
                                    for (int j = 0; j < Random.Range(10, 30); j++)
                                    {
                                        Vector2 a = RWCustom.Custom.RNV();
                                        ShortcutTable.locks[i].rooms[r].AddObject(new Spark(ShortcutTable.locks[i].rooms[r].MiddleOfTile(ShortcutTable.locks[i].shortcuts[r].StartTile) + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), new Color(0.9f, 0.9f, 1f), null, 16, 30));
                                    }
                                    ShortcutTable.locks[i].sinceFlicker = 0;
                                }
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
                    var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                    if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
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
                    var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                    if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
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
}
