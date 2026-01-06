// Campaigns/Technomancer/Ability/Registry/ObjectPositions.cs
/* Given an object and its type, determines the position on screen the object should be placed */

using MoreSlugcats;
using UnityEngine;
using Watcher;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public partial class ObjectRegistry
        {
            public static Vector2 GetPos(object data, OwnerType type, Room room)
            {
                switch (type)
                {
                    case OwnerType.Error:
                        return Vector2.zero;
                    case OwnerType.Shortcut:
                        if (data is ShortcutData tile)
                            return room.MiddleOfTile(tile.StartTile);
                        return Vector2.zero;
                    case OwnerType.Pearl:
                        return (data as DataPearl).firstChunk.pos;
                    case OwnerType.InspectorEye:
                        return (data as OverseerCarcass).firstChunk.pos;
                    case OwnerType.OverseerEye:
                        return (data as OverseerCarcass).firstChunk.pos;
                    case OwnerType.VultureMask:
                        return (data as VultureMask).firstChunk.pos;
                    case OwnerType.SingularityBomb:
                        return (data as SingularityBomb).firstChunk.pos;
                    case OwnerType.Jellyfish:
                        return (data as JellyFish).firstChunk.pos;
                    case OwnerType.ElectricSpear:
                        return (data as ElectricSpear).firstChunk.pos;
                    case OwnerType.Neuron:
                        return (data as SSOracleSwarmer).firstChunk.pos;
                    case OwnerType.MirosBird:
                        return (data as MirosBird).mainBodyChunk.pos;
                    case OwnerType.VultureGrub:
                        return (data as VultureGrub).mainBodyChunk.pos;
                    case OwnerType.Vulture:
                        return (data as Vulture).mainBodyChunk.pos;
                    case OwnerType.Inspector:
                        return (data as Inspector).mainBodyChunk.pos;
                    case OwnerType.Overseer:
                        return (data as Overseer).mainBodyChunk.pos;
                    case OwnerType.YellowLizard:
                        return (data as Lizard).mainBodyChunk.pos;
                    case OwnerType.CyanLizard:
                        return (data as Lizard).mainBodyChunk.pos;
                    case OwnerType.Leviathan:
                        return (data as BigEel).mainBodyChunk.pos;
                    case OwnerType.WhiteLizard:
                        return (data as Lizard).mainBodyChunk.pos;
                    case OwnerType.Centipede:
                        return (data as Centipede).mainBodyChunk.pos;
                    case OwnerType.Centiwing:
                        return (data as Centipede).mainBodyChunk.pos;
                    case OwnerType.RedLizard:
                        return (data as Lizard).mainBodyChunk.pos;
                    case OwnerType.KingVulture:
                        return (data as Vulture).mainBodyChunk.pos;
                    case OwnerType.BabyCentipede:
                        return (data as Centipede).mainBodyChunk.pos;
                    case OwnerType.BabyCentiwing:
                        return (data as Centipede).mainBodyChunk.pos;
                    case OwnerType.SpitterSpider:
                        return (data as BigSpider).mainBodyChunk.pos;
                    case OwnerType.RedCentipede:
                        return (data as Centipede).mainBodyChunk.pos;
                    case OwnerType.Aquapede:
                        return (data as Centipede).mainBodyChunk.pos;
                    case OwnerType.MirosVulture:
                        return (data as Vulture).mainBodyChunk.pos;
                    case OwnerType.Angler:
                        return (data as Angler).mainBodyChunk.pos;
                    case OwnerType.Barnacle:
                        return (data as Barnacle).mainBodyChunk.pos;
                    case OwnerType.BoxWorm:
                        return (data as BoxWorm).mainBodyChunk.pos;
                    case OwnerType.Frog:
                        return (data as Frog).mainBodyChunk.pos;
                    case OwnerType.Dropwig:
                        return (data as DropBug).mainBodyChunk.pos;
                    case OwnerType.BigJellyfish:
                        return (data as BigJellyFish).mainBodyChunk.pos;
                    case OwnerType.MotherSpider:
                        return (data as BigSpider).mainBodyChunk.pos;
                    case OwnerType.WolfSpider:
                        return (data as BigSpider).mainBodyChunk.pos;
                    case OwnerType.TrainBell:
                        return Vector2.zero;
                    case OwnerType.TrackHologram:
                        return Vector2.zero;
                }

                return Vector2.zero;
            }
        }
    }
}