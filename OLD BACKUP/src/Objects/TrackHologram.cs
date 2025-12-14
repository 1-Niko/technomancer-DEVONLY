using static Pom.Pom;
using static Pom.Pom.Vector2ArrayField;

namespace Slugpack
{
    public class TrackHologramData(PlacedObject owner) : ManagedData(owner, null)
    {
        [Vector2Field("radius", 50, -50, Vector2Field.VectorReprType.circle)]
        public Vector2 rad;

        [Vector2ArrayField("handle", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, 20, 60 })]
        public Vector2[] handle;

        [Vector2ArrayField("position", 2, true, Vector2ArrayRepresentationType.Chain, new float[4] { 0, 0, -100, 0 })]
        public Vector2[] hologramPosition;

        [IntegerField("A", 0, 3, 0, ManagedFieldWithPanel.ControlType.arrows, displayName: "Hologram Index")]
        public int index;
    }

    public class TrackHologram(PlacedObject placedObject) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (hologramSprite == null)
            {
                hologramSprite = new TrackHologramObject(placedObject, placedObject.pos);
                room.AddObject(hologramSprite);
            }

            hologramSprite.pos = placedObject.pos;
            hologramSprite.rad = (placedObject.data as TrackHologramData).rad;
            hologramSprite.index = (placedObject.data as TrackHologramData).index;
            hologramSprite.position = (placedObject.data as TrackHologramData).hologramPosition[1];

            bool playerInRange = false;
            float distance = (OptionsMenu.alwaysOnHolograms.Value) ? 0 : RWCustom.Custom.Dist(Vector2.zero, (placedObject.data as TrackHologramData).rad);

            foreach (var player in room.world.game.Players)
            {
                if (player != null && player.Room.realizedRoom == room)
                {
                    Vector2 averageBodyChunkPosition = Vector2.zero;
                    foreach (var bodyChunk in (player.realizedCreature as Player).bodyChunks)
                    {
                        averageBodyChunkPosition += bodyChunk.pos;
                    }
                    averageBodyChunkPosition /= (player.realizedCreature as Player).bodyChunks.Length;

                    float playerDistance = RWCustom.Custom.Dist(averageBodyChunkPosition, placedObject.pos);

                    playerInRange = playerInRange || playerDistance < distance;
                }
            }

            bool trainInRange = Utilities.ClosestTrainPosition(placedObject.pos, room) < distance * 2;
            bool trainInFlickerRange = Utilities.ClosestTrainPosition(placedObject.pos, room) < distance * 4;

            if (trainInFlickerRange)
                hologramSprite.flickering = true;

            if (trainInRange)
            {
                hologramSprite.disabled = true;
                hologramSprite.trainCountdown = 90;
            }

            hologramSprite.enabled = playerInRange && !trainInRange || OptionsMenu.alwaysOnHolograms.Value;
        }
        private TrackHologramObject hologramSprite;
    }

    public class TrackHologramObject(PlacedObject placedObject, Vector2 pos) : CosmeticSprite
    {
        private readonly PlacedObject placedObject = placedObject;

        public override void Update(bool eu)
        {
            base.Update(eu);
            stepTimer++;
            trainCountdown = Mathf.Max(0, trainCountdown - 1);

            if (!Constants.TrackHologramMessage.TryGetValue(placedObject, out var messenger)) Constants.TrackHologramMessage.Add(placedObject, messenger = new TrackHologramMessenger());

            if (!messenger.playerInteracted)
            {
                if (trainCountdown < 18 && trainCountdown != 0)
                {
                    disabled = false;
                    flickering = true;

                    forceHidden = false;
                }
                else if (trainCountdown == 0)
                {
                    flickering = false;
                    messenger.onCooldown = false;
                }
            }
            else
            {
                if (!messenger.onCooldown)
                {
                    disabled = true;
                    trainCountdown = 120;
                    messenger.onCooldown = true;
                }
                else if (trainCountdown == 0)
                {
                    messenger.playerInteracted = false;
                    trainCountdown = 19;
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            switch (index)
            {
                case 0:
                    sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("EntranceSign");
                    sLeaser.sprites[0].color = Utilities.HexToColor("FFFF69");
                    break;
                case 1:
                    sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("WarningSign");
                    sLeaser.sprites[0].color = Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 1f, 1f), (-Mathf.Cos(3.141f * ((float)stepTimer / 10f) / 4) + 1) / 2);
                    break;
                case 2:
                    int time = 20;

                    // This one won't be used on the tracks so it's fine

                    if (trainCountdown > 0)
                    {
                        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("ExitSign_3");
                        stepTimer = time * 4;
                    }
                    else
                    {

                        switch ((stepTimer / time) % 5)
                        {
                            case 0:
                                forceHidden = true;
                                break;
                            case 1:
                                forceHidden = false;
                                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("ExitSign_0");
                                break;
                            case 2:
                                forceHidden = false;
                                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("ExitSign_1");
                                break;
                            case 3:
                                forceHidden = false;
                                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("ExitSign_2");
                                break;
                            case 4:
                                forceHidden = false;
                                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("ExitSign_3");
                                break;
                        }
                    }
                    sLeaser.sprites[0].color = new Color(1f, 0f, 0f); // Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 1f, 1f, 0f), (-Mathf.Cos(3.141f * ((float)stepTimer / 10f) / 2) + 1) / 2);
                    break;
                case 3:
                    sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("SiloWarningSign");
                    sLeaser.sprites[0].color = Utilities.ColourFade(new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 0.5f, 0f, 0.8f), (-Mathf.Cos(3.141f * ((float)stepTimer / 32f) / 4) + 1) / 2);
                    break;
            }

            sLeaser.sprites[0].isVisible = ((!disabled && ((enabled && !flickering) || (enabled && flickering && stepTimer % 4 == 0))) && !forceHidden) || OptionsMenu.alwaysOnHolograms.Value;

            sLeaser.sprites[0].SetPosition(pos + position - rCam.pos);
            sLeaser.sprites[0].scaleX = 1f;
            sLeaser.sprites[0].scaleY = 1f;
            sLeaser.sprites[0].anchorX = 0f;
            sLeaser.sprites[0].anchorY = 0f;

            if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders) && !OptionsMenu.alwaysOnHolograms.Value)
            {
                sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Hologram"];
            }

            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = new FSprite("pixel", true);

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

        public Vector2 rad;

        public Vector2 position;

        public string hex;

        public int index;

        public int stepTimer = 0;

        public bool enabled = true;

        public bool disabled = false;

        public int trainCountdown = 0;

        public bool flickering = false;

        public bool forceHidden = false;
    }
}