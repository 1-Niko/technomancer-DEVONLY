namespace Slugpack;

public enum ForceDisplay
{
    None,
    Day,
    Night
}

public class RadioLightObjectData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2Field("PositionHandleA", 26, -15, Vector2Field.VectorReprType.line)]
    public Vector2 posHandleA;

    [Vector2Field("PositionHandleB", -8, -29, Vector2Field.VectorReprType.line)]
    public Vector2 posHandleB;

    [FloatField("B", 0f, 1f, 0.9f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "R (Day)")]
    public float R_daylight;

    [FloatField("C", 0f, 1f, 0.9f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "G (Day)")]
    public float G_daylight;

    [FloatField("D", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "B (Day)")]
    public float B_daylight;

    [FloatField("E", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "A (Day)")]
    public float A_daylight;

    [FloatField("F", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "R (Night)")]
    public float R_nighttime;

    [FloatField("G", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "G (Night)")]
    public float G_nighttime;

    [FloatField("H", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "B (Night)")]
    public float B_nighttime;

    [FloatField("I", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "A (Night)")]
    public float A_nighttime;

    [BooleanField("J", false, ManagedFieldWithPanel.ControlType.button, "Reset Colour Defaults")]
    public bool resetColours;

    [EnumField<ForceDisplay>("K", ForceDisplay.None, [ForceDisplay.None, ForceDisplay.Day, ForceDisplay.Night], ManagedFieldWithPanel.ControlType.arrows, "Force Display")]
    public ForceDisplay ForceMode;

    [IntegerField("L", 4, 400, 60, ManagedFieldWithPanel.ControlType.slider, "Loop Length (Day)")]
    public int loopLength_daylight;

    [FloatField("M", 0f, 1f, 0.2f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Loop Active (Day)")]
    public float loopActive_daylight;

    [IntegerField("N", 4, 400, 80, ManagedFieldWithPanel.ControlType.slider, "Loop Length (Night)")]
    public int loopLength_nighttime;

    [FloatField("O", 0f, 1f, 0.5f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Loop Active (Night)")]
    public float loopActive_nighttime;

    [BooleanField("P", false, ManagedFieldWithPanel.ControlType.button, "Reset Timer Defaults")]
    public bool resetTimerDefaults;

    [BooleanField("Q", false, ManagedFieldWithPanel.ControlType.button, "Locally Randomize Timers")]
    public bool locallyRandomizeTimers;

    [IntegerField("R", 0, 1024, 0, ManagedFieldWithPanel.ControlType.arrows, "KEYCODE")]
    public int keycode;

    [IntegerField("S", 1, 4096, 3000, ManagedFieldWithPanel.ControlType.slider, "Night Activation Timer")]
    public int daynight_threshold;

    [BooleanField("T", false, ManagedFieldWithPanel.ControlType.button, "Reset Night Timer")]
    public bool reset_Threshold;

    [BooleanField("U", false, ManagedFieldWithPanel.ControlType.button, "Locally Randomize Night Timer")]
    public bool randomizeThreshold;

    [BooleanField("V", false, ManagedFieldWithPanel.ControlType.button, "Force Visibility")]
    public bool forceVisible;
}

public class RadioLight : UpdatableAndDeletable
{
    public RadioLight(PlacedObject placedObject)
    {
        this.placedObject = placedObject;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        RadioLightObjectData radioLightObjectData = (placedObject.data as RadioLightObjectData);

        if (radioLightObjectData.resetColours)
        {
            radioLightObjectData.R_daylight = 0.9f;
            radioLightObjectData.G_daylight = 0.9f;
            radioLightObjectData.B_daylight = 1.0f;
            radioLightObjectData.A_daylight = 1.0f;

            radioLightObjectData.R_nighttime = 1.0f;
            radioLightObjectData.G_nighttime = 0.0f;
            radioLightObjectData.B_nighttime = 0.0f;
            radioLightObjectData.A_nighttime = 1.0f;

            radioLightObjectData.resetColours = false;
        }
        if (radioLightObjectData.resetTimerDefaults)
        {
            radioLightObjectData.loopLength_daylight = 60;
            radioLightObjectData.loopActive_daylight = 0.2f;

            radioLightObjectData.loopLength_nighttime = 80;
            radioLightObjectData.loopActive_nighttime = 0.5f;

            radioLightObjectData.resetTimerDefaults = false;
        }
        if (radioLightObjectData.locallyRandomizeTimers)
        {
            radioLightObjectData.loopLength_daylight = Mathf.Clamp(radioLightObjectData.loopLength_daylight + Random.Range(-10, 10), 4, 400);
            radioLightObjectData.loopActive_daylight = Mathf.Clamp(radioLightObjectData.loopActive_daylight + Random.Range(-0.15f, 0.15f), 0f, 1f);

            radioLightObjectData.loopLength_nighttime = Mathf.Clamp(radioLightObjectData.loopLength_nighttime + Random.Range(-10, 10), 4, 400);
            radioLightObjectData.loopActive_nighttime = Mathf.Clamp(radioLightObjectData.loopActive_nighttime + Random.Range(-0.15f, 0.15f), 0f, 1f);

            radioLightObjectData.locallyRandomizeTimers = false;
        }
        if (radioLightObjectData.reset_Threshold)
        {
            radioLightObjectData.daynight_threshold = 3000;

            radioLightObjectData.reset_Threshold = false;
        }
        if (radioLightObjectData.randomizeThreshold)
        {
            radioLightObjectData.daynight_threshold = Mathf.Clamp(radioLightObjectData.daynight_threshold + Random.Range(-45, 45), 0, 4096);

            radioLightObjectData.randomizeThreshold = false;
        }

        if (dynamicSprite == null)
        {
            dynamicSprite = new RadioLightObject(placedObject.pos + ((radioLightObjectData.posHandleA + radioLightObjectData.posHandleB) / 2f));
            room.AddObject(dynamicSprite);
        }

        dynamicSprite.pos = placedObject.pos + ((radioLightObjectData.posHandleA + radioLightObjectData.posHandleB) / 2f);
        dynamicSprite.colour_day = new Vector4(radioLightObjectData.R_daylight, radioLightObjectData.G_daylight, radioLightObjectData.B_daylight, radioLightObjectData.A_daylight);
        dynamicSprite.colour_night = new Vector4(radioLightObjectData.R_nighttime, radioLightObjectData.G_nighttime, radioLightObjectData.B_nighttime, radioLightObjectData.A_nighttime);

        dynamicSprite.day_timer = radioLightObjectData.loopLength_daylight;
        dynamicSprite.day_activity = radioLightObjectData.loopActive_daylight;

        dynamicSprite.night_timer = radioLightObjectData.loopLength_nighttime;
        dynamicSprite.night_activity = radioLightObjectData.loopActive_nighttime;

        dynamicSprite.alter_threshold = radioLightObjectData.daynight_threshold;

        dynamicSprite.forceVisible = radioLightObjectData.forceVisible;

        ForceDisplay FORCE_MODE = radioLightObjectData.ForceMode;

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

    public PlacedObject placedObject;
    public RadioLightObject dynamicSprite;
}

public class RadioLightObject : CosmeticSprite
{
    public RadioLightObject(Vector2 pos)
    {
        this.pos = pos;
        for (int i = 0; i < 5; i++)
        {
            randomizedTwinkleOffset[i] = Random.Range(0f, 2048f);
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        timer++;

        if (!isNight && this.room.world.rainCycle.ShaderLight == -1)
        {
            if (timer % day_timer == 0)
            {
                nightCountdown--;
            }
            if (nightCountdown == 0)
            {
                isNight = true;
            }
        }
        else
        {
            nightCountdown = alter_threshold / day_timer;
        }
    }

    private float Twinkle(float offset)
    {
        return (Mathf.Sin(2f * ((timer + offset) / 512f)) + Mathf.Cos(Mathf.PI * ((timer + offset) / 512f))) / 16f;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        Vector2 offset = new Vector2(17f, 17f);

        sLeaser.sprites[0].scale = 1f;
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pixel");
        sLeaser.sprites[0].SetPosition(pos - rCam.CamPos(rCam.currentCameraPosition) - offset);

        sLeaser.sprites[1].scale = 1f;
        sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("pixel");
        sLeaser.sprites[1].SetPosition((pos + Vector2.up) - rCam.CamPos(rCam.currentCameraPosition) - offset);

        sLeaser.sprites[2].scale = 1f;
        sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("pixel");
        sLeaser.sprites[2].SetPosition((pos + Vector2.down) - rCam.CamPos(rCam.currentCameraPosition) - offset);

        sLeaser.sprites[3].scale = 1f;
        sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("pixel");
        sLeaser.sprites[3].SetPosition((pos + Vector2.left) - rCam.CamPos(rCam.currentCameraPosition) - offset);

        sLeaser.sprites[4].scale = 1f;
        sLeaser.sprites[4].element = Futile.atlasManager.GetElementWithName("pixel");
        sLeaser.sprites[4].SetPosition((pos + Vector2.right) - rCam.CamPos(rCam.currentCameraPosition) - offset);

        if ((forceNight || isNight) && !forceDay)
        {
            if (forceVisible)
            { sLeaser.sprites[0].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_night.x, colour_night.y, colour_night.z, 1f), Mathf.Clamp((1f * colour_night.w * 2), 0, 1)); }
            else
            { sLeaser.sprites[0].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_night.x, colour_night.y, colour_night.z, 1f), Mathf.Clamp((1f * colour_night.w) + Twinkle(134f + randomizedTwinkleOffset[0]), 0, 1)); }

            for (int i = 0; i < 5; i++)
            {
                ALPHA[i] = Mathf.Clamp((0.5f * colour_night.w) + Twinkle(982f * i + randomizedTwinkleOffset[i]), 0, 1);

                if (i > 0)
                {
                    sLeaser.sprites[i].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_night.x, colour_night.y, colour_night.z, 1f), ALPHA[i]);
                }
                sLeaser.sprites[i].isVisible = ((((timer % night_timer) / (float)night_timer) < night_activity) && ALPHA[i] > (1f / 3f)) || forceVisible;
                // sLeaser.sprites[i].alpha = 0.5f;
                // sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            }
        }
        else
        {
            if (forceVisible)
            { sLeaser.sprites[0].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_day.x, colour_day.y, colour_day.z, 1f), Mathf.Clamp((1f * colour_day.w * 2), 0, 1)); }
            else
            { sLeaser.sprites[0].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_day.x, colour_day.y, colour_day.z, 1f), Mathf.Clamp((1f * colour_day.w) + Twinkle(134f + randomizedTwinkleOffset[0]), 0, 1)); }

            for (int i = 0; i < 5; i++)
            {
                ALPHA[i] = Mathf.Clamp((0.5f * colour_night.w) + Twinkle(982f * i + randomizedTwinkleOffset[i]), 0, 1);

                if (i > 0)
                {
                    sLeaser.sprites[i].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_day.x, colour_day.y, colour_day.z, 1f), ALPHA[i]);
                }
                sLeaser.sprites[i].isVisible = ((((timer % day_timer) / (float)day_timer) < day_activity) && ALPHA[i] > (1f / 3f) && ALPHA[0] > (1f / 3f)) || forceVisible;
                sLeaser.sprites[i].alpha = 1f;
                sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[5];
        for (int i = 0; i < 5; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel", true);
        }

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Water");
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

    public Vector4 colour_day;
    public Vector4 colour_night;

    public int timer;

    public int day_timer;
    public float day_activity;

    public int night_timer;
    public float night_activity;

    public int alter_threshold;

    public bool isNight = false;

    public int nightCountdown;

    public bool forceDay;
    public bool forceNight;

    public float[] randomizedTwinkleOffset = new float[5];
    public float[] ALPHA = new float[5];

    public bool forceVisible;
}