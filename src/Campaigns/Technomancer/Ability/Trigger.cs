// Campaigns/Technomancer/Ability/Trigger.cs
/* Handles determining if the player is in Technomancy mode, and setting the environmental effects for it */

using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public partial class Controller
        {
            public static partial class Trigger
            {
                public static void Init()
                {
                    On.Player.checkInput += Player_checkInput;
                }

                private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
                {
                    orig(self);

                    if (!self.IsTechy())
                        return;

                    isSlowed = false;

                    if (self.input[0].spec) {
                        self.SetAbilityActive();

                        self.EyesClosed();
                        self.PointAtTarget();
                        self.LookAtTarget();

                        self.room.game.cameras[0].mushroomMode = 8;
                        self.room.game.cameras[0].ApplyFade();

                        isSlowed = true;

                        self.input[0].x = 0;
                        self.input[0].y = 0;
                        self.input[0].jmp = false;
                        self.input[0].thrw = false;
                        self.input[0].pckp = false;
                    }
                    else { // Remove the timeslow effect once they are no longer holding the button
                        self.room.game.cameras[0].mushroomMode = self.Adrenaline;
                        self.room.game.cameras[0].ApplyFade();

                        self.SetAbilityInactive();
                    }
                }
            }
        }
    }
}