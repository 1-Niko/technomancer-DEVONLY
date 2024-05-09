using static Pom.Pom;

namespace Slugpack
{
    public class WaterKillBoxData(PlacedObject owner) : ManagedData(owner, null)
    {
        [Vector2Field("BoxArea", 100, 100, Vector2Field.VectorReprType.rect)]
        public Vector2 BoxArea;
    }

    public class WaterKillBox(PlacedObject placedObject, Room room) : UpdatableAndDeletable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);

            foreach (var creature in room.abstractRoom.creatures)
            {
                if (creature != null && creature.Room.realizedRoom == room)
                {
                    Creature realizedCreature = creature.realizedCreature;
                    if (!Constants.WaterCounter.TryGetValue(realizedCreature, out var counter)) Constants.WaterCounter.Add(realizedCreature, counter = new WaterKillBoxCounter());
                    {
                        Vector2 averageBodyChunkPosition = Vector2.zero;
                        foreach (var bodyChunk in realizedCreature.bodyChunks)
                        {
                            averageBodyChunkPosition += bodyChunk.pos;
                        }
                        averageBodyChunkPosition /= realizedCreature.bodyChunks.Length;

                        bool inRangeX = averageBodyChunkPosition.x > placedObject.pos.x && averageBodyChunkPosition.x < placedObject.pos.x + (placedObject.data as WaterKillBoxData).BoxArea.x;
                        bool inRangeY = averageBodyChunkPosition.y > placedObject.pos.y && averageBodyChunkPosition.y < placedObject.pos.y + (placedObject.data as WaterKillBoxData).BoxArea.y;

                        if (realizedCreature.Submersion > 0.1f && room.waterObject != null && !realizedCreature.abstractCreature.lavaImmune && inRangeX && inRangeY)
                        {
                            if (realizedCreature.Submersion > 0.2f)
                            {
                                if (realizedCreature is Player && !realizedCreature.dead)
                                {
                                    if (ModManager.MSC && (realizedCreature as Player).SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                                    {
                                        (realizedCreature as Player).pyroJumpCounter++;
                                        if ((realizedCreature as Player).pyroJumpCounter >= MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value)
                                        {
                                            (realizedCreature as Player).PyroDeath();
                                        }
                                    }
                                    else
                                    {
                                        (realizedCreature as Player).Die();
                                    }
                                }
                                else if (realizedCreature.State is HealthState || (realizedCreature.State is HealthState && (realizedCreature.State as HealthState).health > 1f))
                                {
                                    if (!realizedCreature.dead)
                                    {
                                        realizedCreature.Violence(null, new Vector2?(new Vector2(0f, 5f)), realizedCreature.firstChunk, null, Creature.DamageType.Explosion, 0.2f, 0.1f);
                                    }
                                }
                                else if (!realizedCreature.dead)
                                {
                                    realizedCreature.Die();
                                }
                                if (counter.count == 0)
                                {
                                    realizedCreature.mainBodyChunk.vel.y = 35f;
                                    room.AddObject(new Smoke.Smolder(room, realizedCreature.firstChunk.pos, realizedCreature.firstChunk, null));
                                }
                                else if (counter.count == 1)
                                {
                                    realizedCreature.mainBodyChunk.vel.y = 20f;
                                }
                                else if (counter.count == 2)
                                {
                                    realizedCreature.mainBodyChunk.vel.y = 15f;
                                }
                                else if (counter.count == 3)
                                {
                                    realizedCreature.mainBodyChunk.vel.y = 5f;
                                    room.AddObject(new Smoke.Smolder(room, realizedCreature.firstChunk.pos, realizedCreature.firstChunk, null));
                                }
                                if (counter.count < ((realizedCreature is Player) ? 400 : 30))
                                {
                                    counter.count++;
                                    room.AddObject(new Explosion.ExplosionSmoke(realizedCreature.firstChunk.pos, RWCustom.Custom.RNV() * 5f * Random.value, 1f));
                                }
                                if (!counter.contact)
                                {
                                    if (counter.count <= 3)
                                    {
                                        for (int j = 0; j < 14 + (3 - counter.count) * 5; j++)
                                        {
                                            Vector2 a = RWCustom.Custom.RNV();
                                            room.AddObject(new Spark(realizedCreature.firstChunk.pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 8, 24));
                                        }
                                    }
                                    room.PlaySound(SoundID.Firecracker_Burn, realizedCreature.firstChunk.pos, 0.5f, 0.5f + Random.value * 1.5f);
                                    counter.contact = true;
                                    counter.count++;
                                }
                            }
                            else if (counter.count == 0)
                            {
                                counter.count++;
                                room.AddObject(new Smoke.Smolder(room, realizedCreature.firstChunk.pos, realizedCreature.firstChunk, null));
                                room.PlaySound(SoundID.Firecracker_Burn, realizedCreature.firstChunk.pos, 0.5f, 0.3f + Random.value * 0.6f);
                                counter.contact = true;
                            }
                        }
                        else
                        {
                            counter.contact = false;
                        }
                    }
                }
            }
        }

    }
}