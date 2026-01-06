// Campaigns/Technomancer/Ability/Init.cs
/* Initializes Techy's Technomancy */

/*
NEEDED:
DONE - Some way to identify all valid technological objects
WIP - Visualizer for the node graph (With a menu toggle to go back to the overseer arrow)
DONE - Hook to get the keypress and set the player to technomancy mode
- Some way to cleanly format all of the individual effects (maybe a third effect for each since now we'll have the shift, z, and x keys to use?)

Extensions are the clear way forward, I can see it making the trigger code clean at minimum
*/

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static void Apply()
        {
            Controller.Init();
            TimeEffect.Init();
            PlayerController.Init();
        }
    }
}