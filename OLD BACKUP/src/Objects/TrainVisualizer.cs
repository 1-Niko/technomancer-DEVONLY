namespace Slugpack;

public class TrainControllerHologramData(PlacedObject owner) : ManagedData(owner, null)
{
}

public class TrainControllerHologram(PlacedObject placedObject) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (Hologram == null)
        {
            Hologram = new TrainServerHologram();
            room.AddObject(Hologram);
        }
        Hologram.pos = placedObject.pos;
    }

    private TrainServerHologram Hologram;

    private readonly PlacedObject placedObject = placedObject;
}

public class TrainServerHologram() : CosmeticSprite
{

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[0].SetPosition(pos - rCam.pos);

        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        int spriteCount = 1;
        sLeaser.sprites = new FSprite[spriteCount];

        for (int i = 0; i < spriteCount; i++)
        {
            sLeaser.sprites[i] = new FSprite("TrainHologram", true);
        }

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
    }
}