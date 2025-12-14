// Campaigns/Technomancer/Extension.cs
/* Extends the player object to hold Technomancer related variables */

using System.Runtime.CompilerServices;

namespace Slugpack;

public static partial class Technomancer
{
    private static readonly ConditionalWeakTable<Player, TechyData> TechyCWT = new();
    public static TechyData Techy(this Player player) => TechyCWT.GetValue(player, _ => new TechyData(player));

    public static bool IsTechy(this Player player) => player.Techy().IsTechy;
    public static bool IsTechy(this PlayerGraphics playerGraphics) => (playerGraphics.owner as Player).Techy().IsTechy;

    public class TechyData
    {
        public readonly bool IsTechy;
        public readonly Player player;

        public TechyData(Player player)
        {
            IsTechy = player.slugcatStats.name.value == "technomancer";
            this.player = player;

            if (!IsTechy) return; // This needs to be kept even if there's nothing underneath it yet.
        }
    }
}