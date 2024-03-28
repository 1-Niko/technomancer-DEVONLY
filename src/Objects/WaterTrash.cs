using static Pom.Pom;

namespace Slugpack
{
    public class WaterTrashData(PlacedObject owner) : ManagedData(owner, null)
    {
        [Vector2Field("TrashArea", 100, 100, Vector2Field.VectorReprType.rect)]
        public Vector2 TrashArea;

        [IntegerField("TrashCount", 0, 200, 0, ManagedFieldWithPanel.ControlType.slider, displayName: "TrashCount")]
        public int TrashCount;

        [IntegerField("RandomSeed", 0, 100, 0, ManagedFieldWithPanel.ControlType.slider, displayName: "Seed")]
        public int RandomSeed;

        [IntegerField("FrontDepth", 0, 30, 0, ManagedFieldWithPanel.ControlType.slider, displayName: "Front Depth")]
        public int FrontDepth;

        [IntegerField("BackDepth", 0, 30, 30, ManagedFieldWithPanel.ControlType.slider, displayName: "BackDepth")]
        public int BackDepth;
    }

    public class WaterTrash(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (waterTrash == null)
            {
                waterTrash = new WaterTrashObject(placedObject.pos, (placedObject.data as WaterTrashData).TrashArea,
                                                                         (placedObject.data as WaterTrashData).RandomSeed,
                                                                         (placedObject.data as WaterTrashData).TrashCount);

                room.AddObject(waterTrash);
            }

            waterTrash.frontDepth = (placedObject.data as WaterTrashData).FrontDepth;
            waterTrash.backDepth = (placedObject.data as WaterTrashData).BackDepth;
        }

        public WaterTrashObject waterTrash;

        public PlacedObject placedObject = placedObject;
    }

    public class WaterTrashObject : CosmeticSprite, IDrawable
    {
        public WaterTrashObject(Vector2 pos, Vector2 trashArea, int randomSeed, int trashCount)
        {
            this.pos = pos;
            this.trashArea = trashArea;
            this.randomSeed = randomSeed;
            this.trashCount = trashCount;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Texture2D palette = rCam.currentPalette.texture;

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                Vector2 offset = pos + new Vector2(trashArea.x / trashCount * i, 0);
                sLeaser.sprites[i].isVisible = true;
                sLeaser.sprites[i].SetPosition(new Vector2(offset.x, room.FloatWaterLevel(Mathf.Max(offset.x, 0))) - rCam.pos);

                int depth = (int)Mathf.Round(Utilities.Normalize(depths[i], 0, 30, frontDepth, backDepth));

                sLeaser.sprites[i].color = palette.GetPixel(depth, colour[i] + 2);
                sLeaser.sprites[i].alpha = depth / 30f;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            Random.InitState(randomSeed);

            sLeaser.sprites = new FSprite[trashCount];

            for (int i = 0; i < trashCount; i++)
            {
                sLeaser.sprites[i] = new FSprite($"trash_{Random.Range(0, 105)}", true)
                {
                    shader = rCam.game.rainWorld.Shaders["CustomDepth"]
                };

                depths.Add(Random.Range(0, 30));
                colour.Add(Random.Range(0, 3));
            }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            FContainer Midground = rCam.ReturnFContainer("Water");

            Random.InitState(randomSeed);

            List<int> numbers = Utilities.GenerateNumbers(sLeaser.sprites.Length - 1);
            Utilities.Shuffle(numbers);

            for (int i = 1; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[numbers[i]].RemoveFromContainer();
                Midground.AddChild(sLeaser.sprites[numbers[i]]);

                sLeaser.sprites[numbers[i]].MoveBehindOtherNode(sLeaser.sprites[numbers[i - 1]]);
            }
        }

        public int frontDepth;

        public int backDepth;

        public List<int> depths = [];

        public List<int> colour = [];

        public int randomSeed;

        public int trashCount;

        public Vector2 trashArea;
    }
}