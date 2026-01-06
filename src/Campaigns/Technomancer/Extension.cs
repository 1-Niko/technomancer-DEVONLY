// Campaigns/Technomancer/Extension.cs
/* Extends the player object to hold Technomancer related variables */

using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Slugpack.Technomancer.Ability;

namespace Slugpack;

public static partial class Technomancer
{
    private static readonly ConditionalWeakTable<Player, TechyData> TechyCWT = new();
    public static TechyData Techy(this Player player) => TechyCWT.GetValue(player, _ => new TechyData(player));

    public static bool IsTechy(this Player player) => player.Techy().IsTechy;
    public static bool IsTechy(this PlayerGraphics playerGraphics) => (playerGraphics.owner as Player).Techy().IsTechy;

    public static void SetButtonPress(this Player player, Input input) { player.Techy().PressedButton = input; }
    public static Input GetButtonPress(this Player player) => player.Techy().PressedButton;

    public static void SetAbilityActive(this Player player) { player.Techy().AbilityActive = true; }
    public static void SetAbilityInactive(this Player player) { player.Techy().AbilityActive = false; }

    public static void EyesClosed(this Player player) { player.Techy().graphics.blink = 10; }

    public static void PointAtTarget(this Player player) {
        if (player.room.game.GetNodeUI().playerTargetObject[0] == null)
            return;

        int handIndex = ((player.room.game.GetNodeUI().playerTargetObject[0].pos - player.mainBodyChunk.pos).x > 0) ? 1 : 0;
        player.Techy().graphics.hands[handIndex].absoluteHuntPos = player.Techy().TargetPoint;
        player.Techy().graphics.hands[handIndex].reachingForObject = true;
    }

    public static void LookAtTarget(this Player player) {
        if (player.room.game.GetNodeUI().playerTargetObject[0] == null)
            return; 
        
        player.Techy().graphics.LookAtPoint(player.room.game.GetNodeUI().playerTargetObject[0].pos, 0f);
    }

    // EVERYTHING IN THIS REFERRING TO THE ABILITY INSTEAD OF THE SLUG ITSELF NEEDS TO BE MOVED TO NodeUI
    public class TechyData
    {
        public readonly bool IsTechy;
        public readonly Player player;
        public readonly PlayerGraphics graphics;

        public Input PressedButton;
        public bool AbilityActive;

        public Vector2 TargetPoint;
        public ManipulatableObject TargetObject;

        public bool AbilityInitialized;

        public TechyData(Player player)
        {
            IsTechy = player.slugcatStats.name.value == "technomancer";
            this.player = player;
            this.graphics = player.graphicsModule as PlayerGraphics;

            if (!IsTechy) return;
        }
    }



    private static readonly ConditionalWeakTable<RainWorldGame, NodeUI> NodeCWT = new();
    public static NodeUI GetNodeUI(this RainWorldGame game) => NodeCWT.GetValue(game, _ => new NodeUI(game));

    public static void DeleteNodes(this RainWorldGame game) { game.GetNodeUI().nodeHandler.Destroy(); }

    public class NodeUI
    {
        public Player player;
        public NodeHandler nodeHandler;
        public HackNode[] nodes;

        public bool[] selectionFrozen;

        public bool jmp;
        public bool thrw;
        public bool pckp;

        public int x;
        public int y;

        public Vector2[] playerPosition;
        public ManipulatableObject[] playerTargetObject;

        public NodeUI(RainWorldGame game)
        {
            playerPosition = new Vector2[game.Players.Count];
            playerTargetObject = new ManipulatableObject[game.Players.Count];
            selectionFrozen = new bool[game.Players.Count];
        }
    }
}