using static Pom.Pom;
using UnityEngine;
using static Pom.Pom.Vector2ArrayField;
using System;

namespace Slugpack
{
    public class TrackHologramData : ManagedData
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

        public TrackHologramData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class Hologram : ExtEnum<Hologram>
    {
        public Hologram(string value, bool register = false) : base(value, register)
        {
        }

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

    public class TrackHologram : UpdatableAndDeletable
    {
        private readonly PlacedObject placedObject;

        public static void Apply()
        {
            // On.PlayerGraphics.Update += PlayerGraphics_Update;
        }

        public static void Undo()
        {
            // On.PlayerGraphics.Update -= PlayerGraphics_Update;
        }

        public TrackHologram(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            /*var data = (placedObject.data as SlugcatEyeSelectorData)!;

            var pos = placedObject.pos;

            if (data.size.x < 0)
            {
                data.size.x = -data.size.x;
                pos.x -= data.size.x;
            }
            if (data.size.y < 0)
            {
                data.size.y = -data.size.y;
                pos.y -= data.size.y;
            }

            var affectedRect = new Rect(pos, data.size);

            var cycleProgression = 1 - room.game.world.rainCycle.AmountLeft;
            foreach (var player in room.PlayersInRoom)
            {
                if (player?.graphicsModule is not PlayerGraphics pg) continue;

                var pgData = ForcedEyePlayerData.Get(pg);

                //-- Don't apply more than once in the same update, in case there is overlap between multiple objects
                if (pgData.eu == eu) continue;

                if ((data.afterCycle == 0 || cycleProgression > data.afterCycle) && (data.beforeCycle == 0 || cycleProgression < data.beforeCycle) && affectedRect.Contains(player.mainBodyChunk.pos))
                {
                    pgData.mode = data.mode;
                    pgData.frequency = data.customFrequency;
                    pgData.durationMin = data.customDurationMin;
                    pgData.durationMin = data.customDurationMax;
                    pgData.eu = eu;
                }
                else
                {
                    pgData.mode = null;
                }
            }*/
        }
    }
}