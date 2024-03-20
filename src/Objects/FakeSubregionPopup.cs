using System.Collections.Generic;
using System.Linq;
using IL.MoreSlugcats;
using UnityEngine;
using static Pom.Pom;

namespace Slugpack
{
    public class FakeSubregionPopupData : ManagedData
    {
        [Vector2Field("radius", 50, -50, Vector2Field.VectorReprType.circle)]
        public Vector2 dir;

        // If somebody adds more subregions than the 16 bit integer limit I will NOT be making this compatable
        [IntegerField("A", 1, 65535, 1, ManagedFieldWithPanel.ControlType.arrows, "Index")]
        public int index;

        // [StringField("B", "None", "Subregion")]
        // public string name;

        public FakeSubregionPopupData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class FakeSubregionPopupObject : UpdatableAndDeletable
    {
        public FakeSubregionPopupObject(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            FakeSubregionPopupData dataObj = this.placedObject.data as FakeSubregionPopupData;

            bool nothingNull = this.room != null && this.room.world != null && this.room.world.region != null && this.room.world.region.subRegions != null;

            if (nothingNull && this.subRegions == null)
            {
                this.subRegions = this.room.world.region.subRegions;
            }

            if (dataObj.index > this.subRegions.Count)
                dataObj.index = this.subRegions.Count - 1;

            // So that it loops
            if (dataObj.index == this.subRegions.Count)
                dataObj.index = 1;

            if (name != this.subRegions[dataObj.index])
            {
                name = this.subRegions[dataObj.index];
                Debug.Log($"Region set to {this.subRegions[dataObj.index]}");
            }

            if (nothingNull && this.room == this.room.game.Players[0].realizedCreature.room && this.room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion != dataObj.index)
            {
                Vector2 a = dataObj.dir;

                Vector2 c = this.placedObject.pos;
                Vector2 d = this.room.game.Players[0].realizedCreature.mainBodyChunk.pos;

                Vector2 b = d - c;

                a = a / a.magnitude;
                b = b / b.magnitude;

                float dot = a.x * b.x + a.y * b.y;

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
                        // Debug.Log("Suppressed");
                    }
                    if (previousRegionID == 2 && regionID == 0 && !suppressed && !triggered) // trigger
                    {
                        triggered = true;
                        // Debug.Log("Triggered");
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
                        this.room.game.cameras[0].hud.textPrompt.AddMessage(this.subRegions[dataObj.index], 0, 160, false, true);
                        this.room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = dataObj.index;
                    }
                }
            }

            if (this.room != this.room.game.Players[0].realizedCreature.room)
            {
                inRadius = inRegion = suppressed = triggered = false;

                regionID = previousRegionID = counter = 0;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        PlacedObject placedObject;

        List<string> subRegions;

        string name;

        bool inRadius;

        bool inRegion;

        int regionID;

        int previousRegionID;

        bool suppressed;

        bool triggered;

        int counter;
    }
}