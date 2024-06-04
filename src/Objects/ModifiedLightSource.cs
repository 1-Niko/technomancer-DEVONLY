namespace Slugpack;

public enum BlinkType
{
    None,
    Flash,
    Fade
}

public class ModifiedLightSourceData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2ArrayField("ExtraPosition", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, -50, 50 })]
    public Vector2[] ExtraPosition;

    [Vector2Field("radius1", 50, 50, Vector2Field.VectorReprType.circle)]
    public Vector2 rad1;

    [Vector2Field("radius2", 65, 65, Vector2Field.VectorReprType.circle)]
    public Vector2 rad2;

    [FloatField("A", 0, 2, 0.5f, 0.05f, ManagedFieldWithPanel.ControlType.slider, "Alpha")]
    public float alpha;

    [FloatField("BA", 0, 1, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "R")]
    public float red;

    [FloatField("BB", 0, 1, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "G")]
    public float green;

    [FloatField("BC", 0, 1, 11f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "B")]
    public float blue;

    [BooleanField("C", false, ManagedFieldWithPanel.ControlType.button, "Flat")]
    public bool flat;

    [FloatField("D", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Blink Rate")]
    public float blinkRate;

    [EnumField<BlinkType>("E", BlinkType.None, [BlinkType.None, BlinkType.Flash, BlinkType.Fade], ManagedFieldWithPanel.ControlType.arrows, "Blink Type")]
    public BlinkType blink;

    [IntegerField("F", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen A")]
    public int screenA;

    [BooleanField("G", false, ManagedFieldWithPanel.ControlType.button, "Set Screen A To Current")]
    public bool setScreenA;

    [IntegerField("H", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen B")]
    public int screenB;

    [BooleanField("I", false, ManagedFieldWithPanel.ControlType.button, "Set Screen B To Current")]
    public bool setScreenB;
}

public class ModifiedLightSource(PlacedObject placedObject) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (lightSourceA == null)
        {
            lightSourceA = new LightSource(placedObject.pos + (placedObject.data as ModifiedLightSourceData).ExtraPosition[1], false, new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue), this)
            {
                affectedByPaletteDarkness = 0.5f
            };
            room.AddObject(lightSourceA);
        }

        if (lightSourceB == null)
        {
            lightSourceB = new LightSource(placedObject.pos, false, new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue), this)
            {
                affectedByPaletteDarkness = 0.5f
            };
            room.AddObject(lightSourceB);
        }

        lightSourceA.pos = placedObject.pos + (placedObject.data as ModifiedLightSourceData).ExtraPosition[1];
        lightSourceB.pos = placedObject.pos;

        lightSourceA.color = new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue);
        lightSourceB.color = new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue);

        lightSourceA.rad = RWCustom.Custom.Dist(Vector2.zero, (placedObject.data as ModifiedLightSourceData).rad1);
        lightSourceB.rad = RWCustom.Custom.Dist(Vector2.zero, (placedObject.data as ModifiedLightSourceData).rad2);

        lightSourceA.flat = (placedObject.data as ModifiedLightSourceData).flat;
        lightSourceB.flat = (placedObject.data as ModifiedLightSourceData).flat;

        lightSourceA.blinkRate = (placedObject.data as ModifiedLightSourceData).blinkRate;
        lightSourceB.blinkRate = (placedObject.data as ModifiedLightSourceData).blinkRate;

        BlinkType BLINK_TYPE = (placedObject.data as ModifiedLightSourceData).blink;

        switch (BLINK_TYPE)
        {
            case BlinkType.None:
                {
                    lightSourceA.blinkType = PlacedObject.LightSourceData.BlinkType.None;
                    lightSourceB.blinkType = PlacedObject.LightSourceData.BlinkType.None;
                    break;
                }
            case BlinkType.Fade:
                {
                    lightSourceA.blinkType = PlacedObject.LightSourceData.BlinkType.Fade;
                    lightSourceB.blinkType = PlacedObject.LightSourceData.BlinkType.Fade;
                    break;
                }
            case BlinkType.Flash:
                { 
                    lightSourceA.blinkType = PlacedObject.LightSourceData.BlinkType.Flash;
                    lightSourceB.blinkType = PlacedObject.LightSourceData.BlinkType.Flash;
                    break;
                }
        }

        if (Constants.DamagedShortcuts.TryGetValue(room.game, out var CameraPosition))
        {
            if ((placedObject.data as ModifiedLightSourceData).setScreenA)
            {
                (placedObject.data as ModifiedLightSourceData).screenA = CameraPosition.camPosition;
                (placedObject.data as ModifiedLightSourceData).setScreenA = false;
            }

            if ((placedObject.data as ModifiedLightSourceData).setScreenB)
            {
                (placedObject.data as ModifiedLightSourceData).screenB = CameraPosition.camPosition;
                (placedObject.data as ModifiedLightSourceData).setScreenB = false;
            }

            if (CameraPosition.camPosition == (placedObject.data as ModifiedLightSourceData).screenA)
            {
                lightSourceA.alpha = 0f;
                lightSourceB.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
            }
            if (CameraPosition.camPosition == (placedObject.data as ModifiedLightSourceData).screenB)
            {
                lightSourceA.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
                lightSourceB.alpha = 0f;
            }
            if (CameraPosition.camPosition == (placedObject.data as ModifiedLightSourceData).screenA && (placedObject.data as ModifiedLightSourceData).screenA == (placedObject.data as ModifiedLightSourceData).screenB)
            {
                lightSourceA.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
                lightSourceB.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
            }
        }
    }

    private readonly PlacedObject placedObject = placedObject;

    private LightSource lightSourceA;
    private LightSource lightSourceB;
}