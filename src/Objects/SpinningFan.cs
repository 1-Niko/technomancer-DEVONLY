using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class SpinningFanData(PlacedObject owner) : ManagedData(owner, null)
    {
        [Vector2ArrayField("CHAIN1", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, -50, 0 })]
        public Vector2[] FrontBladeCamOffset;

        [Vector2ArrayField("CHAIN2", 3, true, Vector2ArrayRepresentationType.Chain, new float[6] { 0, 0, 0, -50, -50, -50 })]
        public Vector2[] BackBladeCamOffset;

        [FloatField("A", -40f, 40f, 0f, 0.1f, ManagedFieldWithPanel.ControlType.slider, displayName: "Fan1 Speed")]
        public float frontspeed;

        [FloatField("B", -40f, 40f, 0f, 0.1f, ManagedFieldWithPanel.ControlType.slider, displayName: "Fan2 Speed")]
        public float backspeed;

        [BooleanField("C", false, ManagedFieldWithPanel.ControlType.button, displayName: "Randomize")]
        public bool randomize;

        // Layer (0, 1, 2)
        [IntegerField("D", 0, 2, 0, ManagedFieldWithPanel.ControlType.arrows, "Layer")]
        public int layer;

        // Blade 1 layer depth
        [FloatField("E", 0f, 1f, 0f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Fan1 Depth")]
        public float fan1depth;

        // Blade 2 layer depth
        [FloatField("F", 0f, 1f, 0f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Fan2 Depth")]
        public float fan2depth;

        [BooleanField("G", false, ManagedFieldWithPanel.ControlType.button, displayName: "Enable Shadow (Will mess with depth!)")]
        public bool shadowPlaceholder;

        [IntegerField("H", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen A")]
        public int screenA;

        [BooleanField("I", false, ManagedFieldWithPanel.ControlType.button, "Set Screen A To Current")]
        public bool setScreenA;

        [BooleanField("J", false, ManagedFieldWithPanel.ControlType.button, "Placement Mode")]
        public bool debugging_enabled;

        [BooleanField("K", false, ManagedFieldWithPanel.ControlType.button, "Toggle Fan (Placement)")]
        public bool debugging_backfan_enabled;
    }

    public class SpinningFan : UpdatableAndDeletable
    {
        public SpinningFan(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (dynamicSprite == null)
            {
                dynamicSprite = new SpinningFanObject(placedObject.pos);
                room.AddObject(dynamicSprite);
            }

            if ((placedObject.data as SpinningFanData).randomize)
            {
                (placedObject.data as SpinningFanData).frontspeed = Random.Range(-40f, 40f);
                (placedObject.data as SpinningFanData).backspeed = Random.Range(-40f, 40f);
                (placedObject.data as SpinningFanData).randomize = false;
            }

            dynamicSprite.rotSpeed = (placedObject.data as SpinningFanData).frontspeed;
            dynamicSprite.rotSpeed2 = (placedObject.data as SpinningFanData).backspeed;

            dynamicSprite.chain1 = (placedObject.data as SpinningFanData).FrontBladeCamOffset;
            dynamicSprite.chain2 = (placedObject.data as SpinningFanData).BackBladeCamOffset;

            dynamicSprite.fanDepthA = (placedObject.data as SpinningFanData).fan1depth;
            dynamicSprite.fanDepthB = (placedObject.data as SpinningFanData).fan2depth;
            dynamicSprite.depth = (placedObject.data as SpinningFanData).layer;

            dynamicSprite.shadow = (placedObject.data as SpinningFanData).shadowPlaceholder;
            dynamicSprite.placementMode = (placedObject.data as SpinningFanData).debugging_enabled;
            dynamicSprite.whichBlade = (placedObject.data as SpinningFanData).debugging_backfan_enabled;

            if (Constants.DamagedShortcuts.TryGetValue(room.game, out var CameraPosition))
            {
                if ((placedObject.data as SpinningFanData).setScreenA)
                {
                    (placedObject.data as SpinningFanData).screenA = CameraPosition.camPosition;
                    (placedObject.data as SpinningFanData).setScreenA = false;
                }

                // If you are ever in a situation where you need more than two positions to define the position of a fan, you have bigger issues

                // Now that I think about it the max you'd ever need is probably like
                // four?
                // But then again why are you putting that in the camera intersection there?
                // Just alter the room, silly
                dynamicSprite.fanToggle = CameraPosition.camPosition != (placedObject.data as SpinningFanData).screenA;
            }

            dynamicSprite.pos = placedObject.pos;
        }

        private PlacedObject placedObject;
        private SpinningFanObject dynamicSprite;
    }

    public class SpinningFanObject : CosmeticSprite
    {
        public SpinningFanObject(Vector2 pos)
        {
            this.pos = pos;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            lastRot = rot;
            lastRot2 = rot2;
            rot += rotSpeed;
            rot2 += rotSpeed2;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            float currentRot = Mathf.Lerp(lastRot, rot, timeStacker);
            float currentRot2 = Mathf.Lerp(lastRot2, rot2, timeStacker);

            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(placementMode ? "debugCircle" : "FanBlade");
            sLeaser.sprites[0].SetPosition(pos + chain1[0 + (fanToggle ? 1 : 0)] - rCam.pos);
            sLeaser.sprites[0].rotation = placementMode ? 0 : currentRot;
            sLeaser.sprites[0].color = new Color(placementMode ? 1f : 0f, whichBlade ? 0f : 1f, 1f, (fanDepthA / 3f) + (depth / 3f));

            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName(placementMode ? "debugCircle" : "FanBlade");
            sLeaser.sprites[1].SetPosition(pos + chain2[1 + (fanToggle ? 1 : 0)] - rCam.pos);
            sLeaser.sprites[1].rotation = placementMode ? 0 : currentRot2;
            sLeaser.sprites[1].color = new Color(placementMode ? 1f : 0f, whichBlade ? 1f : 0f, 1f, (fanDepthB / 3f) + (depth / 3f));

            if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
            {
                sLeaser.sprites[0].shader = shaders.SpinningFan;
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_ShadowMask", shaders._shadowMask);

                sLeaser.sprites[1].shader = shaders.SpinningFan;
                sLeaser.sprites[1]._renderLayer?._material?.SetTexture("_ShadowMask", shaders._shadowMask);
            }

            // Shadow Sprites
            if (shadow)
            {
                sLeaser.sprites[2].isVisible = true;
                sLeaser.sprites[3].isVisible = true;

                sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName(placementMode ? "debugCircle" : "FanBlade");
                sLeaser.sprites[2].SetPosition(pos + chain1[0 + (fanToggle ? 1 : 0)] - rCam.pos);
                sLeaser.sprites[2].rotation = placementMode ? 0 : currentRot;
                sLeaser.sprites[2].color = new Color(placementMode ? 1f : 0f, whichBlade ? 0f : 1f, 1f, 1f);

                sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName(placementMode ? "debugCircle" : "FanBlade");
                sLeaser.sprites[3].SetPosition(pos + chain2[1 + (fanToggle ? 1 : 0)] - rCam.pos);
                sLeaser.sprites[3].rotation = placementMode ? 0 : currentRot2;
                sLeaser.sprites[3].color = new Color(placementMode ? 1f : 0f, whichBlade ? 1f : 0f, 1f, 1f);

                if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shadowShaders))
                {
                    sLeaser.sprites[2].shader = shadowShaders.SpinningFan;
                    sLeaser.sprites[2]._renderLayer?._material?.SetTexture("_ShadowMask", shadowShaders._shadowMask);

                    sLeaser.sprites[3].shader = shadowShaders.SpinningFan;
                    sLeaser.sprites[3]._renderLayer?._material?.SetTexture("_ShadowMask", shadowShaders._shadowMask);
                }
            }
            else
            {
                sLeaser.sprites[2].isVisible = false;
                sLeaser.sprites[3].isVisible = false;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];

            for (int i = 0; i < 4; i++)
                sLeaser.sprites[i] = new FSprite("FanBlade", true);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            FContainer midContainer = rCam.ReturnFContainer("Water");
            FContainer shadowContainer = rCam.ReturnFContainer("Midground");

            sLeaser.sprites[0].RemoveFromContainer();
            midContainer.AddChild(sLeaser.sprites[0]);

            sLeaser.sprites[1].RemoveFromContainer();
            midContainer.AddChild(sLeaser.sprites[1]);

            sLeaser.sprites[2].RemoveFromContainer();
            shadowContainer.AddChild(sLeaser.sprites[2]);

            sLeaser.sprites[3].RemoveFromContainer();
            shadowContainer.AddChild(sLeaser.sprites[3]);
        }

        public float rotSpeed;
        public float rotSpeed2;

        public float rot;
        public float rot2;

        public float lastRot;
        public float lastRot2;

        public Vector2[] chain1;
        public Vector2[] chain2;

        public bool fanToggle;

        public float fanDepthA;
        public float fanDepthB;

        public int depth;

        public bool shadow;
        public bool placementMode;
        public bool whichBlade;
    }
}