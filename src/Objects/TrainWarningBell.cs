using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class TrainWarningBellData(PlacedObject owner) : ManagedData(owner, null)
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
    }

    public class TrainWarningBell(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (bellSprite == null)
            {
                bellSprite = new TrainBell(placedObject, placedObject.pos, (placedObject.data as TrainWarningBellData).TimerDelay, (placedObject.data as TrainWarningBellData).ArrowPosition);
                room.AddObject(bellSprite);
            }
            bellSprite.pos = placedObject.pos;
            bellSprite.waitCount = (placedObject.data as TrainWarningBellData).TimerDelay;
            bellSprite.hardEnabled = (placedObject.data as TrainWarningBellData).Enabled;
            bellSprite.hatchMarker = (placedObject.data as TrainWarningBellData).EntranceMarker;
            bellSprite.arrowPosition = (placedObject.data as TrainWarningBellData).ArrowPosition;
            bellSprite.forceWarning = (placedObject.data as TrainWarningBellData).ForceDisplay; // This is also likely what im going to use for the techy action
            bellSprite.projectionLines = (placedObject.data as TrainWarningBellData).projectionLines;
            bellSprite.projectionLinesMarker = (placedObject.data as TrainWarningBellData).projectionLinesMarker;
            bellSprite.LineSizeA = (placedObject.data as TrainWarningBellData).LineSizeA;
            bellSprite.LineSizeB = (placedObject.data as TrainWarningBellData).LineSizeB;
            bellSprite.LineSizeC = (placedObject.data as TrainWarningBellData).LineSizeC;
            bellSprite.LineSizeD = (placedObject.data as TrainWarningBellData).LineSizeD;
            bellSprite.LineCutoffA = (placedObject.data as TrainWarningBellData).LowerCutoffA;
            bellSprite.LineCutoffB = (placedObject.data as TrainWarningBellData).LowerCutoffB;
            bellSprite.LineCutoffC = (placedObject.data as TrainWarningBellData).LowerCutoffC;
            bellSprite.LineCutoffD = (placedObject.data as TrainWarningBellData).LowerCutoffD;
        }

        private TrainBell bellSprite;

        private PlacedObject placedObject = placedObject;
    }

    public class TrainBell(PlacedObject placedObject, Vector2 pos, int waitCount, Vector2 ArrowPosition) : CosmeticSprite
    {
        private readonly PlacedObject placedObject = placedObject;

        public override void Update(bool eu)
        {
            base.Update(eu);
            stepTimer++;

            List<Player> players = [];
            foreach (var category in room.physicalObjects)
            {
                players.AddRange(from physicalObject in category
                                 where physicalObject is Player
                                 select physicalObject as Player);
            }

            enabled = false;
            for (int i = 0; i < players.Count; i++)
            {
                if (RWCustom.Custom.Dist(pos, players[i].mainBodyChunk.pos) < RWCustom.Custom.Dist(pos, pos + (placedObject.data as TrainWarningBellData).rad))
                {
                    enabled = true;
                    break;
                }
                if (enabled)
                    break;
            }

            var trackController = room.updateList.Where(element => element.ToString() == "Slugpack.TrainTrack").ToList();

            if (trackController.Count == 0)
            { enabled = false; } // There are no trains, why is this in this room?
            else if (trackController.Count == 1)
            {
                // This is where normal logic goes
                TrainTrack trainController = trackController[0] as TrainTrack;

                var trainPositions = room.updateList
                    .OfType<TrainObject>()
                    .Select(trainObject => trainObject.pos)
                    .ToList();
                if (trainController.train_spawn_timer >= (trainController.placedObject.data as TrainTrackData).TrainDelay * 35f) // This '35' determines how soon before the train triggers it will switch
                    warning = true;
                else if (trainController.car_queue == 0 && trainPositions.Count == 0) // No more cars being spawned or in the room
                {
                    if (hatchMarker)
                        warning = false;
                    else
                        enabled = false;
                }
            }
            else
            { enabled = false; } // Either there's a negative amount of track controllers, or more than 1. Either way, something has gone wrong

            if (forceWarning)
                warning = true;

            int bellLength = 32;

            for (int i = 0; i < players.Count; i++)
            {
                if (RWCustom.Custom.Dist(pos, players[i].mainBodyChunk.pos) < RWCustom.Custom.Dist(pos, pos + (placedObject.data as TrainWarningBellData).rad) && ((enabled && warning) || forceWarning))
                {
                    if (stepTimer % bellLength == bellLength / 2)
                        room.PlaySound(SoundID.Spear_Bounce_Off_Wall, pos, 1, 0.9f);
                    else if (stepTimer % bellLength == 0)
                        room.PlaySound(SoundID.Spear_Bounce_Off_Wall, pos, 1, 0.5f);
                }
                break;
            }
            if (true)
            {

            }
            else
            {
                Plugin.DebugLog("Hello, World!");
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

            float[] xCoordinates = [pos.x + arrowPosition.x, pos.x + projectionLines[1].x, pos.x + projectionLines[2].x, pos.x + projectionLinesMarker[1].x, pos.x + projectionLinesMarker[2].x];
            float[] yCoordinates = [pos.y + arrowPosition.y, pos.y + projectionLines[1].y, pos.y + projectionLines[2].y, pos.y + projectionLinesMarker[1].y, pos.y + projectionLinesMarker[2].y];

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

            if (hardEnabled && (enabled || forceWarning) && Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
            {
                if (!(warning || forceWarning))
                {
                    sLeaser.sprites[0].SetPosition(pos - rCam.pos);
                    sLeaser.sprites[0].isVisible = true;
                    sLeaser.sprites[1].isVisible = false;

                    sLeaser.sprites[0].shader = Shaders.HologramA;
                    sLeaser.sprites[0]._renderLayer?._material?.SetInt("_Radial", 1);
                    sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_Offset", stepTimer * 2);
                    sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_RandomOffset", stepTimer);
                    sLeaser.sprites[0]._renderLayer?._material?.SetVector("_ColourA", Utilities.ColourFade(new Vector4(0f, 1f, 0f, 1f), new Vector4(0f, 1f, 1f, 1f), (-Mathf.Cos(3.141f * (stepTimer / 10f) / 4) + 1) / 2));

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
                    sLeaser.sprites[4]._renderLayer?._material?.SetFloat("_Cutoff", LineCutoffC);
                    sLeaser.sprites[5]._renderLayer?._material?.SetFloat("_Cutoff", LineCutoffD);
                    sLeaser.sprites[4]._renderLayer?._material?.SetFloat("_Size", LineSizeC);
                    sLeaser.sprites[5]._renderLayer?._material?.SetFloat("_Size", LineSizeD);

                    sLeaser.sprites[4]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(0f, 1f, 0f, 1f), new Vector4(0f, 1f, 1f, 1f), (-Mathf.Cos(3.141f * (stepTimer / 4f) / 4) + 1) / 2));
                    sLeaser.sprites[5]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(0f, 1f, 0f, 1f), new Vector4(0f, 1f, 1f, 1f), (-Mathf.Cos(3.141f * (stepTimer / 4f) / 4) + 1) / 2));
                }
                else
                {
                    sLeaser.sprites[1].SetPosition(pos - rCam.pos);
                    sLeaser.sprites[0].isVisible = false;
                    sLeaser.sprites[1].isVisible = true;

                    sLeaser.sprites[1].shader = Shaders.HologramB;
                    sLeaser.sprites[1]._renderLayer?._material?.SetInt("_Radial", 0);
                    sLeaser.sprites[1]._renderLayer?._material?.SetFloat("_Offset", stepTimer * 2);
                    sLeaser.sprites[1]._renderLayer?._material?.SetFloat("_RandomOffset", stepTimer);
                    sLeaser.sprites[1]._renderLayer?._material?.SetVector("_ColourA", Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 0f, 1f), (-Mathf.Cos(3.141f * (stepTimer / 4f) / 4) + 1) / 2));

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
                    sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_Cutoff", LineCutoffA);
                    sLeaser.sprites[3]._renderLayer?._material?.SetFloat("_Cutoff", LineCutoffB);
                    sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_Size", LineSizeA);
                    sLeaser.sprites[3]._renderLayer?._material?.SetFloat("_Size", LineSizeB);

                    sLeaser.sprites[2]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 0f, 1f), (-Mathf.Cos(3.141f * (stepTimer / 4f) / 4) + 1) / 2));
                    sLeaser.sprites[3]._renderLayer?._material?.SetVector("_Colour", Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 0f, 1f), (-Mathf.Cos(3.141f * (stepTimer / 4f) / 4) + 1) / 2));
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

        public int waitCount = waitCount;

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

        public Vector2 arrowPosition = ArrowPosition;

        public Vector2[] projectionLines;

        public Vector2[] projectionLinesMarker;

        public MaterialPropertyBlock block = new();
    }
}