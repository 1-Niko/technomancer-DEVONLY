// Campaigns/Technomancer/Ability/SlowEffect/Variables.cs
/* Contains variables which need to be widely accessible for the slow effect */


using System.Collections.Generic;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static bool isSlowed;
        public static float accumulator;
        public static float slowdown;
    }
}