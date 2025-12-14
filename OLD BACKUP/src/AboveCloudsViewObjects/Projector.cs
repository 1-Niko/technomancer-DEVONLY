using UnityEngine;

namespace Slugpack;

public class SkyProjector : BackgroundScene.BackgroundSceneElement
{
    private AboveCloudsView AboveCloudsScene
    {
        get
        {
            return this.scene as AboveCloudsView;
        }
    }

    public SkyProjector(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, float atmosphericalDepthAdd) : base(aboveCloudsScene, pos, depth)
    {
        this.atmosphericalDepthAdd = atmosphericalDepthAdd;
        this.alpha = 1f;

        this.scene.LoadGraphic("atc_projector1", true, false);
    }

    public override void Update(bool eu)
    {
        blinkTime = (blinkTime + 1) % 120f;

        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[6];
        sLeaser.sprites[0] = new FSprite("atc_projector1", true);

        if (this.useNonMultiplyShader)
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObjectAlpha"];
        else
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObject"];

        sLeaser.sprites[0].anchorY = 0f;

        for (int i = 1; i < 6; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel", true);
            // sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Background"];
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

        // bool messageBlink = blinkTime < 6f;
        // 
        // sLeaser.sprites[1].x = towerOffset.x + vector.x;
        // sLeaser.sprites[1].y = towerOffset.y + vector.y;
        // sLeaser.sprites[1].color = new UnityEngine.Color(1f, 0.4f, 0.4f);
        // 
        // sLeaser.sprites[2].x = towerOffset.x + vector.x;
        // sLeaser.sprites[2].y = towerOffset.y + vector.y;
        // sLeaser.sprites[2].alpha = 0.5f;
        // sLeaser.sprites[2].color = Color.red;
        // 
        // sLeaser.sprites[3].x = towerOffset.x + vector.x;
        // sLeaser.sprites[3].y = towerOffset.y + vector.y;
        // sLeaser.sprites[3].alpha = 0.5f;
        // sLeaser.sprites[3].color = Color.red;
        // 
        // sLeaser.sprites[4].x = towerOffset.x + vector.x;
        // sLeaser.sprites[4].y = towerOffset.y + vector.y;
        // sLeaser.sprites[4].alpha = 0.5f;
        // sLeaser.sprites[4].color = Color.red;
        // 
        // sLeaser.sprites[5].x = towerOffset.x + vector.x;
        // sLeaser.sprites[5].y = towerOffset.y + vector.y;
        // sLeaser.sprites[5].alpha = 0.5f;
        // sLeaser.sprites[5].color = Color.red;
        // 
        // for (int i = 1; i < 6; i++)
        //     sLeaser.sprites[i].isVisible = messageBlink;

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float atmosphericalDepthAdd;

    public float alpha;

    public bool useNonMultiplyShader;

    public float blinkTime;
}