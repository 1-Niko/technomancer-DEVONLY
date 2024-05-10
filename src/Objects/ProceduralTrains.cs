namespace Slugpack;

public class TrainTrackObjectData(PlacedObject owner) : ManagedData(owner, null)
{
    [IntegerField("A", 0, 65535, 0, ManagedFieldWithPanel.ControlType.slider, displayName: "Seed")]
    public int seed;

    [IntegerField("B", 0, 3, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "Sprite")]
    public int useSprite;

    [BooleanField("C", false, ManagedFieldWithPanel.ControlType.button, displayName: "Force Pipe (If Valid)")]
    public bool forceEntrance;

    [BooleanField("D", false, ManagedFieldWithPanel.ControlType.button, displayName: "Force Shelter (If Valid)")]
    public bool forceShelter;
}

public class TrainLeftHead : UpdatableAndDeletable
{
    public TrainLeftHead(PlacedObject placedObject, Room room)
    {
        this.placedObject = placedObject;
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (dynamicSprite == null)
        {
            dynamicSprite = new TrainSpriteObject(placedObject.pos, (placedObject.data as TrainTrackObjectData).seed);
            room.AddObject(dynamicSprite);
        }

        dynamicSprite.useSprite = (placedObject.data as TrainTrackObjectData).useSprite;
        dynamicSprite.seed = (placedObject.data as TrainTrackObjectData).seed;
        dynamicSprite.forcePipe = (placedObject.data as TrainTrackObjectData).forceEntrance;
        dynamicSprite.forceShelter = (placedObject.data as TrainTrackObjectData).forceShelter;
        dynamicSprite.pos = placedObject.pos;
    }

    private PlacedObject placedObject;
    private TrainSpriteObject dynamicSprite;
}

public class TrainSpriteObject : CosmeticSprite
{
    private static readonly string[,] spriteNames = new string[,] {
        {
            "Train Car N_0", "Train Car N_1", "Train Car N_2",
            "Train Car N_3", "Train Car N_4", "Train Car N_5",
            "Train Car N_6", "Train Car N_6", "Train Car N_6",
            "Train Car N_6"
        },
        {
            "Train Car NE_0", "Train Car NE_1", "Train Car NE_2",
            "Train Car NE_3", "Train Car NE_4", "Train Car NE_5",
            "Train Car NE_6", "Train Car NE_6", "Train Car NE_6",
            "Train Car NE_6"
        },
        {
            "Train Car NW_0", "Train Car NW_1", "Train Car NW_2",
            "Train Car NW_3", "Train Car NW_4", "Train Car NW_5",
            "Train Car NW_6", "Train Car NW_6", "Train Car NW_6",
            "Train Car NW_6"
        },
        {
            "Train Wheel N_0", "Train Wheel N_1", "Train Wheel N_2",
            "Train Wheel N_3", "Train Wheel N_4", "Train Wheel N_5",
            "Train Wheel N_5", "Train Wheel N_5", "Train Wheel N_5",
            "Train Wheel N_5"
        }
    };

