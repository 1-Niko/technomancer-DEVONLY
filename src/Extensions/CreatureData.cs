﻿namespace Slugpack;

public class CreatureData
{
    public readonly bool IsCreature;

    public readonly Creature creature;

    //ShortcutData

    public bool immune;

    public Dictionary<ShortcutData, bool> isLocked = [];

    public CreatureData(Creature creature)
    {
        this.creature = creature;
        IsCreature = creature == this.creature;

        if (!IsCreature) return;

        immune = false;
    }
}