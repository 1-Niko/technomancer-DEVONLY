using BepInEx;
using System.Security.Permissions;
using System.Security;
using BepInEx.Logging;
using Fisobs.Core;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace HiveQueen;

[BepInPlugin(_ID, nameof(HiveQueen), "1.0.0")]
sealed class HiveQueenPlugin : BaseUnityPlugin
{
    [AllowNull] internal static ManualLogSource logger;
    const string _ID = "nko.HiveQueen";

    public void OnEnable()
    {
        logger = Logger;
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == "nko.hivequeen")
                {
                    if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.HiveQueen))
                        MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.HiveQueen);
                    CreatureTemplateType.UnregisterValues();
                    SandboxUnlockID.UnregisterValues();
                    break;
                }
            }
        };
        Content.Register(new HiveQueenCritob());
    }

    public void OnDisable() => logger = default;
}