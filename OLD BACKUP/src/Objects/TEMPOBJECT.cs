using System.Security.Cryptography;
using static Pom.Pom;

// Even though this is technically useless now, some of the functions still show how to do useful things, so don't remove

/*namespace Slugpack
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
            if (prevR != (placedObject.data as ColourObjectData).R)
            {
                hologramSprite.r = (placedObject.data as ColourObjectData).R;
                prevR = (placedObject.data as ColourObjectData).R;
            }
            if (prevG != (placedObject.data as ColourObjectData).G)
            {
                hologramSprite.g = (placedObject.data as ColourObjectData).G;
                prevG = (placedObject.data as ColourObjectData).G;
            }
            if (prevB != (placedObject.data as ColourObjectData).B)
            {
                hologramSprite.b = (placedObject.data as ColourObjectData).B;
                prevB = (placedObject.data as ColourObjectData).B;
            }
            if (prevA != (placedObject.data as ColourObjectData).A)
            {
                hologramSprite.a = (placedObject.data as ColourObjectData).A;
                prevA = (placedObject.data as ColourObjectData).A;
            }
            if (prevI != (placedObject.data as ColourObjectData).I)
            {
                hologramSprite.i = (placedObject.data as ColourObjectData).I;
                prevI = (placedObject.data as ColourObjectData).I;
            }
            if (prevO != (placedObject.data as ColourObjectData).F)
            {
                hologramSprite.o = (placedObject.data as ColourObjectData).F;
                prevO = (placedObject.data as ColourObjectData).F;
            }
        }
        private ColourObjectObject hologramSprite;
        public int prevR;
        public int prevG;
        public int prevB;
        public int prevA;
        public int prevI;
        public bool prevO;
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

                stepStopper++;

                if (o && !finished)// && stepStopper % 1 == 0)
                {
                    Texture2D screencap = ScreenCapture.CaptureScreenshotAsTexture();

                    var (successful, cropped) = CropTexture(screencap, 748, 400, 256, 256);

                    byte[] encoded = cropped.EncodeToPNG();

                    if (successful)
                    {
                        step();

                        File.WriteAllBytes($"C:\\Niko\\Desktop\\shader\\data\\{PadInt(r)}_{PadInt(g)}_{PadInt(b)}_{PadInt(a)}_{PadInt(i)}.png", encoded);
                        UnityEngine.Object.Destroy(cropped); // Adding this line will properly dispose of the Texture2D.
                        UnityEngine.Object.Destroy(screencap); // Adding this line will properly dispose of the Texture2D.
                        cropped = null;
                        screencap = null;
                        encoded = null;
                        // (placedObject.data as ColourObjectData).F = false;

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
                sLeaser.sprites[1].alpha = (a / 255f);
                sLeaser.sprites[0].scale = 1f;
                sLeaser.sprites[0].alpha = 1f;
                sLeaser.sprites[0].color = new UnityEngine.Color(1f, 1f, i / 255f);
                sLeaser.sprites[1].color = new Color(r / 255f, g / 255f, b / 255f);
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

                // result = null;
                pixels = null;

                return (true, result);
            }
            catch (Exception e) { return (false, null); }
        }

        public void step()
        {
            try
            {
                i = Random.RandomRange(0,255);
                a = Random.RandomRange(0,255);
                b = Random.RandomRange(0,255);
                g = Random.RandomRange(0,255);
                r = Random.RandomRange(0,255);

                /*i++;
                if (i == 256)
                { i = 0; a++; }
                if (a == 256)
                { a = 0; b++; }
                if (b == 256)
                { b = 0; g++; }
                if (g == 256)
                { g = 0; r++; }
                if (r == 256)
                { finished = true; }*

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
        public bool runThisFrame = true;

        public int stepStopper = 0;
    }
}*/