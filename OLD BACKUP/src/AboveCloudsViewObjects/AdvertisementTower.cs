using UnityEngine;

namespace Slugpack;

public class AdTower : BackgroundScene.BackgroundSceneElement
{
    private AboveCloudsView AboveCloudsScene
    {
        get
        {
            return this.scene as AboveCloudsView;
        }
    }

    public AdTower(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, float atmosphericalDepthAdd) : base(aboveCloudsScene, pos, depth)
    {
        this.atmosphericalDepthAdd = atmosphericalDepthAdd;
        this.alpha = 1f;

        this.scene.LoadGraphic("atc_advertiser1", true, false);
        this.scene.LoadGraphic("atc_pebbsiHologram", true, false);
    }

    public override void Update(bool eu)
    {
        timer++;

        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("atc_advertiser1", true);
        sLeaser.sprites[1] = new FSprite("atc_pebbsiHologram", true);

        if (this.useNonMultiplyShader)
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObjectAlpha"];
        else
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObject"];

        sLeaser.sprites[0].anchorY = 0f;

        sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Background"];

        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);

        sLeaser.sprites[0].x = vector.x;
        sLeaser.sprites[0].y = vector.y;
        sLeaser.sprites[0].alpha = this.alpha;
        sLeaser.sprites[0].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.9f, 0f, 0f);

        Vector2 offset = new Vector2(-35f, 137f);
        sLeaser.sprites[1].x = vector.x + offset.x;
        sLeaser.sprites[1].y = vector.y + offset.y;
        sLeaser.sprites[1].alpha = (Mathf.Min(Twinkle((timer / 2000f) * 56783f), Twinkle((timer / 10000f) * 74851f)) / 2f) + 0.7f;
        sLeaser.sprites[1].scale = 1f;

        float R = Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.9f;
        float G = Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.8f;
        float B = Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.7f;
        sLeaser.sprites[1].color = new Color(R, G, B);

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    private float Twinkle(float offset)
    {
        float calculation = (Mathf.Sin(2f * ((timer + offset) / 512f)) + Mathf.Cos(Mathf.PI * ((timer + offset) / 512f))) / 16f;
        // Plugin.DebugLog($"Twinkle:{calculation}f");
        return calculation;
    }

    public float atmosphericalDepthAdd;

    public float alpha;

    public bool useNonMultiplyShader;

    public float timer;
}