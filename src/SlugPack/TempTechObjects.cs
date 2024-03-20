using System;
using System.Collections.Generic;
using UnityEngine;
using static Slugpack.DataStructures;

namespace Slugpack
{
    public class LockHologram : CosmeticSprite
    {
        public LockHologram(Vector2 pos, Color colour, int lifetime)
        {
            this.pos = pos;
            this.colour = colour;
            this.lifetime = lifetime;
        }

        public override void Update(bool eu)
        {
            if (this.lifetime < 0)
            {
                //this.RemoveFromRoom();
                //this.room.RemoveObject(this);
                Destroy();
            }
            else
            {
                this.scanline += 0.3f;
                this.lifetime--;
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            this.sLeaser ??= sLeaser;
            /*for (int i = 0; i < spritecount; i++)
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("pixel");
                sLeaser.sprites[i].color = colour;
                sLeaser.sprites[i].isVisible = true;

                if (Math.Sin(1.5f * (scanline + Holograms.HologramA[i].y)) - UnityEngine.Random.value < 0)
                    sLeaser.sprites[i].isVisible = false;

                sLeaser.sprites[i].SetPosition(pos - rCam.pos - new Vector2(Holograms.HologramA[i].x, Holograms.HologramA[i].y) + new Vector2(((pos - rCam.pos).x - 683) / 750f * Holograms.HologramA[i].z,
                    ((pos - rCam.pos).y - 384) / 450f * Holograms.HologramA[i].z));
            }*/

            int i = 0;
            for (int index = 0; index < Holograms.PipeLock.Length; index++)
            {
                Int40 bitArray = new Int40(Holograms.PipeLock[index]);

                //Debug.Log($"Examining Array : {bitArray}");

                for (int bitIndex = 0; bitIndex < 41; bitIndex++)
                {
                    //Debug.Log($"Indexing bit : {bitArray.GetBit(bitIndex)}");

                    if (bitArray.GetBit(bitIndex))
                    {
                        sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("pixel");
                        sLeaser.sprites[i].color = colour;

                        sLeaser.sprites[i].isVisible = (Math.Sin(1.5f * (scanline + bitIndex - (sLeaser.sprites[i].GetPosition().x / 3))) - UnityEngine.Random.value < 0);

                        sLeaser.sprites[i].SetPosition(pos - rCam.pos - new Vector2(index - 20f, bitIndex - 20f));// + new Vector2(((pos - rCam.pos).x - 683) / 0f, ((pos - rCam.pos).y - 384) / 0f));
                        i++;
                    }
                }
            }

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[spritecount];
            for (int i = 0; i < spritecount; i++)
            { sLeaser.sprites[i] = new FSprite("pixel", true); }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Foreground");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public RoomCamera.SpriteLeaser sLeaser;

        public Color colour;

        public float scanline;

        public int lifetime;

        public int spritecount = 254;
    }

    // Even though the graphics for this aren't used anymore, I'm still really proud of how it ended up.
    // Might have to find somewhere to force it back in later
    public class SlugArrow : CosmeticSprite
    {
        public SlugArrow(Vector2 pos, float scanline, Color colour)
        {
            this.pos = pos;
            this.scanline = scanline;
            this.colour = colour;
        }
        public SlugArrow(Vector2 pos, float scanline, Color colour, Creature stickToCreature)
        {
            this.pos = pos;
            this.scanline = scanline;
            this.colour = colour;
            creature = stickToCreature;
        }
        public SlugArrow(Vector2 pos, float scanline, Color colour, PhysicalObject stickToItem)
        {
            this.pos = pos;
            this.scanline = scanline;
            this.colour = colour;
            item = stickToItem;
        }

        float RoundToNearest(float x, float n) => (float)Math.Round(x / n) * n;

        public override void Update(bool eu)
        {
            scanline += 0.3f;
            if (sLeaser != null && room.ViewedByAnyCamera(this.pos, 20f))
            {
                if (this.creature != null)
                {
                    this.pos = this.creature.mainBodyChunk.pos;// + new Vector2(0f, 35f);

                    if (this.creature.inShortcut)
                    {
                        Destroy();
                    }
                }
                else if (this.item != null)
                {
                    this.pos = this.item.firstChunk.pos;// + new Vector2(0f, 15f);
                }
                /*else if (this._object != null)
                {
                    if (this._object.data is TrainWarningBellData)
                    {
                        this.pos = (this._object.data as TrainWarningBellData).owner.pos + (this._object.data as TrainWarningBellData).ArrowPosition + new Vector2(0f, 35f);
                    }
                }*/

                /*for (int i = 0; i < spritecount; i++)
                {
                    float rounded = RoundToNearest(i, 4) / 8;
                    float addToScanline = (i < 2) ? 4 * (2 * i - 1) : ((i > 1 && i < 30) || i == 31) ? rounded * (float)Math.Cos(Math.PI * i) : -rounded + 21.5f;

                    if (Math.Sin(scanline + addToScanline) - UnityEngine.Random.value < 0)
                    {
                        sLeaser.sprites[i].isVisible = false;
                    }
                }*/
            }
        }

        public float shapeX = 0;//5;
        public float shapeY = 0;//18;
        public float separation = 0;//1;

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            /*int rimCount = (int)Math.Abs(Math.Ceiling(2 * (shapeX / separation)));
            int slantCount = (int)Math.Ceiling(Math.Sqrt(Math.Pow(2 * shapeX, 2) + Math.Pow(shapeY, 2)) / separation);

            List<Vector3> points = new();

            for (int i = 1; i < (2 * rimCount) - 1; i++)
            {
                float x = (shapeX / rimCount) * i - shapeX;
                // x, y, depth
                points.Add(new Vector3(x, -shapeY, -shapeX));
                points.Add(new Vector3(x, -shapeY, shapeX));
                points.Add(new Vector3(-shapeX, -shapeY, x));
                points.Add(new Vector3(shapeX, -shapeY, x));
            }

            points.Add(new Vector3(0, 0, 0));

            for (int i = 1; i < slantCount; i++)
            {
                float x = shapeX * ((float)i / slantCount);
                float y = -(shapeY / shapeX) * (shapeX * ((float)i / slantCount));

                points.Add(new Vector3(x, y, x));
                points.Add(new Vector3(x, y, -x));
                points.Add(new Vector3(-x, y, x));
                points.Add(new Vector3(-x, y, -x));
            }

            this.sLeaser ??= sLeaser;
            for (int i = 0; i < points.Count; i++)
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("pixel");
                sLeaser.sprites[i].color = colour;

                /*
                float R1 = RoundToNearest(i, 4);
                float R2 = R1 / 8;

                float depth = (i < 2) ? 4 * (2 * i - 1) : Math.Min(R2, (-R1 * 0.0275f) + 4.73f) * (float)Math.Cos(Math.PI * i);

                float newXValue = Math.Min((-R1 * 0.0277325f) + 4.777f, ((i < 2) ? 0 : 4)) * ((i == 3) ? -1 : -1.42f * (float)Math.Sin(1.57079f * (i + 0.5f)));

                float newYValue = Math.Max(R2 - 25, -18);

                float addToScanline = (i < 2) ? 4 * (2 * i - 1) : ((i > 1 && i < 30) || i == 31) ? R2 * (float)Math.Cos(Math.PI * i) : -R2 + 21.5f;

                Debug.Log($"INDEX {i} : {((i < 2) ? 9 : 1)}, {depth}, {newXValue}, {newYValue}, {addToScanline}");
                *

                /*sLeaser.sprites[i].isVisible = false;// (Math.Sin(1.5f * (scanline + (points[i].x / 3) + points[i].y - (points[i].z / 6))) - UnityEngine.Random.value > 0);

                Vector2 hologramPos = (pos - rCam.pos);
                sLeaser.sprites[i].SetPosition(pos - rCam.pos - new Vector2(points[i].x, points[i].y) + new Vector2((hologramPos.x - 683) / 750f * points[i].z, (hologramPos.y - 384) / 450f * points[i].z));*/
            //}
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            //int rimCount = (int)Math.Abs(Math.Ceiling(2 * (shapeX / separation)));
            //int slantCount = (int)Math.Ceiling(Math.Sqrt(Math.Pow(2 * shapeX, 2) + Math.Pow(shapeY, 2)) / separation);
            //
            //int spritecount = 0;// (8 * rimCount) + (4 * slantCount) - 11;

            //sLeaser.sprites = new FSprite[spritecount];
            //for (int i = 0; i < spritecount; i++)
            //{ sLeaser.sprites[i] = new FSprite("pixel", true); }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            // newContatiner ??= rCam.ReturnFContainer("Foreground");
            // foreach (FSprite fsprite in sLeaser.sprites)
            // {
            //     fsprite.RemoveFromContainer();
            //     newContatiner.AddChild(fsprite);
            // }
        }

        public PhysicalObject item;

        public Creature creature;

        public PlacedObject _object;

        public RoomCamera.SpriteLeaser sLeaser;

        public Color colour;

        public float scanline;

        public int width;
    }
}