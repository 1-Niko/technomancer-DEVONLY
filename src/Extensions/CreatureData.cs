namespace Slugpack;

public class CreatureData
{
    public readonly bool IsCreature;

    public readonly Creature creature;

    //ShortcutData

    public bool tempLockImmune;

    public bool burning;

    public int burnCount;

    public bool processing_pipe_intake;

    public CreatureData(Creature creature)
    {
        this.creature = creature;
        IsCreature = creature == this.creature;

        if (!IsCreature) return;

        tempLockImmune = false;
    }
}