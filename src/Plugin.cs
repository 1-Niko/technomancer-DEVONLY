using BepInEx;
using Slugpack;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

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
            GameHooks.Apply();
            PlayerHooks.Apply();
            PlayerGraphicsHooks.Apply();
            HypothermiaHooks.Apply();
            CreatureHooks.Apply();
            MoonDialogue.Apply();
            InitializeObjects.Apply();
            // ScreenAnimation.Apply();
        }
    }
}