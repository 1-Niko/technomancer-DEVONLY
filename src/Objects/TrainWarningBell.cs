using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class TrainWarningBellData : ManagedData
    {
        [BooleanField("IsCeilingEntranceMarker", true, ManagedFieldWithPanel.ControlType.button, displayName: "Is Marker")]
        public bool EntranceMarker;

        [BooleanField("IsEnabled", true, ManagedFieldWithPanel.ControlType.button, displayName: "Enabled")]
        public bool Enabled;

        [BooleanField("ForceDisplay", false, ManagedFieldWithPanel.ControlType.button, displayName: "Force Display")]
        public bool ForceDisplay;

        [IntegerField("TimerDelay", 0, 160, 100, ManagedFieldWithPanel.ControlType.slider, displayName: "Wait Count")]
        public int TimerDelay;

        [FloatField("ProjectionLineSizeA", 0, 0.2f, 0.1f, 0.001f, ManagedFieldWithPanel.ControlType.slider, "Line Size Warning A")]
        public float LineSizeA;

        [FloatField("ProjectionLineSizeB", 0, 0.2f, 0.1f, 0.001f, ManagedFieldWithPanel.ControlType.slider, "Line Size Warning B")]
        public float LineSizeB;

        [FloatField("ProjectionLineSizeC", 0, 0.2f, 0.1f, 0.001f, ManagedFieldWithPanel.ControlType.slider, "Line Size Marker A")]
        public float LineSizeC;

        [FloatField("ProjectionLineSizeD", 0, 0.2f, 0.1f, 0.001f, ManagedFieldWithPanel.ControlType.slider, "Line Size Marker B")]
        public float LineSizeD;

        [FloatField("LineCutoffA", 0, 1, 0.1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Cutoff Warning A")]
        public float LowerCutoffA;

        [FloatField("LineCutoffB", 0, 1, 0.1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Cutoff Warning B")]
        public float LowerCutoffB;

        [FloatField("LineCutoffC", 0, 1, 0.1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Cutoff Marker A")]
        public float LowerCutoffC;

        [FloatField("LineCutoffD", 0, 1, 0.1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Cutoff Marker B")]
        public float LowerCutoffD;

        [Vector2Field("radius", 50, 50, Vector2Field.VectorReprType.circle)]
        public Vector2 rad;

        [Vector2Field("ArrowLocation", -100, 100, Vector2Field.VectorReprType.line)]
        public Vector2 ArrowPosition;

        [Vector2ArrayField("ProjectionLinesWarning", 3, true, Vector2ArrayRepresentationType.Polygon, new float[6] { 0, 0, -85, 0, 0, 85 })]
        public Vector2[] projectionLines;

        [Vector2ArrayField("ProjectionLinesMarker", 3, true, Vector2ArrayRepresentationType.Polygon, new float[6] { 0, 0, -85, 20, -20, 85 })]
        public Vector2[] projectionLinesMarker;

        public TrainWarningBellData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class TrainWarningBell : UpdatableAndDeletable
    {
        public TrainWarningBell(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.bellSprite == null)
            {
                this.bellSprite = new TrainBell(placedObject, placedObject.pos, (this.placedObject.data as TrainWarningBellData).TimerDelay, (this.placedObject.data as TrainWarningBellData).ArrowPosition);
                this.room.AddObject(this.bellSprite);
            }
            this.bellSprite.pos = placedObject.pos;
            this.bellSprite.waitCount = (this.placedObject.data as TrainWarningBellData).TimerDelay;
            this.bellSprite.hardEnabled = (this.placedObject.data as TrainWarningBellData).Enabled;
            this.bellSprite.hatchMarker = (this.placedObject.data as TrainWarningBellData).EntranceMarker;
            this.bellSprite.arrowPosition = (this.placedObject.data as TrainWarningBellData).ArrowPosition;
            this.bellSprite.forceWarning = (this.placedObject.data as TrainWarningBellData).ForceDisplay; // This is also likely what im going to use for the techy action
            this.bellSprite.projectionLines = (this.placedObject.data as TrainWarningBellData).projectionLines;
            this.bellSprite.projectionLinesMarker = (this.placedObject.data as TrainWarningBellData).projectionLinesMarker;
            this.bellSprite.LineSizeA = (this.placedObject.data as TrainWarningBellData).LineSizeA;
            this.bellSprite.LineSizeB = (this.placedObject.data as TrainWarningBellData).LineSizeB;
            this.bellSprite.LineSizeC = (this.placedObject.data as TrainWarningBellData).LineSizeC;
            this.bellSprite.LineSizeD = (this.placedObject.data as TrainWarningBellData).LineSizeD;
            this.bellSprite.LineCutoffA = (this.placedObject.data as TrainWarningBellData).LowerCutoffA;
            this.bellSprite.LineCutoffB = (this.placedObject.data as TrainWarningBellData).LowerCutoffB;
            this.bellSprite.LineCutoffC = (this.placedObject.data as TrainWarningBellData).LowerCutoffC;
            this.bellSprite.LineCutoffD = (this.placedObject.data as TrainWarningBellData).LowerCutoffD;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        private TrainBell bellSprite;

        private PlacedObject placedObject;
    }

    public class TrainBell : CosmeticSprite
    {
        private readonly PlacedObject placedObject;

        public TrainBell(PlacedObject placedObject, Vector2 pos, int waitCount, Vector2 ArrowPosition)
        {
            this.placedObject = placedObject;
            this.waitCount = waitCount;
            this.arrowPosition = ArrowPosition;

            block = new MaterialPropertyBlock();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            stepTimer++;

            List<Player> players = new();
            foreach (var category in this.room.physicalObjects)
            {
                foreach (var physicalObject in category)
                {
                    if (physicalObject is Player)
                    {
                        players.Add((physicalObject as Player));
                    }
                }
            }

            this.enabled = false;
            for (int i = 0; i < players.Count; i++)
            {
                if (RWCustom.Custom.Dist(this.pos, players[i].mainBodyChunk.pos) < RWCustom.Custom.Dist(this.pos, this.pos + (this.placedObject.data as TrainWarningBellData).rad))
                {
                    this.enabled = true;
                    break;
                }
                if (this.enabled)
                    break;
            }

            var trackController = room.updateList.Where(element => (element.ToString() == "Slugpack.TrainTrack")).ToList();

            if (trackController.Count == 0)
            { this.enabled = false; } // There are no trains, why is this in this room?
            else if (trackController.Count == 1)
            {
                // This is where normal logic goes
                TrainTrack trainController = (trackController[0] as TrainTrack);

                var trainPositions = this.room.updateList
                    .OfType<TrainObject>()
                    .Select(trainObject => trainObject.pos)
                    .ToList();
                if (trainController.train_spawn_timer >= (trainController.placedObject.data as TrainTrackData).TrainDelay * 35f) // This '35' determines how soon before the train triggers it will switch
                    this.warning = true;
                else if (trainController.car_queue == 0 && trainPositions.Count == 0) // No more cars being spawned or in the room
                {
                    if (this.hatchMarker)
                        this.warning = false;
                    else
                        this.enabled = false;
                }
            }
            else
            { this.enabled = false; } // Either there's a negative amount of track controllers, or more than 1. Either way, something has gone wrong

            if (this.forceWarning)
                this.warning = true;

            int bellLength = 32;
            for (int i = 0; i < players.Count; i++)
            {
                if (RWCustom.Custom.Dist(this.pos, players[i].mainBodyChunk.pos) < RWCustom.Custom.Dist(this.pos, this.pos + (this.placedObject.data as TrainWarningBellData).rad))
                {
                    if ((this.enabled && this.warning) || this.forceWarning)
                    {
                        if (stepTimer % bellLength == bellLength / 2)
                            this.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, this.pos, 1, 0.9f);
                        else if (stepTimer % bellLength == 0)
                            this.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, this.pos, 1, 0.5f);
                    }
                }
                break;
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("hatchSign");
            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("trainWarning");
            sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("smallBlank");
            sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("smallBlank");
            sLeaser.sprites[4].element = Futile.atlasManager.GetElementWithName("smallBlank");
            sLeaser.sprites[5].element = Futile.atlasManager.GetElementWithName("smallBlank");

            sLeaser.sprites[0].isVisible = false;
            sLeaser.sprites[1].isVisible = false;

            sLeaser.sprites[2].isVisible = false;
            sLeaser.sprites[3].isVisible = false;
            sLeaser.sprites[4].isVisible = false;
            sLeaser.sprites[5].isVisible = false;

            float[] xCoordinates = new float[5] { this.pos.x + this.arrowPosition.x, this.pos.x + this.projectionLines[1].x, this.pos.x + this.projectionLines[2].x, this.pos.x + this.projectionLinesMarker[1].x, this.pos.x + this.projectionLinesMarker[2].x };
            float[] yCoordinates = new float[5] { this.pos.y + this.arrowPosition.y, this.pos.y + this.projectionLines[1].y, this.pos.y + this.projectionLines[2].y, this.pos.y + this.projectionLinesMarker[1].y, this.pos.y + this.projectionLinesMarker[2].y };

            float image_size = 256f;

            sLeaser.sprites[2].SetPosition(new Vector2((xCoordinates[0] + xCoordinates[1]) / 2f, (yCoordinates[0] + yCoordinates[1]) / 2f) - rCam.pos);
            sLeaser.sprites[2].scaleX = ((xCoordinates[0] > xCoordinates[1]) ? 1 : -1) * ((Mathf.Max(xCoordinates[0], xCoordinates[1]) - Mathf.Min(xCoordinates[0], xCoordinates[1])) / image_size);
            sLeaser.sprites[2].scaleY = ((yCoordinates[0] > yCoordinates[1]) ? 1 : -1) * ((Mathf.Max(yCoordinates[0], yCoordinates[1]) - Mathf.Min(yCoordinates[0], yCoordinates[1])) / image_size);

            sLeaser.sprites[3].SetPosition(new Vector2((xCoordinates[2] + xCoordinates[0]) / 2f, (yCoordinates[2] + yCoordinates[0]) / 2f) - rCam.pos);
            sLeaser.sprites[3].scaleX = ((xCoordinates[2] > xCoordinates[0]) ? -1 : 1) * ((Mathf.Max(xCoordinates[2], xCoordinates[0]) - Mathf.Min(xCoordinates[2], xCoordinates[0])) / image_size);
            sLeaser.sprites[3].scaleY = ((yCoordinates[2] > yCoordinates[0]) ? -1 : 1) * ((Mathf.Max(yCoordinates[2], yCoordinates[0]) - Mathf.Min(yCoordinates[2], yCoordinates[0])) / image_size);

            sLeaser.sprites[4].SetPosition(new Vector2((xCoordinates[0] + xCoordinates[3]) / 2f, (yCoordinates[0] + yCoordinates[3]) / 2f) - rCam.pos);
            sLeaser.sprites[4].scaleX = ((xCoordinates[0] > xCoordinates[3]) ? 1 : -1) * ((Mathf.Max(xCoordinates[0], xCoordinates[3]) - Mathf.Min(xCoordinates[0], xCoordinates[3])) / image_size);
            sLeaser.sprites[4].scaleY = ((yCoordinates[0] > yCoordinates[3]) ? 1 : -1) * ((Mathf.Max(yCoordinates[0], yCoordinates[3]) - Mathf.Min(yCoordinates[0], yCoordinates[3])) / image_size);

            sLeaser.sprites[5].SetPosition(new Vector2((xCoordinates[4] + xCoordinates[0]) / 2f, (yCoordinates[4] + yCoordinates[0]) / 2f) - rCam.pos);
            sLeaser.sprites[5].scaleX = ((xCoordinates[4] > xCoordinates[0]) ? -1 : 1) * ((Mathf.Max(xCoordinates[4], xCoordinates[0]) - Mathf.Min(xCoordinates[4], xCoordinates[0])) / image_size);
            sLeaser.sprites[5].scaleY = ((yCoordinates[4] > yCoordinates[0]) ? -1 : 1) * ((Mathf.Max(yCoordinates[4], yCoordinates[0]) - Mathf.Min(yCoordinates[4], yCoordinates[0])) / image_size);

            if (this.hardEnabled && (this.enabled || this.forceWarning))
            {
                if (Constants.shaders_enabled)
                    if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
                    {
                        if (!(this.warning || this.forceWarning))
                        {
                            sLeaser.sprites[0].SetPosition(this.pos - rCam.pos);
                            sLeaser.sprites[0].isVisible = true;
                            sLeaser.sprites[1].isVisible = false;

                            sLeaser.sprites[0].shader = Shaders.HologramA;
                            sLeaser.sprites[1]._renderLayer?._material?.SetInt("_Radial", 1);
                            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Offset", stepTimer * 2);
                            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_RandomOffset", stepTimer);
                            sLeaser.sprites[0]._renderLayer?._material?.SetVector("_ColourA", Utilities.ColourFade(new Vector4(0f, 1f, 0f, 1f), new Vector4(0f, 1f, 1f, 1f), (-Mathf.Cos((3.141f * (stepTimer / 10f)) / 4) + 1) / 2));

                            sLeaser.sprites[4].shader = Shaders.ProjectionLinesC;
                            sLeaser.sprites[5].shader = Shaders.ProjectionLinesD;
                            for (int i = 0; i < 2; i++)
                            {
                                sLeaser.sprites[i + 4].isVisible = true;

                                sLeaser.sprites[i + 4]._renderLayer?._material?.SetVector("_PointA", new Vector4(1, 0, 0, 0)); // Left Anchor
                                sLeaser.sprites[i + 4]._renderLayer?._material?.SetVector("_PointB", new Vector4(0, 1, 0, 0)); // Origin

                                sLeaser.sprites[i + 4]._renderLayer?._material?.SetFloat("_Width", sLeaser.sprites[i + 4].scaleX);
                                sLeaser.sprites[i + 4]._renderLayer?._material?.SetFloat("_Height", sLeaser.sprites[i + 4].scaleY);
                                sLeaser.sprites[i + 4]._renderLayer?._material?.SetFloat("_Offset", stepTimer * 2);
                                sLeaser.sprites[i + 4]._renderLayer?._material?.SetFloat("_RandomOffset", stepTimer);
                            }
                            sLeaser.sprites[4]._renderLayer?._material?.SetFloat("_Cutoff", this.LineCutoffC);
                            sLeaser.sprites[5]._renderLayer?._material?.SetFloat("_Cutoff", this.LineCutoffD);
                            sLeaser.sprites[4]._renderLayer?._material?.SetFloat("_Size", this.LineSizeC);
                            sLeaser.sprites[5]._renderLayer?._material?.SetFloat("_Size", this.LineSizeD);

                            sLeaser.sprites[4]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(0f, 1f, 0f, 1f), new Vector4(0f, 1f, 1f, 1f), (-Mathf.Cos((3.141f * (stepTimer / 4f)) / 4) + 1) / 2));
                            sLeaser.sprites[5]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(0f, 1f, 0f, 1f), new Vector4(0f, 1f, 1f, 1f), (-Mathf.Cos((3.141f * (stepTimer / 4f)) / 4) + 1) / 2));
                        }
                        else
                        {
                            sLeaser.sprites[1].SetPosition(this.pos - rCam.pos);
                            sLeaser.sprites[0].isVisible = false;
                            sLeaser.sprites[1].isVisible = true;

                            sLeaser.sprites[1].shader = Shaders.HologramB;
                            sLeaser.sprites[1]._renderLayer?._material?.SetInt("_Radial", 0);
                            sLeaser.sprites[1]._renderLayer?._material?.SetFloat("_Offset", stepTimer * 2);
                            sLeaser.sprites[1]._renderLayer?._material?.SetFloat("_RandomOffset", stepTimer);
                            sLeaser.sprites[1]._renderLayer?._material?.SetVector("_ColourA", Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 0f, 1f), (-Mathf.Cos((3.141f * (stepTimer / 4f)) / 4) + 1) / 2));

                            sLeaser.sprites[2].shader = Shaders.ProjectionLinesA;
                            sLeaser.sprites[3].shader = Shaders.ProjectionLinesB;
                            for (int i = 0; i < 2; i++)
                            {
                                sLeaser.sprites[i + 2].isVisible = true;

                                sLeaser.sprites[i + 2]._renderLayer?._material?.SetVector("_PointA", new Vector4(1, 0, 0, 0)); // Left Anchor
                                sLeaser.sprites[i + 2]._renderLayer?._material?.SetVector("_PointB", new Vector4(0, 1, 0, 0)); // Origin

                                sLeaser.sprites[i + 2]._renderLayer?._material?.SetFloat("_Width", sLeaser.sprites[i + 2].scaleX);
                                sLeaser.sprites[i + 2]._renderLayer?._material?.SetFloat("_Height", sLeaser.sprites[i + 2].scaleY);
                                sLeaser.sprites[i + 2]._renderLayer?._material?.SetFloat("_Offset", stepTimer * 2);
                                sLeaser.sprites[i + 2]._renderLayer?._material?.SetFloat("_RandomOffset", stepTimer);
                            }
                            sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_Cutoff", this.LineCutoffA);
                            sLeaser.sprites[3]._renderLayer?._material?.SetFloat("_Cutoff", this.LineCutoffB);
                            sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_Size", this.LineSizeA);
                            sLeaser.sprites[3]._renderLayer?._material?.SetFloat("_Size", this.LineSizeB);

                            sLeaser.sprites[2]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 0f, 1f), (-Mathf.Cos((3.141f * (stepTimer / 4f)) / 4) + 1) / 2));
                            sLeaser.sprites[3]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 0f, 1f), (-Mathf.Cos((3.141f * (stepTimer / 4f)) / 4) + 1) / 2));
                        }
                    }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[6];

            sLeaser.sprites[0] = new FSprite("hatchSign", true);
            sLeaser.sprites[1] = new FSprite("trainWarning", true);
            sLeaser.sprites[2] = new FSprite("smallBlank", true);
            sLeaser.sprites[3] = new FSprite("smallBlank", true);
            sLeaser.sprites[4] = new FSprite("smallBlank", true);
            sLeaser.sprites[5] = new FSprite("smallBlank", true);

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

        public int spriteTimer;

        public int stepTimer;

        public int waitCount;

        public bool warning;

        public bool enabled;

        public bool hardEnabled;

        public bool hatchMarker;

        public bool forceWarning;

        public float LineSizeA;

        public float LineSizeB;

        public float LineSizeC;

        public float LineSizeD;

        public float LineCutoffA;

        public float LineCutoffB;

        public float LineCutoffC;

        public float LineCutoffD;

        public Vector2 arrowPosition;

        public Vector2[] projectionLines;

        public Vector2[] projectionLinesMarker;

        public MaterialPropertyBlock block;
    }
}