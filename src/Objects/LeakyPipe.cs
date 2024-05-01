using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class LeakyPipeData(PlacedObject owner) : ManagedData(owner, null)
    {

        [Vector2ArrayField("handles", 4, true, Vector2ArrayRepresentationType.Chain, new float[8] { 0, 0, -20, 0, -40, 0, -60, 0 })]
        public Vector2[] handles;

        [FloatField("A", 0f, 1f, 0.5f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "Drip Chance")]
        public float chance;
    }

    public class LeakyPipe(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            if ((placedObject.data as LeakyPipeData).chance >= Random.value)
            {
                int length = Random.RandomRange(0, 3);

                Vector2 randomPosition = Vector2.zero;

                switch (length)
                {
                    case 0:
                        randomPosition = Utilities.Lerp(placedObject.pos + (placedObject.data as LeakyPipeData).handles[0], placedObject.pos + (placedObject.data as LeakyPipeData).handles[1], Random.value);
                        break;
                    case 1:
                        randomPosition = Utilities.Lerp(placedObject.pos + (placedObject.data as LeakyPipeData).handles[1], placedObject.pos + (placedObject.data as LeakyPipeData).handles[2], Random.value);
                        break;
                    case 2:
                        randomPosition = Utilities.Lerp(placedObject.pos + (placedObject.data as LeakyPipeData).handles[2], placedObject.pos + (placedObject.data as LeakyPipeData).handles[3], Random.value);
                        break;
                }

                room.AddObject(new WaterDrip(randomPosition, new Vector2(0f, 0f), false));
            }
        }

        public int remainingWait;
    }
}