using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;

namespace Slugpack
{
    internal sealed class PastGreenCritob : Critob
    {
        internal PastGreenCritob() : base(TnEnums.CreatureType.PastGreen)
        {
            Icon = new SimpleIcon("Kill_Past_Green_Lizard", Color.green);
            LoadedPerformanceCost = 200f;
            SandboxPerformanceCost = new(1.5f, 3f);
            ShelterDanger = ShelterDanger.Hostile;
            CreatureName = nameof(TnEnums.CreatureType.PastGreen);
            RegisterUnlock(KillScore.Configurable(25), TnEnums.SandboxUnlock.PastGreen);
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

        public override Creature CreateRealizedCreature(AbstractCreature acrit) => new PastGreenLizard(acrit, acrit.world);

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

        public override IEnumerable<string> WorldFileAliases() => [nameof(TnEnums.CreatureType.PastGreen)];

        public override CreatureType ArenaFallback() => CreatureType.GreenLizard;

        public override string DevtoolsMapName(AbstractCreature acrit) => "pastGreen";

        public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.Lizards];

        public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureType.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureType.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureType.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureType.GreenLizard));

        public override void EstablishRelationships()
        {
            var s = new Relationships(Type);

            s.Rivals(CreatureType.PinkLizard, 0.2f);
            s.Rivals(TnEnums.CreatureType.PastGreen, 0.8f);
            s.Rivals(CreatureType.WhiteLizard, 0.05f);

            s.Eats(CreatureType.BlueLizard, 0.25f);
            s.Eats(CreatureType.EggBug, 0.1f);
            s.Eats(CreatureType.BigSpider, 0.1f);

            s.Fears(CreatureType.KingVulture, 0.5f);
            s.Fears(CreatureType.RedCentipede, 0.5f);

            s.FearedBy(CreatureType.BlueLizard, 0.25f);

            // Non-Green relations
            s.Eats(CreatureType.GreenLizard, 0.3f);
            s.FearedBy(CreatureType.GreenLizard, 0.1f);
        }
    }
}