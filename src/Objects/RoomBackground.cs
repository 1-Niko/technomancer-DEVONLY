namespace Slugpack;

public class RoomBackgroundData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2ArrayField("handle", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, -60, -20 })]
    public Vector2[] handle;

    [IntegerField("A", 0, 50, -1, ManagedFieldWithPanel.ControlType.arrows, "Screen")]
    public int screen;

    [BooleanField("B", false, ManagedFieldWithPanel.ControlType.button, "Set Screen To Current")]
    public bool setScreen;
}

public class RoomBackground(PlacedObject placedObject) : UpdatableAndDeletable
{
    private readonly PlacedObject placedObject = placedObject;
    private FAtlas screenAtlas;
    private RoomBackgroundSprite backgroundSprite;

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (DamagedShortcuts.TryGetValue(room.game, out var cameraPosition))
        {
            if ((placedObject.data as RoomBackgroundData).setScreen)
            {
                (placedObject.data as RoomBackgroundData).screen = cameraPosition.camPosition;
                (placedObject.data as RoomBackgroundData).setScreen = false;
            }

            bool createBackground = false;
            bool keepBackground = false;

            if (room.abstractRoom.name == cameraPosition.room) // If player in room
            {
                if ((placedObject.data as RoomBackgroundData).screen == cameraPosition.camPosition) // If player on screen
                {
                    if (backgroundSprite == null)
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
                    try
                    {
                        foreach (string file in from file in AssetManager.ListDirectory("tn_atlases/Screens")
                                                where Path.GetExtension(file).Equals(".png")
                                                select file)
                        {
                            string atlasPath = Path.ChangeExtension(file, null);
                            screenAtlas = Futile.atlasManager.LoadImage(atlasPath);

                            // Extract the background element name from the file path
                            string backgroundName = Path.GetFileNameWithoutExtension(file).ToLower(); // Convert to lowercase for comparison

                            //Plugin.DebugWarning($"Processing file: {file}, Background name: {backgroundName}"); Console spam causes lag
                            if (screenAtlas != null)
                            {
                                // Iterate through atlas elements to find a matching element
                                bool foundElement = false;
                                foreach (var element in screenAtlas._elementsByName)
                                {
                                    //Plugin.DebugWarning(element); Console Spam causes lag
                                    // Extract the filename from the full element name
                                    string elementName = Path.GetFileNameWithoutExtension(element.Key).ToLower();

                                    if (elementName == backgroundName)
                                    {
                                        backgroundSprite = new()
                                        {
                                            background = element.Value
                                        };
                                        room.AddObject(backgroundSprite);
                                        foundElement = true;
                                        break;
                                    }
                                }

                                if (!foundElement)
                                {
                                    Debug.LogError("Background element not found: " + backgroundName);
                                }
                            }
                            else
                            {
                                Debug.LogError("Screen atlas is null!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.DebugError(ex);
                        throw new Exception($"Failed to load {Plugin.MOD_NAME} atlases!");
                    }
                }

                // if (!keepBackground && !createBackground && screenAtlas != null && room != null && backgroundSprite != null)
                // {
                //     //screenAtlas.Unload();
                //     room.RemoveObject(backgroundSprite);
                //     backgroundSprite.Destroy();
                // }

                if (backgroundSprite != null)
                {
                    backgroundSprite.show = (placedObject.data as RoomBackgroundData).screen == cameraPosition.camPosition;
                }
            }
        }
    }
}

public class RoomBackgroundSprite : CosmeticSprite
{
    public FAtlasElement background;

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (background != null)
        {
            sLeaser.sprites[0].element = background;
            sLeaser.sprites[0].alpha = 0f;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            sLeaser.sprites[0].SetPosition(pos - rCam.pos);
            sLeaser.sprites[0].isVisible = show;
            sLeaser.sprites[0].scaleX = 1f;
            sLeaser.sprites[0].scaleY = 1f;
            sLeaser.sprites[0].anchorX = 0f;
            sLeaser.sprites[0].anchorY = 0f;
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("pixel", true);
        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Foreground");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }

        for (int i = 1; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[i - 1]);
        }
    }

    public bool show;
}