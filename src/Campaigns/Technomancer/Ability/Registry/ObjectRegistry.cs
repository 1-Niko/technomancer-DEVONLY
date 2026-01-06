// Campaigns/Technomancer/Ability/Registry/ObjectRegistry.cs
/* Translates between the game's definitions of objects to a single, simplified system here */

using MoreSlugcats;
using Watcher;
using System;
using System.Collections.Generic;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public partial class ObjectRegistry
        {
            private static readonly List<Func<object, OwnerType?>> _rules = new();

            private static void Match<T>(OwnerType result)
            {
                _rules.Add(obj => obj is T ? result : null);
            }

            private static void Match<T>(Func<T, bool> condition, OwnerType result)
            {
                _rules.Add(obj => (obj is T castedObj && condition(castedObj)) ? result : null);
            }

            static ObjectRegistry()
            {
                Match<ShortcutData>(s => s.destNode != -1 && s.destNode < s.room.abstractRoom.connections.Length && s.room.abstractRoom.connections[s.destNode] != -1 && s.shortCutType == ShortcutData.Type.RoomExit, OwnerType.Shortcut);
                Match<SSOracleSwarmer>(OwnerType.Neuron);

                Match<DataPearl>(OwnerType.Pearl);
                Match<VultureMask>(OwnerType.VultureMask);
                Match<JellyFish>(OwnerType.Jellyfish);
                Match<SingularityBomb>(OwnerType.SingularityBomb);
                Match<ElectricSpear>(OwnerType.ElectricSpear);

                Match<OverseerCarcass>(
                    c => c.abstractPhysicalObject is OverseerCarcass.AbstractOverseerCarcass abs && abs.InspectorMode,
                    OwnerType.InspectorEye
                );
                Match<OverseerCarcass>(OwnerType.OverseerEye);

                Match<Vulture>(v => v.IsMiros, OwnerType.MirosVulture);
                Match<Vulture>(v => v.IsKing, OwnerType.KingVulture);
                Match<Vulture>(OwnerType.Vulture);

                Match<Lizard>(l => l.Template.type == CreatureTemplate.Type.YellowLizard, OwnerType.YellowLizard);
                Match<Lizard>(l => l.Template.type == CreatureTemplate.Type.CyanLizard, OwnerType.CyanLizard);
                Match<Lizard>(l => l.Template.type == CreatureTemplate.Type.WhiteLizard, OwnerType.WhiteLizard);
                Match<Lizard>(l => l.Template.type == CreatureTemplate.Type.RedLizard, OwnerType.RedLizard);

                Match<Centipede>(c => c.Small && c.Centiwing, OwnerType.BabyCentiwing);
                Match<Centipede>(c => c.Small, OwnerType.BabyCentipede);
                Match<Centipede>(c => c.Centiwing, OwnerType.Centiwing);
                Match<Centipede>(c => c.Red, OwnerType.RedCentipede);
                Match<Centipede>(c => c.AquaCenti, OwnerType.Aquapede);
                Match<Centipede>(OwnerType.Centipede);

                Match<BigSpider>(s => s.spitter, OwnerType.SpitterSpider);
                Match<BigSpider>(s => s.mother, OwnerType.MotherSpider);
                Match<BigSpider>(OwnerType.WolfSpider);

                Match<MirosBird>(OwnerType.MirosBird);
                Match<VultureGrub>(OwnerType.VultureGrub);
                Match<Inspector>(OwnerType.Inspector);
                Match<Overseer>(OwnerType.Overseer);
                Match<BigEel>(OwnerType.Leviathan);
                Match<Angler>(OwnerType.Angler);
                Match<BigJellyFish>(OwnerType.BigJellyfish);

                Match<Barnacle>(OwnerType.Barnacle);
                Match<BoxWorm>(OwnerType.BoxWorm);
                Match<Frog>(OwnerType.Frog);
                Match<DropBug>(OwnerType.Dropwig);

                // Match<PlacedObject>(p => p.type.ToString() == "TrainBell", OwnerType.TrainBell);
            }

            public ObjectRegistry() { }

            public static OwnerType Identify(object data)
            {
                foreach (var rule in _rules)
                {
                    var result = rule(data);
                    if (result.HasValue) return result.Value;
                }

                return OwnerType.Error;
            }
        }
    }
}