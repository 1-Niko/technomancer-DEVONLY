using RWCustom;
using UnityEngine;

namespace LizardCosmetics
{
    public class MultipleAntennae : Template
    {
        private int Sprite(int side, int part)
        {
            return this.startSprite + part * 2 + side;
        }

        public MultipleAntennae(LizardGraphics lGraphics, int startSprite, int index) : base(lGraphics, startSprite)
        {
            this.spritesOverlap = Template.SpritesOverlap.InFront;
            this.index = index;
            this.length = Random.value;
            this.segments = Mathf.FloorToInt(Mathf.Lerp(3f, 8f, Mathf.Pow(this.length, Mathf.Lerp(1f, 6f, this.length))));
            this.alpha = this.length * 0.9f + Random.value * 0.1f;
            this.antennae = new GenericBodyPart[2, this.segments];
            for (int i = 0; i < this.segments; i++)
            {
                this.antennae[0, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
                this.antennae[1, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
            }
            this.redderTint = new Color(lGraphics.effectColor.r, lGraphics.effectColor.g, lGraphics.effectColor.b);
            this.redderTint.g = this.redderTint.g * 0.5f;
            this.redderTint.b = this.redderTint.b * 0.5f;
            this.redderTint.r = Mathf.Lerp(this.redderTint.r, 1f, 0.75f);
            this.numberOfSprites = 4;
        }

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < this.segments; j++)
                {
                    this.antennae[i, j].Reset(this.AnchorPoint(i, 1f));
                }
            }
        }

