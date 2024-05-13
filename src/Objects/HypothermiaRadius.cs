namespace Slugpack;

public class HypothermiaRadiusData(PlacedObject owner) : ManagedData(owner, null)
{
    [Vector2Field("Radius", 50f, 50f, Vector2Field.VectorReprType.circle)]
    public Vector2 rad;
}

public class HypothermiaRadius(PlacedObject placedObject) : UpdatableAndDeletable
{
    public override void Update(bool eu)
    {
        base.Update(eu);

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

                float distance = RWCustom.Custom.Dist(Vector2.zero, (placedObject.data as HypothermiaRadiusData).rad);
                float playerDistance = RWCustom.Custom.Dist(averageBodyChunkPosition, placedObject.pos);

                if (playerDistance < distance)
                {
                    float increaseAmount = (-1f / distance * playerDistance) + 1;
                    player.Hypothermia += increaseAmount * 0.001f;
                }
            }
        }
    }

    private PlacedObject placedObject = placedObject;
}