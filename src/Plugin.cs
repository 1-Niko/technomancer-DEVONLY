// Plugin.cs
/* Acts as the main entry point for the mod */

using BepInEx;
using System.IO;
using System.Linq;
using System;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Slugpack;

[BepInPlugin(_ID, "Slugpack", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    const string _ID = "nikki.slugpack";

    public bool IsInit;
    public OptionsMenu optionsMenuInstance;

    public void OnEnable()
    {
        Log.Init(base.Logger);
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        LoadAtlases();

        optionsMenuInstance = new OptionsMenu(this);
        MachineConnector.SetRegisteredOI("splugpack", optionsMenuInstance);
        MachineConnector.SetRegisteredOI("splugpack.DEVBUILD", optionsMenuInstance);

        Technomancer.Init();
        Voyager.Init();
    }

    // Will find some cleaner way to do this later
    private void LoadAtlases()
    {
        try
        {
            var pngFiles = AssetManager.ListDirectory("tn_atlases")
                                       .Where(f => Path.GetExtension(f) == ".png");

            foreach (string file in pngFiles)
            {
                string assetName = Path.ChangeExtension(file, null);
                bool hasAtlasData = File.Exists(assetName + ".txt");

                // Execute the specific logic based on file existence
                if (hasAtlasData)
                    Futile.atlasManager.LoadAtlas(assetName);
                else
                    Futile.atlasManager.LoadImage(assetName);
            }
        }
        catch (Exception ex)
        {
        }
    }
}
