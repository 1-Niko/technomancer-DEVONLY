using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
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
            Futile.atlasManager.LoadAtlas("atlases/slugpackatlas");
            Futile.atlasManager.LoadAtlas("atlases/trainatlas");
            Futile.atlasManager.LoadAtlas("atlases/hologramatlas");

            if (!Constants.SlugpackShaders.TryGetValue(self, out var _))
            { Constants.SlugpackShaders.Add(self, _ = new WeakTables.Shaders()); }

            if (Constants.SlugpackShaders.TryGetValue(self, out var Shaders))
            {
                Shaders.SlugShaders = Utilities.LoadFromEmbeddedResource("Slugpack.slugpack");
                Shaders._shadowMask = Shaders.SlugShaders.LoadAsset<Texture2D>("Assets/TL_V01.png");
                Shaders._effectMask = Shaders.SlugShaders.LoadAsset<Texture2D>("Assets/EFFECT_MASK.png");
                // AssetBundle SlugShaders = Utilities.LoadFromEmbeddedResource("Slugpack.slugpack");

                Shaders.Redify = FShader.CreateShader("Redify", Shaders.SlugShaders.LoadAsset<Shader>("Assets/NoTex.shader"));
                Shaders.ShadowMask = FShader.CreateShader("ShadowMask", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ShadowMask.shader"));
                Shaders.HologramA = FShader.CreateShader("RadialA", Shaders.SlugShaders.LoadAsset<Shader>("Assets/Hologram.shader"));
                Shaders.HologramB = FShader.CreateShader("RadialB", Shaders.SlugShaders.LoadAsset<Shader>("Assets/Hologram.shader"));
                Shaders.Distances = FShader.CreateShader("Distances", Shaders.SlugShaders.LoadAsset<Shader>("Assets/DistancePoints.shader"));
                Shaders.ProjectionLinesA = FShader.CreateShader("ProjetionLinesA", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader"));
                Shaders.ProjectionLinesB = FShader.CreateShader("ProjetionLinesB", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader"));
                Shaders.ProjectionLinesC = FShader.CreateShader("ProjetionLinesC", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader"));
                Shaders.ProjectionLinesD = FShader.CreateShader("ProjetionLinesD", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ProjectionLines.shader"));
                Shaders.ColourChangerShader = FShader.CreateShader("ColourChangerShader", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ChangeEffectColour.shader"));
                Shaders.SelectionShader = FShader.CreateShader("SelectionShader", Shaders.SlugShaders.LoadAsset<Shader>("Assets/SelectionObject.shader"));
                Shaders.ConnectingLine = FShader.CreateShader("ConnectingLine", Shaders.SlugShaders.LoadAsset<Shader>("Assets/ConnectingLine.shader"));
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
            return orig(character, baseAcronym);
        }
    }
}
