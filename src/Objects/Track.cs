namespace Slugpack;

public class TrainTrackData(PlacedObject owner) : ManagedData(owner, null)
{
    [BooleanField("Enabled", true, ManagedFieldWithPanel.ControlType.button, displayName: "Enabled")]
    public bool Enabled;

    [IntegerField("TrainDelay", 0, 60, 30, ManagedFieldWithPanel.ControlType.slider, displayName: "Train Delay")]
    public int TrainDelay;

    [IntegerField("WarningTimer", 0, 60, 10, ManagedFieldWithPanel.ControlType.slider, displayName: "Warning Timer")]
    public int WarningTimer;

    [StringField("RoomName", "TL_V01", displayName: "Room Name")]
    public string RoomName;

    [Vector2Field("TrainArea", 100, 100, Vector2Field.VectorReprType.rect)]
    public Vector2 TrainArea;

    [FloatField("TrainSpeed", 1, 150, 100, 1, ManagedFieldWithPanel.ControlType.slider, "Train Speed")]
    public float TrainSpeed;
}

public class TrainTrack(PlacedObject placedObject, Room room) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

        int padding = 160 * 3;
        int vertical_offset = 126;

        if ((placedObject.data as TrainTrackData).Enabled)
        {
            if (train_spawn_timer >= (placedObject.data as TrainTrackData).TrainDelay * 40)
            {
                car_queue = Random.Range(90, 140);
                first_car_index = car_queue;
                train_spawn_timer = Random.Range(-10, 10);
            }

            if (car_queue > 0 && (trainCar == null || trainCar.pos.x >= padding + connector_distance))
            {
                connector_distance = Random.Range(6, 9);

                int random_choice = car_queue == first_car_index ? Random.Range(0, 8) : Random.Range(0, 25);
                trainCar = new TrainObject(new Vector2(placedObject.pos.x - padding - 60, placedObject.pos.y + vertical_offset + Constants.TrainOffsets[random_choice]),
                                                       (placedObject.data as TrainTrackData).TrainSpeed, random_choice, padding, placedObject.pos.y,
                                                       placedObject.pos.y + (placedObject.data as TrainTrackData).TrainArea.y,
                                                       placedObject.pos.x + (placedObject.data as TrainTrackData).TrainArea.x,
                                                       (placedObject.data as TrainTrackData).RoomName, car_queue > 1, connector_distance);

                /*trainCar = new ProceduralTrain(this.placedObject.pos,
                                               Random.Range(0, 65535),
                                               Random.Range(28, 38),
                                               (this.placedObject.data as TrainTrackData).TrainSpeed,
                                               this.placedObject.pos.y,
                                               this.placedObject.pos.y + (this.placedObject.data as TrainTrackData).TrainArea.y,
                                               this.placedObject.pos.x + (this.placedObject.data as TrainTrackData).TrainArea.x,
                                               this.room);*/

                connector_distance = (connector_distance * 20 / 2) - 20;

                room.AddObject(trainCar);
                train_sprite++;
                car_queue--;
            }

            if (car_queue == 0)
            {
                trainCar = null;
                train_spawn_timer++;
            }
            spawn_timer++;
        }
    }

    public int car_queue;

    public int first_car_index;

    public int connector_distance = 0;

    public int train_spawn_timer;

    public int spawn_timer;

    public int train_sprite;

    public PlacedObject placedObject = placedObject;

    public TrainObject trainCar;
}

