namespace Slugpack;

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
        EffectObject.Colour = (placedObject.data as EffectChangerData).colour;
        EffectObject.EffectA = (placedObject.data as EffectChangerData).effectA;
        EffectObject.EffectB = (placedObject.data as EffectChangerData).effectB;
        EffectObject.Fade = (placedObject.data as EffectChangerData).fade;
        EffectObject.Hue = (placedObject.data as EffectChangerData).hue;
        EffectObject.Saturation = (placedObject.data as EffectChangerData).saturation;
        EffectObject.Brightness = (placedObject.data as EffectChangerData).brightness;
    }

    private EffectChangerObject EffectObject;

    private PlacedObject placedObject = placedObject;
}

public class EffectChangerObject(PlacedObject placedObject, Vector2 pos) : CosmeticSprite
{
    private readonly PlacedObject placedObject = placedObject;
    public int Colour { get; set; }
    public bool EffectA { get; set; }
    public bool EffectB { get; set; }

    public float Fade { get; set; }
    public float Hue { get; set; }
    public float Saturation { get; set; }
    public float Brightness { get; set; }

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

        sLeaser.sprites[0].color = new Color(Utilities.EncodeBools(EffectA, EffectB), (1f - (Colour / 21f)) % 1f, Utilities.EncodeFloats(Fade, Hue, Saturation, Brightness), 0f);

        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room?.world?.game?.rainWorld, out var Shaders))
        {
            sLeaser.sprites[0].shader = Shaders.ColourChangerShader;
            sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_EffectMask", Shaders._effectMask);
            sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_RGB2HSL", Shaders._RGB2HSL);
            sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_HSL2RGB", Shaders._HSL2RGB);
        }

        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
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