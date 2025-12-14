// Campaigns/Technomancer/Appearance/Head.cs
/* Controls Technomancer's head sprites */

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Appearance
    {
        public class Head
        {
            public static void Apply()
            {
                On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            }

            private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
            {
                orig(self, sLeaser, rCam, timeStacker, camPos);

                if (OptionsMenu.furDisabled.Value || !self.IsTechy())
                    return;

                if (sLeaser.sprites[3]?.element?.name is string head && head.StartsWith("Head"))
                    sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("Techy" + head);
            }
        }
    }
}