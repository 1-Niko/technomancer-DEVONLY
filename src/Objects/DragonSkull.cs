using UnityEngine;
using static Pom.Pom;

namespace Slugpack
{
    public class DragonSkullData : ManagedData
    {
        [FloatField("ColourRed", 0, 1, 0, 0f, ManagedFieldWithPanel.ControlType.slider, "R")]
        public float red;

        [FloatField("ColourGreen", 0, 1, 0, 1f, ManagedFieldWithPanel.ControlType.slider, "G")]
        public float green;

        [FloatField("ColourBlue", 0, 1, 0, 0f, ManagedFieldWithPanel.ControlType.slider, "B")]
        public float blue;

        [IntegerField("Depth", 0, 30, 25, ManagedFieldWithPanel.ControlType.slider, "Depth")]
        public int depth;

        [FloatField("DepthA", 0, 1, 0, 0f, ManagedFieldWithPanel.ControlType.slider, "TeethShade")]
        public float teethColour;

        [IntegerField("Rotation", 0, 360, 0, ManagedFieldWithPanel.ControlType.slider, "Rotation")]
        public int rotation;

        [IntegerField("RotationZ", 0, 360, 0, ManagedFieldWithPanel.ControlType.slider, "Jaw Rotation")]
        public int jawRotation;

        [IntegerField("ZZZ_A", 0, 2, 0, ManagedFieldWithPanel.ControlType.arrows, "Skull")]
        public int skull;

        [IntegerField("ZZZ_B", 0, 1, 0, ManagedFieldWithPanel.ControlType.arrows, "Jaw")]
        public int jaw;

        [IntegerField("ZZZ_C", 0, 6, 0, ManagedFieldWithPanel.ControlType.arrows, "Teeth")]
        public int teeth; // 0 is none

        [FloatField("DepthColourOffset", 0, 1, 0, 1, ManagedFieldWithPanel.ControlType.slider, "Colour Depth")]
        public float colOffset;

        [BooleanField("Flip", false, ManagedFieldWithPanel.ControlType.button, "Mirrored")]
        public bool flipped;

        public DragonSkullData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class DragonSkull : UpdatableAndDeletable
    {
        public DragonSkull(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.DragonsSkull == null)
            {
                this.DragonsSkull = new SkullOfTheDragon(placedObject, placedObject.pos);
                this.room.AddObject(this.DragonsSkull);
            }
            this.DragonsSkull.depth = (this.placedObject.data as DragonSkullData).depth;
            this.DragonsSkull.colour = new Color((this.placedObject.data as DragonSkullData).red, (this.placedObject.data as DragonSkullData).green, (this.placedObject.data as DragonSkullData).blue);
            this.DragonsSkull.rotation = (this.placedObject.data as DragonSkullData).rotation;
            this.DragonsSkull.jawRotation = (this.placedObject.data as DragonSkullData).jawRotation;
            this.DragonsSkull.colourOffset = (this.placedObject.data as DragonSkullData).colOffset;
            this.DragonsSkull.flipped = (this.placedObject.data as DragonSkullData).flipped;
            this.DragonsSkull.skull = (this.placedObject.data as DragonSkullData).skull;
            this.DragonsSkull.jaw = (this.placedObject.data as DragonSkullData).jaw;
            this.DragonsSkull.teeth = (this.placedObject.data as DragonSkullData).teeth;
            this.DragonsSkull.teethShade = (this.placedObject.data as DragonSkullData).teethColour;
            this.DragonsSkull.pos = this.placedObject.pos;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        SkullOfTheDragon DragonsSkull;

        PlacedObject placedObject;
    }

    public class SkullOfTheDragon : CosmeticSprite
    {
        private readonly PlacedObject placedObject;

