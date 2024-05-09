using Mono.WebBrowser;
using SlugBase.Features;

namespace Slugpack;

public static class SlugpackExtensions
{
    //Extension for Technomancer
    private static readonly ConditionalWeakTable<Player, TechyData> _cwttn = new();

    public static TechyData Techy(this Player player) => _cwttn.GetValue(player, _ => new TechyData(player));

    public static bool IsTechy(this Player player) => player.Techy().IsTechy;

    public static bool IsTechy(this Player player, out TechyData Techy)
    {
        Techy= player.Techy();
        return Techy.IsTechy;
    }

    //Extension for Voyager
    private static readonly ConditionalWeakTable<Player, VoyagerData> _cwtvy = new();

    public static VoyagerData Voyager(this Player player) => _cwtvy.GetValue(player, _ => new VoyagerData(player));

    public static bool IsVoyager(this Player player) => player.Voyager().IsVoyager;

    public static bool IsVoyager(this Player player, out VoyagerData Voyager)
    {
        Voyager = player.Voyager();
        return Voyager.IsVoyager;
    }

    //Extension for Creatures and Pipes

    private static readonly ConditionalWeakTable<Creature, CreatureData> cdctw_ = new();

    public static CreatureData CreatureData(this Creature creature) => cdctw_.GetValue(creature, _ => new CreatureData(creature));

    public static bool IsCreature(this Creature creature) => creature.CreatureData().IsCreature;

    public static bool IsCreature(this Creature creature, out CreatureData creatureData)
    {
        creatureData = creature.CreatureData();
        return creatureData.IsCreature;
    }

    public static bool IsLockImmune(this Creature creature) => creature.CreatureData().tempLockImmune;
    public static void GrantPassthroughAllowance(this Creature creature) { creature.CreatureData().tempLockImmune = true; }
    public static bool HasPassthroughAllowance(this Creature creature) => creature.CreatureData().tempLockImmune;
    public static void RevokePassthroughAllowance(this Creature creature) { creature.CreatureData().tempLockImmune = false; }

    public static void Burn(this Creature creature) { creature.CreatureData().burnCount++; }
    public static int BurnLevel(this Creature creature) => creature.CreatureData().burnCount;
    public static void SetBurning(this Creature creature) { creature.CreatureData().burning = true; }
    public static bool IsBurning(this Creature creature) { return creature.CreatureData().burning; }
    public static void SetNotBurning(this Creature creature) { creature.CreatureData().burning = false; }
}