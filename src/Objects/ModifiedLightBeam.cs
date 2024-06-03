namespace Slugpack;

public class ModifiedLightBeamData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2ArrayField("LightBeam", 4, true, Vector2ArrayRepresentationType.Polygon, new float[8] { 0, 0, 0, 50, 50, 50, 50, 0 })]
    public Vector2[] BeamPosition;

    [Vector2ArrayField("AlternateLightBeam", 5, true, Vector2ArrayRepresentationType.Polygon, new float[10] { 0, 0, 40, 29, 25, 77, -25, 77, -40, 29 })]
    public Vector2[] AlternateBeamPosition;

    [FloatField("A", 0, 1, 0.5f, 0.05f, ManagedFieldWithPanel.ControlType.slider, "Alpha")]
    public float alpha;

    [FloatField("B", 0, 1, 0, 0.05f, ManagedFieldWithPanel.ControlType.slider, "Depth")]
    public float depth;

    [FloatField("C", 0, 1, 0, 0.05f, ManagedFieldWithPanel.ControlType.slider, "White Pickup")]
    public float white;

    [BooleanField("D", true, ManagedFieldWithPanel.ControlType.button, "Sun")]
    public bool sun;

    [StringField("DE", "FFFFFF", "HEX")]
    public string hex;

    [IntegerField("E", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen A")]
    public int screenA;

    [BooleanField("F", false, ManagedFieldWithPanel.ControlType.button, "Set Screen A To Current")]
    public bool setScreenA;

    [IntegerField("G", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen B")]
    public int screenB;

    [BooleanField("H", false, ManagedFieldWithPanel.ControlType.button, "Set Screen B To Current")]
    public bool setScreenB;
}

public class ModifiedLightBeam(PlacedObject placedObject, Room room) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (LightBeam == null)
        {
            LightBeam = new ModifiedLightBeamSprite(placedObject, placedObject.pos);
            room.AddObject(LightBeam);
        }
        LightBeam.pos = placedObject.pos;

        LightBeam.depth = (placedObject.data as ModifiedLightBeamData).depth;
        LightBeam.opacity = (placedObject.data as ModifiedLightBeamData).alpha;
        LightBeam.whiteness = (placedObject.data as ModifiedLightBeamData).white;
        LightBeam.colour = Utilities.HexToColor((placedObject.data as ModifiedLightBeamData).hex);

        if (Constants.DamagedShortcuts.TryGetValue(room.game, out var CameraPosition))
        {
            if ((placedObject.data as ModifiedLightBeamData).screenA == CameraPosition.camPosition)
            {
                Vector2[] vertices = (placedObject.data as ModifiedLightBeamData).BeamPosition;
                LightBeam.verts = [vertices[0], vertices[3], vertices[1], vertices[2]];
            }
            else if ((placedObject.data as ModifiedLightBeamData).screenB == CameraPosition.camPosition)
            {
                Vector2[] vertices = (placedObject.data as ModifiedLightBeamData).AlternateBeamPosition;
                LightBeam.verts = [vertices[4], vertices[1], vertices[3], vertices[2]];
            }

            if ((placedObject.data as ModifiedLightBeamData).setScreenA)
            {
                (placedObject.data as ModifiedLightBeamData).screenA = CameraPosition.camPosition;
                (placedObject.data as ModifiedLightBeamData).setScreenA = false;
            }

            if ((placedObject.data as ModifiedLightBeamData).setScreenB)
            {
                (placedObject.data as ModifiedLightBeamData).screenB = CameraPosition.camPosition;
                (placedObject.data as ModifiedLightBeamData).setScreenB = false;
            }
        }
    }

    private ModifiedLightBeamSprite LightBeam;

    private PlacedObject placedObject = placedObject;
}

public class ModifiedLightBeamSprite(PlacedObject placedObject, Vector2 pos) : CosmeticSprite
{
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[0].isVisible = true;

        Color sky = colour;

        sLeaser.sprites[0].color = new Color(sky.r, sky.g, sky.b, Utilities.EncodeFloats(depth, opacity, whiteness) / 32767f);

        if ((sLeaser.sprites[0] is TriangleMesh) && verts != null)
        {
            for (int i = 0; i < 4; i++)
            {
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, pos - rCam.pos + verts[i]);
            }
        }

        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam?.room?.world?.game?.rainWorld, out var Shaders))
        {
            sLeaser.sprites[0].shader = Shaders.ModifiedLightBeamShader;
        }

        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = TriangleMesh.MakeGridMesh("LightBeam", 1);

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

    public Vector2[] verts;

    public float opacity = 0;

    public float whiteness = 0;

    public float depth = 0;

    public Color colour;
}