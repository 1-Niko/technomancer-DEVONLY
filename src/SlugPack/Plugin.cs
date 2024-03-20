using BepInEx;
using Fisobs.Core;
using Slugpack;
using System.Security;
using System.Security.Permissions;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SlugpackPlugin
{
    [BepInEx.BepInPlugin(_ID, nameof(SlugpackPlugin), "1.0.0")]

    public class Plugin : BaseUnityPlugin
    {
        const string _ID = "splugpack";

        public void OnEnable()
        {
            try
            {
                GameHooks.Apply();
                PlayerHooks.Apply();
                PlayerGraphicsHooks.Apply();
                HypothermiaHooks.Apply();
                CreatureHooks.Apply();
                MoonDialogue.Apply();
                InitializeObjects.Apply();
                RoomScripts.Apply();

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
            catch (System.Exception e)
            {
                throw e;
            }
        }
    }
}