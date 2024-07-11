using Menu.Remix.MixedUI;

namespace Slugpack;

[BepInPlugin(MOD_ID, MOD_NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_NAME = "SlugpackPlugin";
    public const string MOD_ID = "splugpack";
    public const string VERSION = "1.0.1";

    public bool IsInit;
    private OptionsMenu optionsMenuInstance;

    public static void DebugWarning(object ex) => Logger.LogWarning(ex);

    public static void DebugError(object ex) => Logger.LogError(ex);

    public static void DebugLog(object ex) => Logger.LogInfo(ex);

    public static new ManualLogSource Logger;

    public void OnEnable()
    {
        try
        {
            Logger = base.Logger;
            DebugWarning("Technomancer is loading...");

            ApplyCreatures();

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }
        catch (Exception ex)
        {
            DebugError(ex);
            Debug.LogException(ex);
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;
            IsInit = true;

            TnEnums.Init();

            GameHooks.Apply();
            PlayerGraphicsHooks.Apply();
            PlayerHooks.Apply();
            HypothermiaHooks.Apply();
            CreatureHooks.Apply();
            MoonDialogue.Apply();
            InitializeObjects.Apply();
            RoomScripts.Apply();

            LoadAtlases();

            if (Constants.shaders_enabled)
            {
                if (!Constants.SlugpackShaders.TryGetValue(self, out var _))
                { Constants.SlugpackShaders.Add(self, _ = new Shaders()); }

                if (Constants.SlugpackShaders.TryGetValue(self, out var Shaders))
                {
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
                        Utilities.InPlaceTryCatch(ref Shaders.CustomCustomDepth, FShader.CreateShader("CustomCustomDepth", Shaders.SlugShaders.LoadAsset<Shader>("Assets/CustomCustomDepth.shader")), "Technomancer (SlugPack/Game.cs/%ln): Shader \"CustomCustomDepth\" Failed to set!");
                    }
                    else
                    {
                        DebugLog("Technomancer: Error loading shaders or shader assets!");
                    }
                }
            }

            optionsMenuInstance = new OptionsMenu(this);
            try
            {
                MachineConnector.SetRegisteredOI("REMIX MENU TEMPLATE TEST", optionsMenuInstance);
            }
            catch (Exception ex)
            {
                Debug.Log($"Remix Menu Template examples: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
            }

            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            DebugError(ex);
            DebugError("Technomancer failed to load OnModsInit!");
        }
    }

    private void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (var i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == MOD_ID)
            {
                DebugWarning($"Unregistering Creatures from {MOD_ID}");
                TnEnums.Unregister();
                break;
            }
        }
    }

    private void ApplyCreatures()
    {
        DebugWarning("Loading Creatures from Technomancer");

        HiveQueenHooks.Apply();
        PastGreenHooks.Apply();
        CaveLeechHooks.Apply();

        Content.Register(
            new CaveLeechCritob(),
            new HiveQueenCritob(),
            new PastGreenCritob());
    }

    private void LoadAtlases()
    {
        try
        {
            foreach (string file in from file in AssetManager.ListDirectory("tn_atlases")
                                    where Path.GetExtension(file).Equals(".png")
                                    select file)
            {
                _ = File.Exists(Path.ChangeExtension(file, ".txt"))
                    ? Futile.atlasManager.LoadAtlas(Path.ChangeExtension(file, null))
                    : Futile.atlasManager.LoadImage(Path.ChangeExtension(file, null));
            }
        }
        catch (Exception ex)
        {
            DebugError(ex);
            throw new Exception($"Failed to load {MOD_NAME} atlases!");
        }
    }
}

public class OptionsMenu : OptionInterface
{
    public OptionsMenu(Plugin plugin)
    {
        furToggle = this.config.Bind<bool>("Slugpack_Bool_Checkbox", true); // All of these are where the game saves your settings, the important part is the "Key" field, make sure this will never conflict with another mod's key by having a prefix, like your mod name
    }
    public override void Initialize()
    {
        var opTab1 = new OpTab(this, "Default Canvas");
        this.Tabs = new[] { opTab1 }; // Add the tabs into your list of tabs. If there is only a single tab, it will not show the flap on the side because there is not need to.

        // Tab 1
        OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
        opTab1.AddItems(tab1Container);
        for (int i = 0; i <= 600; i += 10) // Line grid to help align things, don't leave this in your final code. Almost every element starts from bottom-left.
        {
            Color c;
            c = Color.grey;
            if (i % 50 == 0) { c = Color.yellow; }
            if (i % 100 == 0) { c = Color.red; }
            FSprite lineSprite = new FSprite("pixel");
            lineSprite.color = c;
            lineSprite.alpha = 0.2f;
            lineSprite.SetAnchor(new Vector2(0.5f, 0f));
            Vector2 a = new Vector2(i, 0);
            lineSprite.SetPosition(a);
            Vector2 b = new Vector2(i, 600);
            float rot = Custom.VecToDeg(Custom.DirVec(a, b));
            lineSprite.rotation = rot;
            lineSprite.scaleX = 2f;
            lineSprite.scaleY = Custom.Dist(a, b);
            tab1Container.container.AddChild(lineSprite);
            a = new Vector2(0, i);
            b = new Vector2(600, i);
            lineSprite = new FSprite("pixel");
            lineSprite.color = c;
            lineSprite.alpha = 0.2f;
            lineSprite.SetAnchor(new Vector2(0.5f, 0f));
            lineSprite.SetPosition(a);
            rot = Custom.VecToDeg(Custom.DirVec(a, b));
            lineSprite.rotation = rot;
            lineSprite.scaleX = 2f;
            lineSprite.scaleY = Custom.Dist(a, b);
            tab1Container.container.AddChild(lineSprite);
        }

        UIelement[] UIArrayElements2 = new UIelement[] //create an array of ui elements
        {
                new OpLabel(0f, 550f, "Awri Lynn's Remix Menu Template Example", true),
                new OpCheckBox(furToggle, 50, 500),
        };
        opTab1.AddItems(UIArrayElements2);
    }
    public override void Update()
    {
        base.Update();
    }

    // Configurable values. They are bound to the config in constructor, and then passed to UI elements.
    // They will contain values set in the menu. And to fetch them in your code use their NAME.Value. For example to get the boolean testCheckBox.Value, to get the integer testSlider.Value
    //public readonly Configurable<TYPE> NAME;        
    public readonly Configurable<bool> furToggle;
}