        public SkullOfTheDragon(PlacedObject placedObject, Vector2 pos)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (flipped)
            {
                sLeaser.sprites[0].scaleX = -1f;
                sLeaser.sprites[1].scaleX = -1f;
                sLeaser.sprites[2].scaleX = 1f;
                sLeaser.sprites[3].scaleX = -1f;
            }
            else
            {
                sLeaser.sprites[0].scaleX = 1f;
                sLeaser.sprites[1].scaleX = 1f;
                sLeaser.sprites[2].scaleX = -1f;
                sLeaser.sprites[3].scaleX = 1f;
            }

            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"DragonSkull_{skull}");
            sLeaser.sprites[0].SetPosition(this.pos - rCam.pos);
            sLeaser.sprites[0].isVisible = true;
            sLeaser.sprites[0].alpha = depth / 30f;
            sLeaser.sprites[0].color = Utilities.ColourLerp(colour, new Color(0.149f, 0.121f, 0.098f), colourOffset);
            sLeaser.sprites[0].rotation = sLeaser.sprites[1].scaleX * rotation;

            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName($"Jaw_{jaw}");
            sLeaser.sprites[1].SetPosition(Utilities.RotateAroundPoint(this.pos - rCam.pos, new Vector2(sLeaser.sprites[1].scaleX * 17f, -5f), sLeaser.sprites[1].scaleX * -rotation));
            sLeaser.sprites[1].isVisible = true;
            sLeaser.sprites[1].alpha = depth / 30f;
            sLeaser.sprites[1].color = Utilities.ColourLerp(colour, new Color(0.149f, 0.121f, 0.098f), colourOffset);
            sLeaser.sprites[1].rotation = sLeaser.sprites[1].scaleX * (-jawRotation + rotation);

            if (teeth != 0)
            {
                sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName($"Teeth_{teeth - 1}a");
                sLeaser.sprites[2].SetPosition(Utilities.RotateAroundPoint(this.pos - rCam.pos, new Vector2(sLeaser.sprites[1].scaleX * ((teeth == 6) ? -8f : -9f), ((teeth == 2) ? -9f : (teeth != 4) ? -10f : -11) + ((skull == 2) ? 3f : 0f)), sLeaser.sprites[1].scaleX * -rotation));
                sLeaser.sprites[2].isVisible = true;
                sLeaser.sprites[2].alpha = depth / 30f;
                sLeaser.sprites[2].color = Utilities.ColourLerp(Utilities.ColourLerp(colour, new Color(0f, 0f, 0f), teethShade), new Color(0.149f, 0.121f, 0.098f), colourOffset);
                sLeaser.sprites[2].rotation = sLeaser.sprites[1].scaleX * (rotation + 180);

                sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName($"Teeth_{teeth - 1}a");
                sLeaser.sprites[3].SetPosition(Utilities.RotateAroundPoint(sLeaser.sprites[1].GetPosition(), new Vector2(sLeaser.sprites[1].scaleX * -25f, ((teeth == 2) ? 0f : (teeth != 4) ? 1f : 2f) - 1f), sLeaser.sprites[1].scaleX * (-rotation + jawRotation)));
                sLeaser.sprites[3].isVisible = true;
                sLeaser.sprites[3].alpha = depth / 30f;
                sLeaser.sprites[3].color = Utilities.ColourLerp(Utilities.ColourLerp(colour, new Color(0f, 0f, 0f), teethShade), new Color(0.149f, 0.121f, 0.098f), colourOffset);
                sLeaser.sprites[3].rotation = sLeaser.sprites[1].scaleX * (-jawRotation + rotation);
            }
            else
            {
                sLeaser.sprites[2].isVisible = false;
                sLeaser.sprites[3].isVisible = false;
            }

            if (jaw == 0)
            {
                sLeaser.sprites[1].anchorX = 41f / 44f;
                sLeaser.sprites[1].anchorY = 8f / 10f;
            }
            else
            {
                sLeaser.sprites[1].anchorX = 37f / 42f;
                sLeaser.sprites[1].anchorY = 6f / 14f;
            }

            if (teeth == 0)
                sLeaser.sprites[2].isVisible = false;

            if (Constants.shaders_enabled)
                if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
                {
                    sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
                    sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
                    sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
                    sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
                }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];

            sLeaser.sprites[0] = new FSprite($"DragonSkull_{skull}", true);
            sLeaser.sprites[1] = new FSprite($"Jaw_{jaw}", true);
            if (teeth > 0)
            {
                sLeaser.sprites[2] = new FSprite($"Teeth_{teeth - 1}a", true);
                sLeaser.sprites[3] = new FSprite($"Teeth_{teeth - 1}a", true);
            }
            else
            {
                sLeaser.sprites[2] = new FSprite("pixel", true);
                sLeaser.sprites[3] = new FSprite("pixel", true);
            }

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

            for (int i = 1; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[i - 1]);
            }
        }

        public float rotation;

        public float jawRotation;

        public float teethShade;

        public Color colour;

        public int depth;

        public int skull;

        public int jaw;

        public int teeth;

        public bool flipped;

        public float colourOffset;
    }
}