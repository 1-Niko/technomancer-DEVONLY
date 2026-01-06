// Campaigns/Technomancer/Ability/Registry/ObjectParameters.cs
/* Stores the parameters for each unique manipulatable object */

using System.Collections.Generic;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public partial class ObjectRegistry
        {
            private static readonly Dictionary<OwnerType, (int Level, int Firewall)> _params = new();

            private static readonly bool _paramsLoaded = LoadParams();

            private static void Define(OwnerType type, int level, int firewall)
            {
                if (!_params.ContainsKey(type)) _params.Add(type, (level, firewall));
            }

            private static bool LoadParams()
            {
                Define(OwnerType.Shortcut,        level: 0, firewall: 0);
                Define(OwnerType.Neuron,          level: 0, firewall: 0);

                Define(OwnerType.Pearl,           level: 0, firewall: 0);
                Define(OwnerType.VultureMask,     level: 0, firewall: 0);
                Define(OwnerType.Jellyfish,       level: 0, firewall: 0);
                Define(OwnerType.ElectricSpear,   level: 0, firewall: 0);
                Define(OwnerType.SingularityBomb, level: 0, firewall: 0);

                Define(OwnerType.OverseerEye,     level: 0, firewall: 0);
                Define(OwnerType.InspectorEye,    level: 0, firewall: 0);

                Define(OwnerType.YellowLizard,    level: 0, firewall: 0);
                Define(OwnerType.WhiteLizard,     level: 0, firewall: 0);
                Define(OwnerType.CyanLizard,      level: 0, firewall: 0);
                Define(OwnerType.RedLizard,       level: 0, firewall: 0);

                Define(OwnerType.Vulture,         level: 0, firewall: 0);
                Define(OwnerType.KingVulture,     level: 0, firewall: 0);
                Define(OwnerType.MirosVulture,    level: 0, firewall: 0);

                Define(OwnerType.BabyCentipede,   level: 0, firewall: 0);
                Define(OwnerType.BabyCentiwing,   level: 0, firewall: 0);
                Define(OwnerType.Centipede,       level: 0, firewall: 0);
                Define(OwnerType.Centiwing,       level: 0, firewall: 0);
                Define(OwnerType.Aquapede,        level: 0, firewall: 0);
                Define(OwnerType.RedCentipede,    level: 0, firewall: 0);

                Define(OwnerType.WolfSpider,      level: 0, firewall: 0);
                Define(OwnerType.SpitterSpider,   level: 0, firewall: 0);
                Define(OwnerType.MotherSpider,    level: 0, firewall: 0);

                Define(OwnerType.VultureGrub,     level: 0, firewall: 0);
                Define(OwnerType.Frog,            level: 0, firewall: 0);
                Define(OwnerType.BoxWorm,         level: 0, firewall: 0);
                Define(OwnerType.Barnacle,        level: 0, firewall: 0);
                Define(OwnerType.BigJellyfish,    level: 0, firewall: 0);
                Define(OwnerType.Dropwig,         level: 0, firewall: 0);
                Define(OwnerType.MirosBird,       level: 0, firewall: 0);
                Define(OwnerType.Angler,          level: 0, firewall: 0);
                Define(OwnerType.Leviathan,       level: 0, firewall: 0);
                Define(OwnerType.Inspector,       level: 0, firewall: 0);
                Define(OwnerType.Overseer,        level: 0, firewall: 0);

                return true;
            }

            public static int Level(OwnerType type)
            {
                return _params.TryGetValue(type, out var val) ? val.Level : 0;
            }

            public static int Firewall(OwnerType type)
            {
                return _params.TryGetValue(type, out var val) ? val.Firewall : 0;
            }
        }
    }
}