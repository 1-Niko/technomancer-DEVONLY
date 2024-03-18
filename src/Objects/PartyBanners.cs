using System.Linq;
using UnityEngine;
using static Pom.Pom;

namespace Slugpack
{
    public class PartyBannerData : ManagedData
    {
        [Vector2Field("Left Connecction", -100, 100, Vector2Field.VectorReprType.line)]
        public Vector2 LeftAnchor;

        [Vector2Field("Right Connecction", 100, 100, Vector2Field.VectorReprType.line)]
        public Vector2 RightAnchor;

        public PartyBannerData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class PartyBanner : UpdatableAndDeletable
    {
        public PartyBanner(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.streamerLine == null)
            {
                this.streamerLine = new Streamers(placedObject);
                this.room.AddObject(this.streamerLine);
            }
            this.streamerLine.pos = placedObject.pos;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        Streamers streamerLine;

        PlacedObject placedObject;
    }

    public class Streamers : CosmeticSprite
    {
        private readonly PlacedObject placedObject;

        public Streamers(PlacedObject placedObject)
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

            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("blank");

            Vector2 RightAnchorPosition = (this.placedObject.data as PartyBannerData).RightAnchor;
            Vector2 LeftAnchorPosition = (this.placedObject.data as PartyBannerData).LeftAnchor;

            float[] xCoordinates = new float[3] { this.pos.x, this.pos.x + RightAnchorPosition.x, this.pos.x + LeftAnchorPosition.x };
            float[] yCoordinates = new float[3] { this.pos.y, this.pos.y + RightAnchorPosition.y, this.pos.y + LeftAnchorPosition.y };

            Vector2 position = new Vector2((xCoordinates.Min() + xCoordinates.Max()) / 2f, (yCoordinates.Min() + yCoordinates.Max()) / 2f);

            float scaleX = xCoordinates.Max() - xCoordinates.Min();
            float scaleY = yCoordinates.Max() - yCoordinates.Min();

            float image_size = 1024f;

            sLeaser.sprites[0].SetPosition(position - rCam.pos);

            sLeaser.sprites[0].scaleX = scaleX / image_size;
            sLeaser.sprites[0].scaleY = scaleY / image_size;

            sLeaser.sprites[0].isVisible = true;

            if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
            {
                sLeaser.sprites[0].shader = Shaders.Distances;
                sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PointA", new Vector4((xCoordinates[2] - xCoordinates.Min()) / (xCoordinates.Max() - xCoordinates.Min()), (yCoordinates[2] - yCoordinates.Min()) / (yCoordinates.Max() - yCoordinates.Min()), 0, 0)); // Left Anchor
                sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PointB", new Vector4((xCoordinates[0] - xCoordinates.Min()) / (xCoordinates.Max() - xCoordinates.Min()), (yCoordinates[0] - yCoordinates.Min()) / (yCoordinates.Max() - yCoordinates.Min()), 0, 0)); // Origin
                sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PointC", new Vector4((xCoordinates[1] - xCoordinates.Min()) / (xCoordinates.Max() - xCoordinates.Min()), (yCoordinates[1] - yCoordinates.Min()) / (yCoordinates.Max() - yCoordinates.Min()), 0, 0)); // Right Anchor

                sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Width", scaleX / image_size);
                sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Height", scaleY / image_size);
                sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Size", 4 / (xCoordinates.Max() - xCoordinates.Min()));
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = new FSprite("blank", true);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Midground");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
    }
}