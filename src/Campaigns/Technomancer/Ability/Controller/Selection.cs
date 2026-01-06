// Campaigns/Technomancer/Ability/Controller/Selection.cs
/* Handles player input and selected node handling */

using System;
using System.Numerics;

namespace Slugpack;

/*
PROBLEMS
- Does not work when selected object moves off screen (should move to the closest object to where the selected object was)
- Selection feels off. Need to add more nuance to it to make it feel more intuitive
- Can select some objects which are just barely off screen (Issue with previous version, likely caused by ViewedByAnyCamera)
*/

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static class PlayerController
        {
            private static bool prevPckp;
            private static bool prevJmp;
            private static bool prevThrw;

            private static Input currentButton = Input.None;

            public static void Init()
            {
                On.ProcessManager.Update += ProcessManager_Update;
                On.RainWorldGame.Update += RainWorldGame_Update;
            }

            private static void ProcessManager_Update(On.ProcessManager.orig_Update orig, ProcessManager self, float deltaTime)
            {
                orig(self, deltaTime);

                try
                {

                    if ((self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler == null)
                        return;

                    // Get the inputs, needs to be in processmanager for it to survive the slowdown effect,
                    // and adding it to the extension for possible future expansions.
                    // Not sure what that would entail, but its better to future proof.
                    (self.currentMainLoop as RainWorldGame).GetNodeUI().jmp = RWCustom.Custom.rainWorld.options.controls[0].GetButton(0);
                    (self.currentMainLoop as RainWorldGame).GetNodeUI().thrw = RWCustom.Custom.rainWorld.options.controls[0].GetButton(4);
                    (self.currentMainLoop as RainWorldGame).GetNodeUI().pckp = RWCustom.Custom.rainWorld.options.controls[0].GetButton(3);

                    Vector2 analogueDir = new Vector2(RWCustom.Custom.rainWorld.options.controls[0].GetAxis(1), RWCustom.Custom.rainWorld.options.controls[0].GetAxis(2)); ;

                    (self.currentMainLoop as RainWorldGame).GetNodeUI().x = 0;
                    (self.currentMainLoop as RainWorldGame).GetNodeUI().y = 0;

                    if (analogueDir.X < -0.5f)
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().x = -1;
                    if (analogueDir.X > 0.5f)
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().x = 1;

                    if (analogueDir.Y < -0.5f)
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().y = -1;
                    if (analogueDir.Y > 0.5f)
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().y = 1;

                    // Translate the button presses to the Input enum
                    if ((self.currentMainLoop as RainWorldGame).GetNodeUI().pckp && !prevPckp) currentButton = Input.SHIFT;
                    else if ((self.currentMainLoop as RainWorldGame).GetNodeUI().jmp && !prevJmp) currentButton = Input.Z;
                    else if ((self.currentMainLoop as RainWorldGame).GetNodeUI().thrw && !prevThrw) currentButton = Input.X;

                    bool isPressed = currentButton switch
                    {
                        Input.SHIFT => (self.currentMainLoop as RainWorldGame).GetNodeUI().pckp,
                        Input.Z => (self.currentMainLoop as RainWorldGame).GetNodeUI().jmp,
                        Input.X => (self.currentMainLoop as RainWorldGame).GetNodeUI().thrw,
                        _ => false
                    };

                    if (!isPressed)
                        currentButton = Input.None;

                    (prevPckp, prevJmp, prevThrw) = ((self.currentMainLoop as RainWorldGame).GetNodeUI().pckp, (self.currentMainLoop as RainWorldGame).GetNodeUI().jmp, (self.currentMainLoop as RainWorldGame).GetNodeUI().thrw);

                    // Handle selection input
                    ManipulatableObject newTarget = ((self.currentMainLoop as RainWorldGame).GetNodeUI().x, (self.currentMainLoop as RainWorldGame).GetNodeUI().y) switch
                    {
                        // Up?
                        (0, 1) => GetNearestObjectInDirection((self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler.loadedObjects, (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0].pos, Direction.Up),
                        // Down?
                        (0, -1) => GetNearestObjectInDirection((self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler.loadedObjects, (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0].pos, Direction.Down),
                        // Right?
                        (1, 0) => GetNearestObjectInDirection((self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler.loadedObjects, (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0].pos, Direction.Right),
                        // Left?
                        (-1, 0) => GetNearestObjectInDirection((self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler.loadedObjects, (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0].pos, Direction.Left),
                        _ => null
                    };

                    if ((!(self.currentMainLoop as RainWorldGame).GetNodeUI().selectionFrozen[0] || UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)) && newTarget != null) {
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0].sprite.colour = new UnityEngine.Color(1f, 1f, 1f);
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0] = newTarget;
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().playerTargetObject[0].sprite.colour = new UnityEngine.Color(1f, 0f, 0f);
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().selectionFrozen[0] = true;
                    }
                    else if (newTarget == null) {
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().selectionFrozen[0] = false;
                    }
                }
                catch (Exception e) { }
            }

            private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
            {
                // This function will be used eventually to generalize all of Techy's code
                // to work with multiplayer

                try {
                    for (int i = 0; i < self.Players.Count; i++)
                        self.GetNodeUI().playerPosition[i] = (self.Players[i].realizedCreature as Player).mainBodyChunk.pos;
                }
                catch (Exception e) { }

                orig(self);
            }
        }
    }
}