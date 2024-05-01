namespace Slugpack;

public static class DataStructures
{
    public class Int40 : IEnumerable<bool>
    {
        private readonly long value;

        public Int40(long value)
        {
            if (value is < 0 or > 0x7FFFFFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be a 40-bit integer.");

            this.value = value;
        }

        public bool GetBit(int index)
        {
            return index is < 0 or > 40
                ? throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 40.")
                : ((value >> index) & 1) == 1;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            for (int i = 39; i >= 0; i--)
                yield return ((value >> i) & 1) == 1;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Convert.ToString(value, 2).PadLeft(41, '0');
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
            { UnityEngine.Object.Destroy(texture); }
        }
    }

    public class Sprite
    {
        public Sprite(int lockValue, int spriteIndex, Vector2 offset, float rotation, Anchorpoint anchor, Scale scale, float minimum, float maximum) =>
            (Lock, SpriteIndex, Offset, Rotation, AnchorX, AnchorY, ScaleX, ScaleY, Minimum, Maximum) =
            (lockValue, $"FurTuft{spriteIndex}", offset, rotation, anchor.X, anchor.Y, scale.X, scale.Y, minimum, maximum);

        public Sprite(int lockValue, int spriteIndex, Vector2 offset, float rotation, Anchorpoint anchor, Scale scale, Color Colour) =>
            (Lock, SpriteIndex, Offset, Rotation, AnchorX, AnchorY, ScaleX, ScaleY, Color) =
            (lockValue, $"FurTuft{spriteIndex}", offset, rotation, anchor.X, anchor.Y, scale.X, scale.Y, Colour);

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

    public class ScreenSlice(WWW image)
    {
        public WWW Screen { get; set; } = image;
    }

    public class Anchorpoint
    {
        public Anchorpoint(float x, float y) => (X, Y) = (x, y);

        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Scale
    {
        public Scale(float x, float y) => (X, Y) = (x, y);

        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Pixel
    {
        public Pixel(float x, float y, float z) => (X, Y, Z) = (x, y, z);

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Lock
    {
        public Lock(ShortcutData[] shortcuts, Room[] rooms, int time, LockHologram[] holograms) =>
            (Shortcuts, Rooms, Time, Holograms, SinceFlicker) = (shortcuts, rooms, time, holograms, 0);

        public ShortcutData[] Shortcuts { get; set; }
        public Room[] Rooms { get; set; }
        public int Time { get; set; }
        public LockHologram[] Holograms { get; set; }
        public int SinceFlicker { get; set; }
    }

    public class Node
    {
        public Node(Vector2 position, int level, int protection, PhysicalObject anchor, PlacedObject anchor2) =>
            (Position, Level, Protection, Anchor, ObjectAnchor) = (position, level, protection, anchor, anchor2);

        public Vector2 Position { get; set; }
        public int Level { get; set; }
        public int Protection { get; set; }
        public PhysicalObject Anchor { get; set; }
        public PlacedObject ObjectAnchor { get; set; }
    }
}