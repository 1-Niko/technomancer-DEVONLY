namespace Slugpack;

public class HiveQueenCritob : Critob
{
    internal HiveQueenCritob() : base(TnEnums.CreatureType.HiveQueen)
    {
        Icon = new SimpleIcon("Kill_Yellow_Lizard", new(.654f, .811f, .858f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(.5f, .5f);
        RegisterUnlock(KillScore.Configurable(6), TnEnums.SandboxUnlock.HiveQueen);
    }

    public override int ExpeditionScore() => 6;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.yellow;

    public override string DevtoolsMapName(AbstractCreature acrit) => "HQ";

    public override IEnumerable<string> WorldFileAliases() => ["hivequeen"];

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesInside
    ];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureType.LizardTemplate), null, null, null);

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);

        s.IsInPack(CreatureType.YellowLizard, 1f);
        s.MakesUncomfortable(CreatureType.YellowLizard, 1f);

        s.Attacks(TnEnums.CreatureType.HiveQueen, 2.8f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Lizard(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override CreatureType ArenaFallback() => CreatureType.YellowLizard;
}