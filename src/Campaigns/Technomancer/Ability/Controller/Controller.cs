// Campaigns/Technomancer/Ability/Controller/Controller.cs
/* Handles applying the actual effects for the technomancy ability */

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public partial class Controller
        {
            public static void Init()
            {
                Trigger.Init();
                On.Player.Update += Player_Update;
            }

            private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
            {
                orig(self, eu);

                if (self == null || !self.IsTechy())
                    return;

                if (self.Techy().AbilityActive) {
                    EnableNodeGraph(self);

                    if (self.Techy().TargetObject != null && !self.Techy().TargetObject.slatedForDeletetion)
                        self.Techy().TargetPoint = self.Techy().TargetObject.pos;
                    else
                        self.Techy().TargetPoint = self.mainBodyChunk.pos;
                }
                else {
                    DisableNodeGraph(self);

                    self.Techy().TargetObject = null;
                    self.Techy().TargetPoint = self.mainBodyChunk.pos;
                }
            }
        }
    }
}