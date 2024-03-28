using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class ModifiedLightSourceData(PlacedObject owner) : ManagedData(owner, null)
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
    }

    public class ModifiedLightSource(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (lightSourceA == null)
            {
                lightSourceA = new LightSource(placedObject.pos + (placedObject.data as ModifiedLightSourceData).ExtraPosition[1], false, new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue), this)
                {
                    affectedByPaletteDarkness = 0.5f
                };
                room.AddObject(lightSourceA);
            }

            if (lightSourceB == null)
            {
                lightSourceB = new LightSource(placedObject.pos, false, new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue), this)
                {
                    affectedByPaletteDarkness = 0.5f
                };
                room.AddObject(lightSourceB);
            }

            lightSourceA.pos = placedObject.pos + (placedObject.data as ModifiedLightSourceData).ExtraPosition[1];
            lightSourceB.pos = placedObject.pos;

            lightSourceA.color = new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue);
            lightSourceB.color = new Color((placedObject.data as ModifiedLightSourceData).red, (placedObject.data as ModifiedLightSourceData).green, (placedObject.data as ModifiedLightSourceData).blue);

            lightSourceA.rad = RWCustom.Custom.Dist(Vector2.zero, (placedObject.data as ModifiedLightSourceData).rad1);
            lightSourceB.rad = RWCustom.Custom.Dist(Vector2.zero, (placedObject.data as ModifiedLightSourceData).rad2);

            if (Constants.DamagedShortcuts.TryGetValue(room.game, out var CameraPosition))
            {
                if ((placedObject.data as ModifiedLightSourceData).setScreenA)
                {
                    (placedObject.data as ModifiedLightSourceData).screenA = CameraPosition.camPosition;
                    (placedObject.data as ModifiedLightSourceData).setScreenA = false;
                }

                if ((placedObject.data as ModifiedLightSourceData).setScreenB)
                {
                    (placedObject.data as ModifiedLightSourceData).screenB = CameraPosition.camPosition;
                    (placedObject.data as ModifiedLightSourceData).setScreenB = false;
                }

                if (CameraPosition.camPosition == (placedObject.data as ModifiedLightSourceData).screenA)
                {
                    lightSourceA.alpha = 0f;
                    lightSourceB.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
                }
                if (CameraPosition.camPosition == (placedObject.data as ModifiedLightSourceData).screenB)
                {
                    lightSourceA.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
                    lightSourceB.alpha = 0f;
                }
                if (CameraPosition.camPosition == (placedObject.data as ModifiedLightSourceData).screenA && (placedObject.data as ModifiedLightSourceData).screenA == (placedObject.data as ModifiedLightSourceData).screenB)
                {
                    lightSourceA.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
                    lightSourceB.alpha = (placedObject.data as ModifiedLightSourceData).alpha;
                }
            }
        }

        private PlacedObject placedObject = placedObject;

        private LightSource lightSourceA;
        private LightSource lightSourceB;
    }
}