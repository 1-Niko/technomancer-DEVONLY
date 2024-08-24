using UnityEngine.Purchasing;

namespace Slugpack;
public class DistantSatellite : BackgroundScene.BackgroundSceneElement
{
    private AboveCloudsView AboveCloudsScene
    {
        get
        {
            return this.scene as AboveCloudsView;
        }
    }

    public DistantSatellite(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, float minusDepthForLayering, Color nightColour, int nightTimer, float nightActivity, int nightThreshold, bool forceVisible, int orbital) : base(aboveCloudsScene, pos, depth - minusDepthForLayering)
    {
        this.minusDepthForLayering = minusDepthForLayering;

        colour_night = nightColour;
        night_timer = nightTimer;
        night_activity = nightActivity;
        alter_threshold = nightThreshold;
        this.forceVisible = forceVisible;
        this.orbital = orbital;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        timer++;
        float movementTimerAdd = 4;
        for (int i = 0; i < orbital; i++)
        {
            movementTimerAdd = movementTimerAdd / 2f;
        }
        //movementTimer = movementTimer + ((orbital == 0) ? 3 : 1 / Mathf.Sqrt(orbital));
        movementTimer = movementTimer + movementTimerAdd;

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
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("pixel", true);
        sLeaser.sprites[0].scale = 1f;
        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
        {
            sLeaser.sprites[0].shader = shaders.Satellite;
            sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_SkyMask", shaders._skymask);
        }
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);
        sLeaser.sprites[0].x = vector.x + ((movementTimer % 1500f) - 750f);
        float yOffset = 0f;
        float orbitCounter = ((movementTimer % 1500f) - 750f);
        switch (orbital)
        {
            case 0: { yOffset = (-0.0000363267f * orbitCounter * orbitCounter) + (0.1710931268f * orbitCounter) - 86.5630594383f; break; }
            case 1: { yOffset = (-0.0000334082f * orbitCounter * orbitCounter) + (0.1712030144f * orbitCounter) - 65.9813493144f; break; }
            case 2: { yOffset = (-0.0000323288f * orbitCounter * orbitCounter) + (0.1710666561f * orbitCounter) - 47.8399195652f; break; }
            case 3: { yOffset = (-0.0000303659f * orbitCounter * orbitCounter) + (0.1711115019f * orbitCounter) - 32.1212810071f; break; }
            case 4: { yOffset = (-0.0000287223f * orbitCounter * orbitCounter) + (0.1710530134f * orbitCounter) - 18.2210235126f; break; }
            case 5: { yOffset = (-0.0000274961f * orbitCounter * orbitCounter) + (0.1709293376f * orbitCounter) - 5.8558448939f; break; }
            case 6: { yOffset = (-0.0000260650f * orbitCounter * orbitCounter) + (0.1710802118f * orbitCounter) + 5.2381700178f; break; }
            case 7: { yOffset = (-0.0000250335f * orbitCounter * orbitCounter) + (0.1710988528f * orbitCounter) + 15.1542853523f; break; }
            case 8: { yOffset = (-0.0000239273f * orbitCounter * orbitCounter) + (0.1713665901f * orbitCounter) + 24.2633553128f; break; }
            case 9: { yOffset = (-0.0000234322f * orbitCounter * orbitCounter) + (0.1708555706f * orbitCounter) + 32.5331710154f; break; }
            case 10: { yOffset = (-0.0000216621f * orbitCounter * orbitCounter) + (0.1711151146f * orbitCounter) + 39.9845200978f; break; }
            case 11: { yOffset = (-0.0000220592f * orbitCounter * orbitCounter) + (0.1708293211f * orbitCounter) + 46.8635180885f; break; }
            case 12: { yOffset = (-0.0000216171f * orbitCounter * orbitCounter) + (0.1705425743f * orbitCounter) + 53.2632652169f; break; }
            case 13: { yOffset = (-0.0000208150f * orbitCounter * orbitCounter) + (0.1707844536f * orbitCounter) + 59.1532616606f; break; }
            case 14: { yOffset = (-0.0000202718f * orbitCounter * orbitCounter) + (0.1706248968f * orbitCounter) + 64.7266484433f; break; }
            case 15: { yOffset = (-0.0000189377f * orbitCounter * orbitCounter) + (0.1708087470f * orbitCounter) + 69.5016064993f; break; }
            case 16: { yOffset = (-0.0000185779f * orbitCounter * orbitCounter) + (0.1707352130f * orbitCounter) + 74.3526367362f; break; }
            case 17: { yOffset = (-0.0000183122f * orbitCounter * orbitCounter) + (0.1707690558f * orbitCounter) + 78.7941796466f; break; }
            case 18: { yOffset = (-0.0000172209f * orbitCounter * orbitCounter) + (0.1707718221f * orbitCounter) + 82.7937619301f; break; }
            case 19: { yOffset = (-0.0000163887f * orbitCounter * orbitCounter) + (0.1709075477f * orbitCounter) + 86.5130019370f; break; }
            case 20: { yOffset = (-0.0000167171f * orbitCounter * orbitCounter) + (0.1708766189f * orbitCounter) + 90.4854475576f; break; }
            case 21: { yOffset = (-0.0000159250f * orbitCounter * orbitCounter) + (0.1712625822f * orbitCounter) + 93.7098254026f; break; }
            case 22: { yOffset = (-0.0000155831f * orbitCounter * orbitCounter) + (0.1712748680f * orbitCounter) + 96.9214488030f; break; }
            case 23: { yOffset = (-0.0000156110f * orbitCounter * orbitCounter) + (0.1713169652f * orbitCounter) + 99.9895828877f; break; }
            case 24: { yOffset = (-0.0000145937f * orbitCounter * orbitCounter) + (0.1710636304f * orbitCounter) + 102.7652850157f; break; }
            case 25: { yOffset = (-0.0000146095f * orbitCounter * orbitCounter) + (0.1710226112f * orbitCounter) + 105.6145134023f; break; }
            case 26: { yOffset = (-0.0000141062f * orbitCounter * orbitCounter) + (0.1710418138f * orbitCounter) + 108.1091811642f; break; }
            case 27: { yOffset = (-0.0000136275f * orbitCounter * orbitCounter) + (0.1712705948f * orbitCounter) + 110.5913379160f; break; }
            default: { yOffset = 64f; break; }
        }

        sLeaser.sprites[0].y = vector.y - yOffset;
        // sLeaser.sprites[0].y = vector.y + (-0.0000136275f * ((timer % 1500f) - 750f) * ((timer % 1500f) - 750f) + 0.1712705948f * ((timer % 1500f) - 750f) + 110.5913379160f);
        sLeaser.sprites[0].scale = 1;
        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[0].color = colour_night;
        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
        {
            sLeaser.sprites[0].shader = shaders.Satellite;
            sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_SkyMask", shaders._skymask);
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float minusDepthForLayering;

    public Vector4 colour_night = new Color(0f, 1f, 0f, 1f);

    public int timer;
    public float movementTimer;

    public int day_timer = 60;

    public int night_timer = 80;
    public float night_activity = 0.5f;

    public int alter_threshold = 3000;

    public bool isNight = false;

    public int nightCountdown;

    public bool forceDay;
    public bool forceNight;

    public bool forceVisible;

    public int orbital;
}