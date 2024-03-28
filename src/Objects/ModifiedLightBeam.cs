using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class ModifiedLightBeamData : ManagedData
    {
        [Vector2ArrayField("LightBeam", 4, true, Vector2ArrayRepresentationType.Polygon, new float[8] { 0, 0, 0, 50, 50, 50, 50, 0 })]
        public Vector2[] BeamPosition;

        [Vector2ArrayField("AlternateLightBeam", 5, true, Vector2ArrayRepresentationType.Polygon, new float[10] { 0, 0, 40, 29, 25, 77, -25, 77, -40, 29 })]
        public Vector2[] AlternateBeamPosition;

        [FloatField("A", 0, 1, 0.5f, 0.05f, ManagedFieldWithPanel.ControlType.slider, "Alpha")]
        public float alpha;

        [FloatField("B", 0, 1, 0, 0.05f, ManagedFieldWithPanel.ControlType.slider, "Depth")]
        public float depth;

        [FloatField("C", 0, 1, 0, 0.05f, ManagedFieldWithPanel.ControlType.slider, "White Pickup")]
        public float white;

        [BooleanField("D", true, ManagedFieldWithPanel.ControlType.button, "Sun")]
        public bool sun;

        [StringField("DE", "FFFFFF", "HEX")]
        public string hex;

        [IntegerField("E", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen A")]
        public int screenA;

        [BooleanField("F", false, ManagedFieldWithPanel.ControlType.button, "Set Screen A To Current")]
        public bool setScreenA;

        [IntegerField("G", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen B")]
        public int screenB;

        [BooleanField("H", false, ManagedFieldWithPanel.ControlType.button, "Set Screen B To Current")]
        public bool setScreenB;

        public ModifiedLightBeamData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class ModifiedLightBeam : UpdatableAndDeletable
    {
        public ModifiedLightBeam(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.LightBeam == null)
            {
                this.LightBeam = new ModifiedLightBeamSprite(placedObject, placedObject.pos);
                this.room.AddObject(this.LightBeam);
            }
            this.LightBeam.pos = this.placedObject.pos;

            this.LightBeam.depth = (this.placedObject.data as ModifiedLightBeamData).depth;
            this.LightBeam.opacity = (this.placedObject.data as ModifiedLightBeamData).alpha;
            this.LightBeam.whiteness = (this.placedObject.data as ModifiedLightBeamData).white;
            this.LightBeam.colour = Utilities.HexToColor((this.placedObject.data as ModifiedLightBeamData).hex);

            if (Constants.DamagedShortcuts.TryGetValue(this.room.game, out var CameraPosition))
            {
                if ((this.placedObject.data as ModifiedLightBeamData).screenA == CameraPosition.camPosition)
                {
                    Vector2[] vertices = (this.placedObject.data as ModifiedLightBeamData).BeamPosition;
                    this.LightBeam.verts = new Vector2[] { vertices[0], vertices[3], vertices[1], vertices[2] };
                }
                else if ((this.placedObject.data as ModifiedLightBeamData).screenB == CameraPosition.camPosition)
                {
                    Vector2[] vertices = (this.placedObject.data as ModifiedLightBeamData).AlternateBeamPosition;
                    this.LightBeam.verts = new Vector2[] { vertices[4], vertices[1], vertices[3], vertices[2] };
                }

                if ((this.placedObject.data as ModifiedLightBeamData).setScreenA)
                {
                    (this.placedObject.data as ModifiedLightBeamData).screenA = CameraPosition.camPosition;
                    (this.placedObject.data as ModifiedLightBeamData).setScreenA = false;
                }

                if ((this.placedObject.data as ModifiedLightBeamData).setScreenB)
                {
                    (this.placedObject.data as ModifiedLightBeamData).screenB = CameraPosition.camPosition;
                    (this.placedObject.data as ModifiedLightBeamData).setScreenB = false;
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        private ModifiedLightBeamSprite LightBeam;

        private PlacedObject placedObject;
    }

    public class ModifiedLightBeamSprite : CosmeticSprite
    {
        private readonly PlacedObject placedObject;

        public ModifiedLightBeamSprite(PlacedObject placedObject, Vector2 pos)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            sLeaser.sprites[0].isVisible = true;

            Color sky = this.colour;

            sLeaser.sprites[0].color = new Color(sky.r, sky.g, sky.b, Utilities.EncodeFloats(depth, opacity, whiteness) / 32767f);

            if (sLeaser.sprites[0] != null && (sLeaser.sprites[0] as TriangleMesh) != null && this.verts != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, (this.pos - rCam.pos) + this.verts[i]);
                }
            }

            if (Constants.shaders_enabled)
                if (Constants.SlugpackShaders.TryGetValue(rCam?.room?.world?.game?.rainWorld, out var Shaders))
                {
                    sLeaser.sprites[0].shader = Shaders.ModifiedLightBeamShader;
                }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = TriangleMesh.MakeGridMesh("LightBeam", 1);

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

            for (int i = 1; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[i - 1]);
            }
        }

        public Vector2[] verts;

        public float opacity = 0;

        public float whiteness = 0;

        public float depth = 0;

        public Color colour;
    }
}