        public override void Update()
        {
            float num = this.lGraphics.lizard.AI.yellowAI.commFlicker;
            if (!this.lGraphics.lizard.Consious)
            {
                num = 0f;
            }
            float num2 = Mathf.Lerp(10f, 7f, this.length);
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < this.segments; j++)
                {
                    float num3 = (float)j / (float)(this.segments - 1);
                    num3 = Mathf.Lerp(num3, Mathf.InverseLerp(0f, 5f, (float)j), 0.2f);
                    this.antennae[i, j].vel += this.AntennaDir(i, 1f) * (1f - num3 + 0.6f * num);
                    if (this.lGraphics.lizard.room.PointSubmerged(this.antennae[i, j].pos))
                    {
                        this.antennae[i, j].vel *= 0.8f;
                    }
                    else
                    {
                        GenericBodyPart genericBodyPart = this.antennae[i, j];
                        genericBodyPart.vel.y = genericBodyPart.vel.y - 0.4f * num3 * (1f - num);
                    }
                    this.antennae[i, j].Update();
                    this.antennae[i, j].pos += Custom.RNV() * 3f * num;
                    Vector2 p;
                    if (j == 0)
                    {
                        this.antennae[i, j].vel += this.AntennaDir(i, 1f) * 5f;
                        p = this.lGraphics.head.pos;
                        this.antennae[i, j].ConnectToPoint(this.AnchorPoint(i, 1f) * (this.index + 1), num2, true, 0f, this.lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
                    }
                    else
                    {
                        if (j == 1)
                        {
                            p = this.AnchorPoint(i, 1f) * (this.index + 1);
                        }
                        else
                        {
                            p = this.antennae[i, j - 2].pos;
                        }
                        Vector2 a = Custom.DirVec(this.antennae[i, j].pos, this.antennae[i, j - 1].pos);
                        float num4 = Vector2.Distance(this.antennae[i, j].pos, this.antennae[i, j - 1].pos);
                        this.antennae[i, j].pos -= a * (num2 - num4) * 0.5f;
                        this.antennae[i, j].vel -= a * (num2 - num4) * 0.5f;
                        this.antennae[i, j - 1].pos += a * (num2 - num4) * 0.5f;
                        this.antennae[i, j - 1].vel += a * (num2 - num4) * 0.5f;
                    }
                    this.antennae[i, j].vel += Custom.DirVec(p, this.antennae[i, j].pos) * 3f * Mathf.Pow(1f - num3, 0.3f);
                    if (j > 1)
                    {
                        this.antennae[i, j - 2].vel += Custom.DirVec(this.antennae[i, j].pos, this.antennae[i, j - 2].pos) * 3f * Mathf.Pow(1f - num3, 0.3f);
                    }
                    if (!Custom.DistLess(this.lGraphics.head.pos, this.antennae[i, j].pos, 200f))
                    {
                        this.antennae[i, j].pos = this.lGraphics.head.pos;
                    }
                }
            }
        }

        private Vector2 AntennaDir(int side, float timeStacker)
        {
            float num = Mathf.Lerp(this.lGraphics.lastHeadDepthRotation, this.lGraphics.headDepthRotation, timeStacker);
            return Custom.RotateAroundOrigo(new Vector2(((side == 0) ? -1f : 1f) * (1f - Mathf.Abs(num)) * 1.5f + num * 3.5f, -1f).normalized, Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lGraphics.drawPositions[0, 1], this.lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(this.lGraphics.head.lastPos, this.lGraphics.head.pos, timeStacker)));
        }

        private Vector2 AnchorPoint(int side, float timeStacker)
        {
            return Vector2.Lerp(this.lGraphics.drawPositions[0, 1], this.lGraphics.drawPositions[0, 0], timeStacker) + this.AntennaDir(side, timeStacker) * 3f * this.lGraphics.iVars.headSize;
        }

        // Token: 0x06001F91 RID: 8081 RVA: 0x0027F690 File Offset: 0x0027D890
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[this.Sprite(i, j)] = TriangleMesh.MakeLongMesh(this.segments, true, true);
                }
            }
            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[this.Sprite(k, 1)].shader = rCam.room.game.rainWorld.Shaders["LizardAntenna"];
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            float flicker = Mathf.Pow(Random.value, 1f - 0.5f * this.lGraphics.lizard.AI.yellowAI.commFlicker) * this.lGraphics.lizard.AI.yellowAI.commFlicker;
            if (!this.lGraphics.lizard.Consious)
            {
                flicker = 0f;
            }
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[this.startSprite + i].color = this.lGraphics.HeadColor(timeStacker);
                Vector2 vector = Vector2.Lerp(Vector2.Lerp(this.lGraphics.head.lastPos, this.lGraphics.head.pos, timeStacker), this.AnchorPoint(i, timeStacker), 0.5f);
                float num = 1f;
                float num2 = 0f;
                for (int j = 0; j < this.segments; j++)
                {
                    float num3 = (float)j / (float)(this.segments - 1);
                    Vector2 vector2 = Vector2.Lerp(this.antennae[i, j].lastPos, this.antennae[i, j].pos, timeStacker);
                    Vector2 normalized = (vector2 - vector).normalized;
                    Vector2 a = Custom.PerpendicularVector(normalized);
                    float d = Vector2.Distance(vector2, vector) / 5f;
                    float num4 = Mathf.Lerp(3f, 1f, Mathf.Pow(num3, 0.8f));
                    for (int k = 0; k < 2; k++)
                    {
                        (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4, vector - a * (num + num4) * 0.5f + normalized * d - camPos);
                        (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 1, vector + a * (num + num4) * 0.5f + normalized * d - camPos);
                        (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).verticeColors[j * 4] = this.EffectColor(k, (num3 + num2) / 2f, timeStacker, flicker);
                        (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).verticeColors[j * 4 + 1] = this.EffectColor(k, (num3 + num2) / 2f, timeStacker, flicker);
                        (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).verticeColors[j * 4 + 2] = this.EffectColor(k, num3, timeStacker, flicker);
                        if (j < this.segments - 1)
                        {
                            (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - a * num4 - normalized * d - camPos);
                            (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + a * num4 - normalized * d - camPos);
                            (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).verticeColors[j * 4 + 3] = this.EffectColor(k, num3, timeStacker, flicker);
                        }
                        else
                        {
                            (sLeaser.sprites[this.Sprite(i, k)] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - camPos);
                        }
                    }
                    num = num4;
                    vector = vector2;
                    num2 = num3;
                }
            }
        }

        public Color EffectColor(int part, float tip, float timeStacker, float flicker)
        {
            tip = Mathf.Pow(Mathf.InverseLerp(0f, 0.6f, tip), 0.5f);
            if (part == 0)
            {
                return Color.Lerp(this.lGraphics.HeadColor(timeStacker), Color.Lerp(this.lGraphics.effectColor, this.lGraphics.palette.blackColor, flicker), tip);
            }
            return Color.Lerp(new Color(this.redderTint.r, this.redderTint.g, this.redderTint.b, this.alpha), new Color(1f, 1f, 1f, this.alpha), flicker);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
        }

        public GenericBodyPart[,] antennae;

        public Color redderTint;

        public int segments;

        public float length;

        public float alpha;

        public int index;
    }
}
