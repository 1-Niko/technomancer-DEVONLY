namespace Slugpack;

public class CreatureData
{
    public readonly bool IsCreature;

    public readonly Creature creature;

    //ShortcutData

    public bool tempLockImmune;

    public bool burning;

    public int burnCount;

    public static Dictionary<ShortcutData, bool> isLocked = [];

    public CreatureData(Creature creature)
    {
        this.creature = creature;
        IsCreature = creature == this.creature;

        if (!IsCreature) return;

        tempLockImmune = false;
    }
}