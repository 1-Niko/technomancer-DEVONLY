using static Pom.Pom;

namespace Slugpack
{
    public class ColourObjectData(PlacedObject owner) : ManagedData(owner, null)
    {
        [IntegerField("A", 0, 255, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "R")]
        public int R;
        [IntegerField("B", 0, 255, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "G")]
        public int G;
        [IntegerField("C", 0, 255, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "B")]
        public int B;
        [IntegerField("D", 0, 255, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "A")]
        public int A;
        [IntegerField("E", 0, 255, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "I")]
        public int I;

        [BooleanField("F", false, ManagedFieldWithPanel.ControlType.button, displayName: "Screenshot")]
        public bool F;
    }

    public class ColourObject(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (hologramSprite == null)
            {
                hologramSprite = new ColourObjectObject(placedObject, placedObject.pos);
                room.AddObject(hologramSprite);
            }
            hologramSprite.r = (placedObject.data as ColourObjectData).R;
            hologramSprite.g = (placedObject.data as ColourObjectData).G;
            hologramSprite.b = (placedObject.data as ColourObjectData).B;
            hologramSprite.a = (placedObject.data as ColourObjectData).A;
            hologramSprite.i = (placedObject.data as ColourObjectData).I;
            hologramSprite.o = (placedObject.data as ColourObjectData).F;
        }
        private ColourObjectObject hologramSprite;
    }

    public class ColourObjectObject(PlacedObject placedObject, Vector2 pos) : CosmeticSprite
    {
        public static string PadInt(int number)
        {
            return number.ToString("D3");
        }


        private readonly PlacedObject placedObject = placedObject;

        public int counterA = 0;

        public override void Update(bool eu)
        {
            base.Update(eu);
            try
            {
                o = (placedObject.data as ColourObjectData).F;

                if (o && !finished)
                {
                    var (successful, cropped) = CropTexture(ScreenCapture.CaptureScreenshotAsTexture(), 748, 400, 256, 256);

                    if (successful)
                    {
                        File.WriteAllBytes($"C:\\Niko\\Desktop\\shader\\data\\{PadInt(r)}_{PadInt(g)}_{PadInt(b)}_{PadInt(a)}_{PadInt(i)}.png", cropped.EncodeToPNG());
                        step();
                    }
                }
            }
            catch (Exception e) { (placedObject.data as ColourObjectData).F = false; }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            try
            {
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
                sLeaser.sprites[0].SetPosition(placedObject.pos + new Vector2(0f, 360f) - rCam.pos);
                sLeaser.sprites[1].SetPosition(placedObject.pos + new Vector2(0f, 360f) - rCam.pos);
                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("colourExtractor");
                sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("pixel");
                sLeaser.sprites[1].scale = 300;
                sLeaser.sprites[1].alpha = (float)(a / 256f);
                sLeaser.sprites[0].scale = 1f;
                sLeaser.sprites[0].alpha = 1f;
                sLeaser.sprites[0].color = new UnityEngine.Color(1f, 1f, (float)(i / 256f));
                sLeaser.sprites[1].color = new Color((float)(r / 256f), (float)(g / 256f), (float)(b / 256f));
            }
            catch (Exception e) { (placedObject.data as ColourObjectData).F = false; }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];

            sLeaser.sprites[0] = new FSprite("colourExtractor", true);
            sLeaser.sprites[1] = new FSprite("pixel", true);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Foreground");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
        public static (bool, Texture2D) CropTexture(Texture2D source, int x, int y, int width, int height)
        {
            try
            {
                // Create a new texture with the specified width and height.
                Texture2D result = new Texture2D(width, height);
                // Get the pixels from the source texture and apply them to the new texture.
                Color[] pixels = source.GetPixels(x, y, width, height);
                result.SetPixels(pixels);
                // Apply all SetPixel calls.
                result.Apply();
                return (true, result);
            }
            catch (Exception e) { return (false, null); }
        }

        public void step()
        {
            try
            {
                i++;
                if (i == 256)
                { i = 0; a++; }
                if (a == 256)
                { a = 0; b++; }
                if (b == 256)
                { b = 0; g++; }
                if (g == 256)
                { g = 0; r++; }
                if (r == 256)
                { finished = true; }

            (placedObject.data as ColourObjectData).R = r;
                (placedObject.data as ColourObjectData).G = g;
                (placedObject.data as ColourObjectData).B = b;
                (placedObject.data as ColourObjectData).A = a;
                (placedObject.data as ColourObjectData).I = i;
            }
            catch (Exception e) { }
        }

        public int r;
        public int g;
        public int b;
        public int a;
        public int i;

        public bool o;

        public bool finished = false;
        public bool runThisFrame = false;
    }
}