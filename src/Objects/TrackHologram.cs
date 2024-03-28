using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class TrackHologramData(PlacedObject owner) : ManagedData(owner, null)
    {
        // [Vector2Field("size", 100, 100, Vector2Field.VectorReprType.rect)]
        // public Vector2 size;

        // [ExtEnumField<EyeMode>("eyeMode", nameof(EyeMode.Closed), new[] { nameof(EyeMode.Closed), nameof(EyeMode.Stunned), nameof(EyeMode.Dead), nameof(EyeMode.ReverseBlink), nameof(EyeMode.CustomBlink) }, displayName: "Eye Mode")]
        // public EyeMode mode;

        // [FloatField("customFrequency", 0, 1, 0.5f, 0.01f, displayName: "Custom Frequency")]
        // public float customFrequency;

        // [IntegerField("customDurationMin", 0, 400, 3, control: ManagedFieldWithPanel.ControlType.text, displayName: "Custom Duration Min")]
        // public int customDurationMin;

        // [IntegerField("customDurationMax", 0, 400, 10, control: ManagedFieldWithPanel.ControlType.text, displayName: "Custom Duration Max")]
        // public int customDurationMax;

        // [FloatField("beforeCycle", 0, 1, 0, 0.01f, displayName: "Before Cycle")]
        // public float beforeCycle;

        // [FloatField("afterCycle", 0, 1, 0, 0.01f, displayName: "After Cycle")]
        // public float afterCycle;

        [Vector2ArrayField("handles", 5, true, Vector2ArrayRepresentationType.Polygon, new float[10] { 0, 0, -100, 100, 100, 100, 100, 160, 100, 100 })]
        public Vector2[] handles;

        [Vector2Field("radius", 50, -50, Vector2Field.VectorReprType.circle)]
        public Vector2 rad;

        [IntegerField("hologram", 0, 2, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "Hologram")]
        public int hologram;
    }

    public class Hologram(string value, bool register = false) : ExtEnum<Hologram>(value, register)
    {
        public static readonly Hologram None = new(nameof(None), true);
    }

    /*public class ForcedEyePlayerData
    {
        public bool eu;
        public int customBlink;
        public float frequency;
        public int durationMin;
        public int durationMax;

        private static readonly ConditionalWeakTable<PlayerGraphics, ForcedEyePlayerData> _cwt = new();

        public static ForcedEyePlayerData Get(PlayerGraphics pg) => _cwt.GetValue(pg, _ => new());
    }*/

    public class TrackHologramObject(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public static void Apply()
        {
            // On.PlayerGraphics.Update += PlayerGraphics_Update;
        }

        public static void Undo()
        {
            // On.PlayerGraphics.Update -= PlayerGraphics_Update;
        }
    }
}