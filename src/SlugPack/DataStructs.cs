using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

namespace Slugpack
{
    public static class DataStructures
    {
        public class Int40 : IEnumerable<bool>
        {
            private long value;

            public Int40(long value)
            {
                if (value < 0 || value > 0x7FFFFFFFFFF)
                    throw new System.ArgumentOutOfRangeException(nameof(value), "Value must be a 40-bit integer.");

                this.value = value;
            }

            public bool GetBit(int index)
            {
                if (index < 0 || index > 40)
                    throw new System.ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 40.");

                return ((value >> index) & 1) == 1;
            }

            public IEnumerator<bool> GetEnumerator()
            {
                for (int i = 39; i >= 0; i--)
                    yield return ((value >> i) & 1) == 1;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override string ToString() => System.Convert.ToString(value, 2).PadLeft(41, '0');
        }

        public class TexturePool
        {
            private readonly Queue<Texture2D> _pool;
            private readonly int _poolSize;

            public TexturePool(int poolSize) => (_pool, _poolSize) = (new Queue<Texture2D>(), poolSize);

            public Texture2D GetTexture() => _pool.Count > 0 ? _pool.Dequeue() : new Texture2D(1, 1);

            public void ReturnTexture(Texture2D texture)
            {
                if (_pool.Count < _poolSize)
                { _pool.Enqueue(texture); }
                else
                { Object.Destroy(texture); }
            }

        }

        public class Sprite
        {
            public Sprite(int lockValue, int spriteIndex, Vector2 offset, float rotation, Anchorpoint anchor, Scale scale, float minimum, float maximum) =>
                (Lock, SpriteIndex, Offset, Rotation, AnchorX, AnchorY, ScaleX, ScaleY, Minimum, Maximum) = 
                (lockValue, $"FurTuft{spriteIndex}", offset, rotation, anchor.x, anchor.y, scale.x, scale.y, minimum, maximum);
            public Sprite(int lockValue, int spriteIndex, Vector2 offset, float rotation, Anchorpoint anchor, Scale scale, Color Colour) =>
                (Lock, SpriteIndex, Offset, Rotation, AnchorX, AnchorY, ScaleX, ScaleY, Color) = 
                (lockValue, $"FurTuft{spriteIndex}", offset, rotation, anchor.x, anchor.y, scale.x, scale.y, Colour);

            public int Lock { get; set; }
            public string SpriteIndex { get; set; }
            public Vector2 Offset { get; set; }
            public float Rotation { get; set; }
            public float AnchorX { get; set; }
            public float AnchorY { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public float Minimum { get; set; }
            public float Maximum { get; set; }
            public Color Color { get; set; }
        }

        public class ScreenSlice
        {
            public ScreenSlice(WWW image) => Screen = image;

            public WWW Screen { get; set; }
        }

        public class Anchorpoint
        {
            public Anchorpoint(float x, float y) => (this.x, this.y) = (x, y);

            public float x { get; set; }
            public float y { get; set; }
        }

        public class Scale
        {
            public Scale(float x, float y) => (this.x, this.y) = (x, y);

            public float x { get; set; }
            public float y { get; set; }
        }


        public class Pixel
        {
            public Pixel(float x, float y, float z) => (this.x, this.y, this.z) = (x, y, z);

            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
        }

        public class Lock
        {
            public Lock(ShortcutData[] shortcuts, Room[] rooms, int time, LockHologram[] holograms) =>
                (this.shortcuts, this.rooms, this.time, this.holograms, this.sinceFlicker) = (shortcuts, rooms, time, holograms, 0);

            public ShortcutData[] shortcuts { get; set; }
            public Room[] rooms { get; set; }
            public int time { get; set; }
            public LockHologram[] holograms { get; set; }
            public int sinceFlicker { get; set; }
        }

        public class Node
        {
            public Node(Vector2 position, int level, int protection, PhysicalObject anchor) =>
                (this.position, this.level, this.protection, this.anchor) = (position, level, protection, anchor);
            public Vector2 position { get; set; }
            public int level { get; set; }
            public int protection { get; set; }
            public PhysicalObject anchor { get; set; }
        }
    }
}