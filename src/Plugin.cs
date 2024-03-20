using BepInEx;
using Fisobs.Core;
using Slugpack;
using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

namespace SlugpackPlugin;

[BepInPlugin(_ID, nameof(SlugpackPlugin), "1.0.0")]

public class Plugin : BaseUnityPlugin
{
    const string _ID = "splugpack";

    public bool IsInit;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;
            IsInit = true;

            GameHooks.Apply();
            PlayerGraphicsHooks.Apply();
            PlayerHooks.Apply();
            HypothermiaHooks.Apply();
            CreatureHooks.Apply();
            MoonDialogue.Apply();
            InitializeObjects.Apply();
            RoomScripts.Apply();

            //Now loading all with method
            //LoadAtlases();
            Futile.atlasManager.LoadAtlas("atlases/slugpackatlas");
            Futile.atlasManager.LoadAtlas("atlases/hologramatlas");

            //// Train atlases
            //Futile.atlasManager.LoadAtlas("atlases/trainatlas");
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

            // glizard is dead
            /*
            _ = CreatureTemplateType.PastGreen;
            LizardHooks.Apply();

            // Creature initialization
            On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
            {
                orig(self, newlyDisabledMods);
                for (var i = 0; i < newlyDisabledMods.Length; i++)
                {
                    if (newlyDisabledMods[i].id == _ID)
                    {
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.PastGreen))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.PastGreen);
                        CreatureTemplateType.UnregisterValues();
                        SandboxUnlockID.UnregisterValues();
                        break;
                    }
                }
            };
            Content.Register(new PastGreenCritob());
            */
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError(ex);
            throw new Exception("Technomancer failed to load OnModsInit!");
        }
    }

    //private void LoadAtlases()
    //{
    //    foreach (var file in AssetManager.ListDirectory("atlases"))
    //    {
    //        if (".png".Equals(Path.GetExtension(file)))
    //        {
    //            if (File.Exists(Path.ChangeExtension(file, ".txt")))
    //            {
    //                Futile.atlasManager.LoadAtlas(Path.ChangeExtension(file, null));
    //            }
    //            else
    //            {
    //                Futile.atlasManager.LoadImage(Path.ChangeExtension(file, null));
    //            }
    //        }
    //    }
    //}
}