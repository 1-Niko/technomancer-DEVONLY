﻿namespace Slugpack;

public class PartyBannerData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2Field("Left Connecction", -100, 100, Vector2Field.VectorReprType.line)]
    public Vector2 LeftAnchor;

    [Vector2Field("Right Connecction", 100, 100, Vector2Field.VectorReprType.line)]
    public Vector2 RightAnchor;
}

public class PartyBanner(PlacedObject placedObject, Room room) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (streamerLine == null)
        {
            streamerLine = new Streamers(placedObject);
            room.AddObject(streamerLine);
        }
        streamerLine.pos = placedObject.pos;
    }

    private Streamers streamerLine;

    private PlacedObject placedObject = placedObject;
}

public class Streamers(PlacedObject placedObject) : CosmeticSprite
{
    private readonly PlacedObject placedObject = placedObject;

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("blank");

        Vector2 RightAnchorPosition = (placedObject.data as PartyBannerData).RightAnchor;
        Vector2 LeftAnchorPosition = (placedObject.data as PartyBannerData).LeftAnchor;

        float[] xCoordinates = [pos.x, pos.x + RightAnchorPosition.x, pos.x + LeftAnchorPosition.x];
        float[] yCoordinates = [pos.y, pos.y + RightAnchorPosition.y, pos.y + LeftAnchorPosition.y];

        Vector2 position = new((xCoordinates.Min() + xCoordinates.Max()) / 2f, (yCoordinates.Min() + yCoordinates.Max()) / 2f);

        float scaleX = xCoordinates.Max() - xCoordinates.Min();
        float scaleY = yCoordinates.Max() - yCoordinates.Min();

        float image_size = 1024f;

        sLeaser.sprites[0].SetPosition(position - rCam.pos);

        sLeaser.sprites[0].scaleX = scaleX / image_size;
        sLeaser.sprites[0].scaleY = scaleY / image_size;

        sLeaser.sprites[0].isVisible = true;

        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
        {
            sLeaser.sprites[0].shader = Shaders.Distances;
            sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PointA", new Vector4((xCoordinates[2] - xCoordinates.Min()) / (xCoordinates.Max() - xCoordinates.Min()), (yCoordinates[2] - yCoordinates.Min()) / (yCoordinates.Max() - yCoordinates.Min()), 0, 0)); // Left Anchor
            sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PointB", new Vector4((xCoordinates[0] - xCoordinates.Min()) / (xCoordinates.Max() - xCoordinates.Min()), (yCoordinates[0] - yCoordinates.Min()) / (yCoordinates.Max() - yCoordinates.Min()), 0, 0)); // Origin
            sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PointC", new Vector4((xCoordinates[1] - xCoordinates.Min()) / (xCoordinates.Max() - xCoordinates.Min()), (yCoordinates[1] - yCoordinates.Min()) / (yCoordinates.Max() - yCoordinates.Min()), 0, 0)); // Right Anchor

            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Width", scaleX / image_size);
            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Height", scaleY / image_size);
            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Size", 4 / (xCoordinates.Max() - xCoordinates.Min()));
        }

        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
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