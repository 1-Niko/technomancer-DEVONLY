using System.Linq;
using UnityEngine;
using static Pom.Pom;

namespace Slugpack
{
    public class EffectChangerData : ManagedData
    {
        [Vector2Field("EffectArea", 100, 100, Vector2Field.VectorReprType.rect)]
        public Vector2 EffectArea;

        [IntegerField("Colour", 0, 20, 0, ManagedFieldWithPanel.ControlType.arrows, "Colour")]
        public int colour;

        [BooleanField("Effect A", true, ManagedFieldWithPanel.ControlType.button, "Effect A")]
        public bool effectA;

        [BooleanField("Effect B", true, ManagedFieldWithPanel.ControlType.button, "Effect B")]
        public bool effectB;

        [FloatField("Fade", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Background Fade")]
        public float fade;

        [FloatField("Hue", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Hue")]
        public float hue;

        [FloatField("Saturation", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Saturation")]
        public float saturation;

        [FloatField("Brightness", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Brightness")]
        public float brightness;

        public EffectChangerData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class EffectChanger : UpdatableAndDeletable
    {
        public EffectChanger(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.EffectObject == null)
            {
                this.EffectObject = new EffectChangerObject(placedObject, placedObject.pos);
                this.room.AddObject(this.EffectObject);
            }
            this.EffectObject.pos = this.placedObject.pos;
            this.EffectObject.colour = (this.placedObject.data as EffectChangerData).colour;
            this.EffectObject.effectA = (this.placedObject.data as EffectChangerData).effectA;
            this.EffectObject.effectB = (this.placedObject.data as EffectChangerData).effectB;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        EffectChangerObject EffectObject;

        PlacedObject placedObject;
    }

    public class EffectChangerObject : CosmeticSprite
    {
        private readonly PlacedObject placedObject;
        public int colour { get; set; }
        public bool effectA { get; set; }
        public bool effectB { get; set; }

        public EffectChangerObject(PlacedObject placedObject, Vector2 pos)
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

            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pixel");

            float[] xCoordinates = new float[2] { this.pos.x, this.pos.x + (this.placedObject.data as EffectChangerData).EffectArea.x };
            float[] yCoordinates = new float[2] { this.pos.y, this.pos.y + (this.placedObject.data as EffectChangerData).EffectArea.y };

            Vector2 position = new Vector2((xCoordinates.Min() + xCoordinates.Max()) / 2f, (yCoordinates.Min() + yCoordinates.Max()) / 2f);

            float scaleX = xCoordinates.Max() - xCoordinates.Min();
            float scaleY = yCoordinates.Max() - yCoordinates.Min();

            float image_size = 1f;

            sLeaser.sprites[0].SetPosition(position - rCam.pos);

            sLeaser.sprites[0].scaleX = scaleX / image_size;
            sLeaser.sprites[0].scaleY = scaleY / image_size;

            sLeaser.sprites[0].isVisible = true;

            // Green Which Effect Colour
            sLeaser.sprites[0].color = new Color((this.effectA) ? 1 : 0, (1f - (this.colour / 21f)) % 1f, (this.effectB) ? 1 : 0, 1f);
            
            if (Constants.SlugpackShaders.TryGetValue(rCam?.room?.world?.game?.rainWorld, out var Shaders))
            {
                sLeaser.sprites[0].shader = Shaders.ColourChangerShader;
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_EffectMask", Shaders._effectMask);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = new FSprite("pixel", true);

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
    }
}