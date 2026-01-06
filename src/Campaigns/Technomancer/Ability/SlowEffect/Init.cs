// Campaigns/Technomancer/Ability/SlowEffect/Init.cs
/* Handles time slowing down proportionately when Techy's ability is active */

/*
NEEDED:
DONE - Time slowdown end needs to be linked to the player's control scheme instead of the hardcoded release of the C key
DONE - All non-hud graphics need to be slowed down to match the physics slowdown (I believe its just shaders? Since it appears to just be the room's shadow and holograms from what I can tell)
DONE - Set the background to greyscale the INSTANT the time slows down (mushroom effect is too slow for this! it does not trigger instantly) and remove it once it ends
CONFIRMED - Make sure that the slowdown ability is also slowing down the cycle timer
EASY (worked by default apparently) - Make sure the slowdown effect stops if the player gets stunned, grabbed (basically just stunned i think?) or dies
DONE - Slow down ALL sound effects as well
DONE - Make sure hud shaders (like the karma symbol blinking while inside of a gate) do NOT slow down when the effect is active
- Make using the ability too much exhaust techy?
- Make sure the speedrun timer continues ticking correctly
*/

using System.Runtime.CompilerServices;
using System;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static class TimeEffect
        {
            private static ConditionalWeakTable<SoundEmitter, PitchData> _pitchStorage = new ConditionalWeakTable<SoundEmitter, PitchData>();

            private class PitchData
            {
                public float OriginalPitch;
                public float LastSetPitch;
            }

            public static void Init()
            {
                // Handles physics and HUD slowdown
                On.ProcessManager.Update += ProcessManager_Update;

                // Handles sound slowdown
                On.VirtualMicrophone.PositionedSound.Update += PositionedSound_Update;
                On.SoundEmitter.Update += SoundEmitter_Update;
                On.AmbientSoundPlayer.DrawUpdate += AmbientSoundPlayer_DrawUpdate;
            }

            private static void AmbientSoundPlayer_DrawUpdate(On.AmbientSoundPlayer.orig_DrawUpdate orig, AmbientSoundPlayer self, float timeStacker, float timeSpeed, UnityEngine.Vector2 currentListenerPos)
            {
                if (isSlowed)
                    timeSpeed *= 0.02f;

                orig(self, timeStacker, timeSpeed, currentListenerPos);
            }

            private static void SoundEmitter_Update(On.SoundEmitter.orig_Update orig, SoundEmitter self, bool eu)
            {
                orig(self, eu);

                if (isSlowed) {
                    PitchData data = _pitchStorage.GetOrCreateValue(self);

                    if (Math.Abs(self.pitch - data.LastSetPitch) > 0.001f)
                        data.OriginalPitch = self.pitch;

                    float modifier = 0.02f;
                    float newPitch = data.OriginalPitch * modifier;

                    self.pitch = newPitch;
                    data.LastSetPitch = newPitch;
                }
                else {
                    if (_pitchStorage.TryGetValue(self, out PitchData data)) {
                        if (Math.Abs(self.pitch - data.LastSetPitch) < 0.001f) {
                            self.pitch = data.OriginalPitch;
                            data.LastSetPitch = data.OriginalPitch;
                        }
                    }
                }
            }

            private static void PositionedSound_Update(On.VirtualMicrophone.PositionedSound.orig_Update orig, VirtualMicrophone.PositionedSound self, float timeStacker, float timeSpeed)
            {
                orig(self, timeStacker, timeSpeed);

                if (isSlowed)
                    self.SetPitch = 0.02f;
            }

            private static void ProcessManager_Update(On.ProcessManager.orig_Update orig, ProcessManager self, float deltaTime)
            {
                try {
                    if (!isSlowed) {
                        (self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler.Destroy();

                        for (int i = 0; i < (self.currentMainLoop as RainWorldGame).GetNodeUI().nodes.Length; i++)
                            (self.currentMainLoop as RainWorldGame).GetNodeUI().nodes[i].Destroy();
                    }

                    if (isSlowed) {

                        // Detect when the player has released the SPECIAL key, since using the normal detection method would lead to delay, then end the slowdown accordingly
                        if (!RWCustom.Custom.rainWorld.options.controls[0].GetButton(34))
                            isSlowed = false;

                        // Keep the HUD updating smoothly even when the rest of the game is frozen
                        float secondsPerTick = 1f / self.currentMainLoop.framesPerSecond;
                        accumulator += deltaTime;
                        while (accumulator > secondsPerTick) {
                            for (int i = 0; i < (self.currentMainLoop as RainWorldGame).cameras[0].hud.parts.Count; i++)
                                (self.currentMainLoop as RainWorldGame).cameras[0].hud.parts[i].Update();

                            (self.currentMainLoop as RainWorldGame).GetNodeUI().nodeHandler.Update(true);

                            for (int i = 0; i < (self.currentMainLoop as RainWorldGame).GetNodeUI().nodes.Length; i++)
                                (self.currentMainLoop as RainWorldGame).GetNodeUI().nodes[i].GraphicsUpdate();

                            accumulator -= secondsPerTick;
                        }

                        // Slow down the game to a specified speed (WILL BE UPDATED LATER)
                        deltaTime *= 0.02f;
                    }
                }
                catch (Exception e) { }

                orig(self, deltaTime);
            }
        }
    }
}