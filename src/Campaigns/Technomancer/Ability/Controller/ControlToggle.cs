// Campaigns/Technomancer/Ability/Controller/ControlToggle.cs
/* Defines visible node graph control functions */

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public partial class Controller
        {
            public static void EnableNodeGraph(Player player)
            {
                if (player.room.game.GetNodeUI().nodeHandler == null) {
                    player.room.game.GetNodeUI().nodeHandler = new NodeHandler(player);
                    player.room.AddObject(player.room.game.GetNodeUI().nodeHandler);

                    player.room.game.GetNodeUI().nodeHandler.Update(true);
                }

                if (!player.Techy().AbilityInitialized) {
                    if (player.room.game.GetNodeUI().nodeHandler.loadedObjects.Count > 0) {
                        player.room.world.game.GetNodeUI().playerTargetObject[0] = player.room.game.GetNodeUI().nodeHandler.NearestToPlayer();
                        player.room.world.game.GetNodeUI().playerTargetObject[0].sprite.colour = new UnityEngine.Color(1f, 0f, 0f);
                    }

                    player.Techy().AbilityInitialized = true;
                }
            }

            public static void DisableNodeGraph(Player player)
            {
                if (player.room.game.GetNodeUI().nodeHandler != null) {
                    player.room.game.DeleteNodes();
                    player.room.game.GetNodeUI().nodeHandler = null;
                }

                player.Techy().AbilityInitialized = false;
            }
        }
    }
}