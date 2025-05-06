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
        current_angle_rad += 1f / 600f;
        blinkTime = (blinkTime + 1) % 164f;

        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        // First group is the tower/hologram/blinker, second is the fade effect
        int sprite_count = 230 + 207;
        sLeaser.sprites = new FSprite[sprite_count];
        sLeaser.sprites[0] = new FSprite("atc_projector1", true);

        if (this.useNonMultiplyShader)
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObjectAlpha"];
        else
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DistantBkgObject"];

        sLeaser.sprites[0].anchorY = 0f;

        for (int i = 1; i < sprite_count; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel", true);
            sLeaser.sprites[i].color = new UnityEngine.Color(0.937f, 0.647f, 0.015f);
            sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Background"];
        }

        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);

        Vector2 towerOffset = new Vector2(1f, 2f);
        sLeaser.sprites[0].x = vector.x + towerOffset.x;
        sLeaser.sprites[0].y = vector.y - 180f + towerOffset.y;
        sLeaser.sprites[0].alpha = this.alpha;
        sLeaser.sprites[0].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.9f, 0f, 0f);

        Karma10(sLeaser, vector);

        bool messageBlink = blinkTime < 2f;

        sLeaser.sprites[225].x = towerOffset.x + vector.x + 2f;
        sLeaser.sprites[225].y = towerOffset.y + vector.y - 121f;
        sLeaser.sprites[225].color = new UnityEngine.Color(1f, 0.4f, 0.4f);

        sLeaser.sprites[226].x = towerOffset.x + vector.x + 1f;
        sLeaser.sprites[226].y = towerOffset.y + vector.y - 121f;
        sLeaser.sprites[226].alpha = 0.5f;
        sLeaser.sprites[226].color = Color.red;

        sLeaser.sprites[227].x = towerOffset.x + vector.x + 3f;
        sLeaser.sprites[227].y = towerOffset.y + vector.y - 121f;
        sLeaser.sprites[227].alpha = 0.5f;
        sLeaser.sprites[227].color = Color.red;

        sLeaser.sprites[228].x = towerOffset.x + vector.x + 2f;
        sLeaser.sprites[228].y = towerOffset.y + vector.y - 120f;
        sLeaser.sprites[228].alpha = 0.5f;
        sLeaser.sprites[228].color = Color.red;

        sLeaser.sprites[229].x = towerOffset.x + vector.x + 2f;
        sLeaser.sprites[229].y = towerOffset.y + vector.y - 122f;
        sLeaser.sprites[229].alpha = 0.5f;
        sLeaser.sprites[229].color = Color.red;

        for (int i = 225; i < 230; i++)
            sLeaser.sprites[i].isVisible = messageBlink;

        Vector2 projectionPoint = new Vector2(0, -118f);

        float targetHeight = 0f;
        int granularity = 100;
        for (int i = 0; i < granularity; i++)
        {
            float useHeight = (float)i / (float)granularity * 0.5f;
            if (!IsLineObstructedAtHeight(current_angle_rad, useHeight, projectionPoint))
            {
                targetHeight = useHeight;
            }
        }

        if ((current_angle_rad % Mathf.PI) > 1.3f && (current_angle_rad % Mathf.PI) < 1.35f)
            targetHeight = 0.01f;

        float dt = 1f / 40f;

        smoothedHeight = Mathf.Lerp(smoothedHeight, targetHeight, heightLerpSpeed * dt);

        Vector3 bounds = GetHorizontalBoundsAtHeight(current_angle_rad, smoothedHeight);

        float smallestValidHeight = 28f;

        for (int i = 28; i < 207; i++)
        {
            if ((vector.y + i - 116f) >= (vector.y + bounds.z))
                break;
            smallestValidHeight = (float)i;
        }

        float a = 0.3f;
        float b = 0f;

        for (int i = 0; i < 207; i++)
        {
            sLeaser.sprites[230 + i].x = (vector.x + (bounds.x + bounds.y) / 2f);
            sLeaser.sprites[230 + i].y = vector.y + i - 116f;
            if ((vector.y + i - 116f) >= (vector.y + bounds.z))
            {
                Vector3 sBounds = GetHorizontalBoundsAtHeight(current_angle_rad, (i - 27f) / 180f);
                sLeaser.sprites[230 + i].x = (vector.x + (sBounds.x + sBounds.y) / 2f);
                sLeaser.sprites[230 + i].scaleX = Mathf.Abs(sBounds.x - sBounds.y);
            }
            else
            {
                Vector3 sBounds = GetHorizontalBoundsAtHeight(current_angle_rad, (smallestValidHeight - 27f) / 180f);
                sLeaser.sprites[230 + i].x = (vector.x + (sBounds.x + sBounds.y) / 2f) - ((((sBounds.x + sBounds.y) / 2f) - projectionPoint.x) * (1f - i / smallestValidHeight));
                sLeaser.sprites[230 + i].scaleX = (i / smallestValidHeight) * Mathf.Abs(sBounds.x - sBounds.y);// Mathf.Max(bounds.x, bounds.y) - Mathf.Min(bounds.x, bounds.y);
            }
            sLeaser.sprites[230 + i].color = new UnityEngine.Color(0.937f, 0.647f, 0.015f, Mathf.InverseLerp(0f, 1f, a + (i / 206f) * (b - a)));
            sLeaser.sprites[230 + i].alpha = 0.5f;
        }

        // sLeaser.sprites[230].x = vector.x + (bounds.x + bounds.y) / 2f;
        // sLeaser.sprites[230].y = vector.y + bounds.z;
        // sLeaser.sprites[230].scaleX = Mathf.Max(bounds.x, bounds.y) - Mathf.Min(bounds.x, bounds.y);
        // sLeaser.sprites[230].color = Color.red;

        // sLeaser.sprites[230].x = vector.x + (bounds.x + projectionPoint.x) / 2f;
        // sLeaser.sprites[230].y = vector.y + (bounds.z + projectionPoint.y) / 2f;
        // sLeaser.sprites[230].scaleY = RWCustom.Custom.Dist(new Vector2(bounds.x, bounds.z), projectionPoint);
        // sLeaser.sprites[230].rotation = RWCustom.Custom.AimFromOneVectorToAnother(sLeaser.sprites[230].GetPosition(), new Vector2(bounds.x, bounds.z) + vector);
        // sLeaser.sprites[230].color = (IsLineObstructedAtHeight(current_angle_rad, current_angle_rad % 1, projectionPoint)) ? Color.red : Color.white;

        // sLeaser.sprites[231].x = vector.x + (bounds.y + projectionPoint.x) / 2f;
        // sLeaser.sprites[231].y = vector.y + (bounds.z + projectionPoint.y) / 2f;
        // sLeaser.sprites[231].scaleY = RWCustom.Custom.Dist(new Vector2(bounds.y, bounds.z), projectionPoint);
        // sLeaser.sprites[231].rotation = RWCustom.Custom.AimFromOneVectorToAnother(sLeaser.sprites[231].GetPosition(), new Vector2(bounds.y, bounds.z) + vector);
        // sLeaser.sprites[231].color = (IsLineObstructedAtHeight(current_angle_rad, current_angle_rad % 1, projectionPoint)) ? Color.red : Color.white;

        prevHeight = height;

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public void Karma10(RoomCamera.SpriteLeaser sLeaser, Vector2 vector)
    {
        float cosA = Mathf.Cos(current_angle_rad);
        float sinA = Mathf.Sin(current_angle_rad);
        Vector2 p1, p2, midpoint;
        float dist;
        float minZOverall = -827.134892f;
        float zRangeRecip = 0.500345f;
        float sliceStep = 0.500000f;
        int spriteIndex = 1;
        float z1_cam, z2_cam, z_mid, depth;
        float t_start, t_end, t_mid;
        Vector2 seg_p1, seg_p2, seg_mid;
        float seg_dist, seg_rot;

        p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 89.73f);
        p2 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + 86.67f);
        z1_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.08f;
        z2_cam = (0.11f * cosA) + (0.24f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + 86.67f);
        p2 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + 77.70f);
        z1_cam = (0.11f * cosA) + (0.24f * sinA) + -826.08f;
        z2_cam = (0.17f * cosA) + (0.47f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + 77.70f);
        p2 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + 63.45f);
        z1_cam = (0.17f * cosA) + (0.47f * sinA) + -826.08f;
        z2_cam = (0.22f * cosA) + (0.67f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + 63.45f);
        p2 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + 44.86f);
        z1_cam = (0.22f * cosA) + (0.67f * sinA) + -826.09f;
        z2_cam = (0.26f * cosA) + (0.83f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + 44.86f);
        p2 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + 23.22f);
        z1_cam = (0.26f * cosA) + (0.83f * sinA) + -826.11f;
        z2_cam = (0.28f * cosA) + (0.92f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + 23.22f);
        p2 = new Vector2((86.12f * cosA) + (-26.16f * sinA), (-1.58f * cosA) + (-5.21f * sinA));
        z1_cam = (0.28f * cosA) + (0.92f * sinA) + -826.12f;
        z2_cam = (0.29f * cosA) + (0.96f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((86.12f * cosA) + (-26.16f * sinA), (-1.58f * cosA) + (-5.21f * sinA));
        p2 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + -23.22f);
        z1_cam = (0.29f * cosA) + (0.96f * sinA) + -826.14f;
        z2_cam = (0.28f * cosA) + (0.92f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + -23.22f);
        p2 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + -44.86f);
        z1_cam = (0.28f * cosA) + (0.92f * sinA) + -826.15f;
        z2_cam = (0.26f * cosA) + (0.83f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + -44.86f);
        p2 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + -63.45f);
        z1_cam = (0.26f * cosA) + (0.83f * sinA) + -826.17f;
        z2_cam = (0.22f * cosA) + (0.67f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + -63.45f);
        p2 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + -77.71f);
        z1_cam = (0.22f * cosA) + (0.67f * sinA) + -826.18f;
        z2_cam = (0.17f * cosA) + (0.47f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + -77.71f);
        p2 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + -86.67f);
        z1_cam = (0.17f * cosA) + (0.47f * sinA) + -826.19f;
        z2_cam = (0.11f * cosA) + (0.24f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + -86.67f);
        p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -89.73f);
        z1_cam = (0.11f * cosA) + (0.24f * sinA) + -826.19f;
        z2_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.20f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -89.73f);
        p2 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + -86.67f);
        z1_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.20f;
        z2_cam = (-0.01f * cosA) + (-0.26f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + -86.67f);
        p2 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + -77.71f);
        z1_cam = (-0.01f * cosA) + (-0.26f * sinA) + -826.19f;
        z2_cam = (-0.07f * cosA) + (-0.50f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + -77.71f);
        p2 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + -63.45f);
        z1_cam = (-0.07f * cosA) + (-0.50f * sinA) + -826.19f;
        z2_cam = (-0.12f * cosA) + (-0.70f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + -63.45f);
        p2 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + -44.86f);
        z1_cam = (-0.12f * cosA) + (-0.70f * sinA) + -826.18f;
        z2_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + -44.86f);
        p2 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + -23.22f);
        z1_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.17f;
        z2_cam = (-0.19f * cosA) + (-0.95f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + -23.22f);
        p2 = new Vector2((-88.30f * cosA) + (17.44f * sinA), (1.06f * cosA) + (5.34f * sinA));
        z1_cam = (-0.19f * cosA) + (-0.95f * sinA) + -826.15f;
        z2_cam = (-0.19f * cosA) + (-0.98f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-88.30f * cosA) + (17.44f * sinA), (1.06f * cosA) + (5.34f * sinA));
        p2 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + 23.22f);
        z1_cam = (-0.19f * cosA) + (-0.98f * sinA) + -826.14f;
        z2_cam = (-0.19f * cosA) + (-0.95f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + 23.22f);
        p2 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + 44.86f);
        z1_cam = (-0.19f * cosA) + (-0.95f * sinA) + -826.12f;
        z2_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + 44.86f);
        p2 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + 63.45f);
        z1_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.11f;
        z2_cam = (-0.12f * cosA) + (-0.70f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + 63.45f);
        p2 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + 77.70f);
        z1_cam = (-0.12f * cosA) + (-0.70f * sinA) + -826.09f;
        z2_cam = (-0.07f * cosA) + (-0.50f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + 77.70f);
        p2 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + 86.67f);
        z1_cam = (-0.07f * cosA) + (-0.50f * sinA) + -826.08f;
        z2_cam = (-0.01f * cosA) + (-0.26f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + 86.67f);
        p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 89.73f);
        z1_cam = (-0.01f * cosA) + (-0.26f * sinA) + -826.08f;
        z2_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 89.73f);
        p2 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + 86.67f);
        z1_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.08f;
        z2_cam = (0.01f * cosA) + (0.26f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + 86.67f);
        p2 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + 77.70f);
        z1_cam = (0.01f * cosA) + (0.26f * sinA) + -826.08f;
        z2_cam = (0.07f * cosA) + (0.50f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + 77.70f);
        p2 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + 63.45f);
        z1_cam = (0.07f * cosA) + (0.50f * sinA) + -826.08f;
        z2_cam = (0.12f * cosA) + (0.70f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + 63.45f);
        p2 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + 44.86f);
        z1_cam = (0.12f * cosA) + (0.70f * sinA) + -826.09f;
        z2_cam = (0.16f * cosA) + (0.85f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + 44.86f);
        p2 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + 23.22f);
        z1_cam = (0.16f * cosA) + (0.85f * sinA) + -826.11f;
        z2_cam = (0.19f * cosA) + (0.95f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + 23.22f);
        p2 = new Vector2((88.30f * cosA) + (-17.44f * sinA), (-1.06f * cosA) + (-5.34f * sinA));
        z1_cam = (0.19f * cosA) + (0.95f * sinA) + -826.12f;
        z2_cam = (0.19f * cosA) + (0.98f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((88.30f * cosA) + (-17.44f * sinA), (-1.06f * cosA) + (-5.34f * sinA));
        p2 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + -23.22f);
        z1_cam = (0.19f * cosA) + (0.98f * sinA) + -826.14f;
        z2_cam = (0.19f * cosA) + (0.95f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + -23.22f);
        p2 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + -44.86f);
        z1_cam = (0.19f * cosA) + (0.95f * sinA) + -826.15f;
        z2_cam = (0.16f * cosA) + (0.85f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + -44.86f);
        p2 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + -63.45f);
        z1_cam = (0.16f * cosA) + (0.85f * sinA) + -826.17f;
        z2_cam = (0.12f * cosA) + (0.70f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + -63.45f);
        p2 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + -77.71f);
        z1_cam = (0.12f * cosA) + (0.70f * sinA) + -826.18f;
        z2_cam = (0.07f * cosA) + (0.50f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + -77.71f);
        p2 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + -86.67f);
        z1_cam = (0.07f * cosA) + (0.50f * sinA) + -826.19f;
        z2_cam = (0.01f * cosA) + (0.26f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + -86.67f);
        p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -89.73f);
        z1_cam = (0.01f * cosA) + (0.26f * sinA) + -826.19f;
        z2_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.20f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -89.73f);
        p2 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + -86.67f);
        z1_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.20f;
        z2_cam = (-0.11f * cosA) + (-0.24f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + -86.67f);
        p2 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + -77.71f);
        z1_cam = (-0.11f * cosA) + (-0.24f * sinA) + -826.19f;
        z2_cam = (-0.17f * cosA) + (-0.47f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + -77.71f);
        p2 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + -63.45f);
        z1_cam = (-0.17f * cosA) + (-0.47f * sinA) + -826.19f;
        z2_cam = (-0.22f * cosA) + (-0.67f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + -63.45f);
        p2 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + -44.86f);
        z1_cam = (-0.22f * cosA) + (-0.67f * sinA) + -826.18f;
        z2_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + -44.86f);
        p2 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + -23.22f);
        z1_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.17f;
        z2_cam = (-0.28f * cosA) + (-0.92f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + -23.22f);
        p2 = new Vector2((-86.12f * cosA) + (26.16f * sinA), (1.58f * cosA) + (5.21f * sinA));
        z1_cam = (-0.28f * cosA) + (-0.92f * sinA) + -826.15f;
        z2_cam = (-0.29f * cosA) + (-0.96f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-86.12f * cosA) + (26.16f * sinA), (1.58f * cosA) + (5.21f * sinA));
        p2 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + 23.22f);
        z1_cam = (-0.29f * cosA) + (-0.96f * sinA) + -826.14f;
        z2_cam = (-0.28f * cosA) + (-0.92f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + 23.22f);
        p2 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + 44.86f);
        z1_cam = (-0.28f * cosA) + (-0.92f * sinA) + -826.12f;
        z2_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + 44.86f);
        p2 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + 63.45f);
        z1_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.11f;
        z2_cam = (-0.22f * cosA) + (-0.67f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + 63.45f);
        p2 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + 77.70f);
        z1_cam = (-0.22f * cosA) + (-0.67f * sinA) + -826.09f;
        z2_cam = (-0.17f * cosA) + (-0.47f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + 77.70f);
        p2 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + 86.67f);
        z1_cam = (-0.17f * cosA) + (-0.47f * sinA) + -826.08f;
        z2_cam = (-0.11f * cosA) + (-0.24f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + 86.67f);
        p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 89.73f);
        z1_cam = (-0.11f * cosA) + (-0.24f * sinA) + -826.08f;
        z2_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + 59.83f);
        p2 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + 69.93f);
        z1_cam = (0.19f * cosA) + (0.57f * sinA) + -826.10f;
        z2_cam = (0.16f * cosA) + (0.42f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + 69.93f);
        p2 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + 78.00f);
        z1_cam = (0.16f * cosA) + (0.42f * sinA) + -826.09f;
        z2_cam = (0.10f * cosA) + (0.21f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + 78.00f);
        p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 80.75f);
        z1_cam = (0.10f * cosA) + (0.21f * sinA) + -826.08f;
        z2_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 80.75f);
        p2 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + 78.00f);
        z1_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.08f;
        z2_cam = (-0.01f * cosA) + (-0.24f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + 78.00f);
        p2 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + 69.93f);
        z1_cam = (-0.01f * cosA) + (-0.24f * sinA) + -826.08f;
        z2_cam = (-0.06f * cosA) + (-0.45f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + 69.93f);
        p2 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + 59.83f);
        z1_cam = (-0.06f * cosA) + (-0.45f * sinA) + -826.09f;
        z2_cam = (-0.10f * cosA) + (-0.59f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + 59.83f);
        p2 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + 6.32f);
        z1_cam = (-0.10f * cosA) + (-0.59f * sinA) + -826.10f;
        z2_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.13f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + 6.32f);
        p2 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + 59.83f);
        z1_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.13f;
        z2_cam = (0.19f * cosA) + (0.57f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + -59.83f);
        p2 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + -69.94f);
        z1_cam = (0.19f * cosA) + (0.57f * sinA) + -826.18f;
        z2_cam = (0.16f * cosA) + (0.42f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + -69.94f);
        p2 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + -78.00f);
        z1_cam = (0.16f * cosA) + (0.42f * sinA) + -826.18f;
        z2_cam = (0.10f * cosA) + (0.21f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + -78.00f);
        p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -80.76f);
        z1_cam = (0.10f * cosA) + (0.21f * sinA) + -826.19f;
        z2_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -80.76f);
        p2 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + -78.00f);
        z1_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.19f;
        z2_cam = (-0.01f * cosA) + (-0.24f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + -78.00f);
        p2 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + -69.94f);
        z1_cam = (-0.01f * cosA) + (-0.24f * sinA) + -826.19f;
        z2_cam = (-0.06f * cosA) + (-0.45f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + -69.94f);
        p2 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + -59.83f);
        z1_cam = (-0.06f * cosA) + (-0.45f * sinA) + -826.18f;
        z2_cam = (-0.10f * cosA) + (-0.59f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + -59.83f);
        p2 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + -6.32f);
        z1_cam = (-0.10f * cosA) + (-0.59f * sinA) + -826.18f;
        z2_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + -6.32f);
        p2 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + -59.83f);
        z1_cam = (0.05f * cosA) + (-0.01f * sinA) + -826.14f;
        z2_cam = (0.19f * cosA) + (0.57f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + 59.83f);
        p2 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + 69.93f);
        z1_cam = (0.10f * cosA) + (0.59f * sinA) + -826.10f;
        z2_cam = (0.06f * cosA) + (0.45f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + 69.93f);
        p2 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + 78.00f);
        z1_cam = (0.06f * cosA) + (0.45f * sinA) + -826.09f;
        z2_cam = (0.01f * cosA) + (0.24f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + 78.00f);
        p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 80.75f);
        z1_cam = (0.01f * cosA) + (0.24f * sinA) + -826.08f;
        z2_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 80.75f);
        p2 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + 78.00f);
        z1_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.08f;
        z2_cam = (-0.10f * cosA) + (-0.21f * sinA) + -826.08f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + 78.00f);
        p2 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + 69.93f);
        z1_cam = (-0.10f * cosA) + (-0.21f * sinA) + -826.08f;
        z2_cam = (-0.16f * cosA) + (-0.42f * sinA) + -826.09f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + 69.93f);
        p2 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + 59.83f);
        z1_cam = (-0.16f * cosA) + (-0.42f * sinA) + -826.09f;
        z2_cam = (-0.19f * cosA) + (-0.57f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + 59.83f);
        p2 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 6.32f);
        z1_cam = (-0.19f * cosA) + (-0.57f * sinA) + -826.10f;
        z2_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.13f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 6.32f);
        p2 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + 59.83f);
        z1_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.13f;
        z2_cam = (0.10f * cosA) + (0.59f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + -59.83f);
        p2 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + -69.94f);
        z1_cam = (0.10f * cosA) + (0.59f * sinA) + -826.18f;
        z2_cam = (0.06f * cosA) + (0.45f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + -69.94f);
        p2 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + -78.00f);
        z1_cam = (0.06f * cosA) + (0.45f * sinA) + -826.18f;
        z2_cam = (0.01f * cosA) + (0.24f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + -78.00f);
        p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -80.76f);
        z1_cam = (0.01f * cosA) + (0.24f * sinA) + -826.19f;
        z2_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -80.76f);
        p2 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + -78.00f);
        z1_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.19f;
        z2_cam = (-0.10f * cosA) + (-0.21f * sinA) + -826.19f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + -78.00f);
        p2 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + -69.94f);
        z1_cam = (-0.10f * cosA) + (-0.21f * sinA) + -826.19f;
        z2_cam = (-0.16f * cosA) + (-0.42f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + -69.94f);
        p2 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + -59.83f);
        z1_cam = (-0.16f * cosA) + (-0.42f * sinA) + -826.18f;
        z2_cam = (-0.19f * cosA) + (-0.57f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + -59.83f);
        p2 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -6.32f);
        z1_cam = (-0.19f * cosA) + (-0.57f * sinA) + -826.18f;
        z2_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -6.32f);
        p2 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + -59.83f);
        z1_cam = (-0.05f * cosA) + (0.01f * sinA) + -826.14f;
        z2_cam = (0.10f * cosA) + (0.59f * sinA) + -826.18f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + 53.54f);
        p2 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + 40.38f);
        z1_cam = (0.21f * cosA) + (0.63f * sinA) + -826.10f;
        z2_cam = (0.24f * cosA) + (0.74f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + 40.38f);
        p2 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + 20.90f);
        z1_cam = (0.24f * cosA) + (0.74f * sinA) + -826.11f;
        z2_cam = (0.26f * cosA) + (0.83f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + 20.90f);
        p2 = new Vector2((77.40f * cosA) + (-23.98f * sinA), (-1.45f * cosA) + (-4.68f * sinA));
        z1_cam = (0.26f * cosA) + (0.83f * sinA) + -826.12f;
        z2_cam = (0.27f * cosA) + (0.86f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((77.40f * cosA) + (-23.98f * sinA), (-1.45f * cosA) + (-4.68f * sinA));
        p2 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + -20.90f);
        z1_cam = (0.27f * cosA) + (0.86f * sinA) + -826.14f;
        z2_cam = (0.26f * cosA) + (0.83f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + -20.90f);
        p2 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + -40.38f);
        z1_cam = (0.26f * cosA) + (0.83f * sinA) + -826.15f;
        z2_cam = (0.24f * cosA) + (0.74f * sinA) + -826.16f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + -40.38f);
        p2 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + -53.54f);
        z1_cam = (0.24f * cosA) + (0.74f * sinA) + -826.16f;
        z2_cam = (0.21f * cosA) + (0.63f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + -53.54f);
        p2 = new Vector2((5.05f * cosA) + (-5.90f * sinA), (-0.36f * cosA) + (-0.31f * sinA) + 0.03f);
        z1_cam = (0.21f * cosA) + (0.63f * sinA) + -826.17f;
        z2_cam = (0.07f * cosA) + (0.06f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((5.05f * cosA) + (-5.90f * sinA), (-0.36f * cosA) + (-0.31f * sinA) + 0.03f);
        p2 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + 53.54f);
        z1_cam = (0.07f * cosA) + (0.06f * sinA) + -826.14f;
        z2_cam = (0.21f * cosA) + (0.63f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + 53.54f);
        p2 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + 40.38f);
        z1_cam = (-0.11f * cosA) + (-0.66f * sinA) + -826.10f;
        z2_cam = (-0.14f * cosA) + (-0.77f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + 40.38f);
        p2 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + 20.90f);
        z1_cam = (-0.14f * cosA) + (-0.77f * sinA) + -826.11f;
        z2_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + 20.90f);
        p2 = new Vector2((-79.58f * cosA) + (15.26f * sinA), (0.92f * cosA) + (4.82f * sinA));
        z1_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.12f;
        z2_cam = (-0.17f * cosA) + (-0.88f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-79.58f * cosA) + (15.26f * sinA), (0.92f * cosA) + (4.82f * sinA));
        p2 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + -20.90f);
        z1_cam = (-0.17f * cosA) + (-0.88f * sinA) + -826.14f;
        z2_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + -20.90f);
        p2 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + -40.38f);
        z1_cam = (-0.16f * cosA) + (-0.85f * sinA) + -826.15f;
        z2_cam = (-0.14f * cosA) + (-0.77f * sinA) + -826.16f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + -40.38f);
        p2 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + -53.54f);
        z1_cam = (-0.14f * cosA) + (-0.77f * sinA) + -826.16f;
        z2_cam = (-0.11f * cosA) + (-0.66f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + -53.54f);
        p2 = new Vector2((-7.23f * cosA) + (-2.83f * sinA), (-0.17f * cosA) + (0.44f * sinA) + 0.03f);
        z1_cam = (-0.11f * cosA) + (-0.66f * sinA) + -826.17f;
        z2_cam = (0.03f * cosA) + (-0.08f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-7.23f * cosA) + (-2.83f * sinA), (-0.17f * cosA) + (0.44f * sinA) + 0.03f);
        p2 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + 53.54f);
        z1_cam = (0.03f * cosA) + (-0.08f * sinA) + -826.14f;
        z2_cam = (-0.11f * cosA) + (-0.66f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + 53.54f);
        p2 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + 40.38f);
        z1_cam = (0.11f * cosA) + (0.66f * sinA) + -826.10f;
        z2_cam = (0.14f * cosA) + (0.77f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + 40.38f);
        p2 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + 20.90f);
        z1_cam = (0.14f * cosA) + (0.77f * sinA) + -826.11f;
        z2_cam = (0.16f * cosA) + (0.85f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + 20.90f);
        p2 = new Vector2((79.58f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-4.82f * sinA));
        z1_cam = (0.16f * cosA) + (0.85f * sinA) + -826.12f;
        z2_cam = (0.17f * cosA) + (0.88f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((79.58f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-4.82f * sinA));
        p2 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + -20.90f);
        z1_cam = (0.17f * cosA) + (0.88f * sinA) + -826.14f;
        z2_cam = (0.16f * cosA) + (0.85f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + -20.90f);
        p2 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + -40.38f);
        z1_cam = (0.16f * cosA) + (0.85f * sinA) + -826.15f;
        z2_cam = (0.14f * cosA) + (0.77f * sinA) + -826.16f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + -40.38f);
        p2 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + -53.54f);
        z1_cam = (0.14f * cosA) + (0.77f * sinA) + -826.16f;
        z2_cam = (0.11f * cosA) + (0.66f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + -53.54f);
        p2 = new Vector2((7.23f * cosA) + (2.83f * sinA), (0.17f * cosA) + (-0.44f * sinA) + 0.03f);
        z1_cam = (0.11f * cosA) + (0.66f * sinA) + -826.17f;
        z2_cam = (-0.03f * cosA) + (0.08f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((7.23f * cosA) + (2.83f * sinA), (0.17f * cosA) + (-0.44f * sinA) + 0.03f);
        p2 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + 53.54f);
        z1_cam = (-0.03f * cosA) + (0.08f * sinA) + -826.14f;
        z2_cam = (0.11f * cosA) + (0.66f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + 53.54f);
        p2 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + 40.38f);
        z1_cam = (-0.21f * cosA) + (-0.63f * sinA) + -826.10f;
        z2_cam = (-0.24f * cosA) + (-0.74f * sinA) + -826.11f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + 40.38f);
        p2 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + 20.90f);
        z1_cam = (-0.24f * cosA) + (-0.74f * sinA) + -826.11f;
        z2_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.12f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + 20.90f);
        p2 = new Vector2((-77.40f * cosA) + (23.98f * sinA), (1.45f * cosA) + (4.68f * sinA));
        z1_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.12f;
        z2_cam = (-0.27f * cosA) + (-0.86f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-77.40f * cosA) + (23.98f * sinA), (1.45f * cosA) + (4.68f * sinA));
        p2 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + -20.90f);
        z1_cam = (-0.27f * cosA) + (-0.86f * sinA) + -826.14f;
        z2_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.15f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + -20.90f);
        p2 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + -40.38f);
        z1_cam = (-0.26f * cosA) + (-0.83f * sinA) + -826.15f;
        z2_cam = (-0.24f * cosA) + (-0.74f * sinA) + -826.16f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + -40.38f);
        p2 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + -53.54f);
        z1_cam = (-0.24f * cosA) + (-0.74f * sinA) + -826.16f;
        z2_cam = (-0.21f * cosA) + (-0.63f * sinA) + -826.17f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + -53.54f);
        p2 = new Vector2((-5.05f * cosA) + (5.90f * sinA), (0.36f * cosA) + (0.31f * sinA) + 0.03f);
        z1_cam = (-0.21f * cosA) + (-0.63f * sinA) + -826.17f;
        z2_cam = (-0.07f * cosA) + (-0.06f * sinA) + -826.14f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }

        p1 = new Vector2((-5.05f * cosA) + (5.90f * sinA), (0.36f * cosA) + (0.31f * sinA) + 0.03f);
        p2 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + 53.54f);
        z1_cam = (-0.07f * cosA) + (-0.06f * sinA) + -826.14f;
        z2_cam = (-0.21f * cosA) + (-0.63f * sinA) + -826.10f;

        for (float t = 0f; t < 0.9999f; t += sliceStep)
        {
            t_start = t;
            t_end = Mathf.Min(t + sliceStep, 1.0f);
            t_mid = (t_start + t_end) / 2f;

            seg_p1 = Vector2.LerpUnclamped(p1, p2, t_start);
            seg_p2 = Vector2.LerpUnclamped(p1, p2, t_end);
            seg_mid = (seg_p1 + seg_p2) / 2f;

            z_mid = Mathf.LerpUnclamped(z1_cam, z2_cam, t_mid);
            depth = (z_mid - minZOverall) * zRangeRecip;
            depth = Mathf.Clamp01(depth);

            seg_dist = Vector2.Distance(seg_p1, seg_p2);
            seg_rot = RWCustom.Custom.AimFromOneVectorToAnother(seg_mid, seg_p1);

            if (spriteIndex < sLeaser.sprites.Length)
            {
                sLeaser.sprites[spriteIndex].x = vector.x + seg_mid.x;
                sLeaser.sprites[spriteIndex].y = vector.y + seg_mid.y;
                sLeaser.sprites[spriteIndex].scaleY = Mathf.Max(0f, seg_dist);
                sLeaser.sprites[spriteIndex].rotation = seg_rot;
                sLeaser.sprites[spriteIndex].color = ColourSegment(depth);
                sLeaser.sprites[spriteIndex].isVisible = true;
                spriteIndex++;
            }
        }
    }

    public float atmosphericalDepthAdd;

    public float alpha;

    public bool useNonMultiplyShader;

    public float current_angle_rad;

    public float blinkTime;

    public float height;
    public float prevHeight = 0f;

    public float smoothedHeight = 0f;
    public float heightLerpSpeed = 5.0f;

    private Color ColourSegment(float depth)
    {
        return new UnityEngine.Color(Mathf.Lerp(0.807f, 0.501f, depth), Mathf.Lerp(0.576f, 0.403f, depth), Mathf.Lerp(0.058f, 0.16f, depth));
        // return new UnityEngine.Color(0.937f, 0.647f, 0.015f);
    }

    private Vector3 GetHorizontalBoundsAtHeight(float current_angle_rad, float normalizedHeight)
    {
        float cosA = Mathf.Cos(current_angle_rad);
        float sinA = Mathf.Sin(current_angle_rad);
        float targetY = (normalizedHeight - 0.5f) * 180.00f;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float tolerance = 0.000100f;
        bool intersectionFound = false;
        Vector2 p1, p2;
        float intersectX;

        int numConfigEdges = 112;
        for (int edge_idx = 0; edge_idx < numConfigEdges; edge_idx++)
        {
            if (edge_idx == 0)
            {
                p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 89.73f);
                p2 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + 86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 1)
            {
                p1 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + 86.67f);
                p2 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + 77.70f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 2)
            {
                p1 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + 77.70f);
                p2 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + 63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 3)
            {
                p1 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + 63.45f);
                p2 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + 44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 4)
            {
                p1 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + 44.86f);
                p2 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + 23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 5)
            {
                p1 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + 23.22f);
                p2 = new Vector2((86.12f * cosA) + (-26.16f * sinA), (-1.58f * cosA) + (-5.21f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 6)
            {
                p1 = new Vector2((86.12f * cosA) + (-26.16f * sinA), (-1.58f * cosA) + (-5.21f * sinA));
                p2 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + -23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 7)
            {
                p1 = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + -23.22f);
                p2 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + -44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 8)
            {
                p1 = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + -44.86f);
                p2 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + -63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 9)
            {
                p1 = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + -63.45f);
                p2 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + -77.71f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 10)
            {
                p1 = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + -77.71f);
                p2 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + -86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 11)
            {
                p1 = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + -86.67f);
                p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -89.73f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 12)
            {
                p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -89.73f);
                p2 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + -86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 13)
            {
                p1 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + -86.67f);
                p2 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + -77.71f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 14)
            {
                p1 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + -77.71f);
                p2 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + -63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 15)
            {
                p1 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + -63.45f);
                p2 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + -44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 16)
            {
                p1 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + -44.86f);
                p2 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + -23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 17)
            {
                p1 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + -23.22f);
                p2 = new Vector2((-88.30f * cosA) + (17.44f * sinA), (1.06f * cosA) + (5.34f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 18)
            {
                p1 = new Vector2((-88.30f * cosA) + (17.44f * sinA), (1.06f * cosA) + (5.34f * sinA));
                p2 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + 23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 19)
            {
                p1 = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + 23.22f);
                p2 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + 44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 20)
            {
                p1 = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + 44.86f);
                p2 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + 63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 21)
            {
                p1 = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + 63.45f);
                p2 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + 77.70f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 22)
            {
                p1 = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + 77.70f);
                p2 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + 86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 23)
            {
                p1 = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + 86.67f);
                p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 89.73f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 24)
            {
                p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 89.73f);
                p2 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + 86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 25)
            {
                p1 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + 86.67f);
                p2 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + 77.70f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 26)
            {
                p1 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + 77.70f);
                p2 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + 63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 27)
            {
                p1 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + 63.45f);
                p2 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + 44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 28)
            {
                p1 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + 44.86f);
                p2 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + 23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 29)
            {
                p1 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + 23.22f);
                p2 = new Vector2((88.30f * cosA) + (-17.44f * sinA), (-1.06f * cosA) + (-5.34f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 30)
            {
                p1 = new Vector2((88.30f * cosA) + (-17.44f * sinA), (-1.06f * cosA) + (-5.34f * sinA));
                p2 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + -23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 31)
            {
                p1 = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + -23.22f);
                p2 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + -44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 32)
            {
                p1 = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + -44.86f);
                p2 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + -63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 33)
            {
                p1 = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + -63.45f);
                p2 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + -77.71f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 34)
            {
                p1 = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + -77.71f);
                p2 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + -86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 35)
            {
                p1 = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + -86.67f);
                p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -89.73f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 36)
            {
                p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -89.73f);
                p2 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + -86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 37)
            {
                p1 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + -86.67f);
                p2 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + -77.71f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 38)
            {
                p1 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + -77.71f);
                p2 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + -63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 39)
            {
                p1 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + -63.45f);
                p2 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + -44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 40)
            {
                p1 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + -44.86f);
                p2 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + -23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 41)
            {
                p1 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + -23.22f);
                p2 = new Vector2((-86.12f * cosA) + (26.16f * sinA), (1.58f * cosA) + (5.21f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 42)
            {
                p1 = new Vector2((-86.12f * cosA) + (26.16f * sinA), (1.58f * cosA) + (5.21f * sinA));
                p2 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + 23.22f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 43)
            {
                p1 = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + 23.22f);
                p2 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + 44.86f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 44)
            {
                p1 = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + 44.86f);
                p2 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + 63.45f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 45)
            {
                p1 = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + 63.45f);
                p2 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + 77.70f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 46)
            {
                p1 = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + 77.70f);
                p2 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + 86.67f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 47)
            {
                p1 = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + 86.67f);
                p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 89.73f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 48)
            {
                p1 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + 59.83f);
                p2 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + 69.93f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 49)
            {
                p1 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + 69.93f);
                p2 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + 78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 50)
            {
                p1 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + 78.00f);
                p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 80.75f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 51)
            {
                p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 80.75f);
                p2 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + 78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 52)
            {
                p1 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + 78.00f);
                p2 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + 69.93f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 53)
            {
                p1 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + 69.93f);
                p2 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + 59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 54)
            {
                p1 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + 59.83f);
                p2 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + 6.32f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 55)
            {
                p1 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + 6.32f);
                p2 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + 59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 56)
            {
                p1 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + -59.83f);
                p2 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + -69.94f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 57)
            {
                p1 = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + -69.94f);
                p2 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + -78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 58)
            {
                p1 = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + -78.00f);
                p2 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -80.76f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 59)
            {
                p1 = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -80.76f);
                p2 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + -78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 60)
            {
                p1 = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + -78.00f);
                p2 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + -69.94f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 61)
            {
                p1 = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + -69.94f);
                p2 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + -59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 62)
            {
                p1 = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + -59.83f);
                p2 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + -6.32f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 63)
            {
                p1 = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + -6.32f);
                p2 = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + -59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 64)
            {
                p1 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + 59.83f);
                p2 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + 69.93f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 65)
            {
                p1 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + 69.93f);
                p2 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + 78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 66)
            {
                p1 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + 78.00f);
                p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 80.75f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 67)
            {
                p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 80.75f);
                p2 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + 78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 68)
            {
                p1 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + 78.00f);
                p2 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + 69.93f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 69)
            {
                p1 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + 69.93f);
                p2 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + 59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 70)
            {
                p1 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + 59.83f);
                p2 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 6.32f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 71)
            {
                p1 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 6.32f);
                p2 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + 59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 72)
            {
                p1 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + -59.83f);
                p2 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + -69.94f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 73)
            {
                p1 = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + -69.94f);
                p2 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + -78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 74)
            {
                p1 = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + -78.00f);
                p2 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -80.76f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 75)
            {
                p1 = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -80.76f);
                p2 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + -78.00f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 76)
            {
                p1 = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + -78.00f);
                p2 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + -69.94f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 77)
            {
                p1 = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + -69.94f);
                p2 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + -59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 78)
            {
                p1 = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + -59.83f);
                p2 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -6.32f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 79)
            {
                p1 = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -6.32f);
                p2 = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + -59.83f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 80)
            {
                p1 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + 53.54f);
                p2 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + 40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 81)
            {
                p1 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + 40.38f);
                p2 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + 20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 82)
            {
                p1 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + 20.90f);
                p2 = new Vector2((77.40f * cosA) + (-23.98f * sinA), (-1.45f * cosA) + (-4.68f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 83)
            {
                p1 = new Vector2((77.40f * cosA) + (-23.98f * sinA), (-1.45f * cosA) + (-4.68f * sinA));
                p2 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + -20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 84)
            {
                p1 = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + -20.90f);
                p2 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + -40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 85)
            {
                p1 = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + -40.38f);
                p2 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + -53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 86)
            {
                p1 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + -53.54f);
                p2 = new Vector2((5.05f * cosA) + (-5.90f * sinA), (-0.36f * cosA) + (-0.31f * sinA) + 0.03f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 87)
            {
                p1 = new Vector2((5.05f * cosA) + (-5.90f * sinA), (-0.36f * cosA) + (-0.31f * sinA) + 0.03f);
                p2 = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + 53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 88)
            {
                p1 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + 53.54f);
                p2 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + 40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 89)
            {
                p1 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + 40.38f);
                p2 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + 20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 90)
            {
                p1 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + 20.90f);
                p2 = new Vector2((-79.58f * cosA) + (15.26f * sinA), (0.92f * cosA) + (4.82f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 91)
            {
                p1 = new Vector2((-79.58f * cosA) + (15.26f * sinA), (0.92f * cosA) + (4.82f * sinA));
                p2 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + -20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 92)
            {
                p1 = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + -20.90f);
                p2 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + -40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 93)
            {
                p1 = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + -40.38f);
                p2 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + -53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 94)
            {
                p1 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + -53.54f);
                p2 = new Vector2((-7.23f * cosA) + (-2.83f * sinA), (-0.17f * cosA) + (0.44f * sinA) + 0.03f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 95)
            {
                p1 = new Vector2((-7.23f * cosA) + (-2.83f * sinA), (-0.17f * cosA) + (0.44f * sinA) + 0.03f);
                p2 = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + 53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 96)
            {
                p1 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + 53.54f);
                p2 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + 40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 97)
            {
                p1 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + 40.38f);
                p2 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + 20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 98)
            {
                p1 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + 20.90f);
                p2 = new Vector2((79.58f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-4.82f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 99)
            {
                p1 = new Vector2((79.58f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-4.82f * sinA));
                p2 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + -20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 100)
            {
                p1 = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + -20.90f);
                p2 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + -40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 101)
            {
                p1 = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + -40.38f);
                p2 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + -53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 102)
            {
                p1 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + -53.54f);
                p2 = new Vector2((7.23f * cosA) + (2.83f * sinA), (0.17f * cosA) + (-0.44f * sinA) + 0.03f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 103)
            {
                p1 = new Vector2((7.23f * cosA) + (2.83f * sinA), (0.17f * cosA) + (-0.44f * sinA) + 0.03f);
                p2 = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + 53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 104)
            {
                p1 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + 53.54f);
                p2 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + 40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 105)
            {
                p1 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + 40.38f);
                p2 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + 20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 106)
            {
                p1 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + 20.90f);
                p2 = new Vector2((-77.40f * cosA) + (23.98f * sinA), (1.45f * cosA) + (4.68f * sinA));

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 107)
            {
                p1 = new Vector2((-77.40f * cosA) + (23.98f * sinA), (1.45f * cosA) + (4.68f * sinA));
                p2 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + -20.90f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 108)
            {
                p1 = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + -20.90f);
                p2 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + -40.38f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 109)
            {
                p1 = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + -40.38f);
                p2 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + -53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 110)
            {
                p1 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + -53.54f);
                p2 = new Vector2((-5.05f * cosA) + (5.90f * sinA), (0.36f * cosA) + (0.31f * sinA) + 0.03f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
            if (edge_idx == 111)
            {
                p1 = new Vector2((-5.05f * cosA) + (5.90f * sinA), (0.36f * cosA) + (0.31f * sinA) + 0.03f);
                p2 = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + 53.54f);

                if ((p1.y <= targetY + tolerance && targetY - tolerance <= p2.y) || (p2.y <= targetY + tolerance && targetY - tolerance <= p1.y))
                {
                    intersectionFound = true;
                    if (Mathf.Abs(p1.x - p2.x) < tolerance)
                    {
                        intersectX = p1.x;
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                    else if (Mathf.Abs(p1.y - p2.y) < tolerance)
                    {
                        minX = Mathf.Min(minX, p1.x, p2.x);
                        maxX = Mathf.Max(maxX, p1.x, p2.x);
                    }
                    else
                    {
                        float t = (targetY - p1.y) / (p2.y - p1.y);
                        intersectX = p1.x + t * (p2.x - p1.x);
                        minX = Mathf.Min(minX, intersectX);
                        maxX = Mathf.Max(maxX, intersectX);
                    }
                }
            }
        }

        if (!intersectionFound)
        {
            return new Vector3(float.MaxValue, float.MinValue, targetY);
        }

        return new Vector3(minX, maxX, targetY);
    }

    private float Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        if (Mathf.Abs(val) < 1e-05f) return 0f;
        return (val > 0f) ? 1f : 2f;
    }

    private bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        return (q.x <= Mathf.Max(p.x, r.x) + 1e-05f && q.x >= Mathf.Min(p.x, r.x) - 1e-05f &&
                q.y <= Mathf.Max(p.y, r.y) + 1e-05f && q.y >= Mathf.Min(p.y, r.y) - 1e-05f);
    }

    private bool SegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        float e = 1e-05f;
        if ((Vector2.Distance(p1, p2) < e) || (Vector2.Distance(p1, q2) < e) || (Vector2.Distance(q1, p2) < e) || (Vector2.Distance(q1, q2) < e)) { return false; }

        float o1 = Orientation(p1, q1, p2);
        float o2 = Orientation(p1, q1, q2);
        float o3 = Orientation(p2, q2, p1);
        float o4 = Orientation(p2, q2, q1);

        if (o1 != 0f && o2 != 0f && o3 != 0f && o4 != 0f)
        { if (o1 != o2 && o3 != o4) return true; }
        else
        {
            if (o1 == 0f && OnSegment(p1, p2, q1) && Vector2.Distance(p1, p2) > e && Vector2.Distance(q1, p2) > e) return true;
            if (o2 == 0f && OnSegment(p1, q2, q1) && Vector2.Distance(p1, q2) > e && Vector2.Distance(q1, q2) > e) return true;
            if (o3 == 0f && OnSegment(p2, p1, q2) && Vector2.Distance(p2, p1) > e && Vector2.Distance(q2, p1) > e) return true;
            if (o4 == 0f && OnSegment(p2, q1, q2) && Vector2.Distance(p2, q1) > e && Vector2.Distance(q2, q1) > e) return true;
        }
        return false;
    }

    private bool IsLineObstructedAtHeight(float current_angle_rad, float normalizedHeight, Vector2 testPoint)
    {
        float cosA = Mathf.Cos(current_angle_rad);
        float sinA = Mathf.Sin(current_angle_rad);
        Vector3 bounds = GetHorizontalBoundsAtHeight(current_angle_rad, normalizedHeight);
        if (bounds.x == float.MaxValue) return true;
        Vector2 leftBoundPoint = new Vector2(bounds.x, bounds.z);
        Vector2 rightBoundPoint = new Vector2(bounds.y, bounds.z);
        float targetY = bounds.z;
        float tolerance = 0.000100f;
        Vector2 p1_obs, p2_obs;
        int numConfigEdges = 112;

        for (int obs_edge_idx = 0; obs_edge_idx < numConfigEdges; obs_edge_idx++)
        {
            switch (obs_edge_idx)
            {
                case 0:
                    {
                        p1_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 89.73f);
                        p2_obs = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + 86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 1:
                    {
                        p1_obs = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + 86.67f);
                        p2_obs = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + 77.70f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 2:
                    {
                        p1_obs = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + 77.70f);
                        p2_obs = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + 63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 3:
                    {
                        p1_obs = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + 63.45f);
                        p2_obs = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + 44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 4:
                    {
                        p1_obs = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + 44.86f);
                        p2_obs = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + 23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 5:
                    {
                        p1_obs = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + 23.22f);
                        p2_obs = new Vector2((86.12f * cosA) + (-26.16f * sinA), (-1.58f * cosA) + (-5.21f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 6:
                    {
                        p1_obs = new Vector2((86.12f * cosA) + (-26.16f * sinA), (-1.58f * cosA) + (-5.21f * sinA));
                        p2_obs = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + -23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 7:
                    {
                        p1_obs = new Vector2((83.15f * cosA) + (-25.42f * sinA), (-1.54f * cosA) + (-5.03f * sinA) + -23.22f);
                        p2_obs = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + -44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 8:
                    {
                        p1_obs = new Vector2((74.43f * cosA) + (-23.24f * sinA), (-1.41f * cosA) + (-4.50f * sinA) + -44.86f);
                        p2_obs = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + -63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 9:
                    {
                        p1_obs = new Vector2((60.58f * cosA) + (-19.78f * sinA), (-1.20f * cosA) + (-3.67f * sinA) + -63.45f);
                        p2_obs = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + -77.71f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 10:
                    {
                        p1_obs = new Vector2((42.51f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-2.57f * sinA) + -77.71f);
                        p2_obs = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + -86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 11:
                    {
                        p1_obs = new Vector2((21.48f * cosA) + (-10.00f * sinA), (-0.61f * cosA) + (-1.30f * sinA) + -86.67f);
                        p2_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -89.73f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 12:
                    {
                        p1_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -89.73f);
                        p2_obs = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + -86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 13:
                    {
                        p1_obs = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + -86.67f);
                        p2_obs = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + -77.71f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 14:
                    {
                        p1_obs = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + -77.71f);
                        p2_obs = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + -63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 15:
                    {
                        p1_obs = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + -63.45f);
                        p2_obs = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + -44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 16:
                    {
                        p1_obs = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + -44.86f);
                        p2_obs = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + -23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 17:
                    {
                        p1_obs = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + -23.22f);
                        p2_obs = new Vector2((-88.30f * cosA) + (17.44f * sinA), (1.06f * cosA) + (5.34f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 18:
                    {
                        p1_obs = new Vector2((-88.30f * cosA) + (17.44f * sinA), (1.06f * cosA) + (5.34f * sinA));
                        p2_obs = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + 23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 19:
                    {
                        p1_obs = new Vector2((-85.33f * cosA) + (16.70f * sinA), (1.01f * cosA) + (5.16f * sinA) + 23.22f);
                        p2_obs = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + 44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 20:
                    {
                        p1_obs = new Vector2((-76.61f * cosA) + (14.52f * sinA), (0.88f * cosA) + (4.64f * sinA) + 44.86f);
                        p2_obs = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + 63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 21:
                    {
                        p1_obs = new Vector2((-62.76f * cosA) + (11.06f * sinA), (0.67f * cosA) + (3.80f * sinA) + 63.45f);
                        p2_obs = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + 77.70f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 22:
                    {
                        p1_obs = new Vector2((-44.69f * cosA) + (6.54f * sinA), (0.40f * cosA) + (2.71f * sinA) + 77.70f);
                        p2_obs = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + 86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 23:
                    {
                        p1_obs = new Vector2((-23.66f * cosA) + (1.28f * sinA), (0.08f * cosA) + (1.43f * sinA) + 86.67f);
                        p2_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 89.73f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 24:
                    {
                        p1_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 89.73f);
                        p2_obs = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + 86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 25:
                    {
                        p1_obs = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + 86.67f);
                        p2_obs = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + 77.70f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 26:
                    {
                        p1_obs = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + 77.70f);
                        p2_obs = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + 63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 27:
                    {
                        p1_obs = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + 63.45f);
                        p2_obs = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + 44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 28:
                    {
                        p1_obs = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + 44.86f);
                        p2_obs = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + 23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 29:
                    {
                        p1_obs = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + 23.22f);
                        p2_obs = new Vector2((88.30f * cosA) + (-17.44f * sinA), (-1.06f * cosA) + (-5.34f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 30:
                    {
                        p1_obs = new Vector2((88.30f * cosA) + (-17.44f * sinA), (-1.06f * cosA) + (-5.34f * sinA));
                        p2_obs = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + -23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 31:
                    {
                        p1_obs = new Vector2((85.33f * cosA) + (-16.70f * sinA), (-1.01f * cosA) + (-5.16f * sinA) + -23.22f);
                        p2_obs = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + -44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 32:
                    {
                        p1_obs = new Vector2((76.61f * cosA) + (-14.52f * sinA), (-0.88f * cosA) + (-4.64f * sinA) + -44.86f);
                        p2_obs = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + -63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 33:
                    {
                        p1_obs = new Vector2((62.76f * cosA) + (-11.06f * sinA), (-0.67f * cosA) + (-3.80f * sinA) + -63.45f);
                        p2_obs = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + -77.71f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 34:
                    {
                        p1_obs = new Vector2((44.69f * cosA) + (-6.54f * sinA), (-0.40f * cosA) + (-2.71f * sinA) + -77.71f);
                        p2_obs = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + -86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 35:
                    {
                        p1_obs = new Vector2((23.66f * cosA) + (-1.28f * sinA), (-0.08f * cosA) + (-1.43f * sinA) + -86.67f);
                        p2_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -89.73f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 36:
                    {
                        p1_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -89.73f);
                        p2_obs = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + -86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 37:
                    {
                        p1_obs = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + -86.67f);
                        p2_obs = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + -77.71f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 38:
                    {
                        p1_obs = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + -77.71f);
                        p2_obs = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + -63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 39:
                    {
                        p1_obs = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + -63.45f);
                        p2_obs = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + -44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 40:
                    {
                        p1_obs = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + -44.86f);
                        p2_obs = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + -23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 41:
                    {
                        p1_obs = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + -23.22f);
                        p2_obs = new Vector2((-86.12f * cosA) + (26.16f * sinA), (1.58f * cosA) + (5.21f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 42:
                    {
                        p1_obs = new Vector2((-86.12f * cosA) + (26.16f * sinA), (1.58f * cosA) + (5.21f * sinA));
                        p2_obs = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + 23.22f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 43:
                    {
                        p1_obs = new Vector2((-83.15f * cosA) + (25.42f * sinA), (1.54f * cosA) + (5.03f * sinA) + 23.22f);
                        p2_obs = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + 44.86f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 44:
                    {
                        p1_obs = new Vector2((-74.43f * cosA) + (23.24f * sinA), (1.41f * cosA) + (4.50f * sinA) + 44.86f);
                        p2_obs = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + 63.45f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 45:
                    {
                        p1_obs = new Vector2((-60.58f * cosA) + (19.78f * sinA), (1.20f * cosA) + (3.67f * sinA) + 63.45f);
                        p2_obs = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + 77.70f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 46:
                    {
                        p1_obs = new Vector2((-42.51f * cosA) + (15.26f * sinA), (0.92f * cosA) + (2.57f * sinA) + 77.70f);
                        p2_obs = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + 86.67f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 47:
                    {
                        p1_obs = new Vector2((-21.48f * cosA) + (10.00f * sinA), (0.61f * cosA) + (1.30f * sinA) + 86.67f);
                        p2_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 89.73f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 48:
                    {
                        p1_obs = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + 59.83f);
                        p2_obs = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + 69.93f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 49:
                    {
                        p1_obs = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + 69.93f);
                        p2_obs = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + 78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 50:
                    {
                        p1_obs = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + 78.00f);
                        p2_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 80.75f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 51:
                    {
                        p1_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + 80.75f);
                        p2_obs = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + 78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 52:
                    {
                        p1_obs = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + 78.00f);
                        p2_obs = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + 69.93f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 53:
                    {
                        p1_obs = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + 69.93f);
                        p2_obs = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + 59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 54:
                    {
                        p1_obs = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + 59.83f);
                        p2_obs = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + 6.32f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 55:
                    {
                        p1_obs = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + 6.32f);
                        p2_obs = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + 59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 56:
                    {
                        p1_obs = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + -59.83f);
                        p2_obs = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + -69.94f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 57:
                    {
                        p1_obs = new Vector2((38.15f * cosA) + (-14.17f * sinA), (-0.86f * cosA) + (-2.31f * sinA) + -69.94f);
                        p2_obs = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + -78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 58:
                    {
                        p1_obs = new Vector2((19.22f * cosA) + (-9.44f * sinA), (-0.57f * cosA) + (-1.16f * sinA) + -78.00f);
                        p2_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -80.76f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 59:
                    {
                        p1_obs = new Vector2((-1.09f * cosA) + (-4.36f * sinA), (-0.26f * cosA) + (0.07f * sinA) + -80.76f);
                        p2_obs = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + -78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 60:
                    {
                        p1_obs = new Vector2((-21.40f * cosA) + (0.72f * sinA), (0.04f * cosA) + (1.30f * sinA) + -78.00f);
                        p2_obs = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + -69.94f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 61:
                    {
                        p1_obs = new Vector2((-40.33f * cosA) + (5.45f * sinA), (0.33f * cosA) + (2.44f * sinA) + -69.94f);
                        p2_obs = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + -59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 62:
                    {
                        p1_obs = new Vector2((-53.13f * cosA) + (8.65f * sinA), (0.52f * cosA) + (3.22f * sinA) + -59.83f);
                        p2_obs = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + -6.32f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 63:
                    {
                        p1_obs = new Vector2((-1.06f * cosA) + (-4.37f * sinA), (-0.26f * cosA) + (0.06f * sinA) + -6.32f);
                        p2_obs = new Vector2((50.95f * cosA) + (-17.37f * sinA), (-1.05f * cosA) + (-3.08f * sinA) + -59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 64:
                    {
                        p1_obs = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + 59.83f);
                        p2_obs = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + 69.93f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 65:
                    {
                        p1_obs = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + 69.93f);
                        p2_obs = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + 78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 66:
                    {
                        p1_obs = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + 78.00f);
                        p2_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 80.75f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 67:
                    {
                        p1_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 80.75f);
                        p2_obs = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + 78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 68:
                    {
                        p1_obs = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + 78.00f);
                        p2_obs = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + 69.93f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 69:
                    {
                        p1_obs = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + 69.93f);
                        p2_obs = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + 59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 70:
                    {
                        p1_obs = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + 59.83f);
                        p2_obs = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 6.32f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 71:
                    {
                        p1_obs = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + 6.32f);
                        p2_obs = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + 59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 72:
                    {
                        p1_obs = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + -59.83f);
                        p2_obs = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + -69.94f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 73:
                    {
                        p1_obs = new Vector2((40.33f * cosA) + (-5.45f * sinA), (-0.33f * cosA) + (-2.44f * sinA) + -69.94f);
                        p2_obs = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + -78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 74:
                    {
                        p1_obs = new Vector2((21.40f * cosA) + (-0.72f * sinA), (-0.04f * cosA) + (-1.30f * sinA) + -78.00f);
                        p2_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -80.76f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 75:
                    {
                        p1_obs = new Vector2((1.09f * cosA) + (4.36f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -80.76f);
                        p2_obs = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + -78.00f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 76:
                    {
                        p1_obs = new Vector2((-19.22f * cosA) + (9.44f * sinA), (0.57f * cosA) + (1.16f * sinA) + -78.00f);
                        p2_obs = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + -69.94f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 77:
                    {
                        p1_obs = new Vector2((-38.15f * cosA) + (14.17f * sinA), (0.86f * cosA) + (2.31f * sinA) + -69.94f);
                        p2_obs = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + -59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 78:
                    {
                        p1_obs = new Vector2((-50.95f * cosA) + (17.37f * sinA), (1.05f * cosA) + (3.08f * sinA) + -59.83f);
                        p2_obs = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -6.32f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 79:
                    {
                        p1_obs = new Vector2((1.12f * cosA) + (4.35f * sinA), (0.26f * cosA) + (-0.07f * sinA) + -6.32f);
                        p2_obs = new Vector2((53.13f * cosA) + (-8.65f * sinA), (-0.52f * cosA) + (-3.22f * sinA) + -59.83f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 80:
                    {
                        p1_obs = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + 53.54f);
                        p2_obs = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + 40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 81:
                    {
                        p1_obs = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + 40.38f);
                        p2_obs = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + 20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 82:
                    {
                        p1_obs = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + 20.90f);
                        p2_obs = new Vector2((77.40f * cosA) + (-23.98f * sinA), (-1.45f * cosA) + (-4.68f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 83:
                    {
                        p1_obs = new Vector2((77.40f * cosA) + (-23.98f * sinA), (-1.45f * cosA) + (-4.68f * sinA));
                        p2_obs = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + -20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 84:
                    {
                        p1_obs = new Vector2((74.72f * cosA) + (-23.31f * sinA), (-1.41f * cosA) + (-4.52f * sinA) + -20.90f);
                        p2_obs = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + -40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 85:
                    {
                        p1_obs = new Vector2((66.88f * cosA) + (-21.35f * sinA), (-1.29f * cosA) + (-4.05f * sinA) + -40.38f);
                        p2_obs = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + -53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 86:
                    {
                        p1_obs = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + -53.54f);
                        p2_obs = new Vector2((5.05f * cosA) + (-5.90f * sinA), (-0.36f * cosA) + (-0.31f * sinA) + 0.03f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 87:
                    {
                        p1_obs = new Vector2((5.05f * cosA) + (-5.90f * sinA), (-0.36f * cosA) + (-0.31f * sinA) + 0.03f);
                        p2_obs = new Vector2((57.06f * cosA) + (-18.90f * sinA), (-1.14f * cosA) + (-3.45f * sinA) + 53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 88:
                    {
                        p1_obs = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + 53.54f);
                        p2_obs = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + 40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 89:
                    {
                        p1_obs = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + 40.38f);
                        p2_obs = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + 20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 90:
                    {
                        p1_obs = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + 20.90f);
                        p2_obs = new Vector2((-79.58f * cosA) + (15.26f * sinA), (0.92f * cosA) + (4.82f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 91:
                    {
                        p1_obs = new Vector2((-79.58f * cosA) + (15.26f * sinA), (0.92f * cosA) + (4.82f * sinA));
                        p2_obs = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + -20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 92:
                    {
                        p1_obs = new Vector2((-76.90f * cosA) + (14.59f * sinA), (0.88f * cosA) + (4.65f * sinA) + -20.90f);
                        p2_obs = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + -40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 93:
                    {
                        p1_obs = new Vector2((-69.06f * cosA) + (12.63f * sinA), (0.76f * cosA) + (4.18f * sinA) + -40.38f);
                        p2_obs = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + -53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 94:
                    {
                        p1_obs = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + -53.54f);
                        p2_obs = new Vector2((-7.23f * cosA) + (-2.83f * sinA), (-0.17f * cosA) + (0.44f * sinA) + 0.03f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 95:
                    {
                        p1_obs = new Vector2((-7.23f * cosA) + (-2.83f * sinA), (-0.17f * cosA) + (0.44f * sinA) + 0.03f);
                        p2_obs = new Vector2((-59.24f * cosA) + (10.18f * sinA), (0.62f * cosA) + (3.59f * sinA) + 53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 96:
                    {
                        p1_obs = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + 53.54f);
                        p2_obs = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + 40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 97:
                    {
                        p1_obs = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + 40.38f);
                        p2_obs = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + 20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 98:
                    {
                        p1_obs = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + 20.90f);
                        p2_obs = new Vector2((79.58f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-4.82f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 99:
                    {
                        p1_obs = new Vector2((79.58f * cosA) + (-15.26f * sinA), (-0.92f * cosA) + (-4.82f * sinA));
                        p2_obs = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + -20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 100:
                    {
                        p1_obs = new Vector2((76.90f * cosA) + (-14.59f * sinA), (-0.88f * cosA) + (-4.65f * sinA) + -20.90f);
                        p2_obs = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + -40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 101:
                    {
                        p1_obs = new Vector2((69.06f * cosA) + (-12.63f * sinA), (-0.76f * cosA) + (-4.18f * sinA) + -40.38f);
                        p2_obs = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + -53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 102:
                    {
                        p1_obs = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + -53.54f);
                        p2_obs = new Vector2((7.23f * cosA) + (2.83f * sinA), (0.17f * cosA) + (-0.44f * sinA) + 0.03f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 103:
                    {
                        p1_obs = new Vector2((7.23f * cosA) + (2.83f * sinA), (0.17f * cosA) + (-0.44f * sinA) + 0.03f);
                        p2_obs = new Vector2((59.24f * cosA) + (-10.18f * sinA), (-0.62f * cosA) + (-3.59f * sinA) + 53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 104:
                    {
                        p1_obs = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + 53.54f);
                        p2_obs = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + 40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 105:
                    {
                        p1_obs = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + 40.38f);
                        p2_obs = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + 20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 106:
                    {
                        p1_obs = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + 20.90f);
                        p2_obs = new Vector2((-77.40f * cosA) + (23.98f * sinA), (1.45f * cosA) + (4.68f * sinA));
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 107:
                    {
                        p1_obs = new Vector2((-77.40f * cosA) + (23.98f * sinA), (1.45f * cosA) + (4.68f * sinA));
                        p2_obs = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + -20.90f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 108:
                    {
                        p1_obs = new Vector2((-74.72f * cosA) + (23.31f * sinA), (1.41f * cosA) + (4.52f * sinA) + -20.90f);
                        p2_obs = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + -40.38f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 109:
                    {
                        p1_obs = new Vector2((-66.88f * cosA) + (21.35f * sinA), (1.29f * cosA) + (4.05f * sinA) + -40.38f);
                        p2_obs = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + -53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 110:
                    {
                        p1_obs = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + -53.54f);
                        p2_obs = new Vector2((-5.05f * cosA) + (5.90f * sinA), (0.36f * cosA) + (0.31f * sinA) + 0.03f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
                case 111:
                    {
                        p1_obs = new Vector2((-5.05f * cosA) + (5.90f * sinA), (0.36f * cosA) + (0.31f * sinA) + 0.03f);
                        p2_obs = new Vector2((-57.06f * cosA) + (18.90f * sinA), (1.14f * cosA) + (3.45f * sinA) + 53.54f);
                        if (p1_obs.y < targetY - tolerance && p2_obs.y < targetY - tolerance)
                        {
                            if (SegmentsIntersect(testPoint, leftBoundPoint, p1_obs, p2_obs)) { return true; }
                            if (SegmentsIntersect(testPoint, rightBoundPoint, p1_obs, p2_obs)) { return true; }
                        }
                    }
                    break;
            }
        }

        return false; // No obstructions found
    }

}