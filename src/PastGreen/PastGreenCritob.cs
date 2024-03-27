using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace Slugpack
{
    sealed class PastGreenCritob : Critob
    {
        internal PastGreenCritob() : base(CreatureTemplateType.PastGreen)
        {
            Icon = new SimpleIcon("Kill_Past_Green_Lizard", Color.green);
            RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.PastGreen);
            SandboxPerformanceCost = new(3f, 1.5f);
            LoadedPerformanceCost = 200f;
            // ShelterDanger = ShelterDanger.Hostile;
        }

        public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
        {
            if (connection.type == MovementConnection.MovementType.ShortCut)
            {
                if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                    allow = true;
                if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                    allow = true;
            }
            else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
            {
                if (map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                    allow = true;
                if (map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                    allow = true;
            }
        }

        public override IEnumerable<string> WorldFileAliases() => new[] { "pastGreen" };

        public override CreatureTemplate CreateTemplate()
        {
            var t = new CreatureFormula(CreatureTemplateType.PastGreen, Type, "PastGreen")
            {
                /*TileResistances = new()
                {
                    Air = new(1f, Allowed)
                },
                ConnectionResistances = new()
                {
                    Standard = new(1f, Allowed),
                    ShortCut = new(1f, Allowed),
                    BigCreatureShortCutSqueeze = new(10f, Allowed),
                    OffScreenMovement = new(1f, Allowed),
                    BetweenRooms = new(10f, Allowed)
                },*/
                // DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
                // DamageResistances = new() { Base = 200f, Explosion = .03f },
                // StunResistances = new() { Base = 200f },
                HasAI = true,
                Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.GreenLizard),
            }.IntoTemplate();
            return LizardBreeds.BreedTemplate(CreatureTemplateType.PastGreen, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null); ;
        }

        public override void EstablishRelationships()
        {
            var s = new Relationships(Type);

            s.Rivals(CreatureTemplate.Type.PinkLizard, 0.2f);
            s.Rivals(CreatureTemplateType.PastGreen, 0.8f);
            s.Rivals(CreatureTemplate.Type.WhiteLizard, 0.05f);

            s.Eats(CreatureTemplate.Type.BlueLizard, 0.25f);
            s.Eats(CreatureTemplate.Type.EggBug, 0.1f);
            s.Eats(CreatureTemplate.Type.BigSpider, 0.1f);

            s.Fears(CreatureTemplate.Type.KingVulture, 0.5f);
            s.Fears(CreatureTemplate.Type.RedCentipede, 0.5f);

            s.FearedBy(CreatureTemplate.Type.BlueLizard, 0.25f);

            // Non-Green relations
            s.Eats(CreatureTemplate.Type.GreenLizard, 0.3f);
            s.FearedBy(CreatureTemplate.Type.GreenLizard, 0.1f);
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

        public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Lizard(acrit, acrit.world);

        public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

        public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.GreenLizard;
    }
}