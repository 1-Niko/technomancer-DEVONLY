using UnityEngine.Purchasing;

namespace Slugpack;
public class DistantBlinkingLight : BackgroundScene.BackgroundSceneElement
{
    private AboveCloudsView AboveCloudsScene
    {
        get
        {
            return this.scene as AboveCloudsView;
        }
    }

    public DistantBlinkingLight(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, float minusDepthForLayering, Color dayColour, Color nightColour, int dayTimer, float dayActivity, int nightTimer, float nightActivity, int nightThreshold, bool forceDay, bool forceNight, bool forceVisible, bool hideRing) : base(aboveCloudsScene, pos, depth - minusDepthForLayering)
    {
        this.minusDepthForLayering = minusDepthForLayering;
        
        colour_day = dayColour;
        colour_night = nightColour;
        day_timer = dayTimer;
        day_activity = dayActivity;
        night_timer = nightTimer;
        night_activity = nightActivity;
        alter_threshold = nightThreshold;
        this.forceDay = forceDay;
        this.forceNight = forceNight;
        this.forceVisible = forceVisible;
        this.hideRing = hideRing;

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
        float calculation = (Mathf.Sin(2f * ((timer + offset) / 512f)) + Mathf.Cos(Mathf.PI * ((timer + offset) / 512f))) / 16f;
        // Plugin.DebugLog($"Twinkle:{calculation}f");
        return calculation;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[5];
        for (int i = 0; i < 5; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel", true);
            sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[i].scale = 1f;
            sLeaser.sprites[i].isVisible = true;
            sLeaser.sprites[i].color = new Color(1f, 1f, 1f);
        }
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);
        sLeaser.sprites[0].x = vector.x;
        sLeaser.sprites[0].y = vector.y;
        sLeaser.sprites[1].x = vector.x;
        sLeaser.sprites[1].y = vector.y + 1;
        sLeaser.sprites[2].x = vector.x;
        sLeaser.sprites[2].y = vector.y - 1;
        sLeaser.sprites[3].x = vector.x + 1;
        sLeaser.sprites[3].y = vector.y;
        sLeaser.sprites[4].x = vector.x - 1;
        sLeaser.sprites[4].y = vector.y;

        for (int i = 0; i < 5; i++)
        {
            sLeaser.sprites[i].isVisible = true;
        }

        if ((forceNight || isNight) && !forceDay)
        {
            if (forceVisible)
            { sLeaser.sprites[0].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_night.x, colour_night.y, colour_night.z, 1f), Mathf.Clamp((1f * colour_night.w * 2), 0, 1)); }
            else
            { sLeaser.sprites[0].color = Lerp(new Color(0f, 0f, 0f, 1f), new Color(colour_night.x, colour_night.y, colour_night.z, 1f), Mathf.Clamp((1f * colour_night.w) + Twinkle(134f + randomizedTwinkleOffset[0]), 0, 1)); }

            for (int i = 0; i < 5; i++)
            {
                ALPHA[i] = Mathf.Clamp((0.5f * colour_night.w) + Twinkle(982f * i + randomizedTwinkleOffset[i]), 0, 1);

                sLeaser.sprites[i].isVisible = ((((timer % night_timer) / (float)night_timer) < night_activity) && ALPHA[i] > (1f / 3f)) || forceVisible;
                if (ALPHA[i] * colour_night.w < (1f / 3f))
                {
                    sLeaser.sprites[i].isVisible = false;
                }
                if (i > 0)
                {
                    sLeaser.sprites[i].color = new Color(colour_night.x, colour_night.y, colour_night.z, ALPHA[i] * colour_night.w);
                    if (this.hideRing)
                    {
                        sLeaser.sprites[i].isVisible = false;
                    }
                }
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
                ALPHA[i] = Mathf.Clamp((0.5f * colour_day.w) + Twinkle(982f * i + randomizedTwinkleOffset[i]), 0, 1);

                sLeaser.sprites[i].isVisible = ((((timer % day_timer) / (float)day_timer) < day_activity) && ALPHA[i] > (1f / 3f) && ALPHA[0] > (1f / 8f)) || forceVisible;
                if (ALPHA[i] * colour_day.w < (1f / 3f))
                {
                    sLeaser.sprites[i].isVisible = false;
                }
                if (i > 0)
                {
                    sLeaser.sprites[i].color = new Color(colour_day.x, colour_day.y, colour_day.z, ALPHA[i] * colour_day.w);
                    if (this.hideRing)
                    {
                        sLeaser.sprites[i].isVisible = false;
                    }
                }
                // sLeaser.sprites[i].alpha = 1f;
            }
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float minusDepthForLayering;

    public Vector4 colour_day = new Color(0f, 1f, 0f, 1f);
    public Vector4 colour_night = new Color(1f, 0f, 0f, 1f);

    public int timer;

    public int day_timer = 60;
    public float day_activity = 0.2f;

    public int night_timer = 80;
    public float night_activity = 0.5f;

    public int alter_threshold = 3000;

    public bool isNight = false;

    public int nightCountdown;

    public bool forceDay;
    public bool forceNight;

    public float[] randomizedTwinkleOffset = new float[5];
    public float[] ALPHA = new float[5];

    public bool forceVisible;

    public bool hideRing;
}