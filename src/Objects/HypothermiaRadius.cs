using static Pom.Pom;

namespace Slugpack
{
    public class HypothermiaRadiusData : ManagedData
    {
        [Vector2Field("Radius", 50f, 50f, Vector2Field.VectorReprType.circle)]
        public Vector2 rad;

        public HypothermiaRadiusData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class HypothermiaRadius : UpdatableAndDeletable
    {
        public HypothermiaRadius(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            // Debug.Log(RWCustom.Custom.Dist(Vector2.zero, (this.placedObject.data as HypothermiaRadiusData).rad));

            foreach (var player in this.room.world.game.Players)
            {
                if (player != null && player.Room.realizedRoom == room)
                {
                    Vector2 averageBodyChunkPosition = Vector2.zero;
                    foreach (var bodyChunk in (player.realizedCreature as Player).bodyChunks)
                    {
                        averageBodyChunkPosition += bodyChunk.pos;
                    }
                    averageBodyChunkPosition /= (player.realizedCreature as Player).bodyChunks.Length;

                    float distance = RWCustom.Custom.Dist(Vector2.zero, (this.placedObject.data as HypothermiaRadiusData).rad);
                    float playerDistance = RWCustom.Custom.Dist(averageBodyChunkPosition, this.placedObject.pos);

                    if (playerDistance < distance)
                    {
                        float increaseAmount = (-1f / distance) * playerDistance + 1;
                        player.Hypothermia += increaseAmount * 0.001f;
                    }
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        private PlacedObject placedObject;
    }
}