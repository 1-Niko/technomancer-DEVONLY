// Campaigns/Technomancer/Appearance/Body/Legs.cs
/* Controls Technomancer's leg sprites */

using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Appearance
    {
        public static partial class Body
        {
            public class Torso
            {
                public static void Apply()
                {
                    On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
                }

                private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
                {
                    orig(self, sLeaser, rCam, timeStacker, camPos);


                    float num = 0.5f + (0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * 3.1415927f * 2f));
                    float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(RWCustom.Custom.DirVec(Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker), Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker)).y));

                    sLeaser.sprites[0].scaleX = 0.86f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, self.malnourished), 0.05f, num) * num2, 0.15f, (self.owner as Player).sleepCurlUp);
                    sLeaser.sprites[1].scaleX = 0.83f + ((self.owner as Player).sleepCurlUp * 0.2f) + (0.05f * num) - (0.05f * self.malnourished);


                    if (OptionsMenu.furDisabled.Value || !self.IsTechy())
                        return;

                    if (sLeaser.sprites[0]?.element?.name is string body && body.StartsWith("Body"))
                        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("Techy" + body);

                    if (sLeaser.sprites[1]?.element?.name is string hips && hips.StartsWith("Hips"))
                        sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("Techy" + hips);
                }
            }
        }
    }
}