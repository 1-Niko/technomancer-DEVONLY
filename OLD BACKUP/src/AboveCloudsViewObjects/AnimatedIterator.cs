using Unity.Mathematics;

namespace Slugpack;

public class AnimatedIterator : BackgroundScene.BackgroundSceneElement
{
    private AboveCloudsView AboveCloudsScene
    {
        get
        {
            return this.scene as AboveCloudsView;
        }
    }

    public AnimatedIterator(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, float atmosphericalDepthAdd) : base(aboveCloudsScene, pos, depth)
    {
        this.atmosphericalDepthAdd = atmosphericalDepthAdd;
        this.alpha = 1f;
        this.maskIndex = Random.Range(0, 17);

        this.scene.LoadGraphic("atc_structure1-technomancer", true, false);
        this.scene.LoadGraphic($"masks/atc_structure1-technomancer-citymask{this.maskIndex}", true, false);
        this.antennaeMessageIndex = Random.Range(0, 5);
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

        if (isNight)
        {
            lightTimer++;
            antennaeMessageCooldown = Mathf.Max(antennaeMessageCooldown - 1, 0);
            if (!antennaeMessageTriggered && lightTimer > antennaeMessageTime && antennaeMessageCooldown == 0)
            {
                if (antennaeMessageKey < 32)
                {
                    if (antennaeMessageToggle)
                    {
                        antennaeMessageCooldown = antennaeMessageDelay * antennaeMessageMultiplier;
                    }
                    else
                    {
                        antennaeMessageCooldown = funAntennaMessages[antennaeMessageIndex, antennaeMessageKey] * antennaeMessageMultiplier;
                        antennaeMessageKey++;
                    }
                    antennaeMessageToggle = !antennaeMessageToggle;
                }
                else
                {
                    antennaeMessageTriggered = true;
                }
            }
            if (antennaeMessageTriggered)
            {
                antennaeMessageToggle = false;
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];
        sLeaser.sprites[0] = new FSprite("atc_structure1-technomancer", true);
        sLeaser.sprites[1] = new FSprite($"masks/atc_structure1-technomancer-citymask{this.maskIndex}", true);
        sLeaser.sprites[2] = new FSprite("pixel", true);
        if (this.useNonMultiplyShader)
        {
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObjectAlpha"];
        }
        else
        {
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObject"];
        }
        sLeaser.sprites[0].anchorY = 0f;
        sLeaser.sprites[1].anchorY = 0f;
        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
        {
            sLeaser.sprites[1].shader = shaders.DistantCityLights;
        }
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);
        sLeaser.sprites[0].x = vector.x;
        sLeaser.sprites[0].y = vector.y;
        sLeaser.sprites[0].alpha = this.alpha;
        sLeaser.sprites[0].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.9f, 0f, 0f);

        sLeaser.sprites[1].x = vector.x;
        sLeaser.sprites[1].y = vector.y;
        sLeaser.sprites[1].alpha = Mathf.Clamp((float)Mathf.Clamp(lightTimer, 0, lightCount) / (float)lightCount, 0f, 0.99f);
        sLeaser.sprites[1].color = new UnityEngine.Color(1f, 0f, (float)lightTimer); // Mathf.Sin((3.14159f * (float)lightTimer) / (float)lightCount));

        // Plugin.DebugLog($"Variables: Mathf.Clamp((float)Mathf.Clamp({lightTimer}, 0, {lightCount}) / (float){lightCount}, 0f, 0.99f) = {Mathf.Clamp((float)Mathf.Clamp(lightTimer, 0, lightCount) / (float)lightCount, 0f, 0.99f)},new UnityEngine.Color(1f, 0f, (float){lightTimer}) = {new UnityEngine.Color(1f, 0f, (float)lightTimer)}");

        sLeaser.sprites[2].x = vector.x - 13; // CHANGE THIS IF IT IS OFFSET
        sLeaser.sprites[2].y = vector.y + 132;
        sLeaser.sprites[2].scale = 1f;
        Vector3 colour = new Vector3(252, 30, 145);
        sLeaser.sprites[2].color = new UnityEngine.Color(colour[0] / 255f, colour[1] / 255f, colour[2] / 255f);
        sLeaser.sprites[2].isVisible = antennaeMessageToggle;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float atmosphericalDepthAdd;

    public float alpha;

    public bool useNonMultiplyShader;

    public int timer;

    public int maskIndex;

    public int day_timer = 1;

    public int alter_threshold = 1500;

    public bool isNight = false;

    public int nightCountdown;
    public int lightTimer;
    public int lightCount = 4800;

    public int antennaeMessageTime = 3000;
    public int antennaeMessageIndex;
    public int antennaeMessageMultiplier = 2;
    public int antennaeMessageCooldown;
    public int antennaeMessageKey;
    public int antennaeMessageDelay = 2;
    public bool antennaeMessageToggle;
    public bool antennaeMessageTriggered;

    // See if you can decode them ;3
    public int[,] funAntennaMessages = { { 7, 3, 15, 5, 25, 1, 24, 19, 3, 6, 21, 18, 20, 9, 15, 19, 17, 16, 9, 14, 9, 21, 10, 21, 18, 18, 24, 6, 5, 20, 23, 6 },
                                         { 7, 13, 9, 13, 11, 16, 10, 9, 14, 19, 14, 7, 20, 22, 12, 1, 20, 1, 23, 3, 3, 4, 14, 13, 20, 18, 4, 8, 9, 16, 15, 11 },
                                         { 7, 21, 2, 23, 14, 25, 9, 5, 6, 16, 15, 10, 14, 8, 19, 7, 17, 19, 4, 13, 11, 13, 2, 5, 24, 12, 10, 19, 9, 6, 6, 13 },
                                         { 7, 21, 17, 25, 5, 11, 20, 7, 6, 7, 9, 11, 25, 10, 15, 20, 6, 11, 9, 19, 21, 21, 14, 11, 1, 3, 25, 20, 23, 15, 24, 15 },
                                         { 7, 11, 11, 5, 17, 23, 1, 5, 25, 4, 11, 17, 5, 10, 14, 19, 23, 15, 15, 14, 6, 16, 22, 22, 14, 16, 23, 7, 9, 25, 10, 25 }};
}