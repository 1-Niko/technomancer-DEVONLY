namespace Slugpack;

public class DragonSkullData(PlacedObject owner) : ManagedData(owner, null)
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
    public int teeth;

    [FloatField("DepthColourOffset", 0, 1, 0, 1, ManagedFieldWithPanel.ControlType.slider, "Colour Depth")]
    public float colOffset;

    [BooleanField("Flip", false, ManagedFieldWithPanel.ControlType.button, "Mirrored")]
    public bool flipped;
}

public class DragonSkull(PlacedObject placedObject) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (DragonsSkull == null)
        {
            DragonsSkull = new SkullOfTheDragon();
            room.AddObject(DragonsSkull);
        }
        DragonsSkull.depth = (placedObject.data as DragonSkullData).depth;
        DragonsSkull.colour = new Color((placedObject.data as DragonSkullData).red, (placedObject.data as DragonSkullData).green, (placedObject.data as DragonSkullData).blue);
        DragonsSkull.rotation = (placedObject.data as DragonSkullData).rotation;
        DragonsSkull.jawRotation = (placedObject.data as DragonSkullData).jawRotation;
        DragonsSkull.colourOffset = (placedObject.data as DragonSkullData).colOffset;
        DragonsSkull.flipped = (placedObject.data as DragonSkullData).flipped;
        DragonsSkull.skull = (placedObject.data as DragonSkullData).skull;
        DragonsSkull.jaw = (placedObject.data as DragonSkullData).jaw;
        DragonsSkull.teeth = (placedObject.data as DragonSkullData).teeth;
        DragonsSkull.teethShade = (placedObject.data as DragonSkullData).teethColour;
        DragonsSkull.pos = placedObject.pos;
    }

    private SkullOfTheDragon DragonsSkull;

    private readonly PlacedObject placedObject = placedObject;
}

public class SkullOfTheDragon() : CosmeticSprite
{

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
        sLeaser.sprites[0].SetPosition(pos - rCam.pos);
        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[0].alpha = depth / 30f;
        sLeaser.sprites[0].color = ColourLerp(colour, new Color(0.149f, 0.121f, 0.098f), colourOffset);
        sLeaser.sprites[0].rotation = sLeaser.sprites[1].scaleX * rotation;

        sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName($"Jaw_{jaw}");
        sLeaser.sprites[1].SetPosition(RotateAroundPoint(pos - rCam.pos, new Vector2(sLeaser.sprites[1].scaleX * 17f, -5f), sLeaser.sprites[1].scaleX * -rotation));
        sLeaser.sprites[1].isVisible = true;
        sLeaser.sprites[1].alpha = depth / 30f;
        sLeaser.sprites[1].color = ColourLerp(colour, new Color(0.149f, 0.121f, 0.098f), colourOffset);
        sLeaser.sprites[1].rotation = sLeaser.sprites[1].scaleX * (-jawRotation + rotation);

        if (teeth != 0)
        {
            sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName($"Teeth_{teeth - 1}a");
            if ((teeth != 4))
            {
                sLeaser.sprites[2].SetPosition(RotateAroundPoint(pos - rCam.pos, new Vector2(sLeaser.sprites[1].scaleX * ((teeth == 6) ? -8f : -9f), ((teeth == 2) ? -9f : -10f) + ((skull == 2) ? 3f : 0f)), sLeaser.sprites[1].scaleX * -rotation));
            }
            else
            {
                sLeaser.sprites[2].SetPosition(RotateAroundPoint(pos - rCam.pos, new Vector2(sLeaser.sprites[1].scaleX * ((teeth == 6) ? -8f : -9f), ((teeth == 2) ? -9f : -11) + ((skull == 2) ? 3f : 0f)), sLeaser.sprites[1].scaleX * -rotation));
            }
            sLeaser.sprites[2].isVisible = true;
            sLeaser.sprites[2].alpha = depth / 30f;
            sLeaser.sprites[2].color = ColourLerp(ColourLerp(colour, new Color(0f, 0f, 0f), teethShade), new Color(0.149f, 0.121f, 0.098f), colourOffset);
            sLeaser.sprites[2].rotation = sLeaser.sprites[1].scaleX * (rotation + 180);

            sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName($"Teeth_{teeth - 1}a");
            if ((teeth != 4))
            {
                sLeaser.sprites[3].SetPosition(RotateAroundPoint(sLeaser.sprites[1].GetPosition(), new Vector2(sLeaser.sprites[1].scaleX * -25f, ((teeth == 2) ? 0f : 1f) - 1f), sLeaser.sprites[1].scaleX * (-rotation + jawRotation)));
            }
            else
            {
                sLeaser.sprites[3].SetPosition(RotateAroundPoint(sLeaser.sprites[1].GetPosition(), new Vector2(sLeaser.sprites[1].scaleX * -25f, ((teeth == 2) ? 0f : 2f) - 1f), sLeaser.sprites[1].scaleX * (-rotation + jawRotation)));
            }
            sLeaser.sprites[3].isVisible = true;
            sLeaser.sprites[3].alpha = depth / 30f;
            sLeaser.sprites[3].color = ColourLerp(ColourLerp(colour, new Color(0f, 0f, 0f), teethShade), new Color(0.149f, 0.121f, 0.098f), colourOffset);
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
        if (shaders_enabled && SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out _))
        {
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
        }

        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
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