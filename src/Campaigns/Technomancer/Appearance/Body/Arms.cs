// Campaigns/Technomancer/Appearance/Body/Arms.cs
/* Controls Technomancer's arm sprites */

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Appearance
    {
        public static partial class Body
        {
            public class Arms
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

                    if (sLeaser.sprites[5]?.element?.name is string leftArm && leftArm.StartsWith("PlayerArm"))
                        sLeaser.sprites[5].element = Futile.atlasManager.GetElementWithName("Techy" + leftArm);

                    if (sLeaser.sprites[6]?.element?.name is string rightArm && rightArm.StartsWith("PlayerArm"))
                        sLeaser.sprites[6].element = Futile.atlasManager.GetElementWithName("Techy" + rightArm);
                }
            }
        }
    }
}