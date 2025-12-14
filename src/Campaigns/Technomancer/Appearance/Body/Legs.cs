// Campaigns/Technomancer/Appearance/Body/Legs.cs
/* Controls Technomancer's leg sprites */

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Appearance
    {
        public static partial class Body
        {
            public class Legs
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

                    if (sLeaser.sprites[4]?.element?.name is string leftArm && leftArm.StartsWith("Legs"))
                        sLeaser.sprites[4].element = Futile.atlasManager.GetElementWithName("Techy" + leftArm);
                }
            }
        }
    }
}