    public TrainSpriteObject(Vector2 pos, int seed)
    {
        this.pos = pos;
        this.seed = seed;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        DEBUGTIMER++;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        Random.State state = Random.state;
        Random.InitState(seed);

        // Normal tile setting (All tiles have this)
        Vector2 currentPos = Vector2.Lerp(lastPos, pos, timeStacker);

        Vector2 adjustedPos = currentPos - rCam.pos;
        Vector2 screenPosition = adjustedPos / new Vector2(1364f / 2f, 770f / 2f);

        for (int i = 0; i < normal_slice_count; i++)
        {
            foreach (int shadowOffset in new[] { 0, 10 })
            {
                sLeaser.sprites[i + shadowOffset].element = Futile.atlasManager.GetElementWithName(spriteNames[useSprite, i]);
                sLeaser.sprites[i + shadowOffset].isVisible = true;
                sLeaser.sprites[i + shadowOffset].SetPosition(adjustedPos - new Vector2(i * (screenPosition.x - 1f), i * (screenPosition.y - 1f)));

                sLeaser.sprites[i + shadowOffset].color = new Color(i / 9f, seed / 65535f, 0f, -(i / 30) + (29f / 30f));

                if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
                {
                    sLeaser.sprites[i + shadowOffset].shader = shaders.DynamicTrain;
                    sLeaser.sprites[i + shadowOffset]._renderLayer?._material?.SetTexture("_ShadowMask", shaders._shadowMask);
                }
            }
        }

        // Endcap-exclusive randomness
        /*if (this.useSprite == 1 || this.useSprite == 2)
        {
            bool hasAnyPipe = false;
            bool pipeIsShelter = false;
            int pipeId = -1;

            if ((this.forcePipe || this.forceShelter)) //  || UnityEngine.Random.Range(0,4) == 0)
                hasAnyPipe = true;

            if (this.forceShelter) // || hasAnyPipe && UnityEngine.Random.Range(0, 6) == 0)
                pipeIsShelter = true;

            // Since this segment is exclusively counting the endcaps, any non-shelter pipe entrances would necessarily be horizontal
            if (hasAnyPipe && !pipeIsShelter)
                pipeId = 4;
            // If it's made it to this point, then the pipe must be shelter, since we're still checking for "has a pipe", so I just
            // have to account for which endcap is active
            else if (hasAnyPipe)
                pipeId = this.useSprite + 1;

            sLeaser.sprites[normal_slice_count + shadow_slice_count].SetPosition(adjustedPos + new Vector2(-2f + ((this.useSprite == 2) ? 0f : 4f), -28f));
            sLeaser.sprites[normal_slice_count + shadow_slice_count].element = Futile.atlasManager.GetElementWithName(pipeEntranceNames[Mathf.Max(0,pipeId)]);
            sLeaser.sprites[normal_slice_count + shadow_slice_count].isVisible = pipeId != -1;

            sLeaser.sprites[normal_slice_count + shadow_slice_count].color = new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.45f, 0.85f));
        }
        else
        {
            sLeaser.sprites[normal_slice_count + shadow_slice_count].isVisible = false;
        }
        */

        Random.state = state;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        int total_sprite_count = normal_slice_count + shadow_slice_count + possible_entrance_count;

        sLeaser.sprites = new FSprite[total_sprite_count];

        for (int i = 0; i < total_sprite_count; i++)
            sLeaser.sprites[i] = new FSprite("pixel", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        // Why did I set these sprites to be in the hud? I know I had a good reason but I don't remember it

        // Probably something to do with the shader depth, since it seems it is not rendering correctly at the moment
        // Will have to fix that when I get a chance

        // It is a much stickier issue than I had hoped, will probably have to put this off for a while

        // FOR FUTURE REFERENCE:
        // These sprites being in the hud is to solve the BLOOM issue, to keep it from bleeding through incorrectly

        FContainer hudContainer = rCam.ReturnFContainer("HUD");
        FContainer midContainer = rCam.ReturnFContainer("Midground");
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            hudContainer.AddChild(fsprite);
        }

        for (int i = normal_slice_count; i < normal_slice_count + shadow_slice_count; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            midContainer.AddChild(sLeaser.sprites[i]);
        }

        for (int i = 1; i < sLeaser.sprites.Length; i++)
        {
            if (i < 10 + 10)
                sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[i - 1]);
            else
                sLeaser.sprites[i].MoveInFrontOfOtherNode(sLeaser.sprites[0]);
        }
    }

    // Sprite Groups
    public static int normal_slice_count = 10;

    public static int shadow_slice_count = normal_slice_count;

    public static int possible_entrance_count = 0;

    // Everything else
    public int useSprite;

    public int seed;

    public bool forcePipe;

    public bool forceShelter;

    public int DEBUGTIMER;
}