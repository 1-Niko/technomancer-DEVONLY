namespace Slugpack;

internal static class MoonDialogue
{
    internal static void Apply()
    {
        On.SSOracleBehavior.Update += SSOracleBehavior_Update;
    }

    private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
    {
        if (self.oracle.room.game.Players.Any(i => i.realizedCreature is Player && (i.realizedCreature as Player).IsTechy()) &&
            self.oracle.room.game.Players.Where(i => i.realizedCreature is Player).All(i => i.Room.name == "DM_AI"))
        {
            orig(self, eu);

            self.pearlPickupReaction = false;
            self.lastPearlPickedUp = false;

            if (!Constants.OracleInfo.TryGetValue(self.oracle, out var oracleinfo))
            { Constants.OracleInfo.Add(self.oracle, oracleinfo = new OracleData()); }

            if (oracleinfo.unlockShortcuts)
            {
                self.UnlockShortcuts();
            }

            //self.NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);

            if (self.oracle.room.GetTilePosition(self.player.mainBodyChunk.pos).y < 32 &&
                (self.discoverCounter > 220 || RWCustom.Custom.DistLess(self.player.mainBodyChunk.pos,
                self.oracle.firstChunk.pos, 150f) || !RWCustom.Custom.DistLess(self.player.mainBodyChunk.pos,
                self.oracle.room.MiddleOfTile(self.oracle.room.ShortcutLeadingToNode(1).StartTile), 150f)))
            {
                oracleinfo.seenPlayer = true;
            }

            if (!oracleinfo.seenPlayer)
            {
                //self.movementBehavior = SSOracleBehavior.MovementBehavior.Idle;
                //self.action = null;
            }
            else
            {
                if (!oracleinfo.endMain)
                {
                    oracleinfo.timer++;

                    if (oracleinfo.timer == 280)
                    {
                        self.LockShortcuts();
                        self.movementBehavior = SSOracleBehavior.MovementBehavior.KeepDistance;
                        self.action = SSOracleBehavior.Action.MeetWhite_Shocked;
                    }

                    if (oracleinfo.timer == 320)
                    {
                        self.movementBehavior = SSOracleBehavior.MovementBehavior.KeepDistance;
                        self.NewAction(SSOracleBehavior.Action.General_GiveMark);
                    }

                    if (oracleinfo.timer > 320 && self.action != SSOracleBehavior.Action.General_GiveMark && !oracleinfo.setTimer)
                    {
                        oracleinfo.timerOffset = oracleinfo.timer;
                        oracleinfo.setTimer = true;
                        self.action = null;
                    }
                    if (oracleinfo.setTimer)
                    {
                        if (oracleinfo.timer - oracleinfo.timerOffset == 80)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_meet_moon_firsttime[0], Dialogue.techy_meet_moon_firsttime[0].Length * 2);
                        }
                        if (oracleinfo.timer - oracleinfo.timerOffset == 172)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_meet_moon_firsttime[1], Dialogue.techy_meet_moon_firsttime[1].Length * 2);
                        }
                        if (oracleinfo.timer - oracleinfo.timerOffset == 316)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_meet_moon_firsttime[2], Dialogue.techy_meet_moon_firsttime[2].Length * 2);
                        }
                        if (oracleinfo.timer - oracleinfo.timerOffset == 488)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_meet_moon_firsttime[3], Dialogue.techy_meet_moon_firsttime[3].Length * 2);
                        }
                        if (oracleinfo.timer - oracleinfo.timerOffset == 726)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_meet_moon_firsttime[4], (Dialogue.techy_meet_moon_firsttime[4].Length - 2) * 2);
                        }
                        if (oracleinfo.timer - oracleinfo.timerOffset == 890)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_meet_moon_firsttime[5], Dialogue.techy_meet_moon_firsttime[5].Length * 2);
                        }
                        if (oracleinfo.timer - oracleinfo.timerOffset == 1046)
                        {
                            oracleinfo.unlockShortcuts = true;
                            self.movementBehavior = SSOracleBehavior.MovementBehavior.Idle;
                            oracleinfo.endMain = true;
                        }
                    }
                }
                else
                {
                    if (self.oracle.room.game.Players.Where(i => i.realizedCreature is Player).Any(i => i.Room.name == "DM_AI"))
                    {
                        for (int i = 0; i < self.oracle.room.game.Players.Count; i++)
                        {
                            if (self.oracle.room.game.Players[i].realizedCreature.room.abstractRoom.name == "DM_AI")
                            {
                                var velocity = self.oracle.room.game.Players[i].realizedCreature.mainBodyChunk.vel;
                                if (Mathf.Abs(velocity.x) < 3 && Mathf.Abs(velocity.y) < 3)
                                {
                                    velocity += (self.oracle.room.game.Players[i].realizedCreature.mainBodyChunk.pos - self.oracle.room.MiddleOfTile(new Vector2(50f, 40f))) / 20f;
                                }
                            }
                        }

                        oracleinfo.overstayTimer++;
                        int t = Dialogue.techy_meet_moon_firsttime[5].Length * 2;

                        if (oracleinfo.overstayTimer == 240 + 800 + t)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_staying_too_long[0], Dialogue.techy_staying_too_long[0].Length * 2);
                        }
                        if (oracleinfo.overstayTimer == 580 + 1600 + t)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_staying_too_long[1], Dialogue.techy_staying_too_long[1].Length * 2);
                        }
                        if (oracleinfo.overstayTimer == 910 + 2400 + t)
                        {
                            Utilities.ShowMessage(self.oracle.room.game.cameras[0], Dialogue.techy_staying_too_long[2], Dialogue.techy_staying_too_long[2].Length * 2);
                        }
                    }
                }
            }

            /*
            oracleinfo.timer++;
            if (oracleinfo.timer % 70 == 0)
            {
                Utilities.ShowMessage(self.oracle.room.game.cameras[0], "This is a test", 60);
            }
            */
        }
        else
        {
            orig(self, eu);
        }
    }
}