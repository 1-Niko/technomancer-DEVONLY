namespace Slugpack;

public class RadioLightExtenderData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2Field("PositionHandleA", 26, -15, Vector2Field.VectorReprType.line)]
    public Vector2 posHandleA;

    [Vector2Field("PositionHandleB", -8, -29, Vector2Field.VectorReprType.line)]
    public Vector2 posHandleB;

    [IntegerField("A", 0, 1024, 0, ManagedFieldWithPanel.ControlType.arrows, "KEYCODE")]
    public int keycode;
}

public class RadioLightExtender(PlacedObject placedObject) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        RadioLight RadioObj = (room.updateList.Where(element => element.ToString() == "Slugpack.RadioLight").Where(element => ((element as RadioLight).placedObject.data as RadioLightObjectData).keycode == (placedObject.data as RadioLightExtenderData).keycode)).ToList()[0] as RadioLight;
        RadioLightObjectData RadioHead = RadioObj.placedObject.data as RadioLightObjectData;

        RadioLightExtenderData radioExtenderObjectData = (placedObject.data as RadioLightExtenderData);

        if (dynamicSprite == null)
        {
            dynamicSprite = new RadioLightObject(placedObject.pos + ((radioExtenderObjectData.posHandleA + radioExtenderObjectData.posHandleB) / 2f));
            room.AddObject(dynamicSprite);
        }

        dynamicSprite.pos = placedObject.pos + ((radioExtenderObjectData.posHandleA + radioExtenderObjectData.posHandleB) / 2f);
        dynamicSprite.colour_day = new Vector4(RadioHead.R_daylight, RadioHead.G_daylight, RadioHead.B_daylight, RadioHead.A_daylight);
        dynamicSprite.colour_night = new Vector4(RadioHead.R_nighttime, RadioHead.G_nighttime, RadioHead.B_nighttime, RadioHead.A_nighttime);

        dynamicSprite.day_timer = RadioHead.loopLength_daylight;
        dynamicSprite.day_activity = RadioHead.loopActive_daylight;

        dynamicSprite.night_timer = RadioHead.loopLength_nighttime;
        dynamicSprite.night_activity = RadioHead.loopActive_nighttime;

        dynamicSprite.alter_threshold = RadioHead.daynight_threshold;

        dynamicSprite.forceVisible = RadioHead.forceVisible;

        if (RadioObj.dynamicSprite != null)
        {
            dynamicSprite.isNight = RadioObj.dynamicSprite.isNight;
            dynamicSprite.timer = RadioObj.dynamicSprite.timer;
            dynamicSprite.randomizedTwinkleOffset = RadioObj.dynamicSprite.randomizedTwinkleOffset;
        }

        ForceDisplay FORCE_MODE = RadioHead.ForceMode;

        switch (FORCE_MODE)
        {
            case ForceDisplay.None:
                {
                    dynamicSprite.forceDay = false;
                    dynamicSprite.forceNight = false;
                    break;
                }
            case ForceDisplay.Day:
                {
                    dynamicSprite.forceDay = true;
                    dynamicSprite.forceNight = false;
                    break;
                }
            case ForceDisplay.Night:
                {
                    dynamicSprite.forceDay = false;
                    dynamicSprite.forceNight = true;
                    break;
                }
        }
    }

    private RadioLightObject dynamicSprite;
}