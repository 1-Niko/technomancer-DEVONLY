using Newtonsoft.Json;

namespace Slugpack;

public class RoomBackgroundData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2ArrayField("handle", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, 20, 60 })]
    public Vector2[] handle;

    [IntegerField("A", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen")]
    public int screen;

    [BooleanField("B", false, ManagedFieldWithPanel.ControlType.button, "Set Screen To Current")]
    public bool setScreen;

}

public class RoomBackground(PlacedObject placedObject) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        // This should only be active if
        // 1. The player is in the room
        // 2. The player is on the correct screen
        // Otherwise it should unload the atlas and remove the sprite

        if (Constants.DamagedShortcuts.TryGetValue(room.game, out var CameraPosition))
        {
            if ((placedObject.data as RoomBackgroundData).setScreen)
            {
                (placedObject.data as RoomBackgroundData).screen = CameraPosition.camPosition;
                (placedObject.data as RoomBackgroundData).setScreen = false;
            }

            bool createBackground = false;

            bool keepBackground = false;

            if (this.room.abstractRoom.name == CameraPosition.room) // If player in room
            {
                if ((placedObject.data as RoomBackgroundData).screen == CameraPosition.camPosition) // If player on screen
                {
                    if (Background == null)
                    {
                        createBackground = true;
                    }
                    else
                    {
                        keepBackground = true;
                    }
                }

                if (createBackground)
                {
                    // Background will be null here so no need to check

                    // Load expected atlas

                    string atlasPath = $"Screens/{this.room.abstractRoom.name}_{CameraPosition.camPosition}.png";

                    screenAtlas = File.Exists(Path.ChangeExtension(atlasPath, ".txt"))
                    ? Futile.atlasManager.LoadAtlas(Path.ChangeExtension(atlasPath, null))
                    : Futile.atlasManager.LoadImage(Path.ChangeExtension(atlasPath, null));

                    Background = new RoomBackgroundSprite($"{this.room.abstractRoom.name}_{CameraPosition.camPosition}");
                    room.AddObject(Background);
                }
                if (!keepBackground && !createBackground && screenAtlas != null)
                {
                    // Unload the object and the atlas
                    screenAtlas.Unload();
                    room.RemoveObject(Background);
                    Background.Destroy();
                }
            }
        }
    }

    private FAtlas screenAtlas;

    private RoomBackgroundSprite Background;

    private PlacedObject placedObject = placedObject;
}

public class RoomBackgroundSprite(string background_name) : CosmeticSprite
{

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(background_name);
        sLeaser.sprites[0].SetPosition(pos - rCam.pos);
        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[0].scaleX = 1f;
        sLeaser.sprites[0].scaleY = 1f;
        sLeaser.sprites[0].anchorX = 0f;
        sLeaser.sprites[0].anchorY = 0f;

        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite(background_name, true);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Background");
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

    string background_name;
}