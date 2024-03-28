using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class ModifiedLightSourceData : ManagedData
    {
        [Vector2ArrayField("ExtraPosition", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, -50, 50 })]
        public Vector2[] ExtraPosition;

        [Vector2Field("radius1", 50, 50, Vector2Field.VectorReprType.circle)]
        public Vector2 rad1;

        [Vector2Field("radius2", 65, 65, Vector2Field.VectorReprType.circle)]
        public Vector2 rad2;

        [FloatField("A", 0, 2, 0.5f, 0.05f, ManagedFieldWithPanel.ControlType.slider, "Alpha")]
        public float alpha;

        [FloatField("BA", 0, 1, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "R")]
        public float red;

        [FloatField("BB", 0, 1, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "G")]
        public float green;

        [FloatField("BC", 0, 1, 11f, 0.01f, ManagedFieldWithPanel.ControlType.slider, displayName: "B")]
        public float blue;

        [IntegerField("C", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen A")]
        public int screenA;

        [BooleanField("D", false, ManagedFieldWithPanel.ControlType.button, "Set Screen A To Current")]
        public bool setScreenA;

        [IntegerField("E", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen B")]
        public int screenB;

        [BooleanField("F", false, ManagedFieldWithPanel.ControlType.button, "Set Screen B To Current")]
        public bool setScreenB;

        public ModifiedLightSourceData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class ModifiedLightSource : UpdatableAndDeletable
    {
        public ModifiedLightSource(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.lightSourceA == null)
            {
                this.lightSourceA = new LightSource(placedObject.pos + (this.placedObject.data as ModifiedLightSourceData).ExtraPosition[1], false, new Color((this.placedObject.data as ModifiedLightSourceData).red, (this.placedObject.data as ModifiedLightSourceData).green, (this.placedObject.data as ModifiedLightSourceData).blue), this);
                this.lightSourceA.affectedByPaletteDarkness = 0.5f;
                this.room.AddObject(this.lightSourceA);
            }

            if (this.lightSourceB == null)
            {
                this.lightSourceB = new LightSource(placedObject.pos, false, new Color((this.placedObject.data as ModifiedLightSourceData).red, (this.placedObject.data as ModifiedLightSourceData).green, (this.placedObject.data as ModifiedLightSourceData).blue), this);
                this.lightSourceB.affectedByPaletteDarkness = 0.5f;
                this.room.AddObject(this.lightSourceB);
            }

            this.lightSourceA.pos = placedObject.pos + (this.placedObject.data as ModifiedLightSourceData).ExtraPosition[1];
            this.lightSourceB.pos = placedObject.pos;

            this.lightSourceA.color = new Color((this.placedObject.data as ModifiedLightSourceData).red, (this.placedObject.data as ModifiedLightSourceData).green, (this.placedObject.data as ModifiedLightSourceData).blue);
            this.lightSourceB.color = new Color((this.placedObject.data as ModifiedLightSourceData).red, (this.placedObject.data as ModifiedLightSourceData).green, (this.placedObject.data as ModifiedLightSourceData).blue);

            this.lightSourceA.rad = RWCustom.Custom.Dist(Vector2.zero, (this.placedObject.data as ModifiedLightSourceData).rad1);
            this.lightSourceB.rad = RWCustom.Custom.Dist(Vector2.zero, (this.placedObject.data as ModifiedLightSourceData).rad2);

            if (Constants.DamagedShortcuts.TryGetValue(this.room.game, out var CameraPosition))
            {
                if ((this.placedObject.data as ModifiedLightSourceData).setScreenA)
                {
                    (this.placedObject.data as ModifiedLightSourceData).screenA = CameraPosition.camPosition;
                    (this.placedObject.data as ModifiedLightSourceData).setScreenA = false;
                }

                if ((this.placedObject.data as ModifiedLightSourceData).setScreenB)
                {
                    (this.placedObject.data as ModifiedLightSourceData).screenB = CameraPosition.camPosition;
                    (this.placedObject.data as ModifiedLightSourceData).setScreenB = false;
                }

                if (CameraPosition.camPosition == (this.placedObject.data as ModifiedLightSourceData).screenA)
                {
                    this.lightSourceA.alpha = 0f;
                    this.lightSourceB.alpha = (this.placedObject.data as ModifiedLightSourceData).alpha;
                }
                if (CameraPosition.camPosition == (this.placedObject.data as ModifiedLightSourceData).screenB)
                {
                    this.lightSourceA.alpha = (this.placedObject.data as ModifiedLightSourceData).alpha;
                    this.lightSourceB.alpha = 0f;
                }
                if (CameraPosition.camPosition == (this.placedObject.data as ModifiedLightSourceData).screenA && (this.placedObject.data as ModifiedLightSourceData).screenA == (this.placedObject.data as ModifiedLightSourceData).screenB)
                {
                    this.lightSourceA.alpha = (this.placedObject.data as ModifiedLightSourceData).alpha;
                    this.lightSourceB.alpha = (this.placedObject.data as ModifiedLightSourceData).alpha;
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        private PlacedObject placedObject;

        private LightSource lightSourceA;
        private LightSource lightSourceB;
    }
}