public class TrainObject : CosmeticSprite
{
    public TrainObject(Vector2 pos, float velocity, int sprite, int padding, float hit_lower_bound, float hit_upper_bound, float delete_position, string room_name, bool has_connector, int connector_length)
    {
        this.pos = pos;
        this.velocity = velocity;
        this.sprite = sprite;
        this.padding = padding;
        this.hit_lower_bound = hit_lower_bound;
        this.hit_upper_bound = hit_upper_bound;
        this.delete_position = delete_position;
        this.room_name = room_name;
        this.has_connector = has_connector;
        this.connector_length = connector_length;

        block = new MaterialPropertyBlock();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        var filteredObjects = room.physicalObjects
            .SelectMany(objectGroup => objectGroup)
            .Where(physObject =>
                physObject.bodyChunks.Any(chunk =>
                    RWCustom.Custom.Dist(chunk.pos, pos) < 453 &&
                     chunk.pos.y < hit_upper_bound &&
                     chunk.pos.y > hit_lower_bound
                )
            );
        try
        {
            foreach (var physObject in filteredObjects)
            {
                var targetChunks = physObject.bodyChunks
                    .Where(chunk =>
                        RWCustom.Custom.Dist(chunk.pos, pos) < 453 &&
                         chunk.pos.y < hit_upper_bound &&
                         chunk.pos.y > hit_lower_bound
                    );

                foreach (var chunk in targetChunks)
                {
                    if (chunk.pos.x > delete_position)
                    {
                        physObject.RemoveFromRoom();
                        physObject.Destroy();
                    }
                    else
                    {
                        chunk.vel = new Vector2(velocity * 1.25f, 5);

                        if (chunk.owner is not Creature)
                        {
                            if (Random.Range(0, 4) == 0)
                            {
                                // To future me : remember ot make it os that the slughacat tdoes ntot despawn when reaching the end ofth eroom
                                // also, get more sleep.... ou need it
                                if (chunk.owner is Spear && (chunk.owner as Spear).hasHorizontalBeamState)
                                {
                                    (chunk.owner as Spear).resetHorizontalBeamState();
                                    (chunk.owner as Spear).vibrate = 20;
                                    (chunk.owner as Spear).stuckInWall = Vector2.zero;
                                    (chunk.owner as Spear).firstChunk.collideWithTerrain = true;
                                    (chunk.owner as Spear).abstractSpear.stuckInWallCycles = 0;
                                    (chunk.owner as Spear).ChangeMode(Weapon.Mode.Free);
                                }

                                chunk.owner.RemoveFromRoom();
                                chunk.owner.Destroy();
                            }
                        }
                        else if (!has_connector && chunk.pos.x > delete_position)
                        {
                            chunk.owner.RemoveFromRoom();
                            chunk.owner.Destroy();
                        }

                        List<Player> players = room.physicalObjects
                            .SelectMany(category => category)
                            .OfType<Player>()
                            .Where(player => RWCustom.Custom.Dist(pos, player.mainBodyChunk.pos) < 1000f)
                            .ToList();

                        foreach (var player in players)
                        {
                            room.PlaySound(SoundID.Spear_Bounce_Off_Wall, chunk.pos);
                            for (int j = 0; j < 4; j++)
                            {
                                Vector2 a = RWCustom.Custom.RNV();
                                room.AddObject(new Spark(chunk.pos + (a * Random.value * 40f), a * Mathf.Lerp(4f, 5f, Random.value), new Color(0.9f, 0.9f, 1f), null, 4, 18));
                            }
                            if (physObject is Creature)
                            {
                                (physObject as Creature).Die();
                            }
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            //Nothing 
        }


        pos += new Vector2(velocity, 0);

        if (pos.x > room.PixelWidth + padding + 200)
        {
            Destroy();
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        Vector2 currentPos = Vector2.Lerp(lastPos, pos, timeStacker);

        Vector2 connector_offset = new(-padding, -Constants.ConnectorOffsets[connector_length - 6] - Constants.TrainOffsets[sprite]);

        if (room_name == "TL_V01")
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"{sprite}_21");
            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName($"{sprite}_68B");

            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].isVisible = true;
                sLeaser.sprites[i].SetPosition(currentPos - rCam.pos);

                sLeaser.sprites[i].scaleX = 3;
                sLeaser.sprites[i].scaleY = 2;

                sLeaser.sprites[i].alpha = 1f;
            }

            if (has_connector)
            {
                sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName($"Tube_{connector_length - 6}_21");
                sLeaser.sprites[2].isVisible = true;
                sLeaser.sprites[2].SetPosition(currentPos - rCam.pos + connector_offset);

                sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName($"Tube_{connector_length - 6}_68B");
                sLeaser.sprites[3].isVisible = true;
                sLeaser.sprites[3].SetPosition(currentPos - rCam.pos + connector_offset);
            }

            rainWorld = rCam.room.game.rainWorld;
            if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rainWorld, out var Shaders))
            {
                // Window Light
                sLeaser.sprites[0].shader = Shaders.ShadowMask;
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_ShadowMask", Shaders._shadowMask);
                sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Mode", 0);

                sLeaser.sprites[0]._renderLayer?._meshRenderer?.GetPropertyBlock(block); // This copies all data that already exists into your block object.
                block.SetTexture("_ShadowMask", Shaders._shadowMask);
                sLeaser.sprites[0]._renderLayer?._meshRenderer?.SetPropertyBlock(block);

                if (has_connector)
                {
                    // Window Light
                    sLeaser.sprites[2].shader = Shaders.ShadowMask;
                    sLeaser.sprites[2]._renderLayer?._material?.SetTexture("_ShadowMask", Shaders._shadowMask);
                    sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_Mode", 0);

                    sLeaser.sprites[2]._renderLayer?._meshRenderer?.GetPropertyBlock(block); // This copies all data that already exists into your block object.
                    block.SetTexture("_ShadowMask", Shaders._shadowMask);
                    sLeaser.sprites[2]._renderLayer?._meshRenderer?.SetPropertyBlock(block);
                }
            }
        }
        else if (room_name == "TL_OEGATE")
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"{sprite}_19A");

            sLeaser.sprites[0].isVisible = true;
            sLeaser.sprites[0].SetPosition(currentPos - rCam.pos);

            sLeaser.sprites[0].scaleX = 3;
            sLeaser.sprites[0].scaleY = 2;

            sLeaser.sprites[0].alpha = 1f;

            if (has_connector)
            {
                sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName($"Tube_{connector_length - 6}_19A");
                sLeaser.sprites[1].isVisible = true;
                sLeaser.sprites[1].SetPosition(currentPos - rCam.pos + connector_offset);
            }
        }

        // Gonna need this to set the normal sprite once multiple trains are going
        /*
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        // ^ You should store this somewhere, like in your instance of the object, don't make a new one every time
        _batteryMeter._renderLayer._meshRenderer.GetPropertyBlock(block); // This copies all data that already exists into your block object.
        block.SetTexture("texture", null);
        _batteryMeter._renderLayer._meshRenderer.SetPropertyBlock(block); // This updates the stored information
        */
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (room_name == "TL_V01")
        {
            sLeaser.sprites = has_connector ? (new FSprite[4]) : (new FSprite[2]);

            sLeaser.sprites[0] = new FSprite($"{sprite}_21", true);
            sLeaser.sprites[1] = new FSprite($"{sprite}_68B", true);

            sLeaser.sprites[0].isVisible = false;
            sLeaser.sprites[1].isVisible = false;

            sLeaser.sprites[0].SetPosition(pos);
            sLeaser.sprites[1].SetPosition(pos);

            if (has_connector)
            {
                sLeaser.sprites[2] = new FSprite($"Tube_{connector_length - 6}_21", true)
                {
                    isVisible = false
                };
                sLeaser.sprites[2].SetPosition(pos);

                sLeaser.sprites[3] = new FSprite($"Tube_{connector_length - 6}_68B", true)
                {
                    isVisible = false
                };
                sLeaser.sprites[3].SetPosition(pos);
            }
        }
        if (room_name == "TL_OEGATE")
        {
            sLeaser.sprites = has_connector ? (new FSprite[2]) : (new FSprite[1]);

            sLeaser.sprites[0] = new FSprite($"{sprite}_19A", true)
            {
                isVisible = false
            };
            sLeaser.sprites[0].SetPosition(pos);

            if (has_connector)
            {
                sLeaser.sprites[1] = new FSprite($"Tube_{connector_length - 6}_19A", true)
                {
                    isVisible = false
                };
                sLeaser.sprites[1].SetPosition(pos);
            }
        }
        if (room_name == "TL_AC01")
        {
            sLeaser.sprites = has_connector ? (new FSprite[2]) : (new FSprite[1]);

            sLeaser.sprites[0] = new FSprite($"{sprite}_19A", true)
            {
                isVisible = false
            };
            sLeaser.sprites[0].SetPosition(pos);

            if (has_connector)
            {
                sLeaser.sprites[1] = new FSprite($"Tube_{connector_length - 6}_19A", true)
                {
                    isVisible = false
                };
                sLeaser.sprites[1].SetPosition(pos);
            }
        }

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

        for (int i = 1; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[i - 1]);
        }
    }

    public float velocity;

    public int sprite;

    public int padding;

    public int connector_length;

    public bool has_connector;

    public float hit_lower_bound;

    public float hit_upper_bound;

    public float delete_position;

    public float TrainSpeed;

    public string room_name;

    public RainWorld rainWorld;

    public MaterialPropertyBlock block;
}