// Campaigns/Technomancer/Appearance/Tail.cs
/* Controls Technomancer's tail sprites */

using System.Collections.Generic;
using System.Linq;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Appearance
    {
        public class Tail
        {
            public static void Apply()
            {
                On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            }

            private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                orig(self, sLeaser, rCam);

                self.tail = new TailSegment[4];
                self.tail[0] = new TailSegment(self, 5.5f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 3.7f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 2.3f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 1f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);

                List<BodyPart> list = Enumerable.ToList(self.bodyParts);
                _ = list.RemoveAll((BodyPart x) => x is TailSegment);
                list.AddRange(self.tail);
                self.bodyParts = [.. list];
            }
        }
    }
}