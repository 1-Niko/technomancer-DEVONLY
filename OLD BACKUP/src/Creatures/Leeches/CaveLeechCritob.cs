namespace Slugpack;

public class CaveLeechCritob : Critob
{
    public CaveLeechCritob() : base(TnEnums.CreatureType.CaveLeech)
    {
        Icon = new SimpleIcon("Kill_Leech", new Color(0.06f, 0.06f, 0.06f));
        LoadedPerformanceCost = 1f;
        SandboxPerformanceCost = new SandboxPerformanceCost(1f, 0.1f);
        RegisterUnlock(KillScore.Constant(1), TnEnums.SandboxUnlock.CaveLeech);
    }
    public override IEnumerable<string> WorldFileAliases() => ["caveLeech", "CaveLeech"];

    public override string DevtoolsMapName(AbstractCreature acrit) => "CL";

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(0f, 0.5f, 0f);

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Leech(acrit, acrit.world);

    public override CreatureTemplate CreateTemplate()
    {
        var s = new CreatureFormula(CreatureType.Leech, TnEnums.CreatureType.CaveLeech, nameof(TnEnums.CreatureType.CaveLeech))
        {
            HasAI = false,
            Pathing = PreBakedPathing.Ancestral(CreatureType.Leech)
        }.IntoTemplate();
        return s;
    }

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        s.Eats(CreatureType.Slugcat, 1f);
        s.IsInPack(TnEnums.CreatureType.CaveLeech, 1f);
    }
}