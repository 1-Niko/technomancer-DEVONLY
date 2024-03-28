using static Pom.Pom;

namespace Slugpack
{
    public class EffectChangerData(PlacedObject owner) : ManagedData(owner, null)
    {
        [Vector2Field("A", 100, 100, Vector2Field.VectorReprType.rect)]
        public Vector2 EffectArea;

        [IntegerField("B", 0, 20, 0, ManagedFieldWithPanel.ControlType.arrows, "Colour")]
        public int colour;

        [BooleanField("C", true, ManagedFieldWithPanel.ControlType.button, "Effect A")]
        public bool effectA;

        [BooleanField("D", true, ManagedFieldWithPanel.ControlType.button, "Effect B")]
        public bool effectB;

        [FloatField("E", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Background Fade")]
        public float fade;

        [FloatField("F", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Hue")]
        public float hue;

        [FloatField("G", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Saturation")]
        public float saturation;

        [FloatField("H", 0, 1, 0, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Value")]
        public float brightness;
    }

    public class EffectChanger(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (EffectObject == null)
            {
                EffectObject = new EffectChangerObject(placedObject, placedObject.pos);
                room.AddObject(EffectObject);
            }
            EffectObject.pos = placedObject.pos;
            EffectObject.colour = (placedObject.data as EffectChangerData).colour;
            EffectObject.effectA = (placedObject.data as EffectChangerData).effectA;
            EffectObject.effectB = (placedObject.data as EffectChangerData).effectB;
            EffectObject.fade = (placedObject.data as EffectChangerData).fade;
            EffectObject.hue = (placedObject.data as EffectChangerData).hue;
            EffectObject.saturation = (placedObject.data as EffectChangerData).saturation;
            EffectObject.brightness = (placedObject.data as EffectChangerData).brightness;
        }

        private EffectChangerObject EffectObject;

        private PlacedObject placedObject = placedObject;
    }

    public class EffectChangerObject(PlacedObject placedObject, Vector2 pos) : CosmeticSprite
    {
        private readonly PlacedObject placedObject = placedObject;
        public int colour { get; set; }
        public bool effectA { get; set; }
        public bool effectB { get; set; }

        public float fade { get; set; }
        public float hue { get; set; }
        public float saturation { get; set; }
        public float brightness { get; set; }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pixel");

            float[] xCoordinates = [pos.x, pos.x + (placedObject.data as EffectChangerData).EffectArea.x];
            float[] yCoordinates = [pos.y, pos.y + (placedObject.data as EffectChangerData).EffectArea.y];

            Vector2 position = new((xCoordinates.Min() + xCoordinates.Max()) / 2f, (yCoordinates.Min() + yCoordinates.Max()) / 2f);

            float scaleX = xCoordinates.Max() - xCoordinates.Min();
            float scaleY = yCoordinates.Max() - yCoordinates.Min();

            float image_size = 1f;

            sLeaser.sprites[0].SetPosition(position - rCam.pos);

            sLeaser.sprites[0].scaleX = scaleX / image_size;
            sLeaser.sprites[0].scaleY = scaleY / image_size;

            sLeaser.sprites[0].isVisible = true;

            sLeaser.sprites[0].color = new Color(Utilities.EncodeBools(effectA, effectB), (1f - (colour / 21f)) % 1f, Utilities.EncodeFloats(fade, hue, saturation, brightness), 0f);

            if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room?.world?.game?.rainWorld, out var Shaders))
            {
                sLeaser.sprites[0].shader = Shaders.ColourChangerShader;
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_EffectMask", Shaders._effectMask);
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_RGB2HSL", Shaders._RGB2HSL);
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_HSL2RGB", Shaders._HSL2RGB);
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