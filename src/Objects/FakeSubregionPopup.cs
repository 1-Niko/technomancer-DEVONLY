using static Pom.Pom;

namespace Slugpack
{
    public class FakeSubregionPopupData(PlacedObject owner) : ManagedData(owner, null)
    {
        [Vector2Field("radius", 50, -50, Vector2Field.VectorReprType.circle)]
        public Vector2 dir;

        // If somebody adds more subregions than the 16 bit integer limit I will NOT be making this compatable
        [IntegerField("A", 1, 65535, 1, ManagedFieldWithPanel.ControlType.arrows, "Index")]
        public int index;
    }

    public class FakeSubregionPopupObject(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            FakeSubregionPopupData dataObj = placedObject.data as FakeSubregionPopupData;

            bool nothingNull = room != null && room.world != null && room.world.region != null && room.world.region.subRegions != null;

            if (nothingNull && subRegions == null)
            {
                subRegions = room.world.region.subRegions;
            }

            if (dataObj.index > subRegions.Count)
                dataObj.index = subRegions.Count - 1;

            // So that it loops
            if (dataObj.index == subRegions.Count)
                dataObj.index = 1;

            if (name != subRegions[dataObj.index])
            {
                name = subRegions[dataObj.index];
                //Debug.Log($"Region set to {subRegions[dataObj.index]}");
            }

            if (nothingNull && room == room.game.Players[0].realizedCreature.room && room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion != dataObj.index)
            {
                Vector2 a = dataObj.dir;

                Vector2 c = placedObject.pos;
                Vector2 d = room.game.Players[0].realizedCreature.mainBodyChunk.pos;

                Vector2 b = d - c;

                a /= a.magnitude;
                b /= b.magnitude;

                float dot = (a.x * b.x) + (a.y * b.y);

                inRadius = inRegion = false;

                if (Vector2.Distance(c, d) < dataObj.dir.magnitude) // Entered the radius of the object
                {
                    inRadius = true;

                    inRegion = dot >= 0;
                }

                if (!inRadius)
                {
                    regionID = 0;
                }
                else if (inRadius && !inRegion)
                {
                    regionID = 1;
                }
                else if (inRadius && inRegion)
                {
                    regionID = 2;
                }

                if (regionID != previousRegionID)
                {
                    // Someone has moved between the regions and something must happen

                    // if (previousRegionID == 0 && regionID == 1) { } // nothing
                    // if (previousRegionID == 1 && regionID == 0) { } // nothing
                    // if (previousRegionID == 1 && regionID == 2) { primed = true; Debug.Log("Primed!"); } // prime
                    // if (previousRegionID == 2 && regionID == 1) { primed = false; Debug.Log("Unprimed"); } // unprime

                    // It should also automatically supress if you enter the room with your last seen subregion being its display

                    if (previousRegionID == 0 && regionID == 2 && !triggered && !suppressed) // supress
                    {
                        suppressed = true;
                    }
                    if (previousRegionID == 2 && regionID == 0 && !suppressed && !triggered) // trigger
                    {
                        triggered = true;
                    }

                    previousRegionID = regionID;
                }

                if (triggered)
                {
                    if (counter <= 81) // So it doesn't keep counting pointlessly (wouldn't affect anything but can't hurt)
                    {
                        counter++;
                    }
                    if (counter == 81)
                    {
                        room.game.cameras[0].hud.textPrompt.AddMessage(subRegions[dataObj.index], 0, 160, false, true);
                        room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = dataObj.index;
                    }
                }
            }

            if (room is null)
            {
                return;
            }

            if (room != room.game.Players[0].realizedCreature.room)
            {
                inRadius = inRegion = suppressed = triggered = false;

                regionID = previousRegionID = counter = 0;
            }
        }

        private PlacedObject placedObject = placedObject;

        private List<string> subRegions;

        private string name;

        private bool inRadius;

        private bool inRegion;

        private int regionID;

        private int previousRegionID;

        private bool suppressed;

        private bool triggered;

        private int counter;
    }
}