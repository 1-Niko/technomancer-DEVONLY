using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;

namespace HiveQueen;

sealed class HiveQueenCritob : Critob
{
    internal HiveQueenCritob() : base(CreatureTemplateType.HiveQueen)
    {
        Icon = new SimpleIcon("Kill_Yellow_Lizard", new(.654f, .811f, .858f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(.5f, .5f);
        RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.HiveQueen);
        Hooks.Apply();
    }

    public override int ExpeditionScore() => 6;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.yellow;

    public override string DevtoolsMapName(AbstractCreature acrit) => "HQ";

    public override IEnumerable<string> WorldFileAliases() => new[] { "hivequeen" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] 
    { 
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesInside
    };

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);

        s.IsInPack(CreatureTemplate.Type.YellowLizard, 1f);
        s.MakesUncomfortable(CreatureTemplate.Type.YellowLizard, 1f);

        s.Attacks(CreatureTemplateType.HiveQueen, 2.8f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Lizard(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.YellowLizard